using Microsoft.VisualBasic.ApplicationServices;
using OpenTK.Graphics.OpenGL;
using ScottPlot;
using ScottPlot.Colormaps;
using ScottPlot.Statistics;
using ScottPlot.WinForms;
using station1.Forms;
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
        NORMAL
    }
    internal class PdmPlotter
    {
        //private int activeClients = 0;
        private bool initSynchDone = false;
        //private bool[] isChanOk = new bool[3];
        //private int tryCynchCount = 0;
        //public static int samplingRate;
        //public static int maxChunks; // 16 -> 1.04 s //16 old
        //public static int audioLen;
        //public static int SamplesPerChunk; // number of audio samples in a single chunk
        //public static int Capacity;


        public static double yLimMin;
        public static double yLimMax;

        public static double yLimMinCorr= 0;
        public static double yLimMaxCorr = 1;
        public bool exactSynch = false;
        private static string tag = "plotter";

        private static string exportCsvPaht = @"C:\Users\wp1\Desktop\Studia\magisterka\Acustic_source_detection\matlab\Data\";
        private Stopwatch stopWatch = Stopwatch.StartNew();
        private int countClients = 0;
        public FormsPlot formsPlotRef; // reference to windows form plot
        public FormsPlot formsPlotCorrRef;
        private List<AudioChunkChannel> clientsBuffer;
        private ConcurrentDictionary<AudioChunkChannel, AudioRecord> plotBuffer = new();
        private bool doSynch = false;
        private ProcessState processState;

        public PdmPlotter(FormsPlot formsPlotTimeShiftsRef, FormsPlot formsPlotRef, List<AudioChunkChannel> clientsBuffer/*, int audioLen, int maxChunks, int samplingRate*/)
        {
            this.formsPlotCorrRef = formsPlotTimeShiftsRef;
            this.formsPlotRef = formsPlotRef;
            this.clientsBuffer = clientsBuffer;


            PdmPlotter.yLimMin = double.MaxValue;
            PdmPlotter.yLimMax = 0;
            processState = ProcessState.INIT_SYNCH;
        }



        //public void manuallyChangeTimeOffset(string input, string str2look)
        //{
        //    int idx = str2look.Length;
        //    int startIdx = input.IndexOf(str2look, StringComparison.OrdinalIgnoreCase);
        //    int numStartIdx = startIdx + str2look.Length;

        //    if (numStartIdx >= input.Length)
        //    {
        //        Logger.W(tag, "No channel number found after 'Channel'");
        //        return;
        //    }
        //    char channelChar = input[numStartIdx]; // convert char to int
        //    int chanNum = channelChar - '0';

        //    int timeOffsetStrValUs = 0;
        //    string timeOffsetStr = input.Substring(numStartIdx + 1);
        //    try
        //    {
        //        timeOffsetStrValUs = int.Parse(timeOffsetStr);
        //    }
        //    catch (Exception)
        //    {
        //        Logger.W(tag, $"No valid time offset found after channel number {chanNum}");
        //        return;
        //    }

        //    double timeOffsetStrValMs = (double)timeOffsetStrValUs / 1000.0;
        //    Logger.I(tag, $"Adding : {timeOffsetStrValMs} ms to channel {plotBuffer.ElementAt(chanNum).Key.id}");
        //    if (plotBuffer.Count <= chanNum)
        //    {
        //        Logger.W(tag, $"No channel with number {chanNum} found");
        //        return;
        //    }
        //    plotBuffer.ElementAt(chanNum).Key.offsetMs += timeOffsetStrValMs;
        //}



        //public void manuallyChangeFreqOffset(string input, string str2look)
        //{
        //    Logger.I(tag, $"Manually changing frequency offset, input: {input}");
        //    int idx = str2look.Length;
        //    int startIdx = input.IndexOf(str2look, StringComparison.OrdinalIgnoreCase);
        //    int numStartIdx = startIdx + str2look.Length;

        //    if (numStartIdx >= input.Length)
        //    {
        //        Logger.W(tag, "No channel number found after 'Channel'");
        //        return;
        //    }
        //    char channelChar = input[numStartIdx]; // convert char to int
        //    int chanNum = channelChar - '0';
        //    if(chanNum < 0 || chanNum > 2)
        //    {
        //        Logger.W(tag, $"Channel number {chanNum} is out of range, should be 0, 1 or 2");
        //        return;
        //    }

        //    int timeOffsetStrValUs = 0;
        //    string timeOffsetStr = input.Substring(numStartIdx + 1);
        //    try
        //    {
        //        timeOffsetStrValUs = int.Parse(timeOffsetStr);
        //    }
        //    catch (Exception)
        //    {
        //        Logger.W(tag, $"No valid time offset found after channel number {plotBuffer.ElementAt(chanNum).Key.id}");
        //        return;
        //    }

        //    double timeOffsetStrValMs = (double)timeOffsetStrValUs / 10000.0;
        //    Logger.I(tag, $"Adding : {timeOffsetStr} : {timeOffsetStrValMs} Hz to {plotBuffer.ElementAt(chanNum).Key.id}");
        //    if (plotBuffer.Count <= chanNum)
        //    {
        //        Logger.W(tag, $"No channel with number {chanNum} found");
        //        return;
        //    }
        //    plotBuffer.ElementAt(chanNum).Key.offsetFreq += timeOffsetStrValMs;
        //}


        public void Synch()
        {
            Logger.I(tag, "Starting initial synchronisation of all clients");
            //doSynch = true;
            foreach (var it in plotBuffer)
            {
                it.Key.SynchRecord();
                it.Value.cutOffOffset();
                it.Value.clearCorrHist();
            }
            initSynchDone = true;
            exactSynch = false;
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
                    var activeShifts = s.correlations;
                    //if () continue; // no shifts recorded
                    if ((j < N) && activeShifts.Length > 0)
                    {

                        string shifts = s.correlations[j].ToString("R", inv);
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


        //public void startExactSynch()
        //{
        //    doSynch = true;
        //    Logger.I(tag, $"Exact synch button pressed Starting exact synchronisation, doSynch: {doSynch}");
        //}


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
        public void processAudio(ref List<KeyValuePair<AudioChunkChannel, AudioRecord>> snap)
        {

            switch (processState)
            {
                case ProcessState.INIT_SYNCH:
                    //foreach (var it in plotBuffer)
                    //{
                    //    it.Value.cutOffOffset();
                    //}
                    if (initSynchDone && (plotBuffer.Count > 1))
                    {
                        Logger.S(tag, "Initial synchronisation done, changing state to NORMAL");
                        processState = ProcessState.NORMAL;

                    }
                    //Logger.S(tag, "Initial synchronisation done, changing state to NORMAL");
                    //processState = ProcessState.NORMAL;
                    break;


                case ProcessState.NORMAL:
                    {
                        bool logSynch = false;
                        var snapPltBuff = snap;

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
                        for (int i = 1; i < N; i++) // aclulate correlation and applay time shift to each channel
                        {
                            var (chani, reci) = (snapPltBuff[i].Key, snapPltBuff[i].Value);   //the channel to update
                            var Ti = reci.X;
                            var Yi = reci.Y;

                            //double maxLagMs = double.MaxValue; //ms

                            double maxLagMs = Globals.MaxLag;
                            if(exactSynch)
                            {
                                maxLagMs = double.MaxValue; // for exact synch use full range
                            }

                            //data validation
                            if (Y0 == null || Yi == null || Y0.Length < 2 || Yi.Length < 2 || Y0.All(v => v == 0) || Yi.All(v => v == 0) || Y0.Any(double.IsNaN) || Yi.Any(double.IsNaN))
                            {
                                if (logSynch) Logger.W(tag, $"invalid data for correlation, channel {chani.id}");
                                continue;
                            }



                            double corr = AudioProcessing.findTimeShiftAsync(T0, Y0, Ti, Yi, maxLagMs);




                            if (double.IsNaN(corr))
                            {
                                if (logSynch) Logger.W(tag, $"invalid correlation, channel {chani.id} continuing to next channel");
                                continue;
                            }
                            else
                            {
                                int timeStampUs = (int)(T0.Min() * 1000.0); //ms to us
                                int corrUs = (int)(corr * 1000.0); //ms to us
                                string toPlot = $"id: {chani.id}, time: {timeStampUs} us, correlation: {corrUs} us";
                                reci.rememberCorrelation(timeStampUs, corr);
                                Logger.I(tag, toPlot);

                                if (exactSynch)
                                {
                                    Logger.I(tag, $"Exact synch id: {chani.id}, old: {chani.offsetEndMs}, new corr: {corr} ");
                                    chani.accEndMs -= corr;
                                    chani.isExactSynch = true;
                                }
                            }

                        }// for loop
                        
                        if (exactSynch)
                        {
                            // Mark the reference channel as logically done
                            snapPltBuff[0].Key.isExactSynch = true; // reference channel

                            bool allOk = true;
                            foreach (var kvp in snapPltBuff) { // use the same stable snapshot!
                                {
                                    if(kvp.Key.id == refChan.id)
                                    {
                                        kvp.Key.offsetEndMs = 0.0;
                                        kvp.Key.isExactSynchDone = true; ;
                                    }
                                    if (!kvp.Key.isExactSynch) { allOk = false; break; }
                                }
                            }

                            if (allOk)
                            {
                                Logger.I(tag, "Exact synch done");
                                foreach (var kvp in snapPltBuff)
                                {
                                    kvp.Key.isExactSynch = false;
                                    kvp.Key.isExactSynchDone = true;
                                }
                                exactSynch = false;
                            }
                        }


                        break;
                 }
            }

        }


        //int N = plotBuffer.Count;
        //// Reference series
        //var(refChan, refRec) = (snapPltBuff[0].Key, snapPltBuff[0].Value);
        //            var T0 = refRec.X;
        //var Y0 = refRec.Y;

        ///* Check if all channels are already synchronised */
        //bool allChannelsSynch = true;
        //            for (int i = 1; i<N; i++) // aclulate correlation and applay time shift to each channel
        //            {
        //                if (isChanOk[i])
        //                    continue; // already ok  TODO: doesn't work


        //                var(chani, reci) = (snapPltBuff[i].Key, snapPltBuff[i].Value);   //the channel to update



        private void sendData2Clients(ref List<KeyValuePair<AudioChunkChannel, AudioRecord>> snap)
        {

            for (int i =0; i < snap.Count; i++)
            {
                var (chani, reci) = (snap[i].Key, snap[i].Value);   //the channel to update

                long timestampUs = stopWatch.ElapsedMilliseconds * 1000; // in microseconds

                chani.sendTimeStampTcp(timestampUs);

                Task.Delay(1000).Wait(); // small delay to avoid overwhelming the network
                
            }
        }

        public void ExactSynch()
        {
            exactSynch = true;
            Logger.I(tag, $"Starting exact synch");
        }
        public async Task RunProgram(Form_mainDisplay refForm_MainDisplay, CancellationToken clcTok)
        {
            formsPlotCorrRef.Plot.Axes.SetLimitsY(-Globals.MaxLag, Globals.MaxLag);
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
                    AudioRecord newRecord = new AudioRecord(c.id, firstRun);
                    plotBuffer.TryAdd(c, newRecord);


                    firstRun = false;
                }

                foreach (var key in plotBuffer.Keys)
                {
                    if (!snap.Contains(key)) plotBuffer.TryRemove(key, out _);
                }


                var snap2 = plotBuffer                   // snapshot of current clients
                    .OrderBy(kvp => kvp.Key.id)
                    .ToList();

                //updageProcessState();
                countActiveClients();

                //sendData2Clients(ref snap2);

                //////////////////// PROECESS AUDIO ////////////////////
                processAudio(ref snap2);
                ////////////////////////////////////////////////////////






                formsPlotRef.Invoke((MethodInvoker)delegate { formsPlotRef.Plot.Clear(); });
                formsPlotCorrRef.Invoke((MethodInvoker)delegate { formsPlotCorrRef.Plot.Clear(); });

                yLimMin = double.MaxValue;
                yLimMax = 0;
                double serverNowMs = stopWatch.Elapsed.TotalMilliseconds;
                foreach (var it in plotBuffer)
                {
                    AudioChunkChannel cc = it.Key; //ClientChannel
                    AudioRecord ar = it.Value; //AudioRecord
                    ar.appendData(cc, serverNowMs);
                    ar.prepareData(ref formsPlotCorrRef, ref formsPlotRef);
                }

                plot(refForm_MainDisplay);

            }
        }


        private void plot(Form_mainDisplay refForm_MainDisplay)
        {
            refForm_MainDisplay.Invoke((MethodInvoker)delegate
            {
                double currTime = stopWatch.Elapsed.TotalMilliseconds;
                refForm_MainDisplay.label_serverTime.Text =
                    "Server Time: " + ((int)(currTime / 1000)) + " s";
            });

            if (plotBuffer.Count > 0)
            {
                var xVals = plotBuffer.Values.SelectMany(r => r.X)
                    .Where(v => !double.IsNaN(v) && !double.IsInfinity(v))
                    .ToList();

                if (xVals.Count > 1)
                {
                    yLimMin = xVals.Min();
                    yLimMax = xVals.Max();
                    if (yLimMax > yLimMin)
                        formsPlotRef.Plot.Axes.SetLimitsX(yLimMin, yLimMax);
                }

                var tsVals = plotBuffer.Values
                    .SelectMany(r => r.timeStamps ?? Array.Empty<double>())
                    .Where(v => v > 0 && !double.IsNaN(v) && !double.IsInfinity(v))
                    .ToList();

                if (tsVals.Count > 1)
                {
                    yLimMinCorr = tsVals.Min();
                    yLimMaxCorr = tsVals.Max();
                    if (yLimMaxCorr > yLimMinCorr)
                        formsPlotCorrRef.Plot.Axes.SetLimitsX(yLimMinCorr, yLimMaxCorr);
                }
            }

            formsPlotRef.Invoke((MethodInvoker)(() => formsPlotRef.Refresh()));
            formsPlotCorrRef.Invoke((MethodInvoker)(() => formsPlotCorrRef.Refresh()));
        }

    }
}