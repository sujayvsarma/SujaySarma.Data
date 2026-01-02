using System.Collections.Generic;
using System.Data;

namespace SujaySarma.Data.SqlServer;

/// <summary>
/// Provides the results of executing a stored procedure or 
/// SQL Function on SQL Server.
/// </summary>
public class ProcedureOrFunctionResult : Result
{
    /// <summary>
    /// Parameters sent to the procedure or function.
    /// </summary>
    public Dictionary<string, object?>? InputParameters
    {
        get; init;
    }

    /// <summary>
    /// Parameters returned (output) from the procedure or function.
    /// </summary>
    public Dictionary<string, object?>? ReturnParameters
    {
        get; init;
    }

    /// <summary>
    /// The value returned by the procedure or function (RETURN).
    /// </summary>
    public int ReturnValue
    {
        get; init;
    }

    /// <summary>
    /// Data returned by the procedure or function.
    /// </summary>
    public DataSet? Data
    {
        get; set;
    }

    /// <summary>
    /// Initialise the ProcedureOrFunctionResult structure.
    /// </summary>
    /// <param name="queryOrCommand">The query or command that was executed.</param>
    public ProcedureOrFunctionResult(string queryOrCommand) 
        : base(queryOrCommand)
    {
    }
}