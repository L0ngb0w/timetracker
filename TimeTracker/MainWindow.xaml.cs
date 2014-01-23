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
        Timer timer;

        readonly IDatabase database;
        readonly Tables.Configuration configuration = new Tables.Configuration();
        readonly BackgroundWorker populate = new BackgroundWorker();

        readonly StatusViewModel status;

        public ObservableCollection<TaskViewModel> Tasks { get; set; }

        public MainWindow() {
            InitializeComponent();

            Tasks = new ObservableCollection<TaskViewModel>();
            Tasks.CollectionChanged += OnTaskCollectionChanged;

            status = new StatusViewModel(Tasks);

            ListEntry.DataContext = this;
            TimeCurrentRounded.DataContext = status;
            TimeCurrentActual.DataContext = status;
            TimeTotoalRounded.DataContext = status;
            TimeTotalActual.DataContext = status;
            TimeFlexRounded.DataContext = status;
            TimeFlexActual.DataContext = status;
            TimeToWorkEnd.DataContext = status;
            CurrentTime.DataContext = status;
            CurrentYear.DataContext = status;
            ButtonPauseEntry.DataContext = status;
            TextBlockGotoLaterDate.DataContext = status;

            var applicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = System.IO.Path.Combine(applicationData, "TimeTracker");

            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            var uri = string.Format("{0}/{1}", path.Replace('\\', '/'), "timetracker.db");
            database = new Storage.Sqlite.SqliteDatabase(uri);

            database.CreateTable<Tables.TimeEntry>();
            database.CreateTable<Tables.Configuration>();

            using (var query = database.Prepare("SELECT WindowWidth, WindowHeight, WindowX, WindowY FROM [Configuration]")) {
                if (query.Step() == StepResult.Row) {
                    configuration.WindowWidth = query.ColumnDouble(0).Value;
                    configuration.WindowHeight = query.ColumnDouble(1).Value;
                    configuration.WindowX = query.ColumnDouble(2).Value;
                    configuration.WindowY = query.ColumnDouble(3).Value;
                } else {
                    configuration.WindowWidth = this.Width;
                    configuration.WindowHeight = this.Height;
                    configuration.WindowX = this.Left;
                    configuration.WindowY = this.Top;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            this.Width = configuration.WindowWidth;
            this.Height = configuration.WindowHeight;
            this.Left = configuration.WindowX;
            this.Top = configuration.WindowY;

            timer = new Timer(1000);
            timer.Elapsed += OnTimerElapsed;
            timer.AutoReset = true;

            DisableForUpdate();

            populate.DoWork += OnPopulateDoWork;
            populate.RunWorkerCompleted += OnPopulateRunWorkerCompleted;
            populate.RunWorkerAsync(database);
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            timer.Elapsed -= OnTimerElapsed;

            using (var transaction = database.BeginTransaction()) {
                using (var statement = database.Prepare("DELETE FROM [Configuration]")) {
                    statement.Step();
                }

                using (var statement = database.Prepare("INSERT INTO [Configuration] (WindowWidth, WindowHeight, WindowX, WindowY) VALUES (@WindowWidth, @WindowHeight, @WindowX, @WindowY)")) {
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
            timer.Dispose();
            database.Dispose();
            populate.Dispose();
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

                status.Refresh();
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

                taskQuery.BindLong("@Date", status.CurrentDate.ToBinary());

                while (taskQuery.Step() == StepResult.Row) {
                    var task = new Tables.Task(taskQuery.ColumnLong(0) ?? -1, taskQuery.ColumnLong(1) ?? -1, taskQuery.ColumnText(2));
                    var entries = QueryTimeEntries(timeEntryQuery, task);

                    Dispatcher.BeginInvoke(new Action(() => {
                        var tvm = new TaskViewModel(task);
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
                    using (var statement = database.Prepare("UPDATE [Task] Set Text = @Text WHERE TaskId = @TaskId")) {
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
                    using (var statement = database.Prepare("UPDATE [TimeEntry] SET TimeStart = @TimeStart, TimeEnd = @TimeEnd, Text = @Text WHERE EntryId = @EntryId")) {
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
            foreach (var task in Tasks.Where(t => t.IsActive)) {
                task.Refresh();
            }

            status.Refresh();
        }

        private void OnButtonNewClicked(object sender, RoutedEventArgs e) {
            using (var transaction = database.BeginTransaction()) {
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
            var date = status.CurrentDate.ToBinary();
            var text = string.Empty;

            using (var statement = database.Prepare("INSERT INTO [Task] (Date, Text) VALUES (@Date, @Text)")) {
                statement.BindLong("@Date", date);
                statement.BindText("@Text", text);

                statement.Step();
            }

            var task = new Tables.Task(database.LastInsertRowid, date, text);
            var taskViewModel = new TaskViewModel(task);
            Tasks.Add(taskViewModel);

            taskViewModel.AddNewTimeEntry(database);
        }

        private void TerminateCurrent() {
            var active = Tasks.SingleOrDefault(t => t.IsActive);
            if (active != null) {
                active.TerminateActiveTimeEntry(database);
            }
        }

        private void DisableForUpdate() {
            timer.Enabled = false;

            ButtonNewEntry.IsEnabled = false;
            ButtonPauseEntry.IsEnabled = false;
            ListEntry.IsEnabled = false;
            TextBlockGotoEarlierDate.IsEnabled = false;
            TextBlockGotoLaterDate.IsEnabled = false;
        }

        private void EnableAfterUpdate() {
            TextBlockGotoEarlierDate.IsEnabled = true;
            TextBlockGotoLaterDate.IsEnabled = status.CanGotoLaterDate;
            ButtonNewEntry.IsEnabled = true;
            ButtonPauseEntry.IsEnabled = true;
            ListEntry.IsEnabled = true;

            timer.Enabled = true;
        }

        private void OnGotoEarlierDate(object sender, RoutedEventArgs e) {
            if (status.IsRunning)
                TerminateCurrent();

            status.CurrentDate = status.CurrentDate.Subtract(TimeSpan.FromDays(1));

            DisableForUpdate();
            populate.RunWorkerAsync(database);
        }

        private void OnGotoLaterDate(object sender, RoutedEventArgs e) {
            GotoLaterDate();
        }

        private void GotoLaterDate() {
            if (status.IsRunning)
                TerminateCurrent();

            status.CurrentDate = status.CurrentDate.Add(TimeSpan.FromDays(1));

            DisableForUpdate();
            populate.RunWorkerAsync(database);
        }
    }
}
