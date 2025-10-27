using HarfBuzzSharp;
using ScottPlot;
using ScottPlot.Plottables;
using ScottPlot.WinForms;
using ScottPlot.WinForms;
using station1.Models;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace station1.Models
{
    internal class Locater
    {
        private Stopwatch stopWatch = Stopwatch.StartNew();
        private static string tag = "Locater";
        private FormsPlot formsPlot_locate;
        private FormsPlot formsPlot_doa;
        private FormsPlot formsPlot_TDoA;


        //private const double c = 343; //343 m/s -> mm/s          speed of sound
        private double delta12 = -1.0;
        private double delta13 = -1.0;
        //private double phi = 0.0;

        private int plotIdx = 0;
        private double[] angles;
        private double[] timestamps;

        private double lastPlotUpdateMs = 0.0;
        private int plotN = Globals.MaxPlotHist;

        private Scatter? doaScatter;


        private double x11 = 0.0;
        private double y11 = 0.0;

        private double x12 = 250.0;
        private double y12 = 0.0;

        private double x13 = -250.0;
        private double y13 = 0.0;


//#if true

        private double tdoa_x11 = 0.0;
        private double tdoa_y11 = 1500;

        private double tdoa_x12 = 1500;
        private double tdoa_y12 = 0.0;

        private double tdoa_x13 = -1500;
        private double tdoa_y13 = 0.0;
//#else
//        private double tdoa_x11 = 0.0;
//        private double tdoa_y11 = 0.0;

//        private double tdoa_x12 = 250.0;
//        private double tdoa_y12 = 0.0;

//        private double tdoa_x13 = -250.0;
//        private double tdoa_y13 = 0.0;
//#endif
        public Locater(FormsPlot formsPlot_locate, FormsPlot formsPlot_doa, FormsPlot formsPlot_TDoA)
        {
            Logger.I(tag, "Locater initialized");
            this.formsPlot_locate = formsPlot_locate;
            this.formsPlot_doa = formsPlot_doa;
            this.formsPlot_TDoA = formsPlot_TDoA;

            //this.angles = new double[Globals.MaxPlotHist];
            //this.timestamps = new double[Globals.MaxPlotHist];


            this.angles = new double[plotN];
            this.timestamps = new double[plotN];

            doaScatter = formsPlot_doa.Plot.Add.Scatter(new double[plotN], new double[plotN]);
            doaScatter.Color = ScottPlot.Colors.CornflowerBlue;
            doaScatter.MarkerSize = 5;
        }

        public void reset()
        {
            this.delta12 = -1.0;
            this.delta13 = -1.0;
            //this.phi = 0.0;
            this.plotIdx = 0;
            Array.Clear(this.angles, 0, this.angles.Length);
            Array.Clear(this.timestamps, 0, this.timestamps.Length);
            //Draw();
        }


        public void localise(ref List<KeyValuePair<AudioChunkChannel, AudioRecord>> snap)
        {
            double nowMs = stopWatch.Elapsed.TotalMilliseconds;
            if (nowMs > lastPlotUpdateMs + Globals.refreshPlotRate)
            {
                lastPlotUpdateMs = nowMs;
            }
            else
            {
                return; // skip update
            }

            doa(ref snap);
            tdoa(ref snap);

        }
        public void doa(ref List<KeyValuePair<AudioChunkChannel, AudioRecord>> snap)
        {
            bool logEnabled = false;
            AudioRecord reci12 = null;
            AudioRecord reci13 = null;

            for (int i = 0; i < snap.Count; i++)
            {
                var (chani, reci) = (snap[i].Key, snap[i].Value);   //the channel to update

                if (reci.id == 11)
                {
                    continue; // skip reference mic
                }
                else if (reci.id == 12)
                {
                    reci12 = reci;
                }
                else if (reci.id == 13)
                {
                    reci13 = reci;
                }
            }



            const double c_mm_per_s = 343.0; // mm/s
            double d_mm = Math.Abs(x13); // spacing between ref mic and measurement mic


            double dt12_s = reci12.lastCorr;
            double dt13_s = reci13.lastCorr;
            double sinTheta12 =  (c_mm_per_s * dt12_s) / d_mm;
            double sinTheta13 = -(c_mm_per_s * dt13_s) / d_mm;

            double sinTheta = (sinTheta12 + sinTheta13) /2;
            sinTheta = Math.Max(-1.0, Math.Min(1.0, sinTheta));

            double phi = Math.Asin(sinTheta);
            double phiDeg = phi * 180.0 / Math.PI;

            phi += Math.PI / 2.0; // shift to [0, pi] range
            phiDeg += 90.0; // shift to [0, 180] range

            if (logEnabled)
                Logger.I(tag, $"DOA computed: sin theta={sinTheta:F4}, DOA={phiDeg:F2}°");
            

            Draw(phi);
            Plot(phiDeg); 
        }

        public void Plot(double phiDeg)
        {
            double currTime = stopWatch.Elapsed.TotalMilliseconds;
            angles[plotIdx] = phiDeg;
            timestamps[plotIdx] = currTime;

            plotIdx = (plotIdx + 1) % plotN;

            // Instead of Update(), reassign the plottable data
            formsPlot_doa.Plot.Remove(doaScatter); // remove old scatter
            var y0Main = formsPlot_doa.Plot.Add.HorizontalLine(90);
            doaScatter = formsPlot_doa.Plot.Add.Scatter(timestamps, angles);
            doaScatter.Color = ScottPlot.Colors.OrangeRed;
            doaScatter.MarkerSize = 0;
            doaScatter.LineWidth = 3;

            // Add horizontal line at y=0
            //formsPlot_doa.Plot.Remove(doaScatter);
            //var y0Main = formsPlot_doa.Plot.Add.HorizontalLine(90);
            y0Main.Color = new ScottPlot.Color(117, 117, 117);      // gray
            y0Main.LineWidth = 2.5f;


            if (timestamps.Any(x => x > 100))
            {
                double xLimMin = timestamps.Where(x => x > 100).Min();
                double xLimMax = timestamps.Max();
                formsPlot_doa.Plot.Axes.SetLimitsX(xLimMin, xLimMax);
            }
            formsPlot_doa.Plot.Axes.SetLimitsY(0, 180);

            formsPlot_doa.Invoke((MethodInvoker)(() => formsPlot_doa.Refresh()));
        }


        public void Draw(double phi)
        {
            var plt = formsPlot_locate.Plot;
            plt.Clear();

            // direction arrow
            double r = 200;
            double x2 = Math.Cos(phi) * r;
            double y2 = Math.Sin(phi) * r;
            var arrow = plt.Add.Arrow(0, 0, x2, y2);
            //arrow.ArrowFillColor = ScottPlot.Colors.Red;
            arrow.ArrowWidth = 4;
            arrow.ArrowLineWidth = 1;

            // microphones as points (no connecting line)
            plt.Add.Marker(x12, y12, color: ScottPlot.Colors.Red, size: 10);
            plt.Add.Marker(0, 0, color: ScottPlot.Colors.Green, size: 10);
            plt.Add.Marker(x13, y13, color: ScottPlot.Colors.Blue, size: 10);


            plt.Axes.SetLimits(-300, 300, -300, 300);
            formsPlot_locate.Invoke((MethodInvoker)(() => formsPlot_locate.Refresh()));
        }




        //private double x11 = 0.0;
        //private double y11 = 0.0;

        //private double x12 = 250.0;
        //private double y12 = 0.0;

        //private double x13 = -250.0;
        //private double y13 = 0.0;


        public void tdoa(ref List<KeyValuePair<AudioChunkChannel, AudioRecord>> snap)
        {
            // 1. Get the most recent correlation results
            AudioRecord reci12 = null;
            AudioRecord reci13 = null;

            for (int i = 0; i < snap.Count; i++)
            {
                var (chani, reci) = (snap[i].Key, snap[i].Value);

                if (reci.id == 11)
                    continue; // skip reference mic
                else if (reci.id == 12)
                    reci12 = reci;
                else if (reci.id == 13)
                    reci13 = reci;
            }

            // 2. Constants
            const double c_mm_per_s = 343.0; // mm/ms (speed of sound)

            // 3. Convert measured delays to distance differences (Δd = c * Δt)
            double dt12_s = reci12.lastCorr;
            double dt13_s = reci13.lastCorr;
            double Theta12 = c_mm_per_s * dt12_s;
            double Theta13 = c_mm_per_s * dt13_s;

            // 4. Initial guess for source position (somewhere above center)
            double x_s = 0.0;
            double y_s = 800.0;

            double lr = 0.005;        // learning rate
            const int maxIter = 1000; // max iterations
            const double eps = 1e-6;  // convergence threshold

            // 5. Gradient descent optimization
            int iterDone = 0;
            for (int iter = 0; iter < maxIter; iter++)
            {
                // Distances from current guess to each microphone
                double r11 = Math.Sqrt(Math.Pow(x_s - tdoa_x11, 2) + Math.Pow(y_s - tdoa_y11, 2));
                double r12 = Math.Sqrt(Math.Pow(x_s - tdoa_x12, 2) + Math.Pow(y_s - tdoa_y12, 2));
                double r13 = Math.Sqrt(Math.Pow(x_s - tdoa_x13, 2) + Math.Pow(y_s - tdoa_y13, 2));

                // Predicted vs. measured difference
                double diff12 = (r12 - r11) - Theta12;
                double diff13 = (r13 - r11) - Theta13;

                // Loss function (sum of squares + small regularization)
                double E = diff12 * diff12 + diff13 * diff13 + 1e-6 * (x_s * x_s + y_s * y_s);

                // Numerical gradient (central difference)
                double h = 1e-3;
                double dEx = (Error(x_s + h, y_s) - Error(x_s - h, y_s)) / (2 * h);
                double dEy = (Error(x_s, y_s + h) - Error(x_s, y_s - h)) / (2 * h);

                // Gradient update
                x_s -= lr * dEx;
                y_s -= lr * dEy;

                // Stop if converged
                if (Math.Sqrt(dEx * dEx + dEy * dEy) < eps)
                    break;

                // Local error function helper
                double Error(double x, double y)
                {
                    double r11_ = Math.Sqrt(Math.Pow(x - tdoa_x11, 2) + Math.Pow(y - tdoa_y11, 2));
                    double r12_ = Math.Sqrt(Math.Pow(x - tdoa_x12, 2) + Math.Pow(y - tdoa_y12, 2));
                    double r13_ = Math.Sqrt(Math.Pow(x - tdoa_x13, 2) + Math.Pow(y - tdoa_y13, 2));

                    double d12_ = (r12_ - r11_) - Theta12;
                    double d13_ = (r13_ - r11_) - Theta13;
                    return d12_ * d12_ + d13_ * d13_ + 1e-6 * (x * x + y * y);
                }
                iterDone++;
            }

            Logger.I(tag, $"TDoA triangulated source: x={x_s:F2} mm, y={y_s:F2} mm, finised after {iterDone} iterations");

            // 6. Plot the localization result
            plotTDoA(x_s, y_s);
        }

        private void plotTDoA(double x_s, double y_s)
        {
            var plt = formsPlot_TDoA.Plot;
            plt.Clear();

            // microphones as points (no connecting line)
            plt.Add.Marker(tdoa_x11, tdoa_y11, color: ScottPlot.Colors.Green, size: 10);
            plt.Add.Marker(tdoa_x12, tdoa_y12, color: ScottPlot.Colors.Red, size: 10);
            plt.Add.Marker(tdoa_x13, tdoa_y13, color: ScottPlot.Colors.Blue, size: 10);
            plt.Add.Marker(x_s, y_s, color: ScottPlot.Colors.Black, size: 15, shape: MarkerShape.OpenCircle);

            var addText = plt.Add.Text($"Source: \nx={x_s:F1} mm \ny={y_s:F1} mm", -470, 470);
            addText.LabelFontSize = 16;
            addText.LabelBold = true;
            addText.LabelBorderWidth = 1;
            addText.LabelBorderColor = ScottPlot.Colors.Black;

            //plt.Axes.SetLimits(tdoa_x12 + 20, tdoa_x13 - 20, tdoa_x11 - 20, tdoa_y11 + 20);
            plt.Axes.SetLimits(-1500, 1500, -1000, 1500);
            formsPlot_TDoA.Invoke((MethodInvoker)(() => formsPlot_TDoA.Refresh()));
        }



    }

}




