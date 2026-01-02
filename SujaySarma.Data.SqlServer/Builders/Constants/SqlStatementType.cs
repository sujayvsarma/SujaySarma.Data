namespace SujaySarma.Data.SqlServer.Builders.Constants;

/// <summary>
/// Type of statement
/// </summary>
internal enum SqlStatementType
{
    /// <summary>
    /// Query
    /// </summary>
    Query = 1,

    /// <summary>
    /// Insert
    /// </summary>
    Insert = 2,

    /// <summary>
    /// UpdateMany
    /// </summary>
    Update = 4,

    /// <summary>
    /// Insert
    /// </summary>
    Delete = 8,

    /// <summary>
    /// Merge
    /// </summary>
    Merge = 16
}
