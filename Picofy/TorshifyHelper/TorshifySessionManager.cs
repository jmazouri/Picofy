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
                return MusicPlayer.Current.SongPlayer.Session.PlaylistContainer.Playlists.Where(d => d.Type == PlaylistType.Playlist);
            }
        }

        public delegate void LoginFinishedHandler();
        public event LoginFinishedHandler LoginFinished;
        protected virtual void OnLoginFinished()
        {
            LoginFinished?.Invoke();
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

            foreach (var lst in Playlists)
            {
                lst.WaitUntilLoaded(2500);
            }

            MusicPlayer.Current.RequiresLogin = false;
        }
    }
}
