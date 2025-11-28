namespace station1.Forms
{
    partial class Form_mainDisplay
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button_exit = new Button();
            button_start = new Button();
            button_send = new Button();
            textBox_input = new TextBox();
            formsPlot_pdm = new ScottPlot.WinForms.FormsPlot();
            splitContainer1 = new SplitContainer();
            splitContainer2 = new SplitContainer();
            label_serverTime = new Label();
            richTextBox_logger = new RichTextBox();
            tabControl1 = new TabControl();
            tabPage_Correlation = new TabPage();
            formsPlot_timeShifts = new ScottPlot.WinForms.FormsPlot();
            tabPage_Locate = new TabPage();
            splitContainer3 = new SplitContainer();
            formsPlot_locate = new ScottPlot.WinForms.FormsPlot();
            formsPlot_doa = new ScottPlot.WinForms.FormsPlot();
            tabPage_TDoA = new TabPage();
            formsPlot_TDoA = new ScottPlot.WinForms.FormsPlot();
            panel1 = new Panel();
            button_ExactSynch = new Button();
            button_controls = new Button();
            button_export = new Button();
            button_synch = new Button();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage_Correlation.SuspendLayout();
            tabPage_Locate.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).BeginInit();
            splitContainer3.Panel1.SuspendLayout();
            splitContainer3.Panel2.SuspendLayout();
            splitContainer3.SuspendLayout();
            tabPage_TDoA.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // button_exit
            // 
            button_exit.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button_exit.Location = new Point(1071, 6);
            button_exit.Name = "button_exit";
            button_exit.Size = new Size(121, 29);
            button_exit.TabIndex = 0;
            button_exit.Text = "Exit";
            button_exit.UseVisualStyleBackColor = true;
            button_exit.Click += button_exit_Click;
            // 
            // button_start
            // 
            button_start.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button_start.Location = new Point(3, 7);
            button_start.Name = "button_start";
            button_start.Size = new Size(94, 29);
            button_start.TabIndex = 2;
            button_start.Text = "Start";
            button_start.UseVisualStyleBackColor = true;
            button_start.Click += button_start_Click;
            // 
            // button_send
            // 
            button_send.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button_send.Location = new Point(103, 7);
            button_send.Name = "button_send";
            button_send.Size = new Size(94, 29);
            button_send.TabIndex = 3;
            button_send.Text = "Send";
            button_send.UseVisualStyleBackColor = true;
            button_send.Click += button_send_Click;
            // 
            // textBox_input
            // 
            textBox_input.AcceptsReturn = true;
            textBox_input.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBox_input.Location = new Point(203, 7);
            textBox_input.Name = "textBox_input";
            textBox_input.Size = new Size(326, 27);
            textBox_input.TabIndex = 5;
            textBox_input.Text = "Channel1 10000";
            textBox_input.TextChanged += textBox_input_TextChanged;
            // 
            // formsPlot_pdm
            // 
            formsPlot_pdm.DisplayScale = 1.25F;
            formsPlot_pdm.Dock = DockStyle.Fill;
            formsPlot_pdm.Location = new Point(0, 0);
            formsPlot_pdm.Name = "formsPlot_pdm";
            formsPlot_pdm.Size = new Size(1204, 218);
            formsPlot_pdm.TabIndex = 4;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(formsPlot_pdm);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new Size(1204, 447);
            splitContainer1.SplitterDistance = 218;
            splitContainer1.TabIndex = 8;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(label_serverTime);
            splitContainer2.Panel1.Controls.Add(richTextBox_logger);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(tabControl1);
            splitContainer2.Size = new Size(1204, 225);
            splitContainer2.SplitterDistance = 390;
            splitContainer2.TabIndex = 0;
            // 
            // label_serverTime
            // 
            label_serverTime.AutoSize = true;
            label_serverTime.Location = new Point(12, 6);
            label_serverTime.Name = "label_serverTime";
            label_serverTime.Size = new Size(87, 20);
            label_serverTime.TabIndex = 8;
            label_serverTime.Text = "Server time:";
            label_serverTime.Click += label_serverTime_Click;
            // 
            // richTextBox_logger
            // 
            richTextBox_logger.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBox_logger.Location = new Point(12, 29);
            richTextBox_logger.Name = "richTextBox_logger";
            richTextBox_logger.Size = new Size(353, 151);
            richTextBox_logger.TabIndex = 7;
            richTextBox_logger.Text = "";
            richTextBox_logger.TextChanged += richTextBox_logger_TextChanged;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage_Correlation);
            tabControl1.Controls.Add(tabPage_Locate);
            tabControl1.Controls.Add(tabPage_TDoA);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(810, 225);
            tabControl1.TabIndex = 5;
            // 
            // tabPage_Correlation
            // 
            tabPage_Correlation.Controls.Add(formsPlot_timeShifts);
            tabPage_Correlation.Location = new Point(4, 29);
            tabPage_Correlation.Name = "tabPage_Correlation";
            tabPage_Correlation.Padding = new Padding(3);
            tabPage_Correlation.Size = new Size(802, 192);
            tabPage_Correlation.TabIndex = 0;
            tabPage_Correlation.Text = "TDoA Lag";
            tabPage_Correlation.UseVisualStyleBackColor = true;
            // 
            // formsPlot_timeShifts
            // 
            formsPlot_timeShifts.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            formsPlot_timeShifts.DisplayScale = 1.25F;
            formsPlot_timeShifts.Location = new Point(6, 6);
            formsPlot_timeShifts.Name = "formsPlot_timeShifts";
            formsPlot_timeShifts.Size = new Size(759, 145);
            formsPlot_timeShifts.TabIndex = 0;
            // 
            // tabPage_Locate
            // 
            tabPage_Locate.Controls.Add(splitContainer3);
            tabPage_Locate.Location = new Point(4, 29);
            tabPage_Locate.Name = "tabPage_Locate";
            tabPage_Locate.Padding = new Padding(3);
            tabPage_Locate.Size = new Size(802, 192);
            tabPage_Locate.TabIndex = 1;
            tabPage_Locate.Text = "DoA Far";
            tabPage_Locate.UseVisualStyleBackColor = true;
            // 
            // splitContainer3
            // 
            splitContainer3.Dock = DockStyle.Fill;
            splitContainer3.Location = new Point(3, 3);
            splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            splitContainer3.Panel1.Controls.Add(formsPlot_locate);
            // 
            // splitContainer3.Panel2
            // 
            splitContainer3.Panel2.Controls.Add(formsPlot_doa);
            splitContainer3.Size = new Size(796, 186);
            splitContainer3.SplitterDistance = 304;
            splitContainer3.TabIndex = 0;
            // 
            // formsPlot_locate
            // 
            formsPlot_locate.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            formsPlot_locate.DisplayScale = 1.25F;
            formsPlot_locate.Location = new Point(3, 3);
            formsPlot_locate.Name = "formsPlot_locate";
            formsPlot_locate.Size = new Size(298, 145);
            formsPlot_locate.TabIndex = 1;
            formsPlot_locate.Load += formsPlot1_Load;
            // 
            // formsPlot_doa
            // 
            formsPlot_doa.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            formsPlot_doa.DisplayScale = 1.25F;
            formsPlot_doa.Location = new Point(3, 3);
            formsPlot_doa.Name = "formsPlot_doa";
            formsPlot_doa.Size = new Size(480, 145);
            formsPlot_doa.TabIndex = 2;
            // 
            // tabPage_TDoA
            // 
            tabPage_TDoA.Controls.Add(formsPlot_TDoA);
            tabPage_TDoA.Location = new Point(4, 29);
            tabPage_TDoA.Name = "tabPage_TDoA";
            tabPage_TDoA.Size = new Size(802, 192);
            tabPage_TDoA.TabIndex = 2;
            tabPage_TDoA.Text = "TDoA Close";
            tabPage_TDoA.UseVisualStyleBackColor = true;
            // 
            // formsPlot_TDoA
            // 
            formsPlot_TDoA.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            formsPlot_TDoA.DisplayScale = 1.25F;
            formsPlot_TDoA.Location = new Point(3, 3);
            formsPlot_TDoA.Name = "formsPlot_TDoA";
            formsPlot_TDoA.Size = new Size(791, 148);
            formsPlot_TDoA.TabIndex = 1;
            // 
            // panel1
            // 
            panel1.Controls.Add(button_ExactSynch);
            panel1.Controls.Add(button_controls);
            panel1.Controls.Add(button_export);
            panel1.Controls.Add(button_synch);
            panel1.Controls.Add(button_start);
            panel1.Controls.Add(button_send);
            panel1.Controls.Add(textBox_input);
            panel1.Controls.Add(button_exit);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 408);
            panel1.Name = "panel1";
            panel1.Size = new Size(1204, 39);
            panel1.TabIndex = 9;
            // 
            // button_ExactSynch
            // 
            button_ExactSynch.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button_ExactSynch.Location = new Point(835, 5);
            button_ExactSynch.Name = "button_ExactSynch";
            button_ExactSynch.Size = new Size(94, 29);
            button_ExactSynch.TabIndex = 11;
            button_ExactSynch.Text = "Exact sync";
            button_ExactSynch.UseVisualStyleBackColor = true;
            button_ExactSynch.Click += button_ExactSynch_Click;
            // 
            // button_controls
            // 
            button_controls.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button_controls.Location = new Point(535, 5);
            button_controls.Name = "button_controls";
            button_controls.Size = new Size(94, 29);
            button_controls.TabIndex = 10;
            button_controls.Text = "Controls";
            button_controls.UseVisualStyleBackColor = true;
            button_controls.Click += button_controls_Click;
            // 
            // button_export
            // 
            button_export.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button_export.Location = new Point(635, 6);
            button_export.Name = "button_export";
            button_export.Size = new Size(94, 29);
            button_export.TabIndex = 8;
            button_export.Text = "Export";
            button_export.UseVisualStyleBackColor = true;
            button_export.Click += button_export_Click;
            // 
            // button_synch
            // 
            button_synch.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button_synch.Location = new Point(735, 6);
            button_synch.Name = "button_synch";
            button_synch.Size = new Size(94, 29);
            button_synch.TabIndex = 6;
            button_synch.Text = "Init sync";
            button_synch.UseVisualStyleBackColor = true;
            button_synch.Click += button_synch_Click;
            // 
            // Form_mainDisplay
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1204, 447);
            Controls.Add(panel1);
            Controls.Add(splitContainer1);
            Name = "Form_mainDisplay";
            Text = "  ";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel1.PerformLayout();
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            tabPage_Correlation.ResumeLayout(false);
            tabPage_Locate.ResumeLayout(false);
            splitContainer3.Panel1.ResumeLayout(false);
            splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer3).EndInit();
            splitContainer3.ResumeLayout(false);
            tabPage_TDoA.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button button_exit;
        private Button button_start;
        private Button button_send;
        private TextBox textBox_input;
        private ScottPlot.WinForms.FormsPlot formsPlot_pdm;
        private SplitContainer splitContainer1;
        private Panel panel1;
        private Button button_synch;
        private Button button_export;
        private RichTextBox richTextBox_logger;
        private SplitContainer splitContainer2;
        private ScottPlot.WinForms.FormsPlot formsPlot_timeShifts;
        private Button button_controls;
        public Label label_serverTime;
        private Button button_ExactSynch;
        private TabControl tabControl1;
        private TabPage tabPage_Correlation;
        private TabPage tabPage_Locate;
        private ScottPlot.WinForms.FormsPlot formsPlot_locate;
        private SplitContainer splitContainer3;
        private ScottPlot.WinForms.FormsPlot formsPlot_doa;
        private TabPage tabPage_TDoA;
        private ScottPlot.WinForms.FormsPlot formsPlot_TDoA;
    }
}