using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Storage.Markup;

namespace TimeTracker.Storage.Sqlite.UnitTest
{
    [TestFixture]
    class SqliteDatabaseTest
    {
        Mock<ISqlite3> mSqlite3;

        [SetUp]
        public void Setup()
        {
            mSqlite3 = new Mock<ISqlite3>(MockBehavior.Strict);
        }

        [Test]
        public void Constructor_LocationInMemory_OpenCalledCorrectly()
        {
            IntPtr db = IntPtr.Zero;
            IntPtr errMsg = IntPtr.Zero;
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);

            var instance = new SqliteDatabase(Location.Memory, mSqlite3.Object);

            mSqlite3.VerifyAll();
        }

        [Test]
        public void Constructor_LocationInTemporary_OpenCalledCorrectly()
        {
            IntPtr db = IntPtr.Zero;
            IntPtr errMsg = IntPtr.Zero;
            mSqlite3.Setup(e => e.Open16("", out db)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);

            var instance = new SqliteDatabase(Location.Temporary, mSqlite3.Object);

            mSqlite3.VerifyAll();
        }

        [Test]
        public void Construct_OpenFails_ThrowsException()
        {
            IntPtr db = IntPtr.Zero;
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Error);
            mSqlite3.Setup(e => e.Close(db)).Returns(Result.Ok);

