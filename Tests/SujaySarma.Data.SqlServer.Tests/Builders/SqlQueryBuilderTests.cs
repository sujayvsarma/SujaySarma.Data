using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.SqlServer.Attributes;
using SujaySarma.Data.SqlServer.Builders;
using SujaySarma.Data.SqlServer.Builders.Constants;

using System;
using System.Text;

namespace SujaySarma.Data.SqlServer.Tests.Builders;

/// <summary>
/// Unit tests for SqlQueryBuilder class.
/// </summary>
[TestClass]
public class SqlQueryBuilderTests
{
    #region Test Entity Classes

    /// <summary>
    /// Sample entity for testing purposes with proper SQL attributes.
    /// </summary>
    [SqlTable("TestEntities", Schema = "dbo")]
    private class TestEntity
    {
        [SqlTableColumn("Id")]
        public int Id { get; set; }

        [SqlTableColumn("Name")]
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Another sample entity for testing purposes with proper SQL attributes.
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

    #region From<TTable>() Tests

    [TestMethod]
    [TestCategory("Functional")]
    public void From_WithValidType_ReturnsInstance()
    {
        // Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();

        // Assert
        Assert.IsNotNull(builder, "Builder should not be null.");
        Assert.IsInstanceOfType(builder, typeof(SqlQueryBuilder), "Builder should be of type SqlQueryBuilder.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void From_WithDifferentValidType_ReturnsInstance()
    {
        // Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<AnotherTestEntity>();

        // Assert
        Assert.IsNotNull(builder, "Builder should not be null.");
        Assert.IsInstanceOfType(builder, typeof(SqlQueryBuilder), "Builder should be of type SqlQueryBuilder.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void From_CalledMultipleTimes_ReturnsDistinctInstances()
    {
        // Act
        SqlQueryBuilder builder1 = SqlQueryBuilder.From<TestEntity>();
        SqlQueryBuilder builder2 = SqlQueryBuilder.From<TestEntity>();

        // Assert
        Assert.IsNotNull(builder1, "First builder should not be null.");
        Assert.IsNotNull(builder2, "Second builder should not be null.");
        Assert.AreNotSame(builder1, builder2, "Each call to From() should return a distinct instance.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void From_ReturnsInstanceThatInheritsFromSqlStatementBuilder()
    {
        // Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();

        // Assert
        Assert.IsNotNull(builder, "Builder should not be null.");
        Assert.IsInstanceOfType(builder, typeof(SqlStatementBuilder), "Builder should inherit from SqlStatementBuilder.");
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void From_WithTypeWithoutSqlAttributes_ThrowsException()
    {
        // Act & Assert
        Assert.ThrowsExactly<TypeLoadException>(() =>
        {
            SqlQueryBuilder builder = SqlQueryBuilder.From<InvalidEntity>();
        }, "Should throw exception when type does not have SqlTable attribute.");
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void From_WithBuiltInValueType_ThrowsException()
    {
        // Act & Assert
        Assert.ThrowsExactly<TypeLoadException>(() =>
        {
            SqlQueryBuilder builder = SqlQueryBuilder.From<int>();
        }, "Should throw exception when type is a built-in value type without SqlTable attribute.");
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void From_WithBuiltInReferenceType_ThrowsException()
    {
        // Act & Assert
        Assert.ThrowsExactly<TypeLoadException>(() =>
        {
            SqlQueryBuilder builder = SqlQueryBuilder.From<string>();
        }, "Should throw exception when type is a built-in reference type without SqlTable attribute.");
    }

    #endregion

    #region Constructor Access Tests

    [TestMethod]
    [TestCategory("Functional")]
    public void Constructor_IsNotPubliclyAccessible()
    {
        // Arrange
        System.Reflection.ConstructorInfo[] constructors = typeof(SqlQueryBuilder).GetConstructors(
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance);

        // Assert
        Assert.IsEmpty(constructors,
            "SqlQueryBuilder should not have any public constructors. Use From<TTarget>() method instead.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Constructor_IsPrivate()
    {
        // Arrange
        System.Reflection.ConstructorInfo[] constructors = typeof(SqlQueryBuilder).GetConstructors(
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        // Assert
        Assert.HasCount(1, constructors, "SqlQueryBuilder should have exactly one private constructor.");
        Assert.IsTrue(constructors[0].IsPrivate, "The constructor should be private.");
    }

    #endregion

    #region Field Validation Tests

    [TestMethod]
    [TestCategory("Functional")]
    public void PrimaryTableField_IsInitializedAfterConstruction()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();
        System.Reflection.FieldInfo? fieldInfo = typeof(SqlQueryBuilder).GetField("_primaryTable",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        // Act
        object? fieldValue = fieldInfo?.GetValue(builder);

        // Assert
        Assert.IsNotNull(fieldInfo, "_primaryTable field should exist.");
        Assert.IsNotNull(fieldValue, "_primaryTable should be initialized after construction.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void PrimaryTableField_IsReadOnly()
    {
        // Arrange
        System.Reflection.FieldInfo? fieldInfo = typeof(SqlQueryBuilder).GetField("_primaryTable",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        // Assert
        Assert.IsNotNull(fieldInfo, "_primaryTable field should exist.");
        Assert.IsTrue((fieldInfo?.IsInitOnly ?? false), "_primaryTable should be readonly.");
    }

    #endregion

    #region Class Structure Tests

    [TestMethod]
    [TestCategory("Functional")]
    public void SqlQueryBuilder_IsSealed()
    {
        // Arrange
        Type type = typeof(SqlQueryBuilder);

        // Assert
        Assert.IsTrue(type.IsSealed, "SqlQueryBuilder should be sealed to prevent inheritance.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void SqlQueryBuilder_IsPartialClass()
    {
        // Arrange
        Type type = typeof(SqlQueryBuilder);

        // Assert
        Assert.IsTrue(type.IsClass, "SqlQueryBuilder should be a class.");
        Assert.IsFalse(type.IsAbstract, "SqlQueryBuilder should not be abstract.");
    }

    #endregion

    #region Build() Tests

    [TestMethod]
    [TestCategory("Functional")]
    public void Build_WithMinimalQuery_ReturnsValidSqlString()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"SELECT * FROM [dbo].[TestEntities] {alias};";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(result, "Build should return a StringBuilder.");
        Assert.AreEqual(expectedSql, sql);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Build_WithSelectAll_ReturnsAsteriskInSql()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"SELECT * FROM [dbo].[TestEntities] {alias};";

        // Act
        string sql = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expectedSql, sql);
    }

    #endregion

    #region Select() Tests

    [TestMethod]
    [TestCategory("Functional")]
    public void Select_WithEntityType_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .Select<TestEntity>();

        // Assert
        Assert.IsNotNull(builder, "Select should return builder instance for fluent API.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Select_WithColumnSelector_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .Select<TestEntity>(e => e.Name);

        // Assert
        Assert.IsNotNull(builder, "Select with column selector should return builder instance.");
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void Select_WithInvalidEntityType_ThrowsException()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();

        // Act & Assert
        Assert.ThrowsExactly<TypeLoadException>(() =>
        {
            builder.Select<InvalidEntity>();
        }, "Should throw ArgumentException for entity without SQL attributes.");
    }

    #endregion

    #region Top() Tests

    [TestMethod]
    [TestCategory("Functional")]
    public void Top_WithValidCount_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .Top(10);

        // Assert
        Assert.IsNotNull(builder, "Top should return builder instance.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Top_WithZeroCount_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .Top(0);

        // Assert
        Assert.IsNotNull(builder, "Top should accept zero as valid count.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Top_WithPercentTrue_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .Top(50, true);

        // Assert
        Assert.IsNotNull(builder, "Top should accept percentage value.");
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void Top_WithPercentGreaterThan100_ThrowsException()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
        {
            builder.Top(101, true);
        }, "Should throw ArgumentOutOfRangeException when percent value exceeds 100.");
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void Top_CalledTwice_ThrowsException()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .Top(10);

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() =>
        {
            builder.Top(20);
        }, "Should throw InvalidOperationException when TOP is specified more than once.");
    }

    #endregion

    #region Distinct() Tests

    [TestMethod]
    [TestCategory("Functional")]
    public void Distinct_WhenCalled_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .Distinct();

        // Assert
        Assert.IsNotNull(builder, "Distinct should return builder instance.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Distinct_CalledMultipleTimes_DoesNotThrowException()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .Distinct()
            .Distinct();

        // Assert
        Assert.IsNotNull(builder, "Distinct should be idempotent.");
    }

    #endregion

    #region Into() Tests

    [TestMethod]
    [TestCategory("Functional")]
    public void Into_WithEntityType_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .Into<AnotherTestEntity>();

        // Assert
        Assert.IsNotNull(builder, "Merge with entity type should return builder instance.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Into_WithTableName_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .Into("TempTable");

        // Assert
        Assert.IsNotNull(builder, "Merge with table name should return builder instance.");
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void Into_WithNullTableName_ThrowsException()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.Into(null!);
        }, "Should throw ArgumentNullException when table name is NULL.");
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void Into_WithEmptyTableName_ThrowsException()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            builder.Into(string.Empty);
        }, "Should throw ArgumentNullException when table name is empty.");
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void Into_CalledTwice_ThrowsException()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .Into("TempTable");

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() =>
        {
            builder.Into("AnotherTable");
        }, "Should throw InvalidOperationException when INTO is specified more than once.");
    }

    #endregion

    #region Where() Tests

    [TestMethod]
    [TestCategory("Functional")]
    public void Where_WithSingleTableCondition_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .Where<TestEntity>(e => (e.Id == 1));

        // Assert
        Assert.IsNotNull(builder, "Where should return builder instance.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Where_WithTwoTableCondition_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .Where<TestEntity, AnotherTestEntity>((e1, e2) => (e1.Id == 1));

        // Assert
        Assert.IsNotNull(builder, "Where with two tables should return builder instance.");
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void Where_CalledTwice_ThrowsException()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .Where<TestEntity>(e => (e.Id == 1));

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() =>
        {
            builder.Where<TestEntity>(e => (e.Id == 2));
        }, "Should throw InvalidOperationException when Where is called more than once.");
    }

    #endregion

    #region AndWhere() Tests

    [TestMethod]
    [TestCategory("Functional")]
    public void AndWhere_AfterWhere_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .Where<TestEntity>(e => (e.Id == 1))
            .AndWhere<TestEntity>(e => (e.Name == "Test"));

        // Assert
        Assert.IsNotNull(builder, "AndWhere should return builder instance.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void AndWhere_WithTwoTables_AfterWhere_AddingDuplicateConditionThrowsException()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .Where<TestEntity>(e => (e.Id == 1));

        // Assert
        Assert.ThrowsExactly<ArgumentException>(() => builder.AndWhere<TestEntity, AnotherTestEntity>((e1, e2) => (e1.Id == 1)));
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void AndWhere_WithoutWhere_ThrowsException()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() =>
        {
            builder.AndWhere<TestEntity>(e => (e.Id == 1));
        }, "Should throw InvalidOperationException when AndWhere is called before Where.");
    }

    #endregion

    #region OrWhere() Tests

    [TestMethod]
    [TestCategory("Functional")]
    public void OrWhere_AfterWhere_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .Where<TestEntity>(e => (e.Id == 1))
            .OrWhere<TestEntity>(e => (e.Id == 2));

        // Assert
        Assert.IsNotNull(builder, "OrWhere should return builder instance.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void OrWhere_WithTwoTables_AfterWhere_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .Where<TestEntity>(e => (e.Id == 1))
            .OrWhere<TestEntity, AnotherTestEntity>((e1, e2) => (e1.Id == 2));

        // Assert
        Assert.IsNotNull(builder, "OrWhere with two tables should return builder instance.");
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void OrWhere_WithoutWhere_ThrowsException()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() =>
        {
            builder.OrWhere<TestEntity>(e => (e.Id == 1));
        }, "Should throw InvalidOperationException when OrWhere is called before Where.");
    }

    #endregion

    #region OrderByASC() Tests

    [TestMethod]
    [TestCategory("Functional")]
    public void OrderByASC_WithSingleColumn_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .OrderByASC<TestEntity>(e => e.Name);

        // Assert
        Assert.IsNotNull(builder, "OrderByASC should return builder instance.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void OrderByASC_WithMultipleColumns_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .OrderByASC<TestEntity>(e => new { e.Name, e.Id });

        // Assert
        Assert.IsNotNull(builder, "OrderByASC with multiple columns should return builder instance.");
    }

    #endregion

    #region OrderByDESC() Tests

    [TestMethod]
    [TestCategory("Functional")]
    public void OrderByDESC_WithSingleColumn_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .OrderByDESC<TestEntity>(e => e.Id);

        // Assert
        Assert.IsNotNull(builder, "OrderByDESC should return builder instance.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void OrderByDESC_WithMultipleColumns_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .OrderByDESC<TestEntity>(e => new { e.Id, e.Name });

        // Assert
        Assert.IsNotNull(builder, "OrderByDESC with multiple columns should return builder instance.");
    }

    #endregion

    #region GroupBy Tests

    [TestMethod]
    [TestCategory("Functional")]
    public void GroupByRollup_WithValidSelector_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .GroupByRollup<TestEntity>(null!);

        // Assert
        Assert.IsNotNull(builder, "GroupByRollup should return builder instance.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void GroupByCube_WithValidSelector_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .GroupByCube<TestEntity>(null!);

        // Assert
        Assert.IsNotNull(builder, "GroupByCube should return builder instance.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void GroupByGroupingSets_WithValidSelector_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .GroupByGroupingSets<TestEntity>(null!);

        // Assert
        Assert.IsNotNull(builder, "GroupByGroupingSets should return builder instance.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void GroupByEmpty_WithValidType_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .GroupByEmpty<TestEntity>();

        // Assert
        Assert.IsNotNull(builder, "GroupByEmpty should return builder instance.");
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void GroupBy_CalledTwice_ThrowsException()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .GroupByEmpty<TestEntity>();

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() =>
        {
            builder.GroupByEmpty<TestEntity>();
        }, "Should throw InvalidOperationException when GROUP BY is specified more than once.");
    }

    #endregion

    #region Join Tests

    [TestMethod]
    [TestCategory("Functional")]
    public void InnerJoin_WithValidCondition_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .InnerJoin<TestEntity, AnotherTestEntity>((e1, e2) => (e1.Id == 1), SqlHint.None);

        // Assert
        Assert.IsNotNull(builder, "InnerJoin should return builder instance.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void LeftJoin_WithValidCondition_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .LeftJoin<TestEntity, AnotherTestEntity>((e1, e2) => (e1.Id == 1), SqlHint.None);

        // Assert
        Assert.IsNotNull(builder, "LeftJoin should return builder instance.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void RightJoin_WithValidCondition_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .RightJoin<TestEntity, AnotherTestEntity>((e1, e2) => (e1.Id == 1), SqlHint.None);

        // Assert
        Assert.IsNotNull(builder, "RightJoin should return builder instance.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void FullJoin_WithValidCondition_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .FullJoin<TestEntity, AnotherTestEntity>((e1, e2) => (e1.Id == 1), SqlHint.None);

        // Assert
        Assert.IsNotNull(builder, "FullJoin should return builder instance.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void CrossJoin_WithValidTables_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .CrossJoin<TestEntity, AnotherTestEntity>(SqlHint.None);

        // Assert
        Assert.IsNotNull(builder, "CrossJoin should return builder instance.");
    }

    #endregion

    #region With() Tests

    [TestMethod]
    [TestCategory("Functional")]
    public void With_WithValidHint_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .With(SqlHint.NoLock);

        // Assert
        Assert.IsNotNull(builder, "With should return builder instance.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void With_CalledMultipleTimes_ReturnsInstance()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .With(SqlHint.NoLock)
            .With(SqlHint.ReadPast);

        // Assert
        Assert.IsNotNull(builder, "With should allow multiple hints to be added.");
    }

    #endregion

    #region Fluent API Tests

    [TestMethod]
    [TestCategory("Functional")]
    public void FluentApi_ChainedCalls_BuildsComplexQuery()
    {
        // Arrange & Act
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>()
            .Select<TestEntity>(e => e.Name)
            .Top(10)
            .Distinct()
            .Where<TestEntity>(e => (e.Id > 0))
            .AndWhere<TestEntity>(e => (e.Name != ""))
            .OrderByASC<TestEntity>(e => e.Name);
        string alias = GetReferenceAlias<TestEntity>();
        string expectedSql = $"SELECT TOP (10) DISTINCT {alias}.[Name] FROM [dbo].[TestEntities] {alias} WHERE ({alias}.[Id] > 0) AND ({alias}.[Name] <> '') ORDER BY {alias}.[Name] ASC;";

        // Act
        StringBuilder result = builder.Build();
        string sql = result.ToString();

        // Assert
        Assert.IsNotNull(builder, "Fluent API should allow method chaining.");
        Assert.IsNotNull(result, "Build should return result after fluent chain.");
        Assert.AreEqual(expectedSql, sql);
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

    #region Build() - Exact SQL String Validation Tests

    [TestMethod]
    [TestCategory("Functional")]
    public void Build_MinimalQuery_GeneratesExactSql()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();
        string alias = GetReferenceAlias<TestEntity>();
        string expected = $"SELECT * FROM [dbo].[TestEntities] {alias};";

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual, "Minimal query SQL should match exactly.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Build_WithTop_GeneratesExactSql()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();
        string alias = GetReferenceAlias<TestEntity>();
        string expected = $"SELECT TOP (10) * FROM [dbo].[TestEntities] {alias};";

        builder.Top(10);

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual, "Query with TOP should generate exact SQL.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Build_WithTopZero_GeneratesExactSql()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();
        string alias = GetReferenceAlias<TestEntity>();
        string expected = $"SELECT TOP (0) * FROM [dbo].[TestEntities] {alias};";

        builder.Top(0);

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual, "Query with TOP (0) should generate exact SQL.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Build_WithTopPercent_GeneratesExactSql()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();
        string alias = GetReferenceAlias<TestEntity>();
        string expected = $"SELECT TOP (50) PERCENT * FROM [dbo].[TestEntities] {alias};";

        builder.Top(50, true);

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual, "Query with TOP PERCENT should generate exact SQL.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Build_WithDistinct_GeneratesExactSql()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();
        string alias = GetReferenceAlias<TestEntity>();
        string expected = $"SELECT DISTINCT * FROM [dbo].[TestEntities] {alias};";

        builder.Distinct();

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual, "Query with DISTINCT should generate exact SQL.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Build_WithTopAndDistinct_GeneratesExactSql()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();
        string alias = GetReferenceAlias<TestEntity>();
        string expected = $"SELECT TOP (25) DISTINCT * FROM [dbo].[TestEntities] {alias};";

        builder.Top(25).Distinct();

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual, "Query with TOP and DISTINCT should generate exact SQL with correct keyword order.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Build_WithDistinctAndTop_GeneratesExactSql()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();
        string alias = GetReferenceAlias<TestEntity>();
        string expected = $"SELECT TOP (15) DISTINCT * FROM [dbo].[TestEntities] {alias};";

        builder.Distinct().Top(15);

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual, "Query with DISTINCT then TOP should generate exact SQL in correct order.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Build_WithIntoTableName_GeneratesExactSql()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();
        string alias = GetReferenceAlias<TestEntity>();
        string expected = $"SELECT * INTO [TempTable] FROM [dbo].[TestEntities] {alias};";

        builder.Into("TempTable");

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual, "Query with INTO table name should generate exact SQL.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Build_WithIntoEntityType_GeneratesExactSql()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();
        string alias = GetReferenceAlias<TestEntity>();
        string expected = $"SELECT * INTO [dbo].[AnotherTestEntities] FROM [dbo].[TestEntities] {alias};";

        builder.Into<AnotherTestEntity>();

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual, "Query with INTO entity type should generate exact SQL.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Build_WithSelectAllColumns_GeneratesExactSql()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();
        string alias = GetReferenceAlias<TestEntity>();
        string expected = $"SELECT {alias}.[Id], {alias}.[Name] FROM [dbo].[TestEntities] {alias};";

        builder.Select<TestEntity>();

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual, "Query selecting all columns from entity should generate exact SQL.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Build_WithSelectSpecificColumn_GeneratesExactSql()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();
        string alias = GetReferenceAlias<TestEntity>();
        string expected = $"SELECT {alias}.[Name] FROM [dbo].[TestEntities] {alias};";

        builder.Select<TestEntity>(e => e.Name);

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual, "Query selecting single column should generate exact SQL.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Build_WithSelectMultipleColumns_GeneratesExactSql()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();
        string alias = GetReferenceAlias<TestEntity>();
        string expected = $"SELECT {alias}.[Id], {alias}.[Name] FROM [dbo].[TestEntities] {alias};";

        builder.Select<TestEntity>(e => new { e.Id, e.Name });

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual, "Query selecting multiple columns should generate exact SQL.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Build_WithOrderByAsc_GeneratesExactSql()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();
        string alias = GetReferenceAlias<TestEntity>();
        string expected = $"SELECT * FROM [dbo].[TestEntities] {alias} ORDER BY {alias}.[Name] ASC;";

        builder.OrderByASC<TestEntity>(e => e.Name);

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual, "Query with ORDER BY ASC should generate exact SQL.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Build_WithOrderByDesc_GeneratesExactSql()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();
        string alias = GetReferenceAlias<TestEntity>();
        string expected = $"SELECT * FROM [dbo].[TestEntities] {alias} ORDER BY {alias}.[Id] DESC;";

        builder.OrderByDESC<TestEntity>(e => e.Id);

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual, "Query with ORDER BY DESC should generate exact SQL.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Build_WithOrderByMultipleColumns_GeneratesExactSql()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();
        string alias = GetReferenceAlias<TestEntity>();
        string expected = $"SELECT * FROM [dbo].[TestEntities] {alias} ORDER BY {alias}.[Name] ASC, {alias}.[Id] DESC;";

        builder.OrderByASC<TestEntity>(e => e.Name)
               .OrderByDESC<TestEntity>(e => e.Id);

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual, "Query with multiple ORDER BY clauses should generate exact SQL.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Build_WithGroupByEmpty_GeneratesExactSql()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();
        string alias = GetReferenceAlias<TestEntity>();
        string expected = $"SELECT * FROM [dbo].[TestEntities] {alias} GROUP BY ();";

        builder.GroupByEmpty<TestEntity>();

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual, "Query with GROUP BY () should generate exact SQL.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Build_WithTableHints_GeneratesExactSql()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();
        string alias = GetReferenceAlias<TestEntity>();
        string expected = $"SELECT * FROM [dbo].[TestEntities] {alias} WITH (NOLOCK);";

        builder.With(SqlHint.NoLock);

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual, "Query with table hints should generate exact SQL.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Build_WithMultipleTableHints_GeneratesExactSql()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();
        string alias = GetReferenceAlias<TestEntity>();
        string expected = $"SELECT * FROM [dbo].[TestEntities] {alias} WITH (NOLOCK, READPAST);";

        builder.With(SqlHint.NoLock)
               .With(SqlHint.ReadPast);

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual, "Query with multiple table hints should generate exact SQL.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Build_CompleteQueryAllClauses_GeneratesExactSql()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();
        string alias = GetReferenceAlias<TestEntity>();
        string expected = $"SELECT TOP (100) DISTINCT {alias}.[Id], {alias}.[Name] FROM [dbo].[TestEntities] {alias} WHERE ({alias}.[Id] > 0) AND ({alias}.[Name] <> '') ORDER BY {alias}.[Name] ASC;";

        builder.Select<TestEntity>(e => new { e.Id, e.Name })
               .Top(100)
               .Distinct()
               .Where<TestEntity>(e => (e.Id > 0))
               .AndWhere<TestEntity>(e => (e.Name != ""))
               .OrderByASC<TestEntity>(e => e.Name);

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual, "Complete query with all clauses should generate exact SQL with correct clause ordering.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Build_WithTopPercentDistinctInto_GeneratesExactSql()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<TestEntity>();
        string alias = GetReferenceAlias<TestEntity>();
        string expected = $"SELECT TOP (75) PERCENT DISTINCT * INTO [ResultTable] FROM [dbo].[TestEntities] {alias};";

        builder.Top(75, true)
               .Distinct()
               .Into("ResultTable");

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual, "Query with TOP PERCENT, DISTINCT, and INTO should generate exact SQL.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Build_SecondEntityType_GeneratesExactSqlWithCorrectAlias()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<AnotherTestEntity>();
        string alias = GetReferenceAlias<AnotherTestEntity>();
        string expected = $"SELECT * FROM [dbo].[AnotherTestEntities] {alias};";

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual, "Query for different entity type should generate exact SQL with correct table name and alias.");
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Build_WithAllColumnsFromSecondEntity_GeneratesExactSql()
    {
        // Arrange
        SqlQueryBuilder builder = SqlQueryBuilder.From<AnotherTestEntity>();
        string alias = GetReferenceAlias<AnotherTestEntity>();
        string expected = $"SELECT {alias}.[Guid], {alias}.[CreatedDate] FROM [dbo].[AnotherTestEntities] {alias};";

        builder.Select<AnotherTestEntity>();

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual, "Query selecting all columns from second entity should generate exact SQL.");
    }

    #endregion
}