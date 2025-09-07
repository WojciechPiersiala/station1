using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace station1.Models
{
    internal class ClientChannel
    {
        private Stopwatch runtimeWatch = Stopwatch.StartNew();
        public double recentTimestampMs = 0.0; // in ms client current timestamp of the last received packet
        public double? offsetMs = null; // in ms client offset
        public bool synchronise = false;
        public int id { get; }
        public TcpClient tcpClient { get; }
        public ConcurrentQueue<AudioData> sampleQueue { get; } = new();
        public  CancellationTokenSource clcTokenSrc;
        public long lastReadTime;
        public ClientChannel(int id, TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
            this.id = id;
        }
    }
}
