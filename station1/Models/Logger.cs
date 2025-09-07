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
        private static volatile Logger? instance;
        public Logger(RichTextBox textBoxRef)
        {
            this.textBoxRef = textBoxRef;
        }
        public static void Initialize(RichTextBox textBox)
        {
            instance = new Logger(textBox);
        }

        public static void I(string input) => instance?.Log_I(input);
        public static void W(string input) => instance?.Log_W(input);
        public static void E(string input) => instance?.Log_E(input);
        public static void I(string tag, string input) => instance?.Log_I(tag, input);
        public static void W(string tag, string input) => instance?.Log_W(tag, input);
        public static void E(string tag, string input) => instance?.Log_E(tag, input);


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



        public void Log_I(string tag, string input)
        {
            Log(tag + ":   " + input, Color.Black);
        }
        public void Log_E(string tag, string input)
        {
            Log(tag + ":   " + input, Color.Red);
        }
        public void Log_W(string tag, string input)
        {
            Log(tag + ":   " + input, Color.Orange);
        }


    }
}
