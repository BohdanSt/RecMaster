using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using AForge;
using AForge.Video.FFMPEG;
using AForge.Video;
using System.IO;
using System.Windows.Media.Imaging;
using AForge.Video.DirectShow;
using AForge.Imaging.Filters;

namespace RecMaster.Video
{
    class VideoRec
    {
        private Rectangle screenArea;
        
        private int width;
        private int height;

        private bool isRecording;
        private bool isScreenCapture;
        private int sourceNumber;
        private Filters filterNumber = Filters.None;

        private ScreenCaptureStream streamScreen;
        private VideoCaptureDevice streamDevice;
        private VideoFileWriter writer;

        private System.Windows.Controls.Image sampleImage;

        private int fps;
        private VideoCodec videoCodec;
        private BitRate bitRate;

        public List<string> screenNamesList;
        public List<string> videoDevicesNameList;

        private FilterInfoCollection videoDevices;

        public delegate void ThreadLabelTimeDelegate();
        public event ThreadLabelTimeDelegate ThreadLabelTimeEventStart = delegate { };
        public event ThreadLabelTimeDelegate ThreadLabelTimeEventStop = delegate { };

        public delegate void MetroMessageBoxDelegate(string title, string message);
        public event MetroMessageBoxDelegate MetroMessageBoxEvent = delegate { };

        public VideoRec (System.Windows.Controls.Image sampleImage)
        {
            this.width = (int)SystemParameters.VirtualScreenWidth;
            this.height = (int)SystemParameters.VirtualScreenHeight;

            screenNamesList = new List<string>();
            screenNamesList.Add(@"ALL Screens");
            foreach (var screen in Screen.AllScreens)
            {
                screenNamesList.Add(FormatString(screen.DeviceName));
            }

            videoDevicesNameList = new List<string>();
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in videoDevices)
            {
                videoDevicesNameList.Add(FormatString(device.Name));
            }

            this.writer = new VideoFileWriter();
            this.isRecording = false;
            this.sampleImage = sampleImage;
        }

        private string FormatString(string name)
        {
            string res = "";
            for (int i = 0; i < name.Length; i++)
            {
                if (!Char.IsControl(name[i]))
                    res += name[i];
            }
            return res;
        }

        public void StartRec(int selectedNumber, VideoCodec selectedCodec, BitRate selectedBitRate, int selectedfps, string folderPath)
        {
            if (selectedNumber < screenNamesList.Count)
            {
                isScreenCapture = true;
                sourceNumber = selectedNumber;
            }
            else
            {
                isScreenCapture = false;
                sourceNumber = selectedNumber - screenNamesList.Count;
            }

            videoCodec = selectedCodec;
            bitRate = selectedBitRate;
            fps = selectedfps;

            this.InitRec(folderPath);
        }

        private void InitRec(string path)
        {
            if (isScreenCapture)
                this.SetScreenArea();

            if (isScreenCapture)
            {
                StartRecordScreen(path);
            }
            else
            {
                StartRecordDevice(path);
            }
        }

        private void SetScreenArea()
        {
            if (sourceNumber == 0)
            {
                foreach (Screen screen in Screen.AllScreens)
                {
                    this.screenArea = Rectangle.Union(screenArea, screen.Bounds);
                }
            }
            else
            {
                this.screenArea = Screen.AllScreens[sourceNumber - 1].Bounds;
                this.width = this.screenArea.Width;
                this.height = this.screenArea.Height;
            }
        }

        private void StartRecordScreen(string path)
        {
            this.streamScreen = new ScreenCaptureStream(new Rectangle(0, 0, this.width,  this.height));

            string fullName = string.Format(@"{0}\{1}_{2}.avi", path, Environment.UserName.ToUpper(), DateTime.Now.ToString("d_MMM_yyyy_HH_mm_ssff"));

            writer.Open(fullName, this.width, this.height, (int)fps, (VideoCodec)videoCodec, (int)(BitRate)bitRate);

            this.streamScreen.NewFrame += new NewFrameEventHandler(this.NewFrame);

            isRecording = true;
            this.streamScreen.Start();

            OnThreadLabelTimeEventStart();
        }

        private void StartRecordDevice(string path)
        {
            this.streamDevice = new VideoCaptureDevice(videoDevices[sourceNumber].MonikerString);

            string fullName = string.Format(@"{0}\{1}_{2}.avi", path, Environment.UserName.ToUpper(), DateTime.Now.ToString("d_MMM_yyyy_HH_mm_ssff"));

            streamDevice.VideoResolution = streamDevice.VideoCapabilities[0];

            writer.Open(fullName, streamDevice.VideoResolution.FrameSize.Width, streamDevice.VideoResolution.FrameSize.Height,
                (int)fps, (VideoCodec)videoCodec, (int)(BitRate)bitRate);

            this.streamDevice.NewFrame += new NewFrameEventHandler(this.NewFrame);

            isRecording = true;
            this.streamDevice.Start();

            OnThreadLabelTimeEventStart();
        }

