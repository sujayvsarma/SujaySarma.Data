using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.SqlServer.Builders.Internal;

using System;
using System.Collections.Generic;

namespace SujaySarma.Data.SqlServer.Tests.Builders;

/// <summary>
/// Unit tests for the SqlOutput class
/// </summary>
[TestClass]
public class SqlOutputTests
{
    #region AllColumns Tests

    [TestMethod]
    public void AllColumns_Insert_ShouldGenerateInsertedWildcard()
    {
        // Arrange
        SqlOutput output = SqlOutput.WithAllColumns(SqlOutput.EphermeralTableName.INSERTED);

        // Act
        string result = output.ToString();

        // Assert
        Assert.AreEqual("OUTPUT INSERTED.*", result);
    }

    [TestMethod]
    public void AllColumns_Update_ShouldGenerateInsertedAndDeletedWildcards()
    {
        // Arrange
        SqlOutput output = SqlOutput.WithAllColumns(SqlOutput.EphermeralTableName.UPDATED);

        // Act
        string result = output.ToString();

        // Assert
        Assert.AreEqual("OUTPUT INSERTED.*, DELETED.*", result);
    }

    [TestMethod]
    public void AllColumns_Delete_ShouldGenerateDeletedWildcard()
    {
        // Arrange
        SqlOutput output = SqlOutput.WithAllColumns(SqlOutput.EphermeralTableName.DELETED);

        // Act
        string result = output.ToString();

        // Assert
        Assert.AreEqual("OUTPUT DELETED.*", result);
    }

    #endregion

    #region WithColumns Tests

    [TestMethod]
    public void WithColumns_Insert_ShouldPrefixWithInserted()
    {
        // Arrange
        SqlOutput output = SqlOutput.WithColumns(SqlOutput.EphermeralTableName.INSERTED, new string[] { "Id", "Name", "Email" });

        // Act
        string result = output.ToString();

        // Assert
        Assert.AreEqual("OUTPUT INSERTED.[Id], INSERTED.[Name], INSERTED.[Email]", result);
    }

    [TestMethod]
    public void WithColumns_Delete_ShouldPrefixWithDeleted()
    {
        // Arrange
        SqlOutput output = SqlOutput.WithColumns(SqlOutput.EphermeralTableName.DELETED, new string[] { "Id", "Name" });

        // Act
        string result = output.ToString();

        // Assert
        Assert.AreEqual("OUTPUT DELETED.[Id], DELETED.[Name]", result);
    }

    [TestMethod]
    public void WithColumns_Update_ShouldGenerateBothInsertedAndDeleted()
    {
        // Arrange
        SqlOutput output = SqlOutput.WithColumns(SqlOutput.EphermeralTableName.UPDATED, new string[] { "Id", "Name" });

        // Act
        string result = output.ToString();

        // Assert
        Assert.AreEqual("OUTPUT INSERTED.[Id], DELETED.[Id], INSERTED.[Name], DELETED.[Name]", result);
    }

    #endregion

    #region WithAliasedColumns Tests

    [TestMethod]
    public void WithAliasedColumns_Insert_ShouldGenerateAliasedOutput()
    {
        // Arrange
        var columns = new Dictionary<string, string>
        {
            { "Id", "NewId" },
            { "Name", "NewName" }
        };
        SqlOutput output = SqlOutput.WithColumns(SqlOutput.EphermeralTableName.INSERTED, columns);

        // Act
        string result = output.ToString();

        // Assert
        Assert.Contains("INSERTED.[Id] AS [NewId]", result);
        Assert.Contains("INSERTED.[Name] AS [NewName]", result);
    }

    [TestMethod]
    public void WithAliasedColumns_Delete_ShouldGenerateAliasedOutput()
    {
        // Arrange
        var columns = new Dictionary<string, string>
        {
            { "Id", "DeletedId" },
            { "Name", "DeletedName" }
        };
        SqlOutput output = SqlOutput.WithColumns(SqlOutput.EphermeralTableName.DELETED, columns);

        // Act
        string result = output.ToString();

        // Assert
        Assert.Contains("DELETED.[Id] AS [DeletedId]", result);
        Assert.Contains("DELETED.[Name] AS [DeletedName]", result);
    }

