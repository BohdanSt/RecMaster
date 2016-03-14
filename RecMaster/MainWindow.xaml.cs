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

        public MainWindow()
        {
            InitializeComponent();

            audioRec = new AudioRec();
            InitAudioView();

            videoRec = new VideoRec();
            InitVideoView();
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
            foreach (var source in audioRec.sources)
                comboBoxAudioSource.Items.Add(source.ProductName);
        }

        private void InitVideoView()
        {
            foreach (var codec in Enum.GetValues(typeof(VideoCodec)))
            {
                if (codec.Equals(VideoCodec.Raw) || codec.Equals(VideoCodec.MPEG2))
                    continue;
                comboBoxVideoCodec.Items.Add(codec);
            }

            comboBoxVideoBitRate.ItemsSource = Enum.GetValues(typeof(BitRate));
            comboBoxVideoScreens.ItemsSource = videoRec.screenNamesList;
        }
    }
}
