using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio;
using NAudio.Wave;

namespace RecMaster
{
    class AudioRec
    {
        private WaveIn sourceStream;
        private WaveFileWriter waveWriter;

        private int sourceNumber;
        public List<WaveInCapabilities> sources;

        public AudioRec()
        {
            sourceStream = null;
            waveWriter = null;

            sources = new List<WaveInCapabilities>();
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                sources.Add(WaveIn.GetCapabilities(i));
            }
        }

        public string StartRec(int sourceNumber)
        {
            try
            {
                this.sourceNumber = sourceNumber;

                FolderBrowserDialog fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    this.InitRec(fbd.SelectedPath);
                    return "Запис розпочато";
                }
                else return "Choose folder";
            }
            catch (Exception exc)
            {
                return exc.Message;
            }

        }

        void InitRec(string path)
        {
            sourceStream = new WaveIn();
            //  sourceStream.DeviceNumber = sourceNumber;
            //    sourceStream.WaveFormat = new WaveFormat(44100, WaveIn.GetCapabilities(sourceNumber).Channels);
            sourceStream.WaveFormat = new WaveFormat(44100, 1);

            sourceStream.DataAvailable += new EventHandler<WaveInEventArgs>(sourceStream_DataAvailable);
            sourceStream.RecordingStopped += new EventHandler<StoppedEventArgs>(sourceStream_RecordingStopped);

            string fullName = string.Format(@"{0}\{1}_{2}.wav", path, Environment.UserName.ToUpper(), DateTime.Now.ToString("d_MMM_yyyy_HH_mm_ssff"));

            waveWriter = new WaveFileWriter(fullName, sourceStream.WaveFormat);

            sourceStream.StartRecording();
        }

        void sourceStream_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveWriter != null)
            {
                waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
                waveWriter.Flush();
            }
        }

        void sourceStream_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (sourceStream != null)
            {
                sourceStream.Dispose();
                sourceStream = null;
            }

            if (waveWriter != null)
            {
                waveWriter.Close();
                waveWriter.Dispose();
                waveWriter = null;
            }
        }


        public void StopRec()
        {
            sourceStream.StopRecording();
            System.Windows.MessageBox.Show(@"File saved!");
        }
    }
}
