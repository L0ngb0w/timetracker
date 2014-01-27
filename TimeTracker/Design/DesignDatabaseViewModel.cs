using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using TimeTracker.Storage;

namespace TimeTracker.Design {
    class DesignDatabaseViewModel : IDatabaseViewModel {
        public IDatabase Database { get { return null; } }

        public ObservableCollection<ITaskViewModel> Tasks { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public DesignDatabaseViewModel() {
            Tasks = new ObservableCollection<ITaskViewModel> {
                new DesignTaskViewModel(),
                new DesignTaskViewModel(),
                new DesignTaskViewModel(),
            };
        }

        public void Start(DateTime date) {
        }

        public void Terminate() {
        }
    }
}
