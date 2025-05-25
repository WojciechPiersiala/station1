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
            SuspendLayout();
            // 
            // button_exit
            // 
            button_exit.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button_exit.Location = new Point(679, 398);
            button_exit.Name = "button_exit";
            button_exit.Size = new Size(121, 29);
            button_exit.TabIndex = 0;
            button_exit.Text = "Exit";
            button_exit.UseVisualStyleBackColor = true;
            button_exit.Click += button_exit_Click;
            // 
            // richTextBox_logger
            // 
            richTextBox_logger.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBox_logger.Location = new Point(12, 12);
            richTextBox_logger.Name = "richTextBox_logger";
            richTextBox_logger.Size = new Size(788, 380);
            richTextBox_logger.TabIndex = 1;
            richTextBox_logger.Text = "";
            richTextBox_logger.TextChanged += richTextBox_logger_TextChanged;
            // 
            // button_start
            // 
            button_start.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button_start.Location = new Point(12, 398);
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
            button_send.Location = new Point(112, 398);
            button_send.Name = "button_send";
            button_send.Size = new Size(94, 29);
            button_send.TabIndex = 3;
            button_send.Text = "Send";
            button_send.UseVisualStyleBackColor = true;
            button_send.Click += button_send_Click;
            // 
            // Form_mainDisplay
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(812, 439);
            Controls.Add(button_send);
            Controls.Add(button_start);
            Controls.Add(richTextBox_logger);
            Controls.Add(button_exit);
            Name = "Form_mainDisplay";
            Text = "Form_mainDisplay";
            ResumeLayout(false);
        }

        #endregion

        private Button button_exit;
        private RichTextBox richTextBox_logger;
        private Button button_start;
        private Button button_send;
    }
}