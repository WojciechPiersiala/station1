using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace station1.Models
{
    internal class ClientChannel
    {
        public int id { get; }
        public TcpClient tcpClient { get; }
        public ConcurrentQueue<AudioData> sampleQueue { get; } = new();
        public ClientChannel(int id, TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
            this.id = id;
        }
    }
}
