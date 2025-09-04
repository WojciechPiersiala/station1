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
    internal class PdmPlotter
    {

        private List<double> accumulatedXs = new List<double>();
        private List<double> accumulatedYs = new List<double>();

        public FormsPlot formsPlotRef;
        private List<ClientChannel> connectedClients;
        //private ConcurrentQueue<AudioData> sampleQueue;
        public PdmPlotter(FormsPlot formsPlotRef, List<ClientChannel> connectedClients /*, ConcurrentQueue<AudioData> sampleQueue*/)
        {
            this.formsPlotRef = formsPlotRef;
            this.connectedClients = connectedClients;
            //this.sampleQueue = sampleQueue;
        }

        

        public async Task Plot(CancellationToken clcTok)
        {
            formsPlotRef.Invoke((MethodInvoker)delegate
            {
                formsPlotRef.Plot.Axes.SetLimitsY(-3000, -500);
                formsPlotRef.Plot.Axes.SetLimitsX(0, 1000);
            });
            const int maxChunks = 16; // 10 -> 1.04 s
            formsPlotRef.Plot.Axes.SetLimitsY(800, 1600); // set Y limit
            while (!clcTok.IsCancellationRequested)
            {
                if (connectedClients.Count > 0)
                {
                    ConcurrentQueue<AudioData> sampleQueue = connectedClients[0].sampleQueue; // always plot channel 0
                    if (sampleQueue.TryDequeue(out AudioData samples))
                    {
                        double start_ms = (double)samples.timestamp / 1000.0; //ms
                        double dt_ms = 1 / 16000.0 * 1000.0; // 0.0625 ms per sample
                        double stop_ms = start_ms + (dt_ms * samples.length);

                        double[] xs = Enumerable.Range(0, samples.length)
                            .Select(i => start_ms + i * dt_ms)
                            .ToArray();

                        double[] ys = samples.samples.Select(s => (double)s).ToArray();

                        // Accumulate data
                        accumulatedXs.AddRange(xs);
                        accumulatedYs.AddRange(ys);

                        if (accumulatedXs.Count >= maxChunks * samples.length)
                        {
                            formsPlotRef.Invoke((MethodInvoker)delegate
                            {
                                formsPlotRef.Plot.Clear();
                                formsPlotRef.Plot.Add.Scatter(accumulatedXs.ToArray(), accumulatedYs.ToArray());
                                formsPlotRef.Plot.Axes.SetLimitsX(accumulatedXs.ToArray().First(), accumulatedXs.ToArray().Last());
                                formsPlotRef.Refresh();
                            });
                            accumulatedXs.Clear(); // Clear accumulated data after plotting
                            accumulatedYs.Clear();
                        }
                    }
                    await Task.Delay(10, clcTok); // throttle refresh rate
                }
            } //task while loop
        }
    }
}
