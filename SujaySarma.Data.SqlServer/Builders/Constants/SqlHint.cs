using SujaySarma.Data.Core.ReflectionUtilities;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SujaySarma.Data.SqlServer.Builders.Constants;

/// <summary>
/// Hints passed in to SQL statements to guide the SQL Server with various processing requirements.
/// </summary>
[Flags]
public enum SqlHint
{
    /// <summary>
    /// No table hintToAdd is specified.
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
    /// Identical: NOLOCK.
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
    /// IDENTICAL: HOLDLOCK.
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
    /// UpdateMany locks are acquired and held until transaction completes. Locks read at row/page level. 
    /// </summary>
    UpdLock = 262144,

    /// <summary>
    /// An exclusive lock is taken.
    /// </summary>
    XLock = 524288
}

/// <summary>
/// Extensions to SqlHint.
/// </summary>
internal static class SqlQueryHintExtensions
{
    /// <summary>
    /// Isolation levels
    /// </summary>
    public static SqlHint[] IsolationLevels = new SqlHint[] {
        SqlHint.ReadCommitted,
        SqlHint.ReadCommittedLock,
        SqlHint.ReadUncommitted, SqlHint.NoLock,
        SqlHint.RepeatableRead,
        SqlHint.Serializable,
        SqlHint.Snapshot
    };

    /// <summary>
    /// Lock scopes
    /// </summary>
    public static SqlHint[] LockScopes = new SqlHint[] {
        SqlHint.RowLock,
        SqlHint.PagLock,
        SqlHint.TabLock,
        SqlHint.TabLockX
    };

    /// <summary>
    /// Lock types
    /// </summary>
    public static SqlHint[] LockTypes = new SqlHint[]
    {
        SqlHint.Serializable, SqlHint.HoldLock,
        SqlHint.NoWait,
        SqlHint.UpdLock,
        SqlHint.XLock
    };

    /// <summary>
    /// As good as specifying NOLOCK.
    /// </summary>
    public static SqlHint[] NonLockEquivalents = new SqlHint[]
    {
        SqlHint.NoLock, SqlHint.ReadUncommitted,
        SqlHint.ReadPast
    };

    /// <summary>
    /// Hints applicable only to (SELECT) queries.
    /// </summary>
    public static SqlHint[] ApplicableOnlyToQueries = new SqlHint[]
    {
        SqlHint.NoExpand,
        SqlHint.ReadCommitted,
        SqlHint.ReadCommittedLock,
        SqlHint.ReadPast,
        SqlHint.ReadUncommitted, SqlHint.NoLock,
        SqlHint.RepeatableRead,
        SqlHint.Serializable,
        SqlHint.Snapshot
    };

    /// <summary>
    /// Hints applicable only to INSERT operations.
    /// </summary>
    public static SqlHint[] ApplicableOnlyToInserts = new SqlHint[]
    {
        SqlHint.KeepIdentity,
        SqlHint.KeepDefaults,
        SqlHint.Ignore_Constraints,
        SqlHint.Ignore_Triggers
    };

    /// <summary>
    /// Hints applicable only to UPDATE operations.
    /// </summary>
    public static SqlHint[] ApplicableOnlyToUpdates = new SqlHint[]
    {
        // Currently, no specific hints.
    };

    /// <summary>
    /// Hints applicable only to DELETE operations.
    /// </summary>
    public static SqlHint[] ApplicableOnlyToDeletes = new SqlHint[]
    {
        // Currently, no specific hints.
    };

    /// <summary>
    /// Hints applicable only to MERGE operations.
    /// </summary>
    public static SqlHint[] ApplicableOnlyToMerges = new SqlHint[]
    {
        // Currently, no specific hints.
    };

    /// <summary>
    /// Converts a SqlHint enum value to its SQL Server table hintToAdd string representation for use in WITH clauses.
    /// </summary>
    /// <param name="hint">The table hintToAdd enum value to convert.</param>
    /// <returns>SQL Server table hintToAdd string (e.g., "TABLOCK", "KEEPIDENTITY", "READCOMMITTED").</returns>
    public static string ToSQL(this SqlHint hint)
    {
        return hint switch
        {
            SqlHint.None => string.Empty,
            SqlHint.NoExpand => "NOEXPAND",
            SqlHint.KeepIdentity => "KEEPIDENTITY",
            SqlHint.KeepDefaults => "KEEPDEFAULTS",
            SqlHint.ForceScan => "FORCESCAN",
            SqlHint.HoldLock or SqlHint.Serializable => "HOLDLOCK",
            SqlHint.Ignore_Constraints => "IGNORE_CONSTRAINTS",
            SqlHint.Ignore_Triggers => "IGNORE_TRIGGERS",
            SqlHint.NoLock or SqlHint.ReadUncommitted => "NOLOCK",
            SqlHint.NoWait => "NOWAIT",
            SqlHint.PagLock => "PAGLOCK",
            SqlHint.ReadCommitted => "READCOMMITTED",
            SqlHint.ReadCommittedLock => "READCOMMITTEDLOCK",
            SqlHint.ReadPast => "READPAST",
            SqlHint.RepeatableRead => "REPEATABLEREAD",
            SqlHint.RowLock => "ROWLOCK",
            SqlHint.Snapshot => "SNAPSHOT",
            SqlHint.TabLock => "TABLOCK",
            SqlHint.TabLockX => "TABLOCKX",
            SqlHint.UpdLock => "UPDLOCK",
            SqlHint.XLock => "XLOCK",

            _ => throw new ArgumentOutOfRangeException(nameof(hint), hint, $"Unknown or unsupported SqlHint value: {hint}")
        };
    }

