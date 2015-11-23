using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Picofy.Models;
using Picofy.TorshifyHelper;
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
            Player = new MusicPlayer();
            InitializeComponent();
        }



        private void Songlist_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Songlist.SelectedItem != null)
            {
                Player.PlaySong((Song)Songlist.SelectedItem);
            }
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            if (Songlist.Items.Count == 0) { return; }
            if (Songlist.SelectedIndex == 0)
            {
                Songlist.SelectedIndex = Songlist.Items.Count - 1;
            }
            else
            {
                Songlist.SelectedIndex--;
            }

            Player.PlaySong((Song)Songlist.SelectedItem);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (Songlist.Items.Count == 0) { return; }
            if (Songlist.SelectedIndex == Songlist.Items.Count - 1)
            {
                Songlist.SelectedIndex = 0;
            }
            else
            {
                Songlist.SelectedIndex++;
            }

            Player.PlaySong((Song)Songlist.SelectedItem);
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Player.SongPlayer.IsPlaying)
            {
                Player.SongPlayer.Pause();
            }
            else
            {
                Player.SongPlayer.Play();
            }
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

            foreach (var lst in Player.SongPlayer.Session.PlaylistContainer.Playlists)
            {
                if (lst.Type != PlaylistType.Playlist)
                {
                    continue;
                }
                PlaylistList.Items.Add(lst);
            }

            LoadPlaylistSongs(Player.SongPlayer.Session.PlaylistContainer.Playlists[0]);
        }

        void LoadPlaylistSongs(IContainerPlaylist playlist)
        {
            playlist.WaitUntilLoaded();

            Songlist.Items.Clear();

            foreach (var trk in playlist.Tracks)
            {
                trk.WaitUntilLoaded();

                if (trk.IsLocal) { continue; }

                Songlist.Items.Add(new Song
                {
                    Id = trk.ToLink().AsUri(),
                    Name = trk.Name,
                    Artist = new Artist
                    {
                        Name = trk.Artists.First().Name
                    },
                    Album = new Album
                    {
                        CoverArt = "https://i.scdn.co/image/" + trk.Album?.CoverId
                    },
                    TotalSeconds = (int)trk.Duration.TotalSeconds
                });
            }


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
            Player?.SongPlayer?.Dispose();
        }

        private void PlaylistList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (PlaylistList.SelectedItem != null)
            {
                LoadPlaylistSongs((IContainerPlaylist)PlaylistList.SelectedItem);
            }
        }
    }
}
