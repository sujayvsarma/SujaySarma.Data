using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.SqlServer.Attributes;
using SujaySarma.Data.SqlServer.Builders;
using SujaySarma.Data.SqlServer.Builders.Constants;

using System;
using System.Collections.Generic;
using System.Text;

namespace SujaySarma.Data.SqlServer.Tests.Builders;

/// <summary>
/// Unit tests for the <see cref="SqlInsertBuilder"/> class.
/// </summary>
[TestClass]
public class SqlInsertBuilderTests
{
    #region Functional Tests

    /// <summary>
    /// Tests that a simple INSERT statement with a single value is generated correctly.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_SimpleInsertWithValue_GeneratesCorrectStatement()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .Value(entity);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"INSERT INTO [dbo].[TestEntities] {alias} ([Id], [Name]) VALUES (1, 'Test');";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that a simple INSERT statement generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_SimpleInsertWithValue_GeneratesExactSQL()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .Value(entity);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"INSERT INTO [dbo].[TestEntities] {alias} ([Id], [Name]) VALUES (1, 'Test');";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that INSERT with DEFAULT VALUES is generated correctly.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_InsertWithDefaultValues_GeneratesCorrectStatement()
    {
        // Arrange
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .UsingDefaultValues();
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"INSERT INTO [dbo].[TestEntities] {alias} DEFAULT VALUES;";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that INSERT with DEFAULT VALUES generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_InsertWithDefaultValues_GeneratesExactSQL()
    {
        // Arrange
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .UsingDefaultValues();
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"INSERT INTO [dbo].[TestEntities] {alias} DEFAULT VALUES;";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that INSERT with multiple values generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_InsertWithMultipleValues_GeneratesCorrectStatement()
    {
        // Arrange
        TestEntity entity1 = new TestEntity { Id = 1, Name = "Test1" };
        TestEntity entity2 = new TestEntity { Id = 2, Name = "Test2" };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .Values(entity1, entity2);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"INSERT INTO [dbo].[TestEntities] {alias} ([Id], [Name]) VALUES (1, 'Test1'), (2, 'Test2');";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that INSERT with multiple values generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_InsertWithMultipleValues_GeneratesExactSQL()
    {
        // Arrange
        TestEntity entity1 = new TestEntity { Id = 1, Name = "Test1" };
        TestEntity entity2 = new TestEntity { Id = 2, Name = "Test2" };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .Values(new TestEntity[] { entity1, entity2 });
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"INSERT INTO [dbo].[TestEntities] {alias} ([Id], [Name]) VALUES (1, 'Test1'), (2, 'Test2');";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that INSERT with TOP clause generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_InsertWithTop_GeneratesCorrectStatement()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .Top(10)
            .Value(entity);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"INSERT TOP (10) INTO [dbo].[TestEntities] {alias} ([Id], [Name]) VALUES (1, 'Test');";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that INSERT with TOP clause generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_InsertWithTop_GeneratesExactSQL()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .Top(10)
            .Value(entity);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"INSERT TOP (10) INTO [dbo].[TestEntities] {alias} ([Id], [Name]) VALUES (1, 'Test');";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that INSERT with TOP PERCENT clause generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_InsertWithTopPercent_GeneratesCorrectStatement()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .Top(50, isPercent: true)
            .Value(entity);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"INSERT TOP (50) PERCENT INTO [dbo].[TestEntities] {alias} ([Id], [Name]) VALUES (1, 'Test');";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that INSERT with TOP PERCENT clause generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_InsertWithTopPercent_GeneratesExactSQL()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .Top(50, isPercent: true)
            .Value(entity);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"INSERT TOP (50) PERCENT INTO [dbo].[TestEntities] {alias} ([Id], [Name]) VALUES (1, 'Test');";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that INSERT with OUTPUT clause generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_InsertWithOutput_GeneratesCorrectStatement()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .Value(entity)
            .OutputInserted();
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"INSERT INTO [dbo].[TestEntities] {alias} ([Id], [Name]) OUTPUT INSERTED.* VALUES (1, 'Test');";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that INSERT with OUTPUT INSERTED columns generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_InsertWithOutputInsertedColumns_GeneratesCorrectStatement()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .Value(entity)
            .OutputInserted(new string[] { "Id", "Name" });
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"INSERT INTO [dbo].[TestEntities] {alias} ([Id], [Name]) OUTPUT INSERTED.[Id], INSERTED.[Name] VALUES (1, 'Test');";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that INSERT with table hints generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_InsertWithHints_GeneratesCorrectStatement()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .With(SqlHint.TabLock)
            .Value(entity);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"INSERT INTO [dbo].[TestEntities] {alias} WITH (TABLOCK) ([Id], [Name]) VALUES (1, 'Test');";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that INSERT with table hints generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_InsertWithHints_GeneratesExactSQL()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .With(SqlHint.TabLock)
            .Value(entity);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"INSERT INTO [dbo].[TestEntities] {alias} WITH (TABLOCK) ([Id], [Name]) VALUES (1, 'Test');";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that INSERT FROM query generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_InsertFromQuery_GeneratesCorrectStatement()
    {
        // Arrange
        StringBuilder selectQuery = new StringBuilder("SELECT [Id], [Name] FROM [SourceTable]");
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .From(selectQuery);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"INSERT INTO [dbo].[TestEntities] {alias} SELECT [Id], [Name] FROM [SourceTable];";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that INSERT FROM query generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_InsertFromQuery_GeneratesExactSQL()
    {
        // Arrange
        StringBuilder selectQuery = new StringBuilder("SELECT [Id], [Name] FROM [SourceTable]");
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .From(selectQuery);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"INSERT INTO [dbo].[TestEntities] {alias} SELECT [Id], [Name] FROM [SourceTable];";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that INSERT FROM query with column names generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_InsertFromQueryWithColumnNames_GeneratesCorrectStatement()
    {
        // Arrange
        StringBuilder selectQuery = new StringBuilder("SELECT [Id], [Name] FROM [SourceTable]");
        List<string> columns = new List<string> { "Id", "Name" };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .From(selectQuery, columns);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"INSERT INTO [dbo].[TestEntities] {alias} ([Id], [Name]) SELECT [Id], [Name] FROM [SourceTable];";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that INSERT FROM query with column names generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_InsertFromQueryWithColumnNames_GeneratesExactSQL()
    {
        // Arrange
        StringBuilder selectQuery = new StringBuilder("SELECT [Id], [Name] FROM [SourceTable]");
        List<string> columns = new List<string> { "Id", "Name" };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .From(selectQuery, columns);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"INSERT INTO [dbo].[TestEntities] {alias} ([Id], [Name]) SELECT [Id], [Name] FROM [SourceTable];";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that INSERT with dictionary values generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_InsertWithDictionaryValues_GeneratesCorrectStatement()
    {
        // Arrange
        Dictionary<string, object?> values = new Dictionary<string, object?>
        {
            { "Id", 1 },
            { "Name", "Test" }
        };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .Values(values);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"INSERT INTO [dbo].[TestEntities] {alias} ([Id], [Name]) VALUES (1, 'Test');";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that INSERT with dictionary values generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_InsertWithDictionaryValues_GeneratesExactSQL()
    {
        // Arrange
        Dictionary<string, object?> values = new Dictionary<string, object?>
        {
            { "Id", 1 },
            { "Name", "Test" }
        };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .Values(values);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"INSERT INTO [dbo].[TestEntities] {alias} ([Id], [Name]) VALUES (1, 'Test');";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that INSERT with NULL values is properly escaped.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_InsertWithNullValues_GeneratesExactSQL()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = null };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .Value(entity);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"INSERT INTO [dbo].[TestEntities] {alias} ([Id], [Name]) VALUES (1, NULL);";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that INSERT with special characters in values is properly escaped.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_InsertWithSpecialCharacters_GeneratesExactSQL()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test's \"Value\"" };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .Value(entity);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"INSERT INTO [dbo].[TestEntities] {alias} ([Id], [Name]) VALUES (1, 'Test''s \"Value\"');";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that complex INSERT with TOP, hints, and multiple values generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_ComplexInsertStatement_GeneratesExactSQL()
    {
        // Arrange
        TestEntity entity1 = new TestEntity { Id = 1, Name = "First" };
        TestEntity entity2 = new TestEntity { Id = 2, Name = "Second" };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .Top(5)
            .With(SqlHint.TabLock)
            .Values(new TestEntity[] { entity1, entity2 });
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"INSERT TOP (5) INTO [dbo].[TestEntities] {alias} WITH (TABLOCK) ([Id], [Name]) VALUES (1, 'First'), (2, 'Second');";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that INTO method creates a builder instance.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Into_WithValidType_ReturnsBuilderInstance()
    {
        // Act
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>();

        // Assert
        Assert.IsNotNull(builder);
        Assert.IsInstanceOfType(builder, typeof(SqlInsertBuilder));
    }

    /// <summary>
    /// Tests that Top method with zero value is accepted.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Top_WithZeroValue_AcceptsValue()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .Top(0)
            .Value(entity);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"INSERT TOP (0) INTO [dbo].[TestEntities] {alias} ([Id], [Name]) VALUES (1, 'Test');";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that Value method returns self for fluent chaining.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Value_ReturnsBuilderForFluentChaining()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };

        // Act
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>();
        SqlInsertBuilder result = builder.Value(entity);

        // Assert
        Assert.AreSame(builder, result);
    }

    #endregion

    #region Negative Tests

    /// <summary>
    /// Tests that Build throws exception when no values are specified.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void Build_WithNoValues_ThrowsArgumentException()
    {
        // Arrange
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>();

        // Act & Assert
        ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(() => builder.Build());
        Assert.Contains("No values to insert", exception.Message);
    }

    /// <summary>
    /// Tests that UsingDefaultValues throws exception when VALUES already set.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void UsingDefaultValues_AfterValues_ThrowsInvalidOperationException()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .Value(entity);

        // Act & Assert
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => builder.UsingDefaultValues());
        Assert.Contains("DEFAULT VALUES", exception.Message);
    }

    /// <summary>
    /// Tests that UsingDefaultValues throws exception when INSERT FROM already set.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void UsingDefaultValues_AfterInsertFrom_ThrowsInvalidOperationException()
    {
        // Arrange
        StringBuilder selectQuery = new StringBuilder("SELECT Id, Name FROM SourceTable");
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .From(selectQuery);

        // Act & Assert
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => builder.UsingDefaultValues());
        Assert.Contains("INSERT FROM query", exception.Message);
    }

    /// <summary>
    /// Tests that Value throws exception when DEFAULT VALUES already set.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void Value_AfterDefaultValues_ThrowsInvalidOperationException()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .UsingDefaultValues();

        // Act & Assert
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => builder.Value(entity));
        Assert.Contains("DEFAULT VALUES", exception.Message);
    }

    /// <summary>
    /// Tests that Value throws exception when INSERT FROM already set.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void Value_AfterInsertFrom_ThrowsInvalidOperationException()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        StringBuilder selectQuery = new StringBuilder("SELECT Id, Name FROM SourceTable");
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .From(selectQuery);

        // Act & Assert
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => builder.Value(entity));
        Assert.Contains("INSERT FROM query", exception.Message);
    }

    /// <summary>
    /// Tests that Value throws exception when null value provided.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void Value_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>();

        // Act & Assert
        ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(() => builder.Value<TestEntity>(default!));
        Assert.AreEqual("value", exception.ParamName);
    }