    /// <summary>
    /// Validates that <paramref name="hint"/> has not already been added to <paramref name="hints"/> (the "home" collection), against the 
    /// <paramref name="typeOfStatement"/> type of SQL Statement it is meant for. If the <paramref name="hint"/> has already been added to <paramref name="hints"/>, 
    /// or there is a conflict with an already added hint, the function returns an <paramref name="errorMessage"/> with the details. 
    /// NOTE: Either all hints are added, or none are -- appending hints happens only after all validations are successful.
    /// </summary>
    /// <param name="hints">The "home" collection of hints that <paramref name="hint"/> is to be added to.</param>
    /// <param name="hint">The <see cref="SqlHint"/> to validate and then add to <paramref name="hints"/> collection.</param>
    /// <param name="typeOfStatement">The type of SQL Statement this hint is meant for (used for validation).</param>
    /// <param name="errorMessage">[out] If validation failed, this will contain the appropriate error message.</param>
    /// <returns>TRUE: <paramref name="hint"/> was valid and added to <paramref name="hints"/>. FALSE: <paramref name="hint"/> was NOT valid, NOT added to 
    /// <paramref name="hints"/> and <paramref name="errorMessage"/> contains the error message.</returns>
    public static bool TryAdd(this List<SqlHint> hints, SqlHint hint, SqlStatementType typeOfStatement, [NotNullWhen(false)] out string? errorMessage)
    {
        List<SqlHint> individualFlags = hint.GetIndividualFlags(includeZeroValueFlag: false);
        if (individualFlags.Count > 0)
        {
            foreach (SqlHint flag in individualFlags)
            {
                // Validate for type of statement.
                switch (typeOfStatement)
                {
                    case SqlStatementType.Query:
                        if (ApplicableOnlyToInserts.Contains(flag) || ApplicableOnlyToUpdates.Contains(flag) || ApplicableOnlyToDeletes.Contains(flag) || ApplicableOnlyToMerges.Contains(flag))
                        {
                            errorMessage = $"Hint '{flag}' is not applicable to SELECT queries.";
                            return false;
                        }
                        break;

                    case SqlStatementType.Insert:
                        if (ApplicableOnlyToQueries.Contains(flag) || ApplicableOnlyToUpdates.Contains(flag) || ApplicableOnlyToDeletes.Contains(flag) || ApplicableOnlyToMerges.Contains(flag))
                        {
                            errorMessage = $"Hint '{flag}' is not applicable to INSERT statements.";
                            return false;
                        }
                        break;

                    case SqlStatementType.Update:
                        if (ApplicableOnlyToQueries.Contains(flag) || ApplicableOnlyToInserts.Contains(flag) || ApplicableOnlyToDeletes.Contains(flag) || ApplicableOnlyToMerges.Contains(flag))
                        {
                            errorMessage = $"Hint '{flag}' is not applicable to UPDATE statements.";
                            return false;
                        }
                        break;

                    case SqlStatementType.Delete:
                        if (ApplicableOnlyToQueries.Contains(flag) || ApplicableOnlyToInserts.Contains(flag) || ApplicableOnlyToUpdates.Contains(flag) || ApplicableOnlyToMerges.Contains(flag))
                        {
                            errorMessage = $"Hint '{flag}' is not applicable to DELETE statements.";
                            return false;
                        }
                        break;

                    case SqlStatementType.Merge:
                        if (ApplicableOnlyToInserts.Contains(flag))
                        {
                            errorMessage = $"Hint '{flag}' is not applicable to MERGE statements.";
                            return false;
                        }
                        break;
                }
            }

            List<SqlHint> existingWithNew = hints.Concat(individualFlags).ToList();

            // check if flags are specified more than once.
            if (existingWithNew.GroupBy(h => h).Where(g => g.Count() > 1).Any())
            {
                errorMessage = $"One or more hints specified multiple times.";
                return false;
            }

            // Check for multiple isolation levels
            if (existingWithNew.Where(h => IsolationLevels.Contains(h)).Count() > 1)
            {
                errorMessage = $"Multiple isolation-level hints specified.";
                return false;
            }

            // check for multiple scopes
            if (existingWithNew.Where(h => LockScopes.Contains(h)).Count() > 1)
            {
                errorMessage = $"Multiple lock-scope hints specified.";
                return false;
            }

            // check for multiple types of lock
            if (existingWithNew.Where(h => LockTypes.Contains(h)).Count() > 1)
            {
                errorMessage = $"Multiple lock-type hints specified.";
                return false;
            }

            // check if Lock and Non-lock types are specified together
            if (existingWithNew.Where(h => LockTypes.Contains(h)).Any() &&
                existingWithNew.Where(h => NonLockEquivalents.Contains(h)).Any())
            {
                errorMessage = $"Conflicting lock and non-lock hints specified together.";
                return false;
            }
            
            hints.AddRange(individualFlags);
        }

        errorMessage = null;
        return true;
    }
}