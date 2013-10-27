using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Storage.Sqlite
{
    internal enum Result
    {
        /// <summary>
        /// Successful result
        /// </summary>
        Ok = 0,

        /// <summary>
        /// SQL error or missing database
        /// </summary>
        Error = 1,

        /// <summary>
        /// Internal logic error in SQLite
        /// </summary>
        Internal = 2,

        /// <summary>
        /// Access permission denied
        /// </summary>
        Perm = 3,

        /// <summary>
        /// Callback routine requested an abort
        /// </summary>
        Abort = 4,

        /// <summary>
        /// The database file is locked
        /// </summary>
        Busy = 5,

        /// <summary>
        /// A table in the database is locked
        /// </summary>
        Locked = 6,

        /// <summary>
        /// A malloc() failed
        /// </summary>
        NoMem = 7,

        /// <summary>
        /// Attempt to write a readonly database
        /// </summary>
        ReadOnly = 8,

        /// <summary>
        /// Operation terminated by sqlite3_interrupt()
        /// </summary>
        Interrupt = 9,

        /// <summary>
        /// Some kind of disk I/O error occurred
        /// </summary>
        IOErr = 10,

        /// <summary>
        /// The database disk image is malformed
        /// </summary>
        Corrupt = 11,

        /// <summary>
        /// Unknown opcode in sqlite3_file_control()
        /// </summary>
        NotFound = 12,

        /// <summary>
        /// Insertion failed because database is full
        /// </summary>
        Full = 13,

        /// <summary>
        /// Unable to open the database file
        /// </summary>
        CantOpen = 14,

        /// <summary>
        /// Database lock protocol error
        /// </summary>
        Protocol = 15,

        /// <summary>
        /// Database is empty
        /// </summary>
        Empty = 16,

        /// <summary>
        /// The database schema changed
        /// </summary>
        Schema = 17,

        /// <summary>
        /// String or BLOB exceeds size limit
        /// </summary>
        TooBig = 18,

        /// <summary>
        /// Abort due to constraint violation
        /// </summary>
        Constraint = 19,

        /// <summary>
        /// Data type mismatch
        /// </summary>
        Mismatch = 20,

        /// <summary>
        /// Library used incorrectly
        /// </summary>
        Misuse = 21,

        /// <summary>
        /// Uses OS features not supported on host
        /// </summary>
        NoLFS = 22,

        /// <summary>
        /// Authorization denied
        /// </summary>
        Auth = 23,

        /// <summary>
        /// Auxiliary database format error
        /// </summary>
        Format = 24,

        /// <summary>
        /// 2nd parameter to sqlite3_bind out of range
        /// </summary>
        Range = 25,

        /// <summary>
        /// File opened that is not a database file
        /// </summary>
        NotADB = 26,

        /// <summary>
        /// Notifications from sqlite3_log()
        /// </summary>
        Notice = 27,

        /// <summary>
        /// Warnings from sqlite3_log()
        /// </summary>
        Warning = 28,

        /// <summary>
        /// sqlite3_step() has another row ready
        /// </summary>
        Row = 100,

        /// <summary>
        /// sqlite3_step() has finished executing
        /// </summary>
        Done = 101,

        #region Extended Result Codes

        IOERR_READ = (IOErr | (1 << 8)),
        IOERR_SHORT_READ = (IOErr | (2 << 8)),
        IOERR_WRITE = (IOErr | (3 << 8)),
        IOERR_FSYNC = (IOErr | (4 << 8)),
        IOERR_DIR_FSYNC = (IOErr | (5 << 8)),
        IOERR_TRUNCATE = (IOErr | (6 << 8)),
        IOERR_FSTAT = (IOErr | (7 << 8)),
        IOERR_UNLOCK = (IOErr | (8 << 8)),
        IOERR_RDLOCK = (IOErr | (9 << 8)),
        IOERR_DELETE = (IOErr | (10 << 8)),
        IOERR_BLOCKED = (IOErr | (11 << 8)),
        IOERR_NOMEM = (IOErr | (12 << 8)),
        IOERR_ACCESS = (IOErr | (13 << 8)),
        IOERR_CHECKRESERVEDLOCK = (IOErr | (14 << 8)),
        IOERR_LOCK = (IOErr | (15 << 8)),
        IOERR_CLOSE = (IOErr | (16 << 8)),
        IOERR_DIR_CLOSE = (IOErr | (17 << 8)),
        IOERR_SHMOPEN = (IOErr | (18 << 8)),
        IOERR_SHMSIZE = (IOErr | (19 << 8)),
        IOERR_SHMLOCK = (IOErr | (20 << 8)),
        IOERR_SHMMAP = (IOErr | (21 << 8)),
        IOERR_SEEK = (IOErr | (22 << 8)),
        IOERR_DELETE_NOENT = (IOErr | (23 << 8)),
        IOERR_MMAP = (IOErr | (24 << 8)),
        IOERR_GETTEMPPATH = (IOErr | (25 << 8)),
        LOCKED_SHAREDCACHE = (Locked | (1 << 8)),
        BUSY_RECOVERY = (Busy | (1 << 8)),
        BUSY_SNAPSHOT = (Busy | (2 << 8)),
        CANTOPEN_NOTEMPDIR = (CantOpen | (1 << 8)),
        CANTOPEN_ISDIR = (CantOpen | (2 << 8)),
        CANTOPEN_FULLPATH = (CantOpen | (3 << 8)),
        CORRUPT_VTAB = (Corrupt | (1 << 8)),
        READONLY_RECOVERY = (ReadOnly | (1 << 8)),
        READONLY_CANTLOCK = (ReadOnly | (2 << 8)),
        READONLY_ROLLBACK = (ReadOnly | (3 << 8)),
        ABORT_ROLLBACK = (Abort | (2 << 8)),
        CONSTRAINT_CHECK = (Constraint | (1 << 8)),
        CONSTRAINT_COMMITHOOK = (Constraint | (2 << 8)),
        CONSTRAINT_FOREIGNKEY = (Constraint | (3 << 8)),
        CONSTRAINT_FUNCTION = (Constraint | (4 << 8)),
        CONSTRAINT_NOTNULL = (Constraint | (5 << 8)),
        CONSTRAINT_PRIMARYKEY = (Constraint | (6 << 8)),
        CONSTRAINT_TRIGGER = (Constraint | (7 << 8)),
        CONSTRAINT_UNIQUE = (Constraint | (8 << 8)),
        CONSTRAINT_VTAB = (Constraint | (9 << 8)),
        NOTICE_RECOVER_WAL = (Notice | (1 << 8)),
        NOTICE_RECOVER_ROLLBACK = (Notice | (2 << 8)),
        WARNING_AUTOINDEX = (Warning | (1 << 8)),

        #endregion
    }
}
