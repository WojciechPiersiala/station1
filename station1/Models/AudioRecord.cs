using ScottPlot.WinForms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace station1.Models
{
    internal class AudioRecord
    {
        public bool isFirstChannel = false;
        public double[] shifts;
        public double[] timeStamps;
        private int shiftsN = 1000;
        private int shiftIdx = 0;
        //private double recntTime = 0.0;

        private string tag;
        //private int samplingRate;
        //private int maxChunks; // 16 -> 1.04 s //16 old
        //private int audioLen;
        //private int SamplesPerChunk; // number of audio samples in a single chunk
        //private int Capacity;

        public int id = -1;
        private double offsetY = 0;
        private bool doSynch = true;
        public double[] X;  // accumulated time in ms
        public double[] Y;  // accumulated audio samples
        private int chunkIdx = 0;
        public AudioRecord(int id, /*int Capacity,*/ bool isFirstChannel)
        {
            this.isFirstChannel = isFirstChannel;
            tag = $"AudioRecord {id}";
            //this.samplingRate = PdmPlotter.samplingRate;
            //this.maxChunks = PdmPlotter.maxChunks; // 16 -> 1.04 s //16 old
            //this.audioLen = PdmPlotter.audioLen;
            //this.SamplesPerChunk = PdmPlotter.SamplesPerChunk; // number of audio samples in a single chunk
            //this.Capacity = Capacity;

            this.X = new double[Globals.Capacity];
            this.Y = new double[Globals.Capacity];


            this.shifts = new double[shiftsN];
            this.timeStamps = new double[shiftsN];

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
            //Array.Sort(X, Y);
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
                //recntTime = start_ms;
                double dt_ms = 1 / (double)Globals.SamplingRate * 1000.0; // dt in ms
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

        public void updateShift(double timeStamp, double shift /*ms*/)
        {
            //remember time shifts
            if (timeStamp < 100.0) return;
            //if (Math.Abs(shift) > Globals.MinValidSchiftUs) return; //ms

            shifts[shiftIdx] = shift; //ms to us
            timeStamps[shiftIdx] = timeStamp;
            shiftIdx++;

            //Array.Sort(timeStamps, shifts); // keep time sorted
            if (shiftIdx >= shiftsN)
            {
                shiftIdx = 0;
            }
        }

        public void Synch()
        {
            doSynch = true;
        }


        public void clearShiftHistory()
        {
            Array.Clear(shifts, 0, shifts.Length);
            Array.Clear(timeStamps, 0, timeStamps.Length);
            shiftIdx = 0;
        }
        public void prepareData(ref FormsPlot formsPlotTimeShiftsRef, ref FormsPlot formsPlotRef)
        {
            //Array.Sort(X, Y);
            var scatter = formsPlotRef.Plot.Add.Scatter(this.X, this.Y);
            scatter.LegendText = $"Client {this.id}";

            //if (!isFirstChannel) Logger.S(tag, $"{this.id}");
            var scatter2 = formsPlotTimeShiftsRef.Plot.Add.Lollipop(this.shifts, this.timeStamps);

            //var lp = formsPlotRef.Plot.Add.Lollipop(ys, xs);
            scatter2.LegendText = $"dT {this.id}";
            //Logger.I(tag, $"\n\nPrepared data for client \n {this.id} \n timeStamps:{string.Join(", ", timeStamps.Select(v => v.ToString("R", CultureInfo.InvariantCulture)))}, shift: {string.Join(", ", shifts.Select(v => v.ToString("R", CultureInfo.InvariantCulture)))}");


            switch (this.id)
            {
                case 11:
                    scatter.Color = new ScottPlot.Color(0, 125, 0); // Green
                    scatter2.Color = new ScottPlot.Color(0, 125, 0); // Green
                    break;
                case 12:
                    scatter.Color = new ScottPlot.Color(255, 0, 0); // Red
                    scatter2.Color = new ScottPlot.Color(255, 0, 0); // Red
                    break;
                case 13:
                    scatter.Color = new ScottPlot.Color(0, 0, 255); // Blue
                    scatter2.Color = new ScottPlot.Color(0, 0, 255); // Blue
                    break;
                default:
                    scatter.Color = new ScottPlot.Color(0, 0, 0); // Black
                    scatter2.Color = new ScottPlot.Color(0, 0, 0); // Black
                    break;
            }
            //scatter2.LineStyle = ScottPlot.LineStyle;
            //scatter2.LineWidth = 2;
            //scatter2.MarkerShape = ScottPlot.MarkerShape.FilledCircle;
            //scatter2.MarkerSize = 5
        }
    }
}
