using SujaySarma.Data.Core.Reflection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SujaySarma.Data.SqlServer.Builders
{
    /// <summary>
    /// Helps build a SQL query (SELECT) statement. 
    /// Supports: TOP, DISTINCT, INTO, JOINS, WHERE, GROUP BY, ORDER BY.
    /// </summary>
    public sealed class SqlQueryBuilder : SqlStatementBuilder
    {

        /// <summary>
        /// Assembles the SELECT query and returns it as a StringBuilder.
        /// </summary>
        /// <returns>StringBuilder populated with the query.</returns>
        public override StringBuilder Build()
        {
            ClrToTableWithAlias primaryTable = base.Map.GetPrimaryTable()
                ?? throw new InvalidOperationException("A primary table must be registered using From<T>() before Build() may be called.");

            StringBuilder builder = new StringBuilder();

            builder.Append("SELECT ");

            if (_topCount < uint.MaxValue)
            {
                builder.Append($"TOP {_topCount} ");
                if (_topIsPercent)
                {
                    builder.Append("PERCENT ");
                }
            }
            
            if (_isDistinct)
            {
                builder.Append("DISTINCT ");
            }
            
            builder.AppendJoin(',', _selectColumns);
            builder.Append(' ');

            if (! string.IsNullOrWhiteSpace(_intoTable))
            {
                builder.Append($"INTO {_intoTable} ");
            }
            
            builder.Append($"FROM {primaryTable.QualifiedTableNameWithAlias} ");

            if (_tableHints != SqlTableHints.None)
            {
                List<string> hints = new List<string>();
                foreach (SqlTableHints hint in Enum.GetValues<SqlTableHints>())
                {
                    if (_tableHints.HasFlag(hint))
                    {
                        hints.Append($"{hint.ToString().ToUpper()}");
                    }
                }

                if (hints.Count > 0)
                {
                    builder.Append($"WITH ({string.Join(',', hints)}) ");
                }
            }
            
            if (_joins.HasItems)
            {
                builder.Append(_joins.ToString());
                builder.Append(' ');
            }

            if (_where.HasItems)
            {
                builder.Append("WHERE ");
                builder.Append(_where.ToString());
                builder.Append(' ');
            }

            if (! string.IsNullOrWhiteSpace(_groupBy))
            {
                builder.Append(_groupBy);
            }

            if (_orderBy.Length > 0)
            {
                builder.Append("ORDER BY ");
                builder.Append(_orderBy.ToString().Trim());
            }

            builder.Append(';');

            return builder;
        }

        #region Joins

        /// <summary>
        /// Add a JOIN between <typeparamref name="TLeftTable"/> and <typeparamref name="TRightTable"/>.
        /// </summary>
        /// <typeparam name="TLeftTable">Type of .NET object for the LEFT table in the join</typeparam>
        /// <typeparam name="TRightTable">Type of .NET object for the RIGHT table in the join</typeparam>
        /// <param name="joinCondition">Condition to join the tables</param>
        /// <param name="type">The type of join to perform. Default: INNER JOIN</param>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder Join<TLeftTable, TRightTable>(Expression<Func<TLeftTable, TRightTable, bool>> joinCondition, TypesOfJoinsEnum type = TypesOfJoinsEnum.Inner)
        {
            _joins.Add<TLeftTable, TRightTable>(joinCondition, type);
            return this;
        }

        /// <summary>
        /// Add an INNER JOIN between <typeparamref name="TLeftTable"/> and <typeparamref name="TRightTable"/>.
        /// </summary>
        /// <typeparam name="TLeftTable">Type of .NET object for the LEFT table in the join</typeparam>
        /// <typeparam name="TRightTable">Type of .NET object for the RIGHT table in the join</typeparam>
        /// <param name="joinCondition">Condition to join the tables</param>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder InnerJoin<TLeftTable, TRightTable>(Expression<Func<TLeftTable, TRightTable, bool>> joinCondition)
        {
            _joins.Add<TLeftTable, TRightTable>(joinCondition, TypesOfJoinsEnum.Inner);
            return this;
        }

        /// <summary>
        /// Add a LEFT JOIN between <typeparamref name="TLeftTable"/> and <typeparamref name="TRightTable"/>.
        /// </summary>
        /// <typeparam name="TLeftTable">Type of .NET object for the LEFT table in the join</typeparam>
        /// <typeparam name="TRightTable">Type of .NET object for the RIGHT table in the join</typeparam>
        /// <param name="joinCondition">Condition to join the tables</param>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder LeftJoin<TLeftTable, TRightTable>(Expression<Func<TLeftTable, TRightTable, bool>> joinCondition)
        {
            _joins.Add<TLeftTable, TRightTable>(joinCondition, TypesOfJoinsEnum.Left);
            return this;
        }

        /// <summary>
        /// Add a RIGHT JOIN between <typeparamref name="TLeftTable"/> and <typeparamref name="TRightTable"/>.
        /// </summary>
        /// <typeparam name="TLeftTable">Type of .NET object for the LEFT table in the join</typeparam>
        /// <typeparam name="TRightTable">Type of .NET object for the RIGHT table in the join</typeparam>
        /// <param name="joinCondition">Condition to join the tables</param>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder RightJoin<TLeftTable, TRightTable>(Expression<Func<TLeftTable, TRightTable, bool>> joinCondition)
        {
            _joins.Add<TLeftTable, TRightTable>(joinCondition, TypesOfJoinsEnum.Right);
            return this;
        }

        /// <summary>
        /// Add a FULL JOIN between <typeparamref name="TLeftTable"/> and <typeparamref name="TRightTable"/>.
        /// </summary>
        /// <typeparam name="TLeftTable">Type of .NET object for the LEFT table in the join</typeparam>
        /// <typeparam name="TRightTable">Type of .NET object for the RIGHT table in the join</typeparam>
        /// <param name="joinCondition">Condition to join the tables</param>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder FullJoin<TLeftTable, TRightTable>(Expression<Func<TLeftTable, TRightTable, bool>> joinCondition)
        {
            _joins.Add<TLeftTable, TRightTable>(joinCondition, TypesOfJoinsEnum.Full);
            return this;
        }

        /// <summary>
        /// Add a JOIN between the primary table (added via the <see cref="From{TPrimaryTableObject}"/> function) and the free-string table named by <paramref name="rightTableName"/>.
        /// </summary>
        /// <param name="rightTableName">Name of the table joining with.</param>
        /// <param name="joinCondition">Condition to join the tables</param>
        /// <param name="type">The type of join to perform. Default: INNER JOIN</param>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder Join(string rightTableName, string joinCondition, TypesOfJoinsEnum type = TypesOfJoinsEnum.Inner)
        {
            _joins.Add(rightTableName, joinCondition, type);
            return this;
        }

        #endregion

        #region Group By

        /// <summary>
        /// Add one or more GROUP BY column(s) to the collection. This function can be used only once as a SQL query may contain only one GROUP BY clause!
        /// </summary>
        /// <typeparam name="TTable">Type of .NET object</typeparam>
        /// <param name="selector">Linq expression to select the column(s) for the grouping.</param>
        /// <param name="having">Linq expression to select the conditions for the HAVING clause. NULL to exclude the HAVING.</param>
        /// <param name="type">Type of GROUP BY to generate</param>
        public SqlQueryBuilder GroupBy<TTable>(Expression selector, Expression? having = null, TypesOfGroupByEnum type = TypesOfGroupByEnum.Standard)
        {
            StringBuilder sb = new StringBuilder();
            if (! string.IsNullOrWhiteSpace(_groupBy))
            {
                throw new InvalidOperationException("GROUP BY may be set only once.");
            }

            base.Add<TTable>();
            string groupByColumns = ExpressionToSQL(selector);
            string? havingCondition = ((having != null) ? ExpressionToSQL(having) : null);

            switch (type)
            {
                case TypesOfGroupByEnum.Standard:
                    sb.Append($"GROUP BY {groupByColumns}");
                    if (havingCondition != null)
                    {
                        sb.Append($" HAVING {havingCondition}");
                    }
                    break;

                case TypesOfGroupByEnum.Rollup:
                    sb.Append($"GROUP BY ROLLUP({groupByColumns})");
                    if (havingCondition != null)
                    {
                        sb.Append($" HAVING {havingCondition}");
                    }
                    break;

                case TypesOfGroupByEnum.Cube:
                    sb.Append($"GROUP BY CUBE({groupByColumns})");
                    if (havingCondition != null)
                    {
                        sb.Append($" HAVING {havingCondition}");
                    }
                    break;

                case TypesOfGroupByEnum.GroupingSets:
                    sb.Append($"GROUP BY GROUPING SETS ({groupByColumns})");
                    if (havingCondition != null)
                    {
                        sb.Append($" HAVING {havingCondition}");
                    }
                    break;

                case TypesOfGroupByEnum.EmptyGroup:
                    sb.Append("GROUP BY ()");
                    break;
            }

            _groupBy = sb.ToString();

            return this;
        }

        /// <summary>
        /// Add one or more GROUP BY ROLLUP column(s) to the collection. This function can be used only once as a SQL query may contain only one GROUP BY clause!
        /// </summary>
        /// <typeparam name="TTable">Type of .NET object</typeparam>
        /// <param name="selector">Linq expression to select the column(s) for the grouping.</param>
        /// <param name="having">Linq expression to select the conditions for the HAVING clause. NULL to exclude the HAVING.</param>
        public SqlQueryBuilder GroupByRollup<TTable>(Expression selector, Expression? having = null)
        {
            return GroupBy<TTable>(selector, having, TypesOfGroupByEnum.Rollup);
        }

        /// <summary>
        /// Add one or more GROUP BY CUBE column(s) to the collection. This function can be used only once as a SQL query may contain only one GROUP BY clause!
        /// </summary>
        /// <typeparam name="TTable">Type of .NET object</typeparam>
        /// <param name="selector">Linq expression to select the column(s) for the grouping.</param>
        /// <param name="having">Linq expression to select the conditions for the HAVING clause. NULL to exclude the HAVING.</param>
        public SqlQueryBuilder GroupByCube<TTable>(Expression selector, Expression? having = null)
        {
            return GroupBy<TTable>(selector, having, TypesOfGroupByEnum.Cube);
        }

        /// <summary>
        /// Add one or more GROUP BY GROUPING SETS column(s) to the collection. This function can be used only once as a SQL query may contain only one GROUP BY clause!
        /// </summary>
        /// <typeparam name="TTable">Type of .NET object</typeparam>
        /// <param name="selector">Linq expression to select the column(s) for the grouping.</param>
        /// <param name="having">Linq expression to select the conditions for the HAVING clause. NULL to exclude the HAVING.</param>
        public SqlQueryBuilder GroupByGroupingSets<TTable>(Expression selector, Expression? having = null)
        {
            return GroupBy<TTable>(selector, having, TypesOfGroupByEnum.GroupingSets);
        }

        /// <summary>
        /// Add one or more GROUP BY () column(s) to the collection. This function can be used only once as a SQL query may contain only one GROUP BY clause!
        /// </summary>
        /// <typeparam name="TTable">Type of .NET object</typeparam>
        /// <param name="selector">Linq expression to select the column(s) for the grouping.</param>
        /// <param name="having">Linq expression to select the conditions for the HAVING clause. NULL to exclude the HAVING.</param>
        public SqlQueryBuilder GroupByEmpty<TTable>(Expression selector, Expression? having = null)
        {
            return GroupBy<TTable>(selector, having, TypesOfGroupByEnum.EmptyGroup);
        }

        #endregion

        #region Order By

        /// <summary>
        /// Add an expression to order the rows of the result by, also specifying the direction of the ordering.
        /// </summary>
        /// <typeparam name="TTable">Type of .NET object for object reference in condition</typeparam>
        /// <typeparam name="TResult">Type of .NET object</typeparam>
        /// <param name="expression">Selector for parameters of the ORDER BY</param>
        /// <param name="direction">Direction of sorting</param>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder OrderBy<TTable, TResult>(Expression<Func<TTable, TResult>> expression, SortOrderEnum direction = SortOrderEnum.ASC)
        {
            base.Add<TTable>();
            string column = base.ExpressionToSQL(expression);
            string order = (direction != SortOrderEnum.DESC ? "ASC" : "DESC");

            _orderBy.Append($"{column} {order} ");
            return this;
        }

        /// <summary>
        /// Add an expression to order the rows of the result by, using Ascending (ASC) order.
        /// </summary>
        /// <typeparam name="TTable">Type of .NET object for object reference in condition</typeparam>
        /// <typeparam name="TResult">Type of .NET object</typeparam>
        /// <param name="expression">Selector for parameters of the ORDER BY</param>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder OrderByASC<TTable, TResult>(Expression<Func<TTable, TResult>> expression)
        {
            return OrderBy<TTable, TResult>(expression, SortOrderEnum.ASC);
        }

        /// <summary>
        /// Add an expression to order the rows of the result by, using Descending (DESC) order.
        /// </summary>
        /// <typeparam name="TTable">Type of .NET object for object reference in condition</typeparam>
        /// <typeparam name="TResult">Type of .NET object</typeparam>
        /// <param name="expression">Selector for parameters of the ORDER BY</param>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder OrderByDESC<TTable, TResult>(Expression<Func<TTable, TResult>> expression)
        {
            return OrderBy<TTable, TResult>(expression, SortOrderEnum.DESC);
        }

        #endregion

        #region Where

        /// <summary>
        /// Add a condition that helps filter the rows of the returned dataset, also specifying the operator (AND, OR) that joins this condition to ones already added.
        /// </summary>
        /// <typeparam name="TTable">Type of .NET object for object reference in condition</typeparam>
        /// <param name="conditions">One or more conditions in Lambda Expression form</param>
        /// <param name="conditionAppendingOperator">Operator to append the current set of conditions to the ones already added</param>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder Where<TTable>(Expression<Func<TTable, bool>> conditions, OperatorsToJoinConditionsEnum conditionAppendingOperator = OperatorsToJoinConditionsEnum.And)
        {
            _where.Add<TTable>(conditions, conditionAppendingOperator);
            return this;
        }

        /// <summary>
        /// Add a condition that helps filter the rows of the returned dataset, also specifying the operator (AND, OR) that joins this condition to ones already added.
        /// </summary>
        /// <typeparam name="TTable1">Type of .NET object for object reference in condition</typeparam>
        /// <typeparam name="TTable2">Type of .NET object for object reference in condition</typeparam>
        /// <param name="conditions">One or more conditions in Lambda Expression form</param>
        /// <param name="conditionAppendingOperator">Operator to append the current set of conditions to the ones already added</param>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder Where<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> conditions, OperatorsToJoinConditionsEnum conditionAppendingOperator = OperatorsToJoinConditionsEnum.And)
        {
            _where.Add<TTable1, TTable2>(conditions, conditionAppendingOperator);
            return this;
        }

        /// <summary>
        /// Add a condition that helps filter the rows of the returned dataset, joining this condition to ones already added using the OR operator.
        /// </summary>
        /// <typeparam name="TTable">Type of .NET object for object reference in condition</typeparam>
        /// <param name="conditions">One or more conditions in Lambda Expression form</param>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder OrWhere<TTable>(Expression<Func<TTable, bool>> conditions)
        {
            _where.Add<TTable>(conditions, OperatorsToJoinConditionsEnum.Or);
            return this;
        }

        //// <summary>
        /// Add a condition that helps filter the rows of the returned dataset, joining this condition to ones already added using the OR operator.
        /// </summary>
        /// <typeparam name="TTable1">Type of .NET object for object reference in condition</typeparam>
        /// <typeparam name="TTable2">Type of .NET object for object reference in condition</typeparam>
        /// <param name="conditions">One or more conditions in Lambda Expression form</param>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder OrWhere<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> conditions)
        {
            _where.Add<TTable1, TTable2>(conditions, OperatorsToJoinConditionsEnum.Or);
            return this;
        }

        /// <summary>
        /// Add a condition that helps filter the rows of the returned dataset, joining this condition to ones already added using the AND operator.
        /// </summary>
        /// <typeparam name="TTable">Type of .NET object for object reference in condition</typeparam>
        /// <param name="conditions">One or more conditions in Lambda Expression form</param>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder AndWhere<TTable>(Expression<Func<TTable, bool>> conditions)
        {
            _where.Add<TTable>(conditions, OperatorsToJoinConditionsEnum.And);
            return this;
        }

        /// <summary>
        /// Add a condition that helps filter the rows of the returned dataset, joining this condition to ones already added using the AND operator.
        /// </summary>
        /// <typeparam name="TTable1">Type of .NET object for object reference in condition</typeparam>
        /// <typeparam name="TTable2">Type of .NET object for object reference in condition</typeparam>
        /// <param name="conditions">One or more conditions in Lambda Expression form</param>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder AndWhere<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> conditions)
        {
            _where.Add<TTable1, TTable2>(conditions, OperatorsToJoinConditionsEnum.And);
            return this;
        }

        #endregion

        #region INTO (Select...Into)

        /// <summary>
        /// Specify the name of the dynamically created table that the rows are to be inserted into.
        /// </summary>
        /// <typeparam name="TTable2">Type of .NET object of the newly created table (to be inserted into).</typeparam>
        /// <returns>Self-instance.</returns>
        public SqlQueryBuilder Into<TTable2>()
        {
            ClrToTableWithAlias map = base.Add<TTable2>(false);
            _intoTable = map.QualifiedTableName;

            return this;
        }

        /// <summary>
        /// Specify the name of the dynamically created table that the rows are to be inserted into.
        /// </summary>
        /// <param name="tableName">Free-form name (unvalidated!) of the newly created table (to be inserted into).</param>
        /// <returns>Self-instance.</returns>
        public SqlQueryBuilder Into(string tableName)
        {
            _intoTable = tableName;

            return this;
        }

        #endregion

        #region Primary Clause

        /// <summary>
        /// Provide a list of named values that are appended to the list of columns provided via one of the Select() functions.
        /// </summary>
        /// <param name="additionalValues">Dictionary of additional values to inject</param>
        public SqlQueryBuilder WithAdditionalValues(Dictionary<string, object?> additionalValues)
        {
            foreach (string columnName in additionalValues.Keys)
            {
                if (!_selectColumns.Any(cn => cn.Equals(columnName, StringComparison.OrdinalIgnoreCase)))
                {
                    _selectColumns.Add($"{ReflectionUtils.GetSQLStringValue(additionalValues[columnName])} as [{columnName}]");
                }
            }
            return this;
        }

        /// <summary>
        /// Provide the list of columns that are to be selected as one or more expressions and/or literal constants.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="selectors">One or more selectors for the column (eg: u =&gt; u.Id). Do NOT use object selectors such as 'u =&gt; u' this will result in empty selectors. Instead use the Select[TObject]() overload.</param>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder Select<TObject>(params Expression<Func<TObject, object>>[] selectors)
        {
            base.Map.Add<TObject>();
            foreach (Expression selector in selectors)
            {
                _selectColumns.Add(base.ExpressionToSQL(selector, true));
            }
            return this;
        }

        /// <summary>
        /// Specify that all of the eligible columns from the specified <typeparamref name="TObject" /> object are to be selected.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder Select<TObject>()
        {
            ClrToTableWithAlias map = base.Map.Add<TObject>();
            foreach (MemberTypeInfo memberTypeInformation in map.TypeInfo.Members.Values)
            {
                AddColumnIfNotExistsOrSkip($"{map.Alias}.{memberTypeInformation.Column.CreateQualifiedName()}");
            }
            return this;
        }

        /// <summary>
        /// Specify that all of the eligible columns from all added table types are to be selected.
        /// (eg: "SELECT * FROM...")
        /// </summary>
        /// <returns>Self-instance</returns>
        public SqlQueryBuilder Select()
        {
            _selectColumns.Clear();

            // We elect to list the actual column names as this better reflects the .Net object space that we 
            // want to populate/manipulate using this library.

            foreach (ClrToTableWithAlias typeTable in base.Map)
            {
                foreach (MemberTypeInfo memberTypeInformation in typeTable.TypeInfo.Members.Values)
                {
                    AddColumnIfNotExistsOrSkip($"{typeTable.Alias}.{memberTypeInformation.Column.CreateQualifiedName()}");
                }
            }
            return this;
        }


        /// <summary>
        /// Set this query to return only <paramref name="count"/> number of rows.
        /// </summary>
        /// <param name="count">Zero is a valid value.</param>
        /// <param name="percent">If true, <paramref name="count"/> is a percent value, returned rounded up to the next whole number.</param>
        /// <returns>Instance of self.</returns>
        public SqlQueryBuilder Top(uint count, bool percent = false)
        {
            _topCount = count;
            _topIsPercent = percent;
            return this;
        }

        /// <summary>
        /// Set this query to return DISTINCT rows.
        /// </summary>
        /// <returns>Instance of self.</returns>
        public SqlQueryBuilder Distinct()
        {
            _isDistinct = true;
            return this;
        }

        /// <summary>
        /// Specify one or more table hints. You may call this method multiple times to add multiple table hints OR specify all the hints at once 
        /// using the OR pattern.
        /// </summary>
        /// <param name="tableHints">Table hints to specify: may be a single value, or OR'ed with other values.</param>
        /// <returns>Self-instance.</returns>
        public SqlQueryBuilder With(SqlTableHints tableHints)
        {
            // add only those that are not already added.
            foreach(SqlTableHints hint in Enum.GetValues<SqlTableHints>())
            {
                if (tableHints.HasFlag(hint) && (!_tableHints.HasFlag(hint)))
                {
                    _tableHints |= hint;
                }
            }

            return this;
        }

        #endregion

        /// <summary>
        /// Defines the primary .NET object and its backing SQL table that shouldbe used for this query. 
        /// All unqualified column references are deemed to be homed in this object/table.
        /// </summary>
        /// <typeparam name="TPrimaryTableObject">Type of primary .NET object.</typeparam>
        /// <returns>A new instance of the SqlQueryBuilder.</returns>
        public static SqlQueryBuilder From<TPrimaryTableObject>()
            => new SqlQueryBuilder(typeof(TPrimaryTableObject));

        /// <summary>
        /// Initialiser. Consciously private to avoid anyone using the constructor-route!
        /// </summary>
        /// <param name="typeOfFromTable">Type of the primary table</param>
        private SqlQueryBuilder(Type typeOfFromTable)
        {
            base.Add(typeOfFromTable, true);

            _selectColumns = new List<string>();
            _tableHints = SqlTableHints.None;
            _joins = new SqlJoin(base.Map);
            _where = new SqlWhere(base.Map);
            _groupBy = null;
            _orderBy = new StringBuilder();
            _intoTable = null;
        }

        private List<string> _selectColumns;
        private SqlTableHints _tableHints;
        private SqlJoin _joins;
        private SqlWhere _where;
        private string? _groupBy;
        private StringBuilder _orderBy;
        private bool _isDistinct = false;
        private uint _topCount = uint.MaxValue;
        private bool _topIsPercent = false;
        private string? _intoTable;

        /// <summary>
        /// Adds the column name to the list distinctly.
        /// </summary>
        /// <param name="columnName">Name of the column to add.</param>
        private void AddColumnIfNotExistsOrSkip(string columnName)
        {
            if (_selectColumns.Any(cn => cn.Equals(columnName, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            _selectColumns.Add(columnName);
        }
    }
}
