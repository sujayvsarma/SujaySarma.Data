using SujaySarma.Data.Core.ReflectionUtilities;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SujaySarma.Data.SqlServer.Builders;

// Code that helps recognise method references in SQL expressions.
// This helps convert some common .NET methods (when used) to SQL equivalents.
internal sealed partial class SqlExpressionParser
{

    #region Function handlers

    /// <summary>
    /// Handle translation of detected Math and numeric functions to T-SQL. The translation is directly pushed onto the _expressionBuffer.
    /// </summary>
    /// <param name="methodExpression">The original <see cref="MethodCallExpression"/>.</param>
    /// <param name="methodType">The type of method as recognised by <see cref="Recognise(MethodCallExpression)"/>.</param>
    /// <returns>The same expression that returned, or a visitor</returns>
    private Expression HandleMathFunctions(MethodCallExpression methodExpression, RecognisedMethodType methodType)
    {
        List<string> arguments = new List<string>();
        foreach (Expression argExpression in methodExpression.Arguments)
        {
            Visit(argExpression);
            arguments.Add(_expressionBuffer.Pop());
        }

        string sqlFunctionName = (methodType) switch
        {
            RecognisedMethodType.MathAbs => "ABS",
            RecognisedMethodType.MathCeiling => "CEILING",
            RecognisedMethodType.MathFloor => "FLOOR",
            RecognisedMethodType.MathRound => "ROUND",
            RecognisedMethodType.MathPower => "POWER",
            RecognisedMethodType.MathSqrt => "SQRT",
            RecognisedMethodType.MathLog => "LOG",
            RecognisedMethodType.MathLog10 => "LOG10",
            RecognisedMethodType.MathExp => "EXP",
            RecognisedMethodType.MathSign => "SIGN",

            _ => throw new InvalidOperationException("Unknown function type passed into HandleMath()")
        };

        StringBuilder func = new StringBuilder();
        func.Append(sqlFunctionName).Append('(').AppendJoin(',', arguments).Append(')');

        _expressionBuffer.Push(func.ToString());
        return methodExpression;
    }

