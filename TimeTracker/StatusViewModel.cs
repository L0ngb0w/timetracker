using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace TimeTracker
{
  class StatusViewModel : INotifyPropertyChanged
  {
    static readonly TimeSpan flex = TimeSpan.FromHours(7);

    readonly ObservableCollection<TaskViewModel> tasks;
    DateTime currentDate;

    public event PropertyChangedEventHandler PropertyChanged;

    public StatusViewModel(ObservableCollection<TaskViewModel> tasks)
    {
      this.tasks = tasks;
      currentDate = TimeService.Date;
    }

    public TimeSpan CurrentTimeActual
    {
      get
      {
        var active = tasks.SingleOrDefault(t => t.IsActive);
        return active == null ? new TimeSpan() : active.TotalTime;
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
        return tasks.Aggregate(new TimeSpan(), (a, t) => a + t.TotalTime);
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

    public DateTime TimeOfWorkEnd
    {
      get
      {
        return TimeService.Time + FlexActual;
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
      get { return currentDate != TimeService.Date; }
    }

    public bool IsRunning
    {
      get { return tasks.Any(t => t.IsActive); }
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
      FirePropertyChanged("TimeOfWorkEnd");
      FirePropertyChanged("IsFlexActualNegative");
    }

    void FirePropertyChanged(string property)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(property));
    }
  }
}
