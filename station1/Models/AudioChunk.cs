using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace station1.Models
{
    internal struct AudioChunk
    {
        public int length;
        public short[] samples;
        public long timestamp;
        public AudioChunk(long timestamp, int length)
        {
            this.length = length;
            this.samples = new short[length];
            this.timestamp = timestamp;
        }
    }
}