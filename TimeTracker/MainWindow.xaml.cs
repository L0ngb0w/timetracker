using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TimeTracker.Storage;

namespace TimeTracker {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        Timer mTimer;

        readonly IDatabase mDatabase;
        readonly Tables.Configuration mConfiguration = new Tables.Configuration();
        readonly BackgroundWorker mPopulate = new BackgroundWorker();

        readonly IStatusViewModel mStatus;
        readonly IDatabaseViewModel mDatabaseViewModel;

        public ObservableCollection<ITaskViewModel> Tasks { get; set; }

        public MainWindow() {
            InitializeComponent();

            Tasks = new ObservableCollection<ITaskViewModel>();
            Tasks.CollectionChanged += OnTaskCollectionChanged;

            mStatus = new StatusViewModel(Tasks);
            mDatabaseViewModel = new DatabaseViewModel(Tasks);

            ListEntry.DataContext = mDatabaseViewModel;
            TimeCurrentRounded.DataContext = mStatus;
            TimeCurrentActual.DataContext = mStatus;
            TimeTotoalRounded.DataContext = mStatus;
            TimeTotalActual.DataContext = mStatus;
            TimeFlexRounded.DataContext = mStatus;
            TimeFlexActual.DataContext = mStatus;
            TimeToWorkEnd.DataContext = mStatus;
            CurrentTime.DataContext = mStatus;
            CurrentYear.DataContext = mStatus;
            ButtonPauseEntry.DataContext = mStatus;
            TextBlockGotoLaterDate.DataContext = mStatus;

            var applicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = System.IO.Path.Combine(applicationData, "TimeTracker");

            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            var uri = string.Format("{0}/{1}", path.Replace('\\', '/'), "timetracker.db");
            mDatabase = new Storage.Sqlite.SqliteDatabase(uri);

            mDatabase.CreateTable<Tables.Task>();
            mDatabase.CreateTable<Tables.TimeEntry>();
            mDatabase.CreateTable<Tables.Configuration>();

            using (var query = mDatabase.Prepare("SELECT WindowWidth, WindowHeight, WindowX, WindowY FROM [Configuration]")) {
                if (query.Step() == StepResult.Row) {
                    mConfiguration.WindowWidth = query.ColumnDouble(0).Value;
                    mConfiguration.WindowHeight = query.ColumnDouble(1).Value;
                    mConfiguration.WindowX = query.ColumnDouble(2).Value;
                    mConfiguration.WindowY = query.ColumnDouble(3).Value;
                } else {
                    mConfiguration.WindowWidth = this.Width;
                    mConfiguration.WindowHeight = this.Height;
                    mConfiguration.WindowX = this.Left;
                    mConfiguration.WindowY = this.Top;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            this.Width = mConfiguration.WindowWidth;
            this.Height = mConfiguration.WindowHeight;
            this.Left = mConfiguration.WindowX;
            this.Top = mConfiguration.WindowY;

            mTimer = new Timer(1000);
            mTimer.Elapsed += OnTimerElapsed;
            mTimer.AutoReset = true;

            DisableForUpdate();

            mPopulate.DoWork += OnPopulateDoWork;
            mPopulate.RunWorkerCompleted += OnPopulateRunWorkerCompleted;
            mPopulate.RunWorkerAsync(mDatabase);
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            mTimer.Elapsed -= OnTimerElapsed;

            using (var transaction = mDatabase.BeginTransaction()) {
                using (var statement = mDatabase.Prepare("DELETE FROM [Configuration]")) {
                    statement.Step();
                }

                using (var statement = mDatabase.Prepare("INSERT INTO [Configuration] (WindowWidth, WindowHeight, WindowX, WindowY) VALUES (@WindowWidth, @WindowHeight, @WindowX, @WindowY)")) {
                    //<<<<<<< HEAD
                    //            Dispatcher.BeginInvoke(new Action(() =>
                    //            {
                    //                var exception = e.Result as Exception;
                    //                if (exception != null)
                    //                {
                    //                    MessageBox.Show(this, exception.Message, this.Title + " - Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    //                }

                    //                status.Refresh();
                    //                EnableAfterUpdate();
                    //            }));
                    //        }
                    //=======
                    statement.BindDouble("@WindowWidth", this.ActualWidth);
                    statement.BindDouble("@WindowHeight", this.ActualHeight);
                    statement.BindDouble("@WindowX", this.Left);
                    statement.BindDouble("@WindowY", this.Top);
                    //>>>>>>> 0204f5aa289eebe8e1ef36f5dc5e290cd9278d56

                    statement.Step();
                }

                transaction.Commit();
            }
        }

        private void Window_Closed(object sender, EventArgs e) {
            mTimer.Dispose();
            mDatabase.Dispose();
            mPopulate.Dispose();
        }

        void OnPopulateRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            Dispatcher.BeginInvoke(new Action(() => {
                var exception = e.Result as Exception;
                if (exception != null) {
                    MessageBox.Show(this, exception.Message, this.Title + " - Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                //if (timeEntries.Any())
                //    status.IsRunning = timeEntries.Last().EndTime.HasValue;
                //else
                //    status.IsRunning = false;

                mStatus.Refresh();
                EnableAfterUpdate();
            }));
        }

        void OnPopulateDoWork(object sender, DoWorkEventArgs e) {
            IStatement taskQuery = null;
            IStatement timeEntryQuery = null;

            try {
                Dispatcher.BeginInvoke(new Action(() => Tasks.Clear()));

                var database = (IDatabase)e.Argument;

                taskQuery = database.Prepare("SELECT TaskId, Date, Text FROM [Task] WHERE Date = @Date ORDER BY Date");
                timeEntryQuery = database.Prepare("SELECT EntryId, TimeStart, TimeEnd, Text FROM [TimeEntry] WHERE TaskId = @TaskId ORDER BY TimeStart, TimeEnd");

                taskQuery.BindLong("@Date", mStatus.CurrentDate.ToBinary());

                while (taskQuery.Step() == StepResult.Row) {
                    var task = new Tables.Task(taskQuery.ColumnLong(0) ?? -1, taskQuery.ColumnLong(1) ?? -1, taskQuery.ColumnText(2));
                    var entries = QueryTimeEntries(timeEntryQuery, task);

                    Dispatcher.BeginInvoke(new Action(() => {
                        var tvm = new TaskViewModel(mDatabase, task);
                        Tasks.Add(tvm);

                        foreach (var entry in entries) {
                            tvm.TimeEntries.Add(new TimeEntryViewModel(entry));
                        }
                    }));
                }
            } catch (Exception ex) {
                e.Result = ex;
            } finally {
                if (taskQuery != null)
                    taskQuery.Dispose();

                if (timeEntryQuery != null)
                    timeEntryQuery.Dispose();
            }
        }

        static IEnumerable<Tables.TimeEntry> QueryTimeEntries(IStatement query, Tables.Task task) {
            var entries = new List<Tables.TimeEntry>();

            query.Reset();
            query.BindLong("@TaskId", task.TaskId);

            while (query.Step() == StepResult.Row) {
                var entry = new Tables.TimeEntry(query.ColumnLong(0) ?? -1, task.TaskId, query.ColumnLong(1) ?? -1, query.ColumnLong(2), query.ColumnText(3));
                entries.Add(entry);
            }

            return entries;
        }

        void OnTaskCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.NewItems == null)
                return;

            foreach (TaskViewModel item in e.NewItems) {
                item.PropertyChanged += OnTaskPropertyChanged;
                item.TimeEntries.CollectionChanged += OnTimeEntryCollectionChanged;
            }
        }

        void OnTimeEntryCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.NewItems == null)
                return;

