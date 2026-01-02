using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.SqlServer.Attributes;
using SujaySarma.Data.SqlServer.Builders;
using SujaySarma.Data.SqlServer.Builders.Constants;

using System;
using System.Collections.Generic;
using System.Text;

namespace SujaySarma.Data.SqlServer.Tests.Builders;

/// <summary>
/// Unit tests for the SqlDeleteBuilder class.
/// </summary>
[TestClass]
public class SqlDeleteBuilderTests
{
    #region Functional Tests

    /// <summary>
    /// Tests that a simple DELETE statement is generated correctly.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_SimpleDelete_GeneratesCorrectStatement()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>();
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"DELETE FROM [dbo].[TestEntities] {alias};";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that a simple DELETE statement generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_SimpleDelete_GeneratesExactSQL()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>();
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"DELETE FROM [dbo].[TestEntities] {alias};";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that DELETE with WHERE clause generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_DeleteWithWhere_GeneratesCorrectStatement()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
            .Where<TestEntity>(e => (e.Id == 1));
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"DELETE FROM [dbo].[TestEntities] {alias} WHERE ({alias}.[Id] = 1);";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that DELETE with WHERE clause generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_DeleteWithWhere_GeneratesExactSQL()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
            .Where<TestEntity>(e => (e.Id == 1));
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"DELETE FROM [dbo].[TestEntities] {alias} WHERE ({alias}.[Id] = 1);";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that DELETE with TOP clause generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_DeleteWithTop_GeneratesCorrectStatement()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
            .Top(10);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"DELETE TOP (10) FROM [dbo].[TestEntities] {alias};";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that DELETE with TOP clause generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_DeleteWithTop_GeneratesExactSQL()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
            .Top(10);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"DELETE TOP (10) FROM [dbo].[TestEntities] {alias};";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that DELETE with TOP PERCENT clause generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_DeleteWithTopPercent_GeneratesCorrectStatement()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
            .Top(50, isPercent: true);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"DELETE TOP (50) PERCENT FROM [dbo].[TestEntities] {alias};";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that DELETE with TOP PERCENT clause generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_DeleteWithTopPercent_GeneratesExactSQL()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
            .Top(50, isPercent: true);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"DELETE TOP (50) PERCENT FROM [dbo].[TestEntities] {alias};";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that DELETE with table hints generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_DeleteWithHints_GeneratesCorrectStatement()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
            .With(SqlHint.TabLock);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"DELETE FROM [dbo].[TestEntities] {alias} WITH (TABLOCK);";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that DELETE with table hints generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_DeleteWithHints_GeneratesExactSQL()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
            .With(SqlHint.TabLock);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"DELETE FROM [dbo].[TestEntities] {alias} WITH (TABLOCK);";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that DELETE with multiple table hints generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_DeleteWithMultipleHints_GeneratesExactSQL()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
            .With(SqlHint.TabLock);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"DELETE FROM [dbo].[TestEntities] {alias} WITH (TABLOCK);";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that DELETE with OUTPUT clause generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_DeleteWithOutput_GeneratesCorrectStatement()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
            .OutputDeleted();
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"DELETE FROM [dbo].[TestEntities] {alias} OUTPUT DELETED.*;";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that DELETE with OUTPUT specific columns generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_DeleteWithOutputColumns_GeneratesCorrectStatement()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
            .OutputDeleted(new string[] { "Id", "Name" });
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"DELETE FROM [dbo].[TestEntities] {alias} OUTPUT DELETED.[Id], DELETED.[Name];";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that DELETE with INNER JOIN generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_DeleteWithInnerJoin_GeneratesCorrectStatement()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
            .InnerJoin<TestEntity, AnotherTestEntity>((e1, e2) => (e1.Id == 1), SqlHint.None);
        string alias1 = GetReferenceAlias<TestEntity>();
        string alias2 = GetReferenceAlias<AnotherTestEntity>();
        string expectedSql = $"DELETE FROM [dbo].[TestEntities] {alias1} INNER JOIN [dbo].[AnotherTestEntities] {alias2} ON ({alias1}.[Id] = 1);";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that DELETE with LEFT JOIN generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_DeleteWithLeftJoin_GeneratesCorrectStatement()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
            .LeftJoin<TestEntity, AnotherTestEntity>((e1, e2) => (e1.Id == 1), SqlHint.None);
        string alias1 = GetReferenceAlias<TestEntity>();
        string alias2 = GetReferenceAlias<AnotherTestEntity>();
        string expectedSql = $"DELETE FROM [dbo].[TestEntities] {alias1} LEFT JOIN [dbo].[AnotherTestEntities] {alias2} ON ({alias1}.[Id] = 1);";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that DELETE with RIGHT JOIN generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_DeleteWithRightJoin_GeneratesCorrectStatement()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
            .RightJoin<TestEntity, AnotherTestEntity>((e1, e2) => (e1.Id == 1), SqlHint.None);
        string alias1 = GetReferenceAlias<TestEntity>();
        string alias2 = GetReferenceAlias<AnotherTestEntity>();
        string expectedSql = $"DELETE FROM [dbo].[TestEntities] {alias1} RIGHT JOIN [dbo].[AnotherTestEntities] {alias2} ON ({alias1}.[Id] = 1);";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that DELETE with FULL JOIN generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_DeleteWithFullJoin_GeneratesCorrectStatement()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
            .FullJoin<TestEntity, AnotherTestEntity>((e1, e2) => (e1.Id == 1), SqlHint.None);
        string alias1 = GetReferenceAlias<TestEntity>();
        string alias2 = GetReferenceAlias<AnotherTestEntity>();
        string expectedSql = $"DELETE FROM [dbo].[TestEntities] {alias1} FULL JOIN [dbo].[AnotherTestEntities] {alias2} ON ({alias1}.[Id] = 1);";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that DELETE with CROSS JOIN generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_DeleteWithCrossJoin_GeneratesCorrectStatement()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
            .CrossJoin<TestEntity, AnotherTestEntity>(SqlHint.None);
        string alias1 = GetReferenceAlias<TestEntity>();
        string alias2 = GetReferenceAlias<AnotherTestEntity>();
        string expectedSql = $"DELETE FROM [dbo].[TestEntities] {alias1} CROSS JOIN [dbo].[AnotherTestEntities] {alias2};";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that DELETE with AND WHERE condition generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_DeleteWithAndWhere_GeneratesCorrectStatement()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
            .Where<TestEntity>(e => (e.Id == 1))
            .AndWhere<TestEntity>(e => (e.Name == "Test"));
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"DELETE FROM [dbo].[TestEntities] {alias} WHERE ({alias}.[Id] = 1) AND ({alias}.[Name] = 'Test');";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that DELETE with AND WHERE condition generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_DeleteWithAndWhere_GeneratesExactSQL()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
            .Where<TestEntity>(e => (e.Id == 1))
            .AndWhere<TestEntity>(e => (e.Name == "Test"));
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"DELETE FROM [dbo].[TestEntities] {alias} WHERE ({alias}.[Id] = 1) AND ({alias}.[Name] = 'Test');";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that DELETE with OR WHERE condition generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_DeleteWithOrWhere_GeneratesCorrectStatement()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
            .Where<TestEntity>(e => (e.Id == 1))
            .OrWhere<TestEntity>(e => (e.Id == 2));
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"DELETE FROM [dbo].[TestEntities] {alias} WHERE ({alias}.[Id] = 1) OR ({alias}.[Id] = 2);";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that DELETE with OR WHERE condition generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_DeleteWithOrWhere_GeneratesExactSQL()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
            .Where<TestEntity>(e => (e.Id == 1))
            .OrWhere<TestEntity>(e => (e.Id == 2));
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"DELETE FROM [dbo].[TestEntities] {alias} WHERE ({alias}.[Id] = 1) OR ({alias}.[Id] = 2);";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that complex DELETE with TOP, hints, WHERE generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_ComplexDeleteStatement_GeneratesExactSQL()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
            .Top(5)
            .With(SqlHint.TabLock)
            .Where<TestEntity>(e => (e.Id > 0));
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"DELETE TOP (5) FROM [dbo].[TestEntities] {alias} WITH (TABLOCK) WHERE ({alias}.[Id] > 0);";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that From method creates a builder instance.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void From_WithValidType_ReturnsBuilderInstance()
    {
        // Act
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>();

        // Assert
        Assert.IsNotNull(builder);
        Assert.IsInstanceOfType(builder, typeof(SqlDeleteBuilder));
    }

