using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.SqlServer.Attributes;
using SujaySarma.Data.SqlServer.Builders;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SujaySarma.Data.SqlServer.Tests.Builders;


[TestClass]
public class SqlExpressionParserTests
{

    //[SqlTable("Foo")]
    //private class TInnerJoinedSource
    //{
    //    /// <inheritdoc />
    //    [SqlTableColumn("boo")]
    //    public int SourceColumn { get; set; } = 100;
        
    //}


    //[TestMethod(DisplayName = "Parse: (DELETE AFTER USE) Tests for UpdateBuilder")]
    //public void Parse_TestsForUpdateBuilder()
    //{
    //    string foo = "value";

    //    Dictionary<string, Expression<Func<TInnerJoinedSource, object>>> mappings = new()
    //    {
    //        ["Status"] = s => "Active",             // ConstantExpression
    //        ["Count"] = s => foo.ToUpper(),         // MemberExpression → ConstantExpression
    //        ["Column"] = p => p.SourceColumn        // MemberExpression (entity column)
    //    };

    //    string ex;
    //    foreach(string key in mappings.Keys)
    //    {
    //        ex = SqlExpressionParser.Parse(mappings[key]);
    //        //Console.WriteLine($"Key: {key}, Expression: {ex}");
    //    }
    //}


    [TestMethod(DisplayName = "Parse: Simple Expression")]
    public void Parse_SimpleExpression()
    {
        Expression expression = (Person p) => p.Age > 30;
        string result = SqlExpressionParser.Parse(expression);

        // The column is prefixed with table alias, but this can vary when running multiple test methods in parallel.
        // So, check only for the column name and the operator.
        Assert.Contains(".[Age] > 30", result);
    }


    [TestMethod(DisplayName = "Parse: New expression")]
    public void Parse_NewExpression()
    {
        Expression expression = (Person p) => new { Age = p.Age + 5 };
     
        string result = SqlExpressionParser.Parse(expression);
        
        // The column is prefixed with table alias, but this can vary when running multiple test methods in parallel.
        // So, check only for the column name and the operator.
        Assert.Contains(".[Age] + 5", result);
    }


    [TestMethod(DisplayName = "Parse: New expression, assignment as alias")]
    public void Parse_NewExpressionWithAssignmentAsAlias()
    {
        Expression expression = (Person p) => new { Age = p.Age + 5 };
     
        string result = SqlExpressionParser.Parse(expression, assignmentTreatment: SqlExpressionParser.AssignmentTreatment.AsAlias);
        
        // The column is prefixed with table alias, but this can vary when running multiple test methods in parallel.
        // So, check only for the column name and the operator.
        Assert.Contains(".[Age] + 5) AS [Age]", result);
    }


    [TestMethod(DisplayName = "Parse: New expression, assignment as-is")]
    public void Parse_NewExpressionWithAssignmentAsIs()
    {
        Expression expression = (Person p) => new { Age = 55 };
     
        string result = SqlExpressionParser.Parse(expression, assignmentTreatment: SqlExpressionParser.AssignmentTreatment.AsIs);
        
        // The column is prefixed with table alias, but this can vary when running multiple test methods in parallel.
        // So, check only for the column name and the operator.
        Assert.AreEqual("[Age] = 55", result);
    }





    #region Binary Expression Tests

    [TestMethod(DisplayName = "Parse: Binary - Equal operator")]
    public void Parse_BinaryExpression_Equal()
    {
        Expression expression = (Person p) => p.Age == 30;
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains(".[Age] = 30", result);
    }

    [TestMethod(DisplayName = "Parse: Binary - NotEqual operator")]
    public void Parse_BinaryExpression_NotEqual()
    {
        Expression expression = (Person p) => p.Age != 30;
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains(".[Age] <> 30", result);
    }

    [TestMethod(DisplayName = "Parse: Binary - GreaterThan operator")]
    public void Parse_BinaryExpression_GreaterThan()
    {
        Expression expression = (Person p) => p.Age > 30;
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains(".[Age] > 30", result);
    }

