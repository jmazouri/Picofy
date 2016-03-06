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
        private IPlaylist _activePlaylist;

        public MainWindow()
        {
            Player = MusicPlayer.Current;

            SessionManager = new TorshifySessionManager();
            SessionManager.LoginFinished += () =>
            {
                PlaylistList.ItemsSource = SessionManager.Playlists;
                LoadPlaylistSongs(SessionManager.Playlists.First());
            };

            InitializeComponent();

            SongGrid.Sorting += (sender, args) => 
            PicofyConfiguration.CurrentConfiguration.SetSortingForPlaylist(_activePlaylist.Name,
                new PlaylistSorting
                {
                    ColumnName = args.Column.SortMemberPath,
                    SortDirection = args.Column.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending
                });
        }

        private void SongGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SongGrid.SelectedItem != null)
            {
                Player.CurrentPlaylist = _activePlaylist;
                Player.PlaySong((IPlaylistTrack)SongGrid.SelectedItem);
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

            SessionManager.Login(UsernameBox.Text, PasswordBox.Password, Remember.IsChecked == true);
        }

        private void TheWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SessionManager.Login();
        }

        void LoadPlaylistSongs(IContainerPlaylist playlist)
        {
            playlist.WaitUntilLoaded();
            _activePlaylist = playlist;

            SongGrid.ItemsSource = _activePlaylist.Tracks.Where(d => !d.IsLocal);

            PlaylistSorting currentSort = PicofyConfiguration.CurrentConfiguration.GetSortingForPlaylist(playlist.Name);

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
            int newPos = (int) ((e.GetPosition(ProgressB).X/ProgressB.ActualWidth)*Player.SongDuration);
            Player.SongProgress = newPos;
        }

        private void TheWindow_Closing(object sender, CancelEventArgs e)
        {
            foreach (BasicPlugin plugin in MusicPlayer.Current.Plugins)
            {
                plugin.Dispose();
            }
            Player?.SongPlayer?.Dispose();
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
    }
}
