using System.Collections.Generic;

using SujaySarma.Data.SqlServer.Fluid.Tools;

namespace SujaySarma.Data.SqlServer.Fluid
{
    /// <summary>
    /// Fluid builder for DELETE FROM statements. 
    /// </summary>
    /// <typeparam name="TTable">Type of business object mapped to the table being deleted from</typeparam>
    public class SqlDeleteBuilder<TTable> : SqlFluidStatementBuilder
    {

        /// <summary>
        /// Build the DELETE FROM WHERE statement
        /// </summary>
        /// <returns>Sql DELETE statement</returns>
        public override string Build()
        {
            List<string> query = new List<string>()
            {
                "DELETE FROM",
                base.TypeTableMap.GetPrimaryTable().GetQualifiedTableName()
            };

            if (Joins.HasItems)
            {
                query.Add(string.Join(' ', Joins));
            }

            if (Where.HasConditions)
            {
                query.Add("WHERE");
                query.Add(Where.ToString());
            }

            return string.Join(' ', query);
        }

        /// <summary>
        /// Collection of WHERE conditions
        /// </summary>
        public SqlTableWhereConditionsCollection Where
        {
            get;
            init;
        }

        /// <summary>
        /// Collection of JOIN clauses
        /// </summary>
        public SqlTableJoinsCollection Joins
        {
            get;
            init;
        }

        /// <summary>
        /// Register the table to delete from
        /// </summary>
        /// <returns>Created instance of SqlDeleteBuilder</returns>
        public static SqlDeleteBuilder<TTable> Begin()
            => new SqlDeleteBuilder<TTable>();


        /// <inheritdoc />
        private SqlDeleteBuilder() : base()
        {
            base.TypeTableMap.TryAdd<TTable>(isPrimaryTable: true);
            Where = new SqlTableWhereConditionsCollection(base.TypeTableMap);
            Joins = new SqlTableJoinsCollection(base.TypeTableMap);
        }
    }
}
