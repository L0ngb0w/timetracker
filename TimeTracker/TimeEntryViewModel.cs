using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker
{
    public class TimeEntryViewModel : INotifyPropertyChanged
    {
        readonly Tables.TimeEntry entry;

        public event PropertyChangedEventHandler PropertyChanged;

        public Tables.TimeEntry Entry { get { return entry; } }

        public TimeEntryViewModel(Tables.TimeEntry entry)
        {
            this.entry = entry;
        }

        public DateTime StartTime
        {
            get
            {
                return DateTime.FromBinary(entry.TimeStart);
            }
            set
            {
                entry.TimeStart = value.ToBinary();
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("StartTime"));
            }
        }

        public DateTime? EndTime
        {
            get
            {
                return entry.TimeEnd.HasValue ? DateTime.FromBinary(entry.TimeEnd.Value) : (DateTime?)null;
            }
            set
            {
                entry.TimeEnd = value.HasValue ? value.Value.ToBinary() : (long?)null;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("EndTime"));
            }
        }

        public TimeSpan Difference
        {
            get
            {
                DateTime end;
                if (EndTime.HasValue)
                    end = EndTime.Value;
                else
                    end = DateTime.Now;

                return end - StartTime;
            }
        }

        public string Text
        {
            get { return entry.Text; }
            set
            {
                entry.Text = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Text"));
            }
        }
    }
}
