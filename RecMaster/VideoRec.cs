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
        private bool _isRecording;
        //private List<string> _screenNames;
        //private Rectangle _screenSize;
        private UInt32 _frameCount;
        private VideoFileWriter _writer;
        private int _width;
        private int _height;
        private ScreenCaptureStream _streamVideo;
        private Stopwatch _stopWatch;
        //private Rectangle _screenArea;

        private int _fps = 15;
        private VideoCodec _videoCodec = VideoCodec.MPEG4;
        private BitRate _bitRate = BitRate._1000kbit;

        public VideoRec ()
        {
            this._isRecording = false;
            this._frameCount = 0;
            this._width = (int)SystemParameters.VirtualScreenWidth;
            this._height = (int)SystemParameters.VirtualScreenHeight;
            this._stopWatch = new Stopwatch();

            this._writer = new VideoFileWriter();

        }

        public string StartRec()
        {
            try
            {
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
            if (_isRecording == false)
            {
            
                _isRecording = true;

                this._frameCount = 0;
                
                string fullName = string.Format(@"{0}\{1}_{2}.avi", path, Environment.UserName.ToUpper(), DateTime.Now.ToString("d_MMM_yyyy_HH_mm_ssff"));

                // Save File option
                _writer.Open(
                    fullName,
                    this._width,
                    this._height,
                    (int)_fps,
                    (VideoCodec)_videoCodec,
                    (int)(BitRate)_bitRate);

                // Start main work
                this.StartRecord();
            }
        }

        private void StartRecord() //Object stateInfo
        {
            // create screen capture video source
            this._streamVideo = new ScreenCaptureStream(new Rectangle(0, 0, this._width,  this._height));

            // set NewFrame event handler
            this._streamVideo.NewFrame += new NewFrameEventHandler(this.NewFrame);

            // start the video source
            this._streamVideo.Start();

            // _stopWatch
            this._stopWatch.Start();
        }

        private void NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (this._isRecording)
            {
                this._frameCount++;
                this._writer.WriteVideoFrame(eventArgs.Frame);
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
                _stopWatch.Reset();
                _streamVideo.SignalToStop();
                _writer.Close();
            }
        }

        public void StopRec()
        {
            _isRecording = false;
            System.Windows.MessageBox.Show(@"File saved!");
        }

    }
}