    [TestMethod]
    public void WithAliasedColumns_NullDictionary_ShouldThrowArgumentNullException()
    {
        Dictionary<string, string>? columns = null;
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => SqlOutput.WithColumns(SqlOutput.EphermeralTableName.INSERTED, columns!));
    }

    [TestMethod]
    public void WithAliasedColumns_EmptyDictionary_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => SqlOutput.WithColumns(SqlOutput.EphermeralTableName.INSERTED, new Dictionary<string, string>()));
    }

    #endregion

    #region ToTable Tests

    [TestMethod]
    public void ToTable_ValidTableName_ShouldAppendIntoClause()
    {
        // Arrange
        SqlOutput output = SqlOutput.WithAllColumns(SqlOutput.EphermeralTableName.INSERTED);

        // Act
        output.ToTable("AuditLog");
        string result = output.ToString();

        // Assert
        Assert.Contains("INTO", result);
        Assert.Contains("AuditLog", result);
    }

    [TestMethod]
    public void ToTable_WithSchema_ShouldHandleQualifiedTableName()
    {
        // Arrange
        SqlOutput output = SqlOutput.WithAllColumns(SqlOutput.EphermeralTableName.INSERTED);

        // Act
        output.ToTable("dbo.AuditLog");
        string result = output.ToString();

        // Assert
        Assert.Contains("INTO", result);
        Assert.Contains("AuditLog", result);
    }

    [TestMethod]
    public void ToTable_NullTableName_ShouldThrowArgumentNullException()
    {
        // Arrange
        SqlOutput output = SqlOutput.WithAllColumns(SqlOutput.EphermeralTableName.INSERTED);

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => output.ToTable(tableName: null!));
    }

    [TestMethod]
    public void ToTable_EmptyTableName_ShouldThrowArgumentNullException()
    {
        // Arrange
        SqlOutput output = SqlOutput.WithAllColumns(SqlOutput.EphermeralTableName.INSERTED);

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => output.ToTable(string.Empty));
    }

    [TestMethod]
    public void ToTable_WhitespaceTableName_ShouldThrowArgumentNullException()
    {
        // Arrange
        SqlOutput output = SqlOutput.WithAllColumns(SqlOutput.EphermeralTableName.INSERTED);

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => output.ToTable("   "));
    }

    [TestMethod]
    public void ToTable_TableVariable_ShouldThrowArgumentException()
    {
        // Arrange
        SqlOutput output = SqlOutput.WithAllColumns(SqlOutput.EphermeralTableName.INSERTED);

        // Act & Assert - Table variables (@table) are not supported
        Assert.ThrowsExactly<ArgumentException>(() => output.ToTable("@TempTable"));
    }

    [TestMethod]
    public void ToTable_TempTable_ShouldBeAllowed()
    {
        // Arrange
        SqlOutput output = SqlOutput.WithAllColumns(SqlOutput.EphermeralTableName.INSERTED);

        // Act
        output.ToTable("#TempTable");
        string result = output.ToString();

        // Assert
        Assert.Contains("INTO [#TempTable]", result);
    }

    #endregion

    #region Fluent API Tests

    [TestMethod]
    public void FluentAPI_AllColumnsWithTable_ShouldChain()
    {
        // Act
        string result = SqlOutput.WithAllColumns(SqlOutput.EphermeralTableName.INSERTED)
            .ToTable("AuditLog")
            .ToString();

        // Assert
        Assert.Contains("INSERTED.*", result);
        Assert.Contains("INTO", result);
        Assert.Contains("AuditLog", result);
    }

    [TestMethod]
    public void FluentAPI_WithColumnsAndTable_ShouldChain()
    {
        // Act
        string result = SqlOutput.WithColumns(SqlOutput.EphermeralTableName.DELETED, new string[] { "Id", "Name" })
            .ToTable("DeletedRecords")
            .ToString();

        // Assert
        Assert.Contains("INTO", result);
        Assert.Contains("DeletedRecords", result);
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void WithColumns_SingleColumn_Insert_ShouldHandleCorrectly()
    {
        // Arrange
        SqlOutput output = SqlOutput.WithColumns(SqlOutput.EphermeralTableName.INSERTED, new string[] { "Id" });

        // Act
        string result = output.ToString();

        // Assert
        Assert.AreEqual("OUTPUT INSERTED.[Id]", result);
    }

    [TestMethod]
    public void WithColumns_SingleColumn_Update_ShouldGenerateBothVersions()
    {
        // Arrange
        SqlOutput output = SqlOutput.WithColumns(SqlOutput.EphermeralTableName.UPDATED, new string[] { "Status" });

        // Act
        string result = output.ToString();

        // Assert
        Assert.AreEqual("OUTPUT INSERTED.[Status], DELETED.[Status]", result);
    }

    [TestMethod]
    public void ToTable_CalledMultipleTimes_ShouldThrowException()
    {
        // Arrange
        SqlOutput output = SqlOutput.WithAllColumns(SqlOutput.EphermeralTableName.INSERTED);

        // Act
        output.ToTable("FirstTable");

        // Assert
        Assert.ThrowsExactly<InvalidOperationException>(() => output.ToTable("SecondTable"));
    }

    #endregion
}