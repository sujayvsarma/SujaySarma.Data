using SujaySarma.Data.Core.Constants;
using SujaySarma.Data.SqlServer.Attributes;
using SujaySarma.Data.SqlServer.Fluid.AliasMaps;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SujaySarma.Data.SqlServer.LinqParsers
{
    /// <summary>
    /// A visitor that examines parts of an expression and returns the relevant results
    /// </summary>
    internal class SqlLambdaVisitor : ExpressionVisitor
    {

        /// <summary>Parse the Linq expression to a SQL expression</summary>
        /// <param name="expression">Linq expression to parse</param>
        /// <param name="treatAssignmentsAsAlias">[Optional] When set, tells the parser to treat any assignments in the expression as aliases. For eg: 'a = s.Id' will turn into 's.Id as [a]'</param>
        /// <returns>SQL expression</returns>
        public string ParseToSql(Expression expression, bool treatAssignmentsAsAlias = false)
        {
            _treatAssignmentsAsAlias = treatAssignmentsAsAlias;
            _originalExpression = expression;
            Visit(expression);

            StringBuilder stringBuilder = new StringBuilder();
            while (_values.Count > 0)
            {
                stringBuilder.Append(_values.Pop());
                stringBuilder.Append(' ');
            }
            return stringBuilder.ToString().Trim();
        }

        /// <summary>
        /// Resolve a conditional expression ((a &gt; b) ? c : d) to a SQL expression (CASE WHEN ELSE)
        /// </summary>
        protected override Expression VisitConditional(ConditionalExpression node)
        {
            Visit(node.Test);
            string testStr = _values.Pop();

            Visit(node.IfTrue);
            string trueStr = _values.Pop();

            Visit(node.IfFalse);
            string falseStr = _values.Pop();

            _values.Push($"CASE WHEN ({testStr}) THEN {trueStr} ELSE {falseStr} END");
            
            return (Expression)node;
        }

        /// <summary>
        /// Resolve a binary expression (eg: A + B, X == Y, etc) into its SQL expression
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            string operatorName = GetSqlOperatorForExpressionType((Expression)node);
            
            Visit(node.Left);
            string leftStr = _values.Pop();

            Visit(node.Right);
            string rightStr = _values.Pop();

            if (rightStr == "NULL")
            {
                operatorName = ((!(operatorName == "=")) ? "IS NOT" : "IS");
            }

            _values.Push($"({leftStr} {operatorName} {rightStr})");
            return (Expression)node;
        }

        /// <summary>
        /// Resolves a unary expression (NOT x, -ABC, etc) into its SQL expression.
        /// </summary>
        protected override Expression VisitUnary(UnaryExpression node)
        {
            Visit(node.Operand);
            string str = _values.Pop();
            switch (node.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    str = $"(-{str})";
                    break;

                case ExpressionType.Not:
                    str = "NOT " + str;
                    break;
            }

            _values.Push(str);
            return (Expression)node;
        }

        /// <summary>
        /// Resolves a member access (A.B) to its SQL table.column expression. If member is static or
        /// non-table mapped entity, its value is taken instead.
        /// </summary>
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member.DeclaringType == null)
            {
                return (Expression)node;
            }

            FieldInfo member = (FieldInfo)node.Member;

            if ((member != null) && member.IsStatic)
            {
                object? clrValue = member.GetValue(null);

                if (_serialiseEnumsAsStrings)
                {
                    clrValue = Core.ReflectionUtils.ConvertValueIfRequired(clrValue, typeof(string));
                }

                _values.Push(ReflectionUtils.GetSQLStringValue(clrValue));
                return (Expression)node;
            }

            string? aliasOrName = _typeTableAliasMap.GetAliasOrName(node.Member.DeclaringType);
            if (string.IsNullOrWhiteSpace(aliasOrName))
            {
                Type memberType = _originalExpression.GetType();
                if (memberType.IsGenericType)
                {
                    Type[] genericArguments = memberType.GetGenericArguments();
                    if ((genericArguments.Length == 1) && genericArguments[0].IsGenericType)
                    {
                        genericArguments = genericArguments[0].GetGenericArguments();
                    }

                    foreach (Type genericArgumentType in genericArguments)
                    {
                        if (node.Member.DeclaringType.IsAssignableFrom(genericArgumentType))
                        {
                            aliasOrName = _typeTableAliasMap.GetAliasOrName(genericArgumentType);
                            break;
                        }
                    }
                }
            }

            Type propertyDataType = Core.ReflectionUtils.GetFieldOrPropertyDataType(node.Member);
            bool isEnum = propertyDataType.IsEnum;
            if (string.IsNullOrWhiteSpace(aliasOrName))
            {
                object? clrValue = ResolveExpressionAsValue(node);
                if (_serialiseEnumsAsStrings)
                {
                    clrValue = Core.ReflectionUtils.ConvertValueIfRequired(clrValue, typeof(string));
                }

                _values.Push(ReflectionUtils.GetSQLStringValue(clrValue));
                return (Expression)node;
            }

            TableColumnAttribute? customAttribute = node.Member.GetCustomAttribute<TableColumnAttribute>(true);
            if (customAttribute != null)
            {
                string qualifiedName = customAttribute.CreateQualifiedName();
                if (!string.IsNullOrWhiteSpace(qualifiedName))
                {
                    _values.Push($"{aliasOrName}.[{qualifiedName}]");
                    if (isEnum)
                    {
                        _currentEnum = propertyDataType;
                        _serialiseEnumsAsStrings = ((customAttribute.IfEnumSerialiseAs == EnumSerializationStrategy.AsString) ? true : false);
                    }
                }
            }

            return (Expression)node;
        }

        /// <summary>
        /// Get the value of a constant
        /// </summary>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            _values.Push(SerializeToString(node.Value));
            return (Expression)node;
        }

        /// <summary>
        /// Get value of a new object init within an expression, usually of anonymous types.
        /// Eg: x =&gt; new { x.Id, x.Name } --&gt; "t.[Id], t.[Name]..."
        /// </summary>
        protected override Expression VisitNew(NewExpression node)
        {
            ReadOnlyCollection<MemberInfo>? members = node.Members;
            IEnumerable<KeyValuePair<MemberInfo, Expression>>? keyValuePairs = (members?.Zip<MemberInfo, Expression, KeyValuePair<MemberInfo, Expression>>(
                    node.Arguments, 
                        (m, a) => new KeyValuePair<MemberInfo, Expression>(m, a))
                );

            if (keyValuePairs != null)
            {
                List<string> values = new List<string>();
                foreach (KeyValuePair<MemberInfo, Expression> keyValuePair in keyValuePairs)
                {
                    Visit(keyValuePair.Value);
                    string str = _values.Pop();

                    if (!string.IsNullOrWhiteSpace(str))
                    {
                        if (_treatAssignmentsAsAlias)
                        {
                            values.Add($"{str} AS [{keyValuePair.Key.Name}]");
                        }
                        else
                        {
                            values.Add(str);
                        }
                    }
                }
                if (values.Count > 0)
                {
                    _values.Push(string.Join(',', values));
                }
            }

            return (Expression)node;
        }

        /// <summary>
        /// Get the right SQL operator for selected method calls. Other methods are processed as normally
        /// (and often erroneously) by the system
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            RecognisedMethodCallsEnum recognisedMethodCallsEnum = RecognisedMethodCallsEnum.Unknown;
            if (node.Method.Name.Equals("Contains"))
            {
                recognisedMethodCallsEnum = RecognisedMethodCallsEnum.Contains;
            }

            Expression expression = base.VisitMethodCall(node);
            if (recognisedMethodCallsEnum != RecognisedMethodCallsEnum.Contains)
            {
                return expression;
            }

            _values.Push($"{_values.Pop()} IN ({_values.Pop()})");
            return expression;
        }


        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="typeTableMap">Mapping of types with tables</param>
        public SqlLambdaVisitor(TypeTableAliasMapCollection typeTableMap)
            : base()
        {
            _typeTableAliasMap = typeTableMap;
            _values = new Stack<string>();
        }

        /// <summary>
        /// When we need to find the VALUE pointed to by an A.B.C member access expression,
        /// this function recursively walks through to the final element and then walks back
        /// to resolve the value of A.B.C.
        /// </summary>
        /// <param name="expression">Expression to traverse</param>
        /// <returns>The raw value of what we found</returns>
        private static object? ResolveExpressionAsValue(MemberExpression expression)
        {
            object? parentInstance = null;
            Stack<MemberInfo> memberInfoStack = new Stack<MemberInfo>();
            MemberExpression memberExpression = expression;

            bool continueLoop = true;
            do
            {
                memberInfoStack.Push(memberExpression.Member);
                if ((memberExpression.Expression != null) && (memberExpression.Expression is MemberExpression expression1))
                {
                    memberExpression = expression1;
                }
                else
                {
                    continueLoop = false;
                }
            } while (continueLoop);

            if ((memberExpression.Expression != null) && (memberExpression.Expression is ConstantExpression expression2))
            {
                parentInstance = GetValueFromPropertyOrField(memberInfoStack.Pop(), expression2.Value);
            }

            while (memberInfoStack.Count > 0)
            {
                parentInstance = GetValueFromPropertyOrField(memberInfoStack.Pop(), parentInstance);
            }

            return parentInstance;
        }

        /// <summary>
        /// Get a value from a given object member that could be a property or a field
        /// </summary>
        /// <param name="propertyOrFieldInfo">The object-member (a property or a field)</param>
        /// <param name="parentInstance">Instance of object. NULL for static references</param>
        /// <returns>Raw value from object</returns>
        private static object? GetValueFromPropertyOrField(MemberInfo propertyOrFieldInfo, object? parentInstance)
        {
            switch (propertyOrFieldInfo.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)propertyOrFieldInfo).GetValue(parentInstance);

                case MemberTypes.Property:
                    return ((PropertyInfo)propertyOrFieldInfo).GetValue(parentInstance);

                default:
                    throw new InvalidOperationException($"Unsupported operation: Cannot get value from member of type '{propertyOrFieldInfo.MemberType}'");
            }
        }

        /// <summary>
        /// Serialise the given value to a string. The string returned will be T-SQL compatible.
        /// </summary>
        /// <param name="value">Value to serialise</param>
        /// <returns>String compatible with T-SQL</returns>
        private string SerializeToString(object? value)
        {
            string str = string.Empty;
            if ((_currentEnum == null) || (value == null))
            {
                str = ReflectionUtils.GetSQLStringValue(value);
            }
            else
            {
                if (Enum.TryParse(_currentEnum, value.ToString(), out object? e))
                {
                    str = ReflectionUtils.GetSQLStringValue(e);
                }
            }

            return str!;
        }

        /// <summary>
        /// Get the SQL operator for the type of node
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>SQL operator string</returns>
        private static string GetSqlOperatorForExpressionType(Expression node)
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

                default:
                    throw new NotSupportedException($"Operator {node.NodeType} is not supported (yet).");
            }
        }

        private Expression _originalExpression = default!;
        private bool _serialiseEnumsAsStrings = true;
        private Type? _currentEnum = null;
        private readonly Stack<string> _values = new Stack<string>();
        private readonly TypeTableAliasMapCollection _typeTableAliasMap = new TypeTableAliasMapCollection();
        private bool _treatAssignmentsAsAlias = true;


        /// <summary>
        /// The methods that we recognise and know what to do with when
        /// handling the VisitMethodCall visitor.
        /// </summary>
        private enum RecognisedMethodCallsEnum
        {
            /// <summary>
            /// Unknown
            /// </summary>
            Unknown,

            /// <summary>
            /// Linq Contains() translates to T-SQL "A IN [LIST]"
            /// </summary>
            Contains
        }
    }
}
