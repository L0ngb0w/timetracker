using System;
using System.ComponentModel;

namespace TimeTracker.Design {
    class DesignTaskViewModel : ITaskViewModel {
        public string Text { get; set; }

        public DateTime Date {
            get { return DateTime.Today; }
        }

        public TimeSpan TotalTime {
            get { return TimeSpan.FromMinutes(666); }
        }

        public bool IsActive {
            get { return true; }
        }

        public void Refresh() {
        }

        public void Start(Storage.IDatabase database) {
        }

        public void Terminate(Storage.IDatabase database) {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DesignTaskViewModel() {
            Text = Guid.NewGuid().ToString();
        }
    }
}
