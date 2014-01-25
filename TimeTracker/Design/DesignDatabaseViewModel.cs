using System.Collections.ObjectModel;

namespace TimeTracker.Design {
    class DesignDatabaseViewModel : IDatabaseViewModel {
        public ObservableCollection<ITaskViewModel> Tasks { get; set; }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public DesignDatabaseViewModel() {
            Tasks = new ObservableCollection<ITaskViewModel> {
                new DesignTaskViewModel(),
                new DesignTaskViewModel(),
                new DesignTaskViewModel(),
            };
        }
    }
}
