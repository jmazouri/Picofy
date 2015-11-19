using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Models;
using TRock.Music;

namespace Picofy
{
    public class SpotifySongProvider
    {
        private SpotifyWebAPI _spotify;

        public SpotifySongProvider()
        {
            _spotify = new SpotifyWebAPI()
            {
                UseAuth = false, //This will disable Authentication.
            };
        }

        public Song GetSongFromId(string id)
        {
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
    }
}
