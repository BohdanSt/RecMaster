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
using System.IO;
using MahApps.Metro.Controls;
using AForge.Video.FFMPEG;
using AForge.Video;
using MediaFoundation;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System.Windows.Forms;
using RecMaster.Audio;
using RecMaster.Video;
using RecMaster.Common;
using AForge.Video.DirectShow;

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

        RegistryKey regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        string folderPath = Directory.GetCurrentDirectory();

        string configFile = Directory.GetCurrentDirectory() + "\\RecMater.config";

        public MainWindow()
        {
            InitializeComponent();

            audioRec = new AudioRec();
            InitAudioView();
            threadAudio = new LabelTimeThread(LabelTimeAudio);
            audioRec.ThreadLabelTimeEventStart += threadAudio.Start;
            audioRec.ThreadLabelTimeEventStop += threadAudio.Stop;
            audioRec.MetroMessageBoxEvent += MetroMessageBox;

            videoRec = new VideoRec(sampleImage);
            InitVideoView();
            threadVideo = new LabelTimeThread(LabelTimeVideo);
            videoRec.ThreadLabelTimeEventStart += threadVideo.Start;
            videoRec.ThreadLabelTimeEventStop += threadVideo.Stop;
            videoRec.MetroMessageBoxEvent += MetroMessageBox;

            InitSettings();
        }

        private void InitSettings()
        {
            if (regKey.GetValue("RecMaster") == null)
            {
                chkIsAutoRun.IsChecked = false;
            }
            else
            {
                chkIsAutoRun.IsChecked = true;
            }

            string path = "";
            try
            {
                StreamReader fileReader = new StreamReader(configFile);
                path = fileReader.ReadLine();
                fileReader.Close();
            }
            catch (FileNotFoundException exception)
            {
                StreamWriter fileWriter = new StreamWriter(configFile, false);
                fileWriter.WriteLine(folderPath);
                fileWriter.Close();
            }

            if (System.IO.Directory.Exists(path))
            {
                folderPath = path;
            }

            textBlockFolder.Text = folderPath; 
        }

        private void btnVideoRecord_Click(object sender, RoutedEventArgs e)
        {
            videoRec.StartRec(comboBoxVideoScreens.SelectedIndex, (VideoCodec)comboBoxVideoCodec.SelectedValue,
                (BitRate)comboBoxVideoBitRate.SelectedValue, (int)numericVideoFPS.Value, folderPath);
            btnVideoRecord.IsEnabled = false;
            comboBoxVideoBitRate.IsEnabled = false;
            comboBoxVideoCodec.IsEnabled = false;
            comboBoxVideoScreens.IsEnabled = false;
            numericVideoFPS.IsEnabled = false;
            btnVideoSave.IsEnabled = true;
        }

        private void btnVideoSave_Click(object sender, RoutedEventArgs e)
        {
            videoRec.StopRec(true);
            btnVideoRecord.IsEnabled = true;
            comboBoxVideoBitRate.IsEnabled = true;
            comboBoxVideoCodec.IsEnabled = true;
            comboBoxVideoScreens.IsEnabled = true;
            numericVideoFPS.IsEnabled = true;
            btnVideoSave.IsEnabled = false;
        }

        private void btnAudioRecord_Click(object sender, RoutedEventArgs e)
        {
            audioRec.StartRec((int)comboBoxAudioSource.SelectedIndex, (bool)chkIsLoopback.IsChecked, folderPath);
            btnAudioRecord.IsEnabled = false;
            comboBoxAudioSource.IsEnabled = false;
            chkIsLoopback.IsEnabled = false;
            btnAudioSave.IsEnabled = true;
        }

        private void btnAudioSave_Click(object sender, RoutedEventArgs e)
        {
            audioRec.StopRec(true);
            btnAudioRecord.IsEnabled = true;
            comboBoxAudioSource.IsEnabled = true;
            chkIsLoopback.IsEnabled = true;
            btnAudioSave.IsEnabled = false;
        }

        private void InitAudioView()
        {
            foreach (var source in audioRec.sourceIn)
                comboBoxAudioSource.Items.Add(source.ProductName);
            comboBoxAudioSource.Items.Add("Вихідний потік");

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

            foreach (string device in videoRec.screenNamesList)
            {
                comboBoxVideoScreens.Items.Add(device);
            }
            foreach (string device in videoRec.videoDevicesNameList)
            {
                comboBoxVideoScreens.Items.Add(device);
            }
            comboBoxVideoScreens.SelectedIndex = 0;
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            audioRec.StopRec(false);
            videoRec.StopRec(false);

            threadAudio.Stop();
            threadVideo.Stop();
        }

        private async void MetroMessageBox(string title, string message)
        {
            await this.ShowMessageAsync(title, message);
        }

        private void AutoRunChange(object sender, RoutedEventArgs e)
        {
            if (chkIsAutoRun.IsChecked == true)
            {
                regKey.SetValue("RecMaster", "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\"");
            }
            else
            {
                regKey.DeleteValue("RecMaster", false);
            }
        }

        private void textBlockFolder_LostFocus(object sender, RoutedEventArgs e)
        {
            if (System.IO.Directory.Exists(textBlockFolder.Text))
            {
                folderPath = textBlockFolder.Text;

                StreamWriter fileWriter = new StreamWriter(configFile, false);
                fileWriter.WriteLine(folderPath);
                fileWriter.Close();
            }
            else
            {
                textBlockFolder.Text = folderPath;
                MetroMessageBox("Вибір теки", "Обрана тека не існує.");
            }
        }

        private void btnFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                folderPath = fbd.SelectedPath;
                textBlockFolder.Text = folderPath;

                StreamWriter fileWriter = new StreamWriter(configFile, false);
                fileWriter.WriteLine(folderPath);
                fileWriter.Close();
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            float[] values = new float[7];
            values[0] = (float)Slider0.Value;
            values[1] = (float)Slider1.Value;
            values[2] = (float)Slider2.Value;
            values[3] = (float)Slider3.Value;
            values[4] = (float)Slider4.Value;
            values[5] = (float)Slider5.Value;
            values[6] = (float)Slider6.Value;

            audioRec.UpdateEqualizer(values);
        }

        private void Slider_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
                ((Slider)sender).Value = 0;
        }
    }
}
