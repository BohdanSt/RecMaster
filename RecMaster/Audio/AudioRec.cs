using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio;
using NAudio.Wave;
using WPFSoundVisualizationLib;

namespace RecMaster
{
    class AudioRec
    {
        private WaveIn sourceInStream;
        private WaveOut loopbackStream;
        private WasapiLoopbackCapture sourceOutStream;
        private WaveFileWriter waveWriter;
        private WaveInProvider waveInProvider;

        private bool isInputStream;
        private bool isLoopback;

        private int sourceNumber;
        public List<WaveInCapabilities> sourceIn;
        public List<WaveOutCapabilities> sourceOut;
        private WPFSoundVisualizationLib.Equalizer equalizer;

        public delegate void ThreadLabelTimeDelegate();
        public event ThreadLabelTimeDelegate ThreadLabelTimeEventStart = delegate { };
        public event ThreadLabelTimeDelegate ThreadLabelTimeEventStop = delegate { };

        public delegate void MetroMessageBoxDelegate(string title, string message);
        public event MetroMessageBoxDelegate MetroMessageBoxEvent = delegate { };

        public AudioRec(WPFSoundVisualizationLib.Equalizer equalizer)
        { 
            sourceInStream = null;
            sourceOutStream = null;
            waveWriter = null;

            sourceIn = new List<WaveInCapabilities>();
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                sourceIn.Add(WaveIn.GetCapabilities(i));
            }

            sourceOut = new List<WaveOutCapabilities>();
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                sourceOut.Add(WaveOut.GetCapabilities(i));
            }

            this.equalizer = equalizer;
        }

        public void StartRec(int sourceNumber, bool isLoopback, string folderPath)
        {
            if (sourceNumber < sourceIn.Count)
            {
                isInputStream = true;
                this.sourceNumber = sourceNumber - 1;
            }
            else
            {
                isInputStream = false;
                this.sourceNumber = sourceNumber - sourceIn.Count;
            }

            this.isLoopback = isLoopback;
                 
            this.InitStream(folderPath);
        }

        void InitStream(string path)
        {
            if (isInputStream)
            {
                InitInputStream(path);
                if (isLoopback)
                    StartLoopback();
            }
            else
                InitOutputStream(path);
        }

        void InitInputStream(string path)
        {
            sourceInStream = new WaveIn();
            sourceInStream.DeviceNumber = sourceNumber;
            sourceInStream.WaveFormat = new WaveFormat(44100, WaveIn.GetCapabilities(sourceNumber).Channels);

            sourceInStream.DataAvailable += new EventHandler<WaveInEventArgs>(sourceInStream_DataAvailable);
            sourceInStream.RecordingStopped += new EventHandler<StoppedEventArgs>(sourceInStream_RecordingStopped);

            string fullName = string.Format(@"{0}\{1}_{2}.wav", path, Environment.UserName.ToUpper(), DateTime.Now.ToString("d_MMM_yyyy_HH_mm_ssff"));

            waveWriter = new WaveFileWriter(fullName, sourceInStream.WaveFormat);
            
            sourceInStream.StartRecording();

            OnThreadLabelTimeEventStart();
        }

        void InitOutputStream(string path)
        {
            sourceOutStream = new WasapiLoopbackCapture();

            sourceOutStream.DataAvailable += new EventHandler<WaveInEventArgs>(sourceOutStream_DataAvailable);
            sourceOutStream.RecordingStopped += new EventHandler<StoppedEventArgs>(sourceOutStream_RecordingStopped);

            string fullName = string.Format(@"{0}\{1}_{2}.wav", path, Environment.UserName.ToUpper(), DateTime.Now.ToString("d_MMM_yyyy_HH_mm_ssff"));

            waveWriter = new WaveFileWriter(fullName, sourceOutStream.WaveFormat);

            sourceOutStream.StartRecording();

            OnThreadLabelTimeEventStart();
        }


        void sourceInStream_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveWriter != null)
            {
                waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
                waveWriter.Flush();
            }
        }

        void sourceInStream_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (sourceInStream != null)
            {
                sourceInStream.Dispose();
                sourceInStream = null;
            }

            if (waveWriter != null)
            {
                waveWriter.Close();
                waveWriter.Dispose();
                waveWriter = null;
            }
        }

        void sourceOutStream_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (waveWriter != null)
            {
                waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
                waveWriter.Flush();
            }
        }

        void sourceOutStream_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (sourceOutStream != null)
            {
                sourceOutStream.Dispose();
                sourceOutStream = null;
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
            if (isInputStream)
            {
                sourceInStream.StopRecording();
                if (isLoopback)
                    StopLoopback();
            }
            else
                sourceOutStream.StopRecording();
            OnThreadLabelTimeEventStop();
            OnMetroMessageBox("Запис завершено", "Файл було успішно збережено");
        }

        private void StartLoopback()
        {
            if (isInputStream)
            {
                loopbackStream = new WaveOut();
                waveInProvider = new WaveInProvider(sourceInStream);

                loopbackStream.Init(waveInProvider);
                loopbackStream.Play();
            }
        }

        private void StopLoopback()
        {
            if (isInputStream)
            {
                if (loopbackStream != null)
                {
                    loopbackStream.Stop();
                    loopbackStream.Dispose();
                    loopbackStream = null;
                }

                if (waveInProvider != null)
                {
                    waveInProvider = null;
                }
            }
        }

        void OnThreadLabelTimeEventStart()
        {
            ThreadLabelTimeEventStart();
        }

        void OnThreadLabelTimeEventStop()
        {
            ThreadLabelTimeEventStop();
        }

        private void OnMetroMessageBox(string title, string message)
        {
            MetroMessageBoxEvent(title, message);
        }
    }
}
