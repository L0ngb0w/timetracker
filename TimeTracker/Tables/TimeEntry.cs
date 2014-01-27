using TimeTracker.Storage.Markup;

namespace TimeTracker.Tables
{
    [Table(IfNotExists = true)]
    [PrimaryKey("EntryId", AutoIncrement = true)]
    [ForeignKey(Columns = new[] { "TaskId" }, ForeignTable = "Task", ForeignColumns = new[] { "TaskId" }, OnDelete = ReferenceAction.Cascade, OnUpdate = ReferenceAction.Cascade)]
    public class TimeEntry
    {
        [Column(NotNull = true, Unique = true, TypeAffinity = TypeAffinity.Integer)]
        public readonly long EntryId;

        [Column(NotNull = true, Unique = false, TypeAffinity = TypeAffinity.Integer)]
        public readonly long TaskId;

        [Column(NotNull = true, TypeAffinity = TypeAffinity.Integer)]
        public long TimeStart;

        [Column(TypeAffinity = TypeAffinity.Integer)]
        public long? TimeEnd;

        [Column(NotNull = true, TypeAffinity = TypeAffinity.Text)]
        public string Text;

        public TimeEntry(long entryId, long taskId, long timeStart, long? timeEnd, string text)
        {
            EntryId = entryId;
            TaskId = taskId;
            TimeStart = timeStart;
            TimeEnd = timeEnd;
            Text = text;
        }
    }
}
