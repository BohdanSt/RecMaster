using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;

using AForge.Video.FFMPEG;
using AForge.Video;
using System.Diagnostics;

namespace RecMaster
{
    public enum BitRate
    {
        _50kbit = 5000,
        _100kbit = 10000,
        _500kbit = 50000,
        _1000kbit = 1000000,
        _2000kbit = 2000000,
        _3000kbit = 3000000
    }

    class VideoRec
    {
        private bool isRecording;
        private Rectangle screenArea;
        private UInt32 frameCount;
        
        private int width;
        private int height;

        private ScreenCaptureStream streamVideo;
        private VideoFileWriter writer;
        

        private int fps = 15;
        private string screenName;
        private VideoCodec videoCodec;
        private BitRate bitRate;

        public List<string> screenNamesList;

        public delegate void ThreadLabelTimeDelegate();
        public event ThreadLabelTimeDelegate ThreadLabelTimeEventStart = delegate { };
        public event ThreadLabelTimeDelegate ThreadLabelTimeEventStop = delegate { };

        public delegate void MetroMessageBoxDelegate(string title, string message);
        public event MetroMessageBoxDelegate MetroMessageBoxEvent = delegate { };

        public VideoRec ()
        {
            this.isRecording = false;
            this.frameCount = 0;
            this.width = (int)SystemParameters.VirtualScreenWidth;
            this.height = (int)SystemParameters.VirtualScreenHeight;

            this.writer = new VideoFileWriter();

            screenNamesList = new List<string>();
            screenNamesList.Add(@"Select ALL");
            foreach (var screen in Screen.AllScreens)
            {
                screenNamesList.Add(screen.DeviceName);
            }

        }

        public void StartRec(string selectedScreen, VideoCodec selectedCodec, BitRate selectedBitRate, int selectedfps, string folderPath)
        {
            screenName = selectedScreen;
            videoCodec = selectedCodec;
            bitRate = selectedBitRate;
            fps = selectedfps;

            this.InitRec(folderPath);
        }

        private void InitRec(string path)
        {
            if (isRecording == false)
            {
            
                isRecording = true;

                this.SetScreenArea();

                this.frameCount = 0;
                
                string fullName = string.Format(@"{0}\{1}_{2}.avi", path, Environment.UserName.ToUpper(), DateTime.Now.ToString("d_MMM_yyyy_HH_mm_ssff"));

                // Save File option
                writer.Open(fullName, this.width, this.height, (int)fps, (VideoCodec)videoCodec, (int)(BitRate)bitRate);

                // Start main work
                this.StartRecord();
            }
        }

        private void SetScreenArea()
        {
            if (string.Compare(screenName, @"Select ALL", StringComparison.OrdinalIgnoreCase) == 0)
            {
                foreach (Screen screen in Screen.AllScreens)
                {
                    this.screenArea = Rectangle.Union(screenArea, screen.Bounds);
                }
            }
            else
            {
                this.screenArea = Screen.AllScreens
                                        .First(scr => scr.DeviceName.Equals(screenName))
                                        .Bounds;
                this.width = this.screenArea.Width;
                this.height = this.screenArea.Height;
            }
        }

        private void StartRecord()
        {
            this.streamVideo = new ScreenCaptureStream(new Rectangle(0, 0, this.width,  this.height));

            this.streamVideo.NewFrame += new NewFrameEventHandler(this.NewFrame);

            this.streamVideo.Start();

            OnThreadLabelTimeEventStart();
        }

        private void NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (this.isRecording)
            {
                this.frameCount++;
                this.writer.WriteVideoFrame(eventArgs.Frame);
            }
            else
            {
                streamVideo.SignalToStop();
                writer.Close();
                OnThreadLabelTimeEventStop();
            }
        }

        public void StopRec()
        {
            isRecording = false;
            OnMetroMessageBox("Запис завершено", "Файл було успішно збережено");
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