    [TestMethod(DisplayName = "Parse: Binary - GreaterThanOrEqual operator")]
    public void Parse_BinaryExpression_GreaterThanOrEqual()
    {
        Expression expression = (Person p) => p.Age >= 30;
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains(".[Age] >= 30", result);
    }

    [TestMethod(DisplayName = "Parse: Binary - LessThan operator")]
    public void Parse_BinaryExpression_LessThan()
    {
        Expression expression = (Person p) => p.Age < 30;
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains(".[Age] < 30", result);
    }

    [TestMethod(DisplayName = "Parse: Binary - LessThanOrEqual operator")]
    public void Parse_BinaryExpression_LessThanOrEqual()
    {
        Expression expression = (Person p) => p.Age <= 30;
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains(".[Age] <= 30", result);
    }

    [TestMethod(DisplayName = "Parse: Binary - Add operator")]
    public void Parse_BinaryExpression_Add()
    {
        Expression expression = (Person p) => p.Age + 10;
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains(".[Age] + 10", result);
    }

    [TestMethod(DisplayName = "Parse: Binary - Subtract operator")]
    public void Parse_BinaryExpression_Subtract()
    {
        Expression expression = (Person p) => p.Age - 10;
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains(".[Age] - 10", result);
    }

    [TestMethod(DisplayName = "Parse: Binary - Multiply operator")]
    public void Parse_BinaryExpression_Multiply()
    {
        Expression expression = (Person p) => p.Age * 2;
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains(".[Age] * 2", result);
    }

    [TestMethod(DisplayName = "Parse: Binary - Divide operator")]
    public void Parse_BinaryExpression_Divide()
    {
        Expression expression = (Person p) => p.Age / 2;
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains(".[Age] / 2", result);
    }

    [TestMethod(DisplayName = "Parse: Binary - AndAlso operator")]
    public void Parse_BinaryExpression_AndAlso()
    {
        Expression expression = (Person p) => p.Age > 18 && p.Age < 65;
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("AND", result);
        Assert.Contains(".[Age] > 18", result);
        Assert.Contains(".[Age] < 65", result);
    }

    [TestMethod(DisplayName = "Parse: Binary - OrElse operator")]
    public void Parse_BinaryExpression_OrElse()
    {
        Expression expression = (Person p) => p.Age < 18 || p.Age > 65;
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("OR", result);
        Assert.Contains(".[Age] < 18", result);
        Assert.Contains(".[Age] > 65", result);
    }

    [TestMethod(DisplayName = "Parse: Binary - Null comparison uses IS")]
    public void Parse_BinaryExpression_NullComparison()
    {
        Expression expression = (PersonWithNullable p) => p.Name == null;
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("IS NULL", result);
    }

    [TestMethod(DisplayName = "Parse: Binary - NotNull comparison uses IS NOT")]
    public void Parse_BinaryExpression_NotNullComparison()
    {
        Expression expression = (PersonWithNullable p) => p.Name != null;
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("IS NOT NULL", result);
    }

    #endregion

    #region Unary Expression Tests

    [TestMethod(DisplayName = "Parse: Unary - Not operator")]
    public void Parse_UnaryExpression_Not()
    {
        Expression expression = (PersonWithBoolean p) => !p.IsActive;
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("NOT", result);
        Assert.Contains(".[IsActive]", result);
    }

    [TestMethod(DisplayName = "Parse: Unary - Negate operator")]
    public void Parse_UnaryExpression_Negate()
    {
        Expression expression = (Person p) => -p.Age;
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("(-", result);
        Assert.Contains(".[Age]", result);
    }

    #endregion

    #region Constant Expression Tests

    [TestMethod(DisplayName = "Parse: Constant - Integer")]
    public void Parse_ConstantExpression_Integer()
    {
        Expression<Func<int>> expression = () => 42;
        string result = SqlExpressionParser.Parse(expression);

        Assert.AreEqual("42", result);
    }

