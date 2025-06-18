using SujaySarma.Data.SqlServer.Fluid.Constants;
using SujaySarma.Data.SqlServer.Fluid.AliasMaps;

using System;
using System.Linq.Expressions;

namespace SujaySarma.Data.SqlServer.Fluid
{
    /// <summary>
    /// A collection of table JOINs. Enumeration will yield a fully parsed JOIN clause as a STRING that can be
    /// plugged into a SQL query/statement.
    /// </summary>

    public class SqlJoin : SqlClauseCollection
    {
        /// <summary>Register a JOIN between two objects/tables.</summary>
        /// <param name="tableName">Actual name of the table</param>
        /// <param name="condition">Fully formed condition (ON clause) to join the tables. This is neither checked nor parsed</param>
        /// <param name="joinType">The type of join to perform.</param>
        /// <returns>Self-instance</returns>
        public SqlJoin Add(string tableName, string condition, TypesOfJoinsEnum joinType = TypesOfJoinsEnum.Inner)
        {
            string joinName;
            switch (joinType)
            {
                case TypesOfJoinsEnum.Left:
                    joinName = "LEFT JOIN";
                    break;

                case TypesOfJoinsEnum.Right:
                    joinName = "RIGHT JOIN";
                    break;

                case TypesOfJoinsEnum.Full:
                    joinName = "FULL JOIN";
                    break;

                default:
                    joinName = "INNER JOIN";
                    break;
            }

            string? tableNameOrAlias = base.Maps.GetAliasIfDefined(tableName);
            tableNameOrAlias ??= $"j{base.Count}";

            return (SqlJoin)base.Add($"{joinName} [{tableNameOrAlias}] WITH (NOLOCK) ON {condition}");
        }

        /// <summary>Register a JOIN between two objects/tables.</summary>
        /// <typeparam name="TLeft">Type of CLR object for the LEFT table in the join</typeparam>
        /// <typeparam name="TRight">Type of CLR object for the RIGHT table in the join</typeparam>
        /// <param name="onCondition">Condition to join the tables</param>
        /// <param name="joinType">The type of join to perform. Default: INNER JOIN</param>
        /// <returns>Self-instance</returns>
        public SqlJoin Add<TLeft, TRight>(Expression<Func<TLeft, TRight, bool>> onCondition, TypesOfJoinsEnum joinType = TypesOfJoinsEnum.Inner)
        {
            if (base.Maps.GetMap<TLeft>() == null)
            {
                throw new ArgumentOutOfRangeException(nameof(TLeft), "The 'Left' marked table-object should have already been added, or must be a non-SQL persisted object like a runtime variable or constant.");
            }
            base.Maps.TryAdd<TRight>();
            TypeTableAliasMap rightTableMap = base.Maps.GetMap<TRight>()!;
            string conditionSql = base.ExpressionToSQL((Expression)onCondition);
            string joinName;
            switch (joinType)
            {
                case TypesOfJoinsEnum.Left:
                    joinName = "LEFT JOIN";
                    break;

                case TypesOfJoinsEnum.Right:
                    joinName = "RIGHT JOIN";
                    break;

                case TypesOfJoinsEnum.Full:
                    joinName = "FULL JOIN";
                    break;

                default:
                    joinName = "INNER JOIN";
                    break;
            }

            return (SqlJoin)base.Add($"{joinName} [{rightTableMap.Discovery.Container.CreateQualifiedName()}] [{rightTableMap.Alias}] WITH (NOLOCK) ON {conditionSql}");
        }

        /// <summary>
        /// Create the collection. Only accessible to our internal query builders.
        /// </summary>
        /// <param name="tableMap">Current map of tables</param>
        internal SqlJoin(TypeTableAliasMapCollection tableMap)
            : base(tableMap)
        {
        }
    }
}
