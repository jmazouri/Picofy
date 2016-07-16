using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSCore;
using CSCore.Codecs.WAV;
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

        private Server CurrentServer => _client?.FindServers("Friently Gamers").FirstOrDefault();

        private DiscordClient _client;
        private IAudioClient _voiceClient;
        private VolumeSource _volumeProvider;
        private IWaveSource _providerConverted;

        public override void SongPaused()
        {
            _voiceClient?.Clear();
        }

        public override async void ShowDialog()
        {
            LoginDialog dialog = new LoginDialog();

            if (dialog.ShowDialog() == true)
            {
                _client = new DiscordClient();
                _client.UsingAudio(x =>
                {
                    x.Mode = AudioMode.Outgoing;
                    x.Bitrate = null;
                    x.Channels = 2;
                });

                await _client.Connect(dialog.Username, dialog.Password);
                var voiceChannel = CurrentServer.VoiceChannels.FirstOrDefault(d => d.Name == "Bot Test");

                _voiceClient = await _client.GetService<AudioService>()
                    .Join(voiceChannel);
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
                _volumeProvider = new VolumeSource(args.Source.ChangeSampleRate(48000).ToSampleSource());
                _providerConverted = new BufferSource(_volumeProvider.ToWaveSource(16), _volumeProvider.WaveFormat.BytesPerSecond * 4);
            }

            _volumeProvider.Volume = MusicPlayer.Current.Volume;

            byte[] buffer = new byte[_volumeProvider.WaveFormat.BytesPerSecond];
            int byteCount = _providerConverted.Read(buffer, 0, buffer.Length);

            if (byteCount > 0)
            {
                _voiceClient?.Send(buffer, 0, byteCount);
            }
           

            return true;
        }

        public override void SongPlay(ITrack track)
        {
            if (_client != null)
            {
                var foundChannel = CurrentServer?.TextChannels.First(d => d.Name == "bot_tests");
                foundChannel.SendMessage("Now Playing: " + track.Name + " by " + track.Artists[0].Name);

                _providerConverted = null;
            }
        }

        public override void Dispose()
        {
            _client?.Disconnect();
        }
    }
}
