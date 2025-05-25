using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic.Logging;
using station1.Models;
using station1.Utils;

namespace station1.Forms
{
    public partial class Form_mainDisplay : FormWithRef
    {
        private Logger log;
        private TcpServer tcpServer;
        private bool isTcpListening;
        CancellationTokenSource cancelTcp;
        public Form_mainDisplay()
        {
            isTcpListening = false;
            this.ShowIcon = false;
            this.FormBorderStyle = FormBorderStyle.None;
            InitializeComponent();
            this.richTextBox_logger.ReadOnly = true;
            log = new Logger(richTextBox_logger);
            tcpServer = new TcpServer(log);
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
                cancelTcp = new CancellationTokenSource();
                Task taskTCp = Task.Run(() => tcpServer.ListenTcp(cancelTcp.Token));
                isTcpListening = true;
                tmpButton.Text = "Stop";
            }
            else
            {
                isTcpListening = false;
                tcpServer.StopTcp();
                cancelTcp.Cancel();
                tmpButton.Text = "Start";
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
            tcpServer.SendTcp();
        }
    }
}
