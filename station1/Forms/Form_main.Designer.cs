namespace station1.Forms
{
    partial class Form_main
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
            panel_main = new Panel();
            SuspendLayout();
            // 
            // panel_main
            // 
            panel_main.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel_main.Location = new Point(12, 12);
            panel_main.Name = "panel_main";
            panel_main.Size = new Size(968, 317);
            panel_main.TabIndex = 0;
            // 
            // Form_main
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ButtonFace;
            ClientSize = new Size(981, 341);
            Controls.Add(panel_main);
            ForeColor = Color.Black;
            Name = "Form_main";
            Text = "Main";
            Resize += Form_main_Resize;
            ResumeLayout(false);
        }

        #endregion

        private Panel panel_main;
    }
}