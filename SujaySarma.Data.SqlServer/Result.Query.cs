using System.Data;

namespace SujaySarma.Data.SqlServer;

/// <summary>
/// Provides the results from the execution of a query (eg: Query) statement.
/// </summary>
public class QueryResult : Result
{
    /// <summary>
    /// The returned dataset. May contain zero or more tables.
    /// </summary>
    public DataSet Data
    {
        get; private set;
    }

    /// <summary>
    /// Instantiate the QueryResult structure.
    /// </summary>
    /// <param name="queryOrCommand">The query or command that was executed.</param>
    /// <param name="data">The returned dataset. May contain zero or more tables.</param>
    public QueryResult(string queryOrCommand, DataSet data)
        : base(queryOrCommand)
    {
        Data = data;
    }
}