        private void NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (isRecording)
            {
                Bitmap filterderImage = AddFilter(eventArgs.Frame);
                Bitmap cloneImage = (Bitmap)filterderImage.Clone();
                if (App.Current != null)
                    App.Current.Dispatcher.BeginInvoke((Action)(() =>
                    this.sampleImage.Source = BitmapToImageSource(cloneImage)));
                try
                {
                    this.writer.WriteVideoFrame(filterderImage);
                }
                catch (Exception exception) { }
            }
            else
            {
                if (isScreenCapture)
                    streamScreen.SignalToStop();
                else
                    streamDevice.SignalToStop();

                writer.Close();

                if (isScreenCapture)
                    streamScreen = null;
                else
                    streamDevice = null;

                if (App.Current != null)
                    App.Current.Dispatcher.BeginInvoke((Action)(() =>
                    this.sampleImage.Source = new BitmapImage(new Uri(@"Common/Default.png", UriKind.Relative))));
            }
        }

        public void StopRec(bool isShowMessage)
        {
            isRecording = false;

            OnThreadLabelTimeEventStop();
            if (isShowMessage)
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

        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            if (bitmap == null)
                return new BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        private Bitmap AddFilter(Bitmap bitmap)
        {
            switch(filterNumber)
            {
                case Filters.BrightnessCorrection:
                    return ApplyFilter(bitmap, new BrightnessCorrection());

                case Filters.ColorFiltering:
                    return ApplyFilter(bitmap, new ColorFiltering(new IntRange(25, 230), new IntRange(25, 230), new IntRange(25, 230)));

                case Filters.ContrastCorrection:
                    return ApplyFilter(bitmap, new ContrastCorrection());

                case Filters.DifferenceEdgeDetector:
                    bitmap = Grayscale.CommonAlgorithms.RMY.Apply(bitmap);
                    return ApplyFilter(bitmap, new DifferenceEdgeDetector());

                case Filters.FloydSteinbergDithering:
                    bitmap = Grayscale.CommonAlgorithms.RMY.Apply(bitmap);
                    return ApplyFilter(bitmap, new FloydSteinbergDithering());

                case Filters.Grayscale:
                    return ApplyFilter(bitmap, Grayscale.CommonAlgorithms.BT709);

                case Filters.HomogenityEdgeDetector:
                    bitmap = Grayscale.CommonAlgorithms.RMY.Apply(bitmap);
                    return ApplyFilter(bitmap, new HomogenityEdgeDetector());

                case Filters.HSLFiltering:
                    return ApplyFilter(bitmap, new HSLFiltering(new IntRange(330, 30), new Range(0, 1), new Range(0, 1)));

                case Filters.HueModifier:
                    return ApplyFilter(bitmap, new HueModifier(50));

                case Filters.Jitter:
                    return ApplyFilter(bitmap, new Jitter());

                case Filters.LevelsLinear:
                    LevelsLinear lfilter = new LevelsLinear();
                    lfilter.InRed = new IntRange(30, 230);
                    lfilter.InGreen = new IntRange(50, 240);
                    lfilter.InBlue = new IntRange(10, 210);
                    return ApplyFilter(bitmap, lfilter);

                case Filters.OrderedDithering:
                    bitmap = Grayscale.CommonAlgorithms.RMY.Apply(bitmap);
                    return ApplyFilter(bitmap, new OrderedDithering());

                case Filters.RotateChannels:
                    return ApplyFilter(bitmap, new RotateChannels());

                case Filters.SaturationCorrection:
                    return ApplyFilter(bitmap, new SaturationCorrection(0.15f));

                case Filters.Sepia:
                    return ApplyFilter(bitmap, new Sepia());

                case Filters.SobelEdgeDetector:
                    bitmap = Grayscale.CommonAlgorithms.RMY.Apply(bitmap);
                    return ApplyFilter(bitmap, new SobelEdgeDetector());

                case Filters.Threshold:
                    bitmap = Grayscale.CommonAlgorithms.RMY.Apply(bitmap);
                    return ApplyFilter(bitmap, new Threshold());

                case Filters.YCbCrFiltering:
                    return ApplyFilter(bitmap, new YCbCrFiltering(new Range(0.2f, 0.9f), new Range(-0.3f, 0.3f), new Range(-0.3f, 0.3f)));

                case Filters.YCbCrLinear:
                    YCbCrLinear filter = new YCbCrLinear();
                    filter.InCb = new Range(-0.3f, 0.3f);
                    return ApplyFilter(bitmap, filter);

                default:
                    return bitmap;
            }
        }

        private Bitmap ApplyFilter(Bitmap bitmap, IFilter filter)
        {
            return filter.Apply(bitmap);
        }

        public void UpdateFilter(Filters filterNumber)
        {
            this.filterNumber = filterNumber;
        }
    }
}