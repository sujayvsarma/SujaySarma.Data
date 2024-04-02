using System.Collections.Generic;

namespace SujaySarma.Data.Core
{
    /// <summary>
    /// Manages sets of data for a transaction
    /// </summary>
    /// <typeparam name="TEntity">Type of the data entity</typeparam>
    public class TransactionBatchManager<TEntity>
    {
        /// <summary>
        /// Returns the number of items left in the collection
        /// </summary>
        public int ItemsLeft
        {
            get => _allEntities.Count;
        }


        /// <summary>
        /// Get the next set of items for the transaction 
        /// </summary>
        /// <returns>Items in the batch</returns>
        public List<TEntity> GetNext()
        {
            List<TEntity> batch = new List<TEntity>();
            if (_allEntities.Count > 0)
            {
                if (_allEntities.Count <= _batchSize)
                {
                    batch = _allEntities;
                    _allEntities.Clear();
                }
                else
                {
                    for (int i = 0; i < _batchSize; i++)
                    {
                        batch.Add(_allEntities[0]);
                        _allEntities.RemoveAt(0);
                    }
                }
            }            

            return batch;
        }

        /// <summary>
        /// Add an item to the entities list
        /// </summary>
        /// <param name="entity">Entity to add to the transactional batch</param>
        public void Add(TEntity entity)
        {
            _allEntities.Add(entity);
        }

        /// <summary>
        /// Initialise
        /// </summary>
        /// <param name="batchSize">Size of a batch</param>
        /// <param name="entities">The entities to manage through batches</param>
        public TransactionBatchManager(int batchSize, IEnumerable<TEntity> entities)
        {
            _batchSize = batchSize;
            _allEntities = new List<TEntity>(entities);
        }

        /// <summary>
        /// Initialise
        /// </summary>
        /// <param name="batchSize">Size of a batch</param>
        public TransactionBatchManager(int batchSize)
        {
            _batchSize = batchSize;
            _allEntities = new List<TEntity>();
        }

        private List<TEntity> _allEntities;
        private int _batchSize;
    }
}
