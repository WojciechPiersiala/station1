using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace station1.Models
{
    internal class Logger
    {
        private RichTextBox textBoxRef;
        public Logger(RichTextBox textBoxRef)
        {
            this.textBoxRef = textBoxRef;
        }
        private void Log(string input, Color color)
        {
            if (textBoxRef.InvokeRequired)
            {
                textBoxRef.Invoke((MethodInvoker)delegate
                {
                    AppendClorLine(input, color);
                });
            }
            else
            {
                AppendClorLine(input, color);
            }

            if(color == Color.Yellow)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            else if(color == Color.Red)
            {
                Console.ForegroundColor = ConsoleColor.Red; 
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            Console.WriteLine(input);
            Console.ResetColor();
        }

        private void AppendClorLine(string input, Color color)
        {
            textBoxRef.SelectionStart = textBoxRef.TextLength;
            textBoxRef.SelectionLength = 0;
            textBoxRef.SelectionColor = color;
            textBoxRef.AppendText(input + Environment.NewLine);
            textBoxRef.SelectionColor = textBoxRef.ForeColor;
        }

        public void Log_I(string input)
        {
            Log(input, Color.Black);
        }
        public void Log_E(string input)
        {
            Log(input, Color.Red);
        }
        public void Log_W(string input)
        {
            Log(input, Color.Orange);
        }
    }
}