    /// <summary>
    /// Handle translation of detected string functions to T-SQL. The translation is directly pushed onto the _expressionBuffer.
    /// </summary>
    /// <param name="methodExpression">The original <see cref="MethodCallExpression"/>.</param>
    /// <param name="methodType">The type of method as recognised by <see cref="Recognise(MethodCallExpression)"/>.</param>
    /// <returns>The same expression that returned, or a visitor</returns>
    private Expression HandleStringFunctions(MethodCallExpression methodExpression, RecognisedMethodType methodType)
    {
        // For instance methods, process the object first (the string being operated on)
        string? objectOperand = null;
        if ((!methodExpression.Method.IsStatic) && (methodExpression.Object is not null))
        {
            Visit(methodExpression.Object);
            objectOperand = _expressionBuffer.Pop();
        }

        // Process method arguments
        List<string> arguments = new List<string>();
        foreach (Expression argExpression in methodExpression.Arguments)
        {
            Visit(argExpression);
            arguments.Add(_expressionBuffer.Pop());
        }

        StringBuilder func = new StringBuilder();
        switch (methodType)
        {
            case RecognisedMethodType.StringIsNullOrEmpty:
                func.Append(arguments[0]).Append(" IS NULL OR ").Append(arguments[0]).Append(" = ''");
                break;

            case RecognisedMethodType.StringIsNullOrWhiteSpace:
                func.Append(arguments[0]).Append(" IS NULL OR LTRIM(RTRIM(").Append(arguments[0]).Append(")) = ''");
                break;

            case RecognisedMethodType.StringJoin:
                /*
                 *      string.Join(',', FirstName, MiddleName, LastName)
                 *                  [0]  [1]         [2]         [3]...
                 *      
                 *      CONCAT(FirstName, ',', MiddleName, ',', LastName)
                 *             [1]        [0]  [2]         [0]  [3]...
                 */

                string delimiter = arguments[0];
                func.Append("CONCAT(").Append(arguments[1]);
                for (int i = 2; i < arguments.Count; i++)
                {
                    func.Append(delimiter).Append(arguments[i]);
                }
                func.Append(')');
                break;

            case RecognisedMethodType.StringConcat:
                func.Append("CONCAT(").AppendJoin(null, arguments).Append(')');
                break;

            case RecognisedMethodType.StringContains:
                {
                    // string s; s.Contains(abcd); --> s LIKE '%abcd%'
                    func.Append(objectOperand).Append(" LIKE '%' + ").Append(arguments[0]).Append(" + '%'");
                }
                break;

            case RecognisedMethodType.StringStartsWith:
                {
                    // string s; s.StartsWith(abcd); --> s LIKE 'abcd%'
                    func.Append(objectOperand).Append(" LIKE ").Append(arguments[0]).Append(" + '%'");
                }
                break;

            case RecognisedMethodType.StringEndsWith:
                {
                    // string s; s.EndsWith(abcd); --> s LIKE '%abcd'
                    func.Append(objectOperand).Append(" LIKE '%' + ").Append(arguments[0]);
                }
                break;

            case RecognisedMethodType.StringToUpper:
                func.Append("UPPER(").Append(objectOperand).Append(')');
                break;

            case RecognisedMethodType.StringToLower:
                func.Append("LOWER(").Append(objectOperand).Append(')');
                break;

            case RecognisedMethodType.StringTrim:
                func.Append("LTRIM(RTRIM(").Append(objectOperand).Append("))");
                break;

            case RecognisedMethodType.StringTrimStart:
                func.Append("LTRIM(").Append(objectOperand).Append(')');
                break;

            case RecognisedMethodType.StringTrimEnd:
                func.Append("RTRIM(").Append(objectOperand).Append(')');
                break;

            case RecognisedMethodType.StringSubstring:
                {
                    // string s; s.Substring(0, 1); --> SUBSTRING(s, 1, 1) [sql uses 1-based indexes!]
                    string adjustedStart = (int.TryParse(arguments[0], out int start) ? (start + 1).ToString() : $"({arguments[0]} + 1)");

                    func.Append("SUBSTRING(").Append(objectOperand).Append(',').Append(adjustedStart);
                    if (arguments.Count == 2)
                    {
                        func.Append(',').Append(arguments[1]);
                    }
                    func.Append(')');
                }
                break;
        }

        _expressionBuffer.Push(func.ToString());
        return methodExpression;
    }

    /// <summary>
    /// Handle translation of detected Parse() and TryParse() functions to T-SQL. The translation is directly pushed onto the _expressionBuffer.
    /// </summary>
    /// <param name="methodExpression">The original <see cref="MethodCallExpression"/>.</param>
    /// <param name="methodType">The type of method as recognised by <see cref="Recognise(MethodCallExpression)"/>.</param>
    /// <returns>The same expression that returned, or a visitor</returns>
    private Expression HandleParseAndConvertFunctions(MethodCallExpression methodExpression, RecognisedMethodType methodType)
    {
        // type.Parse("") --> CONVERT(type, "")
        if (((methodType == RecognisedMethodType.Parse) || (methodType == RecognisedMethodType.ConvertMethod)) && (methodExpression.Arguments.Count == 1))
        {
            Visit(methodExpression.Arguments[0]);
            string source = _expressionBuffer.Pop();

            _expressionBuffer.Push($"CONVERT({methodExpression.Method.DeclaringType!.GetSqlTypeForClrType()}, {source})");
        }

        return methodExpression;
    }

