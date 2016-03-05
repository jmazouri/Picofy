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

        public MainWindow()
        {
            Player = MusicPlayer.Current;
            Player.SongFinished += delegate
            {
                NextSong();
            };

            InitializeComponent();

            SongGrid.Sorting += (sender, args) => 
            PicofyConfiguration.CurrentConfiguration.SetSortingForPlaylist(Player.CurrentPlaylist.Name,
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
                Player.PlaySong((ITrack)SongGrid.SelectedItem);
            }
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (SongGrid.Items.Count == 0) { return; }
            if (SongGrid.SelectedIndex == 0)
            {
                SongGrid.SelectedIndex = SongGrid.Items.Count - 1;
            }
            else
            {
                SongGrid.SelectedIndex--;
            }

            Player.PlaySong((ITrack)SongGrid.SelectedItem);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            NextSong();
        }

        private void NextSong()
        {
            Dispatcher.Invoke(delegate
            {
                if (SongGrid.Items.Count == 0)
                {
                    return;
                }
                if (SongGrid.SelectedIndex == SongGrid.Items.Count - 1)
                {
                    SongGrid.SelectedIndex = 0;
                }
                else
                {
                    SongGrid.SelectedIndex++;
                }

                Player.PlaySong((ITrack)SongGrid.SelectedItem);
            });
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

            DoTempLogon(UsernameBox.Text, PasswordBox.Password, Remember.IsChecked == true);
        }

        private void DoTempLogon(string username, string password, bool rememberme)
        {
            Player.Connect(username, password, rememberme);
            Player.SongPlayer.Session.PlaylistContainer.WaitUntilLoaded();


            var allPlaylists = Player.SongPlayer.Session.PlaylistContainer.Playlists.Where(d => d.Type == PlaylistType.Playlist);

            foreach (var lst in allPlaylists)
            {
                lst.WaitUntilLoaded(2500);
            }

            PlaylistList.ItemsSource = allPlaylists;

            LoadPlaylistSongs(allPlaylists.First());
        }

        void LoadPlaylistSongs(IContainerPlaylist playlist)
        {
            playlist.WaitUntilLoaded();
            Player.CurrentPlaylist = playlist;

            SongGrid.ItemsSource = Player.CurrentPlaylist.Tracks.Where(d => !d.IsLocal);

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

        private void TheWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (TorshifySongPlayer.HasSavedCredentials())
            {
                DoTempLogon(null, null, true);
            }
        }

        private void ProgressB_MouseUp(object sender, MouseButtonEventArgs e)
        {
            int newPos = (int) ((e.GetPosition(ProgressB).X/ProgressB.ActualWidth)*Player.SongDuration);
            Player.SongProgress = newPos;
        }

        private void TheWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
            this.Close();
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
