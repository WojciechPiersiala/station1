using station1.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace station1.Forms
{
    public partial class Form_Controls : Form
    {
        internal ControlBox controlBoxRef;
        private string tag = "control box";
        public Form_Controls()
        {
            InitializeComponent();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void textBox_currentFrequency_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox_currentLag_TextChanged(object sender, EventArgs e)
        {

        }

        private void button_setFrequency_Click(object sender, EventArgs e)
        {
            controlBoxRef.setFrequency = double.Parse(textBox_SetFrequency.Text);
            controlBoxRef.doSetFrequency = true;
        }
    }
}
