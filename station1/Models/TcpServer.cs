using Microsoft.VisualBasic.Logging;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
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
        //private int audioLen = 2048;
        private int audioLen;
        //private int audioLenSamples;
        private const double clientTimeout  = 10000.0; // client timeout in ms
        //private Logger log;
        Stopwatch timer; // main timer
        private TcpListener server;
        private string tag = "tcpServer";
        public List<ClientChannel> connectedClients { get; } = new(); // handle multiple clients
        private const int headerLen = 8;
        private const bool isLogSamples = false;
        private const bool isLogAudioInfo = false;
        public TcpServer(int audioLen)
        {
            this.audioLen = audioLen;
            //this.audioLenSamples = this.audioLen*2; // 2 bytes per sample (16 bit)
            timer = Stopwatch.StartNew();
            printIp();
        }


        private void printIp()
        {
            string hostName = Dns.GetHostName();
            Console.WriteLine($"Host Name: {hostName}");
            IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);
            foreach (IPAddress ip in ipAddresses)
            {
                Logger.I(tag,$"IP Address: {ip}");
            }

        }

        public async Task MonitorConnections(CancellationToken clcTok)
        {
            while (!clcTok.IsCancellationRequested)
            {
                foreach (var cc in connectedClients)
                {
                    double readTime = (timer.ElapsedMilliseconds - cc.lastReadTime);
                    if(readTime > clientTimeout)
                    {
                        Logger.E(tag, $"Timeout. Client with ID: {cc.id} didn't respond for more than {readTime}. Removing the client...");
                        cc.clcTokenSrc.Cancel();
                        TcpClient tcpClient = cc.tcpClient;

                        if (tcpClient?.Connected == true)
                        {
                            using (Stream stream = tcpClient.GetStream())
                            {
                                stream?.Close();
                                stream?.Dispose();
                            }
                        }

                        tcpClient?.Close();
                        tcpClient?.Dispose();
                        connectedClients.Remove(cc);
                        break; // break foreach to avoid collection modification error
                    }
                }
                await Task.Delay(1000, clcTok); //refresh rate
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
                Logger.I(tag,$"Listener starting ...");
                server.Start();
                Logger.I(tag,$"Listener started: {ipEndPoint.Address}");

                while (!clcTok.IsCancellationRequested)
                {
                    Logger.I(tag,$"Waiting for connection ...");
                    TcpClient newTcpClient = await server.AcceptTcpClientAsync(clcTok);
                    Logger.I(tag,$"Connected!");
                    var clientChannel = new ClientChannel(newTcpClient);
                    connectedClients.Add(clientChannel);

                    string clientIp = ((IPEndPoint)clientChannel.tcpClient.Client.RemoteEndPoint).Address.ToString();
                    Logger.I(tag,$"Client with id: {clientChannel.id} and ip: {clientIp} added to the queue. Number of connected clients: {connectedClients.Count}");
                    clientChannel.clcTokenSrc = new CancellationTokenSource();

                    _ = Task.Run(() => HandleClient(clientChannel, clientChannel.clcTokenSrc.Token));
                } // server while loop

            }
            catch (ObjectDisposedException)
            {
                Logger.W("Server stopped while waiting for a client. Socket was closed.");
            }
            catch (Exception e)
            {
                Logger.E($"Server exception : {e.Message}");
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
                    cc.clcTokenSrc.Cancel();
                    stream?.Close();
                    stream?.Dispose();
                    currentClient?.Close();
                    currentClient?.Dispose();
                    Logger.I(tag,$"Server stopped");
                }
                catch (Exception e)
                {
                    Logger.E($"Error while stopping the tcp server: {e.Message}");
                }
            }
            server?.Stop();
            Logger.I(tag,$"Server stopped");
            connectedClients.Clear();
        }


        public async Task HandleClient(ClientChannel clientChannel, CancellationToken clcTok)
        {
            Logger.I(tag, $"tcp client task with id {clientChannel.id} started");
            NetworkStream stream = clientChannel.tcpClient.GetStream();
            while (!clcTok.IsCancellationRequested)
            { 
                /* Read header */
                byte[] headerBytes = new byte[headerLen];
                int headerRead = 0;
                while (headerRead < headerLen)
                {
                    int read = stream.Read(headerBytes, headerRead, headerLen - headerRead);
                    clientChannel.lastReadTime = timer.ElapsedMilliseconds;
                    if (read == 0) throw new IOException("Connection closed before header received");
                    headerRead += read;
                }
                int timestamp = BitConverter.ToInt32(headerBytes, 1);
                char messageTypeChar = (char)headerBytes[0];


                if (messageTypeChar == 'A') //Audio
                {
                    if (isLogAudioInfo)
                    {
                        Logger.I(tag,$"{messageTypeChar}: {timestamp}");
                    }


                    /* Read audio samples */
                    //int audioLen = 2048; // audio data length
                    byte[] audioBytes = new byte[audioLen];
                    int audioRead = 0;
                    AudioData samples = new AudioData(timestamp, audioLen/2);
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
            Logger.I(tag, $"tcp client task with id {clientChannel.id} cancelled");
        }

    }// class
}// namespace
