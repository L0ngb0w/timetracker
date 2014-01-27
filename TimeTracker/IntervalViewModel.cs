using System;
using System.ComponentModel;

namespace TimeTracker {
    public interface IIntervalViewModel : INotifyPropertyChanged {
        long EntryId { get; }

        DateTime StartTime { get; set; }

        DateTime? EndTime { get; set; }

        string Text { get; set; }

        TimeSpan Difference { get; }

        bool IsActive { get; }

        void Refresh();

        void Terminate();
    }

    public class IntervalViewModel : IIntervalViewModel {
        readonly IDatabaseViewModel mDatabase;
        readonly Tables.TimeEntry mEntry;

        public event PropertyChangedEventHandler PropertyChanged;

        public Tables.TimeEntry Entry { get { return mEntry; } }

        public IntervalViewModel(IDatabaseViewModel database, Tables.TimeEntry entry) {
            if (database == null)
                throw new ArgumentNullException("database");

            if (entry == null)
                throw new ArgumentNullException("entry");

            mDatabase = database;
            mEntry = entry;
        }

        public IntervalViewModel(IDatabaseViewModel database, ITaskViewModel task) {
            if (database == null)
                throw new ArgumentNullException("database");

            if (task == null)
                throw new ArgumentNullException("task");

            mDatabase = database;

            var time = TimeService.Time;
            var startTime = time.ToBinary();
            var text = string.Empty;

            using (var statement = mDatabase.Database.Prepare("INSERT INTO [TimeEntry] (TaskId, TimeStart, Text) VALUES (@TaskId, @TimeStart, @Text)")) {
                statement.BindLong("@TaskId", task.TaskId);
                statement.BindLong("@TimeStart", startTime);
                statement.BindText("@Text", text);

                statement.Step();
            }

            mEntry = new Tables.TimeEntry(mDatabase.Database.LastInsertRowid, task.TaskId, startTime, null, text);
        }

        public long EntryId { get { return mEntry.EntryId; } }

        public DateTime StartTime {
            get {
                return DateTime.FromBinary(mEntry.TimeStart);
            }
            set {
                mEntry.TimeStart = value.ToBinary();

                using (var statement = mDatabase.Database.Prepare("UPDATE [TimeEntry] SET TimeStart = @TimeStart WHERE EntryId = @EntryId AND TaskId = @TaskId")) {
                    statement.BindLong("@EntryId", mEntry.EntryId);
                    statement.BindLong("@TaskId", mEntry.TaskId);
                    statement.BindLong("@TimeStart", mEntry.TimeStart);

                    statement.Step();
                }

                NotifyPropertyChanged("StartTime");
                NotifyPropertyChanged("Difference");
            }
        }

        public DateTime? EndTime {
            get {
                return mEntry.TimeEnd.HasValue ? DateTime.FromBinary(mEntry.TimeEnd.Value) : (DateTime?)null;
            }
            set {
                mEntry.TimeEnd = value.HasValue ? value.Value.ToBinary() : (long?)null;

                using (var statement = mDatabase.Database.Prepare("UPDATE [TimeEntry] SET TimeEnd = @TimeEnd WHERE EntryId = @EntryId AND TaskId = @TaskId")) {
                    statement.BindLong("@EntryId", mEntry.EntryId);
                    statement.BindLong("@TaskId", mEntry.TaskId);
                    statement.BindLong("@TimeEnd", mEntry.TimeEnd);

                    statement.Step();
                }

                NotifyPropertyChanged("EndTime");
                NotifyPropertyChanged("Difference");
                NotifyPropertyChanged("IsActive");
            }
        }

        public TimeSpan Difference {
            get {
                var end = EndTime.HasValue ? EndTime.Value : TimeService.Time;
                return end - StartTime;
            }
        }

        public string Text {
            get { return mEntry.Text; }
            set {
                mEntry.Text = value;

                using (var statement = mDatabase.Database.Prepare("UPDATE [TimeEntry] SET Text = @Text WHERE EntryId = @EntryId AND TaskId = @TaskId")) {
                    statement.BindLong("@EntryId", mEntry.EntryId);
                    statement.BindLong("@TaskId", mEntry.TaskId);
                    statement.BindText("@Text", mEntry.Text);

                    statement.Step();
                }

                NotifyPropertyChanged("Text");
            }
        }

        public bool IsActive {
            get { return !EndTime.HasValue; }
        }

        public void Refresh() {
            NotifyPropertyChanged("Difference");
        }

        public void Terminate() {
            EndTime = TimeService.Time;
        }

        void NotifyPropertyChanged(string propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
