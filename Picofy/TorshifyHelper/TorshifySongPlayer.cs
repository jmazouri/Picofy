using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CSCore;
using CSCore.Codecs;
using CSCore.Codecs.WAV;
using CSCore.SoundIn;
using CSCore.SoundOut;
using CSCore.Streams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Picofy.Models;
using Picofy.Plugins;
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

        private float _volume = 0.25f;
        public float Volume
        {
            get { return _waveOut?.Volume ?? _volume; }
            set
            {
                _volume = value;
                if (_waveOut != null)
                {
                    _waveOut.Volume = _volume;
                }
            }
        }

        public bool _continuePlay = true;

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

            _waveOut = new WaveOut();

            wait.WaitOne(10000);
        }

        private void _session_MusicDeliver(object sender, MusicDeliveryEventArgs e)
        {
            int consumed = 0;

            lock (_lockObject)
            {
                if (_provider == null)
                {
                    _provider = new WriteableBufferingSource(new WaveFormat(e.Rate, 16, e.Channels, AudioEncoding.Pcm), (e.Rate * 2))
                    {
                        FillWithZeros = false
                    };
                }

                List<bool> results = MusicPlayer.Current.Plugins.Select(plugin => plugin.MusicDeliver(new PluginMusicDeliveryArgs(e, _provider))).ToList();

                _continuePlay = results.Count == 0 || results.All(result => !result);

                if (!_continuePlay)
                {
                    _waveOut?.Stop();
                    _waveOut?.Dispose();
                    _waveOut = null;
                }

                if (_continuePlay && _waveOut.PlaybackState == PlaybackState.Stopped)
                {
                    _waveOut.Initialize(_provider);
                    _waveOut.Volume = _volume;
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
            _waveOut?.Stop();
            Session.PlayerPause();
            Session.PlayerUnload();

            lock (_lockObject)
            {
                _provider = null;
            }

            var errorCode = Session.PlayerLoad(song);

            if (!song.WaitUntilLoaded(500) || errorCode != Error.OK)
            {
                MessageBox.Show("Error: " + errorCode);
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