    /// <summary>
    /// Handle translation of detected Guid functions functions to T-SQL. The translation is directly pushed onto the _expressionBuffer.
    /// </summary>
    /// <param name="methodExpression">The original <see cref="MethodCallExpression"/>.</param>
    /// <param name="methodType">The type of method as recognised by <see cref="Recognise(MethodCallExpression)"/>.</param>
    /// <returns>The same expression that returned, or a visitor</returns>
#pragma warning disable IDE0060
    // We are retaining the MethodCallExpression parameter to maintain uniformity with all other HandleXXXFunctions API.
    // It could be used later on if more Guid methods are added -- the API definition does not need to change then.
    private Expression HandleGuidFunctions(MethodCallExpression methodExpression, RecognisedMethodType methodType)
    {
        if (methodType == RecognisedMethodType.GuidNewGuid)
        {
            _expressionBuffer.Push("NEWID()");
        }

        return methodExpression;
    }

#pragma warning restore IDE0068

    /// <summary>
    /// Handle translation of detected ToString() functions functions to T-SQL. The translation is directly pushed onto the _expressionBuffer.
    /// </summary>
    /// <param name="methodExpression">The original <see cref="MethodCallExpression"/>.</param>
    /// <param name="methodType">The type of method as recognised by <see cref="Recognise(MethodCallExpression)"/>.</param>
    /// <returns>The same expression that returned, or a visitor</returns>
    private Expression HandleToStringFunctions(MethodCallExpression methodExpression, RecognisedMethodType methodType)
    {
        Visit(methodExpression.Object);
        string source = _expressionBuffer.Pop();

        switch (methodType)
        {
            case RecognisedMethodType.ToStringWithFormat:
                Visit(methodExpression.Arguments[0]);
                string format = _expressionBuffer.Pop();
                _expressionBuffer.Push($"FORMAT(CONVERT(nvarchar(256), {source}), {format.QuoteStringValueIfRequired(true)})");
                break;

            case RecognisedMethodType.ToStringWithoutFormat:
                _expressionBuffer.Push($"CONVERT(nvarchar(256), {source})");
                break;
        }

        return methodExpression;
    }

    /// <summary>
    /// Handle translation of detected collection functions to T-SQL. The translation is directly pushed onto the _expressionBuffer.
    /// </summary>
    /// <param name="methodExpression">The original <see cref="MethodCallExpression"/>.</param>
    /// <param name="methodType">The type of method as recognised by <see cref="Recognise(MethodCallExpression)"/>.</param>
    /// <returns>The same expression that returned, or a visitor</returns>
    private Expression HandleCollectionFunctions(MethodCallExpression methodExpression, RecognisedMethodType methodType)
    {
        string collectionOfValues, thingBeingChecked;

        if (methodType == RecognisedMethodType.CollectionContains)
        {
            // Maybe instance: coll.Contains(item) -> [1, 2, 3].Contains(3) -> (3 IN (1, 2, 3))
            if ((!methodExpression.Method.IsStatic) && (methodExpression.Object is not null))
            {
                if (methodExpression.Arguments.Count != 1)
                {
                    return base.VisitMethodCall(methodExpression);
                }

                Visit(methodExpression.Object);
                collectionOfValues = _expressionBuffer.Pop();

                Visit(methodExpression.Arguments[0]);
                thingBeingChecked = _expressionBuffer.Pop();
            }
            // Static: Enumerable.Contains(coll, item)
            else
            {
                if (methodExpression.Arguments.Count == 2)
                {
                    Visit(methodExpression.Arguments[0]);
                    collectionOfValues = _expressionBuffer.Pop();

                    Visit(methodExpression.Arguments[1]);
                    thingBeingChecked = _expressionBuffer.Pop();
                }
                else
                {
                    return base.VisitMethodCall(methodExpression);
                }
            }

            _expressionBuffer.Push($"{thingBeingChecked} IN ({collectionOfValues})");
        }

        return methodExpression;
    }
    #endregion