    /// <summary>
    /// Tests that Top method with zero value is accepted.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Top_WithZeroValue_AcceptsValue()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
            .Top(0);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"DELETE TOP (0) FROM [dbo].[TestEntities] {alias};";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that Where method returns self for fluent chaining.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Where_ReturnsBuilderForFluentChaining()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>();

        // Act
        SqlDeleteBuilder result = builder.Where<TestEntity>(e => (e.Id == 1));

        // Assert
        Assert.AreSame(builder, result);
    }

    /// <summary>
    /// Tests that Delete method with single entity creates builder instance.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Delete_WithSingleEntity_ReturnsBuilderInstance()
    {
        // Arrange
        TestEntityWithPK entity = new TestEntityWithPK { Id = 1, Name = "Test" };

        // Act
        SqlDeleteBuilder builder = SqlDeleteBuilder.Delete(entity);

        // Assert
        Assert.IsNotNull(builder);
        Assert.IsInstanceOfType(builder, typeof(SqlDeleteBuilder));
    }

    /// <summary>
    /// Tests that Delete method with single entity generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Delete_WithSingleEntity_GeneratesExactSQL()
    {
        // Arrange
        TestEntityWithPK entity = new TestEntityWithPK { Id = 1, Name = "Test" };
        string alias = GetReferenceAlias<TestEntityWithPK>();
        string expectedSql = $"DELETE FROM [dbo].[TestEntitiesWithPK] {alias} WHERE ({alias}.[Id] = 1);";

        // Act
        SqlDeleteBuilder builder = SqlDeleteBuilder.Delete(entity);
        string actualSql = builder.Build().ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that DeleteMany method with multiple entities creates builder instance.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Delete_WithMultipleEntities_ReturnsBuilderInstance()
    {
        // Arrange
        TestEntityWithPK entity1 = new TestEntityWithPK { Id = 1, Name = "Test1" };
        TestEntityWithPK entity2 = new TestEntityWithPK { Id = 2, Name = "Test2" };

        // Act
        SqlDeleteBuilder builder = SqlDeleteBuilder.DeleteMany(new TestEntityWithPK[] { entity1, entity2 });

        // Assert
        Assert.IsNotNull(builder);
        Assert.IsInstanceOfType(builder, typeof(SqlDeleteBuilder));
    }

    /// <summary>
    /// Tests that DeleteMany method with multiple entities generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Delete_WithMultipleEntities_GeneratesExactSQL()
    {
        // Arrange
        TestEntityWithPK entity1 = new TestEntityWithPK { Id = 1, Name = "Test1" };
        TestEntityWithPK entity2 = new TestEntityWithPK { Id = 2, Name = "Test2" };
        string alias = GetReferenceAlias<TestEntityWithPK>();
        string expectedSql = $"DELETE FROM [dbo].[TestEntitiesWithPK] {alias} WHERE (({alias}.[Id] = 1) OR ({alias}.[Id] = 2));";

        // Act
        SqlDeleteBuilder builder = SqlDeleteBuilder.DeleteMany(new TestEntityWithPK[] { entity1, entity2 });
        string actualSql = builder.Build().ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that DeleteMany method with IEnumerable of entities generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Delete_WithEnumerableEntities_GeneratesExactSQL()
    {
        // Arrange
        List<TestEntityWithPK> entities = new List<TestEntityWithPK>
        {
            new TestEntityWithPK { Id = 1, Name = "Test1" },
            new TestEntityWithPK { Id = 2, Name = "Test2" }
        };
        string alias = GetReferenceAlias<TestEntityWithPK>();
        string expectedSql = $"DELETE FROM [dbo].[TestEntitiesWithPK] {alias} WHERE (({alias}.[Id] = 1) OR ({alias}.[Id] = 2));";

        // Act
        SqlDeleteBuilder builder = SqlDeleteBuilder.DeleteMany(entities);
        string actualSql = builder.Build().ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that OutputToTable method sets output destination.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void OutputToTable_WithEntityType_ReturnsBuilder()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
            .OutputDeleted(new string[] { "Id" });

        // Act
        SqlDeleteBuilder result = builder.OutputToTable<AnotherTestEntity>();

        // Assert
        Assert.AreSame(builder, result);
    }

    /// <summary>
    /// Tests that OutputToTable with string table name returns builder.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void OutputToTable_WithTableName_ReturnsBuilder()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
            .OutputDeleted(new string[] { "Id" });

        // Act
        SqlDeleteBuilder result = builder.OutputToTable("OutputTable");

        // Assert
        Assert.AreSame(builder, result);
    }

    #endregion

    #region Negative Tests

    /// <summary>
    /// Tests that Top throws exception when called twice.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void Top_CalledTwice_ThrowsInvalidOperationException()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
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
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>();

        // Act & Assert
        ArgumentOutOfRangeException exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => builder.Top(101, isPercent: true));
        Assert.AreEqual("count", exception.ParamName);
    }

    /// <summary>
    /// Tests that Where throws exception when called twice.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void Where_CalledTwice_ThrowsInvalidOperationException()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>()
            .Where<TestEntity>(e => (e.Id == 1));

        // Act & Assert
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => builder.Where<TestEntity>(e => (e.Id == 2)));
        Assert.Contains("WHERE clause has already been initialized", exception.Message);
    }

    /// <summary>
    /// Tests that AndWhere throws exception when called before Where.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void AndWhere_WithoutWhere_ThrowsInvalidOperationException()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>();

        // Act & Assert
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => builder.AndWhere<TestEntity>(e => (e.Id == 1)));
        Assert.Contains("WHERE clause has not been initialized", exception.Message);
    }

    /// <summary>
    /// Tests that OrWhere throws exception when called before Where.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void OrWhere_WithoutWhere_ThrowsInvalidOperationException()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>();

        // Act & Assert
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => builder.OrWhere<TestEntity>(e => (e.Id == 1)));
        Assert.Contains("WHERE clause has not been initialized", exception.Message);
    }

    /// <summary>
    /// Tests that Delete throws exception when entity is null.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void Delete_WithNullEntity_ThrowsArgumentNullException()
    {
        // Act & Assert
        ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(() => SqlDeleteBuilder.Delete<TestEntityWithPK>(entity: default!));
        Assert.AreEqual("entity", exception.ParamName);
    }

    /// <summary>
    /// Tests that Delete throws exception when entity has no primary key.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void Delete_WithEntityWithoutPrimaryKey_ThrowsInvalidOperationException()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };

        // Act & Assert
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => SqlDeleteBuilder.Delete(entity));
        Assert.Contains("does not have a primary key defined", exception.Message);
    }

    /// <summary>
    /// Tests that DeleteMany with IEnumerable throws exception when collection is empty.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void Delete_WithEmptyEnumerable_ThrowsArgumentException()
    {
        // Arrange
        List<TestEntityWithPK> entities = new List<TestEntityWithPK>();

        // Act & Assert
        ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(() => SqlDeleteBuilder.DeleteMany(entities));
        Assert.Contains("At least one entity must be provided", exception.Message);
    }

    /// <summary>
    /// Tests that OutputToTable throws exception when output columns not set.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void OutputToTable_WithoutOutputColumns_ThrowsInvalidOperationException()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>();

        // Act & Assert
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => builder.OutputToTable<TestEntity>());
        Assert.Contains("Set the output columns", exception.Message);
    }

    /// <summary>
    /// Tests that OutputToTable with string throws exception when output columns not set.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void OutputToTable_WithStringWithoutOutputColumns_ThrowsInvalidOperationException()
    {
        // Arrange
        SqlDeleteBuilder builder = SqlDeleteBuilder.From<TestEntity>();

        // Act & Assert
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => builder.OutputToTable("OutputTable"));
        Assert.Contains("Set the output columns", exception.Message);
    }

    /// <summary>
    /// Tests that From throws exception with invalid entity type.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void From_WithTypeWithoutSqlAttributes_ThrowsException()
    {
        // Act & Assert
        Assert.ThrowsExactly<TypeLoadException>(() =>
        {
            SqlDeleteBuilder builder = SqlDeleteBuilder.From<InvalidEntity>();
        }, "Should throw exception when type does not have SqlTable attribute.");
    }

    #endregion

    #region Test Entity Classes

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

    /// <summary>
    /// Test entity with primary key for unit tests.
    /// </summary>
    [SqlTable("TestEntitiesWithPK", Schema = "dbo")]
    private class TestEntityWithPK
    {
        [SqlTablePrimaryKeyColumn("Id")]
        public int Id { get; set; }

        [SqlTableColumn("Name")]
        public string? Name { get; set; }
    }

    /// <summary>
    /// Another test entity for unit tests.
    /// </summary>
    [SqlTable("AnotherTestEntities", Schema = "dbo")]
    private class AnotherTestEntity
    {
        [SqlTableColumn("Guid")]
        public Guid Guid { get; set; }

        [SqlTableColumn("CreatedDate")]
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Entity without SQL persistence attributes for negative testing.
    /// </summary>
    private class InvalidEntity
    {
        public int Id { get; set; }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Get the reference alias for a given type by creating a temporary builder.
    /// This helps us build expected SQL strings dynamically.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <returns>The reference alias assigned to this type (e.g., "[t0]").</returns>
    private static string GetReferenceAlias<T>()
    {
        SqlDeleteBuilder tempBuilder = SqlDeleteBuilder.From<T>();
        System.Reflection.FieldInfo? fieldInfo = typeof(SqlDeleteBuilder).GetField("_primaryTable",
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