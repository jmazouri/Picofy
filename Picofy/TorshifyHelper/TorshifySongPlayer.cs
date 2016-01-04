using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSCore;
using CSCore.Codecs;
using CSCore.SoundIn;
using CSCore.SoundOut;
using CSCore.Streams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Picofy.Models;
using Torshify;

namespace Picofy.TorshifyHelper
{
    public class TorshifySongPlayer : IDisposable
    {
        public readonly ISession Session;
        private WriteableBufferingSource _provider;
        private WaveOut _waveOut;

        private readonly object _lockObject = new object();

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

        public static bool HasSavedCredentials()
        {
            string dataPath = Path.Combine(Directory.GetCurrentDirectory(), "PicofyData", "Settings", "settings");

            if (!File.Exists(dataPath))
            {
                return false;
            }

            JToken nameToken;

            if (JObject.Parse(File.ReadAllText(dataPath)).TryGetValue("autologin_canonical_username", out nameToken))
            {
                return true;
            }

            return false;
        }
        
        public TorshifySongPlayer(string username = null, string password = null, bool rememberme = false)
        {
            if (Session == null)
            {
                Session = SessionFactory.CreateSession(Constants.ApplicationKey, Constants.CacheFolder, Constants.SettingsFolder, Constants.UserAgent)
                   .SetPreferredBitrate(Bitrate.Bitrate320k);
            }

            Session.LoginComplete += (sender, eventArgs) =>
            {
                wait.Set();

                _waveOut = new WaveOut {Volume = 0.25f};
            };

            Session.MusicDeliver += _session_MusicDeliver;

            if (username != null && password != null)
            {
                Session.Login(username, password, rememberme);
            }
            else
            {
                Session.Relogin();
            }

            wait.WaitOne(10000);
        }

        private void _session_MusicDeliver(object sender, MusicDeliveryEventArgs e)
        {
            int consumed = 0;

            lock (_lockObject)
            {
                if (_provider == null)
                {
                    _provider = new WriteableBufferingSource(new WaveFormat(e.Rate, 16, e.Channels, AudioEncoding.Pcm), (e.Rate * 2));
                }

                if (_waveOut.PlaybackState == PlaybackState.Stopped)
                {
                    _waveOut.Initialize(_provider);
                    _waveOut.Play();
                }

                if ((_provider.MaxBufferSize - _provider.Length) > e.Samples.Length)
                {
                    _provider.Write(e.Samples, 0, e.Samples.Length);
                    consumed = e.Frames;
                }
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

            Session.PlayerPause();
            _waveOut.Pause();
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

        public void PlaySong(ITrack song)
        {
            lock (_lockObject)
            {
                _waveOut.Stop();
                _provider = null;
            }

            if (!song.WaitUntilLoaded(500) || Session.PlayerLoad(song) != Error.OK)
            {
                return;
            }

            Session.PlayerPlay();
        }

        public void Dispose()
        {
            Session.LogoutComplete += delegate
            {
                Session.Dispose();
                _waveOut?.Dispose();
            };

            Session.PlayerUnload();

            if (IsPlaying)
            {
                Session.StopPlayback += delegate
                {
                    Session.Logout();
                };
            }
            else
            {
                Session.Logout();
            }
        }
    }
}
