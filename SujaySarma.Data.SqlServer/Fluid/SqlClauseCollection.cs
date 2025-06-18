using SujaySarma.Data.SqlServer.Fluid.AliasMaps;
using SujaySarma.Data.SqlServer.LinqParsers;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SujaySarma.Data.SqlServer.Fluid
{
    /// <summary>
    /// A collection of SQL clauses that can be used to build complex SQL queries in a fluid manner.
    /// </summary>
    public class SqlClauseCollection : IEnumerable<string>, IEnumerable
    {

        /// <summary>
        /// Returns if the collection has any items
        /// </summary>
        public bool HasItems
            => ((_clauses.Count > 0) ? true : false);

        /// <summary>
        /// Returns the count of items in this collection.
        /// </summary>
        public int Count
            => _clauses.Count;


        /// <summary>
        /// Exposes the maps collection
        /// </summary>
        internal TypeTableAliasMapCollection Maps
            => _aliasMaps;


        /// <summary>
        /// Adds the provided clause to the collection.
        /// </summary>
        /// <param name="clause">Clause to add</param>
        /// <returns>Instance of self</returns>
        protected SqlClauseCollection Add(string clause)
        {
            clause = clause.Trim();
            if (string.IsNullOrWhiteSpace(clause))
            {
                throw new ArgumentNullException(nameof(clause), "Clause cannot be null or whitespace.");
            }

            _clauses.Add(clause);
            return this;
        }

        /// <summary>
        /// Return the string equivalent of the collection, with clauses separated by the provided separator character.
        /// </summary>
        /// <param name="seperator">The seperator character.</param>
        /// <returns>String. Empty string if there are no items in the collection.</returns>
        public string ToString(char seperator = ' ')
        {
            if (! HasItems)
            {
                return string.Empty;
            }

            return string.Join(seperator, _clauses);
        }

        /// <summary>
        /// Return the string equivalent of the collection, with clauses separated by the provided separator character.
        /// </summary>
        /// <param name="seperator">The seperator character.</param>
        /// <returns>String. Empty string if there are no items in the collection.</returns>
        public string ToString(string seperator = " ")
        {
            if (! HasItems)
            {
                return string.Empty;
            }

            return string.Join(seperator, _clauses);
        }

        /// <summary>
        /// Get the Sql equivalent of the provided expression.
        /// </summary>
        /// <param name="expression">Expression to parse/convert</param>
        /// <returns>SQL string snippet</returns>
        protected string ExpressionToSQL(Expression expression)
            => _visitor.ParseToSql(expression);

        /// <summary>
        /// Initialise (protected)
        /// </summary>
        /// <param name="tableMap">The table/alias mappings</param>
        internal SqlClauseCollection(TypeTableAliasMapCollection tableMap)
        {
            _clauses = new List<string>();
            _aliasMaps = tableMap;
            _visitor = new SqlLambdaVisitor(_aliasMaps);
        }

        private readonly List<string> _clauses;
        private readonly TypeTableAliasMapCollection _aliasMaps;

        // Cached visitor to improve performance across ParseToSql calls.
        private readonly SqlLambdaVisitor _visitor;


        #region Enumerations

        /// <inheritdoc />
        public IEnumerator<string> GetEnumerator()
            => (IEnumerator<string>)_clauses.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
            => (IEnumerator)_clauses.GetEnumerator();

        #endregion
    }
}