    /// <summary>
    /// Attempt to recognise what type of method we have in the MCE. If this needs conversion to T-SQL, 
    /// then we return a <see cref="RecognisedMethodType"/>. An <see cref="RecognisedMethodType.Unknown"/> means 
    /// we do not know how to convert it to T-SQL.
    /// </summary>
    /// <param name="methodExpression">The original <see cref="MethodCallExpression"/> from our <see cref="VisitMethodCall"/> visitor.</param>
    /// <returns>A <see cref="RecognisedMethodType"/>. An <see cref="RecognisedMethodType.Unknown"/> means 
    /// we do not know how to convert it to T-SQL.</returns>
    private static RecognisedMethodType Recognise(MethodCallExpression methodExpression)
    {
        if (methodExpression.Method.DeclaringType is null)
            return RecognisedMethodType.Unknown;

        bool isStaticMethod = methodExpression.Method.IsStatic;
        Type declaringType = methodExpression.Method.DeclaringType!;
        string methodName = methodExpression.Method.Name;

        // Check for IEnumerable/Enumerable's Contains method:
        if ((methodName == "Contains") && (!Type.Equals(declaringType, typeof(string))))
        {
            if (((!isStaticMethod) && (methodExpression.Object is not null) && methodExpression.Object.Type.IsEnumerableType())
                || (isStaticMethod && (declaringType.FullName == "System.Linq.Enumerable" || declaringType.IsEnumerableType())))
            {
                return RecognisedMethodType.CollectionContains;
            }
        }

        if (isStaticMethod)
        {
            if (methodName == "Parse")
                return RecognisedMethodType.Parse;

            // TryParse is explicitly NOT supported because it features an OUT variable 
            // and handling that in the context of expression-type atomic visitors is .... messy!

            if (declaringType.IsNumericType())
                return (methodName) switch
                {
                    "Abs" => RecognisedMethodType.MathAbs,
                    "Ceiling" => RecognisedMethodType.MathCeiling,
                    "Floor" => RecognisedMethodType.MathFloor,
                    "Round" or "Truncate" => RecognisedMethodType.MathRound,
                    "Pow" => RecognisedMethodType.MathPower,
                    "Sqrt" => RecognisedMethodType.MathSqrt,
                    "Log" => RecognisedMethodType.MathLog,
                    "Log10" => RecognisedMethodType.MathLog10,
                    "Exp" => RecognisedMethodType.MathExp,
                    "Sign" => RecognisedMethodType.MathSign,

                    _ => RecognisedMethodType.Unknown
                };

            if (Type.Equals(declaringType, typeof(Convert)))
                return RecognisedMethodType.ConvertMethod;

            if (Type.Equals(declaringType, typeof(string)))
                return (methodName) switch
                {
                    "IsNullOrEmpty" => RecognisedMethodType.StringIsNullOrEmpty,
                    "IsNullOrWhiteSpace" => RecognisedMethodType.StringIsNullOrWhiteSpace,
                    "Join" => RecognisedMethodType.StringJoin,
                    "Concat" => RecognisedMethodType.StringConcat,

                    _ => RecognisedMethodType.Unknown
                };

            if (Type.Equals(declaringType, typeof(Guid)) && (methodName == "NewGuid"))
                return RecognisedMethodType.GuidNewGuid;

        }
        else
        {
            if (methodName == "ToString")
            {
                if ((methodExpression.Arguments.Count >= 1) && Type.Equals(methodExpression.Arguments[0].Type, typeof(string)))
                {
                    // Has a 1st argument of type string. This would be a ToString with format strings.
                    return RecognisedMethodType.ToStringWithFormat;
                }

                // A normal ToString() with no formatting etc.
                return RecognisedMethodType.ToStringWithoutFormat;
            }

            if (Type.Equals(declaringType, typeof(string)))
                return (methodName) switch
                {
                    "Contains" => RecognisedMethodType.StringContains,
                    "StartsWith" => RecognisedMethodType.StringStartsWith,
                    "EndsWith" => RecognisedMethodType.StringEndsWith,
                    "ToUpper" => RecognisedMethodType.StringToUpper,
                    "ToLower" => RecognisedMethodType.StringToLower,
                    "Trim" => RecognisedMethodType.StringTrim,
                    "TrimStart" => RecognisedMethodType.StringTrimStart,
                    "TrimEnd" => RecognisedMethodType.StringTrimEnd,
                    "Substring" => RecognisedMethodType.StringSubstring,

                    _ => RecognisedMethodType.Unknown
                };

        }

        return RecognisedMethodType.Unknown;
    }

