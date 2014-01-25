﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TimeTracker.Design {
    class DesignTaskViewModel : ITaskViewModel {
        ObservableCollection<ITimeEntryViewModel> mEntries;

        public string Text { get; set; }

        public DateTime Date {
            get { return DateTime.Today; }
        }

        public ObservableCollection<ITimeEntryViewModel> TimeEntries {
            get { return mEntries; }
        }

        public TimeSpan TotalTime {
            get { return TimeSpan.FromMinutes(666); }
        }

        public bool IsActive {
            get { return true; }
        }

        public void Refresh() {
        }

        public void Start() {
        }

        public void Terminate() {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DesignTaskViewModel() {
            Text = Guid.NewGuid().ToString();

            mEntries = new ObservableCollection<ITimeEntryViewModel> {
                new DesignTimeEntryViewModel(),
                new DesignTimeEntryViewModel(),
                new DesignTimeEntryViewModel(),
            };
        }
    }
}
