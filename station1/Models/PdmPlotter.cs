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
    /// <summary>
    /// Stany synchronizacji
    /// </summary>
    enum ProcessState
    {
        INIT_SYNCH,
        NORMAL
    }

    /// <summary>
    /// Klasa zajmuje sie rysowaniem wykresow i ganizacja danych do rysowania
    /// </summary>
    internal class PdmPlotter
    {
        private double lastPlotUpdateMs;


        private bool initSynchDone = false;


        public static double yLimMin;
        public static double yLimMax;

        public static double yLimMinCorr = 0;
        public static double yLimMaxCorr = 1;
        public bool exactSynch = false;
        private static string tag = "plotter";

        private static string exportCsvPaht = Globals.exportCsvPaht;
        private Stopwatch stopWatch = Stopwatch.StartNew();
        private int countClients = 0;
        public Locater doaLocater;
        public FormsPlot formsPlotRef;      //Gorne okno z danumi z mikrofonow
        public FormsPlot formsPlotCorrRef; // Dolne okno z opoznieniami
        public FormsPlot formsPlot_locate;  //Doa
        public FormsPlot formsPlot_TDoA;    //wykres ze strzalka
        FormsPlot formsPlot_doa;
        private List<AudioChunkChannel> clientsBuffer;  // bufor z aktualnymi klientami
        private ConcurrentDictionary<AudioChunkChannel, AudioRecord> plotBuffer = new();    // Dictionary z klientami i nagraniami (wektory X i Y)
        private bool doSynch = false;
        private ProcessState processState;

        /// <summary>
        /// Konstruktor klasy PdmPlotter
        /// </summary>
        /// <param name="formsPlotTimeShiftsRef">   wykres  </param>
        /// <param name="formsPlotRef"> wykres  </param>
        /// <param name="formsPlot_locate"> wykres  </param>
        /// <param name="formsPlot_doa"> wykres  </param>
        /// <param name="formsPlot_TDoA">wykres </param>
        /// <param name="clientsBuffer">wektor z klilentami</param>
        public PdmPlotter(FormsPlot formsPlotTimeShiftsRef, FormsPlot formsPlotRef, FormsPlot formsPlot_locate, FormsPlot formsPlot_doa, FormsPlot formsPlot_TDoA,
            List<AudioChunkChannel> clientsBuffer)
        {
            this.formsPlotCorrRef = formsPlotTimeShiftsRef;
            this.formsPlotRef = formsPlotRef;
            this.clientsBuffer = clientsBuffer;
            this.formsPlot_locate = formsPlot_locate;
            this.formsPlot_doa = formsPlot_doa;
            this.formsPlot_TDoA = formsPlot_TDoA;

            PdmPlotter.yLimMin = double.MaxValue;
            PdmPlotter.yLimMax = 0;
            processState = ProcessState.INIT_SYNCH;
            doaLocater = new Locater(formsPlot_locate, formsPlot_doa, formsPlot_TDoA);
        }



        /// <summary>
        /// Synchronizacja wstepna
        /// </summary>
        public void Synch()
        {
            Logger.I(tag, "Starting initial synchronisation of all clients");
            foreach (var it in plotBuffer)
            {
                it.Key.SynchRecord();
                it.Value.cutOffOffset();
                it.Value.clearCorrHist();
            }
            initSynchDone = true;
            exactSynch = false;
        }


        /// <summary>
        /// Eksportuje dane do pliku csv
        /// </summary>
        /// <remarks> Dane uzywane sa przez matlab do analizy pozniejszej </remarks>
        /// /// <remarks> Zapisuje raw data, opoznienia i kat doa </remarks>
        public void ExportData()
        {
            Logger.I(tag, "Exporting data");
            var inv = CultureInfo.InvariantCulture;

            var snap = plotBuffer.Values.ToList();
            if (snap.Count == 0)
            {
                Logger.W(tag, "No data to save to csv");
                return;
            }

            List<string> lines = new();

            StringBuilder sbHeader = new();
            foreach (var s in snap)
            {
                sbHeader.Append($"t{s.id},")
                        .Append($"y{s.id},")
                        .Append($"shifts{s.id},")
                        .Append($"shiftsAvg{s.id},")
                        .Append($"timeStamps{s.id},");
            }
            sbHeader.Append("angTime,angle");
            lines.Add(sbHeader.ToString());
            int maxLen = snap.Max(s => Math.Max(s.X.Length, s.Y.Length));
            Logger.I(tag, $"Exporting {maxLen} samples per channel");

            for (int i = 0; i < maxLen; i++)
            {
                StringBuilder sb = new();

                foreach (var s in snap)
                {
                    string tVal = (i < s.X.Length) ? s.X[i].ToString("R", inv) : "";
                    string yVal = (i < s.Y.Length) ? s.Y[i].ToString("R", inv) : "";
                    sb.Append(tVal).Append(",").Append(yVal).Append(",");

                    if (i < s.correlations.Length)
                    {
                        string shifts = s.correlations[i].ToString("R", inv);
                        string shiftsAvg = s.shiftsAvg[i].ToString("R", inv);
                        string timeStamps = s.timeStamps[i].ToString("R", inv);
                        sb.Append(shifts).Append(",").Append(shiftsAvg).Append(",").Append(timeStamps).Append(",");
                    }
                    else
                    {
                        sb.Append(",,,");
                    }
                }

                string angTim = (i < doaLocater.timestamps.Length)
                    ? doaLocater.timestamps[i].ToString("R", inv)
                    : "";
                string angle = (i < doaLocater.angles.Length)
                    ? doaLocater.angles[i].ToString("R", inv)
                    : "";
                sb.Append(angTim).Append(",").Append(angle);

                lines.Add(sb.ToString());
            }

            double currTime = stopWatch.Elapsed.TotalMilliseconds;
            string fileName = $"{currTime.ToString().Replace(',', '_')}.csv";
            string path = Path.Combine(exportCsvPaht, fileName);
            File.WriteAllLines(path, lines);

            Logger.I(tag, $"Exported dataset: {lines.Count - 1} rows to {fileName}");
        }





        /// <summary>
        /// Zlicza polaczonych klientow
        /// </summary>
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


        /// <summary>
        /// Glowna petla programu
        /// </summary>
        /// <param name="snap"></param>
        public void processAudio(ref List<KeyValuePair<AudioChunkChannel, AudioRecord>> snap)
        {

            switch (processState)
            {
                case ProcessState.INIT_SYNCH: // wstpena synchronizacja
                    if (initSynchDone && (plotBuffer.Count > 1))
                    {
                        Logger.S(tag, "Initial synchronisation done, changing state to NORMAL");
                        processState = ProcessState.NORMAL;

                    }

                    break;


                case ProcessState.NORMAL: // program dziala normalnie
                    {
                        bool logSynch = false;

                        if (plotBuffer.Count < 2)
                        {
                            if (logSynch) Logger.W(tag, $"Exact Synchronisation stopped, not enough chnnels, only{plotBuffer.Count} channels");
                            processState = ProcessState.INIT_SYNCH;
                            break;
                        }

                        int N = plotBuffer.Count;

                        // mikrofon referencyjny
                        var (refChan, refRec) = (snap[0].Key, snap[0].Value);
                        var T0 = refRec.X;
                        var Y0 = refRec.Y;

                        /* Sprawdz czy kazdy klient zostal zsynchronizowany */
                        for (int i = 1; i < N; i++) // dodaj aktualne opoznienei do offsetu. Dla kazdego klienta
                        {
                            var (chani, reci) = (snap[i].Key, snap[i].Value);   // wyciagnij kanal
                            var Ti = reci.X;
                            var Yi = reci.Y;


                            double maxLagMs = Globals.MaxLag; // uzywaj limitu opoznienia
                            if (exactSynch)
                            {
                                maxLagMs = double.MaxValue; // bez limitu, jak jest synchronizacja
                            }

                            //sprawdz dane
                            if (Y0 == null || Yi == null || Y0.Length < 2 || Yi.Length < 2 || Y0.All(v => v == 0) || Yi.All(v => v == 0) || Y0.Any(double.IsNaN) || Yi.Any(double.IsNaN))
                            {
                                if (logSynch) Logger.W(tag, $"invalid data for correlation, channel {chani.id}");
                                continue;
                            }



                            /* ================= */
                            /*GCC-PHAT */
                            double corr = AudioProcessing.findTimeShiftAsync(T0, Y0, Ti, Yi, maxLagMs); // Oblicz opoznienie 
                            /* ================= */


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


                                if (exactSynch)
                                {
                                    Logger.I(tag, $"Exact synch id: {chani.id}, old: {chani.offsetEndMs}, new corr: {corr} ");
                                    chani.accEndMs -= corr;
                                    chani.isExactSynch = true;
                                }
                            }

                        }// for loop

                        if (exactSynch) // Sprawdz czy wszyscy sa zsynchronizowani
                        {
                            snap[0].Key.isExactSynch = true; // sygnal referencyjny

                            bool allOk = true;
                            foreach (var kvp in snap)
                            {
                                {
                                    if (kvp.Key.id == refChan.id)
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
                                foreach (var kvp in snap)
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



        /// <summary>
        /// Przesyla dane do klientow
        /// </summary>
        /// <param name="snap"></param>
        private void sendData2Clients(ref List<KeyValuePair<AudioChunkChannel, AudioRecord>> snap)
        {

            for (int i = 0; i < snap.Count; i++)
            {
                var (chani, reci) = (snap[i].Key, snap[i].Value);   //the channel to update

                long timestampUs = stopWatch.ElapsedMilliseconds * 1000; // in microseconds

                chani.sendTimeStampTcp(timestampUs);

                Task.Delay(1000).Wait(); // small delay to avoid overwhelming the network

            }
        }

        /// <summary>
        /// Dokladna synchronizacja
        /// </summary>
        /// remarks> Polaczone z przyciskiem </remarks>
        public void ExactSynch()
        {
            exactSynch = true;
            Logger.I(tag, $"Starting exact synch");

            foreach (var it in plotBuffer)
            {
                it.Value.clearCorrHist();
                it.Key.resetCompPatter();
            }

            doaLocater.reset();

        }

        /// <summary>
        /// WYkresy i przetwarzanie audio
        /// </summary>
        /// <param name="refForm_MainDisplay"></param>
        /// <param name="clcTok"></param>
        /// <returns></returns>
        public async Task RunProgram(Form_mainDisplay refForm_MainDisplay, CancellationToken clcTok)
        {
            formsPlotCorrRef.Plot.Axes.SetLimitsY(-Globals.MaxLag, Globals.MaxLag);
            formsPlotRef.Plot.Axes.SetLimitsY(-1000, 1000);
            Logger.I(tag, "Plotter started");

            bool firstRun = true;
            while (!clcTok.IsCancellationRequested)
            {
                List<AudioChunkChannel> snap; // migawka klientow
                // Kazdy klient powinien miec pare z AudioRecord
                lock (clientsBuffer) snap = clientsBuffer.ToList();
                foreach (var c in snap)
                {
                    AudioRecord newRecord = new AudioRecord(c.id, firstRun);
                    plotBuffer.TryAdd(c, newRecord);

                    firstRun = false;
                }

                foreach (var key in plotBuffer.Keys)
                {
                    if (!snap.Contains(key)) plotBuffer.TryRemove(key, out _); // usuwa klientow ktorzy sie odlaczyli
                }


                var snap2 = plotBuffer
                    .OrderBy(kvp => kvp.Key.id)
                    .ToList();


                countActiveClients();


                /* ============================== */
                /* Process aduio */
                /* Przeprowadza lokalizacje DoA */
                processAudio(ref snap2);

                if (processState == ProcessState.NORMAL)
                {
                    doaLocater.localise(ref snap2);
                }
                /* ============================== */



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


        /// <summary>
        /// Odswieza wykresy
        /// </summary>
        /// <param name="refForm_MainDisplay"></param>
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

            var y0Main = formsPlotCorrRef.Plot.Add.HorizontalLine(0);
            y0Main.Color = new ScottPlot.Color(117, 117, 117);      // gray
            y0Main.LineWidth = 2.5f;

            formsPlotRef.Invoke((MethodInvoker)(() => formsPlotRef.Refresh()));
            formsPlotCorrRef.Invoke((MethodInvoker)(() => formsPlotCorrRef.Refresh()));
        }

    }
}