    [TestMethod(DisplayName = "Parse: Constant - String")]
    public void Parse_ConstantExpression_String()
    {
        Expression<Func<string>> expression = () => "Hello";
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("'Hello'", result);
    }

    [TestMethod(DisplayName = "Parse: Constant - Null")]
    public void Parse_ConstantExpression_Null()
    {
        Expression<Func<object?>> expression = () => null;
        string result = SqlExpressionParser.Parse(expression);

        Assert.AreEqual("NULL", result);
    }

    #endregion

    #region Conditional Expression Tests

    [TestMethod(DisplayName = "Parse: Conditional - Ternary operator")]
    public void Parse_ConditionalExpression_Ternary()
    {
        Expression expression = (Person p) => p.Age > 18 ? 1 : 0;
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("CASE WHEN", result);
        Assert.Contains("THEN 1", result);
        Assert.Contains("ELSE 0", result);
        Assert.Contains("END", result);
    }

    [TestMethod(DisplayName = "Parse: Conditional - Complex condition")]
    public void Parse_ConditionalExpression_Complex()
    {
        Expression expression = (Person p) => p.Age >= 18 && p.Age <= 65 ? p.Age : 0;
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("CASE WHEN", result);
        Assert.Contains("AND", result);
        Assert.Contains("THEN", result);
        Assert.Contains("ELSE 0", result);
        Assert.Contains("END", result);
    }

    #endregion

    #region Switch Expression Tests

    [TestMethod(DisplayName = "Parse: Switch - Simple switch expression")]
    public void Parse_SwitchExpression_Simple()
    {
        // Use conditional expressions (ternary operators) to simulate switch behavior
        Expression<Func<int, string>> expression = x =>
            x == 1 ? "One" :
            x == 2 ? "Two" :
            "Other";
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("CASE", result);
        Assert.Contains("WHEN", result);
        Assert.Contains("THEN", result);
        Assert.Contains("ELSE", result);
        Assert.Contains("END", result);
    }

    #endregion

    #region Math Function Tests

    [TestMethod(DisplayName = "Parse: Math.Abs")]
    public void Parse_MathFunction_Abs()
    {
        Expression expression = (Person p) => Math.Abs(p.Age);
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("ABS(", result);
        Assert.Contains(".[Age]", result);
    }

    [TestMethod(DisplayName = "Parse: Math.Ceiling")]
    public void Parse_MathFunction_Ceiling()
    {
        Expression expression = (PersonWithDecimal p) => Math.Ceiling(p.Salary);
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("CEILING(", result);
        Assert.Contains(".[Salary]", result);
    }

    [TestMethod(DisplayName = "Parse: Math.Floor")]
    public void Parse_MathFunction_Floor()
    {
        Expression expression = (PersonWithDecimal p) => Math.Floor(p.Salary);
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("FLOOR(", result);
        Assert.Contains(".[Salary]", result);
    }

    [TestMethod(DisplayName = "Parse: Math.Round")]
    public void Parse_MathFunction_Round()
    {
        Expression expression = (PersonWithDecimal p) => Math.Round(p.Salary);
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("ROUND(", result);
        Assert.Contains(".[Salary]", result);
    }

    [TestMethod(DisplayName = "Parse: Math.Pow")]
    public void Parse_MathFunction_Pow()
    {
        Expression expression = (Person p) => Math.Pow(p.Age, 2);
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("POWER(", result);
        Assert.Contains(".[Age]", result);
        Assert.Contains(",2", result);
    }

    [TestMethod(DisplayName = "Parse: Math.Sqrt")]
    public void Parse_MathFunction_Sqrt()
    {
        Expression expression = (Person p) => Math.Sqrt(p.Age);
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("SQRT(", result);
        Assert.Contains(".[Age]", result);
    }

    [TestMethod(DisplayName = "Parse: Math.Log")]
    public void Parse_MathFunction_Log()
    {
        Expression expression = (Person p) => Math.Log(p.Age);
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("LOG(", result);
        Assert.Contains(".[Age]", result);
    }

