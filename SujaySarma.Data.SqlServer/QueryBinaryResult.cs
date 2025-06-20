using System;

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// Results from executing a (SELECT) query that returns raw/binary form data.
    /// </summary>
    public class QueryBinaryResult : ExecutionResult
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
        public byte[] Data
        {

            get; set;

        } = Array.Empty<byte>();

    }
}
