using System.Data;

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// Results from executing a (SELECT) query that returns data.
    /// </summary>
    public class QueryResult : ExecutionResult
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
        public DataSet Data
        {

            get; set;

        } = new DataSet();

    }
}
