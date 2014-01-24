using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.UnitTest
{
    //[TestFixture]
    //class StatusViewModelTest
    //{
    //    [Test]
    //    public void CurrentTimeActual_EmptyCollection_EmptyTimeSpanReturned()
    //    {
    //        var entries = new ObservableCollection<TaskViewModel>();
    //        var instance = new StatusViewModel(entries);

    //        var expected = new TimeSpan();

    //        Assert.AreEqual(expected, instance.CurrentTimeActual);
    //    }

    //    [TestCase(0, 0, 0, 0, 0, 0)]
    //    [TestCase(0, 30, 1, 30, 1, 0)]
    //    public void CurrentTimeActual_CollectionWithOneEntry_CorrectTimeDifference(int startHours, int startMinutes, int endHours, int endMinutes, int diffHours, int diffMinutes)
    //    {
    //        var timeEntries = new[] {
    //            new Tables.TimeEntry(1, 1, new DateTime(2013, 01, 01, startHours, startMinutes, 0).ToBinary(), new DateTime(2013, 01, 01, endHours, endMinutes, 0).ToBinary(), string.Empty)
    //        };

    //        var tasks = new[] {
    //            new Tables.Task(10, new DateTime(2013, 01, 01).ToBinary(), string.Empty)
    //        };

    //        var entries = new ObservableCollection<TaskViewModel>(tasks.Select(t => new TaskViewModel(t)));
    //        var instance = new StatusViewModel(entries);

    //        var expected = new TimeSpan(diffHours, diffMinutes, 0);

    //        Assert.AreEqual(expected, instance.CurrentTimeActual);
    //    }

    //    [Test]
    //    public void CurrentTimeActual_CollectionWithMoreEntries_LastTimeDifferenceReturned()
    //    {
    //        var timeEntries = new[] {
    //            new Tables.TimeEntry(1, 1, new DateTime(2013, 01, 01, 0, 15, 0).ToBinary(), new DateTime(2013, 01, 01, 0, 30, 0).ToBinary(), string.Empty),
    //            new Tables.TimeEntry(1, 1, new DateTime(2013, 01, 01, 0, 30, 0).ToBinary(), new DateTime(2013, 01, 01, 1, 00, 0).ToBinary(), string.Empty),
    //            new Tables.TimeEntry(1, 1, new DateTime(2013, 01, 01, 1, 00, 0).ToBinary(), new DateTime(2013, 01, 01, 2, 15, 0).ToBinary(), string.Empty),
    //        };

    //        var entries = new ObservableCollection<TimeEntryViewModel>(timeEntries.Select(e => new TimeEntryViewModel(e)));
    //        var instance = new StatusViewModel(entries);

    //        var expected = new TimeSpan(1, 15, 0);

    //        Assert.AreEqual(expected, instance.CurrentTimeActual);
    //    }

    //    [Test]
    //    public void CurrentTimeRounded_EmptyCollection_EmptyTimeSpanReturned()
    //    {
    //        var entries = new ObservableCollection<TimeEntryViewModel>();
    //        var instance = new StatusViewModel(entries);

    //        var expected = new TimeSpan();

    //        Assert.AreEqual(expected, instance.CurrentTimeRounded);
    //    }

    //    [TestCase(0, 00, 0, 00, 0, 00)]
    //    [TestCase(0, 00, 0, 15, 0, 15)]
    //    [TestCase(0, 00, 0, 30, 0, 30)]
    //    [TestCase(0, 00, 0, 45, 0, 45)]
    //    [TestCase(0, 00, 1, 00, 1, 00)]
    //    [TestCase(0, 00, 0, 05, 0, 00)]
    //    [TestCase(0, 00, 0, 20, 0, 15)]
    //    [TestCase(0, 00, 0, 35, 0, 30)]
    //    [TestCase(0, 00, 0, 50, 0, 45)]
    //    [TestCase(0, 00, 1, 05, 1, 00)]
    //    [TestCase(0, 00, 0, 10, 0, 15)]
    //    [TestCase(0, 00, 0, 25, 0, 30)]
    //    [TestCase(0, 00, 0, 40, 0, 45)]
    //    [TestCase(0, 00, 0, 55, 1, 00)]
    //    [TestCase(0, 15, 0, 00, 0, -15)]
    //    [TestCase(0, 30, 0, 00, 0, -30)]
    //    [TestCase(0, 45, 0, 00, 0, -45)]
    //    [TestCase(1, 00, 0, 00, -1, 00)]
    //    [TestCase(0, 05, 0, 00, 0, 00)]
    //    [TestCase(0, 20, 0, 00, 0, -15)]
    //    [TestCase(0, 35, 0, 00, 0, -30)]
    //    [TestCase(0, 50, 0, 00, 0, -45)]
    //    [TestCase(1, 05, 0, 00, -1, 00)]
    //    [TestCase(0, 10, 0, 00, 0, -15)]
    //    [TestCase(0, 25, 0, 00, 0, -30)]
    //    [TestCase(0, 40, 0, 00, 0, -45)]
    //    [TestCase(0, 55, 0, 00, -1, 00)]
    //    public void CurrentTimeRounded_IntervalsWithDifferentLength_CorrectlyRoundedIntervalReturned(int startHours, int startMinutes, int endHours, int endMinutes, int diffHours, int diffMinutes)
    //    {
    //        var timeEntries = new[] {
    //            new Tables.TimeEntry(1, 1, new DateTime(2013, 01, 01, startHours, startMinutes, 0).ToBinary(), new DateTime(2013, 01, 01, endHours, endMinutes, 0).ToBinary(), string.Empty)
    //        };

    //        var entries = new ObservableCollection<TimeEntryViewModel>(timeEntries.Select(e => new TimeEntryViewModel(e)));
    //        var instance = new StatusViewModel(entries);

    //        var expected = new TimeSpan(diffHours, diffMinutes, 0);

    //        Assert.AreEqual(expected, instance.CurrentTimeRounded);
    //    }
    //}
}
