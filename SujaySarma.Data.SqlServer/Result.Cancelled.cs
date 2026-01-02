namespace SujaySarma.Data.SqlServer;

/// <summary>
/// Returned from async methods when the operation was cancelled.
/// </summary>
public class CancelledResult : Result
{
    /// <summary>
    /// Initialise the CancelledResult structure.
    /// </summary>
    /// <param name="queryOrCommand"></param>
    public CancelledResult(string queryOrCommand) 
        : base(queryOrCommand)
    {
    }
}