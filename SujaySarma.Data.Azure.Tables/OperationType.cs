namespace SujaySarma.Data.Azure.Tables
{
    /// <summary>
    /// Type of CRUD operation to be performed
    /// </summary>
    public enum OperationType
    {
        /// <summary>
        /// Unknown operation
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Insert new rows
        /// </summary>
        Insert,

        /// <summary>
        /// Update existing rows
        /// </summary>
        Update,

        /// <summary>
        /// Insert or update rows
        /// </summary>
        Upsert,

        /// <summary>
        /// Delete existing rows
        /// </summary>
        Delete
    }
}
