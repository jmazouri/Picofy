using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSCore;
using CSCore.DSP;
using CSCore.SoundOut;
using CSCore.Streams;
using Discord;
using Discord.Audio;
using Picofy.Models;
using Picofy.Plugins;
using Torshify;

namespace Picofy_Discord
{
    public class PicofyDiscord : BasicPlugin
    {
        public override string Name => "Discord Plugin";

        private Server CurrentServer => _client?.AllServers.First(d => d.Name.Contains("Friently"));

        private DiscordClient _client;
        private IDiscordVoiceClient _voiceClient;
        private VolumeSource _volumeProvider;
        private IWaveSource _providerConverted;
        private AudioBuffer _buffer = new AudioBuffer();

        private bool _shouldSend = true;

        public override async void ShowDialog()
        {
            LoginDialog dialog = new LoginDialog();

            if (dialog.ShowDialog() == true)
            {
                _client = new DiscordClient(new DiscordClientConfig
                {
                    EnableVoiceEncryption = true,
                    VoiceMode = DiscordVoiceMode.Outgoing
                });

                await _client.Connect(dialog.Username, dialog.Password);
                await _client.JoinVoiceServer(CurrentServer.VoiceChannels.First(d => d.Members.Any(v => v.Name == "jmazouri")));
                _voiceClient = _client.GetVoiceClient(CurrentServer);

                _client.VoiceDisconnected += (sender, args) =>
                {
                    _shouldSend = false;
                };
            }
        }

        public override bool MusicDeliver(PluginMusicDeliveryArgs args)
        {
            if (_client == null)
            {
                return false;
            }

            if (_providerConverted == null)
            {
                _volumeProvider = new VolumeSource(args.Source.ToMono().ChangeSampleRate(48000).ToSampleSource());
                _providerConverted = _volumeProvider.ToWaveSource(16);
            }

            _volumeProvider.Volume = MusicPlayer.Current.Volume;

            byte[] buffer = new byte[128000];
            int byteCount = _providerConverted.Read(buffer, 0, buffer.Length);
            _buffer.Add(buffer, byteCount);

            if (_shouldSend && _buffer.QueueLength > 64000)
            {
                byte[] data = _buffer.ReadUntil(64000);
                _voiceClient?.SendVoicePCM(data, data.Length);
            }

            return true;
        }

        public override void SongPlay(ITrack track)
        {
            if (_client != null)
            {
                var foundChannel = CurrentServer?.Channels.First(d => d.Name == "bot_tests");
                _client?.SendMessage(foundChannel, "Now Playing: " + track.Name + " by " + track.Artists[0].Name);

                _providerConverted = null;
            }
        }

        public override void Dispose()
        {
            _client?.Disconnect();
        }
    }
}
