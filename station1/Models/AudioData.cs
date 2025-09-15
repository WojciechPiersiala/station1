using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace station1.Models
{
    internal class AudioData
    {
        public int length;
        public short[] samples; 
        public int timestamp;
        public AudioData(int timestamp, int length)
        {   
            this.length = length;
            this.samples = new short[length];
            this.timestamp = timestamp;
        }
    }
}