    [TestMethod(DisplayName = "Parse: Math.Log10")]
    public void Parse_MathFunction_Log10()
    {
        Expression expression = (Person p) => Math.Log10(p.Age);
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("LOG10(", result);
        Assert.Contains(".[Age]", result);
    }

    [TestMethod(DisplayName = "Parse: Math.Exp")]
    public void Parse_MathFunction_Exp()
    {
        Expression expression = (Person p) => Math.Exp(p.Age);
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("EXP(", result);
        Assert.Contains(".[Age]", result);
    }

    [TestMethod(DisplayName = "Parse: Math.Sign")]
    public void Parse_MathFunction_Sign()
    {
        Expression expression = (Person p) => Math.Sign(p.Age);
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("SIGN(", result);
        Assert.Contains(".[Age]", result);
    }

    #endregion

    #region String Function Tests

    [TestMethod(DisplayName = "Parse: string.IsNullOrEmpty")]
    public void Parse_StringFunction_IsNullOrEmpty()
    {
        Expression expression = (PersonWithNullable p) => string.IsNullOrEmpty(p.Name);
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("IS NULL OR", result);
        Assert.Contains("= ''", result);
    }

    [TestMethod(DisplayName = "Parse: string.IsNullOrWhiteSpace")]
    public void Parse_StringFunction_IsNullOrWhiteSpace()
    {
        Expression expression = (PersonWithNullable p) => string.IsNullOrWhiteSpace(p.Name);
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("IS NULL OR LTRIM(RTRIM(", result);
        Assert.Contains(")) = ''", result);
    }

    [TestMethod(DisplayName = "Parse: string.Join")]
    public void Parse_StringFunction_Join()
    {
        Expression expression = (PersonWithMultipleStrings p) => string.Concat(p.FirstName, ",", p.LastName);
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("CONCAT(", result);
        Assert.Contains(".[FirstName]", result);
        Assert.Contains(".[LastName]", result);
    }

    [TestMethod(DisplayName = "Parse: string.Concat")]
    public void Parse_StringFunction_Concat()
    {
        Expression expression = (PersonWithMultipleStrings p) => string.Concat(p.FirstName, p.LastName);
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("CONCAT(", result);
        Assert.Contains(".[FirstName]", result);
        Assert.Contains(".[LastName]", result);
    }

    [TestMethod(DisplayName = "Parse: string.Contains")]
    public void Parse_StringFunction_Contains()
    {
        Expression expression = (PersonWithNullable p) => p.Name.Contains("John");
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("LIKE '%' +", result);
        Assert.Contains("+ '%'", result);
    }

    [TestMethod(DisplayName = "Parse: string.StartsWith")]
    public void Parse_StringFunction_StartsWith()
    {
        Expression expression = (PersonWithNullable p) => p.Name.StartsWith("John");
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("LIKE", result);
        Assert.Contains("+ '%'", result);
    }

    [TestMethod(DisplayName = "Parse: string.EndsWith")]
    public void Parse_StringFunction_EndsWith()
    {
        Expression expression = (PersonWithNullable p) => p.Name.EndsWith("Smith");
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("LIKE '%' +", result);
    }

    [TestMethod(DisplayName = "Parse: string.ToUpper")]
    public void Parse_StringFunction_ToUpper()
    {
        Expression expression = (PersonWithNullable p) => p.Name.ToUpper();
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("UPPER(", result);
        Assert.Contains(".[Name]", result);
    }

    [TestMethod(DisplayName = "Parse: string.ToLower")]
    public void Parse_StringFunction_ToLower()
    {
        Expression expression = (PersonWithNullable p) => p.Name.ToLower();
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("LOWER(", result);
        Assert.Contains(".[Name]", result);
    }

    [TestMethod(DisplayName = "Parse: string.Trim")]
    public void Parse_StringFunction_Trim()
    {
        Expression expression = (PersonWithNullable p) => p.Name.Trim();
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("LTRIM(RTRIM(", result);
        Assert.Contains(".[Name]", result);
    }

