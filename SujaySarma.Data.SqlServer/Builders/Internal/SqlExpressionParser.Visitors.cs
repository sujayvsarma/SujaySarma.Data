using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.SqlServer.Attributes;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SujaySarma.Data.SqlServer.Builders;

// Visitor implementations for the SqlExpressionParser.
internal sealed partial class SqlExpressionParser
{

    /// <summary>
    /// Resolve a C# switch expression (x switch { a => foo; }) into a SQL expression (CASE WHEN ELSE).
    /// </summary>
    protected override Expression VisitSwitch(SwitchExpression node)
    {
        List<string> caseParts = new List<string>();

        // Visit the switch value (the expression being switched on)
        Visit(node.SwitchValue);
        string switchValue = _expressionBuffer.Pop();

        // Process each case
        foreach (SwitchCase switchCase in node.Cases)
        {
            // Each case can have multiple test values (e.g., case 1 or 2:)
            foreach (Expression testValue in switchCase.TestValues)
            {
                Visit(testValue);
                string test = _expressionBuffer.Pop();

                Visit(switchCase.Body);
                string body = _expressionBuffer.Pop();

                caseParts.Add($"WHEN ({switchValue} = {test}) THEN {body}");
            }
        }

        // Handle the default case if present
        string? defaultCase = null;
        if (node.DefaultBody is not null)
        {
            Visit(node.DefaultBody);
            defaultCase = _expressionBuffer.Pop();
        }

        // Build the complete CASE expression
        string caseExpression = $"CASE {string.Join(' ', caseParts)}";
        if (!string.IsNullOrWhiteSpace(defaultCase))
        {
            caseExpression += $" ELSE {defaultCase}";
        }
        caseExpression += " END";

        _expressionBuffer.Push(caseExpression);

        return node;
    }

    /// <summary>
    /// Resolve a conditional expression ((a &gt; b) ? c : d) to a SQL expression (CASE WHEN ELSE).
    /// </summary>
    protected override Expression VisitConditional(ConditionalExpression node)
    {
        Visit(node.Test);
        string test = _expressionBuffer.Pop();

        Visit(node.IfTrue);
        string ifTrue = _expressionBuffer.Pop();

        Visit(node.IfFalse);
        string ifFalse = _expressionBuffer.Pop();

        _expressionBuffer.Push($"CASE WHEN ({test}) THEN {ifTrue} ELSE {ifFalse} END");

        return node;
    }

    /// <summary>
    /// Resolve a binary expression (eg: A + B, X == Y, etc) into its SQL expression.
    /// </summary>
    protected override Expression VisitBinary(BinaryExpression node)
    {
        // Special handling for null-coalescing operator
        if (node.NodeType == ExpressionType.Coalesce)
        {
            Visit(node.Left);
            string left_nc = _expressionBuffer.Pop();
            Visit(node.Right);
            string right_nc = _expressionBuffer.Pop();

            // Translate to SQL: a ?? b  →  ISNULL(a, b) or COALESCE(a, b)
            _expressionBuffer.Push($"ISNULL({left_nc}, {right_nc})");
            return node;
        }

        string operatorName = ConvertExpressionOperatorToSQL(node);
        Visit(node.Left);
        string left = _expressionBuffer.Pop();
        Visit(node.Right);
        string right = _expressionBuffer.Pop();

        if (right is "NULL")
        {
            operatorName = ((operatorName is "=") ? "IS" : "IS NOT");
        }

        _expressionBuffer.Push($"({left} {operatorName} {right})");
        return node;
    }

    /// <summary>
    /// Resolves a unary expression (NOT x, -ABC, etc) into its SQL expression.
    /// </summary>
    protected override Expression VisitUnary(UnaryExpression node)
    {
        Visit(node.Operand);
        string result = _expressionBuffer.Pop();
        switch (node.NodeType)
        {
            case ExpressionType.Negate:
            case ExpressionType.NegateChecked:
                result = $"(-{result})";
                break;

            case ExpressionType.Not:
                result = $"NOT {result}";
                break;
        }

        _expressionBuffer.Push(result);
        return node;
    }

    /// <summary>
    /// Get the value of a constant
    /// </summary>
    protected override Expression VisitConstant(ConstantExpression node)
    {
        _expressionBuffer.Push(SerializeToString(node.Value));
        return node;
    }

