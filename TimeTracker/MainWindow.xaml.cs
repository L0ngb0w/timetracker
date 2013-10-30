using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace TimeTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Timer timer;
        IDatabase database;
        Tables.Configuration configuration = new Tables.Configuration();
        BackgroundWorker populate = new BackgroundWorker();

        StatusViewModel status;
        public ObservableCollection<TimeEntryViewModel> TimeEntries { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            TimeEntries = new ObservableCollection<TimeEntryViewModel>();
            TimeEntries.CollectionChanged += OnTimeEntryCollectionChanged;

            status = new StatusViewModel(TimeEntries);

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

            using (var query = database.Prepare("SELECT WindowWidth, WindowHeight, WindowX, WindowY FROM [Configuration]"))
            {
                if (query.Step() == StepResult.Row)
                {
                    configuration.WindowWidth = query.ColumnDouble(0).Value;
                    configuration.WindowHeight = query.ColumnDouble(1).Value;
                    configuration.WindowX = query.ColumnDouble(2).Value;
                    configuration.WindowY = query.ColumnDouble(3).Value;
                }
                else
                {
                    configuration.WindowWidth = this.Width;
                    configuration.WindowHeight = this.Height;
                    configuration.WindowX = this.Left;
                    configuration.WindowY = this.Top;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            timer.Elapsed -= OnTimerElapsed;

            using (var transaction = database.BeginTransaction())
            {
                using (var statement = database.Prepare("DELETE FROM [Configuration]"))
                {
                    statement.Step();
                }

                using (var statement = database.Prepare("INSERT INTO [Configuration] (WindowWidth, WindowHeight, WindowX, WindowY) VALUES (@WindowWidth, @WindowHeight, @WindowX, @WindowY)"))
                {
                    statement.BindDouble("@WindowWidth", this.ActualWidth);
                    statement.BindDouble("@WindowHeight", this.ActualHeight);
                    statement.BindDouble("@WindowX", this.Left);
                    statement.BindDouble("@WindowY", this.Top);

                    statement.Step();
                }

                transaction.Commit();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            timer.Dispose();
            database.Dispose();
            populate.Dispose();
        }

        void OnPopulateRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var exception = e.Result as Exception;
                if (exception != null)
                {
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

        void OnPopulateDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Dispatcher.BeginInvoke(new Action(() => TimeEntries.Clear()));

                var database = (IDatabase)e.Argument;
                using (var query = database.Prepare("SELECT EntryId, Date, TimeStart, TimeEnd, Text FROM [TimeEntry] WHERE Date = @Date ORDER BY TimeStart, TimeEnd"))
                {
                    query.BindLong("@Date", status.CurrentDate.ToBinary());

                    StepResult result;
                    while ((result = query.Step()) == StepResult.Row)
                    {
                        var entry = new Tables.TimeEntry(query.ColumnLong(0).Value, query.ColumnLong(1).Value, query.ColumnLong(2).Value, query.ColumnLong(3), query.ColumnText(4));
                        Dispatcher.BeginInvoke(new Action(() => TimeEntries.Add(new TimeEntryViewModel(entry))));
                    }
                }
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        void OnTimeEntryCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (TimeEntryViewModel item in e.NewItems)
                {
                    item.PropertyChanged += OnTimeEntryPropertyChanged;
                }
            }
        }

        void OnTimeEntryPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var entry = (TimeEntryViewModel)sender;
            using (var statement = database.Prepare("UPDATE [TimeEntry] SET TimeStart = @TimeStart, TimeEnd = @TimeEnd, Text = @Text WHERE EntryId = @EntryId"))
            {
                statement.BindLong("@EntryId", entry.Entry.EntryId);
                statement.BindLong("@TimeStart", entry.Entry.TimeStart);
                statement.BindLong("@TimeEnd", entry.Entry.TimeEnd);
                statement.BindText("@Text", entry.Entry.Text ?? string.Empty);

                statement.Step();
            }
        }

        void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var entry in TimeEntries)
            {
                entry.Refresh();
            }

            status.Refresh();
        }

        private void OnButtonNewClicked(object sender, RoutedEventArgs e)
        {
            TerminateCurrent();
            AddNewEntry(false);
        }

        private void AddNewEntry(bool isContinue)
        {
            var time = TimeService.Time;
            var date = status.CurrentDate.ToBinary();
            var startTime = time.ToBinary();

            string text = isContinue ? TimeEntries.Last().Text : string.Empty;

            using (var statement = database.Prepare("INSERT INTO [TimeEntry] (Date, TimeStart, Text) VALUES (@Date, @TimeStart, @Text)"))
            {
                statement.BindLong("@Date", date);
                statement.BindLong("@TimeStart", startTime);
                statement.BindText("@Text", text);

                statement.Step();
            }

            var entry = new Tables.TimeEntry(database.LastInsertRowid, date, startTime, (long?)null, text);
            TimeEntries.Add(new TimeEntryViewModel(entry));
        }

        private void TerminateCurrent()
        {
            var time = TimeService.Time;
            if (TimeEntries.Any())
            {
                var last = TimeEntries.Last();
                if (!last.EndTime.HasValue)
                {
                    last.EndTime = time;
                }
            }
        }

        private void OnButtonPauseClicked(object sender, RoutedEventArgs e)
        {
            if (status.IsRunning)
                TerminateCurrent();
            else
                AddNewEntry(true);
        }

        private void DisableForUpdate()
        {
            timer.Enabled = false;

            ButtonNewEntry.IsEnabled = false;
            ButtonPauseEntry.IsEnabled = false;
            ListEntry.IsEnabled = false;
            TextBlockGotoEarlierDate.IsEnabled = false;
            TextBlockGotoLaterDate.IsEnabled = false;
        }

        private void EnableAfterUpdate()
        {
            TextBlockGotoEarlierDate.IsEnabled = true;
            TextBlockGotoLaterDate.IsEnabled = status.CanGotoLaterDate;
            ButtonNewEntry.IsEnabled = true;
            ButtonPauseEntry.IsEnabled = true;
            ListEntry.IsEnabled = true;

            timer.Enabled = true;
        }

        private void OnGotoEarlierDate(object sender, RoutedEventArgs e)
        {
            if (status.IsRunning)
                TerminateCurrent();

            status.CurrentDate = status.CurrentDate.Subtract(TimeSpan.FromDays(1));

            DisableForUpdate();
            populate.RunWorkerAsync(database);
        }

        private void OnGotoLaterDate(object sender, RoutedEventArgs e)
        {
            GotoLaterDate();
        }

        private void GotoLaterDate()
        {
            if (status.IsRunning)
                TerminateCurrent();

            status.CurrentDate = status.CurrentDate.Add(TimeSpan.FromDays(1));

            DisableForUpdate();
            populate.RunWorkerAsync(database);
        }
    }
}
