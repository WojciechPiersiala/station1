using System;
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
        private static string tag = "ClientChannel";
        public int audioLength = 0; // length of audio data in bytes
        private Stopwatch runtimeWatch = Stopwatch.StartNew();
        public double recentTimestampMs = 0.0; // in ms client current timestamp of the last received packet
        public double? offsetMs = null; // in ms client offset
        public bool synchronise = false;
        public int id { get; }
        public TcpClient tcpClient { get; }
        public ConcurrentQueue<AudioChunk> sampleQueue { get; } = new();
        public CancellationTokenSource clcTokenSrc;
        public long lastReadTime;
        public AudioChunkChannel(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
            string clientIp = ((IPEndPoint)this.tcpClient.Client.RemoteEndPoint).Address.ToString();
            string lastOctet = clientIp.Split('.').Last();
            this.id = int.Parse(lastOctet);
            Logger.I(tag, $"Created client with id: {this.id}");

        }
    }
}