    /// <summary>
    /// Get value of a new object init within an expression, usually of anonymous types.
    /// Eg: x =&gt; new { x.Id, x.Name } --&gt; "t.[Id], t.[Name]..."
    /// </summary>
    protected override Expression VisitNew(NewExpression node)
    {
        ReadOnlyCollection<MemberInfo>? members = node.Members;

        // Flatten and map the member expressions (assignments, etc) into a one-dimensional enumeration.
        IEnumerable<KeyValuePair<MemberInfo, Expression>>? memberExpressionMapping = members?.Zip<MemberInfo, Expression, KeyValuePair<MemberInfo, Expression>>(
                    node.Arguments,
                        (m, a) => new KeyValuePair<MemberInfo, Expression>(m, a));

        if (memberExpressionMapping is not null)
        {
            List<string> items = new List<string>();
            foreach (KeyValuePair<MemberInfo, Expression> meMap in memberExpressionMapping)
            {
                // Resolve the expression
                Visit(meMap.Value);
                string value = _expressionBuffer.Pop();

                if (!string.IsNullOrWhiteSpace(value))
                {
                    switch (_treatmentOfAssignmentsInExpression)
                    {
                        case AssignmentTreatment.ActualAssignment:
                            items.Add(value);
                            break;

                        case AssignmentTreatment.AsAlias:
                            // Check if the value already ends with the member name (implicit aliasing case)
                            // For example: [T1].[Id] shouldn't become [T1].[Id] AS [Id]
                            string memberName = meMap.Key.Name;
                            if (value.EndsWith($".[{memberName}]", StringComparison.OrdinalIgnoreCase))
                            {
                                // Implicit alias - the column name already matches, no AS needed
                                items.Add(value);
                            }
                            else
                            {
                                // Explicit alias needed (user renamed or transformed the column)
                                items.Add($"{value} AS [{memberName}]");
                            }

                            break;

                        case AssignmentTreatment.AsIs:
                            items.Add($"[{meMap.Key.Name}] = {value}");
                            break;
                    }
                }
            }

            if (items.Count > 0)
            {
                _expressionBuffer.Push(string.Join(',', items));
            }
        }

        return node;
    }

    /// <summary>
    /// Handle parameter expressions. For lambda parameters that represent entities,
    /// we don't push anything - member access will be handled by VisitMember.
    /// For standalone parameters (like 'x' in x => x == 1), we push the parameter name.
    /// </summary>
    protected override Expression VisitParameter(ParameterExpression node)
    {
        // Check if this parameter represents an entity type (has table metadata)
        // If it does, we don't push anything - member access like p.Age will be handled by VisitMember
        try
        {
            // Try to get metadata - if this succeeds, it's an entity parameter
            var metadata = node.Type.RetrievePersistenceContainerInfoOrThrowException();

            // This is an entity parameter - don't push anything
            // The actual column references will come from MemberExpression nodes (e.g., o.CustomerId)
            return node;
        }
        catch
        {
            // Not an entity type - this is a standalone parameter (like 'x' in x => x == 1)
            // Push the parameter name for use in the SQL
            _expressionBuffer.Push(node.Name ?? node.ToString());
            return node;
        }
    }

    /// <summary>
    /// Get the right SQL operator for selected method calls. Other methods are processed as normally
    /// (and often erroneously) by the system.
    /// </summary>
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        // CRITICAL CHECK: Is this method being called on a captured variable (client-side)
        // or on an entity property (SQL-side)?
        if (node.Object is not null && IsClientSideExpression(node.Object))
        {
            // This is a method call on a captured variable - evaluate it client-side
            // Example: foo.ToUpper() where foo is a captured string variable
            try
            {
                object? result = EvaluateExpression(node);
                _expressionBuffer.Push(SerializeToString(result));
                return node;
            }
            catch
            {
                // If evaluation fails, fall through to SQL translation
            }
        }

