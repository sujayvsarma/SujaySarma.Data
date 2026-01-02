using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.SqlServer.Attributes;
using SujaySarma.Data.SqlServer.Builders;
using SujaySarma.Data.SqlServer.Builders.Constants;

using System;
using System.Collections.Generic;
using System.Text;

namespace SujaySarma.Data.SqlServer.Tests.Builders;

/// <summary>
/// Unit tests for the <see cref="SqlUpdateBuilder"/> class.
/// </summary>
[TestClass]
public class SqlUpdateBuilderTests
{
    #region Functional Tests

    /// <summary>
    /// Tests that a simple UPDATE statement is generated correctly.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_SimpleUpdate_GeneratesCorrectStatement()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .Set(entity)
            .Where<TestEntity>(e => (e.Id == 1));
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"UPDATE [dbo].[TestEntities] {alias} SET {alias}.[Id] = 1, {alias}.[Name] = 'Test' WHERE ({alias}.[Id] = 1);\r\n";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that a simple UPDATE statement generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_SimpleUpdate_GeneratesExactSQL()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .Set(entity)
            .Where<TestEntity>(e => (e.Id == 1));
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"UPDATE [dbo].[TestEntities] {alias} SET {alias}.[Id] = 1, {alias}.[Name] = 'Test' WHERE ({alias}.[Id] = 1);\r\n";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that UPDATE with TOP clause generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_UpdateWithTop_GeneratesCorrectStatement()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .Top(10)
            .Set(entity)
            .Where<TestEntity>(e => (e.Id == 1));
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"UPDATE TOP (10) [dbo].[TestEntities] {alias} SET {alias}.[Id] = 1, {alias}.[Name] = 'Test' WHERE ({alias}.[Id] = 1);\r\n";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that UPDATE with TOP clause generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_UpdateWithTop_GeneratesExactSQL()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .Top(10)
            .Set(entity)
            .Where<TestEntity>(e => (e.Id == 1));
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"UPDATE TOP (10) [dbo].[TestEntities] {alias} SET {alias}.[Id] = 1, {alias}.[Name] = 'Test' WHERE ({alias}.[Id] = 1);\r\n";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that UPDATE with TOP PERCENT clause generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_UpdateWithTopPercent_GeneratesCorrectStatement()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .Top(50, isPercent: true)
            .Set(entity)
            .Where<TestEntity>(e => (e.Id == 1));
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"UPDATE TOP (50) PERCENT [dbo].[TestEntities] {alias} SET {alias}.[Id] = 1, {alias}.[Name] = 'Test' WHERE ({alias}.[Id] = 1);\r\n";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that UPDATE with TOP PERCENT clause generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_UpdateWithTopPercent_GeneratesExactSQL()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .Top(50, isPercent: true)
            .Set(entity)
            .Where<TestEntity>(e => (e.Id == 1));
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"UPDATE TOP (50) PERCENT [dbo].[TestEntities] {alias} SET {alias}.[Id] = 1, {alias}.[Name] = 'Test' WHERE ({alias}.[Id] = 1);\r\n";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that UPDATE with table hints generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_UpdateWithHints_GeneratesCorrectStatement()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .With(SqlHint.TabLock)
            .Set(entity)
            .Where<TestEntity>(e => (e.Id == 1));
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"UPDATE [dbo].[TestEntities] {alias} WITH (TABLOCK) SET {alias}.[Id] = 1, {alias}.[Name] = 'Test' WHERE ({alias}.[Id] = 1);\r\n";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that UPDATE with table hints generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_UpdateWithHints_GeneratesExactSQL()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .With(SqlHint.TabLock)
            .Set(entity)
            .Where<TestEntity>(e => (e.Id == 1));
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"UPDATE [dbo].[TestEntities] {alias} WITH (TABLOCK) SET {alias}.[Id] = 1, {alias}.[Name] = 'Test' WHERE ({alias}.[Id] = 1);\r\n";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that UPDATE with additional values generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_UpdateWithAdditionalValues_GeneratesCorrectStatement()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        Dictionary<string, System.Linq.Expressions.Expression<Func<TestEntity, object>>> additionalValues =
            new Dictionary<string, System.Linq.Expressions.Expression<Func<TestEntity, object>>>
            {
                { "Description", e => "Additional" }
            };
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .Set(entity, additionalValues)
            .Where<TestEntity>(e => (e.Id == 1));
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"UPDATE [dbo].[TestEntities] {alias} SET {alias}.[Id] = 1, {alias}.[Name] = 'Test', {alias}.[Description] = 'Additional' WHERE ({alias}.[Id] = 1);\r\n";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that UPDATE with OUTPUT clause generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_UpdateWithOutput_GeneratesCorrectStatement()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .Set(entity)
            .OutputUpdated()
            .Where<TestEntity>(e => (e.Id == 1));
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"UPDATE [dbo].[TestEntities] {alias} SET {alias}.[Id] = 1, {alias}.[Name] = 'Test' OUTPUT INSERTED.*, DELETED.* WHERE ({alias}.[Id] = 1);\r\n";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that UPDATE with OUTPUT specific columns generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_UpdateWithOutputColumns_GeneratesCorrectStatement()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .Set(entity)
            .OutputUpdated(new string[] { "Id", "Name" })
            .Where<TestEntity>(e => (e.Id == 1));
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"UPDATE [dbo].[TestEntities] {alias} SET {alias}.[Id] = 1, {alias}.[Name] = 'Test' OUTPUT INSERTED.[Id], DELETED.[Id], INSERTED.[Name], DELETED.[Name] WHERE ({alias}.[Id] = 1);\r\n";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that UPDATE with AND WHERE condition generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_UpdateWithAndWhere_GeneratesCorrectStatement()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .Set(entity)
            .Where<TestEntity>(e => (e.Id == 1))
            .AndWhere<TestEntity>(e => (e.Name == "Old"));
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"UPDATE [dbo].[TestEntities] {alias} SET {alias}.[Id] = 1, {alias}.[Name] = 'Test' WHERE ({alias}.[Id] = 1) AND ({alias}.[Name] = 'Old');\r\n";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that UPDATE with AND WHERE condition generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_UpdateWithAndWhere_GeneratesExactSQL()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .Set(entity)
            .Where<TestEntity>(e => (e.Id == 1))
            .AndWhere<TestEntity>(e => (e.Name == "Old"));
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"UPDATE [dbo].[TestEntities] {alias} SET {alias}.[Id] = 1, {alias}.[Name] = 'Test' WHERE ({alias}.[Id] = 1) AND ({alias}.[Name] = 'Old');\r\n";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that UPDATE with OR WHERE condition generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_UpdateWithOrWhere_GeneratesCorrectStatement()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .Set(entity)
            .Where<TestEntity>(e => (e.Id == 1))
            .OrWhere<TestEntity>(e => (e.Id == 2));
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"UPDATE [dbo].[TestEntities] {alias} SET {alias}.[Id] = 1, {alias}.[Name] = 'Test' WHERE ({alias}.[Id] = 1) OR ({alias}.[Id] = 2);\r\n";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that UPDATE with OR WHERE condition generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_UpdateWithOrWhere_GeneratesExactSQL()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .Set(entity)
            .Where<TestEntity>(e => (e.Id == 1))
            .OrWhere<TestEntity>(e => (e.Id == 2));
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"UPDATE [dbo].[TestEntities] {alias} SET {alias}.[Id] = 1, {alias}.[Name] = 'Test' WHERE ({alias}.[Id] = 1) OR ({alias}.[Id] = 2);\r\n";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that UPDATE with INNER JOIN generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_UpdateWithInnerJoin_GeneratesCorrectStatement()
    {
        // Arrange
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .UpdateFrom<TestEntity, AnotherTestEntity>(
                (t, a) => (t.Id == a.Id),
                SqlHint.None,
                new Dictionary<string, System.Linq.Expressions.Expression<Func<AnotherTestEntity, object>>>
                {
                    { "Name", a => a.Description! }
                });
        string alias1 = GetReferenceAlias<TestEntity>();
        string alias2 = GetReferenceAlias<AnotherTestEntity>();
        string expectedSql = $"UPDATE {alias1} SET {alias1}.[Name] = {alias2}.[Description] FROM [dbo].[TestEntities] {alias1} INNER JOIN [dbo].[AnotherTestEntities] {alias2} ON ({alias1}.[Id] = {alias2}.[Id]);\r\n";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that UPDATE with LEFT JOIN generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_UpdateWithLeftJoin_GeneratesCorrectStatement()
    {
        // Arrange
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .UpdateFrom<TestEntity, AnotherTestEntity>(
                (t, a) => (t.Id == a.Id),
                SqlHint.None,
                new Dictionary<string, System.Linq.Expressions.Expression<Func<AnotherTestEntity, object>>>
                {
                    { "Name", a => a.Description! }
                })
            .LeftJoin<TestEntity, AnotherTestEntity>((t, a) => (t.Id == a.Id), SqlHint.None);
        string alias1 = GetReferenceAlias<TestEntity>();
        string alias2 = GetReferenceAlias<AnotherTestEntity>();
        string expectedSql = $"UPDATE {alias1} SET {alias1}.[Name] = {alias2}.[Description] FROM [dbo].[TestEntities] {alias1} INNER JOIN [dbo].[AnotherTestEntities] {alias2} ON ({alias1}.[Id] = {alias2}.[Id]) LEFT JOIN [dbo].[AnotherTestEntities] {alias2} ON ({alias1}.[Id] = {alias2}.[Id]);\r\n";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that UPDATE with RIGHT JOIN generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_UpdateWithRightJoin_GeneratesCorrectStatement()
    {
        // Arrange
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .UpdateFrom<TestEntity, AnotherTestEntity>(
                (t, a) => (t.Id == a.Id),
                SqlHint.None,
                new Dictionary<string, System.Linq.Expressions.Expression<Func<AnotherTestEntity, object>>>
                {
                    { "Name", a => a.Description! }
                })
            .RightJoin<TestEntity, AnotherTestEntity>((t, a) => (t.Id == a.Id), SqlHint.None);
        string alias1 = GetReferenceAlias<TestEntity>();
        string alias2 = GetReferenceAlias<AnotherTestEntity>();
        string expectedSql = $"UPDATE {alias1} SET {alias1}.[Name] = {alias2}.[Description] FROM [dbo].[TestEntities] {alias1} INNER JOIN [dbo].[AnotherTestEntities] {alias2} ON ({alias1}.[Id] = {alias2}.[Id]) RIGHT JOIN [dbo].[AnotherTestEntities] {alias2} ON ({alias1}.[Id] = {alias2}.[Id]);\r\n";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that UPDATE with FULL JOIN generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_UpdateWithFullJoin_GeneratesCorrectStatement()
    {
        // Arrange
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .UpdateFrom<TestEntity, AnotherTestEntity>(
                (t, a) => (t.Id == a.Id),
                SqlHint.None,
                new Dictionary<string, System.Linq.Expressions.Expression<Func<AnotherTestEntity, object>>>
                {
                    { "Name", a => a.Description! }
                })
            .FullJoin<TestEntity, AnotherTestEntity>((t, a) => (t.Id == a.Id), SqlHint.None);
        string alias1 = GetReferenceAlias<TestEntity>();
        string alias2 = GetReferenceAlias<AnotherTestEntity>();
        string expectedSql = $"UPDATE {alias1} SET {alias1}.[Name] = {alias2}.[Description] FROM [dbo].[TestEntities] {alias1} INNER JOIN [dbo].[AnotherTestEntities] {alias2} ON ({alias1}.[Id] = {alias2}.[Id]) FULL JOIN [dbo].[AnotherTestEntities] {alias2} ON ({alias1}.[Id] = {alias2}.[Id]);\r\n";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that UPDATE with CROSS JOIN generates correct statement.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_UpdateWithCrossJoin_GeneratesCorrectStatement()
    {
        // Arrange
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .UpdateFrom<TestEntity, AnotherTestEntity>(
                (t, a) => (t.Id == a.Id),
                SqlHint.None,
                new Dictionary<string, System.Linq.Expressions.Expression<Func<AnotherTestEntity, object>>>
                {
                    { "Name", a => a.Description! }
                })
            .CrossJoin<TestEntity, AnotherTestEntity>(SqlHint.None);

        string alias1 = GetReferenceAlias<TestEntity>();
        string alias2 = GetReferenceAlias<AnotherTestEntity>();
        string expectedSql = $"UPDATE {alias1} SET {alias1}.[Name] = {alias2}.[Description] FROM [dbo].[TestEntities] {alias1} INNER JOIN [dbo].[AnotherTestEntities] {alias2} ON ({alias1}.[Id] = {alias2}.[Id]) CROSS JOIN [dbo].[AnotherTestEntities] {alias2};\r\n";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that complex UPDATE with TOP, hints, and WHERE generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Build_ComplexUpdateStatement_GeneratesExactSQL()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Updated" };
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .Top(5)
            .With(SqlHint.TabLock)
            .Set(entity)
            .Where<TestEntity>(e => (e.Id > 0));
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"UPDATE TOP (5) [dbo].[TestEntities] {alias} WITH (TABLOCK) SET {alias}.[Id] = 1, {alias}.[Name] = 'Updated' WHERE ({alias}.[Id] > 0);\r\n";

        // Act
        StringBuilder result = builder.Build();
        string actualSql = result.ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that Merge method creates a builder instance.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Into_WithValidType_ReturnsBuilderInstance()
    {
        // Act
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>();

        // Assert
        Assert.IsNotNull(builder);
        Assert.IsInstanceOfType(builder, typeof(SqlUpdateBuilder));
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
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .Top(0)
            .Set(entity)
            .Where<TestEntity>(e => (e.Id == 1));
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"UPDATE TOP (0) [dbo].[TestEntities] {alias} SET {alias}.[Id] = 1, {alias}.[Name] = 'Test' WHERE ({alias}.[Id] = 1);\r\n";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    /// <summary>
    /// Tests that Set method returns self for fluent chaining.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Set_ReturnsBuilderForFluentChaining()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };

        // Act
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>();
        SqlUpdateBuilder result = builder.Set(entity);

        // Assert
        Assert.AreSame(builder, result);
    }

    /// <summary>
    /// Tests that UpdateMany method with single entity creates builder instance.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Update_WithSingleEntity_ReturnsBuilderInstance()
    {
        // Arrange
        TestEntityWithPK entity = new TestEntityWithPK { Id = 1, Name = "Test" };

        // Act
        SqlUpdateBuilder builder = SqlUpdateBuilder.Update(entity);

        // Assert
        Assert.IsNotNull(builder);
        Assert.IsInstanceOfType(builder, typeof(SqlUpdateBuilder));
    }

    /// <summary>
    /// Tests that UpdateMany method with single entity generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Update_WithSingleEntity_GeneratesExactSQL()
    {
        // Arrange
        TestEntityWithPK entity = new TestEntityWithPK { Id = 1, Name = "Test" };
        string alias = GetReferenceAlias<TestEntityWithPK>();
        string expectedSql = $"UPDATE [dbo].[TestEntitiesWithPK] {alias} SET {alias}.[Name] = 'Test' WHERE ({alias}.[Id] = 1);\r\n";

        // Act
        SqlUpdateBuilder builder = SqlUpdateBuilder.Update(entity);
        string actualSql = builder.Build().ToString();

        // Assert - Exact match
        Assert.AreEqual(expectedSql, actualSql);
    }

    /// <summary>
    /// Tests that UpdateMany method with multiple entities creates builder instances.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Update_WithMultipleEntities_ReturnsBuilderInstances()
    {
        // Arrange
        TestEntityWithPK entity1 = new TestEntityWithPK { Id = 1, Name = "Test1" };
        TestEntityWithPK entity2 = new TestEntityWithPK { Id = 2, Name = "Test2" };

        // Act
        IEnumerable<SqlUpdateBuilder> builders = SqlUpdateBuilder.UpdateMany(new TestEntityWithPK[] { entity1, entity2 });
        List<SqlUpdateBuilder> buildersList = new List<SqlUpdateBuilder>(builders);

        // Assert
        Assert.HasCount(2, buildersList);
        Assert.IsInstanceOfType(buildersList[0], typeof(SqlUpdateBuilder));
        Assert.IsInstanceOfType(buildersList[1], typeof(SqlUpdateBuilder));
    }

    /// <summary>
    /// Tests that UpdateMany method with multiple entities generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Update_WithMultipleEntities_GeneratesExactSQL()
    {
        // Arrange
        TestEntityWithPK entity1 = new TestEntityWithPK { Id = 1, Name = "Test1" };
        TestEntityWithPK entity2 = new TestEntityWithPK { Id = 2, Name = "Test2" };
        string alias = GetReferenceAlias<TestEntityWithPK>();
        string expectedSql1 = $"UPDATE [dbo].[TestEntitiesWithPK] {alias} SET {alias}.[Name] = 'Test1' WHERE ({alias}.[Id] = 1);\r\n";
        string expectedSql2 = $"UPDATE [dbo].[TestEntitiesWithPK] {alias} SET {alias}.[Name] = 'Test2' WHERE ({alias}.[Id] = 2);\r\n";

        // Act
        IEnumerable<SqlUpdateBuilder> builders = SqlUpdateBuilder.UpdateMany(new TestEntityWithPK[] { entity1, entity2 });
        List<SqlUpdateBuilder> buildersList = new List<SqlUpdateBuilder>(builders);

        // Assert
        Assert.AreEqual(expectedSql1, buildersList[0].Build().ToString());
        Assert.AreEqual(expectedSql2, buildersList[1].Build().ToString());
    }

    /// <summary>
    /// Tests that UpdateMany method with IEnumerable of entities generates exact expected SQL.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Update_WithEnumerableEntities_GeneratesExactSQL()
    {
        // Arrange
        List<TestEntityWithPK> entities = new List<TestEntityWithPK>
        {
            new TestEntityWithPK { Id = 1, Name = "Test1" },
            new TestEntityWithPK { Id = 2, Name = "Test2" }
        };
        string alias = GetReferenceAlias<TestEntityWithPK>();
        string expectedSql1 = $"UPDATE [dbo].[TestEntitiesWithPK] {alias} SET {alias}.[Name] = 'Test1' WHERE ({alias}.[Id] = 1);\r\n";
        string expectedSql2 = $"UPDATE [dbo].[TestEntitiesWithPK] {alias} SET {alias}.[Name] = 'Test2' WHERE ({alias}.[Id] = 2);\r\n";

        // Act
        IEnumerable<SqlUpdateBuilder> builders = SqlUpdateBuilder.UpdateMany(entities);
        List<SqlUpdateBuilder> buildersList = new List<SqlUpdateBuilder>(builders);

        // Assert
        Assert.HasCount(2, buildersList);
        Assert.AreEqual(expectedSql1, buildersList[0].Build().ToString());
        Assert.AreEqual(expectedSql2, buildersList[1].Build().ToString());
    }

    /// <summary>
    /// Tests that OutputToTable method sets output destination.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void OutputToTable_WithEntityType_ReturnsBuilder()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .Set(entity)
            .OutputUpdated(new string[] { "Id" })
            .Where<TestEntity>(e => (e.Id == 1));

        // Act
        SqlUpdateBuilder result = builder.OutputToTable<AnotherTestEntity>();

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
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .Set(entity)
            .OutputUpdated(new string[] { "Id" })
            .Where<TestEntity>(e => (e.Id == 1));

        // Act
        SqlUpdateBuilder result = builder.OutputToTable("OutputTable");

        // Assert
        Assert.AreSame(builder, result);
    }

    /// <summary>
    /// Tests that Set with multiple entities generates multiple UPDATE statements.
    /// </summary>
    [TestMethod]
    [TestCategory("Functional")]
    public void Set_WithMultipleEntities_GeneratesMultipleStatements()
    {
        // Arrange
        TestEntity entity1 = new TestEntity { Id = 1, Name = "Test1" };
        TestEntity entity2 = new TestEntity { Id = 2, Name = "Test2" };
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .Set(null, entity1, entity2)
            .Where<TestEntity>(e => (e.Id > 0));
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"UPDATE [dbo].[TestEntities] {alias} SET {alias}.[Id] = 1, {alias}.[Name] = 'Test1' WHERE ({alias}.[Id] > 0);\r\nUPDATE [dbo].[TestEntities] {alias} SET {alias}.[Id] = 2, {alias}.[Name] = 'Test2' WHERE ({alias}.[Id] > 0);\r\n";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSql, sql);
    }

    #endregion

    #region Negative Tests

    /// <summary>
    /// Tests that Build throws exception when no values are specified.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void Build_WithNoValues_ThrowsInvalidOperationException()
    {
        // Arrange
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .Where<TestEntity>(e => (e.Id == 1));

        // Act & Assert
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => builder.Build());
        Assert.Contains("No values have been provided", exception.Message);
    }

    /// <summary>
    /// Tests that Top throws exception when called twice.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void Top_CalledTwice_ThrowsInvalidOperationException()
    {
        // Arrange
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
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
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>();

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
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
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
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>();

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
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>();

        // Act & Assert
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => builder.OrWhere<TestEntity>(e => (e.Id == 1)));
        Assert.Contains("WHERE clause has not been initialized", exception.Message);
    }

    /// <summary>
    /// Tests that Set throws exception when entity is null.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void Set_WithNullEntity_ThrowsArgumentNullException()
    {
        // Arrange
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>();

        // Act & Assert
        ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(() => builder.Set<TestEntity>(entity: default!));
        Assert.AreEqual("entity", exception.ParamName);
    }

    /// <summary>
    /// Tests that Set throws exception when entities collection is null.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void Set_WithNullEntitiesCollection_ThrowsArgumentNullException()
    {
        // Arrange
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>();

        // Act & Assert
        ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(() => builder.Set<TestEntity>(additionalValues: null, entities: default!));
        Assert.AreEqual("entities", exception.ParamName);
    }

    /// <summary>
    /// Tests that UpdateMany throws exception when entity is null.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void Update_WithNullEntity_ThrowsArgumentNullException()
    {
        // Act & Assert
        ArgumentNullException exception = Assert.ThrowsExactly<ArgumentNullException>(() => SqlUpdateBuilder.Update<TestEntityWithPK>(entity: default!));
        Assert.AreEqual("entity", exception.ParamName);
    }

    /// <summary>
    /// Tests that UpdateMany throws exception when entity has no primary key.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void Update_WithEntityWithoutPrimaryKey_ThrowsInvalidOperationException()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };

        // Act & Assert
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => SqlUpdateBuilder.Update(entity));
        Assert.Contains("does not have a primary key defined", exception.Message);
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
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .Set(entity)
            .Where<TestEntity>(e => (e.Id == 1));

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
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .Set(entity)
            .Where<TestEntity>(e => (e.Id == 1));

