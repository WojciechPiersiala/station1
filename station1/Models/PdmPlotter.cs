using Microsoft.VisualBasic.ApplicationServices;
using OpenTK.Graphics.OpenGL;
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
        internal class AudioRecord
        {
            public int id = -1;
            private double offsetY = 0;
            private bool doSynch = true;
            public double[] X;  // accumulated time in ms
            public double[] Y;  // accumulated audio samples
            private int chunkIdx = 0;
            public AudioRecord(int id, int Capacity)
            {
                this.X = new double[Capacity];
                this.Y = new double[Capacity];

                this.id = id;
            }


            private void appendChunk(double[] xs, double[] ys)
            {
                int chunkSize = PdmPlotter.SamplesPerChunk;

                if (xs.Length != chunkSize || ys.Length != chunkSize)
                {
                    Logger.E(tag, $"Unexpected chunk size: got {xs.Length}, expected {chunkSize}");
                    throw new InvalidOperationException($"Unexpected chunk size: got {xs.Length}, expected {chunkSize}");
                }
                Array.Copy(ys, 0, Y, chunkIdx * chunkSize, chunkSize);
                Array.Copy(xs, 0, X, chunkIdx * chunkSize, chunkSize);

                chunkIdx = (chunkIdx + 1) % PdmPlotter.maxChunks;
            }



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
                        Logger.I(tag, $"Initial Synchronising client: {clientChannel.id}");
                        clientChannel.offsetMs = serverNowMs - packetStart_ms;
                        doSynch = false;
                    }
                    double start_ms = (double)(packetStart_ms + clientChannel.offsetMs);

                    double dt_ms = 1 / (double)samplingRate * 1000.0; // dt in ms
                    double stop_ms = start_ms + (dt_ms * samples.length);

                    double[] xs = Enumerable.Range(0, samples.length)
                        .Select(i => start_ms + i * dt_ms)
                        .ToArray();

                    double[] ys = samples.samples.Select(s => (double)s - offsetY).ToArray();
                    appendChunk(xs, ys);

                    if (synchRequired) // normalize the data during synchronisation
                    {
                        double avgY = Y.Average();
                        //if (Math.Abs(avgY) > 15)
                         offsetY += Y.Average();
                    }
                }
            }

            public void Synch()
            {
                doSynch = true;
            }

            public void prepareData(ref FormsPlot formsPlotRef)
            {
                //Array.Sort(X, Y);
                var scatter = formsPlotRef.Plot.Add.Scatter(this.X, this.Y);
                scatter.LegendText = $"Client {this.id}";

                switch (this.id)
                {
                    case 11:
                        scatter.Color = new ScottPlot.Color(255, 0, 0); // Red
                        break;
                    case 12:
                        scatter.Color = new ScottPlot.Color(0, 255, 0); // Green
                        break;
                    case 13:
                        scatter.Color = new ScottPlot.Color(0, 0, 255); // Blue
                        break;
                    default:
                        scatter.Color = new ScottPlot.Color(0, 0, 0); // Black
                        break;

                }
                

                // set the limits 
                if (this.X.Last() > yLimMax)
                    yLimMax = this.X.Max();

                if (this.X.First() < yLimMin)
                    yLimMin = this.X.Min();

            }


        }// AudioRecord class




        private static string tag = "plotter";
        public static double yLimMin = double.MaxValue;
        public static double yLimMax = 0;
        private static int samplingRate;
        private static int maxChunks; // 16 -> 1.04 s //16 old
        private static int audioLen;
        private static int SamplesPerChunk; // number of audio samples in a single chunk
        private static int Capacity;
        private static string exportCsvPaht = @"C:\Users\wp1\Desktop\Studia\magisterka\Acustic_source_detection\matlab\Data\";
        private Stopwatch stopWatch = Stopwatch.StartNew();
        private int countClients = 0;
        public FormsPlot formsPlotRef; // reference to windows form plot
        private List<ClientChannel> clientsBuffer;
        private ConcurrentDictionary<ClientChannel, AudioRecord> plotBuffer = new ();
        private bool doSynch = false;

        public void changeTimeOffset(string input, string str2look)
        {
            int idx = str2look.Length;
            int startIdx = input.IndexOf(str2look, StringComparison.OrdinalIgnoreCase);
            int numStartIdx = startIdx + str2look.Length;

            if (numStartIdx >= input.Length)
            {
                Logger.W(tag, "No channel number found after 'Channel'");
                return;
            }
            char channelChar = input[numStartIdx]; // convert char to int
            int chanNum = channelChar - '0';

            int timeOffsetStrValUs = 0;
            string timeOffsetStr = input.Substring(numStartIdx + 1);
            try
            {
                timeOffsetStrValUs = int.Parse(timeOffsetStr);
            }
            catch (Exception)
            {
                Logger.W(tag, $"No valid time offset found after channel number {chanNum}");
                return;
            }

            double timeOffsetStrValMs = (double) timeOffsetStrValUs / 1000.0;
            Logger.I(tag, $"Adding : {timeOffsetStrValMs} ms to channel {chanNum}");
            if(plotBuffer.Count <= chanNum)
            {
                Logger.W(tag, $"No channel with number {chanNum} found");
                return;
            }
            plotBuffer.ElementAt(chanNum).Key.offsetMs += timeOffsetStrValMs;
        }


        public PdmPlotter(FormsPlot formsPlotRef, List<ClientChannel> clientsBuffer, int audioLen, int maxChunks, int samplingRate)
        {
            this.formsPlotRef = formsPlotRef;
            this.clientsBuffer = clientsBuffer;

            PdmPlotter.maxChunks = maxChunks;
            PdmPlotter.audioLen = audioLen;
            PdmPlotter.SamplesPerChunk = PdmPlotter.audioLen / 2; // number of audio samples in a single chunk
            PdmPlotter.Capacity = PdmPlotter.SamplesPerChunk * maxChunks;
            PdmPlotter.samplingRate = samplingRate;
        }

        public void ExactSynch()
        {
            doSynch = true;
        }

        public void Synch()
        {
            //doSynch = true;
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
            sbHeader.Length--;
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
                sb.Length--;
                //sb.RemoveAt(sb.Count - 1);
                lines.Add(sb.ToString());

            }
            double  currTime = stopWatch.Elapsed.TotalMilliseconds;
            string path = exportCsvPaht + $"{currTime.ToString()}.csv";
            File.WriteAllLines(path, lines);

            Logger.I(tag, $"Exporting dataset: {header} to file {currTime.ToString()}.csv");
        }

        public async Task Plot(CancellationToken clcTok)
        {
            formsPlotRef.Plot.Axes.SetLimitsY(-1000, 1000);
            Logger.I(tag, "Plotter started");
            int conCountPrev= 0;
            int repetitions = 0;
            bool[] isChanOk = new bool[3];
            while (!clcTok.IsCancellationRequested)
            {
                List<ClientChannel> snap; // snapshot of current clients
                // Ensure every current client has an AudioRecord
                lock (clientsBuffer) snap = clientsBuffer.ToList();
                foreach (var c in snap)
                {
                    plotBuffer.TryAdd(c, new AudioRecord(c.id, PdmPlotter.Capacity));
                }

                foreach (var key in plotBuffer.Keys)
                {
                    if (!snap.Contains(key)) plotBuffer.TryRemove(key, out _);
                }


                // calculate correlation 
                if (doSynch)
                {
                    double startSychMs = stopWatch.Elapsed.TotalMilliseconds;
                    var pltBuffSnap = plotBuffer
                        .OrderBy(kvp => kvp.Key.id)   // stable order (dict is unordered)
                        .Select(kvp => kvp.Value)
                        .ToList();

                    if (plotBuffer.Count < 2)
                    {
                        Logger.W(tag, $"Exact Synchronisation stopped, not enough chnnels, only{plotBuffer.Count} channels");
                    }
                    else
                    {
                        //double serverNowMs = stopWatch.Elapsed.TotalMilliseconds;
                        Logger.I(tag, $"Exact synchronisation Synchronising {snap.Count} channels");
                        
                        int N = plotBuffer.Count;
                        AudioRecord[] snappedCls = new AudioRecord[N];
                        double[][] Tms = new double[N][];
                        double[][] Y = new double[N][];
                        for (int i = 0; i < N; i++)
                        {
                            snappedCls[i] = pltBuffSnap[i];
                            Tms[i] = (double[])snappedCls[i].X.Clone();
                            Y[i] = (double[])snappedCls[i].Y.Clone();
                        }
                        //var r1 = pltBuffSnap[0];
                        //var r2 = pltBuffSnap[1];
                        //double[] T1ms = (double[])r1.X.Clone();
                        //double[] Y1 = (double[])r1.Y.Clone();
                        //double[] T2ms = (double[])r2.X.Clone();
                        //double[] Y2 = (double[])r2.Y.Clone();
                        const double MaxtimeShift = 30.0;
                        bool allChannelsSynch = true;
                        for (int i = 1; i < N; i++)
                        {

                            if(!isChanOk[i])
                            {

                                double timeShift = await AudioProcessing.findTimeShiftAsync(Tms[0], Y[0], Tms[i], Y[i]);

                                if (double.IsNaN(timeShift))
                                {
                                    Logger.W(tag, $"SYNCHRONISATION:     Could not calculate time shift for channel {snappedCls[i].id} continuing to next channel");
                                    allChannelsSynch = false;
                                    break;
                                }

                                var cc2 = plotBuffer.First(kvp => kvp.Value.id == snappedCls[i].id).Key;
                                if (Math.Abs(timeShift) <= MaxtimeShift)
                                {
                                    //Applay shift
                                    cc2.offsetMs -= timeShift;
                                    Logger.I(tag, $"SYNCHRONISATION:     Applied shift of {timeShift} ms to channel {cc2.id}");
                                    isChanOk[i] = true;
                                }
                                else
                                {
                                    Logger.W(tag, $"Calculated audio shift: {timeShift} ms is too high repeating synchronisation for client {cc2.id}");
                                }

                                allChannelsSynch = allChannelsSynch && isChanOk[i];
                            }

                        }// for loop

                        //if (MaxtimeShift > 1)
                        //{
                        //    doSynch = false;
                        //    repetitions++;
                        //    Logger.W(tag, $"Time shift {MaxtimeShift} too hight repeating synchronisation");
                        //}
                        //else if (repetitions < 5)
                        //{
                        //    {
                        //        doSynch = false;
                        //        repetitions = 0;
                        //        Logger.E(tag, $"Too many repetitions");
                        //    }

                        //}
                        doSynch = !allChannelsSynch;
                        //doSynch = false; //tmp

                        if (allChannelsSynch)
                        {
                            double stopSychMs = stopWatch.Elapsed.TotalMilliseconds;
                            double synchTimeMs = stopSychMs - startSychMs;
                            Logger.I(tag, $"Synchronisation done took {synchTimeMs} ms");

                            for (int i=0; i<3; i++)
                                isChanOk[i] = false;
                        }
                    }

                }

                formsPlotRef.Invoke((MethodInvoker)delegate { formsPlotRef.Plot.Clear(); });

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