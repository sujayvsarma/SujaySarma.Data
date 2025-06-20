using SujaySarma.Data.Core.Reflection;
using SujaySarma.Data.SqlServer.Attributes;

using System.Reflection;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SujaySarma.Data.SqlServer.Builders
{
    /// <summary>
    /// Helps build a SQL UPDATE statement.
    /// Supports: TOP, WITH, FROM, JOIN
    /// </summary>
    public sealed class SqlUpdateBuilder : SqlStatementBuilder
    {

        /// <summary>
        /// Assembles the UPDATE statement from provided clauses and returns it as a StringBuilder instance.
        /// </summary>
        /// <returns>Instance of StringBuilder containing the assembled UPDATE statement.</returns>
        public override StringBuilder Build()
        {
            if (_columnsWithValues.Count == 0)
            {
                throw new InvalidOperationException("No values have been provided for UPDATE.");
            }

            StringBuilder builder = new StringBuilder();

            foreach (Dictionary<string, string> columnValueSet in _columnsWithValues)
            {
                builder.Append("UPDATE ");

                if (_topCount < uint.MaxValue)
                {
                    builder.Append($"TOP {_topCount} ");
                    if (_topIsPercent)
                    {
                        builder.Append("PERCENT ");
                    }
                }

                builder.Append($"{_destinationTableName} ");

                if (_tableHints != SqlTableHints.None)
                {
                    List<string> hints = new List<string>();
                    foreach (SqlTableHints hint in Enum.GetValues<SqlTableHints>())
                    {
                        if (_tableHints.HasFlag(hint))
                        {
                            hints.Add($"{hint.ToString().ToUpper()}");
                        }
                    }

                    if (hints.Count > 0)
                    {
                        builder.Append($"WITH ({string.Join(',', hints)}) ");
                    }
                }

                builder.Append("SET ");

                bool isFirst = true;
                foreach (KeyValuePair<string, string> item in columnValueSet)
                {
                    if (!isFirst)
                    {
                        builder.Append(", ");
                    }
                    builder.Append($"{item.Key} = {item.Value}");
                    isFirst = false;
                }
                builder.Append(' ');

                if (_sourceDataQuery != null)
                {
                    builder.Append(_sourceDataQuery);
                    builder.Append(' ');
                }

                if (_joins.HasItems)
                {
                    builder.Append(_joins.ToString());
                    builder.Append(' ');
                }

                if (_where.HasItems)
                {
                    builder.Append("WHERE (").Append(_where.ToString()).Append(')');
                }
                else
                {
                    // Add WHERE clauses for keys in the data being updated.
                    ClrToTableWithAlias map = base.Map.GetPrimaryTable()!;

                    List<string> whereMap = new List<string>();
                    foreach (MemberTypeInfo member in map.TypeInfo.Members.Values)
                    {
                        TableColumnAttribute? columnAttribute = member.FieldOrPropertyInfo.GetCustomAttribute<TableColumnAttribute>();
                        if ((columnAttribute != null) && (columnAttribute.IsSearchKey))
                        {
                            // value maps are in the dictionary.
                            string columnName = columnAttribute.CreateQualifiedName();
                            whereMap.Add($"([{columnName}]={columnValueSet[columnName]})");
                        }
                    }

                    if (whereMap.Count > 0)
                    {
                        builder.Append("WHERE (").AppendJoin(" AND ", whereMap).Append(')');
                    }
                }

                    builder.Append(';');
                builder.AppendLine();
            }

            return builder;
        }

        #region Support for UPDATE FROM

        /// <summary>
        /// Set the datasource as a SQL SELECT query or a table name.
        /// </summary>
        /// <param name="query">An instance of a SqlUpdateBuilder containing the query that will provide the data to update.</param>
        /// <returns>Self-instance.</returns>
        public SqlUpdateBuilder From(SqlUpdateBuilder query)
        {
            _sourceDataQuery = query.Build();
            return this;
        }

        /// <summary>
        /// Set the datasource as a SQL SELECT query or a table name.
        /// </summary>
        /// <param name="query">An instance of a StringBuilder containing the query that will provide the data to update.</param>
        /// <returns>Self-instance.</returns>
        public SqlUpdateBuilder From(StringBuilder query)
        {
            _sourceDataQuery = query;
            return this;
        }

        /// <summary>
        /// Set the datasource as a SQL SELECT query or a table name.
        /// </summary>
        /// <param name="query">A free-form string query that will provide the data to update.</param>
        /// <returns>Self-instance.</returns>
        public SqlUpdateBuilder From(string query)
        {
            _sourceDataQuery = new StringBuilder(query);
            return this;
        }

        #endregion

        #region Joins

        /// <summary>
        /// Add a JOIN between <typeparamref name="TLeftTable"/> and <typeparamref name="TRightTable"/>.
        /// </summary>
        /// <typeparam name="TLeftTable">Type of .NET object for the LEFT table in the join</typeparam>
        /// <typeparam name="TRightTable">Type of .NET object for the RIGHT table in the join</typeparam>
        /// <param name="joinCondition">Condition to join the tables</param>
        /// <param name="type">The type of join to perform. Default: INNER JOIN</param>
        /// <returns>Self-instance</returns>
        public SqlUpdateBuilder Join<TLeftTable, TRightTable>(Expression<Func<TLeftTable, TRightTable, bool>> joinCondition, TypesOfJoinsEnum type = TypesOfJoinsEnum.Inner)
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
        public SqlUpdateBuilder InnerJoin<TLeftTable, TRightTable>(Expression<Func<TLeftTable, TRightTable, bool>> joinCondition)
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
        public SqlUpdateBuilder LeftJoin<TLeftTable, TRightTable>(Expression<Func<TLeftTable, TRightTable, bool>> joinCondition)
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
        public SqlUpdateBuilder RightJoin<TLeftTable, TRightTable>(Expression<Func<TLeftTable, TRightTable, bool>> joinCondition)
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
        public SqlUpdateBuilder FullJoin<TLeftTable, TRightTable>(Expression<Func<TLeftTable, TRightTable, bool>> joinCondition)
        {
            _joins.Add<TLeftTable, TRightTable>(joinCondition, TypesOfJoinsEnum.Full);
            return this;
        }

        /// <summary>
        /// Add a JOIN between the primary table (added via the For function) and the free-string table named by <paramref name="rightTableName"/>.
        /// </summary>
        /// <param name="rightTableName">Name of the table joining with.</param>
        /// <param name="joinCondition">Condition to join the tables</param>
        /// <param name="type">The type of join to perform. Default: INNER JOIN</param>
        /// <returns>Self-instance</returns>
        public SqlUpdateBuilder Join(string rightTableName, string joinCondition, TypesOfJoinsEnum type = TypesOfJoinsEnum.Inner)
        {
            _joins.Add(rightTableName, joinCondition, type);
            return this;
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
        public SqlUpdateBuilder Where<TTable>(Expression<Func<TTable, bool>> conditions, OperatorsToJoinConditionsEnum conditionAppendingOperator = OperatorsToJoinConditionsEnum.And)
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
        public SqlUpdateBuilder Where<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> conditions, OperatorsToJoinConditionsEnum conditionAppendingOperator = OperatorsToJoinConditionsEnum.And)
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
        public SqlUpdateBuilder OrWhere<TTable>(Expression<Func<TTable, bool>> conditions)
        {
            _where.Add<TTable>(conditions, OperatorsToJoinConditionsEnum.Or);
            return this;
        }

        /// <summary>
        /// Add a condition that helps filter the rows of the returned dataset, joining this condition to ones already added using the OR operator.
        /// </summary>
        /// <typeparam name="TTable1">Type of .NET object for object reference in condition</typeparam>
        /// <typeparam name="TTable2">Type of .NET object for object reference in condition</typeparam>
        /// <param name="conditions">One or more conditions in Lambda Expression form</param>
        /// <returns>Self-instance</returns>
        public SqlUpdateBuilder OrWhere<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> conditions)
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
        public SqlUpdateBuilder AndWhere<TTable>(Expression<Func<TTable, bool>> conditions)
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
        public SqlUpdateBuilder AndWhere<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> conditions)
        {
            _where.Add<TTable1, TTable2>(conditions, OperatorsToJoinConditionsEnum.And);
            return this;
        }

        #endregion

        #region Columns and values

        /// <summary>
        /// Values to be updated into the destination table are picked up from the provided <paramref name="obj"/>.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object acting as the datasource.</typeparam>
        /// <param name="obj">Instance of object acting as the datasource.</param>
        /// <returns>Self-instance.</returns>
        public SqlUpdateBuilder Values<TObject>(TObject obj)
        {
            ClrToTableWithAlias map = base.Map.Add<TObject>();
            object? refSource = obj;

            Dictionary<string, string> objValues = new Dictionary<string, string>();
            foreach (MemberTypeInfo member in map.TypeInfo.Members.Values)
            {
                objValues.Add(
                        $"{map.Alias}.{member.Column.CreateQualifiedName()}",
                        ReflectionUtils.GetSQLStringValue(Core.ReflectionUtils.GetValue(ref refSource, member))
                    );
            }

            _columnsWithValues.Add(objValues);

            return this;
        }

        /// <summary>
        /// Values to be updated into the destination table are picked up from the provided <paramref name="objList"/>.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object acting as the datasource.</typeparam>
        /// <param name="objList">A collection of objects acting as the datasource.</param>
        /// <returns>Self-instance.</returns>
        public SqlUpdateBuilder Values<TObject>(IEnumerable<TObject> objList)
        {
            ClrToTableWithAlias map = base.Map.Add<TObject>();
            Dictionary<string, string> objValues = new Dictionary<string, string>();

            /*
             * There are two approaches to extracting column names here:
             * 
             *  1. Extract them in a separate for-loop over map.TypeInfo.Members.Values as we only need the reflected members' TypeInfos.
             *  
             *  2. Do them inside the main for (over objList).
             *  
             *  We are preferring the #2 method here as it also allows us to deal with dynamic objects (like TObject = a generic TableEntities) 
             *  that *may* have differing property/field information between each instance of the same base type.
             * 
             */

            foreach (TObject obj in objList)
            {
                object? refSource = obj;
                foreach (MemberTypeInfo member in map.TypeInfo.Members.Values)
                {
                    objValues.Add(
                            $"{map.Alias}.{member.Column.CreateQualifiedName()}",
                            ReflectionUtils.GetSQLStringValue(Core.ReflectionUtils.GetValue(ref refSource, member))
                        );
                }
                _columnsWithValues.Add(objValues);
                objValues.Clear();
            }

            return this;
        }

        /// <summary>
        /// Values to be updated into the destination table are picked up from the provided <paramref name="objList"/>.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object acting as the datasource.</typeparam>
        /// <param name="objList">A collection of objects acting as the datasource.</param>
        /// <returns>Self-instance.</returns>
        public SqlUpdateBuilder Values<TObject>(params TObject[] objList)
        {
            return Values<TObject>((IEnumerable<TObject>)objList);
        }

        /// <summary>
        /// Values to be updated into the destination table are picked up from the provided <paramref name="values"/> dictionary.
        /// </summary>
        /// <param name="values">An arbitrary dictionary containing name/value pairs.</param>
        /// <returns>Self-instance.</returns>
        public SqlUpdateBuilder Values(Dictionary<string, object?> values)
        {
            Dictionary<string, string> objValues = new Dictionary<string, string>();
            foreach (KeyValuePair<string, object?> kvp in values)
            {
                objValues.Add(kvp.Key, ReflectionUtils.GetSQLStringValue(kvp.Value));
            }
            _columnsWithValues.Add(objValues);
            return this;
        }

        #endregion

        #region Primary Clauses

        /// <summary>
        /// Specify one or more table hints. You may call this method multiple times to add multiple table hints OR specify all the hints at once 
        /// using the OR pattern.
        /// </summary>
        /// <param name="tableHints">Table hints to specify: may be a single value, or OR'ed with other values.</param>
        /// <returns>Self-instance.</returns>
        public SqlUpdateBuilder With(SqlTableHints tableHints)
        {
            // add only those that are not already added.
            foreach (SqlTableHints hint in Enum.GetValues<SqlTableHints>())
            {
                if (tableHints.HasFlag(hint) && (!_tableHints.HasFlag(hint)))
                {
                    _tableHints |= hint;
                }
            }

            return this;
        }

        /// <summary>
        /// Set this statement to update only <paramref name="count"/> number of rows.
        /// </summary>
        /// <param name="count">Zero is a valid value.</param>
        /// <param name="percent">If true, <paramref name="count"/> is a percent value, returned rounded up to the next whole number.</param>
        /// <returns>Instance of self.</returns>
        public SqlUpdateBuilder Top(uint count, bool percent = false)
        {
            _topCount = count;
            _topIsPercent = percent;
            return this;
        }

        /// <summary>
        /// Specify that the provided source dataset be updated into this table.
        /// </summary>
        /// <typeparam name="TTable">Type of .NET object mapped to the destination table.</typeparam>
        /// <returns>A newly created instance of a SqlUpdateBuilder.</returns>
        public static SqlUpdateBuilder For<TTable>()
        {
            SqlUpdateBuilder builder = new SqlUpdateBuilder();
            ClrToTableWithAlias map = builder.Map.Add<TTable>(isPrimaryTable: true);
            builder._destinationTableName = map.QualifiedTableName;
            return builder;
        }

        /// <summary>
        /// Specify that the provided source dataset be updated into this table.
        /// </summary>
        /// <param name="primaryTable">Type of .NET object mapped to the destination table.</param>
        /// <returns>A newly created instance of a SqlUpdateBuilder.</returns>
        public static SqlUpdateBuilder For(Type primaryTable)
        {
            SqlUpdateBuilder builder = new SqlUpdateBuilder();
            ClrToTableWithAlias map = builder.Map.Add(primaryTable, isPrimaryTable: true);
            builder._destinationTableName = map.QualifiedTableName;
            return builder;
        }

        /// <summary>
        /// Specify that the provided source dataset be updated into this table.
        /// </summary>
        /// <param name="destinationTableName">Free-form name of the destination table.</param>
        /// <returns>A newly created instance of a SqlUpdateBuilder.</returns>
        public static SqlUpdateBuilder For(string destinationTableName)
        {
            SqlUpdateBuilder builder = new SqlUpdateBuilder
            {
                _destinationTableName = destinationTableName
            };

            return builder;
        }

        #endregion

        /// <summary>
        /// Private constructor.
        /// </summary>
        private SqlUpdateBuilder()
            : base()
        {
            _sourceDataQuery = null;
            _destinationTableName = null;
            _tableHints = SqlTableHints.None;
            _topCount = uint.MaxValue;
            _topIsPercent = false;
            _columnsWithValues = new List<Dictionary<string, string>>();
            _joins = new SqlJoin(base.Map);
            _where = new SqlWhere(base.Map);
        }

        private string? _destinationTableName;
        private StringBuilder? _sourceDataQuery;
        private SqlTableHints _tableHints;
        private uint _topCount = uint.MaxValue;
        private bool _topIsPercent = false;
        private readonly List<Dictionary<string, string>> _columnsWithValues;
        private readonly SqlJoin _joins;
        private readonly SqlWhere _where;
    }
}
