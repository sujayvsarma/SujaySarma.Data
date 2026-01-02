using SujaySarma.Data.Core.ReflectionUtilities;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;

namespace SujaySarma.Data.SqlServer.Builders;

/// <summary>
/// Our methods in this namespace allow conditions, filters, selectors, etc to be passed in as 
/// Expressions (i.e., Lambda expressions) such as (e => (e.Id == id)) etc. This class helps 
/// parse these expressions, map them to the business entities and entity members (including 
/// variables and state information from the caller/application that may be passed in to the 
/// expression and convert it to equivalent SQL clauses/statements.
/// </summary>
internal sealed partial class SqlExpressionParser : ExpressionVisitor
{
    /// <summary>
    /// The main entry point parser routine.
    /// </summary>
    /// <returns>SQL Expression.</returns>
    private string Parse()
    {
        Visit(_originalExpression);

        StringBuilder builder = new StringBuilder();
        while (_expressionBuffer.Count > 0)
        {
            builder.Append(_expressionBuffer.Pop());
            builder.Append(' ');
        }

        return builder.ToString().Trim();
    }

    /// <summary>
    /// Create and initialise the parser.
    /// </summary>
    /// <param name="expression">The expression to parse.</param>
    /// <param name="assignmentTreatment">How assignments ( a = b ) are treated while parsing assignment expressions.</param>
    public static string Parse(Expression expression, AssignmentTreatment assignmentTreatment = AssignmentTreatment.ActualAssignment)
    {
        SqlExpressionParser parser = new SqlExpressionParser(expression, assignmentTreatment);
        return parser.Parse();
    }

    /// <summary>
    /// Validates that the provided <paramref name="expression"/> is a valid condition expression.
    /// </summary>
    /// <param name="expression">Expression to validate.</param>
    /// <param name="errorMessage">The error message to return when the condition is found invalid.</param>
    /// <returns>True if <paramref name="expression"/> is a valid condition.</returns>
    public static bool IsValidCondition(Expression expression, [NotNullWhen(false)] out string? errorMessage)
    {
        errorMessage = null;

        if (expression is not LambdaExpression lambda)
        {
            errorMessage = "Expression is not a Lambda expression.";
            return false;
        }

        Expression lambdaBody = lambda.Body;
        if (lambdaBody.Type != typeof(bool))
        {
            errorMessage = "Lambda body does not evaluate to a boolean value.";
            return false;
        }

        // Unwrap any Convert nodes (e.g., boxing)
        while (lambdaBody is UnaryExpression { NodeType: ExpressionType.Convert } unary)
        {
            lambdaBody = unary.Operand;
        }

        if (lambdaBody is ConstantExpression)
        {
            errorMessage = "Lambda body is a constant expression.";
            return false;
        }

        bool isValidExpressionType = lambdaBody switch
        {
            BinaryExpression binary => 
                       (binary.NodeType is
                                            ExpressionType.Equal or
                                            ExpressionType.NotEqual or
                                            ExpressionType.GreaterThan or
                                            ExpressionType.GreaterThanOrEqual or
                                            ExpressionType.LessThan or
                                            ExpressionType.LessThanOrEqual or
                                            ExpressionType.AndAlso or
                                            ExpressionType.OrElse or
                                            ExpressionType.Coalesce),

            MethodCallExpression method => 
                        Recognise(method) != RecognisedMethodType.Unknown,

            // Support direct boolean member access (e.g., c.IsActive)
            MemberExpression member => 
                        member.Type == typeof(bool),

            // Support unary NOT expressions (e.g., !c.IsDeleted)
            UnaryExpression { NodeType: ExpressionType.Not } => 
                        true,

            _ => false
        };

        if ((lambdaBody is MemberExpression me) && me.Type.IsNullableEquivalentOf(typeof(bool)))
        {
            errorMessage = "Nullable boolean members must be compared explicitly (e.g., 'n.IsActive == true' instead of 'n.IsActive').";
            return false;
        }

        errorMessage = isValidExpressionType ? null : "Lambda body is not a valid condition expression.";
        return isValidExpressionType;
    }


    /// <summary>
    /// Create and initialise the parser.
    /// </summary>
    /// <param name="expression">The expression to parse.</param>
    /// <param name="assignmentTreatment">How assignments ( a = b ) are treated while parsing assignment expressions.</param>
    private SqlExpressionParser(Expression expression, AssignmentTreatment assignmentTreatment)
    {
        _expressionBuffer = new Stack<string>();
        _treatmentOfAssignmentsInExpression = assignmentTreatment;
        _originalExpression = expression;
    }

    private readonly Stack<string> _expressionBuffer;
    private readonly AssignmentTreatment _treatmentOfAssignmentsInExpression;
    private readonly Expression _originalExpression;
    private bool _serialiseEnumsAsStrings = true;
    private Type? _currentEnum = null;

    /// <summary>
    /// How assignments ( a = b ) are treated while parsing assignment expressions.
    /// </summary>
    public enum AssignmentTreatment
    {
        /// <summary>
        /// Resolve it as an actual assignment.
        /// Ex: a = b, causes value of 'b' to be assigned to 'a'.
        /// </summary>
        ActualAssignment = 0,

        /// <summary>
        /// Resolve it as an alias assignment.
        /// Ex: a = b, causes the string "a AS b" to be generated.
        /// </summary>
        AsAlias = 1,

        /// <summary>
        /// Resolve it, but keep it as is.
        /// Ex: a = b, is retained as "a = b".
        /// </summary>
        AsIs = 2
    }
}
