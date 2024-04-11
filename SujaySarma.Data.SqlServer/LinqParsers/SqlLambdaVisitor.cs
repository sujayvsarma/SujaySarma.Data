using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using SujaySarma.Data.SqlServer.Attributes;
using SujaySarma.Data.SqlServer.Fluid.Tools;

namespace SujaySarma.Data.SqlServer.LinqParsers
{
    /// <summary>
    /// A visitor that examines parts of an expression and returns the relevant results
    /// </summary>
    internal class SqlLambdaVisitor : ExpressionVisitor
    {
        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="typeTableMap">Mapping of types with tables</param>
        public SqlLambdaVisitor(TypeTableAliasMapCollection typeTableMap) : base()
        {
            _typeTableAliasMap = typeTableMap;
            _values = new Stack<string>();
        }

        /// <summary>
        /// Parse the Linq expression to a SQL expression
        /// </summary>
        /// <param name="expression">Linq expression to parse</param>
        /// /// <param name="treatAssignmentsAsAlias">[Optional] When set, tells the parser to treat any assignments in the expression as aliases. For eg: 'a = s.Id' will turn into 's.Id as [a]'</param>
        /// <returns>SQL expression</returns>
        public string ParseToSql(Expression expression, bool treatAssignmentsAsAlias = false)
        {
            _treatAssignmentsAsAlias = treatAssignmentsAsAlias;
            _originalExpression = expression;
            Visit(expression);

            StringBuilder sql = new StringBuilder();
            while (_values.Count > 0)
            {
                sql.Append(_values.Pop());
                sql.Append(' ');
            }

            return sql.ToString().Trim();
        }
        private Expression _originalExpression = default!;

        /// <summary>
        /// Resolve a conditional expression ((a > b) ? c : d) to a SQL expression (CASE WHEN ELSE)
        /// </summary>
        protected override Expression VisitConditional(ConditionalExpression node)
        {
            Visit(node.Test);
            string caseWhenConditionSql = _values.Pop();

            Visit(node.IfTrue);
            string caseWhenTrueSql = _values.Pop();

            Visit(node.IfFalse);
            string caseElseSql = _values.Pop();

            _values.Push($"CASE WHEN ({caseWhenConditionSql}) THEN {caseWhenTrueSql} ELSE {caseElseSql} END");

            return node;
        }

        /// <summary>
        /// Resolve a binary expression (eg: A + B, X == Y, etc) into its SQL expression
        /// </summary>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            string @operator = GetSqlOperatorForExpressionType(node);

            Visit(node.Left);
            string leftOperandSql = _values.Pop();

            Visit(node.Right);
            string rightOperandSql = _values.Pop();

            if (rightOperandSql == "NULL")
            {
                if (@operator == "==")
                {
                    @operator = "IS";
                }
                else
                {
                    @operator = "IS NOT";
                }
            }

            _values.Push($"({leftOperandSql} {@operator} {rightOperandSql})");

            return node;
        }

        /// <summary>
        /// Resolves a unary expression (NOT x, -ABC, etc) into its SQL expression.
        /// </summary>
        protected override Expression VisitUnary(UnaryExpression node)
        {
            // resolve the operand first
            Visit(node.Operand);
            string nodeSql = _values.Pop();

            switch (node.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                    nodeSql = $"(-{nodeSql})";
                    break;

                case ExpressionType.Not:
                    nodeSql = $"NOT {nodeSql}";
                    break;

                default:
                    break;
            }
            _values.Push(nodeSql);

            return node;
        }

