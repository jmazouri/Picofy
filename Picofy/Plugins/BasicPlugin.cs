using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;
using CSCore.Streams;
using Picofy.Models;
using Torshify;

namespace Picofy.Plugins
{
    public abstract class BasicPlugin : IDisposable
    {
        public virtual string Name => "Default Plugin";

        public virtual void SongPaused()
        {
            
        }

        public virtual void ShowDialog()
        {
            
        }

        public virtual void SongPlay(ITrack track)
        {
            
        }

        public virtual bool MusicDeliver(PluginMusicDeliveryArgs args)
        {
            return false;
        }

        public virtual void Dispose()
        {
        }
    }
}
