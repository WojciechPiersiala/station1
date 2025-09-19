using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace station1.Models
{
    public static class Globals
    {
        public static int AudioLen = 4096;
        public static int AudioChunks = 1;
        public static int SamplingRate = 16000;
        public static int MaxChunks = 1;
        public static int SamplesPerChunk = AudioLen / 2;
        public static int Capacity = SamplesPerChunk * MaxChunks;
        public static int DownsampleFact = 1;
        public static int MaxPlotHist= 1000;

        public static double MinValidSchiftUs = 10.0; // 1ms
        public static double VolumeThresshols = 50.0;

        public static bool Downsample = false;
    }
}
