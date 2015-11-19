using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Picofy.Annotations;
using TRock.Music;
using TRock.Music.Torshify;

namespace Picofy.Models
{
    public class MusicPlayer : INotifyPropertyChanged
    {
        private Song _currentSong;

        public Song CurrentSong
        {
            get { return _currentSong; }
            set
            {
                if (Equals(value, _currentSong))
                {
                    return;
                }
                _currentSong = value;
                OnPropertyChanged();
            }
        }

        private int _songProgress;

        public int SongProgress
        {
            get { return _songProgress; }
            set
            {
                if (value == _songProgress)
                {
                    return;
                }
                _songProgress = value;
                OnPropertyChanged();
            }
        }

        private int _songLength;

        public int SongLength
        {
            get { return _songLength; }
            set
            {
                if (value == _songLength)
                {
                    return;
                }
                _songLength = value;
                OnPropertyChanged();
            }
        }

        public TorshifySongPlayerClient SongPlayer;
        public SpotifySongProvider SpotifySongProvider;

        public MusicPlayer()
        {
            var spotifyPlayerHost = new TorshifyServerProcessHandler();
            spotifyPlayerHost.CloseServerTogetherWithClient = true;
            spotifyPlayerHost.TorshifyServerLocation = "TRock.Music.Torshify.Server.exe";
            spotifyPlayerHost.UserName = ConfigurationManager.AppSettings["spotifyUsername"];
            spotifyPlayerHost.Password = ConfigurationManager.AppSettings["spotifyPassword"];
            spotifyPlayerHost.Hidden = true;
            spotifyPlayerHost.Start();

            SpotifySongProvider = new SpotifySongProvider();

            SongPlayer = new TorshifySongPlayerClient(new Uri("http://localhost:8081"));
            SongPlayer.Connect().ContinueWith(queryTask =>
            {
                SongPlayer.Volume = 0.25f;
                SongPlayer.Progress += SongPlayer_Progress;
            });
        }

        private void SongPlayer_Progress(object sender, ValueProgressEventArgs<int> e)
        {
            SongProgress = e.Current;
            SongLength = e.Total;
        }

        public void PlaySong(Song song)
        {
            CurrentSong = song;
            SongPlayer.Start(song);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
