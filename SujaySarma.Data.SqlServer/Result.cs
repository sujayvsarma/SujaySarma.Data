namespace SujaySarma.Data.SqlServer;

/// <summary>
/// Provides the results of processing a query or command 
/// against SQL Server.
/// </summary>
public class Result
{
    /// <summary>
    /// The query or command that was executed.
    /// </summary>
    public string QueryOrCommand
    {
        get; private set;

    } = default!;

    /// <summary>
    /// Message strings returned by SQL Server.
    /// </summary>
    public string? Message
    {
        get; set;

    } = null;

    /// <summary>
    /// Instantiate the Result structure.
    /// </summary>
    /// <param name="queryOrCommand">The query or command that was executed.</param>
    public Result(string queryOrCommand)
    {
        QueryOrCommand = queryOrCommand;
    }
}
