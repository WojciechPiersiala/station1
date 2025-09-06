using Microsoft.VisualBasic.Logging;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace station1.Models
{
    internal class TcpServer
    {
        private Logger log;
        private TcpListener server;
        public List<ClientChannel> connectedClients { get; } = new(); // handle multiple clients
            //= new ConcurrentDictionary<TcpClient, ConcurrentQueue<AudioData>>();
        //private ConcurrentQueue<AudioData> sampleQueue;
        private const int headerLen = 8;
        private const bool isLogSamples = false;
        private const bool isLogAudioInfo = false;

        // default constructor
        public TcpServer(Logger log)
        {
            this.log = log;
            //this.sampleQueue = sampleQueue;
            printIp();
        }


        private void printIp()
        {
            string hostName = Dns.GetHostName();
            Console.WriteLine($"Host Name: {hostName}");
            IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);
            foreach (IPAddress ip in ipAddresses)
            {
                log.Log_I($"IP Address: {ip}");
            }

        }

        public async Task ListenTcp(CancellationToken clcTok)
        {
            server = null; 
            try
            {
                /* start server */
                var ipEndPoint = new IPEndPoint(IPAddress.Any, 5050);
                server = new TcpListener(ipEndPoint);
                log.Log_I("Listener starting ...");
                server.Start();
                log.Log_I($"Listener started: {ipEndPoint.Address}");

                while (!clcTok.IsCancellationRequested)
                {
                    log.Log_I("Waiting for connection ...");
                    TcpClient newTcpClient = await server.AcceptTcpClientAsync(clcTok);
                    log.Log_I("Connected!");
                    //var newSampleQueue = new ConcurrentQueue<AudioData>();
                    var clientChannel = new ClientChannel(connectedClients.Count + 1, newTcpClient);
                    connectedClients.Add(clientChannel);

                    string clientIp = ((IPEndPoint)clientChannel.tcpClient.Client.RemoteEndPoint).Address.ToString();
                    log.Log_I($"Client with id: {clientChannel.id} and ip: {clientIp} added to the queue. Number of connected clients: {connectedClients.Count}");

                    _ = Task.Run(() => HandleClient(clientChannel, clcTok));
                } // server while loop
            }
            catch (ObjectDisposedException)
            {
                log.Log_W("Server stopped while waiting for a client. Socket was closed.");
            }
            catch (Exception e)
            {
                log.Log_E($"Server exception : {e.Message}");
            }
            finally
            {
                server?.Stop();
            }
        }

        public void StopTcp()
        {
            foreach (var cc in connectedClients)
            {
                TcpClient currentClient = cc.tcpClient;
                NetworkStream stream = currentClient.GetStream();
                try
                {
                    stream?.Close();
                    stream?.Dispose();
                    currentClient?.Close();
                    currentClient?.Dispose();
                    log.Log_I("Server stopped");
                }
                catch (Exception e)
                {
                    log.Log_E($"Error while stopping the tcp server: {e.Message}");
                }
                server?.Stop();
                log.Log_I("Server stopped");
            }
        }


        public async Task HandleClient(ClientChannel clientChannel, CancellationToken clcTok)
        {
            while (!clcTok.IsCancellationRequested)
            {
                NetworkStream stream = clientChannel.tcpClient.GetStream();

                /* Read header */
                byte[] headerBytes = new byte[headerLen];
                int headerRead = 0;
                while (headerRead < headerLen)
                {
                    int read = stream.Read(headerBytes, headerRead, headerLen - headerRead);
                    if (read == 0) throw new IOException("Connection closed before header received");
                    headerRead += read;
                }
                int timestamp = BitConverter.ToInt32(headerBytes, 1);
                char messageTypeChar = (char)headerBytes[0];


                if (messageTypeChar == 'A') //Audio
                {
                    if (isLogAudioInfo)
                    {
                        log.Log_I($"{messageTypeChar}: {timestamp}");
                    }


                    /* Read audio samples */
                    int audioLen = 2048; // audio data length
                    byte[] audioBytes = new byte[audioLen];
                    int audioRead = 0;
                    AudioData samples = new AudioData(timestamp);
                    while (audioRead < audioLen)
                    {
                        int read = stream.Read(audioBytes, audioRead, audioLen - audioRead);
                        if (read == 0) throw new IOException("Connection closed before audio received");
                        audioRead += read;
                    }

                    for (int n = 0; n < samples.length; n++)
                    {
                        samples.samples[n] = BitConverter.ToInt16(audioBytes, n * 2);
                        if (isLogSamples)
                            Console.Write($"{samples.samples[n]} ");
                    }
                    clientChannel.sampleQueue.Enqueue(samples);
                }
            } // connection while loop
        }
    }
}
