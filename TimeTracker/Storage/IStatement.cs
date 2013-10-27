using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Storage
{
    public enum StepResult
    {
        Row,
        Done,
        Busy,
    }

    public interface IStatement : IDisposable
    {
        int BindParameterCount { get; }

        int ColumnCount { get; }

        int DataCount { get; }

        StepResult Step();

        void Reset();

        void ClearBindings();

        void BindBlob(string name, byte[] blob);

        void BindBlob(int index, byte[] blob);

        void BindZeroBlob(string name, int byteCount);

        void BindZeroBlob(int index, int byteCount);

        void BindDouble(string name, double value);

        void BindDouble(int index, double value);

        void BindInt(string name, int value);

        void BindInt(int index, int value);

        void BindLong(string name, long? value);

        void BindLong(int index, long? value);

        void BindNull(string name);

        void BindNull(int index);

        void BindText(string name, string text);

        void BindText(int index, string text);

        void BindEnum<T>(string name, T value);

        void BindEnum<T>(int index, T value);

        void BindGuid(string name, Guid value);

        void BindGuid(int index, Guid value);

        int BindParameterIndex(string name);

        string BindParameterName(int index);

        byte[] ColumnBlob(int column);

        double? ColumnDouble(int column);

        int? ColumnInt(int column);

        long? ColumnLong(int column);

        string ColumnText(int column);

        Nullable<T> ColumnEnum<T>(int column) where T : struct;

        Guid? ColumnGuid(int column);

        string ColumnName(int column);
    }
}
