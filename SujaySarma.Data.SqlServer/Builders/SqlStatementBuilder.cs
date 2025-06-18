using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SujaySarma.Data.SqlServer.Builders
{
    /// <summary>
    /// Base class implemented by our fluid statement builders.
    /// </summary>
    public abstract class SqlStatementBuilder
    {

        /// <summary>
        /// Build the statement as a SQL.
        /// </summary>
        /// <returns>SQL statement string OR empty string if there is no valid SQL statement.</returns>
        public virtual StringBuilder Build()
            => throw new NotImplementedException("Ouch! Someone wrote a Fluid-statement builder but forgot to implement the Build() function!");

        /// <summary>
        /// A helper function to parse lambda expressions to SQL
        /// </summary>
        /// <param name="expression">Lambda expression to parse</param>
        /// <param name="treatAssignmentsAsAlias">[Optional] When set, tells the parser to treat any assignments in the expression as aliases. For eg: 'a = s.Id' will turn into 's.Id as [a]'</param>
        /// <returns>SQL string expression</returns>
        protected string ExpressionToSQL(Expression expression, bool treatAssignmentsAsAlias = false)
            => _visitor.Tour(expression, treatAssignmentsAsAlias);

        /// <summary>
        /// Copy elements from <paramref name="source"/> to <paramref name="destination"/>
        /// </summary>
        /// <param name="source">Source dictionary</param>
        /// <param name="destination">Destination dictionary. May be NULL, if so, is initialised prior to population.</param>
        protected static void CopyTo(Dictionary<string, object?> source, ref Dictionary<string, object?>? destination)
        {
            destination ??= new Dictionary<string, object?>();
            foreach(KeyValuePair<string, object?> kvp in source)
            {
                destination[kvp.Key] = kvp.Value;
            }
        }

        /// <summary>
        /// Add a mapping for the given type.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to add mapping for.</typeparam>
        /// <param name="isPrimary">Flag to set this as a primary table for this statement sequence.</param>
        protected ClrToTableWithAlias Add<TObject>(bool isPrimary = false)
            => Map.Add<TObject>(isPrimary);

        /// <summary>
        /// Add a mapping for the given type.
        /// </summary>
        /// <param name="table">Type of .NET object to add mapping for.</param>
        /// <param name="isPrimary">Flag to set this as a primary table for this statement sequence.</param>
        protected ClrToTableWithAlias Add(Type table, bool isPrimary = false)
            => Map.Add(table, isPrimary);


        /// <summary>
        /// Initialize. Only child classes are allowed to call me.
        /// </summary>
        protected SqlStatementBuilder()
        {
            Map = new ClrToTableWithAliasCollection();
            _visitor = new SqlLambdaVisitor(Map);
        }

        // Cached visitor to improve performance across ParseToSql calls.
        private readonly SqlLambdaVisitor _visitor;

        /// <summary>
        /// The table map
        /// </summary>
        protected ClrToTableWithAliasCollection Map;
    }
}
