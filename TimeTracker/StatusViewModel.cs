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
          currentDate = DateTime.Today;
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
                return Truncator.Truncate(ref time, Truncation.Round15);
            }
        }

        public TimeSpan TotalTimeRounded
        {
            get
            {
                var time = TotalTimeActual;
                return Truncator.Truncate(ref time, Truncation.Round15);
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
                return Truncator.Truncate(ref time, Truncation.Round15);
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
            FirePropertyChanged("FlexRounded");
            FirePropertyChanged("IsFlexRoundedNegative");
            FirePropertyChanged("FlexActual");
            FirePropertyChanged("IsFlexActualNegative");
        }

        void FirePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}
