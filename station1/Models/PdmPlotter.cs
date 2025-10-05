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
    enum ProcessState
    {
        INIT_SYNCH,
        EXACT_SYNCH,
        FREQUENCH_SYNCH,
        NORMAL
    }
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
        private ProcessState processState;

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
            ProcessState processState = ProcessState.INIT_SYNCH;
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
            Logger.I(tag, $"Adding : {timeOffsetStrValMs} ms to channel {plotBuffer.ElementAt(chanNum).Key.id}");
            if (plotBuffer.Count <= chanNum)
            {
                Logger.W(tag, $"No channel with number {chanNum} found");
                return;
            }
            plotBuffer.ElementAt(chanNum).Key.offsetMs += timeOffsetStrValMs;
        }
        


        public void manuallyChangeFreqOffset(string input, string str2look)
        {
            Logger.I(tag, $"Manually changing frequency offset, input: {input}");
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
            if(chanNum < 0 || chanNum > 2)
            {
                Logger.W(tag, $"Channel number {chanNum} is out of range, should be 0, 1 or 2");
                return;
            }

            int timeOffsetStrValUs = 0;
            string timeOffsetStr = input.Substring(numStartIdx + 1);
            try
            {
                timeOffsetStrValUs = int.Parse(timeOffsetStr);
            }
            catch (Exception)
            {
                Logger.W(tag, $"No valid time offset found after channel number {plotBuffer.ElementAt(chanNum).Key.id}");
                return;
            }

            double timeOffsetStrValMs = (double)timeOffsetStrValUs / 10000.0;
            Logger.I(tag, $"Adding : {timeOffsetStr} : {timeOffsetStrValMs} Hz to {plotBuffer.ElementAt(chanNum).Key.id}");
            if (plotBuffer.Count <= chanNum)
            {
                Logger.W(tag, $"No channel with number {chanNum} found");
                return;
            }
            plotBuffer.ElementAt(chanNum).Key.offsetFreq += timeOffsetStrValMs;
        }


        public void Synch()
        {
            //doSynch = true;
            foreach (var it in plotBuffer)
                it.Value.Synch();
            initSynchDone = true;
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


        public void startExactSynch()
        {
            doSynch = true;
            Logger.I(tag, $"Exact synch button pressed Starting exact synchronisation, doSynch: {doSynch}");
        }


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

        //public void updageProcessState()
        //{
        //    if(processState == ProcessState.INIT_SYNCH)
        //    {
        //        //Logger.E(tag, "Initial Synchronisation not doene yet");

        //    }
        //    //else if (processState == ProcessState.EXACT_SYNCH)
        //    //{
        //    //    Logger.S(tag, "Exact synchronisation done, changeing state to FREQUENCH_SYNCH");
        //    //    processState = ProcessState.FREQUENCH_SYNCH;
        //    //}


        //}
        public void processAudio(ref List<AudioChunkChannel> snap)
        {

            switch (processState)
            {
                case ProcessState.INIT_SYNCH:
                    if (initSynchDone && doSynch)
                    {
                        Logger.S(tag, "Initial synchronisation done, changing state to EXACT_SYNCH");
                        processState = ProcessState.EXACT_SYNCH;
                    }
                    break;


                case ProcessState.EXACT_SYNCH:
                    { 
                    //if (!initSynchDone) return; // initial synch not done yet
                    bool logSynch = false;
                    //if (!doSynch) return; //don't do anything

                    double startSychMs = stopWatch.Elapsed.TotalMilliseconds; // get curent time
                    var snapPltBuff = plotBuffer                   // snapshot of current clients
                        .OrderBy(kvp => kvp.Key.id)
                        .ToList();

                    if (plotBuffer.Count < 2) // check if you have enough channels to do synch
                    {
                        if (logSynch) Logger.W(tag, $"Exact Synchronisation stopped, not enough chnnels, only{plotBuffer.Count} channels");
                        processState = ProcessState.INIT_SYNCH;
                        break;
                    }

                    /* Convert buffer to series */
                    int N = plotBuffer.Count;
                    // Reference series
                    var (refChan, refRec) = (snapPltBuff[0].Key, snapPltBuff[0].Value);
                    var T0 = refRec.X;
                    var Y0 = refRec.Y;

                    /* Check if all channels are already synchronised */
                    bool allChannelsSynch = true;
                    for (int i = 1; i < N; i++) // aclulate correlation and applay time shift to each channel
                    {
                        if (isChanOk[i])
                            continue; // already ok  TODO: doesn't work


                        var (chani, reci) = (snapPltBuff[i].Key, snapPltBuff[i].Value);   //the channel to update
                        var Ti = reci.X;
                        var Yi = reci.Y;
                        //double maxLag = Globals.MinValidSchiftUs; //ms
                        //if (doSynch) maxLag = 100000.0; //us

                        double maxLag = 100000.0; //us
                        //if (doSynch) maxLag = 100000.0; //us

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
                            int timeStampSv = (int)(T0.Min() * 1000.0); //ms to us
                            int timeShiftSv = (int)(timeShift * 1000.0); //ms to us
                            string toPlot = $"{chani.id}, {timeStampSv} us, {timeShiftSv} us";
                            reci.updateShift(timeStampSv, timeShift);
                            Logger.I(tag, toPlot);
                        }

                        //// SYNCHRONISATION /////
                        chani.offsetMs -= timeShift;
                        Logger.I(tag, $"SYNCHRONISATION:     Applied shift of {timeShift} ms to channel {chani.id}");
                        isChanOk[i] = true;


                        allChannelsSynch = allChannelsSynch && isChanOk[i];

                    }// for loop

                    doSynch = !allChannelsSynch;


                    if (allChannelsSynch)
                    {
                        double stopSychMs = stopWatch.Elapsed.TotalMilliseconds;
                        double synchTimeMs = stopSychMs - startSychMs;

                        for (int i = 0; i < N; i++)
                        {
                            isChanOk[i] = false;
                            var (chani, reci) = (snapPltBuff[i].Key, snapPltBuff[i].Value);   //the channel to update
                            reci.clearShiftHistory();
                        }
                        Logger.S(tag, "Exact synchronisation done, changing state to FREQUENCH_SYNCH");
                        processState = ProcessState.FREQUENCH_SYNCH;
                    }
                    break;
                 }


                      
                case ProcessState.FREQUENCH_SYNCH:
                {
                    if (doSynch)
                    {
                        Logger.S(tag, "repeating exact synch");
                        processState = ProcessState.EXACT_SYNCH;
                        break;
                    }
                    double startSychMs = stopWatch.Elapsed.TotalMilliseconds; // get curent time
                    var snapPltBuff = plotBuffer                   // snapshot of current clients
                        .OrderBy(kvp => kvp.Key.id)
                        .ToList();

                    if (plotBuffer.Count < 2) // check if you have enough channels to do synch
                    {
                        Logger.E(tag, $"Exact Synchronisation stopped, not enough chnnels, only{plotBuffer.Count} channels");
                        processState = ProcessState.INIT_SYNCH;
                        break;
                    }

                    /* Convert buffer to series */
                    int N = plotBuffer.Count;
                    // Reference series
                    var (refChan, refRec) = (snapPltBuff[0].Key, snapPltBuff[0].Value);
                    var T0 = refRec.X;
                    var Y0 = refRec.Y;


                    for (int i = 1; i < N; i++) // aclulate correlation and applay time shift to each channel
                    {
                        var (chani, reci) = (snapPltBuff[i].Key, snapPltBuff[i].Value);   //the channel to update
                        var Ti = reci.X;
                        var Yi = reci.Y;
                        double maxLag = Globals.MinValidSchiftUs; //ms

                        double timeShift = AudioProcessing.findTimeShiftAsync(T0, Y0, Ti, Yi, maxLag);
                        if (double.IsNaN(timeShift))
                        {
                            continue;
                        }
                        else
                        {
                            int timeStampSv = (int)(T0.Min() * 1000.0); //ms to us
                            int timeShiftSv = (int)(timeShift * 1000.0); //ms to us
                            string toPlot = $"{chani.id}, {timeStampSv} us, {timeShiftSv} us";
                            reci.updateShift(timeStampSv, timeShift);
                            Logger.I(tag, toPlot);
                        }


                    }// for loop
                    break;
                }
            }
        

        }



        public async Task Plot(CancellationToken clcTok)
        {
            formsPlotTimeShiftsRef.Plot.Axes.SetLimitsY(-Globals.MinValidSchiftUs, Globals.MinValidSchiftUs);
            formsPlotRef.Plot.Axes.SetLimitsY(-1000, 1000);
            Logger.I(tag, "Plotter started");

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
                //updageProcessState();
                countActiveClients();
                processAudio(ref snap);
                


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