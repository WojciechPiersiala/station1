using HarfBuzzSharp;
using station1.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace station1.Models
{
    internal class ControlBox
    {
        private static string tag = "ControlBox";
        public int clientId = 12;

        public bool doSetFrequency = false;
        public bool doSetLag = false;

        public double setAmplitude = 0.0;
        public double setFrequency = 0.0;
        public double setLag = 0.0;
        public double setOffset = 0.0;

        public double currAmplitude = 0.0;
        public double currFrequency = 0.0;
        public double currLag = 0.0;
        public double currOffset = 0.0;
        public Form_Controls formRef = null;
        //public AudioChunkChannel clientRef = null;

        public ControlBox(Form_Controls formRef)
        {
            this.formRef = formRef;
        }

        //public void selectId()
        //{
        //    Logger.I(tag, $"Selected id: {formRef.comboBox_Ids.SelectedItem}");
        //}
        public void selesctId()
        {
            bool doLog = false;
            if (formRef == null || !formRef.IsHandleCreated) return;

            formRef.Invoke((MethodInvoker)delegate
            {
                if (formRef.comboBox_Ids.SelectedItem != null)
                {
                    string selectedStr = formRef.comboBox_Ids.SelectedItem.ToString();
                    if (int.TryParse(selectedStr, out int id))
                    {
                        clientId = id;
                        if(doLog) Logger.I(tag, $"Selected id: {clientId}");
                    }
                    else
                    {
                        if (doLog) Logger.W(tag, $"Invalid id: {selectedStr}");
                    }
                }
                else
                {
                    if (doLog) Logger.W(tag, "No id selected");
                }
            });
        }

        public void setFrequencyMtd(ref AudioChunkChannel clientRef)
        {
            if (clientRef.id != clientId) return;
            if (doSetFrequency)
            {
                clientRef.Freq = setFrequency;
                doSetFrequency = false;
            }
        }



        public void refreshCurrentValues(ref AudioChunkChannel clientRef)
        {
            if (clientRef.id == clientId)
            {
                currFrequency = clientRef.Freq;
                if (clientRef.offsetMs is null)
                    currLag = 0.0;
                else
                    currLag = (double)clientRef.offsetMs;
            }
            //Logger.I(tag, $"Updated client: {clientRef.id}, Freq: {currFrequency}, Lag: {currLag}");

            if (formRef != null && formRef.IsHandleCreated)
            {
                formRef.BeginInvoke((MethodInvoker)delegate
                {
                    formRef.textBox_currentFrequency.Text = currFrequency.ToString("F12");
                    formRef.textBox_currentLag.Text = currLag.ToString("F12");
                });
            }
        }

        public void updateClient(ref AudioChunkChannel clientRef)
        {
            selesctId();
            setFrequencyMtd(ref clientRef);
            refreshCurrentValues(ref clientRef);

        }
    }
}
