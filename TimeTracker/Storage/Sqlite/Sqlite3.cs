using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Storage.Sqlite
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int Callback(IntPtr userData, int argc, string[] argv, string[] columnName);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void Destructor(IntPtr data);

    internal interface ISqlite3
    {
        Result Open(string filename, out IntPtr db);

        Result Open16(string filename, out IntPtr db);

        Result Close(IntPtr db);

        Result Exec(IntPtr db, string sql, Callback callback, out IntPtr errMsg);

        Result Exec(IntPtr db, string sql, out IntPtr errMsg);

        string Errmsg16(IntPtr db);

        Result Prepare16(IntPtr db, string sql, int count, out IntPtr stmt, out string tail);

        Result DatabaseStatus(IntPtr db, DatabaseStatusFlag op, out int pCur, out int pHiwtr, bool resetFlg);

        long LastInsertRowid(IntPtr db);

        Result Finalize(IntPtr stmt);

        Result Step(IntPtr stmt);

        Result Reset(IntPtr stmt);

        Result ClearBindings(IntPtr stmt);

        Result BindBlob(IntPtr stmt, int index, IntPtr value, int byteCount, Destructor destructor);

        Result BindBlob(IntPtr stmt, int index, IntPtr value, int byteCount, IntPtr destructor);

        Result BindDouble(IntPtr stmt, int index, double value);

        Result BindInt(IntPtr stmt, int index, int value);

        Result BindInt64(IntPtr stmt, int index, long value);

        Result BindNull(IntPtr stmt, int index);

        Result BindText(IntPtr stmt, int index, string value, int byteCount, Destructor destructor);

        Result BindText(IntPtr stmt, int index, string value, int byteCount, IntPtr destructor);

        Result BindText16(IntPtr stmt, int index, string value, int byteCount, Destructor destructor);

        Result BindText16(IntPtr stmt, int index, string value, int byteCount, IntPtr destructor);

        Result BindZeroBlob(IntPtr stmt, int index, int byteCount);

        int BindParameterIndex(IntPtr stmt, string name);

        int BindParameterCount(IntPtr stmt);

        string BindParameterName(IntPtr stmt, int index);

        int ColumnCount(IntPtr stmt);

        int DataCount(IntPtr stmt);

        IntPtr ColumnBlob(IntPtr stmt, int col);

        int ColumnBytes(IntPtr stmt, int col);

        int ColumnBytes16(IntPtr stmt, int col);

        double ColumnDouble(IntPtr stmt, int col);

        int ColumnInt(IntPtr stmt, int col);

        long ColumnInt64(IntPtr stmt, int col);

        string ColumnText(IntPtr stmt, int col);

        string ColumnText16(IntPtr stmt, int col);

        SqliteType ColumnType(IntPtr stmt, int col);

        string ColumnName(IntPtr stmt, int col);

        string ColumnName16(IntPtr stmt, int col);

        void Free(IntPtr ptr);

        long MemoryUsed();

        long MemoryHighWater(bool reset);
    }

    internal class Sqlite3 : ISqlite3
    {
        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_open", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern Result sqlite3_open(string filename, out IntPtr db);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_open16", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        static extern Result sqlite3_open16(string filename, out IntPtr db);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_close_v2", CallingConvention = CallingConvention.Cdecl)]
        static extern Result sqlite3_close_v2(IntPtr db);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_exec", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern Result sqlite3_exec(IntPtr db, string sql, Callback callback, IntPtr userData, out IntPtr errMsg);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_exec", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern Result sqlite3_exec(IntPtr db, string sql, IntPtr callback, IntPtr userData, out IntPtr errMsg);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_prepare16_v2", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        static extern Result sqlite3_prepare16_v2(IntPtr db, string sql, int count, out IntPtr stmt, out IntPtr tail);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_errmsg16", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr sqlite3_errmsg16(IntPtr db);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_db_status", CallingConvention = CallingConvention.Cdecl)]
        static extern Result sqlite3_db_status(IntPtr db, DatabaseStatusFlag op, out int pCur, out int pHiwtr, bool resetFlg);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_last_insert_rowid", CallingConvention = CallingConvention.Cdecl)]
        static extern long sqlite3_last_insert_rowid(IntPtr db);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_finalize", CallingConvention = CallingConvention.Cdecl)]
        static extern Result sqlite3_finalize(IntPtr stmt);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_step", CallingConvention = CallingConvention.Cdecl)]
        static extern Result sqlite3_step(IntPtr stmt);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_reset", CallingConvention = CallingConvention.Cdecl)]
        static extern Result sqlite3_reset(IntPtr stmt);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_clear_bindings", CallingConvention = CallingConvention.Cdecl)]
        static extern Result sqlite3_clear_bindings(IntPtr stmt);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_bind_blob", CallingConvention = CallingConvention.Cdecl)]
        static extern Result sqlite3_bind_blob(IntPtr stmt, int index, IntPtr value, int n, Destructor destructor);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_bind_blob", CallingConvention = CallingConvention.Cdecl)]
        static extern Result sqlite3_bind_blob(IntPtr stmt, int index, IntPtr value, int n, IntPtr destructor);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_bind_double", CallingConvention = CallingConvention.Cdecl)]
        static extern Result sqlite3_bind_double(IntPtr stmt, int index, double value);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_bind_int", CallingConvention = CallingConvention.Cdecl)]
        static extern Result sqlite3_bind_int(IntPtr stmt, int index, int value);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_bind_int64", CallingConvention = CallingConvention.Cdecl)]
        static extern Result sqlite3_bind_int64(IntPtr stmt, int index, long value);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_bind_null", CallingConvention = CallingConvention.Cdecl)]
        static extern Result sqlite3_bind_null(IntPtr stmt, int index);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_bind_text", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern Result sqlite3_bind_text(IntPtr stmt, int index, string value, int n, Destructor destructor);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_bind_text", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern Result sqlite3_bind_text(IntPtr stmt, int index, string value, int n, IntPtr destructor);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_bind_text16", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        static extern Result sqlite3_bind_text16(IntPtr stmt, int index, string value, int n, Destructor destructor);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_bind_text16", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        static extern Result sqlite3_bind_text16(IntPtr stmt, int index, string value, int n, IntPtr destructor);

        //[DllImport("sqlite3.dll", EntryPoint = "sqlite3_bind_value", CallingConvention = CallingConvention.Cdecl)]
        //static extern Result sqlite3_bind_value(IntPtr stmt, int index, const sqlite3_value* value);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_bind_zeroblob", CallingConvention = CallingConvention.Cdecl)]
        static extern Result sqlite3_bind_zeroblob(IntPtr stmt, int index, int n);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_bind_parameter_index", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern int sqlite3_bind_parameter_index(IntPtr stmt, string name);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_bind_parameter_count", CallingConvention = CallingConvention.Cdecl)]
        static extern int sqlite3_bind_parameter_count(IntPtr stmt);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_bind_parameter_name", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern string sqlite3_bind_parameter_name(IntPtr stmt, int index);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_count", CallingConvention = CallingConvention.Cdecl)]
        static extern int sqlite3_column_count(IntPtr stmt);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_data_count", CallingConvention = CallingConvention.Cdecl)]
        static extern int sqlite3_data_count(IntPtr stmt);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_blob", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr sqlite3_column_blob(IntPtr stmt, int col);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_bytes", CallingConvention = CallingConvention.Cdecl)]
        static extern int sqlite3_column_bytes(IntPtr stmt, int col);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_bytes16", CallingConvention = CallingConvention.Cdecl)]
        static extern int sqlite3_column_bytes16(IntPtr stmt, int col);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_double", CallingConvention = CallingConvention.Cdecl)]
        static extern double sqlite3_column_double(IntPtr stmt, int col);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_int", CallingConvention = CallingConvention.Cdecl)]
        static extern int sqlite3_column_int(IntPtr stmt, int col);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_int64", CallingConvention = CallingConvention.Cdecl)]
        static extern long sqlite3_column_int64(IntPtr stmt, int col);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_text", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr sqlite3_column_text(IntPtr stmt, int col);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_text16", CallingConvention = CallingConvention.Cdecl)]
        static extern IntPtr sqlite3_column_text16(IntPtr stmt, int col);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_type", CallingConvention = CallingConvention.Cdecl)]
        static extern SqliteType sqlite3_column_type(IntPtr stmt, int col);

        //[DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_value", CallingConvention = CallingConvention.Cdecl)]
        //sqlite3_value *sqlite3_column_value(IntPtr stmt, int col);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_name", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        static extern string sqlite3_column_name(IntPtr stmt, int col);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_column_name16", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        static extern string sqlite3_column_name16(IntPtr stmt, int col);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_free", CallingConvention = CallingConvention.Cdecl)]
        static extern void sqlite3_free(IntPtr ptr);

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_memory_used", CallingConvention = CallingConvention.Cdecl)]
        static extern long sqlite3_memory_used();

        [DllImport("sqlite3.dll", EntryPoint = "sqlite3_memory_highwater", CallingConvention = CallingConvention.Cdecl)]
        static extern long sqlite3_memory_highwater(bool resetFlag);

        public Result Open(string filename, out IntPtr db)
        {
            return sqlite3_open(filename, out db);
        }

        public Result Open16(string filename, out IntPtr db)
        {
            return sqlite3_open16(filename, out db);
        }

        public Result Close(IntPtr db)
        {
            return sqlite3_close_v2(db);
        }

        public Result Exec(IntPtr db, string sql, Callback callback, out IntPtr errMsg)
        {
            return sqlite3_exec(db, sql, callback, IntPtr.Zero, out errMsg);
        }

        public Result Exec(IntPtr db, string sql, out IntPtr errMsg)
        {
            return sqlite3_exec(db, sql, IntPtr.Zero, IntPtr.Zero, out errMsg);
        }

        public Result Prepare16(IntPtr db, string sql, int count, out IntPtr stmt, out string tail)
        {
            IntPtr t;
            var result = sqlite3_prepare16_v2(db, sql, count, out stmt, out t);

            tail = Marshal.PtrToStringUni(t);
            return result;
        }

        public string Errmsg16(IntPtr db)
        {
            return Marshal.PtrToStringUni(sqlite3_errmsg16(db));
        }

        public Result DatabaseStatus(IntPtr db, DatabaseStatusFlag op, out int pCur, out int pHiwtr, bool resetFlg)
        {
            return sqlite3_db_status(db, op, out pCur, out pHiwtr, resetFlg);
        }

        public long LastInsertRowid(IntPtr db)
        {
            return sqlite3_last_insert_rowid(db);
        }

        public Result Finalize(IntPtr stmt)
        {
            return sqlite3_finalize(stmt);
        }

        public Result Reset(IntPtr stmt)
        {
            return sqlite3_reset(stmt);
        }

        public Result Step(IntPtr stmt)
        {
            return sqlite3_step(stmt);
        }

        public Result ClearBindings(IntPtr stmt)
        {
            return sqlite3_clear_bindings(stmt);
        }

        public Result BindBlob(IntPtr stmt, int index, IntPtr value, int byteCount, Destructor destructor)
        {
            return sqlite3_bind_blob(stmt, index, value, byteCount, destructor);
        }

        public Result BindBlob(IntPtr stmt, int index, IntPtr value, int byteCount, IntPtr destructor)
        {
            return sqlite3_bind_blob(stmt, index, value, byteCount, destructor);
        }

        public Result BindDouble(IntPtr stmt, int index, double value)
        {
            return sqlite3_bind_double(stmt, index, value);
        }

        public Result BindInt(IntPtr stmt, int index, int value)
        {
            return sqlite3_bind_int(stmt, index, value);
        }

        public Result BindInt64(IntPtr stmt, int index, long value)
        {
            return sqlite3_bind_int64(stmt, index, value);
        }

        public Result BindNull(IntPtr stmt, int index)
        {
            return sqlite3_bind_null(stmt, index);
        }

        public Result BindText(IntPtr stmt, int index, string value, int byteCount, Destructor destructor)
        {
            return sqlite3_bind_text(stmt, index, value, byteCount, destructor);
        }

        public Result BindText(IntPtr stmt, int index, string value, int byteCount, IntPtr destructor)
        {
            return sqlite3_bind_text(stmt, index, value, byteCount, destructor);
        }

        public Result BindText16(IntPtr stmt, int index, string value, int byteCount, Destructor destructor)
        {
            return sqlite3_bind_text16(stmt, index, value, byteCount, destructor);
        }

        public Result BindText16(IntPtr stmt, int index, string value, int byteCount, IntPtr destructor)
        {
            return sqlite3_bind_text16(stmt, index, value, byteCount, destructor);
        }

        public Result BindZeroBlob(IntPtr stmt, int index, int byteCount)
        {
            return sqlite3_bind_zeroblob(stmt, index, byteCount);
        }

        public int BindParameterIndex(IntPtr stmt, string name)
        {
            return sqlite3_bind_parameter_index(stmt, name);
        }

        public int BindParameterCount(IntPtr stmt)
        {
            return sqlite3_bind_parameter_count(stmt);
        }

        public string BindParameterName(IntPtr stmt, int index)
        {
            return sqlite3_bind_parameter_name(stmt, index);
        }

        public int ColumnCount(IntPtr stmt)
        {
            return sqlite3_column_count(stmt);
        }

        public int DataCount(IntPtr stmt)
        {
            return sqlite3_data_count(stmt);
        }

        public IntPtr ColumnBlob(IntPtr stmt, int col)
        {
            return sqlite3_column_blob(stmt, col);
        }

        public int ColumnBytes(IntPtr stmt, int col)
        {
            return sqlite3_column_bytes(stmt, col);
        }

        public int ColumnBytes16(IntPtr stmt, int col)
        {
            return sqlite3_column_bytes16(stmt, col);
        }

        public double ColumnDouble(IntPtr stmt, int col)
        {
            return sqlite3_column_double(stmt, col);
        }

        public int ColumnInt(IntPtr stmt, int col)
        {
            return sqlite3_column_int(stmt, col);
        }

        public long ColumnInt64(IntPtr stmt, int col)
        {
            return sqlite3_column_int64(stmt, col);
        }

        public string ColumnText(IntPtr stmt, int col)
        {
            IntPtr ptr = sqlite3_column_text(stmt, col);
            return Marshal.PtrToStringAnsi(ptr);
        }

        public string ColumnText16(IntPtr stmt, int col)
        {
            IntPtr ptr = sqlite3_column_text16(stmt, col);
            return Marshal.PtrToStringUni(ptr);
        }

        public SqliteType ColumnType(IntPtr stmt, int col)
        {
            return sqlite3_column_type(stmt, col);
        }

        public string ColumnName(IntPtr stmt, int col)
        {
            return sqlite3_column_name(stmt, col);
        }

        public string ColumnName16(IntPtr stmt, int col)
        {
            return sqlite3_column_name16(stmt, col);
        }

        public void Free(IntPtr ptr)
        {
            sqlite3_free(ptr);
        }

        public long MemoryUsed()
        {
            return sqlite3_memory_used();
        }

        public long MemoryHighWater(bool reset)
        {
            return sqlite3_memory_highwater(reset);
        }
    }
}
