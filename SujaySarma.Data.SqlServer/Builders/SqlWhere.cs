using System;
using System.Linq.Expressions;

namespace SujaySarma.Data.SqlServer.Builders
{
    /// <summary>
    /// Represents a collection of table WHERE conditions. Enumeration will yield a fully parsed WHERE clause as a STRING that can be added to a SQL query/statement.
    /// </summary>
    public sealed class SqlWhere : SqlClauseCollection
    {

        /// <summary>Register a WHERE clause condition</summary>
        /// <typeparam name="TTable">Type of CLR object for object reference in condition</typeparam>
        /// <param name="conditions">One or more conditions in Lambda Expression form</param>
        /// <param name="conditionAppendingOperator">Operator to append the current set of conditions to the ones already added</param>
        /// <returns>Self-instance</returns>
        public SqlWhere Add<TTable>(Expression<Func<TTable, bool>> conditions, OperatorsToJoinConditionsEnum conditionAppendingOperator = OperatorsToJoinConditionsEnum.And)
        {
            base.Maps.Add<TTable>();

            AddImpl(conditions, conditionAppendingOperator);
            return this;
        }

        /// <summary>Register a WHERE clause condition</summary>
        /// <typeparam name="TTable1">Type of CLR object for object reference in condition</typeparam>
        /// <typeparam name="TTable2">Type of CLR object for object reference in condition</typeparam>
        /// <param name="conditions">One or more conditions in Lambda Expression form</param>
        /// <param name="conditionAppendingOperator">Operator to append the current set of conditions to the ones already added</param>
        /// <returns>Self-instance</returns>
        public SqlWhere Add<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> conditions, OperatorsToJoinConditionsEnum conditionAppendingOperator = OperatorsToJoinConditionsEnum.And)
        {
            base.Maps.Add<TTable1>();
            base.Maps.Add<TTable2>();

            AddImpl(conditions, conditionAppendingOperator);
            return this;
        }

        /// <summary>
        /// Implementation function for the Add() overloads
        /// </summary>
        /// <param name="condition">Condition to apply</param>
        /// <param name="conditionJoiningOperator">The joining operator</param>
        private void AddImpl(Expression condition, OperatorsToJoinConditionsEnum conditionJoiningOperator)
        {
            if (base.HasItems)
            {
                Add(((conditionJoiningOperator != OperatorsToJoinConditionsEnum.Or) ? " AND " : " OR "));
            }

            string sql = base.ExpressionToSQL(condition);

            // Enclose in paranthesis :-)
            if (!sql.StartsWith('('))
                sql = $"({sql})";

            Add(sql);
        }

        /// <inheritdoc />
        public override string ToString()
            => base.ToString(' ');

        /// <summary>
        /// Initialise the collection.
        /// </summary>
        /// <param name="map">The map of .NET objects to SQL tables</param>
        public SqlWhere(ClrToTableWithAliasCollection map)
            : base(map)
        {
        }
    }
}
