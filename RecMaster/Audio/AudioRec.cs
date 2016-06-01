using System;
using System.Collections.Generic;
using NAudio.Wave;

namespace RecMaster.Audio
{
    class AudioRec
    {
        private WaveIn sourceInStream;
        private DirectSoundOut loopbackStream;
        private WasapiLoopbackCapture sourceOutStream;
        private WaveFileWriter waveWriter;
        private WaveInProvider waveInProvider;

        private bool isInputStream;
        private bool isLoopback;

        private int sourceNumber;
        public List<WaveInCapabilities> sourceIn;
        public List<WaveOutCapabilities> sourceOut;

        Equalizer equalizer;
        EqualizerForWrite equalizerWrite;
        EqualizerBand[] eqBand;

        public delegate void ThreadLabelTimeDelegate();
        public event ThreadLabelTimeDelegate ThreadLabelTimeEventStart = delegate { };
        public event ThreadLabelTimeDelegate ThreadLabelTimeEventStop = delegate { };

        public delegate void MetroMessageBoxDelegate(string title, string message);
        public event MetroMessageBoxDelegate MetroMessageBoxEvent = delegate { };

        public AudioRec()
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

            eqBand = new EqualizerBand[7];
            eqBand[0] = new EqualizerBand(90, 0.8f, 0);
            eqBand[1] = new EqualizerBand(250, 0.8f, 0);
            eqBand[2] = new EqualizerBand(500, 0.8f, 0);
            eqBand[3] = new EqualizerBand(1500, 0.8f, 0);
            eqBand[4] = new EqualizerBand(3000, 0.8f, 0);
            eqBand[5] = new EqualizerBand(5000, 0.8f, 0);
            eqBand[6] = new EqualizerBand(8000, 0.8f, 0);
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

            equalizerWrite = new EqualizerForWrite(eqBand, sourceOutStream.WaveFormat.Channels, sourceOutStream.WaveFormat.SampleRate);

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
                equalizerWrite.Transform(e.Buffer, 0, e.BytesRecorded);
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
            if (equalizerWrite != null)
            {
                equalizerWrite = null;
            }
        }

        public void StopRec(bool isShowMessage)
        {
            if (sourceInStream != null)
                sourceInStream.StopRecording();
            if (isLoopback)
                StopLoopback();
            if (sourceOutStream != null)
                sourceOutStream.StopRecording();
            OnThreadLabelTimeEventStop();
            if (isShowMessage)
                OnMetroMessageBox("Запис завершено", "Файл було успішно збережено");
        }

        private void StartLoopback()
        {
            if (isInputStream)
            {
                loopbackStream = new DirectSoundOut();

                waveInProvider = new WaveInProvider(sourceInStream);

                equalizer = new Equalizer(waveInProvider.ToSampleProvider(), eqBand);

                loopbackStream.Init(equalizer);

                loopbackStream.Play();
            }
        }

        private void StopLoopback()
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
            if (equalizer != null)
            {
                equalizer = null;
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

        public void UpdateEqualizer(float[] values)
        {
            if (eqBand != null)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    eqBand[i].Gain = values[i];
                }
            }
            if (equalizer != null)
            {
                equalizer.Update();
            }
            if (equalizerWrite != null)
            {
                equalizerWrite.Update();
            }
        }
    }
}
