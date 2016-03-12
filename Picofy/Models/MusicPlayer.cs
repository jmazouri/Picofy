using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using Picofy.Annotations;
using Picofy.Plugins;
using Picofy.TorshifyHelper;
using Torshify;

namespace Picofy.Models
{
    public sealed class MusicPlayer : INotifyPropertyChanged, IDisposable
    {
        public delegate void SongFinishedHandler();
        public event SongFinishedHandler SongFinished;

        private void OnSongFinished()
        {
            SongFinished?.Invoke();
        }

        public bool AnyPlugins
        {
            get
            {
                return Plugins.Any();
            }
        }

        private bool _requiresLogin = true;

        public bool RequiresLogin
        {
            get
            {
                return _requiresLogin;
            }
            set
            {
                _requiresLogin = value;
                OnPropertyChanged();
            }
        }

        private float _volume = 0.25f;
        public float Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;

                try
                {
                    SongPlayer.Volume = _volume;
                }
                catch { }

                OnPropertyChanged();
            }
        }

        private ITrack _currentSong;

        public ITrack CurrentSong
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

        private IPlaylist _currentPlaylist;

        public IPlaylist CurrentPlaylist
        {
            get
            {
                return _currentPlaylist;
            }
            set
            {
                if (Equals(value, _currentPlaylist))
                {
                    return;
                }

                _currentPlaylist = value;

                SongPlayer.Pause();
                _songTimer.Stop();

                CurrentSong = null;
                CurrentTracklist = value.Tracks.Cast<ITrack>().ToList();

                OnPropertyChanged();
            }
        }

        private List<ITrack> _currentTracklist = null; 
        public List<ITrack> CurrentTracklist
        {
            get
            {
                if (_currentTracklist == null)
                {
                    return CurrentPlaylist.Tracks.Where(d => !d.IsLocal).Cast<ITrack>().ToList();
                }

                return _currentTracklist;
            }
            set
            {
                _currentTracklist = value;
                OnPropertyChanged();
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

        public int SongDuration => (CurrentSong == null ? 0 : (int)CurrentSong.Duration.TotalSeconds);
        
        public TorshifySongPlayer SongPlayer;
        public TorshifySessionManager SessionManager;

        public List<BasicPlugin> Plugins { get; private set; }

        private static MusicPlayer _current;
        public static MusicPlayer Current => _current ?? (_current = new MusicPlayer());

        public MusicPlayer()
        {
            _songTimer = new Timer(1000);
            _songTimer.Elapsed += SongTimerOnElapsed;
            _songTimer.Start();

            Plugins = PluginContainer.Current.Container.GetExports<BasicPlugin>().Select(d=>d.Value).Where(d=>d != null).ToList();
            OnPropertyChanged(nameof(Plugins));
        }

        private void SongTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            OnPropertyChanged(nameof(RequiresLogin));

            if (SongProgress >= SongDuration)
            {
                if (SongDuration > 0)
                {
                    OnSongFinished();

                    NextSong();
                }
                return;
            }

            _songProgress++;

            OnPropertyChanged(nameof(SongProgress));
        }

        public void Connect(string username, string password, bool rememberme)
        {
            if (SongPlayer != null)
            {
                return;
            }

            SongPlayer = new TorshifySongPlayer(username, password, rememberme);
            OnPropertyChanged(nameof(Volume));
        }

        public void PlaySong(ITrack song, IEnumerable<ITrack> trackList = null)
        {
            _songTimer.Stop();

            CurrentSong = song;
            _currentTracklist = trackList?.ToList();

            _songProgress = 0;
            SongPlayer.PlaySong(CurrentSong);

            foreach (BasicPlugin plugin in Plugins)
            {
                plugin.SongPlay(CurrentSong);
            }

            _songTimer.Start();
        }

        public void NextSong()
        {
            var foundSongIndex = _currentTracklist.IndexOf(_currentSong);

            if (_currentTracklist.Count == 0)
            {
                return;
            }

            if (foundSongIndex + 1 == _currentTracklist.Count - 1)
            {
                PlaySong(_currentTracklist.First(), _currentTracklist);
            }
            else
            {
                PlaySong(_currentTracklist[foundSongIndex + 1], _currentTracklist);
            }
        }

        public void PrevSong()
        {
            var foundSongIndex = _currentTracklist.IndexOf(_currentSong);

            if (_currentTracklist.Count == 0)
            {
                return;
            }

            if (foundSongIndex - 1 < 0)
            {
                PlaySong(_currentTracklist.Last(), _currentTracklist);
            }
            else
            {
                PlaySong(_currentTracklist[foundSongIndex - 1], _currentTracklist);
            }
        }

        public void PauseToggle(bool forcePause = false)
        {
            if (SongPlayer.IsPlaying || forcePause)
            {
                _songTimer.Stop();
                SongPlayer.Pause();

                foreach (BasicPlugin plugin in Plugins)
                {
                    plugin.SongPaused();
                }
            }
            else
            {
                _songTimer.Start();
                SongPlayer.Play();

                foreach (BasicPlugin plugin in Plugins)
                {
                    plugin.SongPlay(CurrentSong);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            SongPlayer.Dispose();
        }
    }
}
