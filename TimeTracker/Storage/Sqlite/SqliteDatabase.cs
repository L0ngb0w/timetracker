using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Storage.Markup;

namespace TimeTracker.Storage.Sqlite
{
    public enum Location
    {
        Memory,
        Temporary,
    }

    public class SqliteDatabase : IDatabase
    {
        ISqlite3 mSqlite3;
        IntPtr mDatabase = IntPtr.Zero;

        public long LastInsertRowid
        {
            get { return mSqlite3.LastInsertRowid(mDatabase); }
        }

        SqliteDatabase(ISqlite3 sqlite3)
        {
            if (sqlite3 == null)
                throw new ArgumentNullException("sqlite3");

            this.mSqlite3 = sqlite3;
        }

        internal SqliteDatabase(Location location, ISqlite3 sqlite3)
            : this(sqlite3)
        {
            string file;
            switch (location)
            {
                case Location.Memory:
                    file = ":memory:";
                    break;
                case Location.Temporary:
                    file = string.Empty;
                    break;
                default:
                    throw new ArgumentException(string.Format("Unsupported location: {0}", location));
            }

            Result result = mSqlite3.Open16(file, out mDatabase);
            if (result != Result.Ok)
            {
                mSqlite3.Close(mDatabase);
                throw new Exception("Could not open Sqlite database: " + result.ToString());
            }

            // Explicitly enable foreign key constraints since currently Sqlite3 defaults to disabling then
            Execute("PRAGMA foreign_keys = ON");
        }

        internal SqliteDatabase(string filename, ISqlite3 sqlite3)
            : this(sqlite3)
        {
            Result result = mSqlite3.Open16(filename, out mDatabase);
            if (result != Result.Ok)
            {
                var errMsg = mSqlite3.Errmsg16(mDatabase);

                mSqlite3.Close(mDatabase);
                throw new Exception("Could not open Sqlite database: " + errMsg);
            }

            // Explicitly enable foreign key constraints since currently Sqlite3 defaults to disabling then
            Execute("PRAGMA foreign_keys = ON");
        }

        public SqliteDatabase(Location location)
            : this(location, new Sqlite3())
        {
        }

        public SqliteDatabase(string filename)
            : this(filename, new Sqlite3())
        {
        }

        public void Dispose()
        {
            mSqlite3.Close(mDatabase);
            mDatabase = IntPtr.Zero;
        }

        public void Execute(string sql)
        {
            IntPtr errMsg = IntPtr.Zero;
            Result result = mSqlite3.Exec(mDatabase, sql, out errMsg);
            if (result != Result.Ok)
            {
                var message = Marshal.PtrToStringAnsi(errMsg);
                mSqlite3.Free(errMsg);

                throw new SqlException(string.Format("{0} when executing statement: {1}", result, message), sql);
            }
        }

        public void Execute(string sql, params object[] args)
        {
            Execute(string.Format(sql, args));
        }

        public IStatement Prepare(string sql)
        {
            IntPtr stmt;
            string tail;
            var result = mSqlite3.Prepare16(mDatabase, sql, Encoding.Unicode.GetByteCount(sql), out stmt, out tail);
            if (result != Result.Ok)
            {
                string message;
                if (result == Result.Misuse)
                    message = "Internal misuse of interface";
                else
                    message = mSqlite3.Errmsg16(mDatabase);

                mSqlite3.Finalize(stmt);
                throw new SqlException(string.Format("{0} when preparing statement: {1}", result, message), sql);
            }

            return new SqliteStatement(mDatabase, stmt, mSqlite3);
        }

        public IStatement Prepare(string sql, params object[] args)
        {
            return Prepare(string.Format(sql, args));
        }

        public Status Status(DatabaseStatusFlag flag, bool reset)
        {
            if (flag == DatabaseStatusFlag.MemoryUsed)
            {
                return new Status(mSqlite3.MemoryUsed(), mSqlite3.MemoryHighWater(reset));
            }

            int current;
            int highWater;

            var result = mSqlite3.DatabaseStatus(mDatabase, flag, out current, out highWater, false);
            if (result != Result.Ok)
            {
                string message;
                if (result == Result.Misuse)
                    message = "Internal misuse of interface";
                else
                    message = mSqlite3.Errmsg16(mDatabase);

                throw new Exception(string.Format("{0} when querying database status: {1}", result, message));
            }

            return new Status(current, highWater);
        }

        public ITransaction BeginTransaction()
        {
            return new SqliteTransaction(this);
        }

        public ITransaction BeginTransaction(TransactionBehavior behavior)
        {
            return new SqliteTransaction(this, behavior);
        }

        public void CreateTable<T>() where T : class
        {
            CreateTable(typeof(T));
        }

