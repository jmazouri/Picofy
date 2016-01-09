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
    public class MusicPlayer : INotifyPropertyChanged
    {
        public delegate void SongFinishedHandler();
        public event SongFinishedHandler SongFinished;
        protected virtual void OnSongFinished()
        {
            SongFinished?.Invoke();
        }

        public bool RequiresLogin => SongPlayer?.Session?.ConnectionState != ConnectionState.LoggedIn;

        public float Volume
        {
            get { return SongPlayer?.Volume ?? 0; }
            set
            {
                SongPlayer.Volume = value;
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

        public static List<BasicPlugin> Plugins { get; private set; }

        public MusicPlayer()
        {
            _songTimer = new Timer(1000);
            _songTimer.Elapsed += SongTimerOnElapsed;
            _songTimer.Start();

            Plugins = PluginContainer.Current.Container.GetExports<BasicPlugin>().Select(d=>d.Value).Where(d=>d != null).ToList();
        }

        private void SongTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            OnPropertyChanged(nameof(RequiresLogin));

            if (SongProgress >= SongDuration)
            {
                if (SongDuration > 0)
                {
                    OnSongFinished();
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

        public void PlaySong(ITrack song)
        {
            _songTimer.Stop();

            CurrentSong = song;
            _songProgress = 0;
            SongPlayer.PlaySong(CurrentSong);

            foreach (BasicPlugin plugin in Plugins)
            {
                plugin.SongPlay(CurrentSong);
            }

            _songTimer.Start();
        }

        public void PauseToggle()
        {
            if (SongPlayer.IsPlaying)
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
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
