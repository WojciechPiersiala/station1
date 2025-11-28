using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace station1.Models
{
    /// <summary>
    /// Struktura z danymi do kompensacji dryfu
    /// </summary>
    public struct compPattern
    {
        public double stepCorr;
        public double stepTime;
        public double lastCompTime;

        public compPattern(double stepCorr, double stepTime)
        {

            this.stepCorr = stepCorr;
            this.stepTime = stepTime;
            this.lastCompTime = -1.0;
        }
    }

    /// <summary>
    /// Kanal reprezentujaczycy polaczenie z klientem audio
    /// </summary>
    internal class AudioChunkChannel
    {
        private compPattern compPattern;
        public bool isExactSynch = false;
        public bool isExactSynchDone = false;
        private static string tag = "ClientChannel";
        public int audioLength = 0; // dlugosc danych audio w bajtach
        private Stopwatch runtimeWatch = Stopwatch.StartNew();
        public double recentTimestampMs = 0.0; // Ostatni znak czasu

        public double offsetFreq = 0;
        public bool synchronise = false;
        public int id { get; }
        public TcpClient tcpClient { get; }
        public ConcurrentQueue<AudioChunk> sampleQueue { get; } = new();
        public CancellationTokenSource clcTokenSrc;
        public long lastReadTime;
        private Stopwatch stopWatch = Stopwatch.StartNew();

        public double Freq = 0.0;

        public double? accEndMs = null;
        public double offsetEndMs;

        /// <summary>
        /// Konstruktor kanalu. Bierze zainicjalizowanego TcpClienta
        /// </summary>
        /// <param name="tcpClient"></param>
        public AudioChunkChannel(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
            string clientIp = ((IPEndPoint)this.tcpClient.Client.RemoteEndPoint).Address.ToString();
            string lastOctet = clientIp.Split('.').Last();
            this.id = int.Parse(lastOctet);
            Logger.I(tag, $"Created client with id: {this.id}");


            // Ustawienia metody kompensacji dryfu
            double deltaCorr;
            double deltaTime;

            switch (id)
            {
                case 12:
                    {
                        deltaCorr = -204.0 / 1000;
                        deltaTime = 2 * 96245.000;
                        break;
                    }

                case 13:
                    {
                        deltaCorr = 185.0 / 1000;
                        deltaTime = 84587.000;
                        break;
                    }

                default: //mikrofon referencyjny
                    {
                        deltaCorr = 0.0;
                        deltaTime = 0.0;
                        break;
                    }
            }
            // logi
            Logger.I(tag, $"Using compensation pattern: dCorr={deltaCorr}, dTime={deltaTime}");
            this.compPattern = new compPattern(deltaCorr, deltaTime);
            tag = $"ClientChannel {this.id}";
        }

        /// <summary>
        /// Resetuje mechanizm kompensacji dryfu
        /// </summary>
        /// </remarks>  Uzyte przy ponownej synchronizacji </remarks>
        public void resetCompPatter()
        {
            compPattern.lastCompTime = -1.0;
        }

        /// <summary>
        /// Resetuje dane przy ponownej synchronizacji
        /// </summary>
        public void SynchRecord()
        {
            this.isExactSynch = false;
            this.isExactSynchDone = false;
            this.offsetEndMs = 0.0;
            this.accEndMs = null;
            long timestampUs = stopWatch.ElapsedMilliseconds * 1000; // us
            this.sendTimeStampTcp(timestampUs);

        }


        /// <summary>
        /// Manualna synchronizacja. Wysyla znacznik czasu do klienta przez TCP
        /// </summary>
        /// <param name="timestampUs"></param>
        /// <param name="header"></param>
        public void sendTimeStampTcp(long timestampUs, char header = 'M')
        {
            if (!this.tcpClient.Connected)
            {
                Logger.W(tag, $"Cannot send timestamp to client {this.id} because it is not connected");
                return;
            }

            NetworkStream stream = this.tcpClient.GetStream();

            byte[] headerBytes = new byte[9];
            headerBytes[0] = (byte)header;
            BinaryPrimitives.WriteInt64LittleEndian(headerBytes.AsSpan(1, 8), timestampUs);

            WriteAllAsync(stream, headerBytes, CancellationToken.None).Wait();
            Logger.I(tag, $"Manual synchronisation, timestamp {timestampUs} sent to client {this.id}");
        }

        /// <summary>
        /// Wysyla wszystkie dane przez strumien sieciowy
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"> Zazwyczaj header synchronizacji NTP</param>
        /// <param name="ct"></param>
        /// <returns> Uzywane przy synchronizacji NTP </returns>
        private static async Task WriteAllAsync(NetworkStream stream, ReadOnlyMemory<byte> buffer, CancellationToken ct)
        {
            int offset = 0;
            while (offset < buffer.Length)
            {
                int toSend = buffer.Length - offset;
                await stream.WriteAsync(buffer.Slice(offset, toSend), ct).ConfigureAwait(false);
                offset += toSend;
            }
        }

        /// <summary>
        /// Funkcja kompensujaca dryf czasu klienta
        /// </summary>
        /// <param name="serverNowMs"> Aktualny czas</param>
        /// <returns></returns>
        /// remarks> Co zadany czas aplikuje korekcje lagu </remarks>
        public double compensateDrift(double serverNowMs)
        {
            if (id == 11) return 0.0; // skip for testing

            double timeDiff = (compPattern.stepTime + compPattern.lastCompTime) - serverNowMs;
            if (timeDiff < 0.0) // init comp
            {
                Logger.I(tag, $"{serverNowMs}, Client {id}, compensation time has elapsed, Will aplay compensation of {compPattern.stepCorr}");
                compPattern.lastCompTime += compPattern.stepTime;
                return (compPattern.stepCorr);
            }
            else
            {
                return 0.0;
            }
        }
    }
}