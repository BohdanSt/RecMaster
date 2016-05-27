using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MahApps.Metro.Controls;
using AForge.Video.FFMPEG;
using AForge.Video;
using MediaFoundation;

namespace RecMaster
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        AudioRec audioRec;
        VideoRec videoRec;

        LabelTimeThread threadAudio;
        LabelTimeThread threadVideo;

        public MainWindow()
        {
            InitializeComponent();

            audioRec = new AudioRec(Equalizer);
            InitAudioView();
            threadAudio = new LabelTimeThread(LabelTimeAudio);
            audioRec.ThreadLabelTimeEventStart += threadAudio.Start;
            audioRec.ThreadLabelTimeEventStop += threadAudio.Stop;

            videoRec = new VideoRec();
            InitVideoView();
            threadVideo = new LabelTimeThread(LabelTimeVideo);
            videoRec.ThreadLabelTimeEventStart += threadVideo.Start;
            videoRec.ThreadLabelTimeEventStop += threadVideo.Stop;
        }

        private void btnVideoRecord_Click(object sender, RoutedEventArgs e)
        {
            videoRec.StartRec(comboBoxVideoScreens.SelectedValue.ToString(), (VideoCodec)comboBoxVideoCodec.SelectedValue, (BitRate)comboBoxVideoBitRate.SelectedValue, (int)numericVideoFPS.Value);
        }

        private void btnVideoSave_Click(object sender, RoutedEventArgs e)
        {
            videoRec.StopRec();
        }

        private void btnAudioRecord_Click(object sender, RoutedEventArgs e)
        {
            audioRec.StartRec((int)comboBoxAudioSource.SelectedIndex);
        }

        private void btnAudioSave_Click(object sender, RoutedEventArgs e)
        {
            audioRec.StopRec();
        }

        private void InitAudioView()
        {
            foreach (var source in audioRec.sourceIn)
                comboBoxAudioSource.Items.Add(source.ProductName);
            foreach (var source in audioRec.sourceOut)
                comboBoxAudioSource.Items.Add(source.ProductName);

            comboBoxAudioSource.SelectedIndex = 0;
        }

        private void InitVideoView()
        {
            foreach (var codec in Enum.GetValues(typeof(VideoCodec)))
            {
                if (codec.Equals(VideoCodec.Raw) || codec.Equals(VideoCodec.MPEG2))
                    continue;
                comboBoxVideoCodec.Items.Add(codec);
            }

            Dictionary<BitRate, string> bitRateDictionary = new Dictionary<BitRate, string>();
            foreach (BitRate bitRate in Enum.GetValues(typeof(BitRate)))
            {
                bitRateDictionary.Add(bitRate, bitRate.ToString().Substring(1));
            }

            comboBoxVideoBitRate.ItemsSource = bitRateDictionary;
            comboBoxVideoScreens.ItemsSource = videoRec.screenNamesList;
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            threadAudio.Stop();
            threadVideo.Stop();
        }
    }
}
