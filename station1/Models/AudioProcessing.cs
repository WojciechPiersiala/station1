using HarfBuzzSharp;
using MathNet.Numerics.IntegralTransforms;
using Microsoft.VisualBasic.Logging;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;


namespace station1.Models
{
    public static class AudioProcessing
    {
        private const bool saveAll = false;
        private const string exportCsvPath = @"C:\Users\wp1\Desktop\Studia\magisterka\Acustic_source_detection\matlab\Data\";
        private const string tag = "AudioProcessing";
        private const bool logAudioProcessing = false;


        //private static void if (saveAll)  saveTandYtoCsv(double[] Y, string saveFile)
        //{
        //    Logger.I(tag, $"Exporting data to {saveFile}");
        //    List<string> lines = new();
        //    var inv = CultureInfo.InvariantCulture;
        //    int N = Y.Length;
        //    StringBuilder sb = new();

        //    for (int i = 0; i < N; i++)
        //    {
        //        string audioSample = Y[i].ToString("R", inv);
        //        sb.Append(audioSample).Append(",");
        //    }


        //    sb.Length--;
        //    lines.Add(sb.ToString());
        //    string path = exportCsvPath + saveFile + ".csv";
        //    File.WriteAllLines(path, lines);
        //}



        //public void ExportData()
        //{
        //    Logger.I(tag, "Exporting data");
        //    var inv = CultureInfo.InvariantCulture;

        //    var snap = plotBuffer.Values.ToList();
        //    if (snap.Count < 0)
        //    {
        //        Logger.W(tag, "No data to save to csv");
        //        return;
        //    }
        //    List<string> lines = new();


        //    StringBuilder sbHeader = new();
        //    foreach (var s in snap)
        //    {
        //        sbHeader.Append($"t{s.id},").Append($"y{s.id},");
        //    }
        //    sbHeader.Length--;
        //    string header = sbHeader.ToString();
        //    lines.Add(header);

        //    for (int i = 0; i < Capacity; i++)
        //    {
        //        StringBuilder sb = new();
        //        foreach (var s in snap)
        //        {
        //            string timeSampe = s.X[i].ToString("R", inv);
        //            string audioSample = s.Y[i].ToString("R", inv);
        //            sb.Append(timeSampe).Append(",").Append(audioSample).Append(",");
        //        }
        //        sb.Length--;
        //        //sb.RemoveAt(sb.Count - 1);
        //        lines.Add(sb.ToString());

        //    }
        //    double currTime = stopWatch.Elapsed.TotalMilliseconds;
        //    string path = exportCsvPaht + $"{currTime.ToString()}.csv";
        //    File.WriteAllLines(path, lines);

        //    Logger.I(tag, $"Exporting dataset: {header} to file {currTime.ToString()}.csv");
        //}



        public static void saveTandYtoCsv(double[] T, double[] Y, string saveFile)
        {
            if(logAudioProcessing) Logger.I(tag, $"Exporting data to {saveFile}");
            List<string> lines = new();
            var inv = CultureInfo.InvariantCulture;
            int N = Y.Length;

            lines.Add("T,Y");
            for (int i = 0; i < N; i++)
            {
                string timeSampe = T[i].ToString("R", inv);
                string audioSample = Y[i].ToString("R", inv);
                StringBuilder sb = new();
                sb.Append(timeSampe).Append(",").Append(audioSample);
                //sb.Length--;
                lines.Add(sb.ToString());
            }
            string path = exportCsvPath + saveFile + ".csv";
            File.WriteAllLines(path, lines);
        }


        private static void trimNoise(ref double[] Y1, ref double[] Y2)
        {
            const int cutOffAmp = 300;
            for (int i = 0; i < Y1.Length; i++)
            {
                if (Math.Abs(Y1[i]) < cutOffAmp)
                    Y1[i] = 0;

                if (Math.Abs(Y2[i]) < cutOffAmp)
                    Y2[i] = 0;
            }
        }

        public static void sortAudio(double[] T, double[] Y)
        {
            Array.Sort(T, Y);
        }




