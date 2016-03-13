using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Picofy.Models;
using Picofy.Plugins;
using Picofy.TorshifyHelper;
using Picofy.UIModels;
using Torshify;

namespace Picofy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MusicPlayer Player { get; set; }
        private TorshifySessionManager SessionManager;
        private IContainerPlaylist _activePlaylist;

        public MainWindow()
        {
            Player = MusicPlayer.Current;

            SessionManager = new TorshifySessionManager();
            SessionManager.LoginFinished += () =>
            {
                PlaylistList.ItemsSource = SessionManager.Playlists;
            };

            InitializeComponent();

            SongGrid.LayoutUpdated += (sender, args) => Player.CurrentTracklist = SongGrid.Items.Cast<ITrack>().ToList();

            SongGrid.Sorting += (sender, args) =>
            {
                PicofyConfiguration.Current.SetSortingForPlaylist(_activePlaylist.Name,
                    new PlaylistSorting
                    {
                        ColumnName = args.Column.SortMemberPath,
                        SortDirection =
                            args.Column.SortDirection == ListSortDirection.Ascending
                                ? ListSortDirection.Descending
                                : ListSortDirection.Ascending
                    });
            };
        }

        private void SongGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
            if (SongGrid.SelectedItem != null)
            {
                Player.CurrentPlaylist = _activePlaylist;
                Player.PlaySong((ITrack)SongGrid.SelectedItem, SongGrid.Items.Cast<ITrack>());
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            Player.NextSong();
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            Player.PrevSong();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            Player.PauseToggle();
        }

        private void LoginButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(UsernameBox.Text) || String.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                return;
            }

            LoginDialog.IsOpen = SessionManager.Login(UsernameBox.Text, PasswordBox.Password, Remember.IsChecked == true);
        }

        private void TheWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoginDialog.IsOpen = !SessionManager.Login();
        }

        void LoadPlaylistSongs(IContainerPlaylist playlist)
        {
            playlist.WaitUntilLoaded();
            _activePlaylist = playlist;

            SongGrid.ItemsSource = _activePlaylist.Tracks.Where(d=>d.IsLocal == false);

            PlaylistSorting currentSort = PicofyConfiguration.Current.GetSortingForPlaylist(playlist.Name);

            SongGrid.Items.SortDescriptions.Clear();

            if (currentSort == null) return;

            SongGrid.Items.SortDescriptions.Add(new SortDescription
            {
                PropertyName = currentSort.ColumnName,
                Direction = currentSort.SortDirection.GetValueOrDefault()
            });

            foreach (var col in SongGrid.Columns)
            {
                col.SortDirection = null;
            }

            SongGrid.Columns.FirstOrDefault(d => d.SortMemberPath == currentSort.ColumnName).SortDirection = currentSort.SortDirection;
            SongGrid.Items.Refresh();
        }

        private void ProgressB_MouseUp(object sender, MouseButtonEventArgs e)
        {
            int newPos = (int)((e.GetPosition(ProgressB).X / ProgressB.ActualWidth) * Player.SongDuration);
            Player.SongProgress = newPos;
        }

        private void TheWindow_Closing(object sender, CancelEventArgs e)
        {
            Player.PauseToggle(true);

            foreach (BasicPlugin plugin in MusicPlayer.Current.Plugins)
            {
                plugin.Dispose();
            }

            Player?.Dispose();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlaylistList.SelectedItem != null)
            {
                LoadPlaylistSongs((IContainerPlaylist)PlaylistList.SelectedItem);
            }
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.IsRepeat || e.Key != Key.Enter)
            {
                return;
            }

            var result = Player.SongPlayer.Session.Search(SearchBox.Text, 0, 50, 0, 0, 0, 0, 0, 0, SearchType.Suggest);
            result.WaitForCompletion();

            _activePlaylist = null;
            SongGrid.ItemsSource = result.Tracks;
        }

        private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var source = sender as Button;

            if (source == null)
            {
                return;
            }

            var foundPlugin = MusicPlayer.Current.Plugins.FirstOrDefault(d => d.Name == (string)source.Content);

            foundPlugin?.ShowDialog();
        }

        private void PlaylistList_OnPreviewDrop(object sender, DragEventArgs e)
        {
            var result = VisualTreeHelper.HitTest(PlaylistList, e.GetPosition(PlaylistList));

            var foundRow = FindAnchestor<ListViewItem>(result.VisualHit);
            var foundPlaylist = foundRow.Content as IContainerPlaylist;
            var currentTrack = (e.Data.GetData("spotifyTrack") as ITrack);

            foundRow.RenderTransform = new TranslateTransform();

            DoubleAnimation da = new DoubleAnimation
            {
                To = 0,
                From = 15,
                Duration = new Duration(TimeSpan.FromSeconds(0.2)),
                EasingFunction = new QuadraticEase()
            };

            (foundRow.RenderTransform).BeginAnimation(TranslateTransform.XProperty, da);

            foundPlaylist.Tracks.Add(currentTrack);
        }

        private void PlaylistList_OnDragEnter(object sender, DragEventArgs e)
        {
            var result = VisualTreeHelper.HitTest(PlaylistList, e.GetPosition(PlaylistList));
            var foundRow = FindAnchestor<ListViewItem>(result.VisualHit);

            if (foundRow == null)
            {
                e.Handled = false;
                return;
            }

            DoubleAnimation da = new DoubleAnimation
            {
                From = 0,
                To = 15,
                Duration = new Duration(TimeSpan.FromSeconds(0.2)),
                EasingFunction = new QuadraticEase()
            };

            DoubleAnimation da_back = new DoubleAnimation
            {
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(0.2)),
                EasingFunction = new QuadraticEase()
            };

            foreach (var item in PlaylistList.Items)
            {
                ListViewItem listItem = PlaylistList.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;

                if (listItem == null) { continue; }

                listItem.RenderTransform = new TranslateTransform();

                (listItem.RenderTransform).BeginAnimation(TranslateTransform.XProperty, listItem == foundRow ? da : da_back);
            }

        }

        private void PlaylistList_OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("spotifyTrack"))
            {
                e.Handled = true;
            }
        }

        private static T FindAnchestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        private Point startPoint;

        private void SongGrid_OnMouseMove(object sender, MouseEventArgs e)
        {
            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance))
            {
                // Get the dragged ListViewItem
                DataGrid dataGrid = sender as DataGrid;
                DataGridRow dataGridRow = FindAnchestor<DataGridRow>((DependencyObject)e.OriginalSource);

                if (dataGridRow == null)
                {
                    e.Handled = false;
                    return;
                }

                // Find the data behind the ListViewItem
                ITrack contact = (ITrack) dataGridRow.Item;

                // Initialize the drag & drop operation
                DataObject dragData = new DataObject("spotifyTrack", contact);
                DragDrop.DoDragDrop(dataGridRow, dragData, DragDropEffects.Move);
            }
        }

        private void SongGrid_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(null);
        }


        private void DeleteSong_Click(object sender, RoutedEventArgs e)
        {
            if (_activePlaylist == null)
            {
                MessageBox.Show("Can't delete track from here.");
                return;
            }

            _activePlaylist.Tracks.Remove(PlaylistList.SelectedItem as IPlaylistTrack);
            LoadPlaylistSongs(_activePlaylist);
        }
    }
}