            Assert.Throws<Exception>(() => new SqliteDatabase(Location.Memory, mSqlite3.Object));
            mSqlite3.VerifyAll();
        }

        [Test]
        public void Dispose_MethodInvoked_DatabaseClosed()
        {
            IntPtr db = new IntPtr(123);
            IntPtr errMsg = IntPtr.Zero;
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Close(db)).Returns(Result.Ok);

            var instance = new SqliteDatabase(Location.Memory, mSqlite3.Object);

            instance.Dispose();

            mSqlite3.VerifyAll();
        }

        [Test]
        public void Execute_MethodInvokedReturnsOk_NoException()
        {
            string sql = "Hello, World!";

            IntPtr db = new IntPtr(123);
            IntPtr errMsg = IntPtr.Zero;
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, sql, out errMsg)).Returns(Result.Ok);

            var instance = new SqliteDatabase(Location.Memory, mSqlite3.Object);

            Assert.DoesNotThrow(() => instance.Execute(sql));

            mSqlite3.VerifyAll();
        }

        [Test]
        public void Execute_MethodInvokedReturnsError_ThrowsException()
        {
            string sql = "Hello, World!";

            IntPtr db = new IntPtr(123);
            IntPtr errMsg = new IntPtr(456);
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, sql, out errMsg)).Returns(Result.Error);
            mSqlite3.Setup(e => e.Free(errMsg));

            var instance = new SqliteDatabase(Location.Memory, mSqlite3.Object);

            Assert.Throws<SqlException>(() => instance.Execute(sql));

            mSqlite3.VerifyAll();
        }

        [Test]
        public void Prepare_MethodInvokedReturnsOk_ReturnsStatement()
        {
            string sql = "Hello, World!";

            IntPtr db = new IntPtr(123);
            IntPtr errMsg;
            IntPtr stmt = new IntPtr(456);
            string tail = string.Empty;
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Prepare16(db, sql, 26, out stmt, out tail)).Returns(Result.Ok);

            var instance = new SqliteDatabase(Location.Memory, mSqlite3.Object);

            var statement = instance.Prepare(sql);

            Assert.IsAssignableFrom<SqliteStatement>(statement);
            mSqlite3.VerifyAll();
        }

        [Test]
        public void Prepare_MethodInvokedReturnsError_ThrowsException()
        {
            string sql = "Hello, World!";

            IntPtr db = new IntPtr(123);
            IntPtr errMsg;
            IntPtr stmt = new IntPtr(456);
            string tail = string.Empty;
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Prepare16(db, sql, 26, out stmt, out tail)).Returns(Result.Error);
            mSqlite3.Setup(e => e.Finalize(stmt)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Errmsg16(db)).Returns("error");

            var instance = new SqliteDatabase(Location.Memory, mSqlite3.Object);

            Assert.Throws<SqlException>(() => instance.Prepare(sql));

            mSqlite3.VerifyAll();
        }

        [Test]
        public void Prepare_MethodInvokedReturnsMisuse_ThrowsException()
        {
            string sql = "Hello, World!";

            IntPtr db = new IntPtr(123);
            IntPtr errMsg;
            IntPtr stmt = new IntPtr(456);
            string tail = string.Empty;
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Prepare16(db, sql, 26, out stmt, out tail)).Returns(Result.Misuse);
            mSqlite3.Setup(e => e.Finalize(stmt)).Returns(Result.Ok);

            var instance = new SqliteDatabase(Location.Memory, mSqlite3.Object);

            Assert.Throws<SqlException>(() => instance.Prepare(sql));

            mSqlite3.VerifyAll();
        }

        [Test]
        public void CreateTable_TypeDoesNotHaveTableAttribute_ThrowsException()
        {
            IntPtr db = new IntPtr(123);
            IntPtr errMsg;
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);

            var instance = new SqliteDatabase(Location.Memory, mSqlite3.Object);

            Assert.Throws<ArgumentException>(() => instance.CreateTable<TableA>());

            mSqlite3.VerifyAll();
        }

        [Test]
        public void CreateTable_TableAttributeWithNoName_UseNameOfType()
        {
            IntPtr db = new IntPtr(123);
            IntPtr errMsg;
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "CREATE TABLE  TableB ();", out errMsg)).Returns(Result.Ok);

            var instance = new SqliteDatabase(Location.Memory, mSqlite3.Object);
            instance.CreateTable<TableB>();

            mSqlite3.VerifyAll();
        }

        [Test]
        public void CreateTable_TableAttributeWithName_UseNameFromAttribute()
        {
            IntPtr db = new IntPtr(123);
            IntPtr errMsg;
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "CREATE TABLE  ABC ();", out errMsg)).Returns(Result.Ok);

            var instance = new SqliteDatabase(Location.Memory, mSqlite3.Object);
            instance.CreateTable<TableC>();

            mSqlite3.VerifyAll();
        }

        [Test]
        public void CreateTable_TableAttributeWithIfNotExists_QualifierAddedToStatement()
        {
            IntPtr db = new IntPtr(123);
            IntPtr errMsg;
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "CREATE TABLE IF NOT EXISTS TableD ();", out errMsg)).Returns(Result.Ok);

            var instance = new SqliteDatabase(Location.Memory, mSqlite3.Object);
            instance.CreateTable<TableD>();

            mSqlite3.VerifyAll();
        }

        [Test]
        public void CreateTable_ColumnAttributesOnPublicFieldAndProperty_BothAreIncluded()
        {
            IntPtr db = new IntPtr(123);
            IntPtr errMsg;
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "CREATE TABLE  TableE (ColB,ColA);", out errMsg)).Returns(Result.Ok);

            var instance = new SqliteDatabase(Location.Memory, mSqlite3.Object);
            instance.CreateTable<TableE>();

            mSqlite3.VerifyAll();
        }

        [Test]
        public void CreateTable_ColumnAttributesOnPrivateFieldAndProperty_BothAreIncluded()
        {
            IntPtr db = new IntPtr(123);
            IntPtr errMsg;
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "CREATE TABLE  TableF (ColB,ColA);", out errMsg)).Returns(Result.Ok);

            var instance = new SqliteDatabase(Location.Memory, mSqlite3.Object);
            instance.CreateTable<TableF>();

            mSqlite3.VerifyAll();
        }

        [Test]
        public void CreateTable_FieldAndPropertysWithoutColumnAttribute_AreNotIncluded()
        {
            IntPtr db = new IntPtr(123);
            IntPtr errMsg;
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "CREATE TABLE  TableG ();", out errMsg)).Returns(Result.Ok);

            var instance = new SqliteDatabase(Location.Memory, mSqlite3.Object);
            instance.CreateTable<TableG>();

            mSqlite3.VerifyAll();
        }

        [Test]
        public void CreateTable_ColumnAttributesWithName_ColumnNameIsUsedInsteadOfMemberName()
        {
            IntPtr db = new IntPtr(123);
            IntPtr errMsg;
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "CREATE TABLE  TableH (ABC);", out errMsg)).Returns(Result.Ok);

            var instance = new SqliteDatabase(Location.Memory, mSqlite3.Object);
            instance.CreateTable<TableH>();

            mSqlite3.VerifyAll();
        }

        [Test]
        public void CreateTable_ColumnAttributesNotNull_QualifierAddedToColumn()
        {
            IntPtr db = new IntPtr(123);
            IntPtr errMsg;
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "CREATE TABLE  TableI (ColA NOT NULL);", out errMsg)).Returns(Result.Ok);

            var instance = new SqliteDatabase(Location.Memory, mSqlite3.Object);
            instance.CreateTable<TableI>();

            mSqlite3.VerifyAll();
        }

        [Test]
        public void CreateTable_ColumnAttributesUnique_QualifierAddedToColumn()
        {
            IntPtr db = new IntPtr(123);
            IntPtr errMsg;
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "CREATE TABLE  TableJ (ColA UNIQUE);", out errMsg)).Returns(Result.Ok);

            var instance = new SqliteDatabase(Location.Memory, mSqlite3.Object);
            instance.CreateTable<TableJ>();

            mSqlite3.VerifyAll();
        }

        [Test]
        public void CreateTable_ColumnAttributesTypeAffinity_QualifierAddedToColumn()
        {
            IntPtr db = new IntPtr(123);
            IntPtr errMsg;
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "CREATE TABLE  TableK (ColA,ColB INTEGER,ColC NUM,ColD REAL,ColE TEXT);", out errMsg)).Returns(Result.Ok);

            var instance = new SqliteDatabase(Location.Memory, mSqlite3.Object);
            instance.CreateTable<TableK>();

            mSqlite3.VerifyAll();
        }

        [Test]
        public void CreateTable_IndexAttribute_IndexStatementCreated()
        {
            IntPtr db = new IntPtr(123);
            IntPtr errMsg;
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "CREATE TABLE  TableL ();\nCREATE  INDEX  IndexA ON TableL (ColA,ColB);", out errMsg)).Returns(Result.Ok);

            var instance = new SqliteDatabase(Location.Memory, mSqlite3.Object);
            instance.CreateTable<TableL>();

            mSqlite3.VerifyAll();
        }

        [Test]
        public void CreateTable_IndexAttributeWithUnique_QualifierAddedToIndex()
        {
            IntPtr db = new IntPtr(123);
            IntPtr errMsg;
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "CREATE TABLE  TableM ();\nCREATE UNIQUE INDEX  IndexA ON TableM ();", out errMsg)).Returns(Result.Ok);

            var instance = new SqliteDatabase(Location.Memory, mSqlite3.Object);
            instance.CreateTable<TableM>();

            mSqlite3.VerifyAll();
        }

        [Test]
        public void CreateTable_IndexAttributeWithIfNotExists_QualifierAddedToIndex()
        {
            IntPtr db = new IntPtr(123);
            IntPtr errMsg;
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "CREATE TABLE  TableN ();\nCREATE  INDEX IF NOT EXISTS IndexA ON TableN ();", out errMsg)).Returns(Result.Ok);

            var instance = new SqliteDatabase(Location.Memory, mSqlite3.Object);
            instance.CreateTable<TableN>();

            mSqlite3.VerifyAll();
        }

        [TestCase(typeof(TablePK01))]
        [TestCase(typeof(TablePK07))]
        [TestCase(typeof(TablePK08))]
        public void CreateTable_PrimaryKeyAttributeWithInvalidSetup_ThrowsException(Type tableType)
        {
            IntPtr db = new IntPtr(123);
            IntPtr errMsg;
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Ok);

            var instance = new SqliteDatabase(Location.Memory, mSqlite3.Object);
            Assert.Throws<MarkupException>(() => instance.CreateTable(tableType));

            mSqlite3.VerifyAll();
        }

        [TestCase(typeof(TablePK02), "CREATE TABLE  TablePK02 (ColA,ColB,PRIMARY KEY (ColA,ColB));")]
        [TestCase(typeof(TablePK03), "CREATE TABLE  TablePK03 (ColA,ColB,CONSTRAINT PrimaryKeyA PRIMARY KEY (ColA,ColB));")]
        [TestCase(typeof(TablePK04), "CREATE TABLE  TablePK04 (ColA PRIMARY KEY,ColB);")]
        [TestCase(typeof(TablePK05), "CREATE TABLE  TablePK05 (ColA CONSTRAINT PrimaryKeyA PRIMARY KEY,ColB);")]
        [TestCase(typeof(TablePK06), "CREATE TABLE  TablePK06 (ColA PRIMARY KEY AUTOINCREMENT,ColB);")]
        public void CreateTable_PrimaryKeyAttributeWithValidSetup_PrimaryKeyConstraintCreated(Type tableType, string sql)
        {
            IntPtr db = new IntPtr(123);
            IntPtr errMsg;
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, sql, out errMsg)).Returns(Result.Ok);

            var instance = new SqliteDatabase(Location.Memory, mSqlite3.Object);
            instance.CreateTable(tableType);

            mSqlite3.VerifyAll();
        }

        [TestCase(typeof(TableFK01))]
        [TestCase(typeof(TableFK02))]
        [TestCase(typeof(TableFK03))]
        [TestCase(typeof(TableFK04))]
        [TestCase(typeof(TableFK05))]
        public void CreateTable_ForeignKeyAttributeWithInvalidSetup_ThrowsException(Type tableType)
        {
            IntPtr db = new IntPtr(123);
            IntPtr errMsg;
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);

            var instance = new SqliteDatabase(Location.Memory, mSqlite3.Object);
            Assert.Throws<MarkupException>(() => instance.CreateTable(tableType));

            mSqlite3.VerifyAll();
        }

        [TestCase(typeof(TableFK06), "CREATE TABLE  TableFK06 (ColA REFERENCES ForeignTableA);")]
        [TestCase(typeof(TableFK07), "CREATE TABLE  TableFK07 (ColA REFERENCES ForeignTableA (ForeignColA));")]
        [TestCase(typeof(TableFK08), "CREATE TABLE  TableFK08 (ColA CONSTRAINT ForeignKeyA REFERENCES ForeignTableA);")]
        [TestCase(typeof(TableFK09), "CREATE TABLE  TableFK09 (ColA,ColB,FOREIGN KEY (ColA,ColB) REFERENCES ForeignTableA);")]
        [TestCase(typeof(TableFK10), "CREATE TABLE  TableFK10 (ColA,ColB,FOREIGN KEY (ColA,ColB) REFERENCES ForeignTableA (ForeignColA,ForeignColB));")]
        [TestCase(typeof(TableFK11), "CREATE TABLE  TableFK11 (ColA,ColB,CONSTRAINT ForeignKeyA FOREIGN KEY (ColA,ColB) REFERENCES ForeignTableA);")]
        [TestCase(typeof(TableFK12), "CREATE TABLE  TableFK12 (ColA REFERENCES ForeignTableA ON DELETE CASCADE ON UPDATE CASCADE);")]
        [TestCase(typeof(TableFK13), "CREATE TABLE  TableFK13 (ColA REFERENCES ForeignTableA);")]
        [TestCase(typeof(TableFK14), "CREATE TABLE  TableFK14 (ColA REFERENCES ForeignTableA ON DELETE RESTRICT ON UPDATE RESTRICT);")]
        [TestCase(typeof(TableFK15), "CREATE TABLE  TableFK15 (ColA REFERENCES ForeignTableA ON DELETE SET DEFAULT ON UPDATE SET DEFAULT);")]
        [TestCase(typeof(TableFK16), "CREATE TABLE  TableFK16 (ColA REFERENCES ForeignTableA ON DELETE SET NULL ON UPDATE SET NULL);")]
        public void CreateTable_ForeignKeyAttributeWithValidSetup_ForeignKeyConstraintCreated(Type tableType, string sql)
        {
            IntPtr db = new IntPtr(123);
            IntPtr errMsg;
            mSqlite3.Setup(e => e.Open16(":memory:", out db)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, "PRAGMA foreign_keys = ON", out errMsg)).Returns(Result.Ok);
            mSqlite3.Setup(e => e.Exec(db, sql, out errMsg)).Returns(Result.Ok);

            var instance = new SqliteDatabase(Location.Memory, mSqlite3.Object);
            instance.CreateTable(tableType);

            mSqlite3.VerifyAll();
        }

