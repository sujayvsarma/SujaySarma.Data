using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SujaySarma.Data.SqlServer.Builders
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
        /// Clear all added elements
        /// </summary>
        public void Clear()
            => _clauses.Clear();


        /// <summary>
        /// Exposes the maps collection
        /// </summary>
        internal ClrToTableWithAliasCollection Maps
        {
            get;
            private set;
        }


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
        protected string ToString(char seperator = ' ')
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
        protected string ToString(string seperator = " ")
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
            => _visitor.Tour(expression);

        /// <summary>
        /// Initialise (protected)
        /// </summary>
        /// <param name="tableMap">The table/alias mappings</param>
        internal SqlClauseCollection(ClrToTableWithAliasCollection tableMap)
        {
            _clauses = new List<string>();

            Maps = tableMap;
            _visitor = new SqlLambdaVisitor(Maps);
        }

        private readonly List<string> _clauses;

        // Cached visitor to improve performance across ParseToSql calls.
        private readonly SqlLambdaVisitor _visitor;


        #region IEnumerable

        /// <inheritdoc />
        public IEnumerator<string> GetEnumerator()
            => (IEnumerator<string>)_clauses.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
            => (IEnumerator)_clauses.GetEnumerator();

        #endregion
    }
}
