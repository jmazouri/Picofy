using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Utils;
using NAudio.Wave;
using Picofy.Models;
using Torshify;

namespace Picofy.TorshifyHelper
{
    public class TorshifySongPlayer : IDisposable
    {
        public readonly ISession Session;
        private BufferedWaveProvider _provider;
        private WaveOut _waveOut;

        private ManualResetEvent wait = new ManualResetEvent(false);

        public float Volume
        {
            get { return _waveOut?.Volume ?? 0; }
            set
            {
                if (_waveOut != null)
                {
                    _waveOut.Volume = value;
                }
            }
        }
        
        public TorshifySongPlayer(string username = null, string password = null, bool rememberme = false)
        {
            if (Session == null)
            {
                Session = SessionFactory.CreateSession(Constants.ApplicationKey, Constants.CacheFolder, Constants.SettingsFolder, Constants.UserAgent)
                   .SetPreferredBitrate(Bitrate.Bitrate320k);
            }

            Session.ConnectionStateUpdated += (sender, eventArgs) =>
            {
                if (eventArgs.Status != Error.OK || Session.ConnectionState != ConnectionState.LoggedIn)
                {
                    return;
                }

                wait.Set();

                _waveOut = new WaveOut();
                _waveOut.Volume = 0.25f;

                Session.MusicDeliver += _session_MusicDeliver;
            };

            if (username != null && password != null)
            {
                Session.Login(username, password, rememberme);
            }
            else
            {
                if (Session.GetRememberedUser() != String.Empty)
                {
                    Session.Relogin();
                }
                else
                {
                    return;
                }
            }

            wait.WaitOne(10000);
        }

        private void _session_MusicDeliver(object sender, MusicDeliveryEventArgs e)
        {
            int consumed = 0;

            if ((_provider == null || e.Frames == 0))
            {
                _provider = new BufferedWaveProvider(new WaveFormat(e.Rate, e.Channels))
                {
                    BufferDuration = TimeSpan.FromSeconds(0.35)
                };

                _waveOut.Init(_provider);

                _waveOut.Play();
            }

            if ((_provider.BufferLength - _provider.BufferedBytes) > e.Samples.Length)
            {
                _provider.AddSamples(e.Samples, 0, e.Samples.Length);
                consumed = e.Frames;
            }

            e.ConsumedFrames = consumed;
        }

        public bool IsPlaying => _waveOut?.PlaybackState == PlaybackState.Playing;

        public void Pause()
        {
            if (_waveOut.PlaybackState != PlaybackState.Playing)
            {
                return;
            }

            _waveOut.Pause();
            Session.PlayerPause();
        }

        public void Play()
        {
            if (_waveOut.PlaybackState != PlaybackState.Paused)
            {
                return;
            }

            _waveOut.Resume();
            Session.PlayerPlay();
        }

        public void PlaySong(Song song)
        {
            var track = new SessionLinkFactory(Session).GetLink(song.Id).Object.Track;

            if (!track.WaitUntilLoaded(500) || Session.PlayerLoad(track) != Error.OK)
            {
                return;
            }

            Session.PlayerPlay();
        }

        public void Dispose()
        {
            Session?.PlayerUnload();
            Session?.Logout();
            Session.LogoutComplete += delegate
            {
                Session?.Dispose();
                _waveOut?.Dispose();
            };
        }
    }
}