#pragma warning disable 0649,0169

        #region Tables

        class TableA { }

        [Table]
        class TableB { }

        [Table("ABC")]
        class TableC { }

        [Table(IfNotExists = true)]
        class TableD { }

        [Table]
        class TableE
        {
            [Column]
            public int ColA;

            [Column]
            public int ColB { get; set; }
        }

        [Table]
        class TableF
        {
            [Column]
            private int ColA;

            [Column]
            private int ColB { get; set; }
        }

        [Table]
        class TableG
        {
            private int ColA;

            private int ColB { get; set; }
        }

        [Table]
        class TableH
        {
            [Column("ABC")]
            private int ColA;
        }

        [Table]
        class TableI
        {
            [Column(NotNull = true)]
            private int ColA;
        }

        [Table]
        class TableJ
        {
            [Column(Unique = true)]
            private int ColA;
        }

        [Table]
        class TableK
        {
            [Column(TypeAffinity = TypeAffinity.None)]
            private int ColA;

            [Column(TypeAffinity = TypeAffinity.Integer)]
            private int ColB;

            [Column(TypeAffinity = TypeAffinity.Numeric)]
            private int ColC;

            [Column(TypeAffinity = TypeAffinity.Real)]
            private int ColD;

            [Column(TypeAffinity = TypeAffinity.Text)]
            private int ColE;
        }

        [Table]
        [Index("ColA", "ColB", Name = "IndexA")]
        class TableL
        {
        }

        [Table]
        [Index(Name = "IndexA", Unique = true)]
        class TableM
        {
        }

        [Table]
        [Index(Name = "IndexA", IfNotExists = true)]
        class TableN
        {
        }

        #endregion
        #region Primary key tables

        // Throws exception: Empty primary key
        [Table]
        [PrimaryKey()]
        class TablePK01 { }

        // Table constraint primary key
        [Table]
        [PrimaryKey("ColA", "ColB")]
        class TablePK02
        {
            [Column]
            public int ColA;

            [Column]
            public int ColB;
        }

        // Named table constraint primary key
        [Table]
        [PrimaryKey("ColA", "ColB", Name = "PrimaryKeyA")]
        class TablePK03
        {
            [Column]
            public int ColA;

            [Column]
            public int ColB;
        }

        // Column constraint primary key
        [Table]
        [PrimaryKey("ColA")]
        class TablePK04
        {
            [Column]
            public int ColA;

            [Column]
            public int ColB;
        }

        // Column constraint primary key
        [Table]
        [PrimaryKey("ColA", Name = "PrimaryKeyA")]
        class TablePK05
        {
            [Column]
            public int ColA;

            [Column]
            public int ColB;
        }

        // Column constraint primary key
        [Table]
        [PrimaryKey("ColA", AutoIncrement = true)]
        class TablePK06
        {
            [Column]
            public int ColA;

            [Column]
            public int ColB;
        }

        // Throws exception: AutoIncrement not allowed on table constraint
        [Table]
        [PrimaryKey("ColA", "ColB", AutoIncrement = true)]
        class TablePK07 { }

        // Throws exception: Primary key column did not match any columns
        [Table]
        [PrimaryKey("ColA")]
        class TablePK08 { }

        #endregion
        #region Foreign key tables

        // Throws exception: No columns defined
        [Table]
        [ForeignKey(ForeignTable = "ForeignTableA", ForeignColumns = new[] { "ForeignColA" })]
        class TableFK01 { }

        // Throws exception: Columns doesn't match table columns
        [Table]
        [ForeignKey(Columns = new[] { "ColX" }, ForeignTable = "ForeignTableA", ForeignColumns = new[] { "ForeignColA" })]
        class TableFK02
        {
            [Column]
            public int ColA;
        }

        // Throws exception: No foreign table defined
        [Table]
        [ForeignKey(Columns = new[] { "ColA" }, ForeignColumns = new[] { "ForeignColA" })]
        class TableFK03
        {
            [Column]
            public int ColA;
        }

        // Throws exception: Column and foreign column count mismatch
        [Table]
        [ForeignKey(Columns = new[] { "ColA", "ColB" }, ForeignTable = "ForeignTableA", ForeignColumns = new[] { "ForeignColA" })]
        class TableFK04
        {
            [Column]
            public int ColA;

            [Column]
            public int ColB;
        }

        // Throws exception: Column and foreign column count mismatch
        [Table]
        [ForeignKey(Columns = new[] { "ColA" }, ForeignTable = "ForeignTableA", ForeignColumns = new[] { "ForeignColA", "ForeignColB" })]
        class TableFK05
        {
            [Column]
            public int ColA;
        }

        // Column constraint
        [Table]
        [ForeignKey(Columns = new[] { "ColA" }, ForeignTable = "ForeignTableA")]
        class TableFK06
        {
            [Column]
            public int ColA;
        }

        // Column constraint
        [Table]
        [ForeignKey(Columns = new[] { "ColA" }, ForeignTable = "ForeignTableA", ForeignColumns = new[] { "ForeignColA" })]
        class TableFK07
        {
            [Column]
            public int ColA;
        }

        // Named column constraint
        [Table]
        [ForeignKey(Columns = new[] { "ColA" }, ForeignTable = "ForeignTableA", Name = "ForeignKeyA")]
        class TableFK08
        {
            [Column]
            public int ColA;
        }

        // Table constraint
        [Table]
        [ForeignKey(Columns = new[] { "ColA", "ColB" }, ForeignTable = "ForeignTableA")]
        class TableFK09
        {
            [Column]
            public int ColA;

            [Column]
            public int ColB;
        }

        // Table constraint
        [Table]
        [ForeignKey(Columns = new[] { "ColA", "ColB" }, ForeignTable = "ForeignTableA", ForeignColumns = new[] { "ForeignColA", "ForeignColB" })]
        class TableFK10
        {
            [Column]
            public int ColA;

            [Column]
            public int ColB;
        }

        // Named table constraint
        [Table]
        [ForeignKey(Columns = new[] { "ColA", "ColB" }, ForeignTable = "ForeignTableA", Name = "ForeignKeyA")]
        class TableFK11
        {
            [Column]
            public int ColA;

            [Column]
            public int ColB;
        }

        // Column constraint
        [Table]
        [ForeignKey(Columns = new[] { "ColA" }, ForeignTable = "ForeignTableA", OnDelete = ReferenceAction.Cascade, OnUpdate = ReferenceAction.Cascade)]
        class TableFK12
        {
            [Column]
            public int ColA;
        }

        // Column constraint
        [Table]
        [ForeignKey(Columns = new[] { "ColA" }, ForeignTable = "ForeignTableA", OnDelete = ReferenceAction.NoAction, OnUpdate = ReferenceAction.NoAction)]
        class TableFK13
        {
            [Column]
            public int ColA;
        }

        // Column constraint
        [Table]
        [ForeignKey(Columns = new[] { "ColA" }, ForeignTable = "ForeignTableA", OnDelete = ReferenceAction.Restrict, OnUpdate = ReferenceAction.Restrict)]
        class TableFK14
        {
            [Column]
            public int ColA;
        }

        // Column constraint
        [Table]
        [ForeignKey(Columns = new[] { "ColA" }, ForeignTable = "ForeignTableA", OnDelete = ReferenceAction.SetDefault, OnUpdate = ReferenceAction.SetDefault)]
        class TableFK15
        {
            [Column]
            public int ColA;
        }

        // Column constraint
        [Table]
        [ForeignKey(Columns = new[] { "ColA" }, ForeignTable = "ForeignTableA", OnDelete = ReferenceAction.SetNull, OnUpdate = ReferenceAction.SetNull)]
        class TableFK16
        {
            [Column]
            public int ColA;
        }

        #endregion

#pragma warning restore 0649,0169
    }
}
