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
        VideoRec videoRec;

        public MainWindow()
        {
            InitializeComponent();
            videoRec = new VideoRec();

            comboBoxCodec.ItemsSource = Enum.GetValues(typeof(VideoCodec));
            comboBoxBitRate.ItemsSource = Enum.GetValues(typeof(BitRate));
            comboBoxScreens.ItemsSource = videoRec.screenNamesList;
        }

        private void btnVideoRecord_Click(object sender, RoutedEventArgs e)
        {
            videoRec.StartRec(comboBoxScreens.SelectedValue.ToString(), (VideoCodec)comboBoxCodec.SelectedValue, (BitRate)comboBoxBitRate.SelectedValue, (int)numericFPS.Value);
        }

        private void btnVideoSave_Click(object sender, RoutedEventArgs e)
        {
            videoRec.StopRec();
        }

        private void btnAudioRecord_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnAudioSave_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
