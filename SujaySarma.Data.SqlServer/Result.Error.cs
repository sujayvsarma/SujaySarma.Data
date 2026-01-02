using System;

namespace SujaySarma.Data.SqlServer;

/// <summary>
/// Provides context and the error returned by SQL Server 
/// in response to a query or command.
/// </summary>
public class ErrorResult : Result
{
    /// <summary>
    /// The raw exception that was returned.
    /// </summary>
    public Exception Error
    {
        get; private set;

    } = default!;

    /// <summary>
    /// Instantiate the ErrorResult structure
    /// </summary>
    /// <param name="queryOrCommand">The query or command that was executed.</param>
    /// <param name="exception">The raw exception that was returned.</param>
    public ErrorResult(string queryOrCommand, Exception exception)
        : base(queryOrCommand)
    {
        Error = exception;
    }
}


