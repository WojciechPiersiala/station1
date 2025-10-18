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

        public static int AudioLen = 4096;
        public static double SamplingRate = 32000; // 48000 6144
        public static int MaxChunks = 2;
        public static int SamplesPerChunk = AudioLen / 2;
        public static int Capacity = SamplesPerChunk * MaxChunks;
        
        public static int MaxPlotHist= 10000; // 500 records ok
        public static int Navg= 10;
        //double maxLag = 1.0; //ms

        public static double MinValidSchiftUs = 100_000.0; //  correlation thresshold us
        public static double VolumeThresshols = 150.0;
        public static int MaxShiftCompensation = 5;

        public static bool Downsample = false;
    }
}
