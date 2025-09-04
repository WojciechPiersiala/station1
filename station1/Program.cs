
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using station1.Forms;
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
            ApplicationConfiguration.Initialize();
            Application.Run(new Form_main());

        }
    }
}