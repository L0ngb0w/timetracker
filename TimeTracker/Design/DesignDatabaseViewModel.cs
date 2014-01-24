using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace TimeTracker.Design {
    class DesignDatabaseViewModel : IDatabaseViewModel {
        public ObservableCollection<ITaskViewModel> Tasks { get; set; }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public DesignDatabaseViewModel() {
            var date = DateTime.Today.ToBinary();

            Tasks = new ObservableCollection<ITaskViewModel> {
                new TaskViewModel(new Tables.Task(0, date, "Task 1")),
                new TaskViewModel(new Tables.Task(1, date, "Task 2")),
                new TaskViewModel(new Tables.Task(2, date, "Task 3")),
            };
        }
    }
}
