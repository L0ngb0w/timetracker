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
    class SqliteTransactionTest
    {
        Mock<IDatabase> database;

        [SetUp]
        public void Setup()
        {
            database = new Mock<IDatabase>(MockBehavior.Strict);
        }

        [Test]
        public void SqliteTransaction_DatabaseIsNullNoBehavior_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqliteTransaction(null));
        }

        [Test]
        public void SqliteTransaction_NoBehavior_NoQualifierOnStatement()
        {
            database.Setup(e => e.Execute("BEGIN  TRANSACTION"));

            var instance = new SqliteTransaction(database.Object);

            database.VerifyAll();
        }

        [Test]
        public void SqliteTransaction_DatabaseIsNullWithBehavior_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqliteTransaction(null, TransactionBehavior.Deferred));
        }

        [TestCase(TransactionBehavior.Deferred, "DEFERRED")]
        [TestCase(TransactionBehavior.Exclusive, "EXCLUSIVE")]
        [TestCase(TransactionBehavior.Immediate, "IMMEDIATE")]
        public void SqliteTransaction_WithBehavior_QualifierOnStatement(TransactionBehavior behavior, string qualifer)
        {
            database.Setup(e => e.Execute("BEGIN {0} TRANSACTION", qualifer));

            var instance = new SqliteTransaction(database.Object, behavior);

            database.VerifyAll();
        }

        [Test]
        public void Dispose_NotCommitted_Rollback()
        {
            database.Setup(e => e.Execute("BEGIN  TRANSACTION"));
            database.Setup(e => e.Execute("ROLLBACK"));

            var instance = new SqliteTransaction(database.Object);
            instance.Dispose();

            database.VerifyAll();
        }

        [Test]
        public void Dispose_RolledBack_Rollback()
        {
            database.Setup(e => e.Execute("BEGIN  TRANSACTION"));
            database.Setup(e => e.Execute("ROLLBACK"));

            var instance = new SqliteTransaction(database.Object);
            instance.Rollback();
            instance.Dispose();

            database.VerifyAll();
        }

        [Test]
        public void Dispose_Committed_Commit()
        {
            database.Setup(e => e.Execute("BEGIN  TRANSACTION"));
            database.Setup(e => e.Execute("COMMIT"));

            var instance = new SqliteTransaction(database.Object);
            instance.Commit();
            instance.Dispose();

            database.VerifyAll();
        }

        [Test]
        public void Dispose_DisposeCalledMoreThanOnce_NoEffect()
        {
            database.Setup(e => e.Execute("BEGIN  TRANSACTION"));
            database.Setup(e => e.Execute("ROLLBACK"));

            var instance = new SqliteTransaction(database.Object);
            instance.Dispose();
            instance.Dispose();

            database.VerifyAll();
        }

        [Test]
        public void Commit_Committed_Commit()
        {
            database.Setup(e => e.Execute("BEGIN  TRANSACTION"));
            database.Setup(e => e.Execute("COMMIT"));

            var instance = new SqliteTransaction(database.Object);
            instance.Commit();
            instance.Dispose();

            database.VerifyAll();
        }

        [Test]
        public void Commit_RollbackThenCommit_ThrowsException()
        {
            database.Setup(e => e.Execute("BEGIN  TRANSACTION"));

            var instance = new SqliteTransaction(database.Object);
            instance.Rollback();
            Assert.Throws<Exception>(() => instance.Commit());

            database.VerifyAll();
        }

        [Test]
        public void Rollback_RolledBack_Rollback()
        {
            database.Setup(e => e.Execute("BEGIN  TRANSACTION"));
            database.Setup(e => e.Execute("ROLLBACK"));

            var instance = new SqliteTransaction(database.Object);
            instance.Rollback();
            instance.Dispose();

            database.VerifyAll();
        }

        [Test]
        public void Rollback_CommitThenRollback_ThrowsException()
        {
            database.Setup(e => e.Execute("BEGIN  TRANSACTION"));

            var instance = new SqliteTransaction(database.Object);
            instance.Commit();
            Assert.Throws<Exception>(() => instance.Rollback());

            database.VerifyAll();
        }
    }
}
