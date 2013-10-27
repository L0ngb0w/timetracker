using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Storage.Sqlite
{
    public class SqliteTransaction : ITransaction
    {
        const string begin = "BEGIN {0} TRANSACTION";
        const string rollback = "ROLLBACK";
        const string commit = "COMMIT";

        enum State
        {
            Unknown, Commit, Rollback,
        }

        IDatabase database;
        State state = State.Unknown;

        internal SqliteTransaction(IDatabase database)
        {
            if (database == null)
                throw new ArgumentNullException("database");

            this.database = database;
            database.Execute(string.Format(begin, string.Empty));
        }

        internal SqliteTransaction(IDatabase database, TransactionBehavior behavior)
        {
            if (database == null)
                throw new ArgumentNullException("database");

            this.database = database;

            string b;
            switch (behavior)
            {
                case TransactionBehavior.Deferred:
                    b = "DEFERRED";
                    break;
                case TransactionBehavior.Exclusive:
                    b = "EXCLUSIVE";
                    break;
                case TransactionBehavior.Immediate:
                    b = "IMMEDIATE";
                    break;
                default:
                    throw new ArgumentException(string.Format("Unsupported transaction behavior: {0}", behavior), "behavior");
            }

            database.Execute(begin, b);
        }

        public void Dispose()
        {
            if (database != null)
            {
                if (state == State.Commit)
                    database.Execute(commit);
                else
                    database.Execute(rollback);

                database = null;
            }
        }

        public void Rollback()
        {
            if (state == State.Commit)
                throw new Exception("Attempt to roll back transaction after it has been committed");

            state = State.Rollback;
        }

        public void Commit()
        {
            if (state == State.Rollback)
                throw new Exception("Attempt to commit transaction after it has been rolled back");

            state = State.Commit;
        }
    }
}
