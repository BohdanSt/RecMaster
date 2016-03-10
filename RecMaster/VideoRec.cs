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
        
        private Rectangle screenSize;
        private Rectangle screenArea;
        private UInt32 frameCount;
        
        private int width;
        private int height;

        private ScreenCaptureStream streamVideo;
        private VideoFileWriter writer;
        private Stopwatch stopWatch;
        

        private int fps = 15;
        private string screenName;
        private VideoCodec videoCodec;
        private BitRate bitRate;

        public List<string> screenNamesList;

        public VideoRec ()
        {
            this.isRecording = false;
            this.frameCount = 0;
            this.width = (int)SystemParameters.VirtualScreenWidth;
            this.height = (int)SystemParameters.VirtualScreenHeight;
            this.stopWatch = new Stopwatch();

            this.writer = new VideoFileWriter();

            screenNamesList = new List<string>();
            screenNamesList.Add(@"Select ALL");
            foreach (var screen in Screen.AllScreens)
            {
                screenNamesList.Add(screen.DeviceName);
            }

        }

        public string StartRec(string selectedScreen, VideoCodec selectedCodec, BitRate selectedBitRate, int selectedfps)
        {
            try
            {
                screenName = selectedScreen;
                videoCodec = selectedCodec;
                bitRate = selectedBitRate;
                fps = selectedfps;

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
                this.screenArea = Screen.AllScreens.First(scr => scr.DeviceName.Equals(screenName)).Bounds;
                this.width = this.screenArea.Width;
                this.height = this.screenArea.Height;
            }
        }

        private void StartRecord() //Object stateInfo
        {
            // create screen capture video source
            this.streamVideo = new ScreenCaptureStream(new Rectangle(0, 0, this.width,  this.height));

            // set NewFrame event handler
            this.streamVideo.NewFrame += new NewFrameEventHandler(this.NewFrame);

            // start the video source
            this.streamVideo.Start();

            // _stopWatch
            this.stopWatch.Start();
        }

        private void NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (this.isRecording)
            {
                this.frameCount++;
                this.writer.WriteVideoFrame(eventArgs.Frame);
            /*
                this.lb_1.Invoke(new Action(() =>
                {
                    lb_1.Text = string.Format(@"Frames: {0}", _frameCount);
                }));

                this.lb_stopWatch.Invoke(new Action(() =>
                {
                    this.lb_stopWatch.Text = _stopWatch.Elapsed.ToString();
                }));
            */
            }
            else
            {
                stopWatch.Reset();
                streamVideo.SignalToStop();
                writer.Close();
            }
        }

        public void StopRec()
        {
            isRecording = false;
            System.Windows.MessageBox.Show(@"File saved!");
        }

    }
}
