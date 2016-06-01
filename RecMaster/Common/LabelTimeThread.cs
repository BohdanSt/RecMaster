using System;
using System.Threading;
using System.Windows.Controls;

namespace RecMaster.Common
{
    class LabelTimeThread
    {
        Thread ThreadLabelTime;
        EventWaitHandle EwhThreadLabelTime;
        LabelTime labelTime;
        Label label;

        public LabelTimeThread(Label label)
        {
            this.label = label;
            labelTime = new LabelTime();
        }

        public void Start()
        {
            ThreadLabelTime = new Thread(UpdateLabel);
            ThreadLabelTime.IsBackground = true;
            EwhThreadLabelTime = new ManualResetEvent(false);
            ThreadLabelTime.Start();
        }

        private void UpdateLabel()
        {
            while (true)
            {
                if (EwhThreadLabelTime.WaitOne(10))
                {
                    break;
                }
                else
                {
                    labelTime.Trim();
                    App.Current.Dispatcher.BeginInvoke((Action)(() => label.Content = labelTime.ToString()));
                }
            }
        }

        public void Stop()
        {
            if (EwhThreadLabelTime != null)
                EwhThreadLabelTime.Set();
            labelTime.Reset();
            App.Current.Dispatcher.BeginInvoke((Action)(() => label.Content = "00:00:00"));
        }
    }
}