    /// <summary>
    /// Tests that Values throws exception when null collection provided.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void Values_WithNullCollection_ThrowsArgumentNullException()
    {
        // Arrange
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>();

        // Act & Assert
        ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(() => builder.Values<TestEntity>(default!));
        Assert.AreEqual("values", exception.ParamName);
    }

    /// <summary>
    /// Tests that Values throws exception when collection contains null element.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void Values_WithNullElementInCollection_ThrowsArgumentNullException()
    {
        // Arrange
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>();
        List<TestEntity> entities = new List<TestEntity> { new TestEntity { Id = 1, Name = "Test" }, default! };

        // Act & Assert
        ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(() => builder.Values(entities));
        Assert.AreEqual("values", exception.ParamName);
    }

    /// <summary>
    /// Tests that Top throws exception when called twice.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void Top_CalledTwice_ThrowsInvalidOperationException()
    {
        // Arrange
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .Top(10);

        // Act & Assert
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => builder.Top(20));
        Assert.Contains("TOP has already been specified", exception.Message);
    }

    /// <summary>
    /// Tests that Top throws exception when percent value exceeds 100.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void Top_WithPercentOver100_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>();

        // Act & Assert
        ArgumentOutOfRangeException exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => builder.Top(101, isPercent: true));
        Assert.AreEqual("count", exception.ParamName);
    }

    /// <summary>
    /// Tests that From throws exception when called twice.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void From_CalledTwice_ThrowsInvalidOperationException()
    {
        // Arrange
        StringBuilder query1 = new StringBuilder("SELECT Id, Name FROM Table1");
        StringBuilder query2 = new StringBuilder("SELECT Id, Name FROM Table2");
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .From(query1);

        // Act & Assert
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => builder.From(query2));
        Assert.Contains("INSERT FROM query has already been set", exception.Message);
    }

    /// <summary>
    /// Tests that From throws exception when DEFAULT VALUES already set.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void From_AfterDefaultValues_ThrowsInvalidOperationException()
    {
        // Arrange
        StringBuilder query = new StringBuilder("SELECT Id, Name FROM SourceTable");
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .UsingDefaultValues();

        // Act & Assert
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => builder.From(query));
        Assert.Contains("DEFAULT VALUES", exception.Message);
    }

    /// <summary>
    /// Tests that From throws exception when VALUES already set.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void From_AfterValues_ThrowsInvalidOperationException()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        StringBuilder query = new StringBuilder("SELECT Id, Name FROM SourceTable");
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .Value(entity);

        // Act & Assert
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => builder.From(query));
        Assert.Contains("actual column values", exception.Message);
    }

    /// <summary>
    /// Tests that From throws exception when query is too short.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void From_WithShortQuery_ThrowsArgumentException()
    {
        // Arrange
        StringBuilder query = new StringBuilder("SELECT");
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>();

        // Act & Assert
        ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(() => builder.From(query));
        Assert.AreEqual("query", exception.ParamName);
    }

    /// <summary>
    /// Tests that From throws exception when column names contain null or whitespace.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void From_WithNullColumnName_ThrowsArgumentException()
    {
        // Arrange
        StringBuilder query = new StringBuilder("SELECT Id, Name FROM SourceTable");
        List<string> columns = new List<string> { "Id", "" };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>();

        // Act & Assert
        ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(() => builder.From(query, columns));
        Assert.AreEqual("columnNames", exception.ParamName);
    }

    /// <summary>
    /// Tests that Values with dictionary throws exception when column count differs.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void Values_WithDifferentColumnCount_ThrowsInvalidOperationException()
    {
        // Arrange
        Dictionary<string, object?> values1 = new Dictionary<string, object?>
        {
            { "Id", 1 },
            { "Name", "Test" }
        };
        Dictionary<string, object?> values2 = new Dictionary<string, object?>
        {
            { "Id", 2 }
        };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .Values(values1);

        // Act & Assert
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => builder.Values(values2));
        Assert.Contains("different number of columns", exception.Message);
    }

    /// <summary>
    /// Tests that Values with dictionary throws exception when column names differ.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void Values_WithDifferentColumnNames_ThrowsInvalidOperationException()
    {
        // Arrange
        Dictionary<string, object?> values1 = new Dictionary<string, object?>
        {
            { "Id", 1 },
            { "Name", "Test" }
        };
        Dictionary<string, object?> values2 = new Dictionary<string, object?>
        {
            { "Id", 2 },
            { "Description", "Test" }
        };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .Values(values1);

        // Act & Assert
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => builder.Values(values2));
        Assert.Contains("different column names", exception.Message);
    }

    /// <summary>
    /// Tests that OutputToTable throws exception when output columns not set.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void OutputToTable_WithoutOutputColumns_ThrowsInvalidOperationException()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlInsertBuilder builder = SqlInsertBuilder.Into<TestEntity>()
            .Value(entity);

        // Act & Assert
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => builder.OutputToTable<TestEntity>());
        Assert.Contains("Set the output columns", exception.Message);
    }

    #endregion

    #region Test Entity

    /// <summary>
    /// Test entity for unit tests.
    /// </summary>
    [SqlTable("TestEntities", Schema = "dbo")]
    private class TestEntity
    {
        [SqlTableColumn("Id")]
        public int Id { get; set; }

        [SqlTableColumn("Name")]
        public string? Name { get; set; }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Get the reference alias for a given type by creating a temporary builder.
    /// This helps us build expected SQL strings dynamically.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <returns>The reference alias assigned to this type (e.g., "[T1]").</returns>
    private static string GetReferenceAlias<T>()
    {
        SqlQueryBuilder tempBuilder = SqlQueryBuilder.From<T>();
        System.Reflection.FieldInfo? fieldInfo = typeof(SqlQueryBuilder).GetField("_primaryTable",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        object? primaryTable = fieldInfo?.GetValue(tempBuilder);
        if (primaryTable is null)
        {
            throw new InvalidOperationException("Unable to retrieve _primaryTable field.");
        }

        System.Reflection.PropertyInfo? aliasProperty = primaryTable.GetType().GetProperty("ReferenceAlias");
        string? alias = aliasProperty?.GetValue(primaryTable) as string;

        if (string.IsNullOrEmpty(alias))
        {
            throw new InvalidOperationException("Unable to retrieve ReferenceAlias.");
        }

        return alias;
    }

    #endregion
}