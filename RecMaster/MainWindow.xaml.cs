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
using MediaFoundation;

namespace RecMaster
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        VideoRec _videoRec;
        public MainWindow()
        {
            InitializeComponent();
            _videoRec = new VideoRec();
        }

        private void btnVideoRecord_Click(object sender, RoutedEventArgs e)
        {
            _videoRec.StartRec();
        }

        private void btnVideoSave_Click(object sender, RoutedEventArgs e)
        {
            _videoRec.StopRec();
        }
    }
}