        public void CreateTable(Type tableType)
        {
            var query = new StringBuilder();

            var table = tableType.GetCustomAttribute<TableAttribute>();
            if (table == null)
                throw new ArgumentException("Class must be annotated with the Table attribute");

            var tableName = string.IsNullOrEmpty(table.Name) ? tableType.Name : table.Name;

            var primaryKey = tableType.GetCustomAttribute<PrimaryKeyAttribute>();

            if (primaryKey != null && !primaryKey.Columns.Any())
                throw new MarkupException("PrimaryKey", "There must be at least one column in a primary key");

            if (primaryKey != null && primaryKey.Columns.Count() > 1 && primaryKey.AutoIncrement)
                throw new MarkupException("PrimaryKay", "Primary keys with more than one column can not be auto incrementing");

            var constraints = tableType.GetCustomAttributes<ForeignKeyAttribute>().Select(fk => new ForeignKeyConstraint(fk)).ToArray();

            query
                .AppendFormat("CREATE TABLE {0} {1} (",
                    table.IfNotExists ? "IF NOT EXISTS" : string.Empty,
                    tableName)
                .Append(CreateTableColumns(tableType, primaryKey, constraints))
                .Append(");");

            var indices = tableType.GetCustomAttributes<IndexAttribute>();
            foreach (var index in indices)
            {
                var indexName = string.IsNullOrEmpty(index.Name) ? "Index_" + Guid.NewGuid().ToString("N") : index.Name;

                query
                    .AppendFormat("\nCREATE {0} INDEX {1} {2} ON {3} (",
                        index.Unique ? "UNIQUE" : string.Empty,
                        index.IfNotExists ? "IF NOT EXISTS" : string.Empty,
                        indexName,
                        tableName)
                    .Append(CreateIndexColumns(index))
                    .Append(");");
            }

            var unmatchedConstraint = constraints.FirstOrDefault(c => !c.Matched);
            if (unmatchedConstraint != null)
                throw new MarkupException(unmatchedConstraint.Attribute.GetType().Name, "Constraint could not be generated");

            Execute(query.ToString());
        }

        private static string CreateTableColumns(Type tableType, PrimaryKeyAttribute primaryKey, IEnumerable<IConstraint> constraints)
        {
            bool primaryKeyCreated = false;

            //var columns = new List<string>();
            var columns = new Dictionary<string, string>();
            foreach (var member in tableType.
                GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).
                Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property))
            {
                var column = member.GetCustomAttribute<ColumnAttribute>();
                if (column != null)
                {
                    var clause = new StringBuilder();

                    var columnName = string.IsNullOrEmpty(column.Name) ? member.Name : column.Name;
                    clause.Append(columnName);

                    switch (column.TypeAffinity)
                    {
                        case TypeAffinity.None:
                            break;
                        case TypeAffinity.Integer:
                            clause.Append(" INTEGER");
                            break;
                        case TypeAffinity.Numeric:
                            clause.Append(" NUM");
                            break;
                        case TypeAffinity.Real:
                            clause.Append(" REAL");
                            break;
                        case TypeAffinity.Text:
                        case TypeAffinity.Enum:
                        case TypeAffinity.Guid:
                            clause.Append(" TEXT");
                            break;
                        default:
                            throw new Exception("Unsupported tableType affinity: " + column.TypeAffinity.ToString());
                    }

                    if (column.NotNull)
                        clause.Append(" NOT NULL");

                    if (column.Unique)
                        clause.Append(" UNIQUE");

                    if (primaryKey != null && primaryKey.Columns.Count() == 1 && primaryKey.Columns.First() == columnName)
                    {
                        if (!string.IsNullOrEmpty(primaryKey.Name))
                            clause.Append(" CONSTRAINT ").Append(primaryKey.Name);

                        clause
                            .Append(" PRIMARY KEY")
                            .Append(primaryKey.AutoIncrement ? " AUTOINCREMENT" : string.Empty);

                        primaryKeyCreated = true;
                    }

                    foreach (var constraint in constraints)
                    {
                        var constraintClause = constraint.GenerateAsColumnConstraint(columnName);
                        if (!string.IsNullOrEmpty(constraintClause))
                            clause.Append(" ").Append(constraintClause);
                    }

                    columns.Add(columnName, clause.ToString());
                }
            }

            var constraintClauses = new List<string>();
            if (primaryKey != null && primaryKey.Columns.Count() > 1)
            {
                var constraint = new StringBuilder();

                if (!string.IsNullOrEmpty(primaryKey.Name))
                    constraint.Append("CONSTRAINT ").Append(primaryKey.Name).Append(" ");

                constraint.Append("PRIMARY KEY (").Append(string.Join(",", primaryKey.Columns)).Append(")");

                primaryKeyCreated = true;
                constraintClauses.Add(constraint.ToString());
            }

            if (primaryKey != null && !primaryKeyCreated)
                throw new MarkupException("PrimaryKey", "Primary key did not match any columns");

            var columnNamesSet = new HashSet<string>(columns.Keys);
            foreach (var constraint in constraints)
            {
                var constraintClause = constraint.GenerateAsTableConstraint(columnNamesSet);
                if (!string.IsNullOrEmpty(constraintClause))
                    constraintClauses.Add(constraintClause);
            }

            return string.Join(",", columns.Values.Concat(constraintClauses));
        }

        private static string CreateIndexColumns(IndexAttribute index)
        {
            return string.Join(",", index.Columns);
        }
    }
}
