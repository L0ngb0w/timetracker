using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

using TimeTracker.Tables;

namespace TimeTracker.UnitTest
{
  [TestFixture]
  class TaskViewModelTest
  {
    [Test]
    public void TaskViewModel_TaskIsNull_ThrowException()
    {
      Assert.Throws<ArgumentNullException>(() => new TaskViewModel(null));
    }

    [Test]
    public void Date_DateSetInTask_CorrectDateReturned()
    {
      var date = DateTime.Now;

      var task = new Tables.Task(0, date.ToBinary(), "");
      var viewModel = new TaskViewModel(task);

      Assert.AreEqual(date, viewModel.Date);
    }

    [Test]
    public void TotalTime_NoTimeEntries_EmptyTimeSpanReturned()
    {
      var task = new Tables.Task(0, DateTime.Today.ToBinary(), "");
      var viewModel = new TaskViewModel(task);

      Assert.AreEqual(new TimeSpan(), viewModel.TotalTime);
    }

    [Test]
    public void TotalTime_SomeTimeEntries_TotalTimeEqualSumOfEntries()
    {
      var task = new Tables.Task(0, DateTime.Today.ToBinary(), "");
      var viewModel = new TaskViewModel(task);

      viewModel.TimeEntries.Add(new TimeEntryViewModel(new TimeEntry(0, 0, new DateTime(2013, 01, 01, 01, 15, 0).ToBinary(), new DateTime(2013, 01, 01, 01, 30, 0).ToBinary(), string.Empty))); // 0h 15m 0s
      viewModel.TimeEntries.Add(new TimeEntryViewModel(new TimeEntry(0, 0, new DateTime(2013, 01, 01, 02, 27, 0).ToBinary(), new DateTime(2013, 01, 01, 02, 30, 0).ToBinary(), string.Empty))); // 0h  3m 0s
      viewModel.TimeEntries.Add(new TimeEntryViewModel(new TimeEntry(0, 0, new DateTime(2013, 01, 01, 03, 15, 0).ToBinary(), new DateTime(2013, 01, 01, 04, 30, 0).ToBinary(), string.Empty))); // 1h 15m 0s
      viewModel.TimeEntries.Add(new TimeEntryViewModel(new TimeEntry(0, 0, new DateTime(2013, 01, 01, 01, 15, 0).ToBinary(), new DateTime(2013, 01, 01, 01, 15, 0).ToBinary(), string.Empty))); // 0h  0m 0s
      viewModel.TimeEntries.Add(new TimeEntryViewModel(new TimeEntry(0, 0, new DateTime(2013, 01, 01, 01, 15, 0).ToBinary(), new DateTime(2013, 01, 01, 01, 15, 5).ToBinary(), string.Empty))); // 0h  0m 5s

      var expected = new TimeSpan(0, 1, 33, 5);

      Assert.AreEqual(expected, viewModel.TotalTime);
    }
  }
}
