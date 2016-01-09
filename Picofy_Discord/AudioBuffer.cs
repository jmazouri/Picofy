using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Picofy_Discord
{
    public class AudioBuffer
    {
        private Queue<byte[]> AudioQueue = new Queue<byte[]>();
         
        public void Add(byte[] buffer, int length)
        {
            AudioQueue.Enqueue(buffer.Take(length).ToArray());
        }

        public byte[] ReadUntil(long bytes)
        {
            List<byte> bufferData = new List<byte>();

            while (bufferData.Count < bytes)
            {
                var taken = AudioQueue.Dequeue();
                bufferData.AddRange(taken);
            }

            return bufferData.ToArray();
        }

        public long QueueLength => AudioQueue.Sum(d => d.LongLength);
    }
}
