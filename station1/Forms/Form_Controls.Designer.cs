using station1.Models;

namespace station1.Forms
{
    partial class Form_Controls
    {
        private PdmPlotter pdmPlotter;
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
            comboBox_Ids = new ComboBox();
            textBox1 = new TextBox();
            label1 = new Label();
            label2 = new Label();
            textBox2 = new TextBox();
            label3 = new Label();
            textBox_currentFrequency = new TextBox();
            label4 = new Label();
            label5 = new Label();
            textBox_SetFrequency = new TextBox();
            label6 = new Label();
            label7 = new Label();
            label8 = new Label();
            label9 = new Label();
            label10 = new Label();
            label11 = new Label();
            textBox_currentLag = new TextBox();
            label12 = new Label();
            label13 = new Label();
            textBox_SetLag = new TextBox();
            label14 = new Label();
            label15 = new Label();
            textBox7 = new TextBox();
            label16 = new Label();
            label17 = new Label();
            textBox8 = new TextBox();
            button_setAmplitude = new Button();
            button_setFrequency = new Button();
            button_setLag = new Button();
            button_setOffset = new Button();
            SuspendLayout();
            // 
            // comboBox_Ids
            // 
            comboBox_Ids.AutoCompleteCustomSource.AddRange(new string[] { "11", "12", "13" });
            comboBox_Ids.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_Ids.FormattingEnabled = true;
            comboBox_Ids.Items.AddRange(new object[] { "11", "12", "13" });
            comboBox_Ids.Location = new Point(107, 12);
            comboBox_Ids.Name = "comboBox_Ids";
            comboBox_Ids.Size = new Size(151, 28);
            comboBox_Ids.TabIndex = 1;
            comboBox_Ids.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(12, 72);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(137, 27);
            textBox1.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 49);
            label1.Name = "label1";
            label1.Size = new Size(102, 20);
            label1.TabIndex = 3;
            label1.Text = "Set amplitude";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 137);
            label2.Name = "label2";
            label2.Size = new Size(129, 20);
            label2.TabIndex = 4;
            label2.Text = "Current amplitude";
            label2.Click += label2_Click;
            // 
            // textBox2
            // 
            textBox2.Enabled = false;
            textBox2.Location = new Point(12, 160);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(137, 27);
            textBox2.TabIndex = 5;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 15);
            label3.Name = "label3";
            label3.Size = new Size(89, 20);
            label3.TabIndex = 6;
            label3.Text = "Select client";
            // 
            // textBox_currentFrequency
            // 
            textBox_currentFrequency.Enabled = false;
            textBox_currentFrequency.Location = new Point(183, 160);
            textBox_currentFrequency.Name = "textBox_currentFrequency";
            textBox_currentFrequency.Size = new Size(137, 27);
            textBox_currentFrequency.TabIndex = 10;
            textBox_currentFrequency.TextChanged += textBox_currentFrequency_TextChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(183, 137);
            label4.Name = "label4";
            label4.Size = new Size(126, 20);
            label4.TabIndex = 9;
            label4.Text = "Current frequency";
            label4.Click += label4_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(183, 49);
            label5.Name = "label5";
            label5.Size = new Size(99, 20);
            label5.TabIndex = 8;
            label5.Text = "Set frequency";
            label5.Click += label5_Click;
            // 
            // textBox_SetFrequency
            // 
            textBox_SetFrequency.Location = new Point(183, 72);
            textBox_SetFrequency.Name = "textBox_SetFrequency";
            textBox_SetFrequency.Size = new Size(137, 27);
            textBox_SetFrequency.TabIndex = 7;
            textBox_SetFrequency.Text = "16000,00000000";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(155, 72);
            label6.Name = "label6";
            label6.Size = new Size(21, 20);
            label6.TabIndex = 11;
            label6.Text = "%";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(155, 160);
            label7.Name = "label7";
            label7.Size = new Size(21, 20);
            label7.TabIndex = 12;
            label7.Text = "%";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(326, 79);
            label8.Name = "label8";
            label8.Size = new Size(27, 20);
            label8.TabIndex = 13;
            label8.Text = "Hz";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(326, 167);
            label9.Name = "label9";
            label9.Size = new Size(27, 20);
            label9.TabIndex = 14;
            label9.Text = "Hz";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(504, 167);
            label10.Name = "label10";
            label10.Size = new Size(28, 20);
            label10.TabIndex = 20;
            label10.Text = "ms";
            label10.Click += label10_Click;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(504, 79);
            label11.Name = "label11";
            label11.Size = new Size(28, 20);
            label11.TabIndex = 19;
            label11.Text = "ms";
            // 
            // textBox_currentLag
            // 
            textBox_currentLag.Enabled = false;
            textBox_currentLag.Location = new Point(361, 160);
            textBox_currentLag.Name = "textBox_currentLag";
            textBox_currentLag.Size = new Size(137, 27);
            textBox_currentLag.TabIndex = 18;
            textBox_currentLag.TextChanged += textBox_currentLag_TextChanged;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(361, 137);
            label12.Name = "label12";
            label12.Size = new Size(82, 20);
            label12.TabIndex = 17;
            label12.Text = "Current lag";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(361, 49);
            label13.Name = "label13";
            label13.Size = new Size(55, 20);
            label13.TabIndex = 16;
            label13.Text = "Set lag";
            // 
            // textBox_SetLag
            // 
            textBox_SetLag.Location = new Point(361, 72);
            textBox_SetLag.Name = "textBox_SetLag";
            textBox_SetLag.Size = new Size(137, 27);
            textBox_SetLag.TabIndex = 15;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(684, 167);
            label14.Name = "label14";
            label14.Size = new Size(21, 20);
            label14.TabIndex = 26;
            label14.Text = "%";
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(684, 79);
            label15.Name = "label15";
            label15.Size = new Size(21, 20);
            label15.TabIndex = 25;
            label15.Text = "%";
            // 
            // textBox7
            // 
            textBox7.Enabled = false;
            textBox7.Location = new Point(541, 160);
            textBox7.Name = "textBox7";
            textBox7.Size = new Size(137, 27);
            textBox7.TabIndex = 24;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(541, 137);
            label16.Name = "label16";
            label16.Size = new Size(99, 20);
            label16.TabIndex = 23;
            label16.Text = "Current offset";
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new Point(541, 49);
            label17.Name = "label17";
            label17.Size = new Size(72, 20);
            label17.TabIndex = 22;
            label17.Text = "Set offset";
            // 
            // textBox8
            // 
            textBox8.Location = new Point(541, 72);
            textBox8.Name = "textBox8";
            textBox8.Size = new Size(137, 27);
            textBox8.TabIndex = 21;
            // 
            // button_setAmplitude
            // 
            button_setAmplitude.Location = new Point(12, 105);
            button_setAmplitude.Name = "button_setAmplitude";
            button_setAmplitude.Size = new Size(94, 29);
            button_setAmplitude.TabIndex = 27;
            button_setAmplitude.Text = "Set";
            button_setAmplitude.UseVisualStyleBackColor = true;
            button_setAmplitude.Click += button1_Click;
            // 
            // button_setFrequency
            // 
            button_setFrequency.Location = new Point(183, 105);
            button_setFrequency.Name = "button_setFrequency";
            button_setFrequency.Size = new Size(94, 29);
            button_setFrequency.TabIndex = 28;
            button_setFrequency.Text = "Set";
            button_setFrequency.UseVisualStyleBackColor = true;
            button_setFrequency.Click += button_setFrequency_Click;
            // 
            // button_setLag
            // 
            button_setLag.Location = new Point(361, 105);
            button_setLag.Name = "button_setLag";
            button_setLag.Size = new Size(94, 29);
            button_setLag.TabIndex = 29;
            button_setLag.Text = "Set";
            button_setLag.UseVisualStyleBackColor = true;
            // 
            // button_setOffset
            // 
            button_setOffset.Location = new Point(546, 105);
            button_setOffset.Name = "button_setOffset";
            button_setOffset.Size = new Size(94, 29);
            button_setOffset.TabIndex = 30;
            button_setOffset.Text = "Set";
            button_setOffset.UseVisualStyleBackColor = true;
            // 
            // Form_Controls
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(714, 205);
            Controls.Add(button_setOffset);
            Controls.Add(button_setLag);
            Controls.Add(button_setFrequency);
            Controls.Add(button_setAmplitude);
            Controls.Add(label14);
            Controls.Add(label15);
            Controls.Add(textBox7);
            Controls.Add(label16);
            Controls.Add(label17);
            Controls.Add(textBox8);
            Controls.Add(label10);
            Controls.Add(label11);
            Controls.Add(textBox_currentLag);
            Controls.Add(label12);
            Controls.Add(label13);
            Controls.Add(textBox_SetLag);
            Controls.Add(label9);
            Controls.Add(label8);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(textBox_currentFrequency);
            Controls.Add(label4);
            Controls.Add(label5);
            Controls.Add(textBox_SetFrequency);
            Controls.Add(label3);
            Controls.Add(textBox2);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(textBox1);
            Controls.Add(comboBox_Ids);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "Form_Controls";
            Text = "Form_Controls";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        public ComboBox comboBox_Ids;
        private TextBox textBox1;
        private Label label1;
        private Label label2;
        private TextBox textBox2;
        private Label label3;
        public TextBox textBox_currentFrequency;
        private Label label4;
        private Label label5;
        private TextBox textBox_SetFrequency;
        private Label label6;
        private Label label7;
        private Label label8;
        private Label label9;
        private Label label10;
        private Label label11;
        public TextBox textBox_currentLag;
        private Label label12;
        private Label label13;
        private TextBox textBox_SetLag;
        private Label label14;
        private Label label15;
        private TextBox textBox7;
        private Label label16;
        private Label label17;
        private TextBox textBox8;
        private Button button_setAmplitude;
        private Button button_setFrequency;
        private Button button_setLag;
        private Button button_setOffset;
    }
}