﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSCore;
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
        private IWaveSource _providerConverted;
        private AudioBuffer _buffer = new AudioBuffer();

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
                _providerConverted = args.Source.ToMono().ToSampleSource().ToWaveSource(16).ChangeSampleRate(48000);
            }

            byte[] buffer = new byte[128000];
            int byteCount = _providerConverted.Read(buffer, 0, buffer.Length);
            _buffer.Add(buffer, byteCount);

            if (_buffer.QueueLength >= 256000)
            {
                byte[] data = _buffer.ReadUntil(128000);
                _voiceClient.SendVoicePCM(data, data.Length);
            }

            return true;
        }

        public override void SongPlay(ITrack track)
        {
            if (_client != null)
            {
                var foundChannel = CurrentServer?.Channels.First(d => d.Name == "bot_tests");
                _client?.SendMessage(foundChannel, "Now Playing: " + track.Name + " by " + track.Artists[0].Name);
            }
        }

        public override void Dispose()
        {
            _client?.Disconnect();
        }
    }
}