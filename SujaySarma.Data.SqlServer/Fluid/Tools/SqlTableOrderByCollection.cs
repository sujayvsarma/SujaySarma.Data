using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

using SujaySarma.Data.SqlServer.LinqParsers;

namespace SujaySarma.Data.SqlServer.Fluid.Tools
{
    /// <summary>
    /// A collection of table ORDER BY conditions.
    /// </summary>
    public class SqlTableOrderByCollection : IEnumerable<string>
    {

        /// <summary>
        /// Register an ORDER BY condition
        /// </summary>
        /// <typeparam name="TTable">Type of CLR object for object reference in condition</typeparam>
        /// <typeparam name="TResult">Type of CLR object</typeparam>
        /// <param name="selector">Selector for parameters of the ORDER BY</param>
        /// <param name="direction">Direction of sorting</param>
        /// <returns>Self-instance</returns>
        public SqlTableOrderByCollection Add<TTable, TResult>(Expression<Func<TTable, TResult>> selector, SortOrderEnum direction = SortOrderEnum.ASC)
        {
            _aliasMapCollection.TryAdd<TTable>();
            SqlLambdaVisitor parser = new SqlLambdaVisitor(_aliasMapCollection);
            string orderBy = parser.ParseToSql(selector);
            string dir = direction switch
            {
                SortOrderEnum.DESC => "DESC",
                _ => "ASC"
            };
            _orderBy.Add($"{orderBy} {dir}");

            return this;
        }

        /// <summary>
        /// Returns if there are ORDER BY items registered
        /// </summary>
        public bool HasItems => _orderBy.Count > 0;

        #region IEnumerable

        /// <summary>
        /// Get the enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        public IEnumerator<string> GetEnumerator()
            => _orderBy.GetEnumerator();

        /// <summary>
        /// Get the enumerator
        /// </summary>
        /// <returns>Enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
            => _orderBy.GetEnumerator();

        #endregion

        /// <summary>
        /// Create the collection. Only accessible to our internal query builders
        /// </summary>
        internal SqlTableOrderByCollection(TypeTableAliasMapCollection aliasMapCollection)
        {
            _aliasMapCollection = aliasMapCollection;
            _orderBy = new List<string>();
        }

        private readonly TypeTableAliasMapCollection _aliasMapCollection;
        private readonly List<string> _orderBy;
    }
}
