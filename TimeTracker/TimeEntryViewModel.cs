using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Storage;

namespace TimeTracker {
    public class TimeEntryViewModel : INotifyPropertyChanged {
        readonly Tables.TimeEntry entry;

        public event PropertyChangedEventHandler PropertyChanged;

        public Tables.TimeEntry Entry { get { return entry; } }

        public TimeEntryViewModel(Tables.TimeEntry entry) {
            this.entry = entry;
        }

        public DateTime StartTime {
            get {
                return DateTime.FromBinary(entry.TimeStart);
            }
            set {
                entry.TimeStart = value.ToBinary();
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("StartTime"));
            }
        }

        public DateTime? EndTime {
            get {
                return entry.TimeEnd.HasValue ? DateTime.FromBinary(entry.TimeEnd.Value) : (DateTime?)null;
            }
            set {
                entry.TimeEnd = value.HasValue ? value.Value.ToBinary() : (long?)null;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("EndTime"));
            }
        }

        public TimeSpan Difference {
            get {
                DateTime end;
                if (EndTime.HasValue)
                    end = EndTime.Value;
                else
                    end = TimeService.Time;

                return end - StartTime;
            }
        }

        public string Text {
            get { return entry.Text; }
            set {
                entry.Text = value;
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
