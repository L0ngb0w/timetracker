using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker
{
    class StatusViewModel : INotifyPropertyChanged
    {
        static readonly TimeSpan flex = TimeSpan.FromHours(7);

        readonly ObservableCollection<TimeEntryViewModel> timeEntries;
        DateTime currentDate;

        public event PropertyChangedEventHandler PropertyChanged;

        public StatusViewModel(ObservableCollection<TimeEntryViewModel> timeEntries)
        {
            this.timeEntries = timeEntries;
        }

        public TimeSpan CurrentTimeActual
        {
            get
            {
                if (!timeEntries.Any())
                    return new TimeSpan();

                return timeEntries.Last().Difference;
            }
        }

        public TimeSpan CurrentTimeRounded
        {
            get
            {
                var time = CurrentTimeActual;
                return Round(ref time);
            }
        }

        public TimeSpan TotalTimeRounded
        {
            get
            {
                var time = TotalTimeActual;
                return Round(ref time);
            }
        }

        public TimeSpan TotalTimeActual
        {
            get
            {
                var time = new TimeSpan();
                foreach (var entry in timeEntries)
                {
                    var diff = entry.Difference;
                    time = time.Add(diff);
                }

                return time;
            }
        }

        public TimeSpan FlexRounded
        {
            get
            {
                var time = FlexActual;
                return Round(ref time);
            }
        }

        public bool IsFlexRoundedNegative
        {
          get
          {
              return TotalTimeRounded < flex;
          }
        }

        public TimeSpan FlexActual
        {
            get
            {
                return flex - TotalTimeActual;
            }
        }

        public bool IsFlexActualNegative
        {
            get
            {
                return TotalTimeActual < flex;
            }
        }

        public DateTime CurrentDate
        {
            get { return currentDate; }
            set
            {
                currentDate = value;
                FirePropertyChanged("CurrentDate");
                FirePropertyChanged("CanGotoLaterDate");
            }
        }

        public bool CanGotoLaterDate
        {
            get { return currentDate != DateTime.Today; }
        }

        public bool IsRunning
        {
            get { return timeEntries.Any() && !timeEntries.Last().EndTime.HasValue; }
        }

        public void Refresh()
        {
            FirePropertyChanged("CurrentTimeRounded");
            FirePropertyChanged("CurrentTimeActual");
            FirePropertyChanged("TotalTimeRounded");
            FirePropertyChanged("TotalTimeActual");
            FirePropertyChanged("FlexActual");
            FirePropertyChanged("IsFlexActualNegative");
        }

        void FirePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        static TimeSpan Round(ref TimeSpan time)
        {
            var current = (int)time.TotalSeconds;
            var start = (int)Math.Floor(time.TotalHours) * 3600;
            var end = start + 3600;

            for (var c = start; c < end; c += 900)
            {
                if (c <= current && current < c + 900)
                {
                    if (current - c <= c + 900 - current)
                        return TimeSpan.FromSeconds(c);
                    else
                        return TimeSpan.FromSeconds(c + 900);
                }
            }

            return new TimeSpan();
        }
    }
}
