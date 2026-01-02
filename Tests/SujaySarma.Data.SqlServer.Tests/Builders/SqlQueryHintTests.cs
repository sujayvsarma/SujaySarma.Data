using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.SqlServer.Builders;
using SujaySarma.Data.SqlServer.Builders.Constants;

using System;
using System.Collections.Generic;

namespace SujaySarma.Data.SqlServer.Tests.Builders;

/// <summary>
/// Unit tests for SqlHint enum and SqlQueryHintExtensions class.
/// </summary>
[TestClass]
public class SqlQueryHintTests
{
    #region ToSQL() - Functional Tests

    /// <summary>
    /// Tests that ToSQL returns empty string for NotSet hint.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void ToSQL_WithNone_ReturnsEmptyString()
    {
        // Arrange
        SqlHint hint = SqlHint.None;

        // Act
        string result = hint.ToSQL();

        // Assert
        Assert.AreEqual(string.Empty, result);
        Assert.AreEqual(0, result.Length);
    }

    /// <summary>
    /// Tests that ToSQL returns correct SQL string for NoExpand hint.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void ToSQL_WithNoExpand_ReturnsNOEXPAND()
    {
        // Arrange
        SqlHint hint = SqlHint.NoExpand;

        // Act
        string result = hint.ToSQL();

        // Assert
        Assert.AreEqual("NOEXPAND", result);
        Assert.AreEqual(8, result.Length);
    }

    /// <summary>
    /// Tests that ToSQL returns correct SQL string for KeepIdentity hint.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void ToSQL_WithKeepIdentity_ReturnsKEEPIDENTITY()
    {
        // Arrange
        SqlHint hint = SqlHint.KeepIdentity;

        // Act
        string result = hint.ToSQL();

        // Assert
        Assert.AreEqual("KEEPIDENTITY", result);
        Assert.AreEqual(12, result.Length);
    }

    /// <summary>
    /// Tests that ToSQL returns correct SQL string for KeepDefaults hint.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void ToSQL_WithKeepDefaults_ReturnsKEEPDEFAULTS()
    {
        // Arrange
        SqlHint hint = SqlHint.KeepDefaults;

        // Act
        string result = hint.ToSQL();

        // Assert
        Assert.AreEqual("KEEPDEFAULTS", result);
        Assert.AreEqual(12, result.Length);
    }

    /// <summary>
    /// Tests that ToSQL returns correct SQL string for ForceScan hint.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void ToSQL_WithForceScan_ReturnsFORCESCAN()
    {
        // Arrange
        SqlHint hint = SqlHint.ForceScan;

        // Act
        string result = hint.ToSQL();

        // Assert
        Assert.AreEqual("FORCESCAN", result);
        Assert.AreEqual(9, result.Length);
    }

    /// <summary>
    /// Tests that ToSQL returns correct SQL string for HoldLock hint.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void ToSQL_WithHoldLock_ReturnsHOLDLOCK()
    {
        // Arrange
        SqlHint hint = SqlHint.HoldLock;

        // Act
        string result = hint.ToSQL();

        // Assert
        Assert.AreEqual("HOLDLOCK", result);
        Assert.AreEqual(8, result.Length);
    }

    /// <summary>
    /// Tests that ToSQL returns correct SQL string for Serializable hint.
    /// Serializable is identical to HoldLock.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void ToSQL_WithSerializable_ReturnsHOLDLOCK()
    {
        // Arrange
        SqlHint hint = SqlHint.Serializable;

        // Act
        string result = hint.ToSQL();

        // Assert
        Assert.AreEqual("HOLDLOCK", result);
        Assert.AreEqual(8, result.Length);
    }

    /// <summary>
    /// Tests that ToSQL returns correct SQL string for Ignore_Constraints hint.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void ToSQL_WithIgnoreConstraints_ReturnsIGNORE_CONSTRAINTS()
    {
        // Arrange
        SqlHint hint = SqlHint.Ignore_Constraints;

        // Act
        string result = hint.ToSQL();

        // Assert
        Assert.AreEqual("IGNORE_CONSTRAINTS", result);
        Assert.AreEqual(18, result.Length);
    }

