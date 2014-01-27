using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TimeTracker.Storage;

namespace TimeTracker {
    public interface IDatabaseViewModel : INotifyPropertyChanged {
        IDatabase Database { get; }

        ObservableCollection<ITaskViewModel> Tasks { get; }

        void Start(DateTime date);

        void Terminate();
    }

    public class DatabaseViewModel : IDatabaseViewModel {
        readonly IDatabase mDatabase;
        readonly ObservableCollection<ITaskViewModel> mTasks;

        public IDatabase Database { get { return mDatabase; } }

        public ObservableCollection<ITaskViewModel> Tasks { get { return mTasks; } }

        public event PropertyChangedEventHandler PropertyChanged;

        public DatabaseViewModel(IDatabase database, ObservableCollection<ITaskViewModel> tasks) {
            if (database == null)
                throw new ArgumentNullException("database");

            if (tasks == null)
                throw new ArgumentNullException("tasks");

            mDatabase = database;
            mTasks = tasks;
        }

        public void Start(DateTime date) {
            var taskViewModel = new TaskViewModel(this, date);
            Tasks.Add(taskViewModel);

            taskViewModel.Start();
        }

        public void Terminate() {
            var active = Tasks.SingleOrDefault(t => t.IsActive);
            if (active != null)
                active.Terminate();
        }
    }
}
