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

            ///* Audio and plotter setup */
            //int audioLen = 4096; // audio len in bytesmust be the same as in the client old: 6144
            //int audioCunks = 1; // number of audio chunks to store in the plotter
            //int samplingRate = 16000; //Hz mic frequency

            tcpServer = new TcpServer();
            pdmPlotter = new PdmPlotter(formsPlot_timeShifts, formsPlot_pdm, tcpServer.connectedClients);
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
                Task taskPlot = Task.Run(() => pdmPlotter.RunProgram(this, cancelPlot.Token));

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
            ////tcpServer.SendTcp(textBox_input.Text + "\n");
            //string input = textBox_input.Text;

            //string str2look = "Channel";
            //bool hasChanel = input.Contains(str2look, StringComparison.OrdinalIgnoreCase); // true
            //if (hasChanel)
            //{
            //    pdmPlotter.manuallyChangeTimeOffset(input, str2look);
            //}



            //string str2look2 = "Freq";
            //bool hasChanel2 = input.Contains(str2look2, StringComparison.OrdinalIgnoreCase); // true
            //if (hasChanel2)
            //{
            //    pdmPlotter.manuallyChangeFreqOffset(input, str2look);
            //}

        }


        private void button_synch_Click(object sender, EventArgs e)
        {
            //Logger.W(tag, "Not implemented");
            pdmPlotter.Synch();
        }

        private void button_export_Click(object sender, EventArgs e)
        {
            pdmPlotter.ExportData();
        }

        private void textBox_input_TextChanged(object sender, EventArgs e)
        {

        }

        //private void button_ExactSynch_Click(object sender, EventArgs e)
        //{
        //    pdmPlotter.startExactSynch();
        //}

        private void button_controls_Click(object sender, EventArgs e)
        {
            Form_Controls formControls = new Form_Controls();
            formControls.Show();  // opens it non-blocking
        }

        private void label_serverTime_Click(object sender, EventArgs e)
        {

        }

        private void button_ExactSynch_Click(object sender, EventArgs e)
        {
            pdmPlotter.ExactSynch();
        }
    }
}