using System.Collections.Generic;
using System.Data;

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// The results from execution of a stored procedure or SQL function.
    /// </summary>
    public class ProcedureOrFunctionResult : ExecutionResult
    {
        /// <summary>
        /// Name of the stored procedure or function executed
        /// </summary>
        public string Name 
        { 
            get; set; 
        
        } = default!;

        /// <summary>
        /// Return value parameters
        /// </summary>
        public Dictionary<string, object?> ReturnParameters 
        { 
            get; set; 
        
        } = new Dictionary<string, object?>();

        /// <summary>
        /// Return value of stored procedure or function
        /// </summary>
        public int ReturnValue
        {
            get; set;

        } = 0;

        /// <summary>
        /// Result data set
        /// </summary>
        public DataSet Data
        {

            get; set;

        } = new DataSet();
    }
}
