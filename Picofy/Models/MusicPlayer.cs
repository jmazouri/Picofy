using System;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Timers;
using Picofy.Annotations;
using Picofy.TorshifyHelper;
using Torshify;

namespace Picofy.Models
{
    public class MusicPlayer : INotifyPropertyChanged, IDisposable
    {
        public bool RequiresLogin => SongPlayer?.Session?.ConnectionState != ConnectionState.LoggedIn;

        private float _volume;

        public float Volume
        {
            get { return _volume; }
            set
            {
                if (value.Equals(_volume))
                {
                    return;
                }
                _volume = value;
                SongPlayer.Volume = value;
                OnPropertyChanged();
            }
        }

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
                OnPropertyChanged(nameof(SongDuration));
            }
        }

        private readonly Timer _songTimer;

        private int _songProgress;
        public int SongProgress
        {
            get { return _songProgress; }
            set
            {
                _songTimer.Stop();

                _songProgress = value;
                SongPlayer.Session.PlayerSeek(new TimeSpan(0, 0, value));
                OnPropertyChanged(nameof(SongProgress));

                _songTimer.Start();
            }
        }
        public int SongDuration => CurrentSong?.TotalSeconds ?? 0;

        public TorshifySongPlayer SongPlayer;
        //public SpotifySongProvider SpotifySongProvider { get; private set; }

        public void Connect(string username, string password, bool rememberme)
        {
            if (SongPlayer == null)
            {
                SongPlayer = new TorshifySongPlayer(username, password, rememberme);
            }
            else
            {
                SongPlayer.Session.Login(username, password, rememberme);
                while (SongPlayer.Session.ConnectionState != ConnectionState.LoggedIn) { }
                while (SongPlayer.Session.PlaylistContainer == null) { }
            }
        }

        public MusicPlayer()
        {
            //SpotifySongProvider = new SpotifySongProvider(ConfigurationManager.AppSettings["spotifyClientId"]);
            _songTimer = new Timer(1000);
            _songTimer.Elapsed += delegate
            {
                OnPropertyChanged(nameof(RequiresLogin));

                if (SongProgress >= SongDuration)
                {
                    return;
                }

                _songProgress++;
                OnPropertyChanged(nameof(SongProgress));
            };
            _songTimer.Start();
        }

        public void PlaySong(Song song)
        {
            _songTimer.Stop();

            CurrentSong = song;
            _songProgress = 0;
            SongPlayer.PlaySong(CurrentSong);

            _songTimer.Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            SongPlayer.Dispose();
        }
    }
}
