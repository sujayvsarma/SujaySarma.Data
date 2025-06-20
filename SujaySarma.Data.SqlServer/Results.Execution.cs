using System.Collections.Generic;

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// The results from executing a query, function or stored procedure.
    /// </summary>
    public class ExecutionResult
    {

        /// <summary>
        /// Is an error?
        /// </summary>
        public bool IsError
        {
            get; set;

        } = false;

        /// <summary>
        /// Any messages including error messages
        /// </summary>
        public List<string> Messages
        {
            get; set;

        } = new List<string>();
    }
}
