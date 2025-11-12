using ScottPlot;
using ScottPlot.WinForms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace station1.Models
{


    internal class AudioRecord
    {
        //private Stopwatch stopWatch = Stopwatch.StartNew();
        public double lastCorr = 0.0;
        //public double lastTimeStamp = 0.0;
        public long seq = -1;
        private double lastSmoothedShift = 0;
        private bool shiftsFull = false;
        public bool isFirstChannel = false;
        public double[] correlations;
        public double[] timeStamps;

        public double[] shiftsAvg;

        private int corrIdx = 0;


        private string tag;

        public int id = -1;
        private double offsetY = 0.0;
        //private bool doSynch = true;
        public double[] X;  // accumulated time in ms
        public double[] Y;  // accumulated audio samples
        private int chunkIdx = 0;
        public AudioRecord(int id, bool isFirstChannel)
        {
            this.isFirstChannel = isFirstChannel;
            tag = $"AudioRecord {id}";


            this.X = new double[Globals.Capacity];
            this.Y = new double[Globals.Capacity];


            this.correlations = new double[Globals.MaxPlotHist];
            this.timeStamps = new double[Globals.MaxPlotHist];
            this.shiftsAvg = new double[Globals.MaxPlotHist];

            this.id = id;
            this.isFirstChannel = isFirstChannel;

        }

        public void getCorrForDoa(double lastCorr)
        {
            if(!double.IsNaN(lastCorr))
            {
                this.lastCorr = lastCorr;
                //this.lastTimeStamp = lastTimeStamp;
                //Logger.I(tag, $"Got lastCorr for DoA: {lastCorr}");
            }

        }
        private void appendChunk(double[] xs, double[] ys)
        {
            int chunkSize = Globals.SamplesPerChunk;

            if (xs.Length != chunkSize || ys.Length != chunkSize)
            {
                Logger.E(tag, $"Unexpected chunk size: got {xs.Length}, expected {chunkSize}");
                throw new InvalidOperationException($"Unexpected chunk size: got {xs.Length}, expected {chunkSize}");
            }
            Array.Copy(ys, 0, Y, chunkIdx * chunkSize, chunkSize);
            Array.Copy(xs, 0, X, chunkIdx * chunkSize, chunkSize);

            chunkIdx = (chunkIdx + 1) % Globals.MaxChunks;

        }


        public void cutOffOffset()
        {
            double avgY = Y.Average();
            //if (Math.Abs(avgY) > 15)
            offsetY += Y.Average();    
        }

        public void appendData(AudioChunkChannel clientChannel, double serverNowMs)
        {
            while (clientChannel.sampleQueue.TryDequeue(out AudioChunk samples)) // Wyciagnij następny chunk próbek
            {
                if (samples.length != Globals.SamplesPerChunk)
                {
                    Logger.E(tag, $"Unexpected samples length: got {samples.length}, expected {Globals.SamplesPerChunk}");
                    continue;
                }

                double packetStart_ms = ((double)samples.timestamp / 1000.0); // Odczytaj znak czassu

                double start_ms = packetStart_ms + clientChannel.offsetEndMs; // Zastosuj offset endMs do znaku czasu pakietu
                // Jezeli przeprowadzono dokładną synchronizację zrezygnuj z offsetu endMs i znaku czasu pakietu
                // zamiast tego użyj skumulowanego czasu końcowego
                if (clientChannel.isExactSynchDone) 
                    start_ms = (double)clientChannel.accEndMs; //nowy poczatek pakietu

                // wygeneruj punkty czasu dla próbek w pakiecie na podstawie poczatku pakietu i czestotliwosci probkowania
                double dt_ms = 1 / (double)Globals.SamplingRate * 1000.0;
                double stop_ms = start_ms + (dt_ms * samples.length);

                double[] xs = Enumerable.Range(0, samples.length)
                    .Select(i => start_ms + i * (dt_ms + clientChannel.offsetFreq))
                    .ToArray();

                double[] ys = samples.samples.Select(s => (double)s - offsetY).ToArray();
                appendChunk(xs, ys);
                double chunkDurationMs = (dt_ms * samples.length);


                // Obsługa numerów sekwencyjnych pakietów
                if (seq < 0)
                {
                    Logger.I(tag, $"First chunk received with seq {samples.seq}");
                    seq = samples.seq;
                }
                else
                {
                    long expectedSeq = seq + 1;
                    long seqDiff = samples.seq - expectedSeq;

                    if (seqDiff != 0)
                    {
                        if (seqDiff > 0)
                        {
                            // Pakiet zgubiony
                            Logger.W(tag, $"Missing {seqDiff} packet(s). Expected seq {expectedSeq}, got {samples.seq}");
                            chunkDurationMs *= (seqDiff + 1); // Powiększ czas trwania o zgubione pakiety
                        }
                        else
                        {
                            // Pakiet przyszedł z opóźnieniem lub jest duplikatem
                            Logger.W(tag, $"Out-of-order or duplicate packet. Expected {expectedSeq}, got {samples.seq}");
                            seqDiff = 0;
                        }
                    }

                    seq = samples.seq; // Zaktualizuj oczekiwany numer sekwencyjny
                }
                // Kompensacja dryfu
                double compensationOffset = clientChannel.compensateDrift(serverNowMs);
                compensationOffset = 0.0; // tymczasowo wyłączone
                // Aktualizacja skumulowanego czasu końcowego
                clientChannel.accEndMs = start_ms + chunkDurationMs - compensationOffset;
                //if(tag == "AudioRecord 13")
                //{
                //    double diff = (double)((double)packetStart_ms - clientChannel.accEndMs);
                //    Logger.I(tag, $"serverTime: {serverNowMs}, tiemstamp: {packetStart_ms}, accEndMs: {clientChannel.accEndMs}, diff = {diff}");
                //}
            }
        }




         
        public void rememberCorrelation(double timeStamp, double corr)
        {
            if (timeStamp < 100.0) return; // optional, see note (3)

            // write raw sample
            correlations[corrIdx] = corr;
            timeStamps[corrIdx] = timeStamp;

            // filter on the *current* slot
            double filtered;
            if (corrIdx >= Globals.Navg)
                filtered = medianShifts(correlations, corrIdx - Globals.Navg, Globals.Navg);
            else
                filtered = corr;

            // smooth and store at the SAME index
            lastSmoothedShift = smoothShift(lastSmoothedShift, filtered);
            shiftsAvg[corrIdx] = lastSmoothedShift;

            // advance ring index
            corrIdx++;
            if (corrIdx >= Globals.MaxPlotHist) corrIdx = 0;


            // use last correlation for DoA
            getCorrForDoa(lastSmoothedShift);
        }


        private double medianShifts(double[] shifts, int startIdx, int dataPoints)
        {
            if (shifts.Length < dataPoints || startIdx + dataPoints > shifts.Length) 
                return double.NaN; //niepoprawne dane
            var window = shifts.Skip(startIdx).Take(dataPoints).OrderBy(v => v).ToArray(); //okno obliczeniowe
            int mid = dataPoints / 2;
            if (dataPoints % 2 == 0)
                return (window[mid - 1] + window[mid])/2.0;
            else return window[mid];
        }

        private double smoothShift(double prev, double current, double alpha = 0.1)
        {
            return alpha * current + (1 - alpha) * prev;
        }



        public void clearCorrHist()
        {
            Array.Clear(correlations, 0, correlations.Length);
            Array.Clear(timeStamps, 0, timeStamps.Length);

            Array.Clear(shiftsAvg, 0, shiftsAvg.Length);

            corrIdx = 0;
        }


        public void prepareData(ref FormsPlot formsPlotTimeShiftsRef, ref FormsPlot formsPlotRef)
        {
            // Rebuild ordered arrays (so time always increases)
            double[] orderedTimestamps = new double[Globals.MaxPlotHist];
            double[] orderedShiftsAvg = new double[Globals.MaxPlotHist];

            int n1 = Globals.MaxPlotHist - corrIdx;
            Array.Copy(this.timeStamps, corrIdx, orderedTimestamps, 0, n1);
            Array.Copy(this.shiftsAvg, corrIdx, orderedShiftsAvg, 0, n1);

            if (corrIdx > 0)
            {
                Array.Copy(this.timeStamps, 0, orderedTimestamps, n1, corrIdx);
                Array.Copy(this.shiftsAvg, 0, orderedShiftsAvg, n1, corrIdx);
            }

            // regular audio waveform
            var scatter = formsPlotRef.Plot.Add.Scatter(this.X, this.Y);
            scatter.LegendText = $"Client {this.id}";

            // raw correlations (unchanged)
            var scatter2 = formsPlotTimeShiftsRef.Plot.Add.Scatter(this.timeStamps, this.correlations);
            scatter2.LineWidth = 0;
            scatter2.LegendText = $"dT {this.id}";

            // ✅ FIXED: use ordered arrays for smoothed shifts
            var scatter3 = formsPlotTimeShiftsRef.Plot.Add.Scatter(orderedTimestamps, orderedShiftsAvg);
            scatter3.MarkerSize = 0;
            scatter3.LineWidth = 2.5f;

            // colors (unchanged)
            ScottPlot.Color color;
            switch (this.id)
            {
                case 11: color = new ScottPlot.Color(0, 125, 0); break;
                case 12: color = new ScottPlot.Color(255, 0, 0); break;
                case 13: color = new ScottPlot.Color(0, 0, 255); break;
                default: color = new ScottPlot.Color(0, 0, 0); break;
            }

            scatter.Color = color;
            scatter2.Color = color;
            scatter3.Color = color;
        }



        public void doa()
        {

        }
    }
}
