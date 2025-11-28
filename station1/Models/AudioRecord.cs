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
    /// <summary>
    /// Klasa przechowujaca dane audio z jednego klienta
    /// </summary>
    /// remarks> dziala rezem z AudioChunkChannel. Wykorzystywana w przetwarzaniu danych audio </remarks>
    internal class AudioRecord
    {
        public double lastCorr = 0.0; // ostatnie zarejestrowane opoznienie
        public long seq = -1;           // numer sekwencyjny ostatniego pakietu
        private double lastSmoothedShift = 0; // uzywane w filtracji
        private bool shiftsFull = false;
        public bool isFirstChannel = false;
        public double[] correlations;   // surowe dane opoznien
        public double[] timeStamps;     // znaczniki czasu dla opoznien

        public double[] shiftsAvg;      // przefiltrowane dane opoznien

        private int corrIdx = 0; // indeks do zapisu w tablicach opoznien


        private string tag; //do logowania

        public int id = -1;
        private double offsetY = 0.0;
        public double[] X;  // Wektor z pakietow
        public double[] Y;  // Wektor czasow z pakietow
        private int chunkIdx = 0; // indeks do zapisu w wektorach X i Y
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


        /// <summary>
        /// Getter ostatniej wartosci opoznienia do wyznaczania DoA
        /// </summary>
        /// <param name="lastCorr"></param>
        public void getCorrForDoa(double lastCorr)
        {
            if (!double.IsNaN(lastCorr))
            {
                this.lastCorr = lastCorr;
            }

        }

        /// <summary>
        /// Dodaj chunk probek do wektorow X i Y
        /// </summary>
        /// <param name="xs"></param>
        /// <param name="ys"></param>
        /// <exception cref="InvalidOperationException"></exception>
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


        /// <summary>
        /// Usun stala skladowa z sygnalu audio
        /// </summary>
        public void cutOffOffset()
        {
            double avgY = Y.Average();
            offsetY += Y.Average();
        }

        /// <summary>
        /// Funkcja przetwarzajaca dane z kolejki probek klienta i dodajaca je do wektorow X i Y
        /// </summary>
        /// <param name="clientChannel"></param>
        /// <param name="serverNowMs"></param>
        /// <remarks> Tutaj zachodzi synchronizacja dokladna i kompensacja dryfu</remarks>
        public void appendData(AudioChunkChannel clientChannel, double serverNowMs)
        {
            while (clientChannel.sampleQueue.TryDequeue(out AudioChunk samples)) // Wyciagnij nastepny chunk probek
            {
                if (samples.length != Globals.SamplesPerChunk)
                {
                    Logger.E(tag, $"Unexpected samples length: got {samples.length}, expected {Globals.SamplesPerChunk}");
                    continue;
                }

                double packetStart_ms = ((double)samples.timestamp / 1000.0); // Odczytaj znak czassu

                double start_ms = packetStart_ms + clientChannel.offsetEndMs; // Zastosuj offset endMs do znaku czasu pakietu
                // Jezeli przeprowadzono dokladna synchronizacje zrezygnuj z offsetu endMs i znaku czasu pakietu
                // zamiast tego uzyj skumulowanego czasu koncowego
                if (clientChannel.isExactSynchDone)
                    start_ms = (double)clientChannel.accEndMs; //nowy poczatek pakietu

                // wygeneruj punkty czasu dla probek w pakiecie na podstawie poczatku pakietu i czestotliwosci probkowania
                double dt_ms = 1 / (double)Globals.SamplingRate * 1000.0;
                double stop_ms = start_ms + (dt_ms * samples.length);

                double[] xs = Enumerable.Range(0, samples.length)
                    .Select(i => start_ms + i * (dt_ms + clientChannel.offsetFreq))
                    .ToArray();

                double[] ys = samples.samples.Select(s => (double)s - offsetY).ToArray();
                appendChunk(xs, ys);
                double chunkDurationMs = (dt_ms * samples.length);


                // Obsluga numerow sekwencyjnych pakietow
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
                            chunkDurationMs *= (seqDiff + 1); // Powieksz czas trwania o zgubione pakiety
                        }
                        else
                        {
                            // Pakiet przyszedl z opoznieniem lub jest duplikatem
                            Logger.W(tag, $"Out-of-order or duplicate packet. Expected {expectedSeq}, got {samples.seq}");
                            seqDiff = 0;
                        }
                    }

                    seq = samples.seq; // Zaktualizuj oczekiwany numer sekwencyjny
                }
                // Kompensacja dryfu
                double compensationOffset = clientChannel.compensateDrift(serverNowMs);
                //compensationOffset = 0.0; // tymczasowo wylaczone
                // Aktualizacja skumulowanego czasu koncowego
                clientChannel.accEndMs = start_ms + chunkDurationMs - compensationOffset;

