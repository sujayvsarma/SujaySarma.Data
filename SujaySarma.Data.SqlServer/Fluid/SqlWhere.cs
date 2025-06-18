using SujaySarma.Data.SqlServer.Fluid.Constants;
using SujaySarma.Data.SqlServer.Fluid.AliasMaps;

using System;
using System.Linq.Expressions;

namespace SujaySarma.Data.SqlServer.Fluid
{
    /// <summary>
    /// A collection of table WHERE conditions. 'ToString()' will yield a fully parsed WHERE condition as a STRING that can be
    /// plugged into a SQL query/statement.
    /// </summary>
    public class SqlWhere : SqlClauseCollection
    {
        /// <summary>Register a WHERE clause condition</summary>
        /// <typeparam name="TTable">Type of CLR object for object reference in condition</typeparam>
        /// <param name="conditions">One or more conditions in Lambda Expression form</param>
        /// <param name="conditionAppendingOperator">Operator to append the current set of conditions to the ones already added</param>
        /// <returns>Self-instance</returns>
        public SqlWhere Add<TTable>(Expression<Func<TTable, bool>> conditions, ConditionalClauseOperatorTypesEnum conditionAppendingOperator = ConditionalClauseOperatorTypesEnum.And)
        {
            base.Maps.TryAdd<TTable>();
            
            AddImpl((Expression)conditions, conditionAppendingOperator);
            return this;
        }

        /// <summary>Register a WHERE clause condition</summary>
        /// <typeparam name="TTable1">Type of CLR object for object reference in condition</typeparam>
        /// <typeparam name="TTable2">Type of CLR object for object reference in condition</typeparam>
        /// <param name="conditions">One or more conditions in Lambda Expression form</param>
        /// <param name="conditionAppendingOperator">Operator to append the current set of conditions to the ones already added</param>
        /// <returns>Self-instance</returns>
        public SqlWhere Add<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> conditions, ConditionalClauseOperatorTypesEnum conditionAppendingOperator = ConditionalClauseOperatorTypesEnum.And)
        {
            base.Maps.TryAdd<TTable1>();
            base.Maps.TryAdd<TTable2>();

            AddImpl((Expression)conditions, conditionAppendingOperator);
            return this;
        }

        /// <summary>
        /// Implementation function for the Add() overloads
        /// </summary>
        /// <param name="condition">Condition to apply</param>
        /// <param name="conditionJoiningOperator">The joining operator</param>
        private void AddImpl(Expression condition, ConditionalClauseOperatorTypesEnum conditionJoiningOperator)
        {
            if (base.HasItems)
            {
                Add(((conditionJoiningOperator != ConditionalClauseOperatorTypesEnum.Or) ? " AND " : " OR "));
            }

            string sql = base.ExpressionToSQL(condition);
            if (!sql.StartsWith('('))
                sql = $"({sql})";

            Add(sql);
        }


        /// <summary>
        /// Initialise the collection. Accessible only to our builders.
        /// </summary>
        /// <param name="tableMap">Current map of tables</param>
        internal SqlWhere(TypeTableAliasMapCollection tableMap)
            : base(tableMap)
        {
        }
    }
}