        // Act & Assert
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => builder.OutputToTable("OutputTable"));
        Assert.Contains("Set the output columns", exception.Message);
    }

    /// <summary>
    /// Tests that Merge throws exception with invalid entity type.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void Into_WithTypeWithoutSqlAttributes_ThrowsException()
    {
        // Act & Assert
        Assert.ThrowsExactly<TypeLoadException>(() =>
        {
            SqlUpdateBuilder builder = SqlUpdateBuilder.Into<InvalidEntity>();
        }, "Should throw exception when type does not have SqlTable attribute.");
    }

    /// <summary>
    /// Tests that UpdateFrom throws exception when called with different update mode.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void UpdateFrom_AfterSet_ThrowsInvalidOperationException()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .Set(entity);

        // Act & Assert
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() =>
            builder.UpdateFrom<TestEntity, AnotherTestEntity>(
                (t, a) => (t.Id == a.Id),
                SqlHint.None,
                new Dictionary<string, System.Linq.Expressions.Expression<Func<AnotherTestEntity, object>>>
                {
                    { "Name", a => a.Description! }
                }));
        Assert.Contains("A different update mode has already been chosen", exception.Message);
    }

    /// <summary>
    /// Tests that Set throws exception when called after UpdateFrom.
    /// </summary>
    [TestMethod]
    [TestCategory("Negative")]
    public void Set_AfterUpdateFrom_ThrowsInvalidOperationException()
    {
        // Arrange
        TestEntity entity = new TestEntity { Id = 1, Name = "Test" };
        SqlUpdateBuilder builder = SqlUpdateBuilder.Into<TestEntity>()
            .UpdateFrom<TestEntity, AnotherTestEntity>(
                (t, a) => (t.Id == a.Id),
                SqlHint.None,
                new Dictionary<string, System.Linq.Expressions.Expression<Func<AnotherTestEntity, object>>>
                {
                    { "Name", a => a.Description! }
                });

        // Act & Assert
        InvalidOperationException exception = Assert.ThrowsExactly<InvalidOperationException>(() => builder.Set(entity));
        Assert.Contains("A different update mode has already been chosen", exception.Message);
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
        [SqlTableColumn("Id")]
        public int Id { get; set; }

        [SqlTableColumn("Description")]
        public string? Description { get; set; }
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
        SqlUpdateBuilder tempBuilder = SqlUpdateBuilder.Into<T>();
        System.Reflection.FieldInfo? fieldInfo = typeof(SqlUpdateBuilder).GetField("_primaryTable",
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