using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Picofy_Discord
{
    public class AudioBuffer
    {
        private List<byte> AudioQueue;

        public AudioBuffer()
        {
            AudioQueue = new List<byte>();
        }

        public void Add(IEnumerable<byte> buffer)
        {
            AudioQueue.AddRange(buffer);
        }

        public byte[] ReadAll()
        {
            byte[] ret = AudioQueue.ToArray();
            AudioQueue.Clear();
            return ret;
        }

        public long QueueLength => AudioQueue.Count;
    }
}
