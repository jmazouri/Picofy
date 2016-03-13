using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Picofy.Models;
using Torshify;

namespace Picofy.TorshifyHelper
{
    public class TorshifySessionManager
    {
        public IEnumerable<IContainerPlaylist> Playlists
        {
            get
            {
                return LoadAndRetrievePlaylists().Where(d => d.Type == PlaylistType.Playlist);
            }
        }

        public ISession Session
        {
            get
            {
                return MusicPlayer.Current.SongPlayer.Session;
            }
        }

        public delegate void LoginFinishedHandler();
        public event LoginFinishedHandler LoginFinished;
        protected virtual void OnLoginFinished()
        {
            LoginFinished?.Invoke();
        }

        public IEnumerable<IContainerPlaylist> LoadAndRetrievePlaylists()
        {
            MusicPlayer.Current.SongPlayer.Session.PlaylistContainer.WaitUntilLoaded();

            foreach (IContainerPlaylist playlist in MusicPlayer.Current.SongPlayer.Session.PlaylistContainer.Playlists)
            {
                playlist.WaitUntilLoaded();
                playlist.TracksAdded += (sender, args) =>
                {
                    playlist.WaitUntilLoaded();
                };
            }

            return MusicPlayer.Current.SongPlayer.Session.PlaylistContainer.Playlists;
        } 

        public bool Login(string username = null, string password = null, bool rememberme = true)
        {
            if (TorshifySongPlayer.HasSavedCredentials())
            {
                PerformConnection(null, null, rememberme);
                OnLoginFinished();
                return true;
            }

            if (username != null && password != null)
            {
                try
                {
                    PerformConnection(username, password, rememberme);
                    OnLoginFinished();
                    return true;
                }
                catch (TorshifyException)
                {
                    return false;
                }              
            }

            return false;
        }

        private void PerformConnection(string username, string password, bool rememberme)
        {
            MusicPlayer.Current.Connect(username, password, rememberme);
            MusicPlayer.Current.SongPlayer.Session.PlaylistContainer.WaitUntilLoaded();

            MusicPlayer.Current.RequiresLogin = false;
        }
    }
}
