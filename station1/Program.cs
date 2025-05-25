
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using station1.Forms;
using station1.Utils;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
namespace station1
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]

        static void Main()
        {
            Thread.CurrentThread.Name = "Main";
            bool USE_CONSOLE = true;
            if (USE_CONSOLE)
            {
                [DllImport("kernel32.dll", SetLastError = true)]
                [return: MarshalAs(UnmanagedType.Bool)]
                static extern bool AllocConsole();
                AllocConsole();
            }

            //// socket test
            //CancellationTokenSource cancelTcp = new CancellationTokenSource();
            //Task taskTCp = Task.Run(() => RunTcp(cancelTcp.Token));
            //// end socket test

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form_main());

        }
        //static async void RunTcp(CancellationToken clcTok)
        //{
        //    //while (!clcTok.IsCancellationRequested)
        //    //{
        //    //    await Task.Delay(1000);
        //    //    Console.WriteLine("TEST");
        //    //}
        //    var ipEndPoint = new IPEndPoint(IPAddress.Any, 5050);
        //    TcpListener server = new(ipEndPoint);

        //    ////localIpAddress = IPAddress.Parse("192.168.1.3");
        //    try
        //    {
        //        Console.WriteLine("Listener starting ...");
        //        server.Start();
        //        Console.WriteLine($"Listener started: {ipEndPoint.Address}");
        //        Byte[] bytes = new Byte[256];
        //        String data = null;

        //        while (!clcTok.IsCancellationRequested)
        //        {
        //            Console.WriteLine("Waiting for connection ...");
        //            using TcpClient handler = await server.AcceptTcpClientAsync();
        //            Console.WriteLine("Connected!");
        //        }
        //    }
        //    finally
        //    {
        //        server.Stop();
        //    }
        //}
    }
}