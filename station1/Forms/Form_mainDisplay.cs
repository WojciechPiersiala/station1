using HarfBuzzSharp;
using Microsoft.VisualBasic.Logging;
using ScottPlot.WinForms;
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
    /// <summary>
    /// Glowny panel uzytkownika
    /// </summary>
    public partial class Form_mainDisplay : FormWithRef
    {
        private string tag = "mainDisplay";
        private TcpServer tcpServer;
        private PdmPlotter pdmPlotter;

        private bool isTcpListening;
        CancellationTokenSource cancelTcp;
        CancellationTokenSource cancelPlot;


        public Form_mainDisplay()
        {
            isTcpListening = false;
            this.ShowIcon = false;
            this.FormBorderStyle = FormBorderStyle.None;
            InitializeComponent();
            this.richTextBox_logger.ReadOnly = true;

            Logger.Initialize(richTextBox_logger);

            tcpServer = new TcpServer();
            pdmPlotter = new PdmPlotter(formsPlot_timeShifts, formsPlot_pdm, formsPlot_locate, formsPlot_doa, formsPlot_TDoA,
                tcpServer.connectedClients);
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
            richTextBox_logger.SelectionStart = richTextBox_logger.Text.Length;
            richTextBox_logger.ScrollToCaret();
        }

        private void button_send_Click(object sender, EventArgs e)
        {


        }


        private void button_synch_Click(object sender, EventArgs e)
        {
            pdmPlotter.Synch();
        }

        private void button_export_Click(object sender, EventArgs e)
        {
            pdmPlotter.ExportData();
        }

        private void textBox_input_TextChanged(object sender, EventArgs e)
        {

        }



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

        private void formsPlot1_Load(object sender, EventArgs e)
        {

        }

    }
}