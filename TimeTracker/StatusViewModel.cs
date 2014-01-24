using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace TimeTracker {
    interface IStatusViewModel : INotifyPropertyChanged {
        TimeSpan CurrentTimeActual { get; }

        TimeSpan CurrentTimeRounded { get; }

        TimeSpan TotalTimeActual { get; }

        TimeSpan TotalTimeRounded { get; }

        TimeSpan FlexActual { get; }

        TimeSpan FlexRounded { get; }

        bool IsFlexActualNegative { get; }

        bool IsFlexRoundedNegative { get; }

        DateTime TimeOfWorkEnd { get; }

        DateTime CurrentDate { get; set; }

        bool CanGotoLaterDate { get; }

        bool IsRunning { get; }

        void Refresh();
    }

    class StatusViewModel : IStatusViewModel {
        static readonly TimeSpan Flex = TimeSpan.FromHours(7);

        readonly ObservableCollection<ITaskViewModel> mTasks;
        DateTime mCurrentDate;

        public event PropertyChangedEventHandler PropertyChanged;

        public StatusViewModel(ObservableCollection<ITaskViewModel> tasks) {
            mTasks = tasks;
            mCurrentDate = TimeService.Date;
        }

        public TimeSpan CurrentTimeActual {
            get {
                var active = mTasks.SingleOrDefault(t => t.IsActive);
                return active == null ? new TimeSpan() : active.TotalTime;
            }
        }

        public TimeSpan CurrentTimeRounded {
            get {
                var time = CurrentTimeActual;
                return Truncator.Truncate(ref time, Truncation.Round15);
            }
        }

        public TimeSpan TotalTimeActual {
            get {
                return mTasks.Aggregate(new TimeSpan(), (a, t) => a + t.TotalTime);
            }
        }

        public TimeSpan TotalTimeRounded {
            get {
                var time = TotalTimeActual;
                return Truncator.Truncate(ref time, Truncation.Round15);
            }
        }

        public TimeSpan FlexActual {
            get {
                return Flex - TotalTimeActual;
            }
        }

        public TimeSpan FlexRounded {
            get {
                var time = FlexActual;
                return Truncator.Truncate(ref time, Truncation.Round15);
            }
        }

        public bool IsFlexActualNegative {
            get {
                return TotalTimeActual < Flex;
            }
        }

        public bool IsFlexRoundedNegative {
            get {
                return TotalTimeRounded < Flex;
            }
        }

        public DateTime TimeOfWorkEnd {
            get {
                return TimeService.Time + FlexActual;
            }
        }

        public DateTime CurrentDate {
            get { return mCurrentDate; }
            set {
                mCurrentDate = value;
                FirePropertyChanged("CurrentDate");
                FirePropertyChanged("CanGotoLaterDate");
            }
        }

        public bool CanGotoLaterDate {
            get { return mCurrentDate != TimeService.Date; }
        }

        public bool IsRunning {
            get { return mTasks.Any(t => t.IsActive); }
        }

        public void Refresh() {
            FirePropertyChanged("CurrentTimeRounded");
            FirePropertyChanged("CurrentTimeActual");
            FirePropertyChanged("TotalTimeRounded");
            FirePropertyChanged("TotalTimeActual");
            FirePropertyChanged("FlexRounded");
            FirePropertyChanged("IsFlexRoundedNegative");
            FirePropertyChanged("FlexActual");
            FirePropertyChanged("TimeOfWorkEnd");
            FirePropertyChanged("IsFlexActualNegative");
        }

        void FirePropertyChanged(string property) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}
