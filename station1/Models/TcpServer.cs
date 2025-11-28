using Microsoft.VisualBasic.Logging;
using SkiaSharp;
using System;
using System.Buffers.Binary;
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
    /// <summary>
    /// Klasa serwera TCP odbierajacego dane audio od klientow
    /// </summary>
    internal class TcpServer
    {

        private const double clientTimeout = 10000.0;
        Stopwatch timer; // glowny timer
        private TcpListener server;
        private string tag = "tcpServer";
        public List<AudioChunkChannel> connectedClients { get; } = new();
        private const int headerLen = 17;
        private const bool isLogSamples = false;
        private const bool isLogAudioInfo = false;
        public TcpServer()
        {
            timer = Stopwatch.StartNew();
            printIp();
        }

        /// <summary>
        /// Loguje adresy IP urzadzenia
        /// </summary>
        private void printIp()
        {
            string hostName = Dns.GetHostName();
            Console.WriteLine($"Host Name: {hostName}");
            IPAddress[] ipAddresses = Dns.GetHostAddresses(hostName);
            foreach (IPAddress ip in ipAddresses)
            {
                Logger.I(tag, $"IP Address: {ip}");
            }

        }

        /// <summary>
        /// Watek monitorujacy polaczenia z klientami
        /// </summary>
        /// <param name="clcTok"></param>
        /// <returns></returns>
        public async Task MonitorConnections(CancellationToken clcTok)
        {
            while (!clcTok.IsCancellationRequested)
            {
                foreach (var cc in connectedClients)
                {
                    double readTime = (timer.ElapsedMilliseconds - cc.lastReadTime);
                    if (readTime > clientTimeout)
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
                        break;
                    }
                }
                await Task.Delay(1000, clcTok);
            }
        }


        /// <summary>
        /// Watek serwera TCP nasluchujacy na polaczenia od klientow
        /// </summary>
        /// <param name="clcTok"></param>
        /// <returns> Ciagle chodzi w tle. Jak polaczy sie nowy klient. To tworzy dla niego nowy watek HandleClient </returns>
        public async Task ListenTcp(CancellationToken clcTok)
        {
            server = null;
            try
            {
                var ipEndPoint = new IPEndPoint(IPAddress.Any, 5050);
                server = new TcpListener(ipEndPoint);
                Logger.I(tag, $"Listener starting ...");
                server.Start();
                Logger.I(tag, $"Listener started: {ipEndPoint.Address}");

                while (!clcTok.IsCancellationRequested)
                {
                    Logger.I(tag, $"Waiting for connection ...");
                    TcpClient newTcpClient = await server.AcceptTcpClientAsync(clcTok);
                    newTcpClient.NoDelay = true;
                    Logger.I(tag, $"Connected!");
                    var clientChannel = new AudioChunkChannel(newTcpClient);
                    connectedClients.Add(clientChannel);

                    string clientIp = ((IPEndPoint)clientChannel.tcpClient.Client.RemoteEndPoint).Address.ToString();
                    Logger.I(tag, $"Client with id: {clientChannel.id} and ip: {clientIp} added to the queue. Number of connected clients: {connectedClients.Count}");
                    clientChannel.clcTokenSrc = new CancellationTokenSource();

                    _ = Task.Run(() => HandleClient(clientChannel, clientChannel.clcTokenSrc.Token));
                } // petla while

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


        /// <summary>
        /// Zatrzymaj wszystko
        /// </summary>
        /// remarks>Polaczone z przyciskiem </remarks>
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
                    Logger.I(tag, $"Server stopped");
                }
                catch (Exception e)
                {
                    Logger.E($"Error while stopping the tcp server: {e.Message}");
                }
            }
            server?.Stop();
            Logger.I(tag, $"Server stopped");
            connectedClients.Clear();
        }


        /// <summary>
        /// Obsluga klienta TCP
        /// </summary>
        /// <param name="clientChannel"></param>
        /// <param name="clcTok"></param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        public async Task HandleClient(AudioChunkChannel clientChannel, CancellationToken clcTok)
        {
            Logger.I(tag, $"tcp client task with id {clientChannel.id} started");
            NetworkStream stream = clientChannel.tcpClient.GetStream();

            while (!clcTok.IsCancellationRequested)
            {
                byte[] headerBytes = new byte[headerLen];
                int headerRead = 0;
                while (headerRead < headerLen)
                {
                    int read = stream.Read(headerBytes, headerRead, headerLen - headerRead);
                    clientChannel.lastReadTime = timer.ElapsedMilliseconds;
                    if (read == 0) throw new IOException("Connection closed before header received");
                    headerRead += read;
                }


                long timestampUs = BinaryPrimitives.ReadInt64LittleEndian(headerBytes.AsSpan(1, 8));
                long seq = BinaryPrimitives.ReadInt64LittleEndian(headerBytes.AsSpan(9, 8));
                char messageTypeChar = (char)headerBytes[0];


                switch (messageTypeChar) //Audio
                {
                    case 'A': // Audio data
                        if (isLogAudioInfo)
                        {
                            Logger.I(tag, $"{messageTypeChar}: {timestampUs}");
                        }

                        byte[] audioBytes = new byte[Globals.AudioLen];
                        int audioRead = 0;

                        AudioChunk samples = new AudioChunk(timestampUs, Globals.AudioLen / 2, seq);
                        while (audioRead < Globals.AudioLen)
                        {
                            int read = stream.Read(audioBytes, audioRead, Globals.AudioLen - audioRead);
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
                        break;



                    case 'Q': //synchronizacja ntp
                        {
                            long t1 = BinaryPrimitives.ReadInt64LittleEndian(headerBytes.AsSpan(1, 8));
                            var freq = (double)Stopwatch.Frequency;
                            long t2 = (long)(timer.ElapsedTicks * 1_000_000.0 / freq);


                            var reply = new byte[1 + 8 * 3];
                            reply[0] = (byte)'R';
                            BinaryPrimitives.WriteInt64LittleEndian(reply.AsSpan(1, 8), t1);
                            BinaryPrimitives.WriteInt64LittleEndian(reply.AsSpan(9, 8), t2);

                            long t3 = (long)(timer.ElapsedTicks * 1_000_000.0 / freq);
                            BinaryPrimitives.WriteInt64LittleEndian(reply.AsSpan(17, 8), t3);

                            try
                            {
                                await stream.WriteAsync(reply).ConfigureAwait(false);
                                if (false) Logger.I(tag, $"Replied 'R' to client {clientChannel.id}: t1={t1} t2={t2} t3={t3}");
                            }
                            catch (Exception ex)
                            {
                                Logger.E(tag, $"Write to client {clientChannel.id} failed: {ex.Message}");
                                throw;
                            }
                            break;
                        }
                }

            } // connection while loop
            Logger.I(tag, $"tcp client task with id {clientChannel.id} cancelled");
        }

    }// class
}// namespace