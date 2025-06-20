using System;

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// A result that indicates an error.
    /// </summary>
    public class ErrorResult : ExecutionResult
    {
        /// <summary>
        /// If the base class's <see cref="ExecutionResult.IsError" /> is 'true', then this would have the Exception that had been thrown
        /// </summary>
        public Exception Exception
        {
            get; set;

        } = default!;

        /// <summary>
        /// If the executed script was a query, then the query command. Else, this would be a serialised form of the procedure/function called 
        /// along with all its parameters etc.
        /// </summary>
        public string Text
        {
            get; set;

        } = default!;

    }
}