#if false
                // uzywane tylko podczas eksperymentow
                if (tag == "AudioRecord 13")
                {
                    double diff = (double)((double)packetStart_ms - clientChannel.accEndMs);
                    Logger.I(tag, $"serverTime: {serverNowMs}, tiemstamp: {packetStart_ms}, accEndMs: {clientChannel.accEndMs}, diff = {diff}");
                }
#endif
            }
        }

        /// <summary>
        /// Wykrywa odcinek prosty. Jak tak to buduje okno pomiarowe
        /// </summary>
        /// <remarks> uzywane przy wyznaczaniu i zmianie czestotliwosci </remarks>
        private const double stepThresholdUs = 0.05; // 50 us
        private const int ignoreNew = 3;             // Nie uzywaj nowych punktow do detekcji kroku
        private const int minPoints = 20;             // ilosc punktow minimalna do analizy
        private bool TryBuildPreStepWindow(int M, out List<double> x_win, out List<double> y_win)
        {
            x_win = new();
            y_win = new();

            int idx = (corrIdx - 1 - ignoreNew + Globals.MaxPlotHist) % Globals.MaxPlotHist;

            double? prevY_val = null;
            while (x_win.Count < M)
            {
                double x = timeStamps[idx];
                double y = shiftsAvg[idx];
                if (x <= 0) break;

                if (prevY_val.HasValue && Math.Abs(y - prevY_val.Value) > stepThresholdUs)
                    break; // wykryto krok (funkcja nie jest prosta), przerwij

                x_win.Add(x); y_win.Add(y);
                prevY_val = y;
                idx = (idx - 1 + Globals.MaxPlotHist) % Globals.MaxPlotHist;
            }

            // odwroc kolejnosc
            x_win.Reverse(); y_win.Reverse();
            if (x_win.Count < minPoints)
                return false; // za malo punktow
            else
                return true;
        }



        /* zmienne uzywante tylko przez funkcje nudgeFreq */
        private double lastServoApplyTsMs = -1; // czas od ostatniego zastosowania korekty
        private const double cooldownTime = 5_000; // czas od ostatniej korekty (us)
        private double integPpm = 0;  // czlon calkujacy

        /// <summary>
        /// Zmiana czestotliwosci klienta na podstawie wykrytego nachylenia i fazy
        /// </summary>
        /// <param name="chani"></param>
        /// remarks> Nie dziala </remarks>
        public void nudgeFreq(ref AudioChunkChannel chani)
        {
            const int M = 30;
            const double Kp = 0.3;      // Nastawa proporcjonalna
            const double Ki = 0.003;    // Nastawa calkujaca
            const double maxPPM = 15000;
            const double deadbandSlopePPM = 2.0;   // strefa martwa dla nachylenia (2 ppm)
            const double deadbandPhaseMs = 0.02;  // i na faze (20 us)

            // poczekaj az minie dosc czasu od ostatniej korekty
            int lastIdx = (corrIdx - 1 + Globals.MaxPlotHist) % Globals.MaxPlotHist;
            double nowMs = timeStamps[lastIdx];
            if (nowMs <= 0 || (lastServoApplyTsMs > 0 && (nowMs - lastServoApplyTsMs) < cooldownTime))
                return;

            if (!TryBuildPreStepWindow(M, out var xs, out var ys))
                return;

            // wyznacz regresie liniowa prostego odcinka
            int N = xs.Count;
            double sumX = 0, sumY = 0, sumXX = 0, sumXY = 0;
            for (int i = 0; i < N; i++)
            {
                sumX += xs[i];
                sumY += ys[i];
                sumXX += xs[i] * xs[i];
                sumXY += xs[i] * ys[i];
            }
            double denom = N * sumXX - sumX * sumX;
            if (Math.Abs(denom) < 0.000000001) return;

            double slope = (N * sumXY - sumX * sumY) / denom; // nachylenie odcinka prostego (= ppm/ms)
            double slopePpm = slope * 1_000_000;                    // ppm

            if (Math.Abs(slopePpm) < deadbandSlopePPM) slopePpm = 0;

            double phaseMs = ys[^1];
            double phaseEff = Math.Abs(phaseMs) < deadbandPhaseMs ? 0 : phaseMs;

            // aktualizuj czlon calkujacy
            integPpm += Ki * phaseEff;
            integPpm = Math.Max(-maxPPM, Math.Min(maxPPM, integPpm));

            // zmiana nachylenia
            double changePPM = -(Kp * slopePpm) - integPpm;
            changePPM = Math.Max(-maxPPM, Math.Min(maxPPM, changePPM));

            double corr = 1.0 - (changePPM / 1_000_000);
            chani.Freq *= corr;

            lastServoApplyTsMs = nowMs;
            Logger.I(tag, $"Zmana nachylenia: slope={slopePpm:F1} ppm phase={phaseMs:F4} ms cmd={changePPM:F1} ppm Freq={chani.Freq:F6} Hz");
        }




        /// <summary>
        /// Zapisuje aktualne wartosci opoznienia w wektorze korelacji
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <param name="corr"></param>
        public void rememberCorrelation(double timeStamp, double corr)
        {
            if (timeStamp < 100.0) return;

            correlations[corrIdx] = corr;
            timeStamps[corrIdx] = timeStamp;

            double filtered;
            if (corrIdx >= Globals.Navg)
                filtered = medianShifts(correlations, corrIdx - Globals.Navg, Globals.Navg);
            else
                filtered = corr;


            lastSmoothedShift = smoothShift(lastSmoothedShift, filtered);
            shiftsAvg[corrIdx] = lastSmoothedShift;


            corrIdx++;
            if (corrIdx >= Globals.MaxPlotHist) corrIdx = 0;

            getCorrForDoa(lastSmoothedShift);
        }

        /// <summary>
        /// Filtr medianowy do wyznaczania opoznien
        /// </summary>
        /// <param name="shifts"></param>
        /// <param name="startIdx"></param>
        /// <param name="dataPoints"></param>
        /// <returns></returns>
        private double medianShifts(double[] shifts, int startIdx, int dataPoints)
        {
            if (shifts.Length < dataPoints || startIdx + dataPoints > shifts.Length)
                return double.NaN; //niepoprawne dane
            var window = shifts.Skip(startIdx).Take(dataPoints).OrderBy(v => v).ToArray(); //okno obliczeniowe
            int mid = dataPoints / 2;
            if (dataPoints % 2 == 0)
                return (window[mid - 1] + window[mid]) / 2.0;
            else return window[mid];
        }

        /// <summary>
        /// Filtr wyrownujacy do wyznaczania opoznien
        /// </summary>
        /// <param name="prev"></param>
        /// <param name="current"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        private double smoothShift(double prev, double current, double alpha = 0.1)
        {
            return alpha * current + (1 - alpha) * prev;
        }


        /// <summary>
        /// Usun staredane
        /// </summary>
        /// <remarks> Uzywane przy restarcie nagrywnia </remarks>
        public void clearCorrHist()
        {
            Array.Clear(correlations, 0, correlations.Length);
            Array.Clear(timeStamps, 0, timeStamps.Length);

            Array.Clear(shiftsAvg, 0, shiftsAvg.Length);

            corrIdx = 0;
        }

        /// <summary>
        /// Przygotowuje dane do wykresow
        /// </summary>
        /// <param name="formsPlotTimeShiftsRef"></param>
        /// <param name="formsPlotRef"></param>
        public void prepareData(ref FormsPlot formsPlotTimeShiftsRef, ref FormsPlot formsPlotRef)
        {
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

            var scatter = formsPlotRef.Plot.Add.Scatter(this.X, this.Y);
            scatter.LegendText = $"Client {this.id}";

            var scatter2 = formsPlotTimeShiftsRef.Plot.Add.Scatter(this.timeStamps, this.correlations);
            scatter2.LineWidth = 0;
            scatter2.LegendText = $"dT {this.id}";

            var scatter3 = formsPlotTimeShiftsRef.Plot.Add.Scatter(orderedTimestamps, orderedShiftsAvg);
            scatter3.MarkerSize = 0;
            scatter3.LineWidth = 2.5f;

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

    }
}
