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
using Microsoft.VisualBasic.Logging;
using station1.Models;


namespace station1.Forms
{
    public partial class Form_mainDisplay : FormWithRef
    {
        private Logger log;
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
            log = new Logger(richTextBox_logger);
            //sampleQueue = new ConcurrentQueue<AudioData>();
            tcpServer = new TcpServer(log);
            pdmPlotter = new PdmPlotter(formsPlot_pdm, tcpServer.connectedClients/*, sampleQueue*/);
        }

        private void button_exit_Click(object sender, EventArgs e)
        {
            Form_menu form_main = new Form_menu();
            MainFormReference.ChangeForm(form_main);
        }

        private void button_start_Click(object sender, EventArgs e)
        {
            Button tmpButton = (Button)sender;
            if (!isTcpListening)
            {
                isTcpListening = true;
                tmpButton.Text = "Stop";

                cancelTcp = new CancellationTokenSource();
                log.Log_I("Tcp server thread started");
                Task taskTcp = Task.Run(() => tcpServer.ListenTcp(cancelTcp.Token));
                cancelPlot = new CancellationTokenSource();
                log.Log_I("Plotter thread started");
                Task taskPlot = Task.Run(() => pdmPlotter.Plot(cancelPlot.Token));

            }
            else
            {
                isTcpListening = false;
                tmpButton.Text = "Start";
                tcpServer.StopTcp();

                cancelTcp.Cancel();
                cancelPlot.Cancel();
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

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void Form_mainDisplay_Load(object sender, EventArgs e)
        {

        }

        private void formsPlot_pdm_Load(object sender, EventArgs e)
        {

        }

        private void textBox_input_TextChanged(object sender, EventArgs e)
        {

        }

    }
}