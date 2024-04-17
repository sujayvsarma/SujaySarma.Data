using System;
using System.Linq.Expressions;
using System.Text;

using SujaySarma.Data.SqlServer.Fluid.Constants;
using SujaySarma.Data.SqlServer.LinqParsers;

namespace SujaySarma.Data.SqlServer.Fluid.Tools
{
    /// <summary>
    /// A collection of table WHERE conditions. 'ToString()' will yield a fully parsed WHERE condition as a STRING that can be 
    /// plugged into a SQL query/statement.
    /// </summary>
    public class SqlTableWhereConditionsCollection
    {

        /// <summary>
        /// Register a WHERE clause condition
        /// </summary>
        /// <typeparam name="TTable1">Type of CLR object for object reference in condition</typeparam>
        /// <param name="conditions">One or more conditions in Lambda Expression form</param>
        /// <param name="conditionAppendingOperator">Operator to append the current set of conditions to the ones already added</param>
        /// <returns>Self-instance</returns>
        public SqlTableWhereConditionsCollection Add<TTable1>(Expression<Func<TTable1, bool>> conditions, ConditionalClauseOperatorTypesEnum conditionAppendingOperator = ConditionalClauseOperatorTypesEnum.And)
        {
            _aliasMapCollection.TryAdd<TTable1>();
            
            AddImpl(conditions, conditionAppendingOperator);
            return this;
        }

        /// <summary>
        /// Register a WHERE clause condition
        /// </summary>
        /// <typeparam name="TTable1">Type of CLR object for object reference in condition</typeparam>
        /// <typeparam name="TTable2">Type of CLR object for object reference in condition</typeparam>
        /// <param name="conditions">One or more conditions in Lambda Expression form</param>
        /// <param name="conditionAppendingOperator">Operator to append the current set of conditions to the ones already added</param>
        /// <returns>Self-instance</returns>
        public SqlTableWhereConditionsCollection Add<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> conditions, ConditionalClauseOperatorTypesEnum conditionAppendingOperator = ConditionalClauseOperatorTypesEnum.And)
        {
            _aliasMapCollection.TryAdd<TTable1>();
            _aliasMapCollection.TryAdd<TTable2>();

            AddImpl(conditions, conditionAppendingOperator);

            return this;
        }


        /// <summary>
        /// Returns the SQL expression stored in this clause instance
        /// </summary>
        /// <returns></returns>
        public override string ToString() => _whereConditions.ToString();


        /// <summary>
        /// Returns if there are any conditions present
        /// </summary>
        public bool HasConditions => _whereConditions.Length > 0;


        // Implementation for Where condition
        private void AddImpl(Expression condition, ConditionalClauseOperatorTypesEnum conditionJoiningOperator)
        {
            SqlLambdaVisitor parser = new SqlLambdaVisitor(_aliasMapCollection);
            if (_whereConditions.Length > 0)
            {
                _whereConditions.Append(
                        conditionJoiningOperator switch
                        {
                            ConditionalClauseOperatorTypesEnum.Or => " OR ",
                            _ => " AND "
                        }
                    );
            }

            string sql = parser.ParseToSql(condition);
            if (!sql.StartsWith('('))
            {
                sql = $"({sql})";
            }
            _whereConditions.Append(sql);
        }


        /// <summary>
        /// Create the collection. Only accessible to our internal query builders
        /// </summary>
        internal SqlTableWhereConditionsCollection(TypeTableAliasMapCollection aliasMapCollection)
        {
            _whereConditions = new StringBuilder();
            _aliasMapCollection = aliasMapCollection;
        }

        private readonly TypeTableAliasMapCollection _aliasMapCollection;
        private readonly StringBuilder _whereConditions;
    }
}
