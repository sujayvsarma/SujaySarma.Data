
using System.Collections.Generic;

namespace SujaySarma.Data.Core
{
    /// <summary>
    /// Results of a transaction or operation
    /// </summary>
    /// <typeparam name="TEntity">Type of entities involved in the transaction</typeparam>
    public sealed class TransactionResult<TEntity>
    {
        /// <summary>
        /// A default instance of the result
        /// </summary>
        public static readonly TransactionResult<TEntity> Default = new TransactionResult<TEntity>();

        /// <summary>
        /// Total number of entities
        /// </summary>
        public int TotalEntities { get; set; }

        /// <summary>
        /// Number of entities that passed the transaction
        /// </summary>
        public int Passed { get; set; }

        /// <summary>
        /// Number of entities that failed the transaction
        /// </summary>
        public int Failed { get; set; }

        /// <summary>
        /// Messages from the transaction executant
        /// </summary>
        public List<string> Messages { get; } = new List<string>();

        /// <summary>
        /// Collection of entities that failed the transaction
        /// </summary>
        public List<TEntity> FailedEntities { get; } = new List<TEntity>();
    }
}
