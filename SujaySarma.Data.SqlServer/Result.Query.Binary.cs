namespace SujaySarma.Data.SqlServer;

/// <summary>
/// Provides the results from the execution of a query (eg: Query) statement that 
/// returns binary data (like the contents of a DB table stored file).
/// </summary>
public class QueryBinaryResult : Result
{
    /// <summary>
    /// The returned binary data. May be zero or more bytes in length.
    /// </summary>
    public byte[] Data
    {
        get; private set;
    }

    /// <summary>
    /// Instantiate the QueryBinaryResult structure.
    /// </summary>
    /// <param name="queryOrCommand">The query or command that was executed.</param>
    /// <param name="data">The returned binary data. May be zero or more bytes in length.</param>
    public QueryBinaryResult(string queryOrCommand, byte[] data)
        : base(queryOrCommand)
    {
        Data = data;
    }
}


