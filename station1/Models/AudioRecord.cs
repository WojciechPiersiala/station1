using ScottPlot;
using ScottPlot.WinForms;
using System;
using System.Collections.Generic;
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
        public double lastSmoothedShift = 0;
        private bool shiftsFull = false;
        public bool isFirstChannel = false;
        public double[] shifts;
        public double[] timeStamps;

        public double[] shiftsAvg;

        private int shiftIdx = 0;


        private string tag;

        public int id = -1;
        private double offsetY = 0;
        private bool doSynch = true;
        public double[] X;  // accumulated time in ms
        public double[] Y;  // accumulated audio samples
        private int chunkIdx = 0;




        /*   Frequency synchronisation    */
        private double lastServoApplyTsMs = -1;
        private double integPpm = 0;  // integrator state in ppm
        //private double lastServoApplyTsMs = -1;
        private const double SERVO_COOLDOWN_MS = 2000; // 2 seconds

        private const double STEP_THRESHOLD_MS = 0.05; // 50 µs
        private const int IGNORE_NEWEST = 3;           // don’t use freshest points
        private const int MIN_POINTS = 20;             // enough for a stable fit

        public AudioRecord(int id, /*int Capacity,*/ bool isFirstChannel)
        {
            this.isFirstChannel = isFirstChannel;
            tag = $"AudioRecord {id}";


            this.X = new double[Globals.Capacity];
            this.Y = new double[Globals.Capacity];


            this.shifts = new double[Globals.MaxPlotHist];
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



        //public void appendData(AudioChunkChannel clientChannel, double serverNowMs)
        //{
        //    while (clientChannel.sampleQueue.TryDequeue(out AudioChunk samples))
        //    {
        //        if (samples.length != Globals.SamplesPerChunk)
        //        {
        //            Logger.E(tag, $"Unexpected samples length: got {samples.length}, expected {Globals.SamplesPerChunk}");
        //            continue;
        //        }


        //        double packetStart_ms = ((double)samples.timestamp / 1000.0); //ms
        //        bool synchRequired = (clientChannel.offsetMs == null) || doSynch;
        //        if (synchRequired)
        //        {
        //            // synchronise time series 
        //            //new client offset is not assigned yet.
        //            Logger.I(tag, $"Initial Synchronising client: {clientChannel.id}");
        //            clientChannel.offsetMs = serverNowMs - packetStart_ms;
        //            doSynch = false;
        //            clientChannel.accEndMs = null; // reset accumulated end time
        //        }


        //        double dt_ms = 1 / (double)(clientChannel.Freq) * 1000.0; // dt in ms

        //        //double start_ms = (double)(packetStart_ms + clientChannel.offsetMs);


        //        //double start_ms = clientChannel.accEndMs ?? (packetStart_ms + clientChannel.offsetMs.Value);

        //        double devStart_ms = packetStart_ms + clientChannel.offsetMs.Value;
        //        double expected_ms = clientChannel.accEndMs ?? devStart_ms;
        //        double delta_ms = devStart_ms - expected_ms;

        //        // threshold for gap detection
        //        double gapThresh_ms = 300 * dt_ms;
        //        bool discontinuity = (clientChannel.accEndMs == null) || Math.Abs(delta_ms) > gapThresh_ms;

        //        // pick which start to use
        //        double start_ms = discontinuity ? devStart_ms : expected_ms;

        //        if (discontinuity)
        //        {
        //            Logger.W(tag, $"id {id} Discontinuity {delta_ms:F3} ms ({delta_ms / dt_ms:+0;-0} samples). Re-anchoring.");
        //        }

        //        //double stop_ms = start_ms + (dt_ms * samples.length);

        //        double[] xs = Enumerable.Range(0, samples.length)
        //            .Select(i => start_ms + i * dt_ms)
        //            .ToArray();

        //        double[] ys = samples.samples.Select(s => (double)s - offsetY).ToArray();
        //        appendChunk(xs, ys);

        //        clientChannel.accEndMs = start_ms + dt_ms * samples.length;

        //        if (synchRequired) // normalize the data during synchronisation
        //        {
        //            double avgY = Y.Average();
        //            //if (Math.Abs(avgY) > 15)
        //            offsetY += Y.Average();
        //        }
        //    }
        //}


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
                    clientChannel.accEndMs = null; // reset accumulated end time
                }


                double dt_ms = 1 / (double)(clientChannel.Freq) * 1000.0; // dt in ms

                //double start_ms = (double)(packetStart_ms + clientChannel.offsetMs);

                double start_ms = clientChannel.accEndMs ?? (packetStart_ms + clientChannel.offsetMs.Value);

                //double stop_ms = start_ms + (dt_ms * samples.length);

                double[] xs = Enumerable.Range(0, samples.length)
                    .Select(i => start_ms + i * dt_ms)
                    .ToArray();

                double[] ys = samples.samples.Select(s => (double)s - offsetY).ToArray();
                appendChunk(xs, ys);

                clientChannel.accEndMs = start_ms + dt_ms * samples.length;

                if (synchRequired) // normalize the data during synchronisation
                {
                    double avgY = Y.Average();
                    //if (Math.Abs(avgY) > 15)
                    offsetY += Y.Average();
                }
            }
        }



        /*   Frequency synchronisation    */
        private bool TryBuildPreStepWindow(int M, out List<double> xs, out List<double> ys)
        {
            xs = new(); ys = new();

            int idx = (shiftIdx - 1 - IGNORE_NEWEST + Globals.MaxPlotHist) % Globals.MaxPlotHist;

            double? prevY = null;
            while (xs.Count < M)
            {
                double x = timeStamps[idx];
                double y = shiftsAvg[idx];
                if (x <= 0) break;

                if (prevY.HasValue && Math.Abs(y - prevY.Value) > STEP_THRESHOLD_MS)
                    break; // hit a step → stop, we keep only the pre-step contiguous segment

                xs.Add(x); ys.Add(y);
                prevY = y;
                idx = (idx - 1 + Globals.MaxPlotHist) % Globals.MaxPlotHist;
            }

            // we filled from newest→older, reverse to chronological order
            xs.Reverse(); ys.Reverse();
            return xs.Count >= MIN_POINTS;
        }


        public void nudgeFreq(ref AudioChunkChannel chani)
        {
            const int M = 30;      // regression window (points)
            const double Kp = 0.030;    // proportional on slope (ppm per ppm)
            const double Ki = 0.005;    // integral on phase (ppm per ms per update) -> start small
            const double maxPPM = 15000;   // hard safety clamp
            const double deadbandPPM = 0.0000005;

            // cooldown: avoid reacting before new measurement reflects the last change
            int lastIdx = (shiftIdx - 1 + Globals.MaxPlotHist) % Globals.MaxPlotHist;
            double nowMs = timeStamps[lastIdx];
            if (nowMs <= 0 || (lastServoApplyTsMs > 0 && (nowMs - lastServoApplyTsMs) < 2000))
                return;

            // build robust pre-step window (ignores newest points and any step)
            if (!TryBuildPreStepWindow(M, out var xs, out var ys))
                return;

            // linear regression on (xs, ys)
            int N = xs.Count;
            double sumX = 0, sumY = 0, sumXX = 0, sumXY = 0;
            for (int i = 0; i < N; i++) { sumX += xs[i]; sumY += ys[i]; sumXX += xs[i] * xs[i]; sumXY += xs[i] * ys[i]; }
            double denom = N * sumXX - sumX * sumX;
            if (Math.Abs(denom) < 1e-9) return;

            double slope = (N * sumXY - sumX * sumY) / denom; // ms/ms
            double slopePpm = slope * 1e6;                    // ppm

            // deadband around zero slope
            if (Math.Abs(slopePpm) < deadbandPPM) slopePpm = 0;

            // phase error (latest filtered point)
            double phaseMs = ys[^1]; // target is 0.0 ms

            // PI: proportional on slope, integral on phase
            integPpm += Ki * phaseMs;

            // anti-windup
            if (integPpm > maxPPM) integPpm = maxPPM;
            if (integPpm < -maxPPM) integPpm = -maxPPM;

            double cmdPpm = (Kp * slopePpm) + integPpm;

            // clamp
            if (cmdPpm > maxPPM) cmdPpm = maxPPM;
            if (cmdPpm < -maxPPM) cmdPpm = -maxPPM;

            // apply as multiplicative correction to effective rate
            double corr = 1.0 - (cmdPpm / 1e6);
            chani.Freq *= corr;

            lastServoApplyTsMs = nowMs;

            Logger.I(tag, $"servo: slope={slopePpm:F1}ppm phase={phaseMs:F4}ms cmd={cmdPpm:F1}ppm Freq={chani.Freq:F6}Hz");
        }





        public double rememberShift(double timeStamp, double shift /*ms*/)
        {
            if (timeStamp < 100.0) return double.NaN;

            int idx = shiftIdx;                      

            shifts[idx] = shift;
            timeStamps[idx] = timeStamp;

            double filtered;
            int Navg = Globals.Navg;

            if (countValid(timeStamps) >= Navg)  
            {
                double[] win = new double[Navg];
                for (int k = 0; k < Navg; k++)
                {
                    int j = (idx - k + Globals.MaxPlotHist) % Globals.MaxPlotHist;
                    win[k] = shifts[j];
                }
                Array.Sort(win);
                filtered = (Navg % 2 == 0) ? 0.5 * (win[Navg / 2 - 1] + win[Navg / 2]) : win[Navg / 2];
            }
            else
            {
                filtered = 0.0; // must be 0.0 shift;      
            }


            lastSmoothedShift = smoothShift(lastSmoothedShift, filtered);
            shiftsAvg[idx] = lastSmoothedShift;     

            shiftIdx = (idx + 1) % Globals.MaxPlotHist;
            return lastSmoothedShift;
        }


        private int countValid(double[] ts)
        {
            // counts how many elements have been written (ts > 0)
            int n = 0;
            for (int i = 0; i < ts.Length; i++)
                if (ts[i] > 0) n++;
            return n;
        }


        private double medianShifts(double[] shifts, int startIdx, int dataPoints)
        {
            if (shifts.Length < dataPoints || startIdx + dataPoints > shifts.Length) return double.NaN;
            var window = shifts.Skip(startIdx).Take(dataPoints).OrderBy(v => v).ToArray();
            int mid = dataPoints / 2;
            return (dataPoints % 2 == 0) ? (window[mid - 1] + window[mid]) / 2.0 : window[mid];
        }

        private double smoothShift(double prev, double current, double alpha = 0.3)
        {
            return alpha * current + (1 - alpha) * prev;
        }

        public void Synch()
        {
            doSynch = true;
        }


        public void clearShiftHistory()
        {
            Array.Clear(shifts, 0, shifts.Length);
            Array.Clear(timeStamps, 0, timeStamps.Length);

            Array.Clear(shiftsAvg, 0, shiftsAvg.Length);

            shiftIdx = 0;
        }


        public void prepareData(ref FormsPlot formsPlotTimeShiftsRef, ref FormsPlot formsPlotRef)
        {

            var scatter = formsPlotRef.Plot.Add.Scatter(this.X, this.Y);
            scatter.LegendText = $"Client {this.id}";


            var scatter2 = formsPlotTimeShiftsRef.Plot.Add.Scatter(this.timeStamps, this.shifts);
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
