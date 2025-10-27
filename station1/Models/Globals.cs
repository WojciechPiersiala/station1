using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace station1.Models
{
    public static class Globals
    {
        public static int DownsampleFact = 1;

        public static int AudioLen = 6144;
        public static int SamplingRate = 16000; // 48000 6144
        public static int MaxChunks = 1;
        public static int SamplesPerChunk = AudioLen / 2;
        public static int Capacity = SamplesPerChunk * MaxChunks;
        
        public static int MaxPlotHist= 500;
        public static int Navg= 10;
        //double maxLag = 1.0; //ms

        public static double MaxLag = 1.5; // 1ms correlation thresshold
        public static double VolumeThresshols = 50.0;

        public static bool Downsample = false;

        public static int refreshPlotRate = 300; //ms
    }
}