    /// <summary>
    /// The methods that we recognise and know what to do with when
    /// handling the VisitMethodCall visitor.
    /// </summary>
    private enum RecognisedMethodType
    {
        /// <summary>
        /// Not known.
        /// </summary>
        Unknown,

        #region Math functions -- also applies to numeric types

        /// <summary>
        /// ABS()
        /// </summary>
        MathAbs,

        /// <summary>
        /// CEILING()
        /// </summary>
        MathCeiling,

        /// <summary>
        /// FLOOR()
        /// </summary>
        MathFloor,

        /// <summary>
        /// ROUND()
        /// </summary>
        MathRound,

        /// <summary>
        /// POW()
        /// </summary>
        MathPower,

        /// <summary>
        /// SQRT()
        /// </summary>
        MathSqrt,

        /// <summary>
        /// LOG()
        /// </summary>
        MathLog,

        /// <summary>
        /// LOG10()
        /// </summary>
        MathLog10,

        /// <summary>
        /// EXP() -- exponent
        /// </summary>
        MathExp,

        /// <summary>
        /// SIGN() -- sign of value
        /// </summary>
        MathSign,

        #endregion

        #region String functions

        /// <summary>
        /// string.IsNullOrEmpty
        /// </summary>
        StringIsNullOrEmpty,

        /// <summary>
        /// string.IsNullOrWhiteSpace
        /// </summary>
        StringIsNullOrWhiteSpace,

        /// <summary>
        /// string.Join
        /// </summary>
        StringJoin,

        /// <summary>
        /// string.Concat
        /// </summary>
        StringConcat,

        /// <summary>
        /// LIKE
        /// </summary>
        StringContains,

        /// <summary>
        /// LIKE 'xx%'
        /// </summary>
        StringStartsWith,

        /// <summary>
        /// LIKE '%xx'
        /// </summary>
        StringEndsWith,

        /// <summary>
        /// UPPER()
        /// </summary>
        StringToUpper,

        /// <summary>
        /// LOWER()
        /// </summary>
        StringToLower,

        /// <summary>
        /// LTRIM(RTRIM())
        /// </summary>
        StringTrim,

        /// <summary>
        /// LTRIM()
        /// </summary>
        StringTrimStart,

        /// <summary>
        /// RTRIM()
        /// </summary>
        StringTrimEnd,

        /// <summary>
        /// SUBSTRING -- T-SQL is 1-based indexed!
        /// </summary>
        StringSubstring,

        #endregion

        /// <summary>
        /// CAST() or CONVERT()
        /// </summary>
        Parse,

        /// <summary>
        /// Method from the Convert class (eg: Convert.ToInt32).
        /// </summary>
        ConvertMethod,

        /// <summary>
        /// Guid.NewGuid() -> NEWID()
        /// </summary>
        GuidNewGuid,

        /// <summary>
        /// A ToString() [without the format specified] is converted to CONVERT(xx, string)
        /// </summary>
        ToStringWithoutFormat,

        /// <summary>
        /// A ToString(format) [with the format specified] is converted to FORMAT(xx, format)
        /// </summary>
        ToStringWithFormat,

        /// <summary>
        /// A collection object's Contains -> xxx IN [...]
        /// </summary>
        CollectionContains,
    }

}
