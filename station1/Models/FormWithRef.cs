using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using station1.Forms;

namespace station1.Models
{
    public class FormWithRef : Form
    {
        public Form_main MainFormReference { get; set; }
    }
}
