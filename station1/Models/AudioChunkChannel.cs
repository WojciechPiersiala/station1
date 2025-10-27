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
    public struct compPattern
    {
        public double stepCorr;
        public double stepTime;
        public double lastCompTime;

        public compPattern(double stepCorr, double stepTime)
        {
            //Logger.S("compPatter", $"compPatter created, stepCorr: {stepCorr}, stepTime: {stepTime}");
            this.stepCorr = stepCorr;
            this.stepTime = stepTime;
            this.lastCompTime = -1.0;
        }
    }

    internal class AudioChunkChannel
    {
        private compPattern compPattern;
        public bool isExactSynch = false;
        public bool isExactSynchDone = false;
        private static string tag = "ClientChannel";
        public int audioLength = 0; // length of audio data in bytes
        private Stopwatch runtimeWatch = Stopwatch.StartNew();
        public double recentTimestampMs = 0.0; // in ms client current timestamp of the last received packet
        //public double? offsetMs = null; // in ms client offset
        public double offsetFreq = 0;
        public bool synchronise = false;
        public int id { get; }
        public TcpClient tcpClient { get; }
        public ConcurrentQueue<AudioChunk> sampleQueue { get; } = new();
        public CancellationTokenSource clcTokenSrc;
        public long lastReadTime;
        private Stopwatch stopWatch = Stopwatch.StartNew();

        public double? accEndMs = null;
        public double offsetEndMs;
        public AudioChunkChannel(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
            string clientIp = ((IPEndPoint)this.tcpClient.Client.RemoteEndPoint).Address.ToString();
            string lastOctet = clientIp.Split('.').Last();
            this.id = int.Parse(lastOctet);
            Logger.I(tag, $"Created client with id: {this.id}");


            // todo: replace hardcoded 
            double deltaCorr;
            double deltaTime;

            switch (id)
            {
                case 12:
                    {
                        deltaCorr = -204.0 / 1000;
                        deltaTime = 2*96245.000;
                        break;
                    }

                case 13:
                    {
                        deltaCorr = 185.0 / 1000;
                        deltaTime = 84587.000;
                        break;
                    }

                default: // other id (reference mic)
                    {
                        deltaCorr = 0.0;
                        deltaTime = 0.0;
                        break;
                    }
            }
            Logger.I(tag, $"Using compensation pattern: dCorr={deltaCorr}, dTime={deltaTime}");
            this.compPattern = new compPattern(deltaCorr, deltaTime);
            tag = $"ClientChannel {this.id}";
        }

        public void resetCompPatter()
        {
            compPattern.lastCompTime = -1.0;
        }

        public void SynchRecord()
        {
            this.isExactSynch = false;
            this.isExactSynchDone = false;
            this.offsetEndMs = 0.0;
            this.accEndMs = null;
            long timestampUs = stopWatch.ElapsedMilliseconds * 1000; // in microseconds
            this.sendTimeStampTcp(timestampUs);
            
        }


        // manual sync
        public void sendTimeStampTcp(long timestampUs, char header = 'M') 
        {
            if(!this.tcpClient.Connected)
            {
                Logger.W(tag, $"Cannot send timestamp to client {this.id} because it is not connected");
                return;
            }
            //Logger.I(tag, $"sending data to client {this.id}");
            NetworkStream stream = this.tcpClient.GetStream();

            // header // 'S' for sync
            //long timestampUs = timer.ElapsedMilliseconds * 1000; // in microseconds

            byte[] headerBytes = new byte[9];
            headerBytes[0] = (byte)header;
            BinaryPrimitives.WriteInt64LittleEndian(headerBytes.AsSpan(1, 8), timestampUs);

            WriteAllAsync(stream, headerBytes, CancellationToken.None).Wait();
            Logger.I(tag, $"Manual synchronisation, timestamp {timestampUs} sent to client {this.id}");
        }

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