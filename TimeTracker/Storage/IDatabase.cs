using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Storage
{
    public enum DatabaseStatusFlag
    {
        LookasideUsed = 0,
        CacheUsed = 1,
        SchemaUsed = 2,
        StatementUsed = 3,
        LookasideHit = 4,
        LookasideMissSize = 5,
        LookasideMissFull = 6,
        CacheHit = 7,
        CacheMiss = 8,
        CacheWrite = 9,
        DeferredForeignKeys = 10,
        MemoryUsed = 1 << 10,
    }

  public enum ConflictBehavior
  {
    /// <summary>
    /// Does not create the table. Equivalent to specifying IF NOT EXISTS in the create table statement.
    /// </summary>
    DoNothing,

    /// <summary>
    /// Throw an exception if the table already exists.
    /// </summary>
    ThrowError,

    /// <summary>
    /// Drop the existing and create the new table. This will delete all data in the existing table.
    /// </summary>
    DropExisting,

    /// <summary>
    /// Update the existing table by creating the new table, copy the existing data to it and then drop the existing table.
    /// </summary>
    UpdateExisting,
  }

    public struct Status
    {
        public readonly long Current;

        public readonly long HighWater;

        internal Status(int current, int highWater)
        {
            Current = current;
            HighWater = highWater;
        }

        internal Status(long current, long highWater)
        {
            Current = current;
            HighWater = highWater;
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}]", Current, HighWater);
        }

        public override bool Equals(object obj)
        {
            var other = (Status)obj;

            return
                Current.Equals(other.Current) &&
                HighWater.Equals(other.HighWater);
        }

        public override int GetHashCode()
        {
            return Current.GetHashCode() ^ HighWater.GetHashCode();
        }
    }

    public interface IDatabase : IDisposable
    {
        long LastInsertRowid { get; }

        //void Execute(string sql);

        void Execute(string sql, params object[] args);

        //IStatement Prepare(string sql);

        IStatement Prepare(string sql, params object[] args);

        Status Status(DatabaseStatusFlag flag, bool reset);

        ITransaction BeginTransaction();

        ITransaction BeginTransaction(TransactionBehavior behavior);

        void CreateTable<T>(/*ConflictBehavior conflictBehavior*/) where T : class;

        void CreateTable(Type tableType/*, ConflictBehavior conflictBehavior*/);
    }
}
