using System;
using System.Linq.Expressions;

namespace SujaySarma.Data.SqlServer.Builders
{
    /// <summary>
    /// Represents a collection of table JOINs. Enumeration will yield a fully parsed JOIN clause as a STRING that can be added to a SQL query/statement.
    /// </summary>
    public sealed class SqlJoin : SqlClauseCollection
    {
        /// <summary>
        /// Register a JOIN between two objects/tables.
        /// </summary>
        /// <typeparam name="TLeftTable">Type of CLR object for the LEFT table in the join</typeparam>
        /// <typeparam name="TRightTable">Type of CLR object for the RIGHT table in the join</typeparam>
        /// <param name="joinCondition">Condition to join the tables</param>
        /// <param name="type">The type of join to perform. Default: INNER JOIN</param>
        /// <returns>Self-instance</returns>
        public SqlJoin Add<TLeftTable, TRightTable>(Expression<Func<TLeftTable, TRightTable, bool>> joinCondition, TypesOfJoinsEnum type = TypesOfJoinsEnum.Inner)
        {
            if (base.Maps.Get<TLeftTable>() == null)
            {
                throw new ArgumentOutOfRangeException(nameof(TLeftTable), "The 'Left' marked table-object should have already been added, or must be a non-SQL persisted object like a runtime variable or constant.");
            }

            ClrToTableWithAlias rightTable = base.Maps.Add<TRightTable>();
            string conditionSql = base.ExpressionToSQL(joinCondition);
            base.Add($"{type.ToSqlString()} {rightTable.QualifiedTableNameWithAlias} WITH (NOLOCK) ON {conditionSql}");

            return this;
        }


        /// <summary>
        /// Register a JOIN between two objects/tables.
        /// </summary>
        /// <param name="rightTableName">Name of the RHS table.</param>
        /// <param name="joinCondition">Condition to join the tables</param>
        /// <param name="type">The type of join to perform. Default: INNER JOIN</param>
        /// <returns>Self-instance</returns>
        public SqlJoin Add(string rightTableName, string joinCondition, TypesOfJoinsEnum type = TypesOfJoinsEnum.Inner)
        {
            rightTableName = base.Maps.GetNameWithAlias(rightTableName) ?? $"[{rightTableName}]";
            base.Add($"{type.ToSqlString()} {rightTableName} WITH (NOLOCK) ON {joinCondition}");
            return this;
        }


        /// <inheritdoc />
        public override string ToString()
            => base.ToString(' ');


        /// <summary>
        /// Initialise the collection.
        /// </summary>
        /// <param name="map">The map of .NET objects to SQL tables</param>
        public SqlJoin(ClrToTableWithAliasCollection map)
            : base(map)
        {
        }
    }
}
