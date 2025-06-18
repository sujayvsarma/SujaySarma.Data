namespace SujaySarma.Data.SqlServer.Fluid
{
    /// <summary>Fluid builder for DELETE FROM statements.</summary>
    /// <typeparam name="TTable">Type of business object mapped to the table being deleted from</typeparam>
    public class SqlDeleteBuilder<TTable> : SqlFluidStatementBuilder
    {

        /// <summary>
        /// Finalise and build the DELETE FROM WHERE statement
        /// </summary>
        /// <returns>Sql DELETE statement</returns>
        public override string Build()
        {
            return string.Join(' ',
                    "DELETE FROM",
                    TypeTableMap.GetPrimaryTable().GetQualifiedTableName(),
                    Joins.ToString(' '),
                    (Where.HasItems ? "WHERE " + Where.ToString(string.Empty) : string.Empty)
                );
        }


        /// <summary>
        /// Collection of WHERE conditions
        /// </summary>
        public SqlWhere Where { get; init; }

        /// <summary>
        /// Collection of JOIN clauses
        /// </summary>
        public SqlJoin Joins { get; init; }

        /// <summary>
        /// Register the table to delete from
        /// </summary>
        /// <returns>Created instance of SqlDeleteBuilder</returns>
        public static SqlDeleteBuilder<TTable> Begin() 
            => new SqlDeleteBuilder<TTable>();


        /// <inheritdoc />
        private SqlDeleteBuilder()
        {
            TypeTableMap.TryAdd<TTable>(true);
            Where = new SqlWhere(TypeTableMap);
            Joins = new SqlJoin(TypeTableMap);
        }
    }
}
