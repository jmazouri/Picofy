using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CsQuery;
using Picofy.Models;
using TRock.Music;

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

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            CQ.CreateFromUrlAsync(Query.Text).Then(
            responseSuccess =>
            {
                var result = responseSuccess.Dom["meta"]
                    .Where(d=>d.GetAttribute("property") == "music:song")
                    .Select(d=>Player.SpotifySongProvider.GetSongFromId(d.GetAttribute("content").Split('/').Last()));

                Dispatcher.Invoke(delegate
                {
                    Songlist.Items.Clear();

                    foreach (Song s in result)
                    {
                        Songlist.Items.Add(s);
                    }

                    Songlist.SelectedIndex = 0;
                    PlaySong((Song)Songlist.SelectedItem);
                });
            }, 
            responseFail =>
            {
                MessageBox.Show("Couldn't load playlist. Error: " + responseFail.Error);
            });
        }

        private void PlaySong(Song sng)
        {
            Player.PlaySong(sng);

            /*
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(sng.Album.CoverArt);
            bmp.EndInit();

            bmp.DownloadCompleted += delegate
            {
                Background = new SolidColorBrush(Util.AverageColorOfImage(bmp));
            };
            */
        }

        private void Songlist_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PlaySong((Song)Songlist.SelectedItem);
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

            PlaySong((Song)Songlist.SelectedItem);
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

            PlaySong((Song)Songlist.SelectedItem);
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
    }
}
