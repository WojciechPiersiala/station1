using ScottPlot;
using ScottPlot.WinForms;
using System;
using System.Collections.Generic;
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
        public AudioRecord(int id, /*int Capacity,*/ bool isFirstChannel)
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
            while (clientChannel.sampleQueue.TryDequeue(out AudioChunk samples))
            {
                if (samples.length != Globals.SamplesPerChunk)
                {
                    Logger.E(tag, $"Unexpected samples length: got {samples.length}, expected {Globals.SamplesPerChunk}");
                    continue;
                }

                double packetStart_ms = ((double)samples.timestamp / 1000.0); //ms

                //double start_ms = clientChannel.accEndMs ?? (packetStart_ms);

                double start_ms = packetStart_ms + clientChannel.offsetEndMs;

                if(clientChannel.isExactSynchDone)
                {
                    //Logger.I(tag, "using fake time");
                    start_ms = (double)clientChannel.accEndMs;
                }


                    //recntTime = start_ms;
                double dt_ms = 1 / (double)Globals.SamplingRate * 1000.0; // dt in ms
                double stop_ms = start_ms + (dt_ms * samples.length);

                double[] xs = Enumerable.Range(0, samples.length)
                    .Select(i => start_ms + i * (dt_ms + clientChannel.offsetFreq))
                    .ToArray();

                double[] ys = samples.samples.Select(s => (double)s - offsetY).ToArray();
                appendChunk(xs, ys);
                //Logger.I(tag, $"client: {clientChannel.id}, offset: {clientChannel.offsetEndMs}");
                clientChannel.accEndMs = start_ms + (dt_ms * samples.length);
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
        }


        private double medianShifts(double[] shifts, int startIdx, int dataPoints)
        {
            if (shifts.Length < dataPoints || startIdx + dataPoints > shifts.Length) return double.NaN;
            var window = shifts.Skip(startIdx).Take(dataPoints).OrderBy(v => v).ToArray();
            int mid = dataPoints / 2;
            return (dataPoints % 2 == 0) ? (window[mid - 1] + window[mid]) / 2.0 : window[mid];
        }

        private double smoothShift(double prev, double current, double alpha = 0.5)
        {
            return alpha * current + (1 - alpha) * prev;
        }

        //public void SynchRecord()
        //{
        //    Logger.W(tag, $"NOT IMPLEMENTED");
        //}


        public void clearCorrHist()
        {
            Array.Clear(correlations, 0, correlations.Length);
            Array.Clear(timeStamps, 0, timeStamps.Length);

            Array.Clear(shiftsAvg, 0, shiftsAvg.Length);

            corrIdx = 0;
        }


        public void prepareData(ref FormsPlot formsPlotTimeShiftsRef, ref FormsPlot formsPlotRef)
        {

            var scatter = formsPlotRef.Plot.Add.Scatter(this.X, this.Y);
            scatter.LegendText = $"Client {this.id}";


            var scatter2 = formsPlotTimeShiftsRef.Plot.Add.Scatter(this.timeStamps, this.correlations);
            scatter2.LineWidth = 0;
            scatter2.LegendText = $"dT {this.id}";

            var scatter3 = formsPlotTimeShiftsRef.Plot.Add.Scatter(this.timeStamps, this.shiftsAvg);
            scatter3.MarkerSize = 0;

            switch (this.id)
            {
                case 11:
                    var color = new ScottPlot.Color(0, 125, 0);
                    scatter.Color = color;
                    scatter2.Color = color;
                    scatter3.Color = color;
                    break;
                case 12:
                    //scatter.Color = new ScottPlot.Color(255, 0, 0); // Red
                    color = new ScottPlot.Color(255, 0, 0);
                    scatter.Color = color;
                    scatter2.Color = color;
                    scatter3.Color = color;
                    break;
                case 13:
                    //scatter.Color = new ScottPlot.Color(0, 0, 255); // Blue
                    color = new ScottPlot.Color(0, 0, 255);
                    scatter.Color = color;
                    scatter2.Color = color;
                    scatter3.Color = color;
                    break;
                default:
                    //scatter.Color = new ScottPlot.Color(0, 0, 0); // Black
                    //var color = new ScottPlot.Color(0, 125, 0);
                    color = new ScottPlot.Color(0, 0, 0);
                    scatter.Color = color;
                    scatter2.Color = color;
                    scatter3.Color = color;
                    break;
            }
        }
    }
}
