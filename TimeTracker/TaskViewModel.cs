using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TimeTracker.Storage;

namespace TimeTracker {
    public interface ITaskViewModel : INotifyPropertyChanged {
        string Text { get; set; }

        DateTime Date { get; }

        ObservableCollection<ITimeEntryViewModel> TimeEntries { get; }

        TimeSpan TotalTime { get; }

        bool IsActive { get; }

        void Refresh();

        void Start();

        void Terminate();
    }

    public class TaskViewModel : ITaskViewModel {
        readonly IDatabase mDatabase;
        readonly Tables.Task mTask;

        readonly ObservableCollection<ITimeEntryViewModel> mTimeEntries;

        public event PropertyChangedEventHandler PropertyChanged;

        public Tables.Task Task { get { return mTask; } }

        public TaskViewModel(IDatabase database, Tables.Task task) {
            if (database == null)
                throw new ArgumentNullException("database");

            if (task == null)
                throw new ArgumentNullException("task");

            mDatabase = database;
            mTask = task;
            mTimeEntries = new ObservableCollection<ITimeEntryViewModel>();
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

        public ObservableCollection<ITimeEntryViewModel> TimeEntries {
            get { return mTimeEntries; }
        }

        public TimeSpan TotalTime {
            get { return mTimeEntries.Aggregate(new TimeSpan(), (a, t) => a + t.Difference); }
        }

        public bool IsActive {
            get { return mTimeEntries.Any(e => e.IsActive); }
        }

        public void Refresh() {
            var active = mTimeEntries.SingleOrDefault(t => t.IsActive);
            if (active != null) {
                active.Refresh();

                NotifyPropertyChanged("TotalTime");
            }
        }

        public void Start() {
            Terminate();

            var time = TimeService.Time;
            var startTime = time.ToBinary();
            var text = string.Empty;

            using (var statement = mDatabase.Prepare("INSERT INTO [TimeEntry] (TaskId, TimeStart, Text) VALUES (@TaskId, @TimeStart, @Text)")) {
                statement.BindLong("@TaskId", Task.TaskId);
                statement.BindLong("@TimeStart", startTime);
                statement.BindText("@Text", text);

                statement.Step();
            }

            var entry = new Tables.TimeEntry(mDatabase.LastInsertRowid, Task.TaskId, startTime, null, text);
            TimeEntries.Add(new TimeEntryViewModel(entry));

            NotifyPropertyChanged("IsActive");
            NotifyPropertyChanged("TotalTime");
        }

        //public void Store(IDatabase database) {
        //    using (var statement = database.Prepare("UPDATE [TimeEntry] SET TimeStart = @TimeStart, TimeEnd = @TimeEnd, Text = @Text")) {
        //        statement.BindLong("@TimeStart", Time)
        //    }
        //}

        public void Terminate() {
            var active = mTimeEntries.SingleOrDefault(t => t.IsActive);
            if (active != null) {
                active.Terminate(mDatabase);

                NotifyPropertyChanged("IsActive");
                NotifyPropertyChanged("TotalTime");
            }
        }

        void NotifyPropertyChanged(string propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