    /// <summary>
    /// Tests that ToSQL returns correct SQL string for Ignore_Triggers hint.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void ToSQL_WithIgnoreTriggers_ReturnsIGNORE_TRIGGERS()
    {
        // Arrange
        SqlHint hint = SqlHint.Ignore_Triggers;

        // Act
        string result = hint.ToSQL();

        // Assert
        Assert.AreEqual("IGNORE_TRIGGERS", result);
        Assert.AreEqual(15, result.Length);
    }

    /// <summary>
    /// Tests that ToSQL returns correct SQL string for NoLock hint.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void ToSQL_WithNoLock_ReturnsNOLOCK()
    {
        // Arrange
        SqlHint hint = SqlHint.NoLock;

        // Act
        string result = hint.ToSQL();

        // Assert
        Assert.AreEqual("NOLOCK", result);
        Assert.AreEqual(6, result.Length);
    }

    /// <summary>
    /// Tests that ToSQL returns correct SQL string for ReadUncommitted hint.
    /// ReadUncommitted is identical to NoLock.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void ToSQL_WithReadUncommitted_ReturnsNOLOCK()
    {
        // Arrange
        SqlHint hint = SqlHint.ReadUncommitted;

        // Act
        string result = hint.ToSQL();

        // Assert
        Assert.AreEqual("NOLOCK", result);
        Assert.AreEqual(6, result.Length);
    }

    /// <summary>
    /// Tests that ToSQL returns correct SQL string for NoWait hint.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void ToSQL_WithNoWait_ReturnsNOWAIT()
    {
        // Arrange
        SqlHint hint = SqlHint.NoWait;

        // Act
        string result = hint.ToSQL();

        // Assert
        Assert.AreEqual("NOWAIT", result);
        Assert.AreEqual(6, result.Length);
    }

    /// <summary>
    /// Tests that ToSQL returns correct SQL string for PagLock hint.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void ToSQL_WithPagLock_ReturnsPAGLOCK()
    {
        // Arrange
        SqlHint hint = SqlHint.PagLock;

        // Act
        string result = hint.ToSQL();

        // Assert
        Assert.AreEqual("PAGLOCK", result);
        Assert.AreEqual(7, result.Length);
    }

    /// <summary>
    /// Tests that ToSQL returns correct SQL string for ReadCommitted hint.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void ToSQL_WithReadCommitted_ReturnsREADCOMMITTED()
    {
        // Arrange
        SqlHint hint = SqlHint.ReadCommitted;

        // Act
        string result = hint.ToSQL();

        // Assert
        Assert.AreEqual("READCOMMITTED", result);
        Assert.AreEqual(13, result.Length);
    }

    /// <summary>
    /// Tests that ToSQL returns correct SQL string for ReadCommittedLock hint.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void ToSQL_WithReadCommittedLock_ReturnsREADCOMMITTEDLOCK()
    {
        // Arrange
        SqlHint hint = SqlHint.ReadCommittedLock;

        // Act
        string result = hint.ToSQL();

        // Assert
        Assert.AreEqual("READCOMMITTEDLOCK", result);
        Assert.AreEqual(17, result.Length);
    }

    /// <summary>
    /// Tests that ToSQL returns correct SQL string for ReadPast hint.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void ToSQL_WithReadPast_ReturnsREADPAST()
    {
        // Arrange
        SqlHint hint = SqlHint.ReadPast;

        // Act
        string result = hint.ToSQL();

        // Assert
        Assert.AreEqual("READPAST", result);
        Assert.AreEqual(8, result.Length);
    }

    /// <summary>
    /// Tests that ToSQL returns correct SQL string for RepeatableRead hint.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void ToSQL_WithRepeatableRead_ReturnsREPEATABLEREAD()
    {
        // Arrange
        SqlHint hint = SqlHint.RepeatableRead;

        // Act
        string result = hint.ToSQL();

        // Assert
        Assert.AreEqual("REPEATABLEREAD", result);
        Assert.AreEqual(14, result.Length);
    }

