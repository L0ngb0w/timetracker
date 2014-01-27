using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using System.Windows;
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

            mStatus = new StatusViewModel(Tasks);

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
            mDatabaseViewModel = new DatabaseViewModel(mDatabase, Tasks);
            ListEntry.DataContext = mDatabaseViewModel;

            mDatabase.CreateTable<Tables.Task>();
            mDatabase.CreateTable<Tables.TimeEntry>();
            mDatabase.CreateTable<Tables.Configuration>();

            using (var query = mDatabase.Prepare("SELECT WindowWidth, WindowHeight, WindowX, WindowY FROM [Configuration]")) {
                if (query.Step() == StepResult.Row) {
                    mConfiguration.WindowWidth = query.ColumnDouble(0) ?? 400;
                    mConfiguration.WindowHeight = query.ColumnDouble(1) ?? 300;
                    mConfiguration.WindowX = query.ColumnDouble(2) ?? 0;
                    mConfiguration.WindowY = query.ColumnDouble(3) ?? 0;
                } else {
                    mConfiguration.WindowWidth = Width;
                    mConfiguration.WindowHeight = Height;
                    mConfiguration.WindowX = Left;
                    mConfiguration.WindowY = Top;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Width = mConfiguration.WindowWidth;
            Height = mConfiguration.WindowHeight;
            Left = mConfiguration.WindowX;
            Top = mConfiguration.WindowY;

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
                    statement.BindDouble("@WindowWidth", ActualWidth);
                    statement.BindDouble("@WindowHeight", ActualHeight);
                    statement.BindDouble("@WindowX", Left);
                    statement.BindDouble("@WindowY", Top);

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
                    MessageBox.Show(this, exception.Message, Title + " - Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

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
                timeEntryQuery = database.Prepare("SELECT EntryId, TimeStart, TimeEnd, Text FROM [TimeEntry] WHERE TaskId = @TaskId ORDER BY EntryId, TimeStart, TimeEnd");

                taskQuery.BindLong("@Date", mStatus.CurrentDate.ToBinary());

                while (taskQuery.Step() == StepResult.Row) {
                    var task = new Tables.Task(taskQuery.ColumnLong(0) ?? -1, taskQuery.ColumnLong(1) ?? -1, taskQuery.ColumnText(2));
                    var entries = QueryTimeEntries(timeEntryQuery, task);

                    Dispatcher.BeginInvoke(new Action(() => {
                        var tvm = new TaskViewModel(mDatabaseViewModel, task);
                        Tasks.Add(tvm);

                        foreach (var entry in entries) {
                            tvm.Intervals.Add(new IntervalViewModel(mDatabaseViewModel, entry));
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

        void OnTimerElapsed(object sender, ElapsedEventArgs e) {
            var active = Tasks.SingleOrDefault(t => t.IsActive);
            if (active == null)
                return;

            active.Refresh();
            mStatus.Refresh();
        }

        private void OnButtonNewClicked(object sender, RoutedEventArgs e) {
            using (var transaction = mDatabase.BeginTransaction()) {
                mDatabaseViewModel.Terminate();
                mDatabaseViewModel.Start(mStatus.CurrentDate);

                transaction.Commit();
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
            ChangeDate(-1);
        }

        private void OnGotoLaterDate(object sender, RoutedEventArgs e) {
            ChangeDate(1);
        }

        private void ChangeDate(double days) {
            mDatabaseViewModel.Terminate();

            mStatus.CurrentDate = mStatus.CurrentDate.Add(TimeSpan.FromDays(days));

            DisableForUpdate();
            mPopulate.RunWorkerAsync(mDatabase);
        }
    }
}
