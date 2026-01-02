using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.SqlServer.Attributes;
using SujaySarma.Data.SqlServer.Builders.Constants;
using SujaySarma.Data.SqlServer.Builders.Internal;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SujaySarma.Data.SqlServer.Tests.Builders;

[TestClass]
public class SqlJoinTests
{
    // Static collection to store results from all tests
    private static readonly List<(string TestName, string JoinClause, bool Success)> _testResults = new();
    private static readonly object _lockObject = new();

    // Output file path
    private static readonly string _outputFilePath = Path.Combine(
        Path.GetDirectoryName(typeof(SqlJoinTests).Assembly.Location) ?? ".", 
        "SqlJoinOutputLogs",
        $"{DateTime.Now:yyyyMMddHHmm}-SqlJoinTests_Output.txt"
    );

    // TestContext for accessing current test information
    public TestContext? TestContext { get; set; }

    /// <summary>
    /// Called once before any tests in this class run
    /// </summary>
    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        _testResults.Clear();

        string dirName = Path.GetDirectoryName(_outputFilePath)!;
        if (!Directory.Exists(dirName))
        {
            Directory.CreateDirectory(dirName);
        }
        
        // Insert existing file if it exists
        if (File.Exists(_outputFilePath))
        {
            File.Delete(_outputFilePath);
        }
    }

    /// <summary>
    /// Called once after all tests in this class have run
    /// </summary>
    [ClassCleanup]
    public static void ClassCleanup()
    {
        // Write all collected results to file
        StringBuilder output = new StringBuilder();
        output.AppendLine("SQL JOIN Test Results");
        output.AppendLine("Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        output.AppendLine(new string('=', 80));
        output.AppendLine();

        int successCount = 0;
        int exceptionCount = 0;

        foreach (var (testName, joinClause, success) in _testResults)
        {
            output.AppendLine($"Test: {testName}");
            output.AppendLine($"Status: {(success ? "SUCCESS" : "EXCEPTION (Expected)")}");
            output.AppendLine(new string('-', 80));
            
            if (success)
            {
                output.AppendLine(joinClause);
                successCount++;
            }
            else
            {
                output.AppendLine("[Test threw expected exception - no JOIN clause generated]");
                exceptionCount++;
            }
            
            output.AppendLine();
        }

        output.AppendLine(new string('=', 80));
        output.AppendLine($"Summary: {successCount} successful, {exceptionCount} expected exceptions");

        File.WriteAllText(_outputFilePath, output.ToString());
        
        Console.WriteLine($"JOIN clauses written to: {_outputFilePath}");
        Console.WriteLine($"Summary: {successCount} successful, {exceptionCount} expected exceptions");
    }

    /// <summary>
    /// Helper method to capture JOIN clause from a test
    /// </summary>
    private void CaptureJoinClause(string testName, SqlJoin join)
    {
        string result = string.Join(" ", join);
        
        lock (_lockObject)
        {
            _testResults.Add((testName, result, true));
        }
    }

    /// <summary>
    /// Helper method to mark a test that threw an expected exception
    /// </summary>
    private void CaptureException(string testName)
    {
        lock (_lockObject)
        {
            _testResults.Add((testName, string.Empty, false));
        }
    }

    #region Test Entity Classes

    [SqlTable("Orders")]
    private class Order
    {
        [SqlTableColumn("OrderId")]
        public int OrderId { get; set; }

        [SqlTableColumn("CustomerId")]
        public int CustomerId { get; set; }

        [SqlTableColumn("ProductId")]
        public int ProductId { get; set; }

        [SqlTableColumn("Quantity")]
        public int Quantity { get; set; }

        [SqlTableColumn("IsDeleted")]
        public bool IsDeleted { get; set; }
    }

    [SqlTable("Customers")]
    private class Customer
    {
        [SqlTableColumn("CustomerId")]
        public int CustomerId { get; set; }

        [SqlTableColumn("Name")]
        public string Name { get; set; } = string.Empty;

        [SqlTableColumn("IsActive")]
        public bool IsActive { get; set; }

        [SqlTableColumn("IsDeleted")]
        public bool IsDeleted { get; set; }
    }

    [SqlTable("Products")]
    private class Product
    {
        [SqlTableColumn("ProductId")]
        public int ProductId { get; set; }

        [SqlTableColumn("Name")]
        public string Name { get; set; } = string.Empty;

        [SqlTableColumn("Price")]
        public decimal Price { get; set; }

        [SqlTableColumn("CategoryId")]
        public int CategoryId { get; set; }
    }

    [SqlTable("Categories")]
    private class Category
    {
        [SqlTableColumn("CategoryId")]
        public int CategoryId { get; set; }

        [SqlTableColumn("Name")]
        public string Name { get; set; } = string.Empty;
    }

    #endregion


    #region Basic Join Tests

    [TestMethod(DisplayName = "InnerJoin: Simple join with single condition")]
    public void InnerJoin_SimpleCondition()
    {
        SqlJoin join = new SqlJoin();
        Expression<Func<Order, Customer, bool>> condition = (o, c) => o.CustomerId == c.CustomerId;

        join.InnerJoin(condition, SqlHint.None);

        string result = string.Join(" ", join);
        CaptureJoinClause(nameof(InnerJoin_SimpleCondition), join);
        
        Assert.Contains("INNER JOIN", result);
        Assert.Contains("[dbo].[Customers]", result);
        Assert.Contains("ON", result);
    }

    [TestMethod(DisplayName = "LeftJoin: Simple join with single condition")]
    public void LeftJoin_SimpleCondition()
    {
        SqlJoin join = new SqlJoin();
        Expression<Func<Order, Customer, bool>> condition = (o, c) => o.CustomerId == c.CustomerId;

        join.LeftJoin(condition, SqlHint.None);

        string result = string.Join(" ", join);
        CaptureJoinClause(nameof(LeftJoin_SimpleCondition), join);
        
        Assert.Contains("LEFT JOIN", result);
        Assert.Contains("[dbo].[Customers]", result);
    }

    [TestMethod(DisplayName = "RightJoin: Simple join with single condition")]
    public void RightJoin_SimpleCondition()
    {
        SqlJoin join = new SqlJoin();
        Expression<Func<Order, Customer, bool>> condition = (o, c) => o.CustomerId == c.CustomerId;

        join.RightJoin(condition, SqlHint.None);

        string result = string.Join(" ", join);
        CaptureJoinClause(nameof(RightJoin_SimpleCondition), join);
        
        Assert.Contains("RIGHT JOIN", result);
    }

    [TestMethod(DisplayName = "FullJoin: Simple join with single condition")]
    public void FullJoin_SimpleCondition()
    {
        SqlJoin join = new SqlJoin();
        Expression<Func<Order, Customer, bool>> condition = (o, c) => o.CustomerId == c.CustomerId;

        join.FullJoin(condition, SqlHint.None);

        string result = string.Join(" ", join);
        CaptureJoinClause(nameof(FullJoin_SimpleCondition), join);
        
        Assert.Contains("FULL JOIN", result);
    }

    [TestMethod(DisplayName = "CrossJoin: No ON clause generated")]
    public void CrossJoin_NoOnClause()
    {
        SqlJoin join = new SqlJoin();
        join.CrossJoin<Order, Customer>(SqlHint.None);

        string result = string.Join(" ", join);
        CaptureJoinClause(nameof(CrossJoin_NoOnClause), join);
        
        Assert.Contains("CROSS JOIN", result);
        Assert.DoesNotContain(" ON ", result);
    }

    #endregion

    #region Complex Condition Tests

    [TestMethod(DisplayName = "InnerJoin: Multiple conditions with AND")]
    public void InnerJoin_MultipleConditionsAnd()
    {
        SqlJoin join = new SqlJoin();
        Expression<Func<Order, Customer, bool>> condition = (o, c) => 
            o.CustomerId == c.CustomerId && !c.IsDeleted;

        join.InnerJoin(condition, SqlHint.None);

        CaptureJoinClause("InnerJoin_MultipleConditionsAnd", join);

        string result = string.Join(" ", join);
        Assert.Contains("AND", result);
        Assert.Contains("IsDeleted", result);
    }

    [TestMethod(DisplayName = "InnerJoin: Multiple conditions with OR")]
    public void InnerJoin_MultipleConditionsOr()
    {
        SqlJoin join = new SqlJoin();
        Expression<Func<Order, Customer, bool>> condition = (o, c) => 
            o.CustomerId == c.CustomerId || c.IsActive;

        join.InnerJoin(condition, SqlHint.None);

        CaptureJoinClause("InnerJoin_MultipleConditionsOr", join);

        string result = string.Join(" ", join);
        Assert.Contains("OR", result);
    }

    [TestMethod(DisplayName = "InnerJoin: Complex nested conditions")]
    public void InnerJoin_ComplexConditions()
    {
        SqlJoin join = new SqlJoin();
        Expression<Func<Order, Customer, bool>> condition = (o, c) => 
            o.CustomerId == c.CustomerId && (c.IsActive || !c.IsDeleted);

        join.InnerJoin(condition, SqlHint.None);

        CaptureJoinClause("InnerJoin_ComplexConditions", join);

        string result = string.Join(" ", join);
        Assert.Contains("AND", result);
        Assert.Contains("OR", result);
    }

    [TestMethod(DisplayName = "InnerJoin: Different comparison operators")]
    public void InnerJoin_DifferentOperators()
    {
        SqlJoin join = new SqlJoin();
        Expression<Func<Order, Product, bool>> condition = (o, p) => 
            o.ProductId == p.ProductId && p.Price > 100;

        join.InnerJoin(condition, SqlHint.None);

        CaptureJoinClause("InnerJoin_DifferentOperators", join);

        string result = string.Join(" ", join);
        Assert.Contains("=", result);
        Assert.Contains(">", result);
    }

    #endregion

    #region Multiple Join Tests

    [TestMethod(DisplayName = "Multiple joins: Order -> Customer -> Product")]
    public void MultipleJoins_ThreeTables()
    {
        SqlJoin join = new SqlJoin();
        
        join.InnerJoin<Order, Customer>((o, c) => o.CustomerId == c.CustomerId, SqlHint.None);
        join.InnerJoin<Order, Product>((o, p) => o.ProductId == p.ProductId, SqlHint.None);

        CaptureJoinClause(nameof(MultipleJoins_ThreeTables), join);

        string result = string.Join(" ", join);
        int joinCount = result.Split("INNER JOIN", StringSplitOptions.None).Length - 1;
        Assert.AreEqual(2, joinCount);
    }

    [TestMethod(DisplayName = "Multiple joins: Different join types")]
    public void MultipleJoins_DifferentTypes()
    {
        SqlJoin join = new SqlJoin();
        
        join.InnerJoin<Order, Customer>((o, c) => o.CustomerId == c.CustomerId, SqlHint.None);
        join.LeftJoin<Product, Category>((p, cat) => p.CategoryId == cat.CategoryId, SqlHint.None);

        CaptureJoinClause(nameof(MultipleJoins_DifferentTypes), join);

        string result = string.Join(" ", join);
        Assert.Contains("INNER JOIN", result);
        Assert.Contains("LEFT JOIN", result);
    }

    [TestMethod(DisplayName = "Multiple joins: Chain of four tables")]
    public void MultipleJoins_FourTables()
    {
        SqlJoin join = new SqlJoin();
        
        join.InnerJoin<Order, Customer>((o, c) => o.CustomerId == c.CustomerId, SqlHint.None);
        join.InnerJoin<Order, Product>((o, p) => o.ProductId == p.ProductId, SqlHint.None);
        join.InnerJoin<Product, Category>((p, cat) => p.CategoryId == cat.CategoryId, SqlHint.None);

        CaptureJoinClause(nameof(MultipleJoins_FourTables), join);

        string result = string.Join(" ", join);
        int joinCount = result.Split("INNER JOIN", StringSplitOptions.None).Length - 1;
        Assert.AreEqual(3, joinCount);
    }

    #endregion

    #region Duplicate Join Detection Tests

    [TestMethod(DisplayName = "Duplicate: Exact same join throws exception")]
    public void DuplicateJoin_ExactSame_ThrowsException()
    {
        SqlJoin join = new SqlJoin();
        Expression<Func<Order, Customer, bool>> condition = (o, c) => o.CustomerId == c.CustomerId;

        join.InnerJoin(condition, SqlHint.None);

        // Attempting to add the exact same join again
        Assert.ThrowsExactly<InvalidOperationException>(() => join.InnerJoin(condition, SqlHint.None));
        
        CaptureException(nameof(DuplicateJoin_ExactSame_ThrowsException));
    }

    [TestMethod(DisplayName = "Duplicate: Same tables, different join type allowed")]
    public void DuplicateJoin_SameTablesDifferentType_Allowed()
    {
        SqlJoin join = new SqlJoin();
        Expression<Func<Order, Customer, bool>> condition = (o, c) => o.CustomerId == c.CustomerId;

        join.InnerJoin(condition, SqlHint.None);

        // Different join type should be allowed
        try
        {
            join.LeftJoin(condition, SqlHint.None);
            CaptureJoinClause(nameof(DuplicateJoin_SameTablesDifferentType_Allowed), join);
        }
        catch
        {
            Assert.Fail("Different join type should be allowed for same tables and condition.");
        }
    }

    [TestMethod(DisplayName = "Duplicate: Same tables but additional condition throws exception")]
    public void DuplicateJoin_SameTablesAdditionalCondition_ThrowsException()
    {
        SqlJoin join = new SqlJoin();
        
        join.InnerJoin<Order, Customer>((o, c) => o.CustomerId == c.CustomerId, SqlHint.None);

        // Same tables with additional filter condition
        Assert.ThrowsExactly<InvalidOperationException>(() => join.InnerJoin<Order, Customer>((o, c) => o.CustomerId == c.CustomerId && c.IsDeleted == false, SqlHint.None));
        
        CaptureException(nameof(DuplicateJoin_SameTablesAdditionalCondition_ThrowsException));
    }

    [TestMethod(DisplayName = "Duplicate: Reverse table order detected as duplicate")]
    public void DuplicateJoin_ReverseTableOrder_ThrowsException()
    {
        SqlJoin join = new SqlJoin();
        
        join.InnerJoin<Order, Customer>((o, c) => o.CustomerId == c.CustomerId, SqlHint.None);

        // Reversed order should still be detected as duplicate
        Assert.ThrowsExactly<InvalidOperationException>(() => 
            join.InnerJoin<Customer, Order>((c, o) => c.CustomerId == o.CustomerId, SqlHint.None));
        
        CaptureException(nameof(DuplicateJoin_ReverseTableOrder_ThrowsException));
    }

    [TestMethod(DisplayName = "Duplicate: Different tables allowed")]
    public void DuplicateJoin_DifferentTables_Allowed()
    {
        SqlJoin join = new SqlJoin();
        
        join.InnerJoin<Order, Customer>((o, c) => o.CustomerId == c.CustomerId, SqlHint.None);
        
        // Different right-side table should be allowed
        join.InnerJoin<Order, Product>((o, p) => o.ProductId == p.ProductId, SqlHint.None);

        CaptureJoinClause(nameof(DuplicateJoin_DifferentTables_Allowed), join);

        string result = string.Join(" ", join);
        int joinCount = result.Split("INNER JOIN", StringSplitOptions.None).Length - 1;
        Assert.AreEqual(2, joinCount);
    }

    #endregion

    #region Operator Parsing Tests

    [TestMethod(DisplayName = "Operators: Equality operator")]
    public void Operators_Equality()
    {
        SqlJoin join = new SqlJoin();
        join.InnerJoin<Order, Customer>((o, c) => o.CustomerId == c.CustomerId, SqlHint.None);

        CaptureJoinClause(nameof(Operators_Equality), join);

        string result = string.Join(" ", join);
        Assert.Contains("=", result);
    }

    [TestMethod(DisplayName = "Operators: Inequality operator")]
    public void Operators_Inequality()
    {
        SqlJoin join = new SqlJoin();
        join.InnerJoin<Order, Product>((o, p) => o.ProductId != p.ProductId, SqlHint.None);

        CaptureJoinClause(nameof(Operators_Inequality), join);

        string result = string.Join(" ", join);
        Assert.Contains("<>", result);
    }

    [TestMethod(DisplayName = "Operators: Greater than operator")]
    public void Operators_GreaterThan()
    {
        SqlJoin join = new SqlJoin();
        join.InnerJoin<Order, Product>((o, p) => o.Quantity > 10, SqlHint.None);

        CaptureJoinClause(nameof(Operators_GreaterThan), join);

        string result = string.Join(" ", join);
        Assert.Contains(">", result);
    }

    [TestMethod(DisplayName = "Operators: Less than or equal operator")]
    public void Operators_LessThanOrEqual()
    {
        SqlJoin join = new SqlJoin();
        join.InnerJoin<Order, Product>((o, p) => p.Price <= 500, SqlHint.None);

        CaptureJoinClause(nameof(Operators_LessThanOrEqual), join);

        string result = string.Join(" ", join);
        Assert.Contains("<=", result);
    }

    [TestMethod(DisplayName = "Operators: IS NULL condition")]
    public void Operators_IsNull()
    {
        SqlJoin join = new SqlJoin();
        join.InnerJoin<Order, Customer>((o, c) => c.Name == null, SqlHint.None);

        CaptureJoinClause(nameof(Operators_IsNull), join);

        string result = string.Join(" ", join);
        Assert.Contains("IS NULL", result);
    }

    [TestMethod(DisplayName = "Operators: IS NOT NULL condition")]
    public void Operators_IsNotNull()
    {
        SqlJoin join = new SqlJoin();
        join.InnerJoin<Order, Customer>((o, c) => c.Name != null, SqlHint.None);

        CaptureJoinClause(nameof(Operators_IsNotNull), join);

        string result = string.Join(" ", join);
        Assert.Contains("IS NOT NULL", result);
    }

    #endregion

    #region Edge Cases and Error Handling

    [TestMethod(DisplayName = "Error: Invalid expression throws ArgumentException")]
    public void Error_InvalidExpression_ThrowsException()
    {
        SqlJoin join = new SqlJoin();
        
        // This will fail IsValidCondition check
        Expression<Func<Order, Customer, bool>> invalidCondition = (o, c) => true;

        Assert.ThrowsExactly<ArgumentException>(() => join.InnerJoin(invalidCondition, SqlHint.None));
        
        CaptureException(nameof(Error_InvalidExpression_ThrowsException));
    }

    [TestMethod(DisplayName = "Edge: Empty SqlJoin produces empty string")]
    public void Edge_EmptyJoin()
    {
        SqlJoin join = new SqlJoin();
        
        CaptureJoinClause(nameof(Edge_EmptyJoin), join);
        
        string result = string.Join(" ", join);
        Assert.AreEqual(string.Empty, result.Trim());
    }

    [TestMethod(DisplayName = "Edge: Table aliases are used in conditions")]
    public void Edge_TableAliasesUsed()
    {
        SqlJoin join = new SqlJoin();
        join.InnerJoin<Order, Customer>((o, c) => o.CustomerId == c.CustomerId, SqlHint.None);

        CaptureJoinClause(nameof(Edge_TableAliasesUsed), join);

        string result = string.Join(" ", join);
        // Should contain alias-qualified column references
        Assert.Contains("[", result);
        Assert.Contains("]", result);
    }

    [TestMethod(DisplayName = "Edge: Boolean column without comparison")]
    public void Edge_BooleanColumnDirect()
    {
        SqlJoin join = new SqlJoin();
        join.InnerJoin<Order, Customer>((o, c) => c.IsActive, SqlHint.None);

        CaptureJoinClause(nameof(Edge_BooleanColumnDirect), join);

        string result = string.Join(" ", join);
        Assert.Contains("IsActive", result);
    }

    [TestMethod(DisplayName = "Edge: Negated boolean column")]
    public void Edge_NegatedBooleanColumn()
    {
        SqlJoin join = new SqlJoin();
        join.InnerJoin<Order, Customer>((o, c) => !c.IsDeleted, SqlHint.None);

        CaptureJoinClause(nameof(Edge_NegatedBooleanColumn), join);

        string result = string.Join(" ", join);
        Assert.IsTrue(result.Contains("NOT") || result.Contains("IsDeleted"));
    }

    #endregion

    #region Nullable Boolean Tests

    [SqlTable("NullableTestTable")]
    private class NullableTestEntity
    {
        [SqlTableColumn("Id")]
        public int Id { get; set; }

        [SqlTableColumn("IsActive")]
        public bool? IsActive { get; set; }

        [SqlTableColumn("IsDeleted")]
        public bool? IsDeleted { get; set; }
    }

    [TestMethod(DisplayName = "Nullable: Explicit true comparison")]
    public void Nullable_ExplicitTrueComparison()
    {
        SqlJoin join = new SqlJoin();
        join.InnerJoin<Order, NullableTestEntity>((o, n) => n.IsActive == true, SqlHint.None);

        CaptureJoinClause(nameof(Nullable_ExplicitTrueComparison), join);

        string result = string.Join(" ", join);
        Assert.Contains("IsActive", result);
        Assert.Contains("= 1", result);
    }

    [TestMethod(DisplayName = "Nullable: Explicit false comparison")]
    public void Nullable_ExplicitFalseComparison()
    {
        SqlJoin join = new SqlJoin();
        join.InnerJoin<Order, NullableTestEntity>((o, n) => n.IsDeleted == false, SqlHint.None);

        CaptureJoinClause(nameof(Nullable_ExplicitFalseComparison), join);

        string result = string.Join(" ", join);
        Assert.Contains("IsDeleted", result);
        Assert.Contains("= 0", result);
    }

    [TestMethod(DisplayName = "Nullable: Null coalescing operator")]
    public void Nullable_NullCoalescing()
    {
        SqlJoin join = new SqlJoin();
        join.InnerJoin<Order, NullableTestEntity>((o, n) => (n.IsActive ?? false), SqlHint.None);

        CaptureJoinClause(nameof(Nullable_NullCoalescing), join);

        string result = string.Join(" ", join);
        Assert.Contains("IsActive", result);
    }

    [TestMethod(DisplayName = "Nullable: Null-coalescing operator treats null as false")]
    public void Nullable_NullCoalescingOperator()
    {
        SqlJoin join = new SqlJoin();
        join.InnerJoin<Order, NullableTestEntity>((o, n) => (n.IsActive ?? false), SqlHint.None);

        CaptureJoinClause(nameof(Nullable_NullCoalescingOperator), join);

        string result = string.Join(" ", join);
        Assert.Contains("ISNULL", result);
        Assert.Contains("IsActive", result);
        Assert.Contains(", 0)", result);  // false translates to 0
    }

    #endregion

    #region Enumeration Tests

    [TestMethod(DisplayName = "Enumeration: Multiple joins enumerate in order")]
    public void Enumeration_MultipleJoinsInOrder()
    {
        SqlJoin join = new SqlJoin();
        
        join.InnerJoin<Order, Customer>((o, c) => o.CustomerId == c.CustomerId, SqlHint.None);
        join.InnerJoin<Order, Product>((o, p) => o.ProductId == p.ProductId, SqlHint.None);

        CaptureJoinClause(nameof(Enumeration_MultipleJoinsInOrder), join);

        var joins = join.ToList();
        Assert.HasCount(2, joins);
        Assert.Contains("Customers", joins[0]);
        Assert.Contains("Products", joins[1]);
    }

    [TestMethod(DisplayName = "Enumeration: Join clauses are complete")]
    public void Enumeration_CompleteJoinClauses()
    {
        SqlJoin join = new SqlJoin();
        join.InnerJoin<Order, Customer>((o, c) => o.CustomerId == c.CustomerId, SqlHint.None);

        CaptureJoinClause(nameof(Enumeration_CompleteJoinClauses), join);

        foreach (string joinClause in join)
        {
            Assert.Contains("INNER JOIN", joinClause);
            Assert.Contains("[dbo].[Customers]", joinClause);
            Assert.Contains("ON", joinClause);
        }
    }

    #endregion

    #region Schema Tests

    [SqlTable("SpecialOrders")]
    private class SpecialOrder
    {
        [SqlTableColumn("OrderId")]
        public int OrderId { get; set; }
    }

    [TestMethod(DisplayName = "Schema: Default schema is dbo")]
    public void Schema_DefaultSchemaDbo()
    {
        SqlJoin join = new SqlJoin();
        join.InnerJoin<Order, Customer>((o, c) => o.CustomerId == c.CustomerId, SqlHint.None);

        CaptureJoinClause(nameof(Schema_DefaultSchemaDbo), join);

        string result = string.Join(" ", join);
        Assert.Contains("[dbo]", result);
    }

    #endregion
}