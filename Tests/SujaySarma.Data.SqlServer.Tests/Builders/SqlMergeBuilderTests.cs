using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.SqlServer.Attributes;
using SujaySarma.Data.SqlServer.Builders;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace SujaySarma.Data.SqlServer.Tests.Builders;

/// <summary>
/// Unit tests for the <see cref="SqlMergeBuilder{TTarget}"/> class.
/// </summary>
[TestClass]
public class SqlMergeBuilderTests
{

    #region Functional Tests

    [TestMethod]
    [TestCategory("Functional")]
    public void BasicMerge_WithTableSource()
    {
        // Arrange
        DateTime utcNow = DateTime.UtcNow;
        SqlMergeBuilder<Product> builder = SqlMergeBuilder<Product>.Create()
            .UsingTable<ProductUpdate>((t, s) => t.Id == s.Id)
                .BeginMatches()
                    .WhenMatched()
                        .Update()
                            .Set(
                                new Dictionary<string, Expression<Func<ProductUpdate, object>>>() {
                                    { "Name", src => src.Name },
                                    { "Price", src => src.Price },
                                    { "LastModified", src => utcNow }   // DateTime.UtcNow works as well, but we need a specific d/t to compare for the test!
                                })
                    .EndMatches();

        string t1Alias = GetReferenceAlias<Product>();
        string t2Alias = GetReferenceAlias<ProductUpdate>();
        string expected = $"MERGE [dbo].[Products] WITH (HOLDLOCK) AS {t1Alias} \r\nUSING [dbo].[ProductUpdates] AS {t2Alias}\r\n ON ({t1Alias}.[Id] = {t2Alias}.[Id]) {Environment.NewLine}WHEN MATCHED THEN UPDATE SET {t1Alias}.[Name] = {t2Alias}.[Name], {t1Alias}.[Price] = {t2Alias}.[Price], {t1Alias}.[LastModified] = '{utcNow:yyyy-MM-dd HH:mm:ss.fff}';";


        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual);
    }


    [TestMethod]
    [TestCategory("Functional")]
    public void Merge_WithConditionalUpdate()
    {
        // Arrange
        DateTime utcNow = DateTime.UtcNow;
        SqlMergeBuilder<Product> builder = SqlMergeBuilder<Product>.Create()
            .UsingTable<ProductUpdate>((t, s) => t.Id == s.Id)
                .BeginMatches()
                    .WhenMatched((t, s) => t.LastModified < s.LastModified)
                        .Update()
                            .Set(
                                new Dictionary<string, Expression<Func<ProductUpdate, object>>>() {
                                    { "Name", src => src.Name },
                                    { "Price", src => src.Price },
                                    { "LastModified", src => utcNow }
                                })
                .EndMatches();

        string t1Alias = GetReferenceAlias<Product>();
        string t2Alias = GetReferenceAlias<ProductUpdate>();
        string expected = $"MERGE [dbo].[Products] WITH (HOLDLOCK) AS {t1Alias} \r\nUSING [dbo].[ProductUpdates] AS {t2Alias}\r\n ON ({t1Alias}.[Id] = {t2Alias}.[Id]) {Environment.NewLine}WHEN MATCHED AND ({t1Alias}.[LastModified] < {t2Alias}.[LastModified]) THEN UPDATE SET {t1Alias}.[Name] = {t2Alias}.[Name], {t1Alias}.[Price] = {t2Alias}.[Price], {t1Alias}.[LastModified] = '{utcNow:yyyy-MM-dd HH:mm:ss.fff}';";

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual);
    }


    [TestMethod]
    [TestCategory("Functional")]
    public void Merge_WithDelete()
    {
        // Arrange
        SqlMergeBuilder<Product> builder = SqlMergeBuilder<Product>.Create()
            .UsingTable<ProductUpdate>((t, s) => t.Id == s.Id)
                .BeginMatches()
                    .WhenMatched((t, s) => !s.IsActive)
                        .Delete()
                .EndMatches();

        string t1Alias = GetReferenceAlias<Product>();
        string t2Alias = GetReferenceAlias<ProductUpdate>();
        string expected = $"MERGE [dbo].[Products] WITH (HOLDLOCK) AS {t1Alias} \r\nUSING [dbo].[ProductUpdates] AS {t2Alias}\r\n ON ({t1Alias}.[Id] = {t2Alias}.[Id]) {Environment.NewLine}WHEN MATCHED AND NOT {t2Alias}.[IsActive] THEN DELETE;";

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Merge_WithInsertForUnmatched()
    {
        // Arrange
        DateTime utcNow = DateTime.UtcNow;
        SqlMergeBuilder<Product> builder = SqlMergeBuilder<Product>.Create()
            .UsingTable<ProductUpdate>((t, s) => t.Id == s.Id)
                .BeginMatches()
                    .WhenNotMatchedByTarget()
                        .Insert()
                            .Set(
                                    new Dictionary<string, Expression<Func<ProductUpdate, object>>>()
                                    {
                                        { "Id", src => src.Id },
                                        { "Name", src => src.Name },
                                        { "Price", src => src.Price },
                                        { "IsActive", src => src.IsActive },
                                        { "CreatedDate", src => utcNow },
                                        { "LastModified", src => utcNow }
                                    }
                                )
                .EndMatches();

        string t1Alias = GetReferenceAlias<Product>();
        string t2Alias = GetReferenceAlias<ProductUpdate>();
        string expected = $"MERGE [dbo].[Products] WITH (HOLDLOCK) AS {t1Alias} \r\nUSING [dbo].[ProductUpdates] AS {t2Alias}\r\n ON ({t1Alias}.[Id] = {t2Alias}.[Id]) {Environment.NewLine}WHEN NOT MATCHED BY TARGET THEN INSERT ({t1Alias}.[Id], {t1Alias}.[Name], {t1Alias}.[Price], {t1Alias}.[IsActive], {t1Alias}.[CreatedDate], {t1Alias}.[LastModified]) VALUES ({t2Alias}.[Id], {t2Alias}.[Name], {t2Alias}.[Price], {t2Alias}.[IsActive], '{utcNow:yyyy-MM-dd HH:mm:ss.fff}', '{utcNow:yyyy-MM-dd HH:mm:ss.fff}');";

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual);
    }


    [TestMethod]
    [TestCategory("Functional")]
    public void Merge_WithConditionalInsert()
    {
        // Arrange
        DateTime utcNow = DateTime.UtcNow;
        SqlMergeBuilder<Product> builder = SqlMergeBuilder<Product>.Create()
            .UsingTable<ProductUpdate>((t, s) => t.Id == s.Id)
                .BeginMatches()
                    .WhenNotMatchedByTarget((_, s) => s.IsActive)
                        .Insert()
                            .Set(
                                    new Dictionary<string, Expression<Func<ProductUpdate, object>>>()
                                    {
                                        { "Id", src => src.Id },
                                        { "Name", src => src.Name },
                                        { "Price", src => src.Price },
                                        { "IsActive", src => src.IsActive },
                                        { "CreatedDate", src => utcNow },
                                        { "LastModified", src => utcNow }
                                    }
                                )
                .EndMatches();

        string t1Alias = GetReferenceAlias<Product>();
        string t2Alias = GetReferenceAlias<ProductUpdate>();
        string expected = $"MERGE [dbo].[Products] WITH (HOLDLOCK) AS {t1Alias} \r\nUSING [dbo].[ProductUpdates] AS {t2Alias}\r\n ON ({t1Alias}.[Id] = {t2Alias}.[Id]) {Environment.NewLine}WHEN NOT MATCHED BY TARGET AND {t2Alias}.[IsActive] THEN INSERT ({t1Alias}.[Id], {t1Alias}.[Name], {t1Alias}.[Price], {t1Alias}.[IsActive], {t1Alias}.[CreatedDate], {t1Alias}.[LastModified]) VALUES ({t2Alias}.[Id], {t2Alias}.[Name], {t2Alias}.[Price], {t2Alias}.[IsActive], '{utcNow:yyyy-MM-dd HH:mm:ss.fff}', '{utcNow:yyyy-MM-dd HH:mm:ss.fff}');";

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual);
    }


    [TestMethod]
    [TestCategory("Functional")]
    public void Merge_WithSourceBasedDelete()
    {
        // Arrange
        SqlMergeBuilder<Product> builder = SqlMergeBuilder<Product>.Create()
            .UsingTable<ProductUpdate>((t, s) => t.Id == s.Id)
                .BeginMatches()
                    .WhenNotMatchedBySource()
                        .Delete()
                .EndMatches();
        string t1Alias = GetReferenceAlias<Product>();
        string t2Alias = GetReferenceAlias<ProductUpdate>();
        string expected = $"MERGE [dbo].[Products] WITH (HOLDLOCK) AS {t1Alias} \r\nUSING [dbo].[ProductUpdates] AS {t2Alias}\r\n ON ({t1Alias}.[Id] = {t2Alias}.[Id]) {Environment.NewLine}WHEN NOT MATCHED BY SOURCE THEN DELETE;";

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual);
    }


    [TestMethod]
    [TestCategory("Functional")]
    public void Merge_WithSourceBasedUpdate()
    {
        // Arrange
        DateTime utcNow = DateTime.UtcNow;
        SqlMergeBuilder<Product> builder = SqlMergeBuilder<Product>.Create()
            .UsingTable<ProductUpdate>((t, s) => t.Id == s.Id)
                .BeginMatches()
                    .WhenNotMatchedBySource((t, _) => !t.IsArchived)
                        .Update()
                            .Set(
                                new Dictionary<string, Expression<Func<ProductUpdate, object>>>() {
                                    { "IsArchived", src => true },
                                    { "LastModified", src => utcNow }
                                })
                .EndMatches();
        string t1Alias = GetReferenceAlias<Product>();
        string t2Alias = GetReferenceAlias<ProductUpdate>();
        string expected = $"MERGE [dbo].[Products] WITH (HOLDLOCK) AS {t1Alias} \r\nUSING [dbo].[ProductUpdates] AS {t2Alias}\r\n ON ({t1Alias}.[Id] = {t2Alias}.[Id]) {Environment.NewLine}WHEN NOT MATCHED BY SOURCE AND NOT {t1Alias}.[IsArchived] THEN UPDATE SET {t1Alias}.[IsArchived] = 1, {t1Alias}.[LastModified] = '{utcNow:yyyy-MM-dd HH:mm:ss.fff}';";

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Merge_WithAllClauses()
    {
        DateTime utcNow = DateTime.UtcNow;
        SqlMergeBuilder<Product> builder = SqlMergeBuilder<Product>.Create()
            .UsingTable<ProductUpdate>((t, s) => t.Id == s.Id)
                .BeginMatches()
                    .WhenMatched((t, s) => t.LastModified < s.LastModified)
                        .Update()
                            .Set(
                                new Dictionary<string, Expression<Func<ProductUpdate, object>>>() {
                                    { "Name", src => src.Name },
                                    { "Price", src => src.Price },
                                    { "LastModified", src => utcNow }
                                })
                    .WhenMatched((t, s) => !s.IsActive)
                        .Delete()
                    .WhenNotMatchedByTarget((_, s) => s.IsActive)
                        .Insert()
                            .Set(
                                    new Dictionary<string, Expression<Func<ProductUpdate, object>>>()
                                    {
                                        { "Id", src => src.Id },
                                        { "Name", src => src.Name },
                                        { "Price", src => src.Price },
                                        { "IsActive", src => src.IsActive },
                                        { "CreatedDate", src => utcNow },
                                        { "LastModified", src => utcNow }
                                    }
                                )
                    .WhenNotMatchedBySource((t, _) => !t.IsArchived)
                        .Update()
                            .Set(
                                new Dictionary<string, Expression<Func<ProductUpdate, object>>>() {
                                    { "IsArchived", src => true },
                                    { "LastModified", src => utcNow }
                                })
                .EndMatches();

        string t1Alias = GetReferenceAlias<Product>();
        string t2Alias = GetReferenceAlias<ProductUpdate>();

        string expected = $"MERGE [dbo].[Products] WITH (HOLDLOCK) AS {t1Alias} \r\nUSING [dbo].[ProductUpdates] AS {t2Alias}\r\n ON ({t1Alias}.[Id] = {t2Alias}.[Id]) " +
            $"{Environment.NewLine}WHEN MATCHED AND ({t1Alias}.[LastModified] < {t2Alias}.[LastModified]) THEN UPDATE SET {t1Alias}.[Name] = {t2Alias}.[Name], {t1Alias}.[Price] = {t2Alias}.[Price], {t1Alias}.[LastModified] = '{utcNow:yyyy-MM-dd HH:mm:ss.fff}' " +
            $"{Environment.NewLine}WHEN MATCHED AND NOT {t2Alias}.[IsActive] THEN DELETE " +
            $"{Environment.NewLine}WHEN NOT MATCHED BY TARGET AND {t2Alias}.[IsActive] THEN INSERT ({t1Alias}.[Id], {t1Alias}.[Name], {t1Alias}.[Price], {t1Alias}.[IsActive], {t1Alias}.[CreatedDate], {t1Alias}.[LastModified]) VALUES ({t2Alias}.[Id], {t2Alias}.[Name], {t2Alias}.[Price], {t2Alias}.[IsActive], '{utcNow:yyyy-MM-dd HH:mm:ss.fff}', '{utcNow:yyyy-MM-dd HH:mm:ss.fff}') " +
            $"{Environment.NewLine}WHEN NOT MATCHED BY SOURCE AND NOT {t1Alias}.[IsArchived] THEN UPDATE SET {t1Alias}.[IsArchived] = 1, {t1Alias}.[LastModified] = '{utcNow:yyyy-MM-dd HH:mm:ss.fff}';";

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual);
    }


    [TestMethod]
    [TestCategory("Functional")]
    public void Merge_WithQuerySource()
    {
        // Arrange
        SqlQueryBuilder sourceQuery = SqlQueryBuilder.From<ProductUpdate>()
            .Where<ProductUpdate>(pu => pu.IsActive);

        SqlMergeBuilder<Product> builder = SqlMergeBuilder<Product>.Create()
            .UsingQuery<ProductUpdate>(
                sourceQuery,
                (t, s) => t.Id == s.Id)
                .BeginMatches()
                    .WhenMatched()
                        .Update()
                            .Set(
                                new Dictionary<string, Expression<Func<ProductUpdate, object>>>() {
                                    { "Name", src => src.Name },
                                    { "Price", src => src.Price }
                                })
                .EndMatches();

        string t1Alias = GetReferenceAlias<Product>();
        string t2Alias = GetReferenceAlias<ProductUpdate>();

        string expected = $"MERGE [dbo].[Products] WITH (HOLDLOCK) AS {t1Alias} \r\nUSING (SELECT * FROM [dbo].[ProductUpdates] WHERE [IsActive]) AS {t2Alias}\r\n ON ({t1Alias}.[Id] = {t2Alias}.[Id]) {Environment.NewLine}WHEN MATCHED THEN UPDATE SET {t1Alias}.[Name] = {t2Alias}.[Name], {t1Alias}.[Price] = {t2Alias}.[Price];";

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual);
    }


    [TestMethod]
    [TestCategory("Functional")]
    public void Merge_WithSelfJoin()
    {
        // Arrange
        SqlMergeBuilder<Product> builder = SqlMergeBuilder<Product>.Create()
            .UsingTable<Product>((parent, child) => parent.Id == child.ParentId)
                .BeginMatches()
                    .WhenMatched()
                        .Update()
                            .Set(
                                new Dictionary<string, Expression<Func<Product, object>>>() {
                                    { "Level", src => src.Level + 1 }
                                })
                .EndMatches();

        string t1Alias = GetReferenceAlias<Product>();
        string t2Alias = GetReferenceAlias<Product>();

        string expected = $"MERGE [dbo].[Products] WITH (HOLDLOCK) AS {t1Alias} \r\nUSING [dbo].[Products] AS {t2Alias}\r\n ON ({t1Alias}.[Id] = {t2Alias}.[ParentId]) {Environment.NewLine}WHEN MATCHED THEN UPDATE SET {t1Alias}.[Level] = ({t2Alias}.[Level] + 1);";

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Merge_WithOutputClause()
    {
        // Arrange
        SqlMergeBuilder<Product> builder = SqlMergeBuilder<Product>.Create()
            .UsingTable<ProductUpdate>((t, s) => t.Id == s.Id)
                .BeginMatches()
                    .WhenMatched()
                        .Update()
                            .Set(
                                new Dictionary<string, Expression<Func<ProductUpdate, object>>>() {
                                    { "Name", src => src.Name },
                                    { "Price", src => src.Price }
                                })
                .EndMatches()
            .WithOutput()
                .AddActionColumn("action")
                .AddTable<Product>()
                .AddInserted()
                .AddDeleted()
            .EndOutput();

        string t1Alias = GetReferenceAlias<Product>();
        string t2Alias = GetReferenceAlias<ProductUpdate>();

        string expected = $"MERGE [dbo].[Products] WITH (HOLDLOCK) AS {t1Alias} " +
            $"\r\nUSING [dbo].[ProductUpdates] AS {t2Alias}" +
            $"\r\n ON ({t1Alias}.[Id] = {t2Alias}.[Id]) " +
            $"{Environment.NewLine}WHEN MATCHED THEN UPDATE SET {t1Alias}.[Name] = {t2Alias}.[Name], {t1Alias}.[Price] = {t2Alias}.[Price] " +
            $"{Environment.NewLine}OUTPUT $action AS [action], [Id], [Name], [Price], [IsActive], [IsArchived], [ParentId], [Level], [CreatedDate], [LastModified], INSERTED.*, DELETED.*;";

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [TestCategory("Functional")]
    public void Merge_WithTopClause()
    {
        // Arrange
        DateTime utcNow = DateTime.UtcNow;
        uint top = 100;

        SqlMergeBuilder<Product> builder = SqlMergeBuilder<Product>.Create(top: top)
            .UsingTable<ProductUpdate>((t, s) => t.Id == s.Id)
                .BeginMatches()
                    .WhenMatched()
                        .Update()
                            .Set(
                                new Dictionary<string, Expression<Func<ProductUpdate, object>>>() {
                                    { "Name", src => src.Name },
                                    { "Price", src => src.Price },
                                    { "LastModified", src => utcNow }   // DateTime.UtcNow works as well, but we need a specific d/t to compare for the test!
                                })
                    .EndMatches();

        string t1Alias = GetReferenceAlias<Product>();
        string t2Alias = GetReferenceAlias<ProductUpdate>();
        string expected = $"MERGE TOP ({top}) [dbo].[Products] WITH (HOLDLOCK) AS {t1Alias} \r\nUSING [dbo].[ProductUpdates] AS {t2Alias}\r\n ON ({t1Alias}.[Id] = {t2Alias}.[Id]) {Environment.NewLine}WHEN MATCHED THEN UPDATE SET {t1Alias}.[Name] = {t2Alias}.[Name], {t1Alias}.[Price] = {t2Alias}.[Price], {t1Alias}.[LastModified] = '{utcNow:yyyy-MM-dd HH:mm:ss.fff}';";


        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    #endregion

    #region Negative Tests

    [TestMethod]
    [TestCategory("Negative")]
    public void Create_WithInvalidTopPercent_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            SqlMergeBuilder<Product>.Create(top: 101, topIsPercent: true));
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void UsingTable_WithInvalidCondition_ThrowsArgumentException()
    {
        // Arrange
        var builder = SqlMergeBuilder<Product>.Create();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() =>
            builder.UsingTable<ProductUpdate>((t, s) => true));
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void UsingQuery_WithMismatchedTableType_ThrowsArgumentException()
    {
        // Arrange
        SqlQueryBuilder sourceQuery = SqlQueryBuilder.From<ProductUpdate>();
        var builder = SqlMergeBuilder<Product>.Create();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() =>
            builder.UsingQuery<Product>(sourceQuery, (t, s) => t.Id == s.Id));
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void BeginMatches_CalledTwice_ThrowsInvalidOperationException()
    {
        // Arrange
        var matchBuilder = SqlMergeBuilder<Product>.Create()
            .UsingTable<ProductUpdate>((t, s) => t.Id == s.Id);

        matchBuilder.BeginMatches();

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            matchBuilder.BeginMatches());
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void WhenMatched_WithoutCondition_CalledTwice_ThrowsInvalidOperationException()
    {
        // Arrange
        var whenBuilder = SqlMergeBuilder<Product>.Create()
            .UsingTable<ProductUpdate>((t, s) => t.Id == s.Id)
            .BeginMatches();

        whenBuilder.WhenMatched().Delete();

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            whenBuilder.WhenMatched());
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void WhenNotMatchedByTarget_WithoutCondition_CalledTwice_ThrowsInvalidOperationException()
    {
        // Arrange
        var whenBuilder = SqlMergeBuilder<Product>.Create()
            .UsingTable<ProductUpdate>((t, s) => t.Id == s.Id)
            .BeginMatches();

        whenBuilder.WhenNotMatchedByTarget().Insert().Set(
            new Dictionary<string, Expression<Func<ProductUpdate, object>>>()
            {
                { "Id", src => src.Id },
                { "Name", src => src.Name }
            });

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            whenBuilder.WhenNotMatchedByTarget());
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void WhenNotMatchedBySource_WithoutCondition_CalledTwice_ThrowsInvalidOperationException()
    {
        // Arrange
        var whenBuilder = SqlMergeBuilder<Product>.Create()
            .UsingTable<ProductUpdate>((t, s) => t.Id == s.Id)
            .BeginMatches();

        whenBuilder.WhenNotMatchedBySource().Delete();

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            whenBuilder.WhenNotMatchedBySource());
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void InsertAction_WithDuplicateColumnMapping_ThrowsInvalidOperationException()
    {
        // Arrange
        var insertAction = SqlMergeBuilder<Product>.Create()
            .UsingTable<ProductUpdate>((t, s) => t.Id == s.Id)
            .BeginMatches()
            .WhenNotMatchedByTarget()
            .Insert();

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            insertAction.Set(
                new Dictionary<string, Expression<Func<ProductUpdate, object>>>()
                {
                    { "Name", src => src.Name },
                    { "Price", src => src.Price }
                },
                new Dictionary<string, Expression<Func<Product, object>>>()
                {
                    { "Name", t => "Duplicate" }
                }));
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void UpdateAction_WithDuplicateColumnMapping_ThrowsInvalidOperationException()
    {
        // Arrange
        var updateAction = SqlMergeBuilder<Product>.Create()
            .UsingTable<ProductUpdate>((t, s) => t.Id == s.Id)
            .BeginMatches()
            .WhenMatched()
            .Update();

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() =>
            updateAction.Set(
                new Dictionary<string, Expression<Func<ProductUpdate, object>>>()
                {
                    { "Name", src => src.Name },
                    { "Price", src => src.Price }
                },
                new Dictionary<string, Expression<Func<Product, object>>>()
                {
                    { "Name", t => "Duplicate" }
                }));
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void InsertAction_WithEmptyColumnMappings_BuildsStatement()
    {
        // Arrange
        var builder = SqlMergeBuilder<Product>.Create()
            .UsingTable<ProductUpdate>((t, s) => t.Id == s.Id)
            .BeginMatches()
            .WhenNotMatchedByTarget()
            .Insert()
            .Set(new Dictionary<string, Expression<Func<ProductUpdate, object>>>())
            .EndMatches();

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.Contains("INSERT", actual);
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void UpdateAction_WithEmptyColumnMappings_BuildsStatement()
    {
        // Arrange
        var builder = SqlMergeBuilder<Product>.Create()
            .UsingTable<ProductUpdate>((t, s) => t.Id == s.Id)
            .BeginMatches()
            .WhenMatched()
            .Update()
            .Set(new Dictionary<string, Expression<Func<ProductUpdate, object>>>())
            .EndMatches();

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.Contains("UPDATE SET", actual);
    }

    [TestMethod]
    [TestCategory("Negative")]
    public void Merge_WithNoWhenClauses_BuildsValidStatement()
    {
        // Arrange
        var builder = SqlMergeBuilder<Product>.Create()
            .UsingTable<ProductUpdate>((t, s) => t.Id == s.Id)
            .BeginMatches()
            .EndMatches();

        string t1Alias = GetReferenceAlias<Product>();
        string t2Alias = GetReferenceAlias<ProductUpdate>();

        // Act
        string actual = builder.Build().ToString();

        // Assert
        Assert.StartsWith("MERGE", actual);
        Assert.Contains("USING", actual);
        Assert.EndsWith(";", actual);
    }

    #endregion

    #region Performance Tests

    [TestMethod]
    [TestCategory("Performance")]
    public void Performance_BuildSimpleMerge_CompletesWithinThreshold()
    {
        // Arrange
        const int iterations = 1000;
        const int maxMilliseconds = 100;
        Stopwatch stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        for (int i = 0; i < iterations; i++)
        {
            SqlMergeBuilder<Product> builder = SqlMergeBuilder<Product>.Create()
                .UsingTable<ProductUpdate>((t, s) => t.Id == s.Id)
                .BeginMatches()
                .WhenMatched()
                .Update()
                .Set(new Dictionary<string, Expression<Func<ProductUpdate, object>>>()
                {
                    { "Name", src => src.Name },
                    { "Price", src => src.Price }
                })
                .EndMatches();

            _ = builder.Build().ToString();
        }
        stopwatch.Stop();

        // Assert
        Assert.IsLessThan(maxMilliseconds, stopwatch.ElapsedMilliseconds, $"Performance test failed. Expected < {maxMilliseconds}ms, actual: {stopwatch.ElapsedMilliseconds}ms for {iterations} iterations.");
    }

    [TestMethod]
    [TestCategory("Performance")]
    public void Performance_BuildComplexMerge_CompletesWithinThreshold()
    {
        // Arrange
        const int iterations = 500;
        const int maxMilliseconds = 100;
        DateTime utcNow = DateTime.UtcNow;
        Stopwatch stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        for (int i = 0; i < iterations; i++)
        {
            SqlMergeBuilder<Product> builder = SqlMergeBuilder<Product>.Create()
                .UsingTable<ProductUpdate>((t, s) => t.Id == s.Id)
                .BeginMatches()
                .WhenMatched((t, s) => t.LastModified < s.LastModified)
                .Update()
                .Set(new Dictionary<string, Expression<Func<ProductUpdate, object>>>()
                {
                    { "Name", src => src.Name },
                    { "Price", src => src.Price },
                    { "LastModified", src => utcNow }
                })
                .WhenMatched((t, s) => !s.IsActive)
                .Delete()
                .WhenNotMatchedByTarget((_, s) => s.IsActive)
                .Insert()
                .Set(new Dictionary<string, Expression<Func<ProductUpdate, object>>>()
                {
                    { "Id", src => src.Id },
                    { "Name", src => src.Name },
                    { "Price", src => src.Price },
                    { "IsActive", src => src.IsActive },
                    { "CreatedDate", src => utcNow },
                    { "LastModified", src => utcNow }
                })
                .WhenNotMatchedBySource((t, _) => !t.IsArchived)
                .Update()
                .Set(new Dictionary<string, Expression<Func<ProductUpdate, object>>>()
                {
                    { "IsArchived", src => true },
                    { "LastModified", src => utcNow }
                })
                .EndMatches();

            _ = builder.Build().ToString();
        }
        stopwatch.Stop();

        // Assert
        Assert.IsLessThan(maxMilliseconds, stopwatch.ElapsedMilliseconds, $"Performance test failed. Expected < {maxMilliseconds}ms, actual: {stopwatch.ElapsedMilliseconds}ms for {iterations} iterations.");
    }

    [TestMethod]
    [TestCategory("Performance")]
    public void Performance_BuildMergeWithQuery_CompletesWithinThreshold()
    {
        // Arrange
        const int iterations = 500;
        const int maxMilliseconds = 150;
        Stopwatch stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        for (int i = 0; i < iterations; i++)
        {
            SqlQueryBuilder sourceQuery = SqlQueryBuilder.From<ProductUpdate>()
                .Where<ProductUpdate>(pu => pu.IsActive);

            SqlMergeBuilder<Product> builder = SqlMergeBuilder<Product>.Create()
                .UsingQuery<ProductUpdate>(sourceQuery, (t, s) => t.Id == s.Id)
                .BeginMatches()
                .WhenMatched()
                .Update()
                .Set(new Dictionary<string, Expression<Func<ProductUpdate, object>>>()
                {
                    { "Name", src => src.Name },
                    { "Price", src => src.Price }
                })
                .EndMatches();

            _ = builder.Build().ToString();
        }
        stopwatch.Stop();

        // Assert
        Assert.IsLessThan(maxMilliseconds, stopwatch.ElapsedMilliseconds, $"Performance test failed. Expected < {maxMilliseconds}ms, actual: {stopwatch.ElapsedMilliseconds}ms for {iterations} iterations.");
    }

    [TestMethod]
    [TestCategory("Performance")]
    public void Performance_BuildMultipleMergesInParallel_CompletesWithinThreshold()
    {
        // Arrange
        const int iterations = 100;
        const int maxMilliseconds = 200;
        DateTime utcNow = DateTime.UtcNow;
        Stopwatch stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        System.Threading.Tasks.Parallel.For(0, iterations, i =>
        {
            SqlMergeBuilder<Product> builder = SqlMergeBuilder<Product>.Create()
                .UsingTable<ProductUpdate>((t, s) => t.Id == s.Id)
                .BeginMatches()
                .WhenMatched()
                .Update()
                .Set(new Dictionary<string, Expression<Func<ProductUpdate, object>>>()
                {
                    { "Name", src => src.Name },
                    { "Price", src => src.Price },
                    { "LastModified", src => utcNow }
                })
                .EndMatches();

            _ = builder.Build().ToString();
        });
        stopwatch.Stop();

        // Assert
        Assert.IsLessThan(maxMilliseconds, stopwatch.ElapsedMilliseconds, $"Performance test failed. Expected < {maxMilliseconds}ms, actual: {stopwatch.ElapsedMilliseconds}ms for {iterations} parallel iterations.");
    }

    [TestMethod]
    [TestCategory("Performance")]
    public void Performance_BuildMergeWithLargeColumnSet_CompletesWithinThreshold()
    {
        // Arrange
        const int iterations = 500;
        const int maxMilliseconds = 150;
        DateTime utcNow = DateTime.UtcNow;
        Stopwatch stopwatch = new Stopwatch();

        // Act
        stopwatch.Start();
        for (int i = 0; i < iterations; i++)
        {
            SqlMergeBuilder<Product> builder = SqlMergeBuilder<Product>.Create()
                .UsingTable<ProductUpdate>((t, s) => t.Id == s.Id)
                .BeginMatches()
                .WhenMatched()
                .Update()
                .Set(new Dictionary<string, Expression<Func<ProductUpdate, object>>>()
                {
                    { "Name", src => src.Name },
                    { "Price", src => src.Price },
                    { "IsActive", src => src.IsActive },
                    { "LastModified", src => utcNow }
                })
                .WhenNotMatchedByTarget()
                .Insert()
                .Set(new Dictionary<string, Expression<Func<ProductUpdate, object>>>()
                {
                    { "Id", src => src.Id },
                    { "Name", src => src.Name },
                    { "Price", src => src.Price },
                    { "IsActive", src => src.IsActive },
                    { "CreatedDate", src => utcNow },
                    { "LastModified", src => utcNow }
                },
                new Dictionary<string, Expression<Func<Product, object>>>()
                {
                    { "IsArchived", t => false },
                    { "Level", t => 0 }
                })
                .EndMatches();

            _ = builder.Build().ToString();
        }
        stopwatch.Stop();

        // Assert
        Assert.IsLessThan(maxMilliseconds, stopwatch.ElapsedMilliseconds, $"Performance test failed. Expected < {maxMilliseconds}ms, actual: {stopwatch.ElapsedMilliseconds}ms for {iterations} iterations.");
    }

    [TestMethod]
    [TestCategory("Performance")]
    public void Performance_MemoryUsage_BuildMultipleMerges_DoesNotLeak()
    {
        // Arrange
        const int iterations = 1000;
        long initialMemory = GC.GetTotalMemory(true);

        // Act
        for (int i = 0; i < iterations; i++)
        {
            SqlMergeBuilder<Product> builder = SqlMergeBuilder<Product>.Create()
                .UsingTable<ProductUpdate>((t, s) => t.Id == s.Id)
                .BeginMatches()
                .WhenMatched()
                .Update()
                .Set(new Dictionary<string, Expression<Func<ProductUpdate, object>>>()
                {
                    { "Name", src => src.Name },
                    { "Price", src => src.Price }
                })
                .EndMatches();

            _ = builder.Build().ToString();
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long finalMemory = GC.GetTotalMemory(false);
        long memoryIncrease = finalMemory - initialMemory;

        // Assert - memory increase should be reasonable (less than 5MB for 1000 iterations)
        Assert.IsLessThan(5 * 1024 * 1024, memoryIncrease, $"Memory usage test failed. Memory increased by {memoryIncrease / 1024}KB for {iterations} iterations.");
    }

    #endregion

    #region Test Entity Classes

    /// <summary>
    /// Sample target table entity for testing MERGE operations.
    /// Represents a product in the target database.
    /// </summary>
    [SqlTable("Products")]
    private class Product
    {
        [SqlTablePrimaryKeyColumn("Id")]
        public Guid Id { get; set; }

        [SqlTableColumn("Name")]
        public string Name { get; set; } = string.Empty;

        [SqlTableColumn("Price")]
        public decimal Price { get; set; }

        [SqlTableColumn("IsActive")]
        public bool IsActive { get; set; }

        [SqlTableColumn("IsArchived")]
        public bool IsArchived { get; set; }

        [SqlTableColumn("ParentId")]
        public Guid? ParentId { get; set; }

        [SqlTableColumn("Level")]
        public int Level { get; set; }

        [SqlTableColumn("CreatedDate")]
        public DateTime CreatedDate { get; set; }

        [SqlTableColumn("LastModified")]
        public DateTime LastModified { get; set; }
    }

    /// <summary>
    /// Sample source table entity for testing MERGE operations.
    /// Represents incoming product updates from an external source.
    /// </summary>
    [SqlTable("ProductUpdates")]
    private class ProductUpdate
    {
        [SqlTablePrimaryKeyColumn("Id")]
        public Guid Id { get; set; }

        [SqlTableColumn("Name")]
        public string Name { get; set; } = string.Empty;

        [SqlTableColumn("Price")]
        public decimal Price { get; set; }

        [SqlTableColumn("IsActive")]
        public bool IsActive { get; set; }

        [SqlTableColumn("LastModified")]
        public DateTime LastModified { get; set; }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Get the reference alias for a given type by creating a temporary builder.
    /// This helps us build expected SQL strings dynamically.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <returns>The reference alias assigned to this type (e.g., "{t1Alias}").</returns>
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