            foreach (TimeEntryViewModel item in e.NewItems) {
                item.PropertyChanged += OnTimeEntryPropertyChanged;
            }
        }

        void OnTaskPropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "Text":
                    var task = (TaskViewModel)sender;
                    using (var statement = mDatabase.Prepare("UPDATE [Task] Set Text = @Text WHERE TaskId = @TaskId")) {
                        statement.BindLong("@TaskId", task.Task.TaskId);
                        statement.BindText("@Text", task.Task.Text ?? string.Empty);

                        statement.Step();
                    }
                    break;
            }
        }

        void OnTimeEntryPropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "Text":
                case "TimeStart":
                case "TimeEnd":
                    var entry = (TimeEntryViewModel)sender;
                    using (var statement = mDatabase.Prepare("UPDATE [TimeEntry] SET TimeStart = @TimeStart, TimeEnd = @TimeEnd, Text = @Text WHERE EntryId = @EntryId")) {
                        statement.BindLong("@EntryId", entry.Entry.EntryId);
                        statement.BindLong("@TimeStart", entry.Entry.TimeStart);
                        statement.BindLong("@TimeEnd", entry.Entry.TimeEnd);
                        statement.BindText("@Text", entry.Entry.Text ?? string.Empty);

                        statement.Step();
                    }
                    break;
            }
        }

        void OnTimerElapsed(object sender, ElapsedEventArgs e) {
            var active = Tasks.SingleOrDefault(t => t.IsActive);
            if (active == null)
                return;

            active.Refresh();
            mStatus.Refresh();
        }

        private void OnButtonNewClicked(object sender, RoutedEventArgs e) {
            using (var transaction = mDatabase.BeginTransaction()) {
                TerminateCurrent();
                AddNewTask();

                transaction.Commit();
            }
        }

        //private void OnButtonPauseClicked(object sender, RoutedEventArgs e) {
        //    using (var transaction = database.BeginTransaction()) {
        //        if (status.IsRunning)
        //            TerminateCurrent();
        //        else
        //            AddNewEntry();

        //        transaction.Commit();
        //    }
        //}

        private void AddNewTask() {
            var date = mStatus.CurrentDate.ToBinary();
            var text = Guid.NewGuid().ToString(); //string.Empty;

            using (var statement = mDatabase.Prepare("INSERT INTO [Task] (Date, Text) VALUES (@Date, @Text)")) {
                statement.BindLong("@Date", date);
                statement.BindText("@Text", text);

                statement.Step();
            }

            var task = new Tables.Task(mDatabase.LastInsertRowid, date, text);
            var taskViewModel = new TaskViewModel(mDatabase, task);
            Tasks.Add(taskViewModel);

            taskViewModel.Start();
        }

        private void TerminateCurrent() {
            var active = Tasks.SingleOrDefault(t => t.IsActive);
            if (active != null) {
                active.Terminate();
            }
        }

        private void DisableForUpdate() {
            mTimer.Enabled = false;

            ButtonNewEntry.IsEnabled = false;
            ButtonPauseEntry.IsEnabled = false;
            ListEntry.IsEnabled = false;
            TextBlockGotoEarlierDate.IsEnabled = false;
            TextBlockGotoLaterDate.IsEnabled = false;
        }

        private void EnableAfterUpdate() {
            TextBlockGotoEarlierDate.IsEnabled = true;
            TextBlockGotoLaterDate.IsEnabled = mStatus.CanGotoLaterDate;
            ButtonNewEntry.IsEnabled = true;
            ButtonPauseEntry.IsEnabled = true;
            ListEntry.IsEnabled = true;

            mTimer.Enabled = true;
        }

        private void OnGotoEarlierDate(object sender, RoutedEventArgs e) {
            if (mStatus.IsRunning)
                TerminateCurrent();

            mStatus.CurrentDate = mStatus.CurrentDate.Subtract(TimeSpan.FromDays(1));

            DisableForUpdate();
            mPopulate.RunWorkerAsync(mDatabase);
        }

        private void OnGotoLaterDate(object sender, RoutedEventArgs e) {
            GotoLaterDate();
        }

        private void GotoLaterDate() {
            if (mStatus.IsRunning)
                TerminateCurrent();

            mStatus.CurrentDate = mStatus.CurrentDate.Add(TimeSpan.FromDays(1));

            DisableForUpdate();
            mPopulate.RunWorkerAsync(mDatabase);
        }
    }
}
