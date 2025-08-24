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
        private ConcurrentQueue<short[]> sampleQueue;
        public TcpServer(Logger log, ConcurrentQueue<short[]> sampleQueue)
        {
            this.log = log;
            this.sampleQueue = sampleQueue;
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

                byte[] buffer = new byte[2048];  // match ESP32 sends

                while (!clcTok.IsCancellationRequested)
                {
                    log.Log_I("Waiting for connection ...");
                    currentClient = await server.AcceptTcpClientAsync(clcTok);
                    log.Log_I("Connected!");

                    //data = null;
                    stream = currentClient.GetStream();
                    //int i = 0;

                    //// Loop to receive all the data sent by the client.
                    //while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    //{
                    //    data = Encoding.ASCII.GetString(bytes, 0, i);
                    //    log.Log_I($"Received: {data}");
                    //    Console.WriteLine($"Received: {data} \n\n");
                    //}
                    int bytesRead;
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        short[] samples = new short[bytesRead / 2];
                        for (int n = 0; n < samples.Length; n++)
                        {
                            samples[n] = BitConverter.ToInt16(buffer, n * 2);
                            //Console.Write($"{samples[n]} ");
                        }
                        //Console.WriteLine("\n\n");
                        sampleQueue.Enqueue(samples);
                    }
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

        public void SendTcp(string data= "Test \"Response from the laptop\"")
        {
            byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

            // Send back a response.
            try 
            {
                if (server != null && stream != null && stream.CanWrite)
                {
                    log.Log_I("Sending response ...");
                    try
                    {
                        stream.Write(msg, 0, msg.Length);
                        log.Log_I($"Response sent: \"{data}\"");
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