        public static double findTimeShiftAsync(double[] T1, double[] Y1, double[] T2, double[] Y2, double maxLagMs = 20, CancellationToken clcTok = default)
        {

            //trimNoise(ref Y1, ref Y2);  //tmp

            double Y1max = Y1.Max();
            double Y2max = Y2.Max();
            double minThresshold = Globals.VolumeThresshols;
            if (Y1max < minThresshold || Y2max < minThresshold)
            {
                if (logAudioProcessing) Logger.W(tag, "One of the signals is too weak to process");
                return double.NaN;
            }

            if (saveAll)  saveTandYtoCsv(T1, Y1, "t1y1unordered");
            if (saveAll)  saveTandYtoCsv(T2, Y2, "t2y2unordered");
            // sort unordered data
            if (logAudioProcessing) Logger.I(tag, "Sorting data...");
            //sortAudio(T1, Y1);
            //sortAudio(T2, Y2);


            LinearizeRingInPlace(T1, Y1);
            LinearizeRingInPlace(T2, Y2);

            if (saveAll)  saveTandYtoCsv(T1, Y1, "t1y1ordered");
            if (saveAll)  saveTandYtoCsv(T2, Y2, "t2y2ordered");
            // Resample to uniform grid 
            // Build a common uniform time grid over the overlap
            double Tmax = Math.Max(T1.Max(), T2.Max());
            double Tmin = Math.Min(T1.Min(), T2.Min());

            double measureWindow = Tmax - Tmin;
            if (measureWindow > Globals.AudioLen*4)
            {
                Logger.W(tag, $"Very large measure window, signals may not overlap. Measure window: {measureWindow} ms > {Globals.AudioLen * 4} ms ");
                return double.NaN;
            }
            //else
            //{
            //    Logger.I(tag, $"Measure window OK: {measureWindow} ms > {Globals.AudioLen * 4} ms ");
            //}

            //Logger.I(tag, $"Measure window: {measureWindow} ms");
            if (Tmax < Tmin)
            {
                if (logAudioProcessing) Logger.E(tag, "No overlap between samples");
                // TODO: ADD ERROR
            }

            //Choose grid step as the finer of the two median spacings
            double dt1 = T1[1] - T1[0];
            double dt2 = T2[1] - T2[0];

            //double dt1 = T1.Average();
            //double dt2 = T2.Average();

            double dt = Math.Min(dt1, dt2); // get the sample time

            if(Globals.Downsample)
                dt *= Globals.DownsampleFact; // downsample to speed up the calculations, 8 is arbitrary

            int n = (int)Math.Floor((Tmax - Tmin) / dt) + 1;
            double[] t = Enumerable.Range(0, n).Select(i => Tmin + i * dt).ToArray(); //uniform time series combaining T1 and T2, used for interpolation
            if (logAudioProcessing) Logger.I(tag, "Interpolating data...");
            //Interpolate onto the common grid
            double[] y1i = interploate(T1, Y1, t);
            double[] y2i = interploate(T2, Y2, t);
            if (saveAll)  saveTandYtoCsv(t, y1i, "ty1interpolated");
            if (saveAll)  saveTandYtoCsv(t, y2i, "ty2interpolated");
            if (logAudioProcessing) Logger.I(tag, "Calculating crosscorelation data...");

            deMean(ref t, ref y1i);
            normalize(ref t, ref y1i);
            deMean(ref t, ref y2i);
            normalize(ref t, ref y2i);
#if USE_CORR
            double corr = calcCorrelation(t, y1i, y2i);
#else
            double corr = FindTimeShiftPhat( y1i, y2i, ((double)Globals.SamplingRate), maxLagMs);
#endif
            return corr;// ms
        }

    
        public static void deMean(ref double[]T, ref double[] Y)
        {
            double mean = 0;
            for (int i = 0; i < Y.Length; i++)
                mean += Y[i];
            mean /= Y.Length;

            for (int i = 0; i < Y.Length; i++)
                Y[i] -= mean;
        }

        public static void normalize(ref double[] T, ref double[] Y)
        {
            double max = Math.Abs(Y[0]);
            for (int i = 1; i < Y.Length; i++)
            {
                if (Math.Abs(Y[i]) > max)
                    max = Math.Abs(Y[i]);
            }
            if (max == 0) return;
            for (int i = 0; i < Y.Length; i++)
                Y[i] /= max;
        }



        public static void LinearizeRingInPlace(double[] T, double[] Y)
        {
            // Find first point where time decreases -> wrap pivot
            int n = T.Length;
            int pivot = 0;
            for (int i = 1; i < n; i++)
            {
                if (T[i] < T[i - 1]) { pivot = i; break; }
            }
            if (pivot == 0) return; // already linear

            // Rotate [pivot..n-1] + [0..pivot-1] into a contiguous view
            var t = new double[n];
            var y = new double[n];

            int tail = n - pivot;
            Array.Copy(T, pivot, t, 0, tail);
            Array.Copy(T, 0, t, tail, pivot);
            Array.Copy(Y, pivot, y, 0, tail);
            Array.Copy(Y, 0, y, tail, pivot);

            Array.Copy(t, T, n);
            Array.Copy(y, Y, n);
        }



        /// <summary>
        /// Wyznacza opoznienie wzgledem sygnalu referencyjnego, wykorzystujac prosta korelacje
        /// </summary>
        /// <param name="T1"> sereg czasowy </param>
        /// <param name="Y1"> referencyjny sygnal dzwiekowy </param>
        /// <param name="Y2"> porownywany sygnal dzwiekowy </param>
        /// <returns>obliczone przesuniecie w czasie</returns>
        public static double calcCorrelation(double[] T1, double[] Y1, double[] Y2) 
        {
            double dt = T1[1] - T1[0]; // oblicz dt - czas trwania jednej probki
            int N = T1.Length; // oblicz dlugosc sygnalu dzwiekowego


            double[] zeros = new double[N];
            double[] Y2pad = zeros.Concat(Y2).ToArray().Concat(zeros).ToArray(); // dopelneinie sygnalu T2 zerami, trzykrotne zwiekrzenie okna obliczen
            double[] corrArray = new double[2 * N];
            double[] timeShifts = new double[corrArray.Length];

            for (int i = 0; i < N * 2; i++) // petla przez wszystkie probki
            {
                double corr = 0;
                for (int j = 0; j < N; j++)
                {
                    corr += Y1[j] * Y2pad[j + i];
                }
                corrArray[i] = corr;
                timeShifts[i] = (i - N) * dt;
            }
            int maxIndex = Array.IndexOf(corrArray, corrArray.Max());
            double shiftSamples = maxIndex - N;
            double shiftTime = shiftSamples * dt;

            if (saveAll)  saveTandYtoCsv(timeShifts, corrArray, "correlation"); // logi
            if (logAudioProcessing) Logger.I($"calculated shift time: {shiftTime} ms");
            return shiftTime;
        }


