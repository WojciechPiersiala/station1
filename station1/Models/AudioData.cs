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
        public AudioData(int timestamp)     
        {   
            this.length = 1024;
            this.samples = new short[length];
            this.timestamp = timestamp;
        }
    }
}
