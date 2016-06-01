using System;

namespace RecMaster.Common
{
    class LabelTime
    {
        private int hours = 0;
        private int minutes = 0;
        private int seconds = 0;
        private int milliseconds = 0;
        private int timeNowMillseconds = DateTime.UtcNow.Millisecond;

        public LabelTime()
        { }

        public string GetHours()
        {
            if (hours == 0)
                return "";
            else if (hours >= 10)
                return (hours.ToString() + ":");
            else return ("0" + hours.ToString() + ":");
        }

        public string GetMinutes()
        {
            if (minutes >= 10)
                return (minutes.ToString() + ":");
            else return ("0" + minutes.ToString() + ":");
        }
        public string GetSeconds()
        {
            if (seconds >= 10)
                return (seconds.ToString() + ":");
            else return ("0" + seconds.ToString() + ":");
        }
        public string GetMilliseconds()
        {
            if (milliseconds >= 100)
                return (milliseconds / 10).ToString();
            else return ("0" + (milliseconds / 10).ToString());
        }

        public override string ToString()
        {
            return (GetHours() + GetMinutes() + GetSeconds() + GetMilliseconds());
        }

        public void Trim()
        {
            int newTimeMilliseconds = DateTime.UtcNow.Millisecond;

            milliseconds += (newTimeMilliseconds >= timeNowMillseconds)?
                (newTimeMilliseconds - timeNowMillseconds)
                : (1000 - timeNowMillseconds + newTimeMilliseconds);
            if (milliseconds >= 1000)
            {
                seconds++;
                milliseconds -= 1000;
                if (seconds >= 60)
                {
                    minutes++;
                    seconds -= 60;
                    if (minutes >= 60)
                    {
                        hours = (hours + 1) % 24;
                        minutes -= 60;
                    }
                }
            }
            timeNowMillseconds = newTimeMilliseconds;
        }

        public void Reset()
        {
            hours = minutes = seconds = milliseconds = 0;
        }
    }
}