    [TestMethod(DisplayName = "Parse: string.TrimStart")]
    public void Parse_StringFunction_TrimStart()
    {
        Expression expression = (PersonWithNullable p) => p.Name.TrimStart();
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("LTRIM(", result);
        Assert.Contains(".[Name]", result);
    }

    [TestMethod(DisplayName = "Parse: string.TrimEnd")]
    public void Parse_StringFunction_TrimEnd()
    {
        Expression expression = (PersonWithNullable p) => p.Name.TrimEnd();
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("RTRIM(", result);
        Assert.Contains(".[Name]", result);
    }

    [TestMethod(DisplayName = "Parse: string.Substring with two arguments")]
    public void Parse_StringFunction_Substring()
    {
        Expression expression = (PersonWithNullable p) => p.Name.Substring(0, 5);
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("SUBSTRING(", result);
        Assert.Contains(".[Name]", result);
        // SQL uses 1-based indexing, so 0 becomes 1
        Assert.Contains(",1,", result);
        Assert.Contains(",5", result);
    }

    [TestMethod(DisplayName = "Parse: string.Substring with one argument")]
    public void Parse_StringFunction_SubstringOneArg()
    {
        Expression expression = (PersonWithNullable p) => p.Name.Substring(3);
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("SUBSTRING(", result);
        Assert.Contains(".[Name]", result);
        // SQL uses 1-based indexing, so 3 becomes 4
        Assert.Contains(",4)", result);
    }

    #endregion

    #region Guid Function Tests

    [TestMethod(DisplayName = "Parse: Guid.NewGuid")]
    public void Parse_GuidFunction_NewGuid()
    {
        Expression<Func<Guid>> expression = () => Guid.NewGuid();
        string result = SqlExpressionParser.Parse(expression);

        Assert.AreEqual("NEWID()", result);
    }

    #endregion

    #region ToString Function Tests

    [TestMethod(DisplayName = "Parse: ToString without format")]
    public void Parse_ToStringFunction_WithoutFormat()
    {
        Expression expression = (Person p) => p.Age.ToString();
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("CONVERT(nvarchar(256),", result);
        Assert.Contains(".[Age]", result);
    }

    [TestMethod(DisplayName = "Parse: ToString with format")]
    public void Parse_ToStringFunction_WithFormat()
    {
        Expression expression = (Person p) => p.Age.ToString("D5");
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("FORMAT(CONVERT(nvarchar(256),", result);
        Assert.Contains(".[Age]", result);
        Assert.Contains("'D5'", result);
    }

    #endregion

    #region Parse/Convert Function Tests

    [TestMethod(DisplayName = "Parse: int.Parse")]
    public void Parse_ParseFunction_IntParse()
    {
        Expression expression = (PersonWithNullable p) => int.Parse(p.Name);
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("CONVERT(", result);
        Assert.Contains(".[Name]", result);
    }

    [TestMethod(DisplayName = "Parse: Convert method")]
    public void Parse_ConvertMethod()
    {
        Expression expression = (PersonWithNullable p) => Convert.ToInt32(p.Name);
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("CONVERT(", result);
        Assert.Contains(".[Name]", result);
    }

    #endregion

    #region Collection Function Tests

    [TestMethod(DisplayName = "Parse: Collection.Contains (instance method)")]
    public void Parse_CollectionFunction_InstanceContains()
    {
        var ages = new List<int> { 25, 30, 35 };
        Expression expression = (Person p) => ages.Contains(p.Age);
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("IN (", result);
        Assert.Contains(".[Age]", result);
    }

    [TestMethod(DisplayName = "Parse: Enumerable.Contains (static method)")]
    public void Parse_CollectionFunction_StaticContains()
    {
        var ages = new[] { 25, 30, 35 };
        Expression expression = (Person p) => Enumerable.Contains(ages, p.Age);
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("IN (", result);
        Assert.Contains(".[Age]", result);
    }

    #endregion

    #region Complex Expression Tests

