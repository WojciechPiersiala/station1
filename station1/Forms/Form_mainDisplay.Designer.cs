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
            richTextBox_logger = new RichTextBox();
            formsPlot_pdm = new ScottPlot.WinForms.FormsPlot();
            splitContainer1 = new SplitContainer();
            panel1 = new Panel();
            button_ExactSynch = new Button();
            button_export = new Button();
            button_synch = new Button();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // button_exit
            // 
            button_exit.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button_exit.Location = new Point(928, 6);
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
            textBox_input.Size = new Size(396, 27);
            textBox_input.TabIndex = 5;
            textBox_input.Text = "Channel1 10000";
            textBox_input.TextChanged += textBox_input_TextChanged;
            // 
            // richTextBox_logger
            // 
            richTextBox_logger.Dock = DockStyle.Fill;
            richTextBox_logger.Location = new Point(0, 0);
            richTextBox_logger.Name = "richTextBox_logger";
            richTextBox_logger.Size = new Size(1061, 218);
            richTextBox_logger.TabIndex = 7;
            richTextBox_logger.Text = "";
            richTextBox_logger.TextChanged += richTextBox_logger_TextChanged;
            // 
            // formsPlot_pdm
            // 
            formsPlot_pdm.DisplayScale = 1.25F;
            formsPlot_pdm.Dock = DockStyle.Fill;
            formsPlot_pdm.Location = new Point(0, 0);
            formsPlot_pdm.Name = "formsPlot_pdm";
            formsPlot_pdm.Size = new Size(1061, 215);
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
            splitContainer1.Panel2.Controls.Add(richTextBox_logger);
            splitContainer1.Size = new Size(1061, 437);
            splitContainer1.SplitterDistance = 215;
            splitContainer1.TabIndex = 8;
            // 
            // panel1
            // 
            panel1.Controls.Add(button_ExactSynch);
            panel1.Controls.Add(button_export);
            panel1.Controls.Add(button_synch);
            panel1.Controls.Add(button_start);
            panel1.Controls.Add(button_send);
            panel1.Controls.Add(textBox_input);
            panel1.Controls.Add(button_exit);
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 398);
            panel1.Name = "panel1";
            panel1.Size = new Size(1061, 39);
            panel1.TabIndex = 9;
            // 
            // button_ExactSynch
            // 
            button_ExactSynch.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button_ExactSynch.Enabled = false;
            button_ExactSynch.Location = new Point(805, 7);
            button_ExactSynch.Name = "button_ExactSynch";
            button_ExactSynch.Size = new Size(94, 29);
            button_ExactSynch.TabIndex = 9;
            button_ExactSynch.Text = "Exact Synch";
            button_ExactSynch.UseVisualStyleBackColor = true;
            button_ExactSynch.Click += button_ExactSynch_Click;
            // 
            // button_export
            // 
            button_export.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button_export.Location = new Point(605, 6);
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
            button_synch.Location = new Point(705, 6);
            button_synch.Name = "button_synch";
            button_synch.Size = new Size(94, 29);
            button_synch.TabIndex = 6;
            button_synch.Text = "Synch";
            button_synch.UseVisualStyleBackColor = true;
            button_synch.Click += button_synch_Click;
            // 
            // Form_mainDisplay
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1061, 437);
            Controls.Add(panel1);
            Controls.Add(splitContainer1);
            Name = "Form_mainDisplay";
            Text = "  ";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button button_exit;
        private Button button_start;
        private Button button_send;
        private TextBox textBox_input;
        private RichTextBox richTextBox_logger;
        private ScottPlot.WinForms.FormsPlot formsPlot_pdm;
        private SplitContainer splitContainer1;
        private Panel panel1;
        private Button button_synch;
        private Button button_export;
        private Button button_ExactSynch;
    }
}