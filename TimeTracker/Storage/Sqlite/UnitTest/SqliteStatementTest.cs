using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Storage.Sqlite.UnitTest
{
    [TestFixture]
    class SqliteStatementTest
    {
        const string text = "Hello, World!";

        readonly IntPtr database = new IntPtr(999);
        readonly IntPtr statement = new IntPtr(666);

        Mock<ISqlite3> mSqlite3;
        Mock<SqliteStatement.IMarshal> mMarshal = new Mock<SqliteStatement.IMarshal>();

        [SetUp]
        public void Setup()
        {
            mSqlite3 = new Mock<ISqlite3>(MockBehavior.Strict);
            mMarshal = new Mock<SqliteStatement.IMarshal>(MockBehavior.Strict);
        }

        [Test]
        public void Constructor_Sqlite3IsNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqliteStatement(database, statement, null, mMarshal.Object));
        }

        [Test]
        public void Constructor_MarshalIsNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqliteStatement(database, statement, mSqlite3.Object, null));
        }

        [Test]
        public void Dispose_MethodInvoked_StatementFinalized()
        {
            mSqlite3.Setup(e => e.Finalize(statement)).Returns(Result.Ok);

            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);
            instance.Dispose();

            mSqlite3.VerifyAll();
        }

        [Test]
        public void BindParameterCount_Get_ReturnsCount()
        {
            int expected = 42;

            mSqlite3.Setup(e => e.BindParameterCount(statement)).Returns(expected);

            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);
            var actual = instance.BindParameterCount;

            Assert.AreEqual(expected, actual);
        }

        [TestCase(Result.Row, StepResult.Row)]
        [TestCase(Result.Busy, StepResult.Busy)]
        [TestCase(Result.Done, StepResult.Done)]
        public void Step_MethodInvoked_ResultIsAsExpected(Result result, StepResult expected)
        {
            mSqlite3.Setup(e => e.Step(statement)).Returns(result);

            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);
            var actual = instance.Step();

            Assert.AreEqual(expected, actual);
            mSqlite3.VerifyAll();
        }

        [Test]
        public void Step_MethodInvokedReturnsError_ThrowsException()
        {
            mSqlite3.Setup(e => e.Step(statement)).Returns(Result.Error);
            mSqlite3.Setup(e => e.Errmsg16(database)).Returns(string.Empty);

            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);
            Assert.Throws<Exception>(() => instance.Step());

            mSqlite3.VerifyAll();
        }

        [Test]
        public void Reset_MethodInvokedReturnsOk_DoesNotThrowException()
        {
            mSqlite3.Setup(e => e.Reset(statement)).Returns(Result.Ok);

            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.DoesNotThrow(() => instance.Reset());

            mSqlite3.VerifyAll();
        }

        [Test]
        public void Reset_MethodInvokedReturnsError_ThrowsException()
        {
            mSqlite3.Setup(e => e.Reset(statement)).Returns(Result.Error);
            mSqlite3.Setup(e => e.Errmsg16(database)).Returns(string.Empty);

            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.Throws<Exception>(() => instance.Reset());

            mSqlite3.VerifyAll();
        }

        [Test]
        public void ClearBindings_MethodInvokedReturnsOk_DoesNotThrowException()
        {
            mSqlite3.Setup(e => e.ClearBindings(statement)).Returns(Result.Ok);

            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.DoesNotThrow(() => instance.ClearBindings());

            mSqlite3.VerifyAll();
        }

        [Test]
        public void ClearBindings_MethodInvokedReturnsError_ThrowsException()
        {
            mSqlite3.Setup(e => e.ClearBindings(statement)).Returns(Result.Error);
            mSqlite3.Setup(e => e.Errmsg16(database)).Returns(string.Empty);

            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.Throws<Exception>(() => instance.ClearBindings());

            mSqlite3.VerifyAll();
        }

        [Test]
        public void BindBlob_IndexBelowOne_ThrowsException()
        {
            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.Throws<ArgumentOutOfRangeException>(() => instance.BindBlob(0, new byte[] { }));
        }

        [Test]
        public void BindBlob_MethodInvokationThrowsException_MemoryFreed()
        {
            byte[] blob = new byte[] { 1, 2, 3, 4, 5 };
            IntPtr ptr = new IntPtr(555);

            mSqlite3.Setup(e => e.BindBlob(statement, 1, ptr, 5, It.IsAny<Destructor>())).Throws<Exception>();
            mMarshal.Setup(e => e.AllocHGlobal(5)).Returns(ptr);
            mMarshal.Setup(e => e.Copy(blob, 0, ptr, 5));
            mMarshal.Setup(e => e.FreeHGlobal(ptr));

            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.Throws<Exception>(() => instance.BindBlob(1, blob));

            mSqlite3.VerifyAll();
            mMarshal.VerifyAll();
        }

        [Test]
        public void BindBlob_MethodInvokedReturnsError_ThrowsException()
        {
            byte[] blob = new byte[] { 1, 2, 3, 4, 5 };
            IntPtr ptr = new IntPtr(555);

            mSqlite3.Setup(e => e.BindBlob(statement, 1, ptr, 5, It.IsAny<Destructor>())).Returns(Result.Error).Callback((IntPtr a1, int a2, IntPtr a3, int a4, Destructor a5) => a5(ptr));
            mSqlite3.Setup(e => e.Errmsg16(database)).Returns(string.Empty);
            mMarshal.Setup(e => e.AllocHGlobal(5)).Returns(ptr);
            mMarshal.Setup(e => e.Copy(blob, 0, ptr, 5));
            mMarshal.Setup(e => e.FreeHGlobal(ptr));

            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.Throws<Exception>(() => instance.BindBlob(1, blob));

            mSqlite3.VerifyAll();
            mMarshal.VerifyAll();
        }

        [Test]
        public void BindBlob_MethodInvokedReturnsOk_DoesNotThrowException()
        {
            byte[] blob = new byte[] { 1, 2, 3, 4, 5 };
            IntPtr ptr = new IntPtr(555);

            mSqlite3.Setup(e => e.BindBlob(statement, 1, ptr, 5, It.IsAny<Destructor>())).Returns(Result.Ok).Callback((IntPtr a1, int a2, IntPtr a3, int a4, Destructor a5) => a5(ptr));
            mMarshal.Setup(e => e.AllocHGlobal(5)).Returns(ptr);
            mMarshal.Setup(e => e.Copy(blob, 0, ptr, 5));
            mMarshal.Setup(e => e.FreeHGlobal(ptr));

            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.DoesNotThrow(() => instance.BindBlob(1, blob));

            mSqlite3.VerifyAll();
            mMarshal.VerifyAll();
        }

        [Test]
        public void BindZeroBlob_IndexBelowOne_ThrowsException()
        {
            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.Throws<ArgumentOutOfRangeException>(() => instance.BindZeroBlob(0, 0));
        }

        [Test]
        public void BindZeroBlob_ByteCountBelowZero_ThrowsException()
        {
            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.Throws<ArgumentOutOfRangeException>(() => instance.BindZeroBlob(1, -1));
        }

        [Test]
        public void BindZeroBlob_MethodInvokedReturnsOk_DoesNotThrowException()
        {
            mSqlite3.Setup(e => e.BindZeroBlob(statement, 1, 42)).Returns(Result.Ok);

            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.DoesNotThrow(() => instance.BindZeroBlob(1, 42));

            mSqlite3.VerifyAll();
        }

        [Test]
        public void BindZeroBlob_MethodInvokedReturnsError_ThrowsException()
        {
            mSqlite3.Setup(e => e.BindZeroBlob(statement, 1, 42)).Returns(Result.Error);
            mSqlite3.Setup(e => e.Errmsg16(database)).Returns(string.Empty);

            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.Throws<Exception>(() => instance.BindZeroBlob(1, 42));

            mSqlite3.VerifyAll();
        }

        [Test]
        public void BindDouble_IndexBelowOne_ThrowsException()
        {
            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.Throws<ArgumentOutOfRangeException>(() => instance.BindDouble(0, 0));
        }

        [Test]
        public void BindDouble_MethodInvokedReturnsOk_DoesNotThrowException()
        {
            mSqlite3.Setup(e => e.BindDouble(statement, 1, 42)).Returns(Result.Ok);

            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.DoesNotThrow(() => instance.BindDouble(1, 42));

            mSqlite3.VerifyAll();
        }

        [Test]
        public void BindDouble_MethodInvokedReturnsError_ThrowsException()
        {
            mSqlite3.Setup(e => e.BindDouble(statement, 1, 42)).Returns(Result.Error);
            mSqlite3.Setup(e => e.Errmsg16(database)).Returns(string.Empty);

            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.Throws<Exception>(() => instance.BindDouble(1, 42));

            mSqlite3.VerifyAll();
        }

        [Test]
        public void BindInt_IndexBelowOne_ThrowsException()
        {
            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.Throws<ArgumentOutOfRangeException>(() => instance.BindInt(0, 0));
        }

        [Test]
        public void BindInt_MethodInvokedReturnsOk_DoesNotThrowException()
        {
            mSqlite3.Setup(e => e.BindInt(statement, 1, 42)).Returns(Result.Ok);

            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.DoesNotThrow(() => instance.BindInt(1, 42));

            mSqlite3.VerifyAll();
        }

        [Test]
        public void BindInt_MethodInvokedReturnsError_ThrowsException()
        {
            mSqlite3.Setup(e => e.BindInt(statement, 1, 42)).Returns(Result.Error);
            mSqlite3.Setup(e => e.Errmsg16(database)).Returns(string.Empty);

            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.Throws<Exception>(() => instance.BindInt(1, 42));

            mSqlite3.VerifyAll();
        }

        [Test]
        public void BindLong_IndexBelowOne_ThrowsException()
        {
            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.Throws<ArgumentOutOfRangeException>(() => instance.BindLong(0, 0));
        }

        [Test]
        public void BindLong_MethodInvokedReturnsOk_DoesNotThrowException()
        {
            mSqlite3.Setup(e => e.BindInt64(statement, 1, 42)).Returns(Result.Ok);

            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.DoesNotThrow(() => instance.BindLong(1, 42));

            mSqlite3.VerifyAll();
        }

        [Test]
        public void BindLong_MethodInvokedReturnsError_ThrowsException()
        {
            mSqlite3.Setup(e => e.BindInt64(statement, 1, 42)).Returns(Result.Error);
            mSqlite3.Setup(e => e.Errmsg16(database)).Returns(string.Empty);

            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.Throws<Exception>(() => instance.BindLong(1, 42));

            mSqlite3.VerifyAll();
        }

        [Test]
        public void BindNull_IndexBelowOne_ThrowsException()
        {
            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.Throws<ArgumentOutOfRangeException>(() => instance.BindNull(0));
        }

        [Test]
        public void BindNull_MethodInvokedReturnsOk_DoesNotThrowException()
        {
            mSqlite3.Setup(e => e.BindNull(statement, 1)).Returns(Result.Ok);

            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.DoesNotThrow(() => instance.BindNull(1));

            mSqlite3.VerifyAll();
        }

        [Test]
        public void BindNull_MethodInvokedReturnsError_ThrowsException()
        {
            mSqlite3.Setup(e => e.BindNull(statement, 1)).Returns(Result.Error);
            mSqlite3.Setup(e => e.Errmsg16(database)).Returns(string.Empty);

            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.Throws<Exception>(() => instance.BindNull(1));

            mSqlite3.VerifyAll();
        }

        [Test]
        public void BindText_IndexBelowOne_ThrowsException()
        {
            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.Throws<ArgumentOutOfRangeException>(() => instance.BindText(0, string.Empty));
        }

        [Test]
        public void BindText_MethodInvokedReturnsOk_DoesNotThrowException()
        {
            mSqlite3.Setup(e => e.BindText16(statement, 1, text, It.IsAny<int>(), new IntPtr(-1))).Returns(Result.Ok);

            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.DoesNotThrow(() => instance.BindText(1, text));

            mSqlite3.VerifyAll();
        }

        [Test]
        public void BindText_MethodInvokedReturnsError_ThrowsException()
        {
            mSqlite3.Setup(e => e.BindText16(statement, 1, text, It.IsAny<int>(), new IntPtr(-1))).Returns(Result.Error);
            mSqlite3.Setup(e => e.Errmsg16(database)).Returns(string.Empty);

            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.Throws<Exception>(() => instance.BindText(1, text));

            mSqlite3.VerifyAll();
        }

        [Test]
        public void BindParameterIndex_NoParameterFound_ThrowsException()
        {
            mSqlite3.Setup(e => e.BindParameterIndex(statement, string.Empty)).Returns(0);

            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);

            Assert.Throws<ArgumentException>(() => instance.BindParameterIndex(string.Empty));
        }

        [Test]
        public void BindParameterIndex_ParameterIndexFound_IndexReturned()
        {
            var expected = 42;

            mSqlite3.Setup(e => e.BindParameterIndex(statement, string.Empty)).Returns(expected);

            var instance = new SqliteStatement(database, statement, mSqlite3.Object, mMarshal.Object);
            var actual = instance.BindParameterIndex(string.Empty);

            Assert.AreEqual(expected, actual);
        }
    }
}
