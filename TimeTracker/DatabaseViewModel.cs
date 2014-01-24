using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace TimeTracker {
    interface IDatabaseViewModel : INotifyPropertyChanged {
        ObservableCollection<ITaskViewModel> Tasks { get; }
    }

    class DatabaseViewModel : IDatabaseViewModel {
        readonly ObservableCollection<ITaskViewModel> mTasks;

        public ObservableCollection<ITaskViewModel> Tasks { get { return mTasks; } }

        public event PropertyChangedEventHandler PropertyChanged;

        public DatabaseViewModel(ObservableCollection<ITaskViewModel> tasks) {
            mTasks = tasks;
        }
    }
}
