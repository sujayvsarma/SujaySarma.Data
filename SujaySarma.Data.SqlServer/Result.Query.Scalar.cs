namespace SujaySarma.Data.SqlServer;

/// <summary>
/// Provides the results from the execution of a query (eg: Query) statement that 
/// returns a single field/column of data.
/// </summary>
public class QueryScalarResult : Result
{
    /// <summary>
    /// The returned column data. May be NULL.
    /// </summary>
    public object? Data
    {
        get; private set;
    }

    /// <summary>
    /// Instantiate the QueryScalarResult structure.
    /// </summary>
    /// <param name="queryOrCommand">The query or command that was executed.</param>
    /// <param name="data">The returned column data. May be NULL.</param>
    public QueryScalarResult(string queryOrCommand, object? data)
        : base(queryOrCommand)
    {
        Data = data;
    }
}


