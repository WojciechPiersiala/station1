using HarfBuzzSharp;
using Microsoft.VisualBasic.Logging;
using station1.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace station1.Forms
{
    public partial class Form_mainDisplay : FormWithRef
    {
        private string tag = "mainDisplay";
        //private Logger log;
        private TcpServer tcpServer;
        private PdmPlotter pdmPlotter;

        private bool isTcpListening;
        CancellationTokenSource cancelTcp;
        CancellationTokenSource cancelPlot;

        //private ConcurrentQueue<AudioData> sampleQueue;
        public Form_mainDisplay()
        {
            isTcpListening = false;
            this.ShowIcon = false;
            this.FormBorderStyle = FormBorderStyle.None;
            InitializeComponent();
            this.richTextBox_logger.ReadOnly = true;
            //log = new Logger(richTextBox_logger);
            //sampleQueue = new ConcurrentQueue<AudioData>();
            Logger.Initialize(richTextBox_logger);

            /* Audio and plotter setup */
            int audioLen = 6144; // audio len in bytesmust be the same as in the client
            int audioCunks = 2; // number of audio chunks to store in the plotter
            int samplingRate = 52000; //Hz mic frequency

            tcpServer = new TcpServer(audioLen);
            pdmPlotter = new PdmPlotter(formsPlot_pdm, tcpServer.connectedClients, audioLen, audioCunks, samplingRate);
        }

        private void button_exit_Click(object sender, EventArgs e)
        {
            Form_menu form_main = new Form_menu();
            MainFormReference.ChangeForm(form_main);
            Application.Exit();
        }

        private void button_start_Click(object sender, EventArgs e)
        {
            Button tmpButton = (Button)sender;
            if (!isTcpListening)
            {
                isTcpListening = true;
                tmpButton.Text = "Stop";

                cancelTcp = new CancellationTokenSource();
                Logger.I(tag, $"Tcp server thread started");
                Task taskTcp = Task.Run(() => tcpServer.ListenTcp(cancelTcp.Token));
                Logger.I(tag, $"Tcp server monitor connections thread started");
                Task taskTcpMon = Task.Run(() => tcpServer.MonitorConnections(cancelTcp.Token));

                cancelPlot = new CancellationTokenSource();
                Logger.I(tag, $"Plotter thread started");
                Task taskPlot = Task.Run(() => pdmPlotter.Plot(cancelPlot.Token));

            }
            else
            {
                isTcpListening = false;
                tmpButton.Text = "Start";

                cancelTcp.Cancel();
                cancelPlot.Cancel();

                tcpServer.StopTcp();
            }
        }

        private void richTextBox_logger_TextChanged(object sender, EventArgs e)
        {
            // set the current caret position to the end
            richTextBox_logger.SelectionStart = richTextBox_logger.Text.Length;
            // scroll it automatically
            richTextBox_logger.ScrollToCaret();
        }

        private void button_send_Click(object sender, EventArgs e)
        {
            //tcpServer.SendTcp(textBox_input.Text + "\n");
            string input = textBox_input.Text;

            string str2look = "Channel";
            bool hasChanel = input.Contains(str2look, StringComparison.OrdinalIgnoreCase); // true
            if (hasChanel)
            {
                pdmPlotter.changeTimeOffset(input, str2look);
            }

        }


        private void button_synch_Click(object sender, EventArgs e)
        {
            pdmPlotter.Synch();
            button_ExactSynch.Enabled = true;
        }

        private void button_export_Click(object sender, EventArgs e)
        {
            pdmPlotter.ExportData();
        }

        private void textBox_input_TextChanged(object sender, EventArgs e)
        {

        }

        private void button_ExactSynch_Click(object sender, EventArgs e)
        {
            pdmPlotter.ExactSynch();
        }
    }
}