    /// <summary>
    /// Tests that ToSQL returns correct SQL string for RowLock hint.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void ToSQL_WithRowLock_ReturnsROWLOCK()
    {
        // Arrange
        SqlHint hint = SqlHint.RowLock;

        // Act
        string result = hint.ToSQL();

        // Assert
        Assert.AreEqual("ROWLOCK", result);
        Assert.AreEqual(7, result.Length);
    }

    /// <summary>
    /// Tests that ToSQL returns correct SQL string for Snapshot hint.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void ToSQL_WithSnapshot_ReturnsSNAPSHOT()
    {
        // Arrange
        SqlHint hint = SqlHint.Snapshot;

        // Act
        string result = hint.ToSQL();

        // Assert
        Assert.AreEqual("SNAPSHOT", result);
        Assert.AreEqual(8, result.Length);
    }

    /// <summary>
    /// Tests that ToSQL returns correct SQL string for TabLock hint.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void ToSQL_WithTabLock_ReturnsTABLOCK()
    {
        // Arrange
        SqlHint hint = SqlHint.TabLock;

        // Act
        string result = hint.ToSQL();

        // Assert
        Assert.AreEqual("TABLOCK", result);
        Assert.AreEqual(7, result.Length);
    }

    /// <summary>
    /// Tests that ToSQL returns correct SQL string for TabLockX hint.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void ToSQL_WithTabLockX_ReturnsTABLOCKX()
    {
        // Arrange
        SqlHint hint = SqlHint.TabLockX;

        // Act
        string result = hint.ToSQL();

        // Assert
        Assert.AreEqual("TABLOCKX", result);
        Assert.AreEqual(8, result.Length);
    }

    /// <summary>
    /// Tests that ToSQL returns correct SQL string for UpdLock hint.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void ToSQL_WithUpdLock_ReturnsUPDLOCK()
    {
        // Arrange
        SqlHint hint = SqlHint.UpdLock;

        // Act
        string result = hint.ToSQL();

        // Assert
        Assert.AreEqual("UPDLOCK", result);
        Assert.AreEqual(7, result.Length);
    }

    /// <summary>
    /// Tests that ToSQL returns correct SQL string for XLock hint.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void ToSQL_WithXLock_ReturnsXLOCK()
    {
        // Arrange
        SqlHint hint = SqlHint.XLock;

        // Act
        string result = hint.ToSQL();

        // Assert
        Assert.AreEqual("XLOCK", result);
        Assert.AreEqual(5, result.Length);
    }

    #endregion

    #region ToSQL() - Negative Tests

    /// <summary>
    /// Tests that ToSQL throws ArgumentOutOfRangeException for invalid hint value.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void ToSQL_WithInvalidHint_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        SqlHint invalidHint = (SqlHint)999999;

