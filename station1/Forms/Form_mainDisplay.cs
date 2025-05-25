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
        CancellationTokenSource cancelTcp;
        public Form_mainDisplay()
        {
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
            if (tmpButton.Text == "Start")
            {
                cancelTcp = new CancellationTokenSource();
                Task taskTCp = Task.Run(() => tcpServer.RunTcp(cancelTcp.Token));
                tmpButton.Text = "Stop";
            }
            else if(tmpButton.Text == "Stop")
            {
                tcpServer.stopTcp();
                //cancelTcp.Cancel();
                tmpButton.Text = "Start";
            }

        }
    }
}