        RecognisedMethodType methodType = Recognise(node);
        return (methodType) switch
        {
            RecognisedMethodType.CollectionContains => HandleCollectionFunctions(node, methodType),

            // --------- PARSE/CONVERT FUNCTIONS ---------
            RecognisedMethodType.ConvertMethod or RecognisedMethodType.ConvertMethod or RecognisedMethodType.Parse 
                    => HandleParseAndConvertFunctions(node, methodType),

            RecognisedMethodType.GuidNewGuid 
                    => HandleGuidFunctions(node, methodType),

            // --------- MATH FUNCTIONS ---------
            RecognisedMethodType.MathAbs or RecognisedMethodType.MathCeiling or RecognisedMethodType.MathExp
                or RecognisedMethodType.MathFloor or RecognisedMethodType.MathLog or RecognisedMethodType.MathLog10
                    or RecognisedMethodType.MathPower or RecognisedMethodType.MathRound or RecognisedMethodType.MathSign
                        or RecognisedMethodType.MathSqrt

                        => HandleMathFunctions(node, methodType),

            // --------- STRING FUNCTIONS ---------
            RecognisedMethodType.StringConcat or RecognisedMethodType.StringContains or RecognisedMethodType.StringEndsWith
                or RecognisedMethodType.StringIsNullOrEmpty or RecognisedMethodType.StringIsNullOrWhiteSpace or RecognisedMethodType.StringJoin
                    or RecognisedMethodType.StringStartsWith or RecognisedMethodType.StringSubstring or RecognisedMethodType.StringToLower
                        or RecognisedMethodType.StringToUpper or RecognisedMethodType.StringTrim or RecognisedMethodType.StringTrimEnd
                            or RecognisedMethodType.StringTrimStart

                        => HandleStringFunctions(node, methodType),

            // --------- TOSTRING FUNCTIONS ---------
            RecognisedMethodType.ToStringWithFormat or RecognisedMethodType.ToStringWithoutFormat
                        => HandleToStringFunctions(node, methodType),

            _ => base.VisitMethodCall(node)
        };
    }


    /// <summary>
    /// Serialise the given value to a string. The string returned will be T-SQL compatible.
    /// </summary>
    /// <param name="value">Value to serialise.</param>
    /// <returns>String compatible with T-SQL.</returns>
    private string SerializeToString(object? value)
    {
        string str = string.Empty;
        if ((_currentEnum is null) || (value is null))
        {
            // This should return "NULL" as a string.
            str = value.GetSQLStringValue();
        }
        else
        {
            if (Enum.TryParse(_currentEnum, value.ToString(), out object? e))
            {
                str = e.GetSQLStringValue();
            }
        }

        return str;
    }

    /// <summary>
    /// Determines if an expression is purely client-side (doesn't reference entity properties).
    /// </summary>
    private bool IsClientSideExpression(Expression expression)
    {
        // Unwrap Convert nodes
        while (expression is UnaryExpression { NodeType: ExpressionType.Convert } unary)
        {
            expression = unary.Operand;
        }

        switch (expression)
        {
            case ConstantExpression:
                // Direct constant
                return true;

            case MemberExpression member when member.Expression is ConstantExpression:
                // Captured variable (closure)
                return true;

            case MemberExpression member:
                // Check if this member has SqlTableColumn attribute
                if (member.Member.TryGetAttribute<SqlTableColumn>(out _))
                {
                    // This is an entity property - NOT client-side
                    return false;
                }

                // Check the parent expression recursively
                return member.Expression is not null && IsClientSideExpression(member.Expression);

            case ParameterExpression param:
                // Check if parameter is an entity type
                try
                {
                    param.Type.ValidateForOrmWithException();

                    // Has table metadata - this is SQL-side
                    return false;
                }
                catch
                {
                    // Not an entity - client-side
                    return true;
                }

            default:
                return false;
        }
    }

    /// <summary>
    /// Evaluates an expression client-side and returns the result.
    /// </summary>
    private static object? EvaluateExpression(Expression expression)
    {
        // Compile and execute the expression
        var lambda = Expression.Lambda<Func<object>>(
            Expression.Convert(expression, typeof(object))
        );
        return lambda.Compile()();
    }

    /// <summary>
    /// Get the SQL operator for the type of node.
    /// </summary>
    /// <param name="node">Node.</param>
    /// <returns>SQL operator string.</returns>
    private static string ConvertExpressionOperatorToSQL(Expression node)
    {
        switch (node.NodeType)
        {
            case ExpressionType.AndAlso:
                return "AND";

            case ExpressionType.Equal:
                return "=";

            case ExpressionType.GreaterThan:
                return ">";

            case ExpressionType.GreaterThanOrEqual:
                return ">=";

            case ExpressionType.LessThan:
                return "<";

            case ExpressionType.LessThanOrEqual:
                return "<=";

            case ExpressionType.Not:
                return "NOT";

            case ExpressionType.NotEqual:
                return "<>";

            case ExpressionType.OrElse:
                return "OR";

            case ExpressionType.Add:
                return "+";

            case ExpressionType.Divide:
                return "/";

            case ExpressionType.Multiply:
                return "*";

            case ExpressionType.Subtract:
                return "-";

            default:
                throw new NotSupportedException($"Operator {node.NodeType} is not supported (yet).");
        }
    }
}
