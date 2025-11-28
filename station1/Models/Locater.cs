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
    /// <summary>
    /// Klasa zajmujaca sie lokalizacja zrodla dzwieku na podstawie DoA
    /// </summary>
    internal class Locater
    {
        private Stopwatch stopWatch = Stopwatch.StartNew();
        private static string tag = "Locater";
        private FormsPlot formsPlot_locate;
        private FormsPlot formsPlot_doa;
        private FormsPlot formsPlot_TDoA;



        private double delta12 = -1.0;
        private double delta13 = -1.0;


        private int plotIdx = 0;
        public double[] angles;
        public double[] timestamps;

        private double lastPlotUpdateMs = 0.0;
        private int plotN = Globals.MaxPlotHist;

        private Scatter? doaScatter;

        //private double x11 = 0.0;
        private double y11 = 0.0;
        private double x11 = -250.0;

        private double x12 = 250.0;
        //private double x12 = 500.0;
        private double y12 = 0.0;

        private double x13 = -250.0;
        //private double x13 = -500.0;
        //private double y13 = 0.0;
        private double y13 = 500.0;

        private double x_s = 0.0;
        private double y_s = 100.0;

        // historia polozen zrodel
        // Uzywana przez doa2
        private int oldsN = 50;
        private double[] olds_x;
        private double[] olds_y;
        private int oldsIter = 0;


        /// <summary>
        /// Konstruktor klasy Locater
        /// </summary>
        /// <param name="formsPlot_locate"> referencja do wykresu </param>
        /// <param name="formsPlot_doa"> referencja do wykresu  </param>
        /// <param name="formsPlot_TDoA"> referencja do wykresu  </param>
        public Locater(FormsPlot formsPlot_locate, FormsPlot formsPlot_doa, FormsPlot formsPlot_TDoA)
        {
            Logger.I(tag, "Locater initialized");
            this.formsPlot_locate = formsPlot_locate;
            this.formsPlot_doa = formsPlot_doa;
            this.formsPlot_TDoA = formsPlot_TDoA;

            this.angles = new double[plotN];
            this.timestamps = new double[plotN];

            doaScatter = formsPlot_doa.Plot.Add.Scatter(new double[plotN], new double[plotN]);
            doaScatter.Color = ScottPlot.Colors.CornflowerBlue;

            doaScatter.MarkerSize = 5;
            olds_x = new double[plotN];
            olds_y = new double[plotN];
        }


        /// <summary>
        /// Resetuje wszystkie dane
        /// </summary>
        public void reset()
        {
            this.delta12 = -1.0;
            this.delta13 = -1.0;

            this.plotIdx = 0;
            Array.Clear(this.angles, 0, this.angles.Length);
            Array.Clear(this.timestamps, 0, this.timestamps.Length);


            resetTDoAHistory(); // usun historie DOA2
        }


        /// <summary>
        /// Wykonuje lokalizacje doa i doa2
        /// </summary>
        /// <param name="snap"></param>
        public void localise(ref List<KeyValuePair<AudioChunkChannel, AudioRecord>> snap)
        {
            double nowMs = stopWatch.Elapsed.TotalMilliseconds;
            if (nowMs > lastPlotUpdateMs + Globals.refreshPlotRate)
            {
                lastPlotUpdateMs = nowMs;
            }
            else
            {
                return;
            }

            doa(ref snap); // bliskie pole
            doa2(ref snap); // dalekie pole

        }

        /// <summary>
        /// Oblicza Direction of Arrival (DOA) na podstawie czasow opoznien miedzy mikrofonami.
        /// </summary>
        /// <param name="snap"> Aktualny stan wektoru z mikrofonami</param>
        public void doa(ref List<KeyValuePair<AudioChunkChannel, AudioRecord>> snap)
        {
            bool logEnabled = false;
            AudioRecord reci12 = null;
            AudioRecord reci13 = null;

            for (int i = 0; i < snap.Count; i++)
            {
                var (chani, reci) = (snap[i].Key, snap[i].Value);   //wybierz kanal do aktualizacji

                if (reci.id == 11)
                    continue; // pomin mikrofon referencyjny
                else if (reci.id == 12)
                    reci12 = reci;
                else if (reci.id == 13)
                    reci13 = reci;
            }


            const double c_mm_per_s = 343.0; // mm/s
            double d_mm = Math.Abs(x13); // odleklosc od mikrofonu 1 do 3 (symetrycznie do mikrofonu 2)

            double dt12_s = reci12.lastCorr; // znienie czasowe miedzy mikrofonem 11 a 12
            double dt13_s = reci13.lastCorr; // znienie czasowe miedzy mikrofonem 11 a 13
            double sinTheta12 = (c_mm_per_s * dt12_s) / d_mm;
            double sinTheta13 = -(c_mm_per_s * dt13_s) / d_mm;

            double sinTheta = (sinTheta12 + sinTheta13) / 2;
            sinTheta = Math.Max(-1.0, Math.Min(1.0, sinTheta));

            double theta = Math.Asin(sinTheta);
            double thetaiDeg = theta * 180.0 / Math.PI;

            theta += Math.PI / 2.0;
            thetaiDeg += 90.0; // radiany do stopni

            if (logEnabled)
                Logger.I(tag, $"DOA computed: sin theta={sinTheta:F4}, DOA={thetaiDeg:F2}?");

            Draw(theta); // narysuj kierunek
            Plot(thetaiDeg);  // zaktualizuj wykres DOA
        }


        /// <summary>
        /// Przygotowuje dane do wykresu DOA
        /// </summary>
        /// <param name="phiDeg"></param>
        public void Plot(double phiDeg)
        {
            double currTime = stopWatch.Elapsed.TotalMilliseconds;
            angles[plotIdx] = phiDeg;
            timestamps[plotIdx] = currTime;

            plotIdx = (plotIdx + 1) % plotN;


            double[] orderedTimestamps = new double[plotN];
            double[] orderedAngles = new double[plotN];

            int n1 = plotN - plotIdx;
            Array.Copy(timestamps, plotIdx, orderedTimestamps, 0, n1);
            Array.Copy(angles, plotIdx, orderedAngles, 0, n1);

            if (plotIdx > 0)
            {
                Array.Copy(timestamps, 0, orderedTimestamps, n1, plotIdx);
                Array.Copy(angles, 0, orderedAngles, n1, plotIdx);
            }

            formsPlot_doa.Plot.Clear();

            var y0Main = formsPlot_doa.Plot.Add.HorizontalLine(90);
            y0Main.Color = new ScottPlot.Color(117, 117, 117);
            y0Main.LineWidth = 2.5f;

            doaScatter = formsPlot_doa.Plot.Add.Scatter(orderedTimestamps, orderedAngles);
            doaScatter.Color = ScottPlot.Colors.OrangeRed;
            doaScatter.MarkerSize = 0;
            doaScatter.LineWidth = 2;

            if (orderedTimestamps.Any(x => x > 100))
            {
                double xLimMin = timestamps.Where(x => x > 100).Min();
                double xLimMax = orderedTimestamps.Max();
                formsPlot_doa.Plot.Axes.SetLimitsX(xLimMin, xLimMax);
            }
            formsPlot_doa.Plot.Axes.SetLimitsY(0, 190);

            formsPlot_doa.Invoke((MethodInvoker)(() => formsPlot_doa.Refresh()));
        }


        /// <summary>
        /// Rysuje strzalke doa
        /// </summary>
        /// <param name="phi"> Kat strzalki</param>
        public void Draw(double phi)
        {
            var plt = formsPlot_locate.Plot;
            plt.Clear();

            double r = 200;
            double x2 = Math.Cos(phi) * r;
            double y2 = Math.Sin(phi) * r;
            var arrow = plt.Add.Arrow(0, 0, x2, y2);

            arrow.ArrowWidth = 4;
            arrow.ArrowLineWidth = 1;

            plt.Add.Marker(x12, y12, color: ScottPlot.Colors.Red, size: 10);
            plt.Add.Marker(0, 0, color: ScottPlot.Colors.Green, size: 10);
            plt.Add.Marker(x13, y13, color: ScottPlot.Colors.Blue, size: 10);


            plt.Axes.SetLimits(-300, 300, -300, 300);
            formsPlot_locate.Invoke((MethodInvoker)(() => formsPlot_locate.Refresh()));
        }





        /// <summary>
        /// Lokalizacja zrodla dzwieku w dalekim polu na podstawie TDoA
        /// </summary>
        /// <param name="snap"></param>
        /// <remark> kiedys tdoa</remark>
        public void doa2(ref List<KeyValuePair<AudioChunkChannel, AudioRecord>> snap)
        {
            AudioRecord reci12 = null;
            AudioRecord reci13 = null;
            for (int i = 0; i < snap.Count; i++)
            {
                var (chani, reci) = (snap[i].Key, snap[i].Value);   // kanal do aktualizaji
                if (reci.id == 11) continue; // pomin mikrofon referencyjny
                else if (reci.id == 12) reci12 = reci;
                else if (reci.id == 13) reci13 = reci;
            }
            const double c_mm_per_s = 343.0; // predkosc dzwieku mm/ms
            double dt12_s = reci12.lastCorr;
            double dt13_s = reci13.lastCorr;

            // Konwersja opoznien czasowych na odleglosci
            double Theta12 = c_mm_per_s * dt12_s;
            double Theta13 = c_mm_per_s * dt13_s;

            // poczatkowe przyblizenie polozenia zrodlai
            double x_s = 0.0;
            double y_s = 100.0;

            //double lr = 0.001;  // predkosc uczenia
            double lr = 0.003;  // predkosc uczenia
            //const int k_max = 500;
            const int k_max = 1000;
            const double eps = 0.000001;

            // Petla spadku gradientowego
            for (int k = 0; k < k_max; k++)
            {
                // Odleglosci do mikrofonow
                double r11 = Math.Sqrt(Math.Pow(x_s - x11, 2) + Math.Pow(y_s - y11, 2));
                double r12 = Math.Sqrt(Math.Pow(x_s - x12, 2) + Math.Pow(y_s - y12, 2));
                double r13 = Math.Sqrt(Math.Pow(x_s - x13, 2) + Math.Pow(y_s - y13, 2));

                // Roznica odleglosci pomiedzy przewidzianym a zmierzonym
                double diff12 = (r12 - r11) - Theta12;
                double diff13 = (r13 - r11) - Theta13;

                // Funkcja bledu do zminimalizowania (suma kwadratow roznic)
                double E = Math.Pow(diff12, 2) + Math.Pow(diff13, 2);

                // Oblicz pochodne czesciowe (gradient numeryczny)
                double h = 0.001; // krok
                double dEx = (Error(x_s + h, y_s) - Error(x_s - h, y_s)) / (2 * h);
                double dEy = (Error(x_s, y_s + h) - Error(x_s, y_s - h)) / (2 * h);

                // krok w kierunku przeciwnym do gradientu
                x_s -= lr * dEx;
                y_s -= lr * dEy;

                // Zatrzymaj, jesli blad jest wystarczajaco maly
                if (Math.Sqrt(dEx * dEx + dEy * dEy) < eps) break;

                double Error(double x, double y)
                {
                    double r11_ = Math.Sqrt(Math.Pow(x - x11, 2) + Math.Pow(y - y11, 2));
                    double r12_ = Math.Sqrt(Math.Pow(x - x12, 2) + Math.Pow(y - y12, 2));
                    double r13_ = Math.Sqrt(Math.Pow(x - x13, 2) + Math.Pow(y - y13, 2));

                    double d12_ = (r12_ - r11_) - Theta12;
                    double d13_ = (r13_ - r11_) - Theta13;
                    return Math.Pow(d12_, 2) + Math.Pow(d13_, 2);
                }
            }
            plotTDoA(x_s, y_s);
        }


        /// <summary>
        ///  Usuwa stare punkty historii TDoA
        /// </summary>
        public void resetTDoAHistory()
        {
            Logger.I(tag, "Resetting TDoA history");
            oldsIter = 0;
            for (int i = 0; i < oldsN; i++)
            {
                olds_x[i] = 0.0;
                olds_y[i] = 0.0;
            }
        }

        /// <summary>
        /// Rysuje punkty na mapie doa2
        /// </summary>
        /// <param name="x_s"></param>
        /// <param name="y_s"></param>
        private void plotTDoA(double x_s, double y_s)
        {
            var plt = formsPlot_TDoA.Plot;
            plt.Clear();

            updateOldSPointd(x_s, y_s);

            int n = Math.Min(oldsIter == 0 ? oldsN : oldsIter, oldsN);
            double[] xs = new double[n];
            double[] ys = new double[n];
            for (int i = 0; i < n; i++)
            {
                int idx = (oldsIter - n + i + oldsN) % oldsN;
                xs[i] = olds_x[idx];
                ys[i] = olds_y[idx];
            }

            for (int i = 0; i < n; i++)
            {
                double age01 = (double)i / Math.Max(1, n - 1);
                int size = 4 + (int)(8 * age01);

                double shade = 1.0 - age01;

                byte r = (byte)(180 * shade + 70);
                byte g = (byte)(100 * shade + 20);
                byte b = (byte)(200 * shade + 55);
                ScottPlot.Color col = new ScottPlot.Color(r, g, b);

                plt.Add.Marker(xs[i], ys[i], color: col, size: size, shape: MarkerShape.FilledCircle);
            }




            plt.Add.Marker(x11, y11, color: ScottPlot.Colors.Green, size: 20);
            plt.Add.Marker(x12, y12, color: ScottPlot.Colors.Red, size: 20);
            plt.Add.Marker(x13, y13, color: ScottPlot.Colors.Blue, size: 20);
            plt.Add.Marker(x_s, y_s, color: ScottPlot.Colors.Purple, size: 35, shape: MarkerShape.Eks);

            var addText = plt.Add.Text($"Source: \nx={x_s:F1} mm \ny={y_s:F1} mm", -470, 470);
            addText.LabelFontSize = 16;
            addText.LabelBold = true;
            addText.LabelBorderWidth = 1;
            addText.LabelBorderColor = ScottPlot.Colors.Black;


            plt.Axes.SetLimits(-1000, 1000, -1000, 1000);
            formsPlot_TDoA.Invoke((MethodInvoker)(() => formsPlot_TDoA.Refresh()));
        }


        /// <summary>
        /// Zmienia historie polozen zrodel
        /// </summary>
        /// <param name="x_s"></param>
        /// <param name="y_s"></param>
        /// <remarks> uzywane przez doa2, stare punkty sa jasniejsze</remarks>
        private void updateOldSPointd(double x_s, double y_s)
        {
            double lastS_x = olds_x[oldsIter];
            double lastS_y = olds_y[oldsIter];

            double dist = Math.Sqrt(Math.Pow(x_s - lastS_x, 2) + Math.Pow(y_s - lastS_y, 2));

            if (dist > 50)
            {
                olds_x[oldsIter] = x_s;
                olds_y[oldsIter] = y_s;
                oldsIter++;
            }

            if (oldsIter > oldsN)
            {
                oldsIter = 0;
            }
        }
    }
}




