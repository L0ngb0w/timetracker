using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace TimeTracker
{
  public class TaskViewModel : INotifyPropertyChanged
  {
    readonly Tables.Task task;

    readonly ObservableCollection<TimeEntryViewModel> timeEntries;

    public event PropertyChangedEventHandler PropertyChanged;

    public Tables.Task Task { get { return task; } }

    public TaskViewModel(Tables.Task task)
    {
      if (task == null)
        throw new ArgumentNullException("task");

      this.task = task;
      timeEntries = new ObservableCollection<TimeEntryViewModel>();
    }

    public DateTime Date
    {
      get { return DateTime.FromBinary(task.Date); }
    }

    public string Text
    {
      get { return task.Text; }
      set
      {
        task.Text = value;
        NotifyPropertyChanged("Text");
      }
    }

    public ObservableCollection<TimeEntryViewModel> TimeEntries { get { return timeEntries; } }

    public TimeSpan TotalTime
    {
      get { return timeEntries.Aggregate(new TimeSpan(), (a, t) => a + t.Difference); }
    }

    public bool IsActive
    {
      get { return timeEntries.Any(e => e.IsActive); }
    }

    public void Refresh()
    {
      NotifyPropertyChanged("TotalTime");

      foreach (var entry in timeEntries)
      {
        entry.Refresh();
      }
    }

    void NotifyPropertyChanged(string propertyName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
