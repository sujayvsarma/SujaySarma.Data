using System;
using System.Collections.Generic;

namespace SujaySarma.Data.Core.Transactions;


/// <summary>
/// Provides access to the added collection of strongly-typed items (<typeparamref name="TEntity"/>) in sizes of pre-configured length.
/// </summary>
/// <typeparam name="TEntity">Type of entities added to this collection.</typeparam>
public sealed class BatchCollection<TEntity>
{
    /// <summary>
    /// Retrieve the next set of items for a batch. If the number of waiting items exceeds the configured 
    /// batch size, we only the batch size number of items. Otherwise, all remaining items are returned.
    /// </summary>
    /// <returns>Yield-returned enumeration of elements.</returns>
    public IEnumerable<TEntity> GetNext()
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
    public BatchCollection<TEntity> Clear()
    {
        _queue.Clear();
        return this;
    }

    /// <summary>
    /// Adds the specified entity to the batch collection.
    /// </summary>
    /// <param name="entity">The entity to add to the batch. Cannot be NULL.</param>
    /// <returns>Instance of self.</returns>
    public BatchCollection<TEntity> Add(TEntity entity)
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
        _queue = new Queue<TEntity>();
    }

    private uint _batchSize;
    private readonly Queue<TEntity> _queue;
}