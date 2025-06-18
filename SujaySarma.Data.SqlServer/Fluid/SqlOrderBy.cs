using SujaySarma.Data.SqlServer.Fluid.AliasMaps;

using System;
using System.Linq.Expressions;

namespace SujaySarma.Data.SqlServer.Fluid
{
    /// <summary>
    /// A collection of table ORDER BY clauses
    /// </summary>
    public class SqlOrderBy : SqlClauseCollection
    {

        /// <summary>Register an ORDER BY condition</summary>
        /// <typeparam name="TTable">Type of CLR object for object reference in condition</typeparam>
        /// <typeparam name="TResult">Type of CLR object</typeparam>
        /// <param name="expression">Selector for parameters of the ORDER BY</param>
        /// <param name="direction">Direction of sorting</param>
        /// <returns>Self-instance</returns>
        public SqlOrderBy Add<TTable, TResult>(Expression<Func<TTable, TResult>> expression, SortOrderEnum direction = SortOrderEnum.ASC)
        {
            base.Maps.TryAdd<TTable>();
            string column = base.ExpressionToSQL((Expression)expression);
            string order = (direction != SortOrderEnum.DESC ? "ASC" : "DESC");

            return (SqlOrderBy)base.Add($"{column} {order}");
        }

        /// <summary>
        /// Create the collection. Only accessible to our internal query builders.
        /// </summary>
        /// <param name="tableMap">Current map of tables</param>
        internal SqlOrderBy(TypeTableAliasMapCollection tableMap)
            : base(tableMap)
        {
        }
    }
}
