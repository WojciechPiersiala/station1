using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic.Logging;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO;
using System.Linq.Expressions;
using System.Collections.Concurrent;

namespace station1.Models
{
    internal class TcpServer
    {
        private Logger log;
        private TcpListener server;
        private NetworkStream stream;
        private TcpClient currentClient;
        private ConcurrentQueue<AudioData> sampleQueue;
        private const int headerLen = 8;
        private const bool isLogSamples = false;
        private const bool isLogAudioInfo = true;
        public TcpServer(Logger log, ConcurrentQueue<AudioData> sampleQueue)
        {
            this.log = log;
            this.sampleQueue = sampleQueue;
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
                var ipEndPoint = new IPEndPoint(IPAddress.Any, 5050);
                server = new TcpListener(ipEndPoint);
                log.Log_I("Listener starting ...");
                server.Start();
                log.Log_I($"Listener started: {ipEndPoint.Address}");
                //byte[] bytes = new byte[256];
                //string data = null;

                //byte[] audioBytes = new byte[2048];  // match ESP32 sends

                while (!clcTok.IsCancellationRequested)
                {
                    log.Log_I("Waiting for connection ...");
                    currentClient = await server.AcceptTcpClientAsync(clcTok);
                    log.Log_I("Connected!");

                    //MessageType messageType = MessageType.Audio;

                    /* Recieved audio data */
                    //if(messageType == MessageType.Audio)
                    while(true)
                    {
                        stream = currentClient.GetStream();

                        /* Read header */
                        byte[] headerBytes = new byte[headerLen];
                        int headerRead = 0;
                        while (headerRead < headerLen)
                        {
                            int read = stream.Read(headerBytes, headerRead, headerLen - headerRead);
                            if (read == 0) throw new IOException("Connection closed before header received");
                            headerRead += read;
                        }
                        //string headerStr = Encoding.ASCII.GetString(headerBytes[0]);
                        //string headerType = Encoding.ASCII.GetString(new byte[] { headerBytes[0] });
                        int timestamp = BitConverter.ToInt32(headerBytes, 1);
                        char messageTypeChar = (char)headerBytes[0];


                        //if (MessageTypeMap.TryGetValue(messageTypeChar, out string messageType))
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
                            //short[] samples = new short[audioLen / 2];
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
                            sampleQueue.Enqueue(samples);
                        }
                    }
                    log.Log_W("Connection stopped");
                }
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

        public void SendTcp(string data= "testdata")
        {
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

            // Send back a response.
            try 
            {
                if (server != null && stream != null && stream.CanWrite)
                {
                    try
                    {
                        stream.Write(msg, 0, msg.Length);
                        log.Log_I($"Message sent: \"{data}\"");
                    }
                    catch (Exception e)
                    {
                        log.Log_E(e.Message);
                    }

                }
                else
                {
                    log.Log_W("Cannot send server null or closed");
                }
            }
            catch (Exception e)
            {
                log.Log_E($"Error while sending a response: \"{e.Message}\"");
            }
        }
    }
}
