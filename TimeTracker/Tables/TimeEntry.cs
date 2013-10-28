using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Storage.Markup;

namespace TimeTracker.Tables
{
    [Table(IfNotExists = true)]
    [PrimaryKey("EntryId", AutoIncrement = true)]
    [Index("Date")]
    public class TimeEntry
    {
        [Column(NotNull = true, Unique = true, TypeAffinity = TypeAffinity.Integer)]
        public readonly long EntryId;

        [Column(NotNull = true, TypeAffinity = TypeAffinity.Integer)]
        public readonly long Date;

        [Column(NotNull = true, TypeAffinity = TypeAffinity.Integer)]
        public long TimeStart;

        [Column(TypeAffinity = TypeAffinity.Integer)]
        public long? TimeEnd;

        [Column(NotNull = true, TypeAffinity = TypeAffinity.Text)]
        public string Text;

        public TimeEntry(long entryId, long date, long timeStart, long? timeEnd, string text)
        {
            EntryId = entryId;
            Date = date;
            TimeStart = timeStart;
            TimeEnd = timeEnd;
            Text = text;
        }
    }
}
