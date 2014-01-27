using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace TimeTracker {
    public interface ITaskViewModel : INotifyPropertyChanged {
        long TaskId { get; }

        string Text { get; set; }

        IDatabaseViewModel Database { get; }

        ObservableCollection<ITimeEntryViewModel> TimeEntries { get; }

        TimeSpan TotalTime { get; }

        bool IsActive { get; }

        void Refresh();

        void Start();

        void Terminate();
    }

    public class TaskViewModel : ITaskViewModel {
        readonly IDatabaseViewModel mDatabase;
        readonly Tables.Task mTask;

        readonly ObservableCollection<ITimeEntryViewModel> mTimeEntries;

        public event PropertyChangedEventHandler PropertyChanged;

        public TaskViewModel(IDatabaseViewModel database, Tables.Task task) {
            if (database == null)
                throw new ArgumentNullException("database");

            if (task == null)
                throw new ArgumentNullException("task");

            mDatabase = database;
            mTask = task;
            mTimeEntries = new ObservableCollection<ITimeEntryViewModel>();
        }

        public TaskViewModel(IDatabaseViewModel database, DateTime date)
            : this(database, date, Guid.NewGuid().ToString()) {
        }

        public TaskViewModel(IDatabaseViewModel database, DateTime date, string text) {
            if (database == null)
                throw new ArgumentNullException("database");

            mDatabase = database;
            mTimeEntries = new ObservableCollection<ITimeEntryViewModel>();

            using (var statement = mDatabase.Database.Prepare("INSERT INTO [Task] (Date, Text) VALUES (@Date, @Text)")) {
                statement.BindLong("@Date", date.ToBinary());
                statement.BindText("@Text", text);

                statement.Step();
            }

            mTask = new Tables.Task(mDatabase.Database.LastInsertRowid, date.ToBinary(), text);
        }

        public long TaskId { get { return mTask.TaskId; } }

        public string Text {
            get { return mTask.Text; }
            set {
                mTask.Text = value;

                using (var statement = mDatabase.Database.Prepare("UPDATE [Task] Set Text = @Text WHERE TaskId = @TaskId")) {
                    statement.BindLong("@TaskId", mTask.TaskId);
                    statement.BindText("@Text", mTask.Text ?? string.Empty);

                    statement.Step();
                }

                NotifyPropertyChanged("Text");
            }
        }

        public IDatabaseViewModel Database { get { return mDatabase; } }

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
            TimeEntries.Add(new TimeEntryViewModel(mDatabase, this));

            NotifyPropertyChanged("IsActive");
            NotifyPropertyChanged("TotalTime");
        }

        public void Terminate() {
            mTimeEntries.Single(t => t.IsActive).Terminate();

            NotifyPropertyChanged("IsActive");
            NotifyPropertyChanged("TotalTime");
        }

        void NotifyPropertyChanged(string propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
