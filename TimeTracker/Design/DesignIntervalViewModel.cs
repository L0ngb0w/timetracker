using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeTracker.Design {
    class DesignIntervalViewModel : IIntervalViewModel {
        public long EntryId { get { return 456; } }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string Text { get; set; }

        public TimeSpan Difference {
            get { return EndTime.HasValue ? EndTime.Value - StartTime : DateTime.Now - StartTime; }
        }

        public bool IsActive {
            get { return EndTime.HasValue; }
        }

        public void Refresh() {
        }

        public void Terminate() {
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public DesignIntervalViewModel() {
            Text = Guid.NewGuid().ToString();

            StartTime = DateTime.Now - TimeSpan.FromMinutes(5.234);
            EndTime = null;
        }
    }
}
