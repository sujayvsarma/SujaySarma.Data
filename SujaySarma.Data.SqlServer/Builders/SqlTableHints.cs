using System;

namespace SujaySarma.Data.SqlServer.Builders
{
    /// <summary>
    /// Hints passed in to SQL statements to guide the SQL Server with various processing requirements.
    /// </summary>
    [Flags]
    public enum SqlTableHints
    {
        /// <summary>
        /// No table hint is specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// (Appl: Indexed views) Indexed views are not expanded to access underlying tables.
        /// </summary>
        NoExpand = 1,

        /// <summary>
        /// (Appl: INSERT) Any identity values in input dataset are applied to IDENTITY columns in table. 
        /// Otherwise, they are automatically processed as per (SEED, INC)
        /// </summary>
        KeepIdentity = 2,

        /// <summary>
        /// (Appl: INSERT) Insert column's DEFAULT value and not NULL from input dataset.
        /// </summary>
        KeepDefaults = 4,

        /// <summary>
        /// Query optimiser must use Index Scan to access table or view.
        /// </summary>
        ForceScan = 8,

        /// <summary>
        /// Holds shared locks until entire transaction completes.
        /// </summary>
        HoldLock = 16,

        /// <summary>
        /// (Appl: INSERT + BULK + OPENROWSET) BULK Imports ignore constraints on table.
        /// NOTE: UNIQUE, PRIMARY KEY and NOT NULL are *always* enforced.
        /// </summary>
        Ignore_Constraints = 32,

        /// <summary>
        /// (Appl: INSERT + BULK + OPENROWSET) Triggers do not fire during a bulk import.
        /// </summary>
        Ignore_Triggers = 64,

        /// <summary>
        /// Dirty reads are allowed. No shared locks are issued.
        /// </summary>
        NoLock = 128,

        /// <summary>
        /// Returns a message as soon as a LOCK is encountered on a table. Equivalent to 
        /// specifying LOCK_TIMEOUT of zero.
        /// </summary>
        NoWait = 256,

        /// <summary>
        /// Takes page locks.
        /// </summary>
        PagLock = 512,

        /// <summary>
        /// Reads comply with READ COMMITTED isolation level.
        /// </summary>
        ReadCommitted = 1024,

        /// <summary>
        /// Reads comply with READ COMMITTED isolation level using locking.
        /// </summary>
        ReadCommittedLock = 2048,

        /// <summary>
        /// Row-level locks are skipped, page-locks are not skipped. Rows that are locked by other transactions are read.
        /// </summary>
        ReadPast = 4096,

        /// <summary>
        /// Dirty reads are allowed. No shared locks are issued.
        /// </summary>
        ReadUncommitted = NoLock,

        /// <summary>
        /// Sets isolation level to REPEATABLE READ.
        /// </summary>
        RepeatableRead = 8192,

        /// <summary>
        /// Row locks are taken instead of page/table locks.
        /// </summary>
        RowLock = 16384,

        /// <summary>
        /// Holds shared locks until entire transaction completes.
        /// </summary>
        Serializable = HoldLock,

        /// <summary>
        /// Sets SNAPSHOT isolation level.
        /// </summary>
        Snapshot = 32768,

        /// <summary>
        /// Acquires table locks.
        /// </summary>
        TabLock = 65536,

        /// <summary>
        /// Acquires exclusive table locks.
        /// </summary>
        TabLockX = 131072,

        /// <summary>
        /// Update locks are acquired and held until transaction completes. Locks read at row/page level. 
        /// </summary>
        UpdLock = 262144,

        /// <summary>
        /// An exclusive lock is taken.
        /// </summary>
        XLock = 524288
    }
}
