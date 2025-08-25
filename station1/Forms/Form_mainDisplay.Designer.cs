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
            richTextBox_logger = new RichTextBox();
            button_start = new Button();
            button_send = new Button();
            formsPlot_pdm = new ScottPlot.WinForms.FormsPlot();
            textBox_input = new TextBox();
            SuspendLayout();
            // 
            // button_exit
            // 
            button_exit.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button_exit.Location = new Point(679, 396);
            button_exit.Name = "button_exit";
            button_exit.Size = new Size(121, 29);
            button_exit.TabIndex = 0;
            button_exit.Text = "Exit";
            button_exit.UseVisualStyleBackColor = true;
            button_exit.Click += button_exit_Click;
            // 
            // richTextBox_logger
            // 
            richTextBox_logger.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBox_logger.Location = new Point(41, 222);
            richTextBox_logger.Name = "richTextBox_logger";
            richTextBox_logger.Size = new Size(747, 135);
            richTextBox_logger.TabIndex = 1;
            richTextBox_logger.Text = "";
            richTextBox_logger.TextChanged += richTextBox_logger_TextChanged;
            // 
            // button_start
            // 
            button_start.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button_start.Location = new Point(12, 396);
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
            button_send.Location = new Point(112, 396);
            button_send.Name = "button_send";
            button_send.Size = new Size(94, 29);
            button_send.TabIndex = 3;
            button_send.Text = "Send";
            button_send.UseVisualStyleBackColor = true;
            button_send.Click += button_send_Click;
            // 
            // formsPlot_pdm
            // 
            formsPlot_pdm.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            formsPlot_pdm.DisplayScale = 1.25F;
            formsPlot_pdm.Location = new Point(12, 1);
            formsPlot_pdm.Name = "formsPlot_pdm";
            formsPlot_pdm.Size = new Size(788, 204);
            formsPlot_pdm.TabIndex = 4;
            // 
            // textBox_input
            // 
            textBox_input.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBox_input.Location = new Point(41, 363);
            textBox_input.Name = "textBox_input";
            textBox_input.Size = new Size(747, 27);
            textBox_input.TabIndex = 5;
            // 
            // Form_mainDisplay
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(812, 437);
            Controls.Add(textBox_input);
            Controls.Add(formsPlot_pdm);
            Controls.Add(button_send);
            Controls.Add(button_start);
            Controls.Add(richTextBox_logger);
            Controls.Add(button_exit);
            Name = "Form_mainDisplay";
            Text = "Form_mainDisplay";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button_exit;
        private RichTextBox richTextBox_logger;
        private Button button_start;
        private Button button_send;
        private ScottPlot.WinForms.FormsPlot formsPlot_pdm;
        private TextBox textBox_input;
    }
}