using System;

namespace SujaySarma.Data.Core.Constants
{

    /// <summary>
    /// Controls the inclusion of a column/field in data-modification
    /// (insert, update and delete) operations.
    /// </summary>
    [Flags]
    public enum ColumnInclusionStrategy : byte
    {
        /// <summary>
        /// Never
        /// </summary>
        Never = 0,
        
        /// <summary>
        /// Insert operations
        /// </summary>
        Inserts = 1,
        
        /// <summary>
        /// Update operations
        /// </summary>
        Updates = 2,
        
        /// <summary>
        /// Both Insert and Update operations
        /// </summary>
        InsertsAndUpdates = Inserts + Updates,
        
        /// <summary>
        /// Delete operations
        /// </summary>
        Deletes = 4,
        
        /// <summary>
        /// All insert, update and delete operations
        /// </summary>
        InsertUpdateAndDelete = InsertsAndUpdates + Deletes
    }

}