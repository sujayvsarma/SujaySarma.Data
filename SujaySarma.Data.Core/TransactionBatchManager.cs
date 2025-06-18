
using System.Collections.Generic;
using System.Linq;

namespace SujaySarma.Data.Core
{
    /// <summary>
    /// Manages sets of data for a transaction
    /// </summary>
    /// <typeparam name="TEntity">Type of the data entity</typeparam>
    public sealed class TransactionBatchManager<TEntity>
    {
        /// <summary>
        /// Collection of all entities added to be "batch-managed"
        /// </summary>
        private readonly Queue<TEntity> _allEntities;

        /// <summary>
        /// The maximum number of items permitted in a batch
        /// </summary>
        private readonly int _batchSize;

        /// <summary>
        /// Returns the number of items left in the collection
        /// </summary>
        public int ItemsLeft 
            => _allEntities.Count;

        /// <summary>
        /// Get the next set of items for the transaction
        /// </summary>
        /// <returns>Items in the batch</returns>
        public List<TEntity> GetNext()
        {
            List<TEntity> next = new List<TEntity>();
            if (_allEntities.Count > 0)
            {
                if (_allEntities.Count <= _batchSize)
                {
                    next = _allEntities.ToList();
                    _allEntities.Clear();
                }
                else
                {
                    int ptr = 0;
                    while ((ptr < _batchSize) && (_allEntities.Count > 0))
                    {
                        next.Add(_allEntities.Dequeue());
                        ptr++;
                    }
                }
            }
            return next;
        }

        /// <summary>
        /// Add an item to the entities list
        /// </summary>
        /// <param name="entity">Entity to add to the transactional batch</param>
        public void Add(TEntity entity) 
            => _allEntities.Enqueue(entity);

        /// <summary>
        /// Clear all items from the entities list.
        /// </summary>
        public void Clear() 
            => _allEntities.Clear();


        /// <summary>
        /// Initialise
        /// </summary>
        /// <param name="batchSize">The maximum number of items permitted in a batch</param>
        /// <param name="entities">The collection of entities to be added</param>
        public TransactionBatchManager(int batchSize, IEnumerable<TEntity> entities)
        {
            _batchSize = batchSize;
            _allEntities = new Queue<TEntity>(entities);
        }

        /// <summary>
        /// Initialise
        /// </summary>
        /// <param name="batchSize">The maximum number of items permitted in a batch</param>
        public TransactionBatchManager(int batchSize)
        {
            _batchSize = batchSize;
            _allEntities = new Queue<TEntity>();
        }
    }
}
