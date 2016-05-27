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
        private WaveOut sourceOutStream;
        private WaveFileWriter waveWriter;

        IWavePlayer waveOut;
        private WaveRecorder recorder;

        private bool isInputStream;

        private int sourceNumber;
        public List<WaveInCapabilities> sourceIn;
        public List<WaveOutCapabilities> sourceOut;
        private WPFSoundVisualizationLib.Equalizer equalizer;

        public delegate void ThreadLabelTimeDelegate();
        public event ThreadLabelTimeDelegate ThreadLabelTimeEventStart = delegate { };
        public event ThreadLabelTimeDelegate ThreadLabelTimeEventStop = delegate { };

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

        public string StartRec(int sourceNumber)
        {
            try
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
                 

                FolderBrowserDialog fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    this.InitStream(fbd.SelectedPath);
                    return "Запис розпочато";
                }
                else return "Choose folder";
            }
            catch (Exception exc)
            {
                return exc.Message;
            }

        }

        void InitStream(string path)
        {
            if (isInputStream)
                InitInputStream(path);
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
            var sineWaveProvider = new SineWaveProvider16();
            sineWaveProvider.SetWaveFormat(16000, 1); // 16kHz mono
            sineWaveProvider.Frequency = 500;
            sineWaveProvider.Amplitude = 0.1f;
            recorder = new WaveRecorder(sineWaveProvider, @"C:\Users\Mark\Documents\sine.wav");
            waveOut = new WaveOut();
            waveOut.Init(recorder);
            waveOut.Play();
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


        void StopPlay()
        {
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }

            if (recorder != null)
            {
                recorder.Dispose();
                recorder = null;
            }
        }

        public void StopRec()
        {
            if (isInputStream)
                sourceInStream.StopRecording();
            else
                StopPlay();
            OnThreadLabelTimeEventStop();
            System.Windows.MessageBox.Show(@"File saved!");
        }

        void OnThreadLabelTimeEventStart()
        {
            ThreadLabelTimeEventStart();
        }

        void OnThreadLabelTimeEventStop()
        {
            ThreadLabelTimeEventStop();
        }
    }

    public class WaveRecorder : IWaveProvider, IDisposable
    {
        private WaveFileWriter writer;
        private IWaveProvider source;

        public WaveRecorder(IWaveProvider source, string destination)
        {
            this.source = source;
            this.writer = new WaveFileWriter(destination, source.WaveFormat);
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = source.Read(buffer, offset, count);
            writer.Write(buffer, offset, bytesRead);
            return bytesRead;
        }

        public WaveFormat WaveFormat
        {
            get { return source.WaveFormat; }
        }

        public void Dispose()
        {
            if (writer != null)
            {
                writer.Dispose();
                writer = null;
            }
        }
    }

    class SineWaveProvider16 : WaveProvider16
    {
        int sample;
        public SineWaveProvider16()
        {
            Frequency = 1000;
            Amplitude = 0.25f;
        }
        public float Frequency { get; set; }
        public float Amplitude { get; set; }
        public override int Read(short[] buffer, int offset, int sampleCount)
        {
            int sampleRate = WaveFormat.SampleRate;
            for (int n = 0; n < sampleCount; n++)
            {
                buffer[n + offset] = (short)(Amplitude * Math.Sin((2 * Math.PI * sample * Frequency) / sampleRate));
                sample++;
                if (sample >= sampleRate) sample = 0;
            }
            return sampleCount;
        }
    }
}
