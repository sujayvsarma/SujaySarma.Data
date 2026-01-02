using System;
using System.Collections;

namespace SujaySarma.Data.Core.Transactions;

/// <summary>
/// Provides access to the added collection of untyped items in sizes of pre-configured length.
/// </summary>
public sealed class BatchCollection
{
    /// <summary>
    /// Retrieve the next set of items for a batch. If the number of waiting items exceeds the configured 
    /// batch size, we only the batch size number of items. Otherwise, all remaining items are returned.
    /// </summary>
    /// <returns>Yield-returned enumeration of elements.</returns>
    public IEnumerable GetNext()
    {
        uint thisBatchLength = ((_queue.Count > _batchSize) ? _batchSize : (uint)_queue.Count);
        uint batchElement = 0;
        while (batchElement < thisBatchLength)
        {
            ++batchElement;
            yield return _queue.Dequeue();
        }
    }

    /// <summary>
    /// Returns the number of items added to the collection.
    /// </summary>
    public int Count
        => _queue.Count;

    /// <summary>
    /// Clears all items from the collection.
    /// </summary>
    /// <returns>Instance of self.</returns>
    public BatchCollection Clear()
    {
        _queue.Clear();
        return this;
    }

    /// <summary>
    /// Adds the specified entity to the batch collection.
    /// </summary>
    /// <param name="entity">The entity to add to the batch. Cannot be NULL.</param>
    /// <returns>Instance of self.</returns>
    public BatchCollection Add(object entity)
    {
        if (entity is null)
        {
            throw new ArgumentNullException(nameof(entity), "Entity cannot be NULL or default.");
        }

        _queue.Enqueue(entity);
        return this;
    }

    /// <summary>
    /// Initialise the batch.
    /// </summary>
    /// <param name="batchSize">Size of a batch. The collection will return this many items per fetch.</param>
    public BatchCollection(uint batchSize)
    {
        if (batchSize == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(batchSize), "Size of a batch cannot be zero.");
        }

        _batchSize = batchSize;
        _queue = new Queue();
    }

    private uint _batchSize;
    private readonly Queue _queue;
}
