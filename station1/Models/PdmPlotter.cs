using Microsoft.VisualBasic.ApplicationServices;
using OpenTK.Graphics.OpenGL;
using ScottPlot;
using ScottPlot.Colormaps;
using ScottPlot.Statistics;
using ScottPlot.WinForms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        private int activeClients = 0;
        private bool initSynchDone = false;
        private bool[] isChanOk = new bool[3];
        private int tryCynchCount = 0;
        //public static int samplingRate;
        //public static int maxChunks; // 16 -> 1.04 s //16 old
        //public static int audioLen;
        //public static int SamplesPerChunk; // number of audio samples in a single chunk
        //public static int Capacity;
        public static double yLimMin;
        public static double yLimMax;

        public static double yLimMinShifts = 0;
        public static double yLimMaxShifts = 1;

        private static string tag = "plotter";

        private static string exportCsvPaht = @"C:\Users\wp1\Desktop\Studia\magisterka\Acustic_source_detection\matlab\Data\";
        private Stopwatch stopWatch = Stopwatch.StartNew();
        private int countClients = 0;
        public FormsPlot formsPlotRef; // reference to windows form plot
        public FormsPlot formsPlotTimeShiftsRef;
        private List<AudioChunkChannel> clientsBuffer;
        private ConcurrentDictionary<AudioChunkChannel, AudioRecord> plotBuffer = new();
        private bool doSynch = false;


        public PdmPlotter(FormsPlot formsPlotTimeShiftsRef, FormsPlot formsPlotRef, List<AudioChunkChannel> clientsBuffer/*, int audioLen, int maxChunks, int samplingRate*/)
        {
            this.formsPlotTimeShiftsRef = formsPlotTimeShiftsRef;
            this.formsPlotRef = formsPlotRef;
            this.clientsBuffer = clientsBuffer;

            //PdmPlotter.maxChunks = maxChunks;
            //PdmPlotter.audioLen = audioLen;
            //PdmPlotter.SamplesPerChunk = PdmPlotter.audioLen / 2; // number of audio samples in a single chunk
            //PdmPlotter.Capacity = PdmPlotter.SamplesPerChunk * maxChunks;
            //PdmPlotter.samplingRate = samplingRate;

            PdmPlotter.yLimMin = double.MaxValue;
            PdmPlotter.yLimMax = 0;
        }



        public void manuallyChangeTimeOffset(string input, string str2look)
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

            double timeOffsetStrValMs = (double)timeOffsetStrValUs / 1000.0;
            Logger.I(tag, $"Adding : {timeOffsetStrValMs} ms to channel {chanNum}");
            if (plotBuffer.Count <= chanNum)
            {
                Logger.W(tag, $"No channel with number {chanNum} found");
                return;
            }
            plotBuffer.ElementAt(chanNum).Key.offsetMs += timeOffsetStrValMs;
        }



        public void Synch()
        {
            initSynchDone = true;
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
                sbHeader.Append($"t{s.id},").Append($"y{s.id},").Append($"shifts{s.id},").Append($"shiftsAvg{s.id},").Append($"timeStamps{s.id},");
            }
            sbHeader.Length--;
            string header = sbHeader.ToString();
            lines.Add(header);


            //export shifts
            int N = Globals.MaxPlotHist;
            int j = 0;
            for (int i = 0; i < Globals.Capacity; i++)
            {
                StringBuilder sb = new();
                foreach (var s in snap)
                {
                    //audio data
                    string timeSampe = s.X[i].ToString("R", inv);
                    string audioSample = s.Y[i].ToString("R", inv);

                    sb.Append(timeSampe).Append(",").Append(audioSample).Append(",");
                    // shifts
                    var activeShifts = s.shifts;
                    //if () continue; // no shifts recorded
                    if ((j < N) && activeShifts.Length > 0)
                    {

                        string shifts = s.shifts[j].ToString("R", inv);
                        string shiftsAvg = s.shiftsAvg[j].ToString("R", inv);
                        string timeStamps = s.timeStamps[j].ToString("R", inv);

                        sb.Append(shifts).Append(",").Append(shiftsAvg).Append(",").Append(timeStamps).Append(",");
                        j++;
                    }

                }
                sb.Length--;
                //sb.RemoveAt(sb.Count - 1);
                lines.Add(sb.ToString());

            }
            double currTime = stopWatch.Elapsed.TotalMilliseconds;
            string path = exportCsvPaht + $"{currTime.ToString()}.csv";
            File.WriteAllLines(path, lines);

            Logger.I(tag, $"Exporting dataset: {header} to file {currTime.ToString()}.csv");
        }


        public void startExactSynch() => doSynch = true;


        private void countActiveClients()
        {
            int conCount = plotBuffer.Count;
            if (conCount != countClients)
            {
                Logger.I(tag, $"Number of active clients changed from {countClients} to {conCount}");
                countClients = conCount;
                initSynchDone = false;
            }
        }


        public void exactSynch(ref List<AudioChunkChannel> snap)
        {
            
            if (!initSynchDone) return; // initial synch not done yet
            bool logSynch = false;
            //if (!doSynch) return; //don't do anything

            double startSychMs = stopWatch.Elapsed.TotalMilliseconds;
            var snapPltBuff = plotBuffer                   // one ordered snapshot
                .OrderBy(kvp => kvp.Key.id)
                .ToList();

            if (plotBuffer.Count < 2)
            {
                if(logSynch) Logger.W(tag, $"Exact Synchronisation stopped, not enough chnnels, only{plotBuffer.Count} channels");
                doSynch = false;
                return;
            }

            //double serverNowMs = stopWatch.Elapsed.TotalMilliseconds;
            if (logSynch) Logger.I(tag, $"Exact synchronisation Synchronising {snap.Count} channels");

            int N = plotBuffer.Count;

            //AudioRecord[] snappedCls = new AudioRecord[N];
            //double[][] Tms = new double[N][];
            //double[][] Y = new double[N][];
            //for (int i = 0; i < N; i++)
            //{
            //    snappedCls[i] = pltBuffSnap[i];
            //    Tms[i] = snappedCls[i].X;
            //    Y[i] = snappedCls[i].Y;
            //}


            // Reference series
            var (refChan, refRec) = (snapPltBuff[0].Key, snapPltBuff[0].Value);
            var T0 = refRec.X;
            var Y0 = refRec.Y;


            //const double MaxtimeShift = 1;
            bool allChannelsSynch = true;
            bool CheckSynch = false;
            for (int i = 1; i < N; i++) // aclulate correlation and applay time shift to each channel
            {
                if (isChanOk[i])
                {
                    //Logger.I(tag,$"Channel {snapPltBuff[i].Key.id} already synchronised");
                    continue; // already ok  TODO: doesn't work
                }
                

                var (chani, reci) = (snapPltBuff[i].Key, snapPltBuff[i].Value);   //the channel to update
                var Ti = reci.X;
                var Yi = reci.Y;
                double maxLag = Globals.MinValidSchiftUs; //ms
                if (doSynch) maxLag = 100000.0; //us

                //Logger.I(tag, $"{isChanOk[0]}, {isChanOk[1]}, {isChanOk[2]}, {maxLag}");

                double timeShift = AudioProcessing.findTimeShiftAsync(T0, Y0, Ti, Yi, maxLag);
                if (double.IsNaN(timeShift))
                {
                    if (logSynch) Logger.W(tag, $"SYNCHRONISATION:     Could not calculate time shift for channel {chani.id} continuing to next channel");
                    allChannelsSynch = false;
                    continue;
                }
                else
                {
                    int timeStampSv = (int)(T0.Min()*1000.0); //ms to us
                    int timeShiftSv = (int)(timeShift * 1000.0); //ms to us
                    string toPlot = $"{chani.id}, {timeStampSv} us, {timeShiftSv} us";
                    reci.updateShift(timeStampSv, timeShift);
                    Logger.I(tag, toPlot);
                    CheckSynch = true;
                }

                //var cc2 = plotBuffer.First(kvp => kvp.Value.id == snappedCls[i].id).Key;
                


                //// SYNCHRONISATION /////
                if (!doSynch) continue; //don't do anything


                //if (Math.Abs((double)(chani.offsetMs - timeShift)) < 10000)
                //{
                //    Logger.I(tag, $"SYNCHRONISATION:     Channel {chani.id} already synchronised");
                //}

                chani.offsetMs -= timeShift;
                if (true) Logger.I(tag, $"SYNCHRONISATION:     Applied shift of {timeShift} ms to channel {chani.id}");
                isChanOk[i] = true;
                //if (Math.Abs(timeShift) <= 0.1) //ms
                //{
                //    //Applay     
                //    Task.Delay(2000); // delay 2 seconds
                //    isChanOk[i] = true;
                //}
                //else
                //{
                //    if (logSynch) Logger.W(tag, $"Calculated audio shift: {timeShift} ms is too high repeating synchronisation for client {chani.id}");
                //}

                allChannelsSynch = allChannelsSynch && isChanOk[i];

            }// for loop

            if (!doSynch) return; //don't do anything
            doSynch = !allChannelsSynch;


            if (allChannelsSynch)
            {
                double stopSychMs = stopWatch.Elapsed.TotalMilliseconds;
                double synchTimeMs = stopSychMs - startSychMs;
                Logger.S(tag, $"Synchronisation done took {synchTimeMs} ms");

                for (int i = 0; i < N; i++)
                {
                    isChanOk[i] = false;
                    var (chani, reci) = (snapPltBuff[i].Key, snapPltBuff[i].Value);   //the channel to update
                    reci.clearShiftHistory();
                }
            }
            //else
            //{
            //    if(CheckSynch) tryCynchCount++;
            //    if(tryCynchCount >= 50) // gieve up after too many attempts
            //    {
            //        Logger.W(tag, $"Synchronisation failed after too many attempts, {tryCynchCount} attempts");
            //        doSynch = false;
            //        tryCynchCount = 0;
            //    }
            //}
        }



        public async Task Plot(CancellationToken clcTok)
        {
            formsPlotTimeShiftsRef.Plot.Axes.SetLimitsY(-Globals.MinValidSchiftUs, Globals.MinValidSchiftUs);
            formsPlotRef.Plot.Axes.SetLimitsY(-1000, 1000);
            Logger.I(tag, "Plotter started");
            //int conCountPrev = 0;
            //int repetitions = 0;
            bool firstRun = true;
            while (!clcTok.IsCancellationRequested)
            {
                List<AudioChunkChannel> snap; // snapshot of current clients
                // Ensure every current client has an AudioRecord
                lock (clientsBuffer) snap = clientsBuffer.ToList();
                foreach (var c in snap)
                {
                    plotBuffer.TryAdd(c, new AudioRecord(c.id, /*PdmPlotter.Capacity,*/ firstRun));
                    firstRun = false;
                }

                foreach (var key in plotBuffer.Keys)
                {
                    if (!snap.Contains(key)) plotBuffer.TryRemove(key, out _);
                }

                countActiveClients();
                exactSynch(ref snap);
                


                formsPlotRef.Invoke((MethodInvoker)delegate { formsPlotRef.Plot.Clear(); });
                formsPlotTimeShiftsRef.Invoke((MethodInvoker)delegate { formsPlotTimeShiftsRef.Plot.Clear(); });

                yLimMin = double.MaxValue;
                yLimMax = 0;
                double serverNowMs = stopWatch.Elapsed.TotalMilliseconds;
                foreach (var it in plotBuffer)
                {
                    AudioChunkChannel cc = it.Key; //ClientChannel
                    AudioRecord ar = it.Value; //AudioRecord
                    ar.appendData(cc, serverNowMs);
                    ar.prepareData(ref formsPlotTimeShiftsRef, ref formsPlotRef);
                }

                if(plotBuffer.Count > 0)
                {
                    yLimMin = plotBuffer.Values.SelectMany(r => r.X).Min();
                    yLimMax = plotBuffer.Values.SelectMany(r => r.X).Max();
                    formsPlotRef.Plot.Axes.SetLimitsX(yLimMin, yLimMax);

                    //do the same for shifts plot
                    //double second = vals.OrderBy(v => v).Skip(1).First();
                    //var vals = plotBuffer.Values.SelectMany(r => r.timeStamps);
                    //if(vals.Count() > 2)
                    //{
                    //    yLimMinShifts = vals.Distinct().OrderBy(v => v).Skip(1).First();
                    //}

                    var positives = plotBuffer.Values
                        .SelectMany(r => r.timeStamps ?? Array.Empty<double>())
                        .Where(v => v > 0 && !double.IsNaN(v) && !double.IsInfinity(v))
                        .ToList();

                    if (positives.Count > 0)
                        yLimMinShifts = positives.Min();
                    else
                        yLimMinShifts = 0; 

                    yLimMaxShifts = plotBuffer.Values.SelectMany(r => r.timeStamps).Max();
                    formsPlotTimeShiftsRef.PerformAutoScale();
                }


                //refresh plot
                formsPlotRef.Plot.Axes.SetLimitsX(yLimMin, yLimMax);
                formsPlotRef.Invoke((MethodInvoker)delegate { formsPlotRef.Refresh(); });

                formsPlotTimeShiftsRef.Plot.Axes.SetLimitsX(yLimMinShifts, yLimMaxShifts);
                formsPlotRef.Invoke((MethodInvoker)delegate { formsPlotTimeShiftsRef.Refresh(); });
                //await Task.Delay(100, clcTok); //refresh rate
            }
        }
    }
}