    [TestMethod(DisplayName = "Parse: Complex nested conditions")]
    public void Parse_ComplexExpression_NestedConditions()
    {
        Expression expression = (Person p) => (p.Age > 18 && p.Age < 30) || (p.Age > 60 && p.Age < 70);
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("AND", result);
        Assert.Contains("OR", result);
        Assert.Contains(".[Age]", result);
    }

    [TestMethod(DisplayName = "Parse: Multiple member access")]
    public void Parse_ComplexExpression_MultipleMemberAccess()
    {
        Expression expression = (PersonWithMultipleStrings p) => new { p.FirstName, p.LastName };
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains(".[FirstName]", result);
        Assert.Contains(".[LastName]", result);
    }

    [TestMethod(DisplayName = "Parse: Chained string operations")]
    public void Parse_ComplexExpression_ChainedStringOperations()
    {
        Expression expression = (PersonWithNullable p) => p.Name.ToUpper().Trim();
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains("LTRIM(RTRIM(", result);
        Assert.Contains("UPPER(", result);
    }

    [TestMethod(DisplayName = "Parse: Combined math operations")]
    public void Parse_ComplexExpression_CombinedMathOperations()
    {
        Expression expression = (Person p) => (p.Age + 10) * 2 - 5;
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains(".[Age] + 10", result);
        Assert.Contains("* 2", result);
        Assert.Contains("- 5", result);
    }

    #endregion

    #region Edge Case Tests

    [TestMethod(DisplayName = "Parse: Static field access")]
    public void Parse_EdgeCase_StaticFieldAccess()
    {
        Expression<Func<string>> expression = () => string.Empty;
        string result = SqlExpressionParser.Parse(expression);

        Assert.AreEqual("''", result);
    }

    [TestMethod(DisplayName = "Parse: Comparison with constant from closure")]
    public void Parse_EdgeCase_ClosureConstant()
    {
        int compareAge = 30;
        Expression expression = (Person p) => p.Age > compareAge;
        string result = SqlExpressionParser.Parse(expression);

        Assert.Contains(".[Age] > 30", result);
    }

    [TestMethod(DisplayName = "Parse: New expression with multiple properties")]
    public void Parse_EdgeCase_NewExpressionMultipleProperties()
    {
        Expression expression = (PersonWithMultipleStrings p) => new { Name = p.FirstName, Surname = p.LastName };
        string result = SqlExpressionParser.Parse(expression, assignmentTreatment: SqlExpressionParser.AssignmentTreatment.AsAlias);

        Assert.Contains(".[FirstName]", result);
        Assert.Contains("AS [Name]", result);
        Assert.Contains(".[LastName]", result);
        Assert.Contains("AS [Surname]", result);
    }

    #endregion

    #region Exception/Error Tests

    [TestMethod(DisplayName = "Parse: Unsupported operator throws exception")]
    public void Parse_Exception_UnsupportedOperator()
    {
        // BitAnd (&) is not supported
        Expression expression = (Person p) => p.Age & 1;
        
        Assert.ThrowsExactly<NotSupportedException>(() => SqlExpressionParser.Parse(expression));
    }

    #endregion

    #region Test Model Classes

    [SqlTable("Person")]
    private class Person
    {
        [SqlTableColumn("Age")]
        public int Age { get; internal set; }
    }

    [SqlTable("PersonWithNullable")]
    private class PersonWithNullable
    {
        [SqlTableColumn("Name")]
        public string Name { get; set; } = string.Empty;
    }

    [SqlTable("PersonWithBoolean")]
    private class PersonWithBoolean
    {
        [SqlTableColumn("IsActive")]
        public bool IsActive { get; set; }
    }

    [SqlTable("PersonWithDecimal")]
    private class PersonWithDecimal
    {
        [SqlTableColumn("Salary")]
        public decimal Salary { get; set; }
    }

    [SqlTable("PersonWithMultipleStrings")]
    private class PersonWithMultipleStrings
    {
        [SqlTableColumn("FirstName")]
        public string FirstName { get; set; } = string.Empty;

        [SqlTableColumn("LastName")]
        public string LastName { get; set; } = string.Empty;
    }

    #endregion
}
