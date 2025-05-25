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
using station1.Utils;

namespace station1.Models
{
    internal class TcpServer
    {
        private Logger log;
        private TcpListener server;

        public TcpServer(Logger log)
        {
            this.log = log;
        }

        //public async void StopTcp()
        public async Task RunTcp(CancellationToken clcTok)
        {
            //while (!clcTok.IsCancellationRequested)
            //{
            //    await Task.Delay(1000);
            //    log.Log("TEST");
            //}


            ////localIpAddress = IPAddress.Parse("192.168.1.3");
            ///
            server = null;
            try
            {
                var ipEndPoint = new IPEndPoint(IPAddress.Any, 5050);
                server = new TcpListener(ipEndPoint);
                log.Log_I("Listener starting ...");
                server.Start();
                log.Log_I($"Listener started: {ipEndPoint.Address}");
                byte[] bytes = new byte[256];
                string data = null;

                while (!clcTok.IsCancellationRequested)
                {
                    //Task waitForConn = new Task(() => log.Log_W("wait"));

                    //CancellationTokenSource clcWait = new();
                    //_ = Task.Run(() =>
                    //{
                    //    while (!clcWait.Token.IsCancellationRequested)
                    //    {
                    //        log.Log_E("Wait");
                    //        Thread.Sleep(1000);
                    //    }
                    //}, clcWait.Token);

                    log.Log_I("Waiting for connection ...");
                    using TcpClient handler = await server.AcceptTcpClientAsync();
                    log.Log_I("Connected!");
                    //clcWait.Cancel();

                    data = null;
                    NetworkStream stream = handler.GetStream();
                    int i = 0;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        data = Encoding.ASCII.GetString(bytes, 0, i);
                        log.Log_I($"Received: {data}");
                        // Process the data sent by the client.
                        //data = data.ToUpper();

                        //byte[] msg = Encoding.ASCII.GetBytes(data);

                        // Send back a response.
                        //stream.Write(msg, 0, msg.Length);
                        //log.Log_I($"Sent: {data}");
                    }
                }
            }
            catch(Exception e)
            {
                log.Log_E($"Server exception : {e.Message}");
                //MessageBox.Show($"SocketException: {e.Message}");
            }
            finally
            {
                server?.Stop();
            }
        }

        public void stopTcp()
        {
            server?.Stop();
            log.Log_I("Server stopped");
        }
    }

}
