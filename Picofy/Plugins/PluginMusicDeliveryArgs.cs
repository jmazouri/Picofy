using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore.Streams;
using Torshify;

namespace Picofy.Plugins
{
    public class PluginMusicDeliveryArgs
    {
        public byte[] Samples { get; }
        public int Channels { get; }
        public int Frames { get; }
        public int Rate{ get; }
        public WriteableBufferingSource Source { get; }

        public PluginMusicDeliveryArgs(MusicDeliveryEventArgs args, WriteableBufferingSource source)
        {
            Samples = args.Samples;
            Channels = args.Channels;
            Frames = args.Frames;
            Rate = args.Rate;

            Source = source;
        }
    }
}
