using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TimeTracker.Storage;

namespace TimeTracker {
    public class TaskViewModel : INotifyPropertyChanged {
        readonly Tables.Task mTask;

        readonly ObservableCollection<TimeEntryViewModel> mTimeEntries;

        public event PropertyChangedEventHandler PropertyChanged;

        public Tables.Task Task { get { return mTask; } }

        public TaskViewModel(Tables.Task task) {
            if (task == null)
                throw new ArgumentNullException("task");

            mTask = task;
            mTimeEntries = new ObservableCollection<TimeEntryViewModel>();
        }

        public DateTime Date {
            get { return DateTime.FromBinary(mTask.Date); }
        }

        public string Text {
            get { return mTask.Text; }
            set {
                mTask.Text = value;
                NotifyPropertyChanged("Text");
            }
        }

        public ObservableCollection<TimeEntryViewModel> TimeEntries { get { return mTimeEntries; } }

        public TimeSpan TotalTime {
            get { return mTimeEntries.Aggregate(new TimeSpan(), (a, t) => a + t.Difference); }
        }

        public bool IsActive {
            get { return mTimeEntries.Any(e => e.IsActive); }
        }

        public void Refresh() {
            NotifyPropertyChanged("TotalTime");

            foreach (var entry in mTimeEntries) {
                entry.Refresh();
            }
        }

        public void AddNewTimeEntry(IDatabase database) {
            TerminateActiveTimeEntry(database);

            var time = TimeService.Time;
            var startTime = time.ToBinary();
            var text = string.Empty;

            using (var statement = database.Prepare("INSERT INTO [TimeEntry] (TaskId, TimeStart, Text) VALUES (@TaskId, @TimeStart, @Text)")) {
                statement.BindLong("@TaskId", Task.TaskId);
                statement.BindLong("@TimeStart", startTime);
                statement.BindText("@Text", text);

                statement.Step();
            }

            var entry = new Tables.TimeEntry(database.LastInsertRowid, Task.TaskId, startTime, null, text);
            TimeEntries.Add(new TimeEntryViewModel(entry));
        }

        public void TerminateActiveTimeEntry(IDatabase database) {
            var active = mTimeEntries.SingleOrDefault(t => t.IsActive);
            if (active != null) {
                active.Terminate(database);
            }
        }

        void NotifyPropertyChanged(string propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
