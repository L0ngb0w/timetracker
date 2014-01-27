using TimeTracker.Storage.Markup;

namespace TimeTracker.Tables
{
  [Table(IfNotExists = true)]
  [PrimaryKey("TaskId", AutoIncrement = true)]
  [Index("Date")]
  public class Task
  {
    [Column(NotNull = true, Unique = true, TypeAffinity = TypeAffinity.Integer)]
    public readonly long TaskId;

    [Column(NotNull = true, TypeAffinity = TypeAffinity.Integer)]
    public readonly long Date;

    [Column(NotNull = true, TypeAffinity = TypeAffinity.Text)]
    public string Text;

    public Task(long taskId, long date, string text)
    {
      TaskId = taskId;
      Date = date;
      Text = text;
    }
  }
}
