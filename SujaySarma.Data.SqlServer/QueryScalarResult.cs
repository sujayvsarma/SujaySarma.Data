namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// Results from executing a scalar (SELECT) query that returns only a single column of data.
    /// </summary>
    public class QueryScalarResult<TType> : ExecutionResult
    {
        /// <summary>
        /// The original query.
        /// </summary>
        public string Query
        {
            get; set;

        } = default!;


        /// <summary>
        /// Result data set
        /// </summary>
        public TType? Data
        {

            get; set;

        } = default(TType);

    }
}
