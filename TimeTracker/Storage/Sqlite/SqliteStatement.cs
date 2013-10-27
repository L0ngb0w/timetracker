using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Storage.Sqlite
{
    internal enum SqliteType
    {
        Integer = 1,
        Float = 2,
        Text = 3,
        Blob = 4,
        Null = 5,
    }

    internal class SqliteStatement : IStatement
    {
        internal interface IMarshal
        {
            IntPtr AllocHGlobal(int cb);

            void FreeHGlobal(IntPtr hglobal);

            void Copy(byte[] source, int startIndex, IntPtr destination, int length);

            void Copy(IntPtr source, byte[] destination, int startIndex, int length);
        }

        sealed class MarshalWrapper : IMarshal
        {
            public IntPtr AllocHGlobal(int cb)
            {
                return Marshal.AllocHGlobal(cb);
            }

            public void FreeHGlobal(IntPtr hglobal)
            {
                Marshal.FreeHGlobal(hglobal);
            }

            public void Copy(byte[] source, int startIndex, IntPtr destination, int length)
            {
                Marshal.Copy(source, startIndex, destination, length);
            }

            public void Copy(IntPtr source, byte[] destination, int startIndex, int length)
            {
                Marshal.Copy(source, destination, startIndex, length);
            }
        }

        ISqlite3 sqlite3;
        IMarshal marshal;

        IntPtr mDatabase;
        IntPtr mStatement;

        public int BindParameterCount
        {
            get { return sqlite3.BindParameterCount(mStatement); }
        }

        public int ColumnCount
        {
            get { return sqlite3.ColumnCount(mStatement); }
        }

        public int DataCount
        {
            get { return sqlite3.DataCount(mStatement); }
        }

        internal SqliteStatement(IntPtr database, IntPtr statement, ISqlite3 sqlite3, IMarshal marshal)
        {
            if (sqlite3 == null)
                throw new ArgumentNullException("sqlite3");

            if (marshal == null)
                throw new ArgumentNullException("marshal");

            this.sqlite3 = sqlite3;
            this.marshal = marshal;

            mDatabase = database;
            mStatement = statement;
        }

        internal SqliteStatement(IntPtr database, IntPtr statement, ISqlite3 sqlite3)
            : this(database, statement, sqlite3, new MarshalWrapper())
        {
        }

        public void Dispose()
        {
            sqlite3.Finalize(mStatement);

            mDatabase = IntPtr.Zero;
            mStatement = IntPtr.Zero;
        }

        public StepResult Step()
        {
            var result = sqlite3.Step(mStatement);
            switch (result)
            {
                case Result.Row:
                    return StepResult.Row;
                case Result.Busy:
                    return StepResult.Busy;
                case Result.Done:
                    return StepResult.Done;
                default:
                    string message = sqlite3.Errmsg16(mDatabase);
                    throw new Exception(string.Format("{0} when stepping statement: {1}", result, message));
            }
        }

        public void Reset()
        {
            var result = sqlite3.Reset(mStatement);
            if (result != Result.Ok)
            {
                var message = sqlite3.Errmsg16(mDatabase);
                throw new Exception(string.Format("{0} when resetting statement: {1}", result, message));
            }
        }

        public void ClearBindings()
        {
            var result = sqlite3.ClearBindings(mStatement);
            if (result != Result.Ok)
            {
                var message = sqlite3.Errmsg16(mDatabase);
                throw new Exception(string.Format("{0} when clearing bindings: {1}", result, message));
            }
        }

        public void BindBlob(string name, byte[] blob)
        {
            BindBlob(BindParameterIndex(name), blob);
        }

        public void BindBlob(int index, byte[] blob)
        {
            if (index < 1)
                throw new ArgumentOutOfRangeException("index", "Index must be greater than zero");

            IntPtr unmanagedPointer = marshal.AllocHGlobal(blob.Length);
            marshal.Copy(blob, 0, unmanagedPointer, blob.Length);

            Result result;
            try
            {
                result = sqlite3.BindBlob(mStatement, index, unmanagedPointer, blob.Length, new Destructor(ptr => marshal.FreeHGlobal(ptr)));
            }
            catch (Exception)
            {
                marshal.FreeHGlobal(unmanagedPointer);
                throw;
            }

            if (result != Result.Ok)
            {
                var message = sqlite3.Errmsg16(mDatabase);
                throw new Exception(string.Format("Error {0} binding blob: {1}", result, message));
            }
        }

        public void BindZeroBlob(string name, int byteCount)
        {
            BindZeroBlob(BindParameterIndex(name), byteCount);
        }

        public void BindZeroBlob(int index, int byteCount)
        {
            if (index < 1)
                throw new ArgumentOutOfRangeException("index", "Index must be greater than zero");

            if (byteCount < 0)
                throw new ArgumentOutOfRangeException("byteCount");

            var result = sqlite3.BindZeroBlob(mStatement, index, byteCount);
            if (result != Result.Ok)
            {
                var message = sqlite3.Errmsg16(mDatabase);
                throw new Exception(string.Format("{0} when binding zero blob: {1}", result, message));
            }
        }

        public void BindDouble(string name, double value)
        {
            BindDouble(BindParameterIndex(name), value);
        }

        public void BindDouble(int index, double value)
        {
            if (index < 1)
                throw new ArgumentOutOfRangeException("index", "Index must be greater than zero");

            var result = sqlite3.BindDouble(mStatement, index, value);
            if (result != Result.Ok)
            {
                var message = sqlite3.Errmsg16(mDatabase);
                throw new Exception(string.Format("{0} when binding double: {1}", result, message));
            }
        }

        public void BindInt(string name, int value)
        {
            BindInt(BindParameterIndex(name), value);
        }

        public void BindInt(int index, int value)
        {
            if (index < 1)
                throw new ArgumentOutOfRangeException("index", "Index must be greater than zero");

            var result = sqlite3.BindInt(mStatement, index, value);
            if (result != Result.Ok)
            {
                var message = sqlite3.Errmsg16(mDatabase);
                throw new Exception(string.Format("{0} when binding int: {1}", result, message));
            }
        }

        public void BindLong(string name, long? value)
        {
            BindLong(BindParameterIndex(name), value);
        }

        public void BindLong(int index, long? value)
        {
            if (index < 1)
                throw new ArgumentOutOfRangeException("index", "Index must be greater than zero");

            Result result;
            if (value.HasValue)
                result = sqlite3.BindInt64(mStatement, index, value.Value);
            else
                result = sqlite3.BindNull(mStatement, index);

            if (result != Result.Ok)
            {
                var message = sqlite3.Errmsg16(mDatabase);
                throw new Exception(string.Format("{0} when binding long: {1}", result, message));
            }
        }

        public void BindNull(string name)
        {
            BindNull(BindParameterIndex(name));
        }

        public void BindNull(int index)
        {
            if (index < 1)
                throw new ArgumentOutOfRangeException("index", "Index must be greater than zero");

            var result = sqlite3.BindNull(mStatement, index);
            if (result != Result.Ok)
            {
                var message = sqlite3.Errmsg16(mDatabase);
                throw new Exception(string.Format("{0} when binding null: {1}", result, message));
            }
        }

        public void BindText(string name, string text)
        {
            BindText(BindParameterIndex(name), text);
        }

        public void BindText(int index, string text)
        {
            if (index < 1)
                throw new ArgumentOutOfRangeException("index", "Index must be greater than zero");

            var result = sqlite3.BindText16(mStatement, index, text, Encoding.Unicode.GetByteCount(text), new IntPtr(-1));
            if (result != Result.Ok)
            {
                var message = sqlite3.Errmsg16(mDatabase);
                throw new Exception(string.Format("{0} when binding text: {1}", result, message));
            }
        }

        public void BindEnum<T>(string name, T value)
        {
            BindEnum<T>(BindParameterIndex(name), value);
        }

        public void BindEnum<T>(int index, T value)
        {
            BindText(index, Enum.GetName(typeof(T), value));
        }

        public void BindGuid(string name, Guid value)
        {
            BindGuid(BindParameterIndex(name), value);
        }

        public void BindGuid(int index, Guid value)
        {
            BindBlob(index, value.ToByteArray());
        }

        public int BindParameterIndex(string name)
        {
            int index = sqlite3.BindParameterIndex(mStatement, name);
            if (index == 0)
                throw new ArgumentException(string.Format("A parameter named '{0}' was not found", name), "name");

            return index;
        }

        public string BindParameterName(int index)
        {
            return sqlite3.BindParameterName(mStatement, index);
        }

        public byte[] ColumnBlob(int column)
        {
            if (sqlite3.ColumnType(mStatement, column) == SqliteType.Null)
                return null;

            int size = sqlite3.ColumnBytes(mStatement, column);
            IntPtr ptr = sqlite3.ColumnBlob(mStatement, column);

            byte[] blob = new byte[size];
            marshal.Copy(ptr, blob, 0, size);

            return blob;
        }

        public double? ColumnDouble(int column)
        {
            if (sqlite3.ColumnType(mStatement, column) == SqliteType.Null)
                return null;

            return sqlite3.ColumnDouble(mStatement, column);
        }

        public int? ColumnInt(int column)
        {
            if (sqlite3.ColumnType(mStatement, column) == SqliteType.Null)
                return null;

            return sqlite3.ColumnInt(mStatement, column);
        }

        public long? ColumnLong(int column)
        {
            if (sqlite3.ColumnType(mStatement, column) == SqliteType.Null)
                return null;

            return sqlite3.ColumnInt64(mStatement, column);
        }

        public string ColumnText(int column)
        {
            if (sqlite3.ColumnType(mStatement, column) == SqliteType.Null)
                return null;

            return sqlite3.ColumnText16(mStatement, column);
        }

        public Nullable<T> ColumnEnum<T>(int column) where T : struct
        {
            if (sqlite3.ColumnType(mStatement, column) == SqliteType.Null)
                return null;

            var value = sqlite3.ColumnText16(mStatement, column);
            if (value == null)
                return null;

            return (T)Enum.Parse(typeof(T), value);
        }

        public Guid? ColumnGuid(int column)
        {
            var bytes = ColumnBlob(column);
            if (bytes == null)
                return null;

            return new Guid(bytes);
        }

        public string ColumnName(int column)
        {
            return sqlite3.ColumnName16(mStatement, column);
        }
    }
}
