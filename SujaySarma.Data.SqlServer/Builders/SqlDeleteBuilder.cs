using SujaySarma.Data.Core.Reflection;
using SujaySarma.Data.SqlServer.Attributes;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SujaySarma.Data.SqlServer.Builders
{
    /// <summary>
    /// Helps build a SQL DELETE statement.
    /// Supports: TOP, WITH, FROM, JOIN
    /// </summary>
    public sealed class SqlDeleteBuilder : SqlStatementBuilder
    {
        /// <summary>
        /// Assembles the DELETE statement from provided clauses and returns it as a StringBuilder instance.
        /// </summary>
        /// <returns>Instance of StringBuilder containing the assembled DELETE statement.</returns>
        public override StringBuilder Build()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("DELETE ");

            if (_topCount < uint.MaxValue)
            {
                builder.Append($"TOP {_topCount} ");
                if (_topIsPercent)
                {
                    builder.Append("PERCENT ");
                }
            }

            builder.Append($"FROM {_primaryTable.QualifiedTableNameWithAlias} ");

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
        public SqlDeleteBuilder Join<TLeftTable, TRightTable>(Expression<Func<TLeftTable, TRightTable, bool>> joinCondition, TypesOfJoinsEnum type = TypesOfJoinsEnum.Inner)
        {
            _joins.Add<TLeftTable, TRightTable>(joinCondition, type);
            return this;
        }

        /// <summary>
        /// Add a JOIN between the primary table and the free-string table named by <paramref name="rightTableName"/>.
        /// </summary>
        /// <param name="rightTableName">Name of the table joining with.</param>
        /// <param name="joinCondition">Condition to join the tables</param>
        /// <param name="type">The type of join to perform. Default: INNER JOIN</param>
        /// <returns>Self-instance</returns>
        public SqlDeleteBuilder Join(string rightTableName, string joinCondition, TypesOfJoinsEnum type = TypesOfJoinsEnum.Inner)
        {
            _joins.Add(rightTableName, joinCondition, type);
            return this;
        }

        #endregion

        #region Where

        /// <summary>
        /// Add a condition to filter the rows to delete.
        /// </summary>
        /// <typeparam name="TTable">Type of .NET object for object reference in condition</typeparam>
        /// <param name="conditions">One or more conditions in Lambda Expression form</param>
        /// <param name="conditionAppendingOperator">Operator to append the current set of conditions to the ones already added</param>
        /// <returns>Self-instance</returns>
        public SqlDeleteBuilder Where<TTable>(Expression<Func<TTable, bool>> conditions, OperatorsToJoinConditionsEnum conditionAppendingOperator = OperatorsToJoinConditionsEnum.And)
        {
            _where.Add<TTable>(conditions, conditionAppendingOperator);
            return this;
        }

        /// <summary>
        /// Add a condition to filter the rows to delete, joining this condition to ones already added using the OR operator.
        /// </summary>
        /// <typeparam name="TTable">Type of .NET object for object reference in condition</typeparam>
        /// <param name="conditions">One or more conditions in Lambda Expression form</param>
        /// <returns>Self-instance</returns>
        public SqlDeleteBuilder OrWhere<TTable>(Expression<Func<TTable, bool>> conditions)
        {
            _where.Add<TTable>(conditions, OperatorsToJoinConditionsEnum.Or);
            return this;
        }

        /// <summary>
        /// Add a condition to filter the rows to delete, joining this condition to ones already added using the AND operator.
        /// </summary>
        /// <typeparam name="TTable">Type of .NET object for object reference in condition</typeparam>
        /// <param name="conditions">One or more conditions in Lambda Expression form</param>
        /// <returns>Self-instance</returns>
        public SqlDeleteBuilder AndWhere<TTable>(Expression<Func<TTable, bool>> conditions)
        {
            _where.Add<TTable>(conditions, OperatorsToJoinConditionsEnum.And);
            return this;
        }

        /// <summary>
        /// Add a condition to delete the row specific to the provided <paramref name="obj"/> instance's data.
        /// </summary>
        /// <typeparam name="TTable">Type of .NET object for object reference in condition</typeparam>
        /// <param name="obj"></param>
        /// <param name="conditionAppendingOperator">Operator to append the current set of conditions to the ones already added</param>
        /// <returns>Self-instance</returns>
        public SqlDeleteBuilder Where<TTable>(TTable obj, OperatorsToJoinConditionsEnum conditionAppendingOperator = OperatorsToJoinConditionsEnum.And)
        {
            // Add WHERE clauses for keys in the data being updated.
            ClrToTableWithAlias map = base.Map.Get<TTable>() ?? base.Map.Add<TTable>(true);
            object? refInstance = obj;

            List<string> whereMap = new List<string>();
            foreach (MemberTypeInfo member in map.TypeInfo.Members.Values)
            {
                TableColumnAttribute? columnAttribute = member.FieldOrPropertyInfo.GetCustomAttribute<TableColumnAttribute>();
                if ((columnAttribute != null) && (columnAttribute.IsSearchKey))
                {
                    whereMap.Add($"([{columnAttribute.CreateQualifiedName()}]={ReflectionUtils.GetSQLStringValue(Core.ReflectionUtils.GetValue(ref refInstance, member))})");
                }
            }

            if (whereMap.Count > 0)
            {
                _where.Add(whereMap, conditionAppendingOperator);
            }

            return this;
        }

        #endregion

        #region Primary Clauses

        /// <summary>
        /// Specify one or more table hints.
        /// </summary>
        /// <param name="tableHints">Table hints to specify: may be a single value, or OR'ed with other values.</param>
        /// <returns>Self-instance.</returns>
        public SqlDeleteBuilder With(SqlTableHints tableHints)
        {
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
        /// Set this statement to delete only <paramref name="count"/> number of rows.
        /// </summary>
        /// <param name="count">Zero is a valid value.</param>
        /// <param name="percent">If true, <paramref name="count"/> is a percent value, returned rounded up to the next whole number.</param>
        /// <returns>Instance of self.</returns>
        public SqlDeleteBuilder Top(uint count, bool percent = false)
        {
            _topCount = count;
            _topIsPercent = percent;
            return this;
        }

        /// <summary>
        /// Specify the primary table for the DELETE statement.
        /// </summary>
        /// <typeparam name="TTable">Type of .NET object mapped to the table.</typeparam>
        /// <returns>A newly created instance of a SqlDeleteBuilder.</returns>
        public static SqlDeleteBuilder From<TTable>()
        {
            SqlDeleteBuilder builder = new SqlDeleteBuilder();
            builder._primaryTable = builder.Map.Add<TTable>(isPrimaryTable: true);
            return builder;
        }

        /// <summary>
        /// Specify the primary table for the DELETE statement.
        /// </summary>
        /// <param name="tableName">Free-form name of the table.</param>
        /// <returns>A newly created instance of a SqlDeleteBuilder.</returns>
        public static SqlDeleteBuilder From(string tableName)
        {
            SqlDeleteBuilder builder = new SqlDeleteBuilder
            {
                _primaryTable = new ClrToTableWithAlias(typeof(object), true, tableName)
            };
            return builder;
        }

        #endregion

        /// <summary>
        /// Private constructor.
        /// </summary>
        private SqlDeleteBuilder()
            : base()
        {
            _primaryTable = default!;
            _tableHints = SqlTableHints.None;
            _topCount = uint.MaxValue;
            _topIsPercent = false;
            _joins = new SqlJoin(base.Map);
            _where = new SqlWhere(base.Map);
        }

        private ClrToTableWithAlias _primaryTable;
        private SqlTableHints _tableHints;
        private uint _topCount = uint.MaxValue;
        private bool _topIsPercent = false;
        private readonly SqlJoin _joins;
        private readonly SqlWhere _where;

    }
}
