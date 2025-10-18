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
        public double? accEndMs = null;
        public double Freq = Globals.SamplingRate;
        public double shiftCompStep = -1.0;
        public double shiftCompTime1 = -1.0;
        public double shiftCompTime2 = -1.0;
        public bool foundCompInterval = false;
        public double shiftCompInterval = -1.0;
        public bool synchronise = false;
        public int shiftComp_tryCount = -1;
        public int id { get; }
        public TcpClient tcpClient { get; }
        public ConcurrentQueue<AudioChunk> sampleQueue { get; } = new();
        public CancellationTokenSource clcTokenSrc;
        public long lastReadTimeSynch; // synchronisation time
        public double lastReadTimeComp;  // compensation time
        public AudioChunkChannel(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
            string clientIp = ((IPEndPoint)this.tcpClient.Client.RemoteEndPoint).Address.ToString();
            string lastOctet = clientIp.Split('.').Last();
            this.id = int.Parse(lastOctet);
            Logger.I(tag, $"Created client with id: {this.id}");

        }

        public void resetSynchData()
        {
            shiftCompStep = -1.0;
            shiftCompTime1 = -1.0;
            shiftCompTime2 = -1.0;
            shiftCompInterval = -1.0;
            foundCompInterval = false;
            lastReadTimeComp = 0.0;
            shiftComp_tryCount = -1;
        }

        public void compensateShift(double currentTime)
        {
            if (shiftCompInterval > 0 && currentTime - lastReadTimeComp >= shiftCompInterval)
            {
                offsetMs -= shiftCompStep;
                accEndMs -= shiftCompStep;
                lastReadTimeComp += lastReadTimeComp;
                Logger.I(tag, $"Client {id} compensated shift by {shiftCompStep:F4} ms, new offset: {offsetMs:F4} ms");
            }
        }
    }
}
