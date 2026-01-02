using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.SqlServer.Attributes;
using SujaySarma.Data.SqlServer.Builders.Internal;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace SujaySarma.Data.SqlServer.Tests.Builders;

/// <summary>
/// Unit tests for SqlWhere class.
/// </summary>
[TestClass]
public class SqlWhereTests
{
    // Static collection to store results from all tests
    private static readonly List<(string TestName, string WhereClause, bool Success)> _testResults = new();
    private static readonly object _lockObject = new();

    // Output file path
    private static readonly string _outputFilePath = Path.Combine(
        Path.GetDirectoryName(typeof(SqlWhereTests).Assembly.Location) ?? ".",
        "SqlWhereOutputLogs",
        $"{DateTime.Now:yyyyMMddHHmm}-SqlWhereTests_Output.txt"
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
        output.AppendLine("SQL WHERE Test Results");
        output.AppendLine("Generated: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        output.AppendLine(new string('=', 80));
        output.AppendLine();

        int successCount = 0;
        int exceptionCount = 0;

        foreach (var (testName, whereClause, success) in _testResults)
        {
            output.AppendLine($"Test: {testName}");
            output.AppendLine($"Status: {(success ? "SUCCESS" : "EXCEPTION (Expected)")}");
            output.AppendLine(new string('-', 80));

            if (success)
            {
                output.AppendLine(whereClause);
                successCount++;
            }
            else
            {
                output.AppendLine("[Test threw expected exception - no WHERE clause generated]");
                exceptionCount++;
            }

            output.AppendLine();
        }

        output.AppendLine(new string('=', 80));
        output.AppendLine($"Summary: {successCount} successful, {exceptionCount} expected exceptions");

        File.WriteAllText(_outputFilePath, output.ToString());

        Console.WriteLine($"WHERE clauses written to: {_outputFilePath}");
        Console.WriteLine($"Summary: {successCount} successful, {exceptionCount} expected exceptions");
    }

    /// <summary>
    /// Helper method to capture WHERE clause from a test
    /// </summary>
    private void CaptureWhereClause(string testName, SqlWhere where)
    {
        string result = string.Join(" ", where);

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

        [SqlTableColumn("Price")]
        public decimal Price { get; set; }

        [SqlTableColumn("IsDeleted")]
        public bool IsDeleted { get; set; }

        [SqlTableColumn("OrderDate")]
        public DateTime OrderDate { get; set; }
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

        [SqlTableColumn("Age")]
        public int Age { get; set; }
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

        [SqlTableColumn("Stock")]
        public int Stock { get; set; }
    }

    #endregion

    #region Initializer Tests - Functional

    [TestMethod(DisplayName = "Initializer: Single table condition")]
    [TestCategory("Functional")]
    public void Initializer_SingleTableCondition()
    {
        SqlWhere where = SqlWhere.Where<Order>(o => o.OrderId == 1);

        CaptureWhereClause(nameof(Initializer_SingleTableCondition), where);

        string result = string.Join(" ", where);
        Assert.Contains("OrderId", result);
        Assert.Contains("= 1", result);
        Assert.DoesNotContain("AND", result);
        Assert.DoesNotContain("OR", result);
    }

    [TestMethod(DisplayName = "Initializer: Two table condition")]
    [TestCategory("Functional")]
    public void Initializer_TwoTableCondition()
    {
        SqlWhere where = SqlWhere.Where<Order, Customer>((o, c) => o.CustomerId == c.CustomerId);

        CaptureWhereClause(nameof(Initializer_TwoTableCondition), where);

        string result = string.Join(" ", where);
        Assert.Contains("CustomerId", result);
        Assert.Contains("=", result);
    }

    #endregion

    #region Basic AND Tests - Functional

    [TestMethod(DisplayName = "AND: Single condition after Where")]
    [TestCategory("Functional")]
    public void And_SingleConditionAfterWhere()
    {
        SqlWhere where = SqlWhere.Where<Order>(o => o.OrderId == 1);
        where.AndWhere<Order>(o => o.CustomerId == 100);

        CaptureWhereClause(nameof(And_SingleConditionAfterWhere), where);

        string result = string.Join(" ", where);
        Assert.Contains("OrderId", result);
        Assert.Contains("CustomerId", result);
        Assert.Contains("AND", result);
    }

    [TestMethod(DisplayName = "AND: Multiple conditions")]
    [TestCategory("Functional")]
    public void And_MultipleConditions()
    {
        SqlWhere where = SqlWhere.Where<Order>(o => o.OrderId == 1);
        where.AndWhere<Order>(o => o.CustomerId == 100);
        where.AndWhere<Order>(o => o.Price > 0);

        CaptureWhereClause(nameof(And_MultipleConditions), where);

        string result = string.Join(" ", where);
        int andCount = result.Split("AND", StringSplitOptions.None).Length - 1;
        Assert.AreEqual(2, andCount);
    }

    [TestMethod(DisplayName = "AND: Complex expression with multiple fields")]
    [TestCategory("Functional")]
    public void And_ComplexExpression()
    {
        SqlWhere where = SqlWhere.Where<Order>(o => o.OrderId > 0);
        where.AndWhere<Order>(o => o.Price < 100 && o.Quantity > 5);

        CaptureWhereClause(nameof(And_ComplexExpression), where);

        string result = string.Join(" ", where);
        Assert.Contains("OrderId", result);
        Assert.Contains("Price", result);
        Assert.Contains("Quantity", result);
        Assert.Contains("AND", result);
    }

    [TestMethod(DisplayName = "AND: Two table condition after Where")]
    [TestCategory("Functional")]
    public void And_TwoTableCondition()
    {
        SqlWhere where = SqlWhere.Where<Order>(o => o.OrderId > 0);
        where.AndWhere<Order, Customer>((o, c) => o.CustomerId == c.CustomerId);

        CaptureWhereClause(nameof(And_TwoTableCondition), where);

        string result = string.Join(" ", where);
        Assert.Contains("CustomerId", result);
        Assert.Contains("AND", result);
    }

    #endregion

    #region Basic OR Tests - Functional

    [TestMethod(DisplayName = "OR: Single condition after Where")]
    [TestCategory("Functional")]
    public void Or_SingleConditionAfterWhere()
    {
        SqlWhere where = SqlWhere.Where<Order>(o => o.OrderId == 1);
        where.OrWhere<Order>(o => o.OrderId == 2);

        CaptureWhereClause(nameof(Or_SingleConditionAfterWhere), where);

        string result = string.Join(" ", where);
        Assert.Contains("OrderId", result);
        Assert.Contains("OR", result);
    }

    [TestMethod(DisplayName = "OR: Multiple conditions")]
    [TestCategory("Functional")]
    public void Or_MultipleConditions()
    {
        SqlWhere where = SqlWhere.Where<Order>(o => o.OrderId == 1);
        where.OrWhere<Order>(o => o.OrderId == 2);
        where.OrWhere<Order>(o => o.OrderId == 3);

        CaptureWhereClause(nameof(Or_MultipleConditions), where);

        string result = string.Join(" ", where);
        int orCount = result.Split("OR", StringSplitOptions.None).Length - 1;
        Assert.AreEqual(2, orCount);
    }

    #endregion

    #region Operator Tests - Functional

    [TestMethod(DisplayName = "Operators: Equality")]
    [TestCategory("Functional")]
    public void Operators_Equality()
    {
        SqlWhere where = SqlWhere.Where<Order>(o => o.OrderId == 100);

        CaptureWhereClause(nameof(Operators_Equality), where);

        string result = string.Join(" ", where);
        Assert.Contains("=", result);
        Assert.Contains("100", result);
    }

    [TestMethod(DisplayName = "Operators: Inequality")]
    [TestCategory("Functional")]
    public void Operators_Inequality()
    {
        SqlWhere where = SqlWhere.Where<Order>(o => o.OrderId != 100);

        CaptureWhereClause(nameof(Operators_Inequality), where);

        string result = string.Join(" ", where);
        Assert.Contains("<>", result);
    }

    [TestMethod(DisplayName = "Operators: Greater than")]
    [TestCategory("Functional")]
    public void Operators_GreaterThan()
    {
        SqlWhere where = SqlWhere.Where<Order>(o => o.Quantity > 10);

        CaptureWhereClause(nameof(Operators_GreaterThan), where);

        string result = string.Join(" ", where);
        Assert.Contains(">", result);
        Assert.Contains("10", result);
    }

    [TestMethod(DisplayName = "Operators: Greater than or equal")]
    [TestCategory("Functional")]
    public void Operators_GreaterThanOrEqual()
    {
        SqlWhere where = SqlWhere.Where<Order>(o => o.Quantity >= 10);

        CaptureWhereClause(nameof(Operators_GreaterThanOrEqual), where);

        string result = string.Join(" ", where);
        Assert.Contains(">=", result);
    }

    [TestMethod(DisplayName = "Operators: Less than")]
    [TestCategory("Functional")]
    public void Operators_LessThan()
    {
        SqlWhere where = SqlWhere.Where<Order>(o => o.Price < 100.50m);

        CaptureWhereClause(nameof(Operators_LessThan), where);

        string result = string.Join(" ", where);
        Assert.Contains("<", result);
    }

    [TestMethod(DisplayName = "Operators: Less than or equal")]
    [TestCategory("Functional")]
    public void Operators_LessThanOrEqual()
    {
        SqlWhere where = SqlWhere.Where<Order>(o => o.Price <= 100.50m);

        CaptureWhereClause(nameof(Operators_LessThanOrEqual), where);

        string result = string.Join(" ", where);
        Assert.Contains("<=", result);
    }

    [TestMethod(DisplayName = "Operators: IS NULL")]
    [TestCategory("Functional")]
    public void Operators_IsNull()
    {
        SqlWhere where = SqlWhere.Where<Customer>(c => c.Name == null);

        CaptureWhereClause(nameof(Operators_IsNull), where);

        string result = string.Join(" ", where);
        Assert.Contains("IS NULL", result);
    }

    [TestMethod(DisplayName = "Operators: IS NOT NULL")]
    [TestCategory("Functional")]
    public void Operators_IsNotNull()
    {
        SqlWhere where = SqlWhere.Where<Customer>(c => c.Name != null);

        CaptureWhereClause(nameof(Operators_IsNotNull), where);

        string result = string.Join(" ", where);
        Assert.Contains("IS NOT NULL", result);
    }

    #endregion

    #region Duplicate Detection Tests - Negative

    [TestMethod(DisplayName = "Duplicate: Exact same condition throws exception")]
    [TestCategory("Negative")]
    public void Duplicate_ExactSameCondition_ThrowsException()
    {
        SqlWhere where = SqlWhere.Where<Order>(o => o.OrderId == 1);

        // Attempting to add the exact same condition again
        ArgumentException ex = Assert.ThrowsExactly<ArgumentException>(() => where.AndWhere<Order>(o => o.OrderId == 1));

        CaptureException(nameof(Duplicate_ExactSameCondition_ThrowsException));

        Assert.Contains("duplicate condition", ex.Message);
    }

    [TestMethod(DisplayName = "Duplicate: Reversed operands detected as duplicate")]
    [TestCategory("Negative")]
    public void Duplicate_ReversedOperands_ThrowsException()
    {
        SqlWhere where = SqlWhere.Where<Order, Customer>((o, c) => o.CustomerId == c.CustomerId);

        // Reversed operands should still be detected as duplicate
        ArgumentException ex = Assert.ThrowsExactly<ArgumentException>(
            () => where.AndWhere<Order, Customer>((o, c) => c.CustomerId == o.CustomerId)
        );

        CaptureException(nameof(Duplicate_ReversedOperands_ThrowsException));

        Assert.Contains("duplicate condition", ex.Message);
    }

    [TestMethod(DisplayName = "Duplicate: Same condition with different operator allowed")]
    [TestCategory("Functional")]
    public void Duplicate_DifferentOperator_Allowed()
    {
        SqlWhere where = SqlWhere.Where<Order>(o => o.OrderId > 10);
        where.AndWhere<Order>(o => o.OrderId < 100);

        CaptureWhereClause(nameof(Duplicate_DifferentOperator_Allowed), where);

        string result = string.Join(" ", where);
        Assert.Contains(">", result);
        Assert.Contains("<", result);
    }

    [TestMethod(DisplayName = "Duplicate: Same field different values allowed")]
    [TestCategory("Functional")]
    public void Duplicate_DifferentValues_Allowed()
    {
        SqlWhere where = SqlWhere.Where<Order>(o => o.OrderId == 1);
        where.OrWhere<Order>(o => o.OrderId == 2);

        CaptureWhereClause(nameof(Duplicate_DifferentValues_Allowed), where);

        string result = string.Join(" ", where);
        Assert.Contains("OR", result);
    }

    #endregion

    #region Complex Condition Tests - Functional

    [TestMethod(DisplayName = "Complex: Multiple AND with OR")]
    [TestCategory("Functional")]
    public void Complex_MultipleAndWithOr()
    {
        SqlWhere where = SqlWhere.Where<Order>(o => o.OrderId > 0);
        where.AndWhere<Order>(o => o.IsDeleted == false);
        where.OrWhere<Order>(o => o.CustomerId == 100);

        CaptureWhereClause(nameof(Complex_MultipleAndWithOr), where);

        string result = string.Join(" ", where);
        Assert.Contains("AND", result);
        Assert.Contains("OR", result);
    }

    [TestMethod(DisplayName = "Complex: Nested conditions")]
    [TestCategory("Functional")]
    public void Complex_NestedConditions()
    {
        SqlWhere where = SqlWhere.Where<Order>(o => (o.OrderId > 0 && o.Price < 100) || o.IsDeleted == false);

        CaptureWhereClause(nameof(Complex_NestedConditions), where);

        string result = string.Join(" ", where);
        Assert.Contains("AND", result);
        Assert.Contains("OR", result);
    }

    [TestMethod(DisplayName = "Complex: Multiple tables with multiple conditions")]
    [TestCategory("Functional")]
    public void Complex_MultipleTablesMultipleConditions()
    {
        SqlWhere where = SqlWhere.Where<Order, Customer>((o, c) => o.CustomerId == c.CustomerId);
        where.AndWhere<Customer>(c => c.IsActive);
        where.AndWhere<Order>(o => o.Price > 0);

        CaptureWhereClause(nameof(Complex_MultipleTablesMultipleConditions), where);

        string result = string.Join(" ", where);
        int andCount = result.Split("AND", StringSplitOptions.None).Length - 1;
        Assert.IsGreaterThanOrEqualTo(2, andCount);
    }

    #endregion

    #region String Method Tests - Functional

    [TestMethod(DisplayName = "String: Contains")]
    [TestCategory("Functional")]
    public void String_Contains()
    {
        SqlWhere where = SqlWhere.Where<Customer>(c => c.Name.Contains("John"));

        CaptureWhereClause(nameof(String_Contains), where);

        string result = string.Join(" ", where);
        Assert.Contains("LIKE", result);
        Assert.Contains("John", result);
    }

    [TestMethod(DisplayName = "String: StartsWith")]
    [TestCategory("Functional")]
    public void String_StartsWith()
    {
        SqlWhere where = SqlWhere.Where<Customer>(c => c.Name.StartsWith("John"));

        CaptureWhereClause(nameof(String_StartsWith), where);

        string result = string.Join(" ", where);
        Assert.Contains("LIKE", result);
    }

    [TestMethod(DisplayName = "String: EndsWith")]
    [TestCategory("Functional")]
    public void String_EndsWith()
    {
        SqlWhere where = SqlWhere.Where<Customer>(c => c.Name.EndsWith("Doe"));

        CaptureWhereClause(nameof(String_EndsWith), where);

        string result = string.Join(" ", where);
        Assert.Contains("LIKE", result);
    }

    #endregion

    #region Validation Tests - Negative

    [TestMethod(DisplayName = "Validation: Invalid expression in Where throws exception")]
    [TestCategory("Negative")]
    public void Validation_InvalidExpressionInWhere_ThrowsException()
    {
        // Constant expression should fail validation
        ArgumentException ex = Assert.ThrowsExactly<ArgumentException>(
            () => SqlWhere.Where<Order>(o => true)
        );

        CaptureException(nameof(Validation_InvalidExpressionInWhere_ThrowsException));

        Assert.Contains("Cannot add condition", ex.Message);
    }

    [TestMethod(DisplayName = "Validation: Invalid expression in And throws exception")]
    [TestCategory("Negative")]
    public void Validation_InvalidExpressionInAnd_ThrowsException()
    {
        SqlWhere where = SqlWhere.Where<Order>(o => o.OrderId > 0);

        // Constant expression should fail validation
        ArgumentException ex = Assert.ThrowsExactly<ArgumentException>(
            () => where.AndWhere<Order>(o => true)
        );

        CaptureException(nameof(Validation_InvalidExpressionInAnd_ThrowsException));

        Assert.Contains("Cannot add condition", ex.Message);
    }

    [TestMethod(DisplayName = "Validation: Non-ORM type in Where throws exception")]
    [TestCategory("Negative")]
    public void Validation_NonOrmTypeInWhere_ThrowsException()
    {
        // Using a type without SqlTable attribute should fail
        TypeLoadException ex = Assert.ThrowsExactly<TypeLoadException>(
            () => SqlWhere.Where<string>(s => s.Length > 0)
        );

        CaptureException(nameof(Validation_NonOrmTypeInWhere_ThrowsException));

        Assert.Contains("not valid", ex.Message);
    }

    #endregion

    #region Edge Cases - Functional

    [TestMethod(DisplayName = "Edge: Boolean column direct reference")]
    [TestCategory("Functional")]
    public void Edge_BooleanDirectReference()
    {
        SqlWhere where = SqlWhere.Where<Order>(o => o.IsDeleted);

        CaptureWhereClause(nameof(Edge_BooleanDirectReference), where);

        string result = string.Join(" ", where);
        Assert.Contains("IsDeleted", result);
    }

    [TestMethod(DisplayName = "Edge: Negated boolean column")]
    [TestCategory("Functional")]
    public void Edge_NegatedBoolean()
    {
        SqlWhere where = SqlWhere.Where<Order>(o => !o.IsDeleted);

        CaptureWhereClause(nameof(Edge_NegatedBoolean), where);

        string result = string.Join(" ", where);
        Assert.IsTrue(result.Contains("NOT") || result.Contains("IsDeleted"));
    }

    [TestMethod(DisplayName = "Edge: Chained conditions")]
    [TestCategory("Functional")]
    public void Edge_ChainedConditions()
    {
        // Demonstrates fluent builder pattern
        SqlWhere where = SqlWhere.Where<Order>(o => o.OrderId > 0);
        where.AndWhere<Order>(o => o.Price < 1000);
        where.AndWhere<Order>(o => !o.IsDeleted);
        where.OrWhere<Order>(o => o.CustomerId == 999);

        CaptureWhereClause(nameof(Edge_ChainedConditions), where);

        string result = string.Join(" ", where);
        Assert.Contains("AND", result);
        Assert.Contains("OR", result);
    }

    #endregion

    #region Performance Tests

    [TestMethod(DisplayName = "Performance: Multiple conditions")]
    [TestCategory("Performance")]
    public void Performance_MultipleConditions()
    {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

        for (int i = 0; i < 1000; i++)
        {
            SqlWhere where = SqlWhere.Where<Order>(o => o.OrderId > 0);
            where.AndWhere<Order>(o => o.Price < 100);
            where.OrWhere<Order>(o => o.IsDeleted == false);
            string result = string.Join(" ", where);
        }

        stopwatch.Stop();

        Assert.IsLessThan(1000, stopwatch.ElapsedMilliseconds, $"Operation took {stopwatch.ElapsedMilliseconds}ms, expected < 1000ms");
    }

    #endregion
}