        /// <summary>
        /// Resolves a member access (A.B) to its SQL table.column expression. If member is static or 
        /// non-table mapped entity, its value is taken instead.
        /// </summary>
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member.DeclaringType == null)
            {
                return node;
            }

            if ((node.Member is FieldInfo fi) && fi.IsStatic)
            {
                object? value = fi.GetValue(null);
                if (_serialiseEnumsAsStrings)
                {
                    value = SujaySarma.Data.Core.Reflection.ReflectionUtils.ConvertValueIfRequired(
                            value, 
                            typeof(string)
                        );
                }

                _values.Push(
                        ReflectionUtils.GetSQLStringValue(value)
                    );

                return node;
            }

            // normal property access
            string? tid = _typeTableAliasMap.GetAliasOrName(node.Member.DeclaringType);
            if (string.IsNullOrWhiteSpace(tid))
            {
                Type expType = _originalExpression.GetType();
                if (expType.IsGenericType)
                {
                    Type[] expParams = expType.GetGenericArguments();
                    if ((expParams.Length == 1) && expParams[0].IsGenericType)
                    {
                        expParams = expParams[0].GetGenericArguments();
                    }
                    foreach(Type ept in expParams)
                    {
                        if (node.Member.DeclaringType.IsAssignableFrom(ept))
                        {
                            tid = _typeTableAliasMap.GetAliasOrName(ept);
                            break;
                        }
                    }
                }
            }

            Type propertyOrFieldType = SujaySarma.Data.Core.Reflection.ReflectionUtils.GetFieldOrPropertyDataType(node.Member);
            bool isEnumProperty = propertyOrFieldType.IsEnum;

            if (string.IsNullOrWhiteSpace(tid))
            {
                // not a mapped type, so we need the value of what is being referenced
                object? resolvedValueOfExpression = ResolveExpressionAsValue(node);
                if (_serialiseEnumsAsStrings)
                {
                    resolvedValueOfExpression = SujaySarma.Data.Core.Reflection.ReflectionUtils.ConvertValueIfRequired(
                            resolvedValueOfExpression,
                            typeof(string)
                        );
                }
                _values.Push(
                        ReflectionUtils.GetSQLStringValue(resolvedValueOfExpression)
                    );
                return node;
            }

            // property of an object mapped to a table we can use
            TableColumnAttribute? columnAttribute = node.Member.GetCustomAttribute<TableColumnAttribute>(true);
            if (columnAttribute != null)
            {
                string columnName = columnAttribute.CreateQualifiedName();
                if (! string.IsNullOrWhiteSpace(columnName))
                {
                    // we have the table.column accessor!
                    _values.Push($"{tid}.[{columnName}]");
                    if (isEnumProperty)
                    {
                        _currentEnum = propertyOrFieldType;
                        _serialiseEnumsAsStrings = ((columnAttribute.EnumSerializationStrategy == Core.Constants.EnumSerializationBehaviour.AsString) ? true : false);
                    }
                }
            }

            return node;
        }

        /// <summary>
        /// Get the value of a constant
        /// </summary>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            _values.Push(SerializeToString(node.Value));
            return node;
        }

        /// <summary>
        /// Get value of a new object init within an expression, usually of anonymous types. 
        /// Eg: x => new { x.Id, x.Name } --> "t.[Id], t.[Name]..."
        /// </summary>
        protected override Expression VisitNew(NewExpression node)
        {
            IEnumerable<KeyValuePair<MemberInfo, Expression>>? args = 
                node.Members?.Zip(
                    node.Arguments, 
                    (m, a) => 
                        new KeyValuePair<MemberInfo, Expression>(m, a)
                );

            if (args != null)
            {
                List<string> list = new();
                foreach (KeyValuePair<MemberInfo, Expression> item in args)
                {
                    Visit(item.Value);

                    string s = _values.Pop();
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        if (_treatAssignmentsAsAlias)
                        {
                            list.Add($"{s} AS [{item.Key.Name}]");
                        }
                        else
                        {
                            list.Add(s);
                        }
                    }
                }

                if (list.Count > 0)
                {
                    _values.Push(string.Join(',', list));
                }
            }
            return node;
        }

        /// <summary>
        /// Get the right SQL operator for selected method calls. Other methods are processed as normally 
        /// (and often erroneously) by the system
        /// </summary>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            RecognisedMethodCallsEnum recognisedCallType = RecognisedMethodCallsEnum.Unknown;
            if (node.Method.Name.Equals("Contains"))
            {
                recognisedCallType = RecognisedMethodCallsEnum.Contains;
            }

            Expression processResult = base.VisitMethodCall(node);
            switch (recognisedCallType)
            {
                case RecognisedMethodCallsEnum.Contains:
                    // Stack order is           : <list> <variable>
                    // We need to change it to  : <variable> IN <list>

                    string fixedOrder = $"{_values.Pop()} IN ({_values.Pop()})";
                    _values.Push(fixedOrder);
                    
                    break;
            }
            
            return processResult;
        }

        /// <summary>
        /// Get the SQL operator for the type of node
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns>SQL operator string</returns>
        private static string GetSqlOperatorForExpressionType(Expression node)
            => node.NodeType switch
            {
                ExpressionType.Equal => "=",
                ExpressionType.NotEqual => "<>",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",

                ExpressionType.OrElse => "OR",
                ExpressionType.AndAlso => "AND",
                ExpressionType.Not => "NOT",

                _ => throw new NotSupportedException($"Operator {node.NodeType} is not supported (yet).")
            };

        /// <summary>
        /// When we need to find the VALUE pointed to by an A.B.C member access expression, 
        /// this function recursively walks through to the final element and then walks back 
        /// to resolve the value of A.B.C.
        /// </summary>
        /// <param name="expression">Expression to traverse</param>
        /// <returns>The raw value of what we found</returns>
        private static object? ResolveExpressionAsValue(MemberExpression expression)
        {
            object? result = null;
            Stack<MemberInfo> _recursiveResolverStack = new Stack<MemberInfo>();

            MemberExpression memberExpression = expression;
            do
            {
                _recursiveResolverStack.Push(memberExpression.Member);
                if ((memberExpression.Expression == null) || (memberExpression.Expression is not MemberExpression mex))
                {
                    break;
                }
                memberExpression = mex;

            } while (true);

            if (memberExpression.Expression != null)
            {
                if (memberExpression.Expression is ConstantExpression cex)
                {
                    MemberInfo parentPropertyOrField = _recursiveResolverStack.Pop();
                    object? referenceObject = cex.Value;
                    result = GetValueFromPropertyOrField(parentPropertyOrField, referenceObject);
                }
                // else: what else can it be!
            }

            while (_recursiveResolverStack.Count > 0)
            {
                MemberInfo parentPropertyOrField = _recursiveResolverStack.Pop();
                result = GetValueFromPropertyOrField(parentPropertyOrField, result);
            }
            return result;
        }

        /// <summary>
        /// Get a value from a given object member that could be a property or a field
        /// </summary>
        /// <param name="propertyOrFieldInfo">The object-member (a property or a field)</param>
        /// <param name="parentInstance">Instance of object. NULL for static references</param>
        /// <returns>Raw value from object</returns>
        private static object? GetValueFromPropertyOrField(MemberInfo propertyOrFieldInfo, object? parentInstance)
            => propertyOrFieldInfo.MemberType switch
            {
                MemberTypes.Property => ((PropertyInfo)propertyOrFieldInfo).GetValue(parentInstance),
                MemberTypes.Field => ((FieldInfo)propertyOrFieldInfo).GetValue(parentInstance),

                _ => throw new InvalidOperationException($"Unsupported operation: Cannot get value from member of type '{propertyOrFieldInfo.MemberType}'")
            };

        /// <summary>
        /// Serialise the given value to a string. The string returned will be T-SQL compatible.
        /// </summary>
        /// <param name="value">Value to serialise</param>
        /// <returns>String compatible with T-SQL</returns>
        private string SerializeToString(object? value)
        {
            string result;
            if (_currentEnum == null)
            {
                result = ReflectionUtils.GetSQLStringValue(value);
            }
            else
            {
                result = ReflectionUtils.GetSQLStringValue(
                        ((value == null) ? null : Enum.Parse(_currentEnum, value.ToString()!))
                    );

                _currentEnum = null;
            }

            return result;
        }


        private bool _serialiseEnumsAsStrings = false;
        private Type? _currentEnum = null;

        private readonly Stack<string> _values;
        private readonly TypeTableAliasMapCollection _typeTableAliasMap;
        private bool _treatAssignmentsAsAlias = false;

        /// <summary>
        /// The methods that we recognise and know what to do with when 
        /// handling the VisitMethodCall visitor.
        /// </summary>
        private enum RecognisedMethodCallsEnum
        {
            /// <summary>
            /// Unknown
            /// </summary>
            Unknown = 0,


            /// <summary>
            /// Linq Contains() translates to T-SQL "A IN [LIST]"
            /// </summary>
            Contains
        }
    }
}
