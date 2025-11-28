using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace station1.Models
{
    /// <summary>
    /// Struktura przechowujaca kawalek danych audio
    /// </summary>
    /// <remarks> uzywana w kolejce pakietow audio, jest zdefiniowana tak samo w kliencie </remarks>
    internal struct AudioChunk
    {
        public int length;
        public short[] samples;
        public long timestamp;
        public long seq;
        public AudioChunk(long timestamp, int length, long seq)
        {
            this.length = length;
            this.samples = new short[length];
            this.timestamp = timestamp;
            this.seq = seq;
        }
    }
}