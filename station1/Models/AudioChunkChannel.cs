using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace station1.Models
{
    internal class AudioChunkChannel
    {
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
    }
}