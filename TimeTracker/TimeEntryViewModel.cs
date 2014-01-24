using System;
using System.ComponentModel;
using TimeTracker.Storage;

namespace TimeTracker {
    public interface ITimeEntryViewModel : INotifyPropertyChanged {
        DateTime StartTime { get; set; }

        DateTime? EndTime { get; set; }

        string Text { get; set; }

        TimeSpan Difference { get; }

        bool IsActive { get; }

        void Refresh();

        void Terminate(IDatabase database);
    }

    public class TimeEntryViewModel : ITimeEntryViewModel {
        readonly Tables.TimeEntry mEntry;

        public event PropertyChangedEventHandler PropertyChanged;

        public Tables.TimeEntry Entry { get { return mEntry; } }

        public TimeEntryViewModel(Tables.TimeEntry entry) {
            mEntry = entry;
        }

        public DateTime StartTime {
            get {
                return DateTime.FromBinary(mEntry.TimeStart);
            }
            set {
                mEntry.TimeStart = value.ToBinary();
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("StartTime"));
            }
        }

        public DateTime? EndTime {
            get {
                return mEntry.TimeEnd.HasValue ? DateTime.FromBinary(mEntry.TimeEnd.Value) : (DateTime?)null;
            }
            set {
                mEntry.TimeEnd = value.HasValue ? value.Value.ToBinary() : (long?)null;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("EndTime"));
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
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Text"));
            }
        }

        public bool IsActive {
            get { return !EndTime.HasValue; }
        }

        public void Refresh() {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Difference"));
        }

        public void Terminate(IDatabase database) {
            EndTime = TimeService.Time;

            using (var statement = database.Prepare("UPDATE [TimeEntry] SET TimeEnd = @TimeEnd WHERE EntryId = @EntryId AND TaskId = @TaskId")) {
                statement.BindLong("@EntryId", Entry.EntryId);
                statement.BindLong("@TaskId", Entry.TaskId);
                statement.BindLong("@TimeEnd", Entry.TimeEnd);

                statement.Step();
            }
        }
    }
}
