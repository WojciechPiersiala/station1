using ScottPlot;
using ScottPlot.WinForms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace station1.Models
{
    internal class AudioRecord
    {
        private Logger log;
        public readonly List<double> X = new();  // accumulated time in ms
        public readonly List<double> Y = new();  // accumulated audio samples
        public bool recordFull = false;
        const int maxChunks = 64; // 10 -> 1.04 s //16 old
        public AudioRecord(Logger log)
        {
            this.log = log;
        }

        public void appendData(ClientChannel clientChannel)
        {
            while (clientChannel.sampleQueue.TryDequeue(out AudioData samples))
            {
                double start_ms = (double)samples.timestamp / 1000.0; //ms
                double dt_ms = 1 / 16000.0 * 1000.0; // 0.0625 ms per sample
                double stop_ms = start_ms + (dt_ms * samples.length);

                double[] xs = Enumerable.Range(0, samples.length)
                    .Select(i => start_ms + i * dt_ms)
                    .ToArray();

                double[] ys = samples.samples.Select(s => (double)s).ToArray();

                // Accumulate data
                X.AddRange(xs);
                Y.AddRange(ys);
                recordFull = (X.Count >= maxChunks * samples.length);
            }
        }
    }






    internal class PdmPlotter
    {
        private int countClients = 0;
        private string tag = "plotter";
        private Logger log;
        public FormsPlot formsPlotRef; // reference to windows form plot
        private List<ClientChannel> clientsBuffer;
        //private List<AudioRecord> recordsBuffer = new();
        private ConcurrentDictionary<ClientChannel, AudioRecord> plotBuffer = new ();
        public PdmPlotter(FormsPlot formsPlotRef, List<ClientChannel> clientsBuffer, Logger log)
        {
            this.formsPlotRef = formsPlotRef;
            this.clientsBuffer = clientsBuffer;
            this.log = log;
        }



        public async Task Plot(CancellationToken clcTok)
        {
            log.Log_I(tag, "Plotter started");
            int conCountPrev= 0;
            int conCount= clientsBuffer.Count;
            while (!clcTok.IsCancellationRequested)
            {
                //// New client connected
                //conCount = clientsBuffer.Count;
                ////log.Log_I(tag, $" connected now: {conCount}, previously: {conCountPrev}");
                //if (conCount > conCountPrev)
                //{
                //    plotBuffer.TryAdd(clientsBuffer.Last(), new AudioRecord(log));
                //    log.Log_I("New client detected in the plotter");
                //    //log.Log_I($"clients buffer len: {clientsBuffer.Count.ToString()}");
                //    log.Log_I($"plot buffer len: {plotBuffer.Count.ToString()}");
                //    conCountPrev = conCount;
                //}

                List<ClientChannel> snap;
                // Ensure every current client has an AudioRecord
                lock (clientsBuffer) snap = clientsBuffer.ToList();
                foreach (var c in snap)
                {
                    plotBuffer.TryAdd(c, new AudioRecord(log));
                }

                // Remove records for clients that disconnected
                foreach (var key in plotBuffer.Keys)
                {
                    if (!snap.Contains(key)) plotBuffer.TryRemove(key, out _);
                }


                formsPlotRef.Invoke((MethodInvoker)delegate { formsPlotRef.Plot.Clear(); });
                //if (plotBuffer.Count > countClients)
                //{
                //    countClients = plotBuffer.Count;
                //    log.Log_I(tag, $"New client detected. Starting client synchronisation. Number of clients to plot: {countClients}");
                //    foreach (var it in plotBuffer)
                //        it.Value.synch();
                //}
                double yLimMin = double.MaxValue;
                double yLimMax = 0;
                foreach (var it in plotBuffer)
                {
                    //private ConcurrentDictionary<ClientChannel, AudioRecord> plotBuffer = new ();
                    ClientChannel cc = it.Key; //ClientChannel
                    AudioRecord ar = it.Value; //AudioRecord
                    ar.appendData(cc);
                    
                    if (ar.X.Count == 0) 
                        continue; // nothing to plot
                    //formsPlotRef.Invoke((MethodInvoker)delegate
                    //{
                        formsPlotRef.Plot.Add.Scatter(ar.X.ToArray(), ar.Y.ToArray());
                        //formsPlotRef.Plot.Axes.SetLimitsX(ar.X.ToArray().First(), ar.X.ToArray().Last());
                    //});

                    // set the limits 
                    if(ar.X.ToArray().Last() > yLimMax)
                        yLimMax = ar.X.ToArray().Last();

                    if (ar.X.ToArray().First() < yLimMin)
                        yLimMin = ar.X.ToArray().First();

                    if (ar.recordFull)
                    {
                        ar.X.Clear(); // Clear accumulated data after plotting
                        ar.Y.Clear();
                        ar.recordFull = false;
                    }
                }
                //refresh plot
                formsPlotRef.Plot.Axes.SetLimitsX(yLimMin, yLimMax);
                formsPlotRef.Invoke((MethodInvoker)delegate { formsPlotRef.Refresh(); });
                await Task.Delay(100, clcTok); //refresh rate

            }
            //    formsPlotRef.Invoke((MethodInvoker)delegate
            //    {
            //        formsPlotRef.Plot.Axes.SetLimitsY(-3000, -500);
            //        formsPlotRef.Plot.Axes.SetLimitsX(0, 1000);
            //    });
            //    //const int maxChunks = 16; // 10 -> 1.04 s
            //    //formsPlotRef.Plot.Axes.SetLimitsY(800, 1600); // set Y limit
            //    while (!clcTok.IsCancellationRequested)
            //    {
            //        foreach (var client in clientsBuffer)
            //        {

            //        }

            //            if (accumulatedXs.Count >= maxChunks * samples.length)
            //                {
            //                    formsPlotRef.Invoke((MethodInvoker)delegate
            //                    {
            //                        formsPlotRef.Plot.Clear();
            //                        formsPlotRef.Plot.Add.Scatter(accumulatedXs.ToArray(), accumulatedYs.ToArray());
            //                        formsPlotRef.Plot.Axes.SetLimitsX(accumulatedXs.ToArray().First(), accumulatedXs.ToArray().Last());
            //                        formsPlotRef.Refresh();
            //                    });
            //                    accumulatedXs.Clear(); // Clear accumulated data after plotting
            //                    accumulatedYs.Clear();
            //                }
            //            }
            //            await Task.Delay(10, clcTok); // throttle refresh rate
            //        }
            //    } //task while loop
        }
    }
}