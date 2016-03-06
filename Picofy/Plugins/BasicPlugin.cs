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

        public abstract void SongPaused();

        public abstract void ShowDialog();

        public abstract void SongPlay(ITrack track);

        public virtual bool MusicDeliver(PluginMusicDeliveryArgs args)
        {
            return false;
        }

        public virtual void Dispose()
        {
        }
    }
}
