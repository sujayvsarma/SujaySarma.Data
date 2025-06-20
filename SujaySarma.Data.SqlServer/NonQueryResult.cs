namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// The results from executing a non-query operation.
    /// </summary>
    public class NonQueryResult : ExecutionResult
    {
        /// <summary>
        /// The original command text.
        /// </summary>
        public string Text
        {
            get; set;

        } = default!;

        /// <summary>
        /// Number of rows affected.
        /// </summary>
        public int RowsAffected
        {
            get; set;

        } = 0;


    }
}