        /// <summary>
        /// Wyznacza opoznienie wzgledem sygnalu referencyjnego, wykorzystujac prosta korelacje
        /// </summary>
        /// <param name="y1"> sygnal referencyjny </param>
        /// <param name="y2"> porownywany sygnal </param>
        /// <param name="sampleRateHz"> czestotliwosc probkowania</param>
        /// <param name="maxLagMs"> maksymalne dopuszczalne przesuniecie</param>
        /// <returns> przesuniecie sygnalow w ms</returns>
        public static double FindTimeShiftPhat(double[] y1, double[] y2, double sampleRateHz, double maxLagMs = 20)
        {
            int N = y1.Length;
            int M = 1;
            while (M < 2 * N)
                M = M * 2;

            var X = new Complex[M];
            var Y = new Complex[M];
            for (int i = 0; i < N; i++)
            { 
                X[i] = new Complex(y1[i], 0);
                Y[i] = new Complex(y2[i], 0);
            }

            Fourier.Forward(X, FourierOptions.NoScaling);
            Fourier.Forward(Y, FourierOptions.NoScaling);

            var R = new Complex[M];
            for (int i = 0; i < M; i++)
            {
                Complex c = Complex.Conjugate(X[i]) * Y[i];
                double mag = c.Magnitude;
                if (mag > 0.000000000001)
                    R[i] = c / mag;
                else
                    R[i] = Complex.Zero;
            }
            Fourier.Inverse(R, FourierOptions.NoScaling);

            // znajdz maksimum
            int kMax = 0;
            double vMax = double.NegativeInfinity;
            for (int k = 0; k < M; k++)
            {
                double v = R[k].Real;
                if (v > vMax) { vMax = v; kMax = k; }
            }

            int timeLag;
            if (kMax > M / 2)
                timeLag = kMax - M;
            else
                timeLag = kMax;

            // interpolacja kwadratowa wokol maksimum
            int km1 = (kMax - 1 + M) % M;
            int kp1 = (kMax + 1) % M;
            double denom = R[km1].Real - 2 * R[kMax].Real + R[kp1].Real;
            double fraction; // ulamkowa czesc przesuniecia miedzy sygnalami
            if (Math.Abs(denom) > 0.000000000001)
                fraction = 0.5 * (R[km1].Real - R[kp1].Real) / denom;
            else
                fraction = 0.0;

            double lagSamples = timeLag + fraction;

            // ogranicz do abs(maxLagMs)
            double lagMs = (lagSamples / sampleRateHz) * 1000.0;
            if (Math.Abs(lagMs) > maxLagMs)
                return double.NaN;

            return lagMs;
        }




        //public static double resampleToUniformGrid(double[] T1, double[] Y1, double[] T2, double[] Y2)
        //{


        //    //Cross - correlation to get lag in samples on this grid
        //    return 0.0;
        //}


        /// <summary>
        /// Interpolacja liniowa sygnalu Y1 z punktow T1 do punktow Tq
        /// </summary>
        /// <param name="T1"> Orginalna seria czasowa</param>
        /// <param name="Y1"> Orginalny sygnal dzwiekowy  </param>
        /// <param name="Tq"> Nowa seria czasowa </param>
        /// <returns> Sygnal audio przekonwertowany to nowej bazy czasowej </returns>
        public static double[] interploate(double[] T1, double[] Y1, double[] Tq)
        {
            double[] Yq = new double[Tq.Length]; // skwantyfikowane probki

            for (int i = 0; i < Tq.Length; i++)
            {
                double t = Tq[i];
                // pomin probki poza zakresem
                if (t < T1.First() || t > T1.Last())
                {
                    Yq[i] = 0;
                    continue;
                }

                int findT = Array.BinarySearch(T1, t);
                if (findT >= 0) // dokladne dopasowanie
                {
                    Yq[i] = Y1[findT];
                }
                else // interpolacja liniowa
                {
                    int insertionIndex = ~findT;      // indeks, gdzie należałoby wstawić t
                    int leftIndex = insertionIndex - 1; // lewy sąsiad
                    findT = leftIndex;
                    double t0 = T1[findT], t1 = T1[findT + 1];
                    double y0 = Y1[findT], y1 = Y1[findT + 1];
                    double weight = (t - t0) / (t1 - t0); // waga interpolacji

                    Yq[i] = y0 + weight * (y1 - y0);
                }
            }
            return Yq;
        }
    }


}