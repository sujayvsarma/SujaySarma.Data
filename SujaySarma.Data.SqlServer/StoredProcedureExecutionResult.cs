using System;
using System.Collections.Generic;

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// Result of a stored procedure (or function) execution
    /// </summary>
    public class StoredProcedureExecutionResult
    {
        /// <summary>
        /// Is an error?
        /// </summary>
        public bool IsError { get; set; } = false;

        /// <summary>
        /// Any messages including error messages
        /// </summary>
        public string? Messages { get; set; }

        /// <summary>
        /// If <see cref="IsError"/> is 'true', then this would have the Exception that had been thrown
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// Result data set
        /// </summary>
        public System.Data.DataSet? Results { get; set; }

        /// <summary>
        /// Procedure run
        /// </summary>
        public string ProcedureName { get; set; } = default!;

        /// <summary>
        /// Result parameters
        /// </summary>
        public Dictionary<string, object?> ReturnParameters { get; set; } = new();

        /// <summary>
        /// Return value of sproc
        /// </summary>
        public int ReturnValue { get; set; } = 0;
    }
}
