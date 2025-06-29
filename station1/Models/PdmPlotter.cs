using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ScottPlot;
using ScottPlot.WinForms;

namespace station1.Models
{
    internal class PdmPlotter
    {
        public FormsPlot formsPlotRef;
        private ConcurrentQueue<short[]> sampleQueue;
        public PdmPlotter(FormsPlot formsPlotRef, ConcurrentQueue<short[]> sampleQueue)
        {
            this.formsPlotRef = formsPlotRef;
            this.sampleQueue = sampleQueue;
        }

        

        public async Task Plot(CancellationToken clcTok)
        {
            formsPlotRef.Invoke((MethodInvoker)delegate
            {
                formsPlotRef.Plot.Axes.SetLimitsY(-3000, -500);
                formsPlotRef.Plot.Axes.SetLimitsX(0, 1000);
            });
            
            while (!clcTok.IsCancellationRequested)
            {
                if (sampleQueue.TryDequeue(out short[] samples))
                {
                    //Random rand = new Random();
                    //short[] samples = new short[500];
                    //for (int i = 0; i < samples.Length; i++)
                    //    samples[i] = (short)rand.Next(-3000, 3000); // scale for visibility

                    double[] ys = samples.Select(s => (double)s).ToArray();
                    double[] xs = Enumerable.Range(0, ys.Length).Select(i => (double)i).ToArray();

                    formsPlotRef.Invoke((MethodInvoker)delegate
                    {
                        formsPlotRef.Plot.Clear();
                        formsPlotRef.Plot.Add.Scatter(xs, ys);
                        //formsPlotRef.Plot.Axes.AutoScale();
                        formsPlotRef.Refresh();
                    });
                }
                await Task.Delay(10, clcTok); // throttle refresh rate
            }
        }
    }
}
