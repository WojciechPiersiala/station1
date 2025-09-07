using ScottPlot;
using ScottPlot.Colormaps;
using ScottPlot.Statistics;
using ScottPlot.WinForms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace station1.Models
{

    internal class PdmPlotter
    {
        private static string tag = "plotter";
        public static double yLimMin = double.MaxValue;
        public static double yLimMax = 0;


        const int maxChunks = 16; // 16 -> 1.04 s //16 old
        const int SamplesPerChunk = 1024; // number of audio samples in a single chunk
        private const int Capacity = SamplesPerChunk * maxChunks;
        

        internal class AudioRecord
        {
            public int id = -1;
            private double offsetY = 0;
            private bool doSynch = true;
            public readonly List<double> X = new();  // accumulated time in ms
            public readonly List<double> Y = new();  // accumulated audio samples
            public bool recordFull = false;

            public AudioRecord(int id)
            {
                this.id = id;
            }

            //public string[] ExportDataRow()
            //{
            //    var linesX = this.X.Select(v => v.ToString("R", CultureInfo.InvariantCulture));
            //    var linesY = this.Y.Select(v => v.ToString("R", CultureInfo.InvariantCulture));
            //}

            public void appendData(ClientChannel clientChannel, double serverNowMs)
            {
                while (clientChannel.sampleQueue.TryDequeue(out AudioData samples))
                {
                    double packetStart_ms = ((double)samples.timestamp / 1000.0); //ms
                    bool synchRequired = (clientChannel.offsetMs == null) || doSynch;
                    if (synchRequired)
                    {
                        // synchronise time series 
                        //new client offset is not assigned yet.
                        Logger.I(tag, $"Synchronising client: {clientChannel.id}");
                        clientChannel.offsetMs = serverNowMs - packetStart_ms;
                        doSynch = false;
                    }
                    double start_ms = (double)(packetStart_ms + clientChannel.offsetMs);

                    double dt_ms = 1 / 16000.0 * 1000.0; // 0.0625 ms per sample
                    double stop_ms = start_ms + (dt_ms * samples.length);

                    double[] xs = Enumerable.Range(0, samples.length)
                        .Select(i => start_ms + i * dt_ms)
                        .ToArray();

                    double[] ys = samples.samples.Select(s => (double)s - offsetY).ToArray();

                    // Accumulate data   // TODO: USE CIRCULAR BUFFER INSTEAD 
                    X.AddRange(xs);
                    Y.AddRange(ys);

                    if (synchRequired) // normalize the data during synchronisation
                    {
                        //remove offset in audio data
                        double avgY = Y.Average();
                        if (Math.Abs(avgY) > 15)
                            offsetY = Y.Average();
                    }
                    recordFull = (X.Count >= maxChunks * samples.length);
                }
            }

            public void Synch()
            {
                doSynch = true;
            }

            public void prepareData(ref FormsPlot formsPlotRef)
            {
                if (this.X.Count == 0)
                    return;

                var scatter = formsPlotRef.Plot.Add.Scatter(this.X.ToArray(), this.Y.ToArray());
                scatter.LegendText = $"Client {this.id}";

                // set the limits 
                if (this.X.ToArray().Last() > yLimMax)
                    yLimMax = this.X.ToArray().Last();

                if (this.X.ToArray().First() < yLimMin)
                    yLimMin = this.X.ToArray().First();

                int over = X.Count - Capacity;
                if (over > 0)
                {
                    X.RemoveRange(0, over);   // drop oldest 'over' samples
                    Y.RemoveRange(0, over);
                }
#if false
                if (this.recordFull)
                {
                    this.X.Clear(); // Clear accumulated data after plotting
                    this.Y.Clear();
                    this.recordFull = false;
                }
#endif
            }
        }// AudioRecord class





        private Stopwatch stopWatch = Stopwatch.StartNew();
        private int countClients = 0;
        public FormsPlot formsPlotRef; // reference to windows form plot
        private List<ClientChannel> clientsBuffer;
        private ConcurrentDictionary<ClientChannel, AudioRecord> plotBuffer = new ();
        public PdmPlotter(FormsPlot formsPlotRef, List<ClientChannel> clientsBuffer)
        {
            this.formsPlotRef = formsPlotRef;
            this.clientsBuffer = clientsBuffer;
        }
        public void Synch()
        {
            foreach (var it in plotBuffer)
                it.Value.Synch();
        }


        public void ExportData()
        {
            Logger.I(tag, "Exporting data");
            var inv = CultureInfo.InvariantCulture;

            var snap = plotBuffer.Values.ToList();
            if (snap.Count < 0)
            {
                Logger.W(tag, "No data to save to csv");
                return;
            }   
            List<string> lines = new();


            StringBuilder sbHeader = new();
            foreach (var s in snap)
            {
                sbHeader.Append($"t{s.id},").Append($"y{s.id},");
            }
            sbHeader[sbHeader.Length - 1] = ';';
            string header = sbHeader.ToString();
            lines.Add(header);

            for (int i = 0; i < Capacity; i++)
            {
                StringBuilder sb = new();
                foreach(var s in snap)
                {
                    string timeSampe = s.X[i].ToString("R", inv);
                    string audioSample = s.Y[i].ToString("R", inv);
                    sb.Append(timeSampe).Append(",").Append(audioSample).Append(",");
                }
                sb[sb.Length - 1] = ';';
                lines.Add(sb.ToString());

            }
            double  currTime = stopWatch.Elapsed.TotalMilliseconds;
            string path = @$"C:\Users\wp1\Desktop\Studia\magisterka\Acustic_source_detection\station1\Data\{currTime.ToString()}.csv";
            File.WriteAllLines(path, lines);
        }

        public async Task Plot(CancellationToken clcTok)
        {
            formsPlotRef.Plot.Axes.SetLimitsY(-1000, 1000);
            Logger.I(tag, "Plotter started");
            int conCountPrev= 0;
            while (!clcTok.IsCancellationRequested)
            {
                List<ClientChannel> snap; // snapshot of current clients
                // Ensure every current client has an AudioRecord
                lock (clientsBuffer) snap = clientsBuffer.ToList();
                foreach (var c in snap)
                {
                    plotBuffer.TryAdd(c, new AudioRecord(c.id));
                }

                foreach (var key in plotBuffer.Keys)
                {
                    if (!snap.Contains(key)) plotBuffer.TryRemove(key, out _);
                }


                formsPlotRef.Invoke((MethodInvoker) delegate { formsPlotRef.Plot.Clear(); });

                yLimMin = double.MaxValue;
                yLimMax = 0;
                double serverNowMs = stopWatch.Elapsed.TotalMilliseconds;
                foreach (var it in plotBuffer)
                {
                    ClientChannel cc = it.Key; //ClientChannel
                    AudioRecord ar = it.Value; //AudioRecord
                    ar.appendData(cc, serverNowMs);
                    ar.prepareData(ref formsPlotRef);
                }
                //refresh plot
                formsPlotRef.Plot.Axes.SetLimitsX(yLimMin, yLimMax);
                formsPlotRef.Invoke((MethodInvoker)delegate { formsPlotRef.Refresh(); });
                await Task.Delay(10, clcTok); //refresh rate
            }
        }
    }
}