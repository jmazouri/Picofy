using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Picofy.Models;
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

            try
            {
                Player.Volume = 0.25f;
                Dispatcher.Invoke(LoadFirstPlaylistSongs);
            }
            catch
            {
                
            }
        }

        void LoadFirstPlaylistSongs()
        {
            Songlist.Items.Clear();

            /*
            foreach (Song s in Player.SpotifySongProvider.GetFirstPlaylistSongs())
            {
                Songlist.Items.Add(s);
            }
            */

            var playlistTracks = Player.SongPlayer.Session.PlaylistContainer.Playlists[0].Tracks;

            foreach (ITrack trk in playlistTracks)
            {
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
            DoTempLogon(null, null, true);
        }

        private void TheWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Player.Dispose();
        }

        private void ProgressB_MouseUp(object sender, MouseButtonEventArgs e)
        {
            int newPos = (int) ((e.GetPosition(ProgressB).X/ProgressB.ActualWidth)*Player.SongDuration);
            Player.SongProgress = newPos;
        }
    }
}
