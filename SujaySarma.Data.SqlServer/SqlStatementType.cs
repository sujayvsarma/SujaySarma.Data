namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// Type of SQL statement
    /// </summary>
    public enum SqlStatementType
    {
        /*
        /// <summary>
        /// CREATE
        /// </summary>
        Create,

        /// <summary>
        /// ALTER
        /// </summary>
        Alter,

        /// <summary>
        /// DROP
        /// </summary>
        Drop,

        /// <summary>
        /// EXEC
        /// </summary>
        Exec,

        /// <summary>
        /// SELECT
        /// </summary>
        Select,
        */

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
        Delete
    }
}
