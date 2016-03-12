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
            }

            return MusicPlayer.Current.SongPlayer.Session.PlaylistContainer.Playlists;
        } 

        public void Login(string username = null, string password = null, bool rememberme = true)
        {
            if (TorshifySongPlayer.HasSavedCredentials())
            {
                PerformConnection(null, null, rememberme);
            }
            else
            {
                if (username != null && password != null)
                {
                    PerformConnection(username, password, rememberme);
                }
                else
                {
                    return;
                }
            }

            OnLoginFinished();
        }

        private void PerformConnection(string username, string password, bool rememberme)
        {
            MusicPlayer.Current.Connect(username, password, rememberme);
            MusicPlayer.Current.SongPlayer.Session.PlaylistContainer.WaitUntilLoaded();

            MusicPlayer.Current.RequiresLogin = false;
        }
    }
}