        // Act & Assert
        ArgumentOutOfRangeException ex = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => invalidHint.ToSQL());
        Assert.Contains("Unknown or unsupported SqlHint value", ex.Message);
        Assert.AreEqual("hint", ex.ParamName);
    }

    /// <summary>
    /// Tests that ToSQL throws ArgumentOutOfRangeException for default hint value.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void ToSQL_WithDefaultHint_ReturnsEmptyString()
    {
        // Arrange
        SqlHint hint = default;

        // Act
        string result = hint.ToSQL();

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    #endregion

    #region TryAdd() - Functional Tests for Query Statement Type

    /// <summary>
    /// Tests that TryAdd successfully adds valid hint for Query statement type.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void TryAdd_WithValidQueryHint_ReturnsTrue()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();
        SqlHint hint = SqlHint.NoLock;

        // Act
        bool result = hints.TryAdd(hint, SqlStatementType.Query, out string? errorMessage);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(errorMessage);
        Assert.HasCount(1, hints);
        Assert.Contains(SqlHint.NoLock, hints);
    }

    /// <summary>
    /// Tests that TryAdd successfully adds multiple compatible hints for Query statement type.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void TryAdd_WithMultipleCompatibleQueryHints_ReturnsTrue()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();

        // Act
        bool result1 = hints.TryAdd(SqlHint.NoLock, SqlStatementType.Query, out string? errorMessage1);
        bool result2 = hints.TryAdd(SqlHint.RowLock, SqlStatementType.Query, out string? errorMessage2);

        // Assert
        Assert.IsTrue(result1);
        Assert.IsNull(errorMessage1);
        Assert.IsTrue(result2);
        Assert.IsNull(errorMessage2);
        Assert.HasCount(2, hints);
    }

    /// <summary>
    /// Tests that TryAdd successfully adds NoExpand hint for Query statement type.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void TryAdd_WithNoExpandForQuery_ReturnsTrue()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();

        // Act
        bool result = hints.TryAdd(SqlHint.NoExpand, SqlStatementType.Query, out string? errorMessage);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(errorMessage);
        Assert.HasCount(1, hints);
        Assert.Contains(SqlHint.NoExpand, hints);
    }

    #endregion

    #region TryAdd() - Functional Tests for Insert Statement Type

    /// <summary>
    /// Tests that TryAdd successfully adds valid hint for Insert statement type.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void TryAdd_WithValidInsertHint_ReturnsTrue()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();
        SqlHint hint = SqlHint.KeepIdentity;

        // Act
        bool result = hints.TryAdd(hint, SqlStatementType.Insert, out string? errorMessage);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(errorMessage);
        Assert.HasCount(1, hints);
        Assert.Contains(SqlHint.KeepIdentity, hints);
    }

    /// <summary>
    /// Tests that TryAdd successfully adds multiple compatible hints for Insert statement type.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void TryAdd_WithMultipleCompatibleInsertHints_ReturnsTrue()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();

        // Act
        bool result1 = hints.TryAdd(SqlHint.KeepIdentity, SqlStatementType.Insert, out string? errorMessage1);
        bool result2 = hints.TryAdd(SqlHint.KeepDefaults, SqlStatementType.Insert, out string? errorMessage2);

        // Assert
        Assert.IsTrue(result1);
        Assert.IsNull(errorMessage1);
        Assert.IsTrue(result2);
        Assert.IsNull(errorMessage2);
        Assert.HasCount(2, hints);
    }

    /// <summary>
    /// Tests that TryAdd successfully adds Ignore_Constraints hint for Insert statement type.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void TryAdd_WithIgnoreConstraintsForInsert_ReturnsTrue()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();

        // Act
        bool result = hints.TryAdd(SqlHint.Ignore_Constraints, SqlStatementType.Insert, out string? errorMessage);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(errorMessage);
        Assert.HasCount(1, hints);
        Assert.Contains(SqlHint.Ignore_Constraints, hints);
    }

    /// <summary>
    /// Tests that TryAdd successfully adds Ignore_Triggers hint for Insert statement type.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void TryAdd_WithIgnoreTriggersForInsert_ReturnsTrue()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();

        // Act
        bool result = hints.TryAdd(SqlHint.Ignore_Triggers, SqlStatementType.Insert, out string? errorMessage);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(errorMessage);
        Assert.HasCount(1, hints);
        Assert.Contains(SqlHint.Ignore_Triggers, hints);
    }

    #endregion

    #region TryAdd() - Functional Tests for Update Statement Type

    /// <summary>
    /// Tests that TryAdd successfully adds valid hint for UpdateMany statement type.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void TryAdd_WithValidUpdateHint_ReturnsTrue()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();
        SqlHint hint = SqlHint.RowLock;

        // Act
        bool result = hints.TryAdd(hint, SqlStatementType.Update, out string? errorMessage);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(errorMessage);
        Assert.HasCount(1, hints);
        Assert.Contains(SqlHint.RowLock, hints);
    }

    #endregion

    #region TryAdd() - Functional Tests for Delete Statement Type

    /// <summary>
    /// Tests that TryAdd successfully adds valid hint for Insert statement type.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void TryAdd_WithValidDeleteHint_ReturnsTrue()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();
        SqlHint hint = SqlHint.TabLock;

        // Act
        bool result = hints.TryAdd(hint, SqlStatementType.Delete, out string? errorMessage);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(errorMessage);
        Assert.HasCount(1, hints);
        Assert.Contains(SqlHint.TabLock, hints);
    }

    #endregion

    #region TryAdd() - Negative Tests for Statement Type Validation

    /// <summary>
    /// Tests that TryAdd rejects INSERT-only hint for Query statement type.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void TryAdd_WithInsertHintForQuery_ReturnsFalse()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();

        // Act
        bool result = hints.TryAdd(SqlHint.KeepIdentity, SqlStatementType.Query, out string? errorMessage);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNotNull(errorMessage);
        Assert.Contains("not applicable to SELECT queries", errorMessage);
        Assert.Contains("KeepIdentity", errorMessage);
        Assert.IsEmpty(hints);
    }

    /// <summary>
    /// Tests that TryAdd rejects Query-only hint for Insert statement type.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void TryAdd_WithQueryHintForInsert_ReturnsFalse()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();

        // Act
        bool result = hints.TryAdd(SqlHint.NoExpand, SqlStatementType.Insert, out string? errorMessage);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNotNull(errorMessage);
        Assert.Contains("not applicable to INSERT statements", errorMessage);
        Assert.Contains("NoExpand", errorMessage);
        Assert.IsEmpty(hints);
    }

    /// <summary>
    /// Tests that TryAdd rejects Query-only hint for UpdateMany statement type.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void TryAdd_WithQueryHintForUpdate_ReturnsFalse()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();

        // Act
        bool result = hints.TryAdd(SqlHint.ReadCommitted, SqlStatementType.Update, out string? errorMessage);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNotNull(errorMessage);
        Assert.Contains("not applicable to UPDATE statements", errorMessage);
        Assert.Contains("ReadCommitted", errorMessage);
        Assert.IsEmpty(hints);
    }

    /// <summary>
    /// Tests that TryAdd rejects INSERT-only hint for Insert statement type.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void TryAdd_WithInsertHintForDelete_ReturnsFalse()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();

        // Act
        bool result = hints.TryAdd(SqlHint.KeepDefaults, SqlStatementType.Delete, out string? errorMessage);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNotNull(errorMessage);
        Assert.Contains("not applicable to DELETE statements", errorMessage);
        Assert.Contains("KeepDefaults", errorMessage);
        Assert.IsEmpty(hints);
    }

    #endregion

    #region TryAdd() - Negative Tests for Duplicate Hints

    /// <summary>
    /// Tests that TryAdd rejects duplicate hints.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void TryAdd_WithDuplicateHint_ReturnsFalse()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();
        hints.TryAdd(SqlHint.NoLock, SqlStatementType.Query, out string? _);

        // Act
        bool result = hints.TryAdd(SqlHint.NoLock, SqlStatementType.Query, out string? errorMessage);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNotNull(errorMessage);
        Assert.Contains("specified multiple times", errorMessage);
        Assert.HasCount(1, hints);
    }

    #endregion

    #region TryAdd() - Negative Tests for Multiple Isolation Levels

    /// <summary>
    /// Tests that TryAdd rejects multiple isolation level hints.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void TryAdd_WithMultipleIsolationLevels_ReturnsFalse()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();
        hints.TryAdd(SqlHint.ReadCommitted, SqlStatementType.Query, out string? _);

        // Act
        bool result = hints.TryAdd(SqlHint.RepeatableRead, SqlStatementType.Query, out string? errorMessage);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNotNull(errorMessage);
        Assert.Contains("Multiple isolation-level hints", errorMessage);
        Assert.HasCount(1, hints);
    }

    /// <summary>
    /// Tests that TryAdd rejects ReadCommitted with Snapshot isolation levels.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void TryAdd_WithReadCommittedAndSnapshot_ReturnsFalse()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();
        hints.TryAdd(SqlHint.ReadCommitted, SqlStatementType.Query, out string? _);

        // Act
        bool result = hints.TryAdd(SqlHint.Snapshot, SqlStatementType.Query, out string? errorMessage);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNotNull(errorMessage);
        Assert.Contains("Multiple isolation-level hints", errorMessage);
    }

    /// <summary>
    /// Tests that TryAdd rejects Serializable with ReadCommittedLock isolation levels.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void TryAdd_WithSerializableAndReadCommittedLock_ReturnsFalse()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();
        hints.TryAdd(SqlHint.Serializable, SqlStatementType.Query, out string? _);

        // Act
        bool result = hints.TryAdd(SqlHint.ReadCommittedLock, SqlStatementType.Query, out string? errorMessage);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNotNull(errorMessage);
        Assert.Contains("Multiple isolation-level hints", errorMessage);
    }

    #endregion

    #region TryAdd() - Negative Tests for Multiple Lock Scopes

    /// <summary>
    /// Tests that TryAdd rejects multiple lock scope hints.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void TryAdd_WithMultipleLockScopes_ReturnsFalse()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();
        hints.TryAdd(SqlHint.RowLock, SqlStatementType.Query, out string? _);

        // Act
        bool result = hints.TryAdd(SqlHint.TabLock, SqlStatementType.Query, out string? errorMessage);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNotNull(errorMessage);
        Assert.Contains("Multiple lock-scope hints", errorMessage);
        Assert.HasCount(1, hints);
    }

    /// <summary>
    /// Tests that TryAdd rejects PagLock with TabLockX scope hints.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void TryAdd_WithPagLockAndTabLockX_ReturnsFalse()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();
        hints.TryAdd(SqlHint.PagLock, SqlStatementType.Update, out string? _);

        // Act
        bool result = hints.TryAdd(SqlHint.TabLockX, SqlStatementType.Update, out string? errorMessage);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNotNull(errorMessage);
        Assert.Contains("Multiple lock-scope hints", errorMessage);
    }

    #endregion

    #region TryAdd() - Negative Tests for Multiple Lock Types

    /// <summary>
    /// Tests that TryAdd rejects multiple lock type hints.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void TryAdd_WithMultipleLockTypes_ReturnsFalse()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();
        hints.TryAdd(SqlHint.UpdLock, SqlStatementType.Update, out string? _);

        // Act
        bool result = hints.TryAdd(SqlHint.XLock, SqlStatementType.Update, out string? errorMessage);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNotNull(errorMessage);
        Assert.Contains("Multiple lock-type hints", errorMessage);
        Assert.HasCount(1, hints);
    }

    /// <summary>
    /// Tests that TryAdd rejects HoldLock with NoWait lock type hints.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void TryAdd_WithHoldLockAndNoWait_ReturnsFalse()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();
        hints.TryAdd(SqlHint.HoldLock, SqlStatementType.Query, out string? _);

        // Act
        bool result = hints.TryAdd(SqlHint.NoWait, SqlStatementType.Query, out string? errorMessage);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNotNull(errorMessage);
        Assert.Contains("Multiple lock-type hints", errorMessage);
    }

    #endregion

    #region TryAdd() - Negative Tests for Conflicting Lock and Non-Lock Hints

    /// <summary>
    /// Tests that TryAdd rejects conflicting lock and non-lock hints.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void TryAdd_WithLockAndNoLock_ReturnsFalse()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();
        hints.TryAdd(SqlHint.UpdLock, SqlStatementType.Query, out string? _);

        // Act
        bool result = hints.TryAdd(SqlHint.NoLock, SqlStatementType.Query, out string? errorMessage);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNotNull(errorMessage);
        Assert.Contains("Conflicting lock and non-lock hints", errorMessage);
        Assert.HasCount(1, hints);
    }

    /// <summary>
    /// Tests that TryAdd rejects XLock with ReadUncommitted hints.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void TryAdd_WithXLockAndReadUncommitted_ReturnsFalse()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();
        hints.TryAdd(SqlHint.XLock, SqlStatementType.Update, out string? _);

        // Act
        bool result = hints.TryAdd(SqlHint.ReadUncommitted, SqlStatementType.Update, out string? errorMessage);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNotNull(errorMessage);
        Assert.Contains("Hint 'ReadUncommitted' is not applicable to UPDATE statements.", errorMessage);
    }

    /// <summary>
    /// Tests that TryAdd rejects Serializable with ReadPast hints.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void TryAdd_WithSerializableAndReadPast_ReturnsFalse()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();
        hints.TryAdd(SqlHint.Serializable, SqlStatementType.Query, out string? _);

        // Act
        bool result = hints.TryAdd(SqlHint.ReadPast, SqlStatementType.Query, out string? errorMessage);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNotNull(errorMessage);
        Assert.Contains("Conflicting lock and non-lock hints", errorMessage);
    }

    #endregion

    #region TryAdd() - Negative Tests with NULL and Default Values

    /// <summary>
    /// Tests that TryAdd handles NotSet hint gracefully.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void TryAdd_WithNoneHint_ReturnsTrue()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();

        // Act
        bool result = hints.TryAdd(SqlHint.None, SqlStatementType.Query, out string? errorMessage);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(errorMessage);
        Assert.IsEmpty(hints);
    }

    /// <summary>
    /// Tests that TryAdd handles default hint gracefully.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void TryAdd_WithDefaultHint_ReturnsTrue()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();
        SqlHint hint = default;

        // Act
        bool result = hints.TryAdd(hint, SqlStatementType.Query, out string? errorMessage);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(errorMessage);
        Assert.IsEmpty(hints);
    }

    /// <summary>
    /// Tests that TryAdd handles default! hint gracefully.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void TryAdd_WithDefaultNonNullHint_ReturnsTrue()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();
        SqlHint hint = default!;

        // Act
        bool result = hints.TryAdd(hint, SqlStatementType.Query, out string? errorMessage);

        // Assert
        Assert.IsTrue(result);
        Assert.IsNull(errorMessage);
        Assert.IsEmpty(hints);
    }

    #endregion

    #region TryAdd() - Performance Tests

    /// <summary>
    /// Tests performance of adding multiple hints sequentially.
    /// </summary>
    [TestMethod]
    [TestCategory("Performance")]
    public void TryAdd_WithMultipleHintsSequentially_CompletesInReasonableTime()
    {
        // Arrange
        List<SqlHint> hints = new List<SqlHint>();
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            hints.Clear();
            hints.TryAdd(SqlHint.NoLock, SqlStatementType.Query, out string? _);
            hints.TryAdd(SqlHint.RowLock, SqlStatementType.Query, out string? _);
            hints.TryAdd(SqlHint.ForceScan, SqlStatementType.Query, out string? _);
        }

        stopwatch.Stop();

        // Assert
        Assert.IsLessThan(1000, stopwatch.ElapsedMilliseconds, $"Operation took {stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
    }

    /// <summary>
    /// Tests performance of ToSQL conversion for all hints.
    /// </summary>
    [TestMethod]
    [TestCategory("Performance")]
    public void ToSQL_ConversionForAllHints_CompletesInReasonableTime()
    {
        // Arrange
        SqlHint[] allHints = new SqlHint[]
        {
            SqlHint.None, SqlHint.NoExpand, SqlHint.KeepIdentity,
            SqlHint.KeepDefaults, SqlHint.ForceScan, SqlHint.HoldLock,
            SqlHint.Ignore_Constraints, SqlHint.Ignore_Triggers, SqlHint.NoLock,
            SqlHint.NoWait, SqlHint.PagLock, SqlHint.ReadCommitted,
            SqlHint.ReadCommittedLock, SqlHint.ReadPast, SqlHint.RepeatableRead,
            SqlHint.RowLock, SqlHint.Snapshot, SqlHint.TabLock,
            SqlHint.TabLockX, SqlHint.UpdLock, SqlHint.XLock
        };

        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 10000; i++)
        {
            foreach (SqlHint hint in allHints)
            {
                string sql = hint.ToSQL();
            }
        }

        stopwatch.Stop();

        // Assert
        Assert.IsLessThan(1000, stopwatch.ElapsedMilliseconds, $"Operation took {stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
    }

    #endregion
}