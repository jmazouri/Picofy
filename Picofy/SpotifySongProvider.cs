/*
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Picofy.Annotations;
using Picofy.Models;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

namespace Picofy
{
    public class SpotifySongProvider : INotifyPropertyChanged
    {
        static ImplicitGrantAuth auth;
        private SpotifyWebAPI _spotify;
        private string _clientId;

        private bool _requiresLogin;

        public bool RequiresLogin
        {
            get { return _requiresLogin; }
            set
            {
                if (value == _requiresLogin)
                {
                    return;
                }
                _requiresLogin = value;
                OnPropertyChanged();
            }
        }

        public SpotifySongProvider(string clientId)
        {
            _clientId = clientId;
            RequiresLogin = true;
        }

        public void Login()
        {
            auth = new ImplicitGrantAuth
            {
                ClientId = _clientId,
                RedirectUri = "http://localhost:8283",
                Scope = Scope.PlaylistReadPrivate
            };

            auth.StartHttpServer(8283);
            auth.OnResponseReceivedEvent += Auth_OnResponseReceivedEvent;
            auth?.DoAuth();
        }

        private void Auth_OnResponseReceivedEvent(Token token, string state)
        {
            auth.StopHttpServer();

            _spotify = new SpotifyWebAPI()
            {
                TokenType = token.TokenType,
                AccessToken = token.AccessToken
            };

            RequiresLogin = false;
        }

        public List<Song> GetFirstPlaylistSongs()
        {
            var currentUserId = _spotify.GetPrivateProfile().Id;
            var userFirstPlaylist = _spotify.GetUserPlaylists(_spotify.GetPrivateProfile().Id, 1).Items[0];

            List<Song> ret = new List<Song>();

            foreach (PlaylistTrack t in _spotify.GetPlaylistTracks(currentUserId, userFirstPlaylist.Id).Items)
            {
                if (!t.IsLocal)
                {
                    ret.Add(GetSongFromId(t.Track.Id));
                }
            }

            return ret;
        } 

        public Song GetSongFromId(string id)
        {
            if (_spotify == null) { throw new InvalidOperationException("You need to log in."); }

            FullTrack track = _spotify.GetTrack(id);

            return new Song
            {
                Id = track.Uri,
                Name = track.Name,
                Artist = new Artist
                {
                    Id = track.Artists.FirstOrDefault()?.Id,
                    Name = track.Artists.FirstOrDefault()?.Name
                },
                Album = new Album
                {
                    Name = track.Album.Name,
                    Id = track.Album.Id,
                    Provider = "Spotify",
                    CoverArt = track.Album.Images.FirstOrDefault()?.Url
                },
                Provider = "Spotify",
                TotalSeconds = track.DurationMs / 1000
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
*/