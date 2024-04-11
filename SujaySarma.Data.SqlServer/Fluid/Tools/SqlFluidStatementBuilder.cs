using System;
using System.Linq.Expressions;

using SujaySarma.Data.SqlServer.LinqParsers;

namespace SujaySarma.Data.SqlServer.Fluid.Tools
{
    /// <summary>
    /// Base class implemented by our fluid statement builders
    /// </summary>
    public abstract class SqlFluidStatementBuilder
    {

        /// <summary>
        /// The table map
        /// </summary>
        internal TypeTableAliasMapCollection TypeTableMap
        {
            get;
        }

        /// <summary>
        /// Build the statement as a SQL
        /// </summary>
        /// <returns>SQL statement string</returns>
        public virtual string Build()
        {
            throw new NotImplementedException("Ouch! Someone wrote a Fluid-statement builder but forgot to implement the Build() function!");
        }

        /// <summary>
        /// A helper function to parse lambda expressions to SQL
        /// </summary>
        /// <param name="expression">Lambda expression to parse</param>
        /// <param name="treatAssignmentsAsAlias">[Optional] When set, tells the parser to treat any assignments in the expression as aliases. For eg: 'a = s.Id' will turn into 's.Id as [a]'</param>
        /// <returns>SQL string expression</returns>
        protected string ParseToSql(Expression expression, bool treatAssignmentsAsAlias = false)
        {
            SqlLambdaVisitor parser = new SqlLambdaVisitor(TypeTableMap);
            return parser.ParseToSql(expression, treatAssignmentsAsAlias);
        }

        /// <summary>
        /// Initialize. Only child classes are allowed to call me.
        /// </summary>
        protected SqlFluidStatementBuilder()
        {
            TypeTableMap = new TypeTableAliasMapCollection();
        }
    }
}
