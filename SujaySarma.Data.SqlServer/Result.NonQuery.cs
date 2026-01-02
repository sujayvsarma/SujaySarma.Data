namespace SujaySarma.Data.SqlServer;

/// <summary>
/// Provides the results from the execution of a non-query (eg: Insert) statement.
/// </summary>
public class NonQueryResult : Result
{
    /// <summary>
    /// Number of rows affected by the command.
    /// </summary>
    public int RowsAffected
    {
        get; private set;
    }

    /// <summary>
    /// Instantiate the NonQueryResult structure.
    /// </summary>
    /// <param name="queryOrCommand">The query or command that was executed.</param>
    /// <param name="rowsAffected">Number of rows affected by the command.</param>
    public NonQueryResult(string queryOrCommand, int rowsAffected)
        : base(queryOrCommand)
    {
        RowsAffected = rowsAffected;
    }
}


