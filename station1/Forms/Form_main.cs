using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using station1.Models;

namespace station1.Forms
{
    public partial class Form_main : Form
    {
        public FormWithRef Current_form { get; set; }
        public Form_main()
        {
            InitializeComponent();
            this.Size = new System.Drawing.Size(1100, 600);

            Form_mainDisplay form_MainDisplay = new();
            this.ChangeForm(form_MainDisplay);
        }

        public void ChangeForm(FormWithRef newForm)
        {
            panel_main.Controls.Clear();
            newForm.MainFormReference = this;

            newForm.TopLevel = false;
            newForm.TopMost = true;
            panel_main.Controls.Add(newForm);
            newForm.Show();
            newForm.Size = panel_main.Size;
            Current_form = newForm;
        }

        public void Form_main_Resize(object sender, EventArgs e)
        {
            if(Current_form != null)
                Current_form.Size = panel_main.Size;
        }
    }
}
