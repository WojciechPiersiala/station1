using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace station1.Models
{
    /// <summary>
    /// Zbior globalnych stalej uzywanych w aplikacji
    /// </summary>
    public static class Globals
    {
        public static int DownsampleFact = 1; // zmiana probkowania, uzywana prezez gcc-phat do przyspieszenia obliczen

        /* Na kliencie musi byc tak samo */
        public static int AudioLen = 6144; // dlugosc wiadomosci. Tak samo w kliencie
        public static int SamplingRate = 16000; // czestotliwosc probkowania mikrofonu w Hz

        public static int MaxChunks = 1; // ilosc chunkow w wektorach X i Y
        public static int SamplesPerChunk = AudioLen / 2;   // ilosc probek w jednym chunku (2 bajty na probek)
        public static int Capacity = SamplesPerChunk * MaxChunks;

        public static int MaxPlotHist = 200; //ile danych ma byc wyswietlanych na wykresie
        public static int Navg = 10;


        public static double MaxLag = 1.5; // Zakres przeszukiwania w gcc-phat w ms
        public static double VolumeThresshols = 50.0; //Minimalna wartosc glosnosci do liczenia TDoA

        public static bool Downsample = false;

        public static int refreshPlotRate = 300; //ms

        public static string exportCsvPaht = @"C:\Users\wp1\Desktop\Studia\magisterka\Acustic_source_detection\matlab\Data\";
    }
}
