using HarfBuzzSharp;
using Microsoft.VisualBasic.Logging;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace station1.Models
{
    public static class AudioProcessing
    {
        private const string exportCsvPath = @"C:\Users\wp1\Desktop\Studia\magisterka\Acustic_source_detection\matlab\Data\";
        private const string tag = "AudioProcessing";


        //private static void saveTandYtoCsv(double[] Y, string saveFile)
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



        public static void saveTandYtoCsv(double[]T, double[] Y, string saveFile)
        {
            Logger.I(tag, $"Exporting data to {saveFile}");
            List<string> lines = new();
            var inv = CultureInfo.InvariantCulture;
            int N = Y.Length;

            lines.Add("T,Y");
            for (int i = 0; i < N; i++) { 
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


        //private static void stripNoise(ref double[] Y1,ref double[] Y2)
        //{
        //    for(int i=0; i<Y1.Length; i++)
        //    {
        //        if (Y1[i] < 150)
        //            Y1[i] = 0;

        //        if (Y2[i] < 150)
        //            Y2[i] = 0;
        //    }
        //}

        public static void sortAudio(double[] T, double[] Y)
        {
            Array.Sort(T, Y);
        }




        public static Task <double> findTimeShiftAsync(double[] T1, double[] Y1, double[] T2, double[] Y2, CancellationToken clcTok = default)
        {
            double Y1max = Y1.Max();
            double Y2max = Y2.Max();
            double minThresshold = 1000.0;
            if (Y1max < minThresshold || Y2max < minThresshold)
            {
                Logger.W(tag, "One of the signals is too weak to process");
                return Task.FromResult(double.NaN);
            }

            saveTandYtoCsv(T1, Y1, "t1y1unordered");
            saveTandYtoCsv(T2, Y2, "t2y2unordered");
            // sort unordered data
            Logger.I(tag, "Sorting data...");
            sortAudio(T1, Y1);
            sortAudio(T2, Y2);

            saveTandYtoCsv(T1, Y1, "t1y1ordered");
            saveTandYtoCsv(T2, Y2, "t2y2ordered");
            // Resample to uniform grid 
            // Build a common uniform time grid over the overlap
            double Tmax = Math.Max(T1.Max(), T2.Max());
            double Tmin = Math.Min(T1.Min(), T2.Min());

            if (Tmax < Tmin)
            {
                Logger.E(tag, "No overlap between samples");
                // TODO: ADD ERROR
            }

            //Choose grid step as the finer of the two median spacings
            double dt1 = T1[1] - T1[0];
            double dt2 = T2[1] - T2[0];

            double dt = Math.Min(dt1, dt2); // get the sample time

            int n = (int)Math.Floor((Tmax - Tmin) / dt) + 1;
            double[] t = Enumerable.Range(0, n).Select(i => Tmin + i * dt).ToArray(); //uniform time series combaining T1 and T2, used for interpolation
            Logger.I(tag, "Interpolating data...");
            //Interpolate onto the common grid
            double[] y1i = interploate(T1, Y1, t);
            double[] y2i = interploate(T2, Y2, t);
            saveTandYtoCsv(t, y1i, "ty1interpolated");
            saveTandYtoCsv(t, y2i, "ty2interpolated");
            Logger.I(tag, "Calculating crosscorelation data...");
            return Task.Run(() => calcCorrelation(t, y1i, t, y2i), clcTok);
        }


        public static double calcCorrelation(double[] T1, double[] Y1, double[] T2, double[] Y2)
        {

            double dt = T1[1] - T1[0];
            // claculates pseudo correlation
            int N = T1.Length; //audio chunk lenght is always the same.
            

            double[] zeros = new double[N];
            double[] Y2pad = zeros.Concat(Y2).ToArray().Concat(zeros).ToArray(); //padding y2 with zeros, increase the callculation window by 3 times
            double[] corrArray = new double[2 * N];
            double[] timeShifts = new double[corrArray.Length];

            for (int i = 0; i < N*2; i++)
            {
                double corr = 0;
                for (int j = 0; j < N; j++)
                {
                    corr += Y1[j] * Y2pad[j + i];
                }
                corrArray[i] = corr;
                timeShifts[i] = (i -N) * dt;
            }
            int maxIndex = Array.IndexOf(corrArray, corrArray.Max());
            double shiftSamples = maxIndex - N;
            double shiftTime = shiftSamples * dt;

            saveTandYtoCsv(timeShifts, corrArray, "correlation");
            Logger.I($"calculated shift time: {shiftTime} ms");
            return shiftTime;
        }



        //public static double resampleToUniformGrid(double[] T1, double[] Y1, double[] T2, double[] Y2)
        //{


        //    //Cross - correlation to get lag in samples on this grid
        //    return 0.0;
        //}
        


        public static double[] interploate(double[] T1, double[] Y1, double[] Tq)
        {
            double[] Yq = new double[Tq.Length]; // quantified samples;

            for (int i = 0; i < Tq.Length; i++)
            {
                double t = Tq[i];
                // outside the range
                if (t < T1.First() || t > T1.Last())
                {
                    Yq[i] = 0;
                    continue;
                }

                int findT = Array.BinarySearch(T1, t);
                if (findT >= 0) // exact match
                {
                    Yq[i] = Y1[findT];
                }
                else // interpolate
                {
                    findT = ~findT - 1; //left neighbour of insertion indes  (The index where the value would be inserted to keep the array sorted.)
                    double t0 = T1[findT], t1 = T1[findT + 1];
                    double y0 = Y1[findT], y1 = Y1[findT + 1];
                    double w = (t - t0) / (t1 - t0); // linear weight

                    Yq[i] = y0 + w * (y1 - y0);
                }
            }
            return Yq;
        }


    }


}
