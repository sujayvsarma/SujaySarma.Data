using System.Collections.Generic;

namespace SujaySarma.Data.Core.Transactions;

/// <summary>
/// Results of an operation.
/// </summary>
/// <typeparam name="TEntity">The type of the entities.</typeparam>
public struct Result<TEntity>
{
    /// <summary>
    /// The total number of items in or affected by the operation.
    /// </summary>
    public int Count;

    /// <summary>
    /// The number of items for which the operation was successful.
    /// </summary>
    public int Passed;

    /// <summary>
    /// The number of items for which the operation failed.
    /// </summary>
    public readonly int Failed
        => FailedEntities.Count;

    /// <summary>
    /// Messages from the transaction executant. May include a 
    /// mix of success and failure messages.
    /// </summary>
    public List<string> Messages;

    /// <summary>
    /// A collection of entities for which the operation failed.
    /// </summary>
    public List<TEntity> FailedEntities;
}
