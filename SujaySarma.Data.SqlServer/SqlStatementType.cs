namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// Type of SQL statement
    /// </summary>
    public enum SqlStatementType
    {
        /// <summary>
        /// INSERT
        /// </summary>
        Insert,

        /// <summary>
        /// UPDATE
        /// </summary>
        Update,

        /// <summary>
        /// Upsert or MERGE
        /// </summary>
        Upsert,

        /// <summary>
        /// DELETE
        /// </summary>
        Delete,

        /// <summary>
        /// SELECT
        /// </summary>
        Query
    }
}
