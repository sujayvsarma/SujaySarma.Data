using System.Collections.Generic;
using System.Linq;

using Azure.Data.Tables;

using SujaySarma.Data.Azure.Tables.Reflection;

namespace SujaySarma.Data.Azure.Tables
{

    /*
        This file performs generics, synchronous operations
    */


    /// <summary>
    /// A completely connection-less approach to interacting with Azure Tables. Supports both 
    /// Azure Storage Tables and Azure Cosmos DB with Tables API.
    /// </summary>
    public partial class AzureTablesContext
    {

        #region Insert/Update/Delete Wrapper Overloads

        /// <summary>
        /// Insert data
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">Collection of data items to insert</param>
        /// <returns>Statistics of the operation and the collection of error messages if any.</returns>
        public (long Passed, long Failed, List<string> Messages) Insert<T>(IEnumerable<T> data) where T : class
            => ExecuteNonQuery<T>(OperationType.Insert, data);

        /// <summary>
        /// Insert data
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">Collection of data items to insert</param>
        /// <returns>Statistics of the operation and the collection of error messages if any.</returns>
        public (long Passed, long Failed, List<string> Messages) Insert<T>(params T[] data) where T : class
            => Insert<T>(data.AsEnumerable<T>());

        /// <summary>
        /// Update data
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">Collection of data items to update</param>
        /// <returns>Statistics of the operation and the collection of error messages if any.</returns>
        public (long Passed, long Failed, List<string> Messages) Update<T>(IEnumerable<T> data) where T : class
            => ExecuteNonQuery<T>(OperationType.Update, data);

        /// <summary>
        /// Update data
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">Collection of data items to update</param>
        /// <returns>Statistics of the operation and the collection of error messages if any.</returns>
        public (long Passed, long Failed, List<string> Messages) Update<T>(params T[] data) where T : class
            => Update<T>(data.AsEnumerable<T>());

        /// <summary>
        /// Insert/Update data
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">Collection of data items to insert or update</param>
        /// <returns>Statistics of the operation and the collection of error messages if any.</returns>
        public (long Passed, long Failed, List<string> Messages) Upsert<T>(IEnumerable<T> data) where T : class
            => ExecuteNonQuery<T>(OperationType.Upsert, data);

        /// <summary>
        /// Insert/Update data
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">Collection of data items to insert or update</param>
        /// <returns>Statistics of the operation and the collection of error messages if any.</returns>
        public (long Passed, long Failed, List<string> Messages) Upsert<T>(params T[] data) where T : class
            => Upsert<T>(data.AsEnumerable<T>());

        /// <summary>
        /// Delete data
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">Collection of data items to delete</param>
        /// <param name="forceDelete">(Optional, Default: false) Set 'true' to perform a hard-delete on rows of tables that have the 'soft delete' flag set.</param>
        /// <returns>Statistics of the operation and the collection of error messages if any.</returns>
        public (long Passed, long Failed, List<string> Messages) Delete<T>(IEnumerable<T> data, bool forceDelete = false) where T : class
            => ExecuteNonQuery<T>(OperationType.Delete, data, forceDelete);

        /// <summary>
        /// Delete data
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="forceDelete">(Optional, Default: false) Set 'true' to perform a hard-delete on rows of tables that have the 'soft delete' flag set.</param>
        /// <param name="data">Collection of data items to delete</param>
        /// <returns>Statistics of the operation and the collection of error messages if any.</returns>
        public (long Passed, long Failed, List<string> Messages) Delete<T>(bool forceDelete = false, params T[] data) where T : class
            => Delete<T>(data.AsEnumerable<T>(), forceDelete);

        /// <summary>
        /// Delete data
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="partitionKey">(Optional) Value of PartitionKey</param>
        /// <param name="rowKey">(Optional) Value of RowKey</param>
        /// <param name="filter">(Optional) A valid OData filter string</param>
        /// <param name="forceDelete">(Optional, Default: false) Set 'true' to perform a hard-delete on rows of tables that have the 'soft delete' flag set.</param>
        /// <returns>Statistics of the operation and the collection of error messages if any.</returns>
        public (long Passed, long Failed, List<string> Messages) Delete<T>(string? partitionKey = null, string? rowKey = null, string? filter = null, bool forceDelete = false)
            where T : class
        {
            TypeMetadata metadata = TypeMetadata.Discover<T>();
            List<TableEntity> entities = new();
            foreach (TableEntity entity in Query(metadata.TableName, PrepareColumnsList(metadata, false), PrepareFilterString(metadata.UseSoftDelete, partitionKey, rowKey, filter, forceDelete)))
            {
                entities.Add(entity);
            }

            if (entities.Count > 0)
            {
                return ExecuteNonQueryImpl(
                       metadata.TableName, OperationType.Delete,
                           entities,
                               metadata.UseSoftDelete, forceDelete
                   );
            }

            return (0, 0, new());
        }

        /// <summary>
        /// Delete data
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="partitionKey">(Optional) Value of PartitionKey</param>
        /// <param name="rowKey">(Optional) Value of RowKey</param>
        /// <param name="filter">(Optional) A valid OData filter string</param>
        /// <param name="forceDelete">(Optional, Default: false) Set 'true' to perform a hard-delete on rows of tables that have the 'soft delete' flag set.</param>
        /// <returns>Statistics of the operation and the collection of error messages if any.</returns>
        public (long Passed, long Failed, List<string> Messages) Delete<T>(object? partitionKey = null, object? rowKey = null, string? filter = null, bool forceDelete = false)
            where T : class
            => Delete<T>(
                    (partitionKey == null) ? null : ReflectionUtils.GetAcceptableValue<string>(partitionKey),
                    (rowKey == null) ? null : ReflectionUtils.GetAcceptableValue<string>(rowKey),
                    filter,
                    forceDelete
               );

        #endregion

        #region Execute NonQuery Overloads

        /// <summary>
        /// Execute a non-query (insert/update/delete) operation
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="actionType">The insert/update/delete operation to perform</param>
        /// <param name="data">Collection of business objects to insert, update or delete</param>
        /// <param name="forDeletePerformHardDelete">(Optional, Default: false) Set 'true' to perform a hard-delete on rows of tables that have the 'soft delete' flag set.</param>
        /// <returns>Statistics of the operation and the collection of error messages if any.</returns>
        public (long Passed, long Failed, List<string> Messages) ExecuteNonQuery<T>(OperationType actionType, bool forDeletePerformHardDelete = false, params T[] data)
            where T : class
            => ExecuteNonQuery<T>(actionType, data.AsEnumerable<T>(), forDeletePerformHardDelete);

        /// <summary>
        /// Execute a non-query (insert/update/delete) operation
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="actionType">The insert/update/delete operation to perform</param>
        /// <param name="data">Collection of business objects to insert, update or delete</param>
        /// <param name="forDeletePerformHardDelete">(Optional, Default: false) Set 'true' to perform a hard-delete on rows of tables that have the 'soft delete' flag set.</param>
        /// <returns>Statistics of the operation and the collection of error messages if any.</returns>
        public (long Passed, long Failed, List<string> Messages) ExecuteNonQuery<T>(OperationType actionType, IEnumerable<T> data, bool forDeletePerformHardDelete = false)
            where T : class
        {
            TypeMetadata metadata = TypeMetadata.Discover<T>();
            bool minimalSurface = (actionType == OperationType.Delete);
            List<TableEntity> entities = new();
            foreach (T item in data)
            {
                entities.Add(metadata.Transform(item, minimalSurface));
            }

            if (entities.Count > 0)
            {
                return ExecuteNonQueryImpl(
                       metadata.TableName, actionType,
                           entities,
                               metadata.UseSoftDelete, forDeletePerformHardDelete
                   );
            }

            return (0, 0, new());
        }

        #endregion

        #region "Select(...)" Wrapper Overloads

        /// <summary>
        ///  Execute the query against the <see cref="TableClient"/> and retrieve the only result. If there were no results returned, returns NULL.
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="partitionKey">(Optional) Value of PartitionKey</param>
        /// <param name="rowKey">(Optional) Value of RowKey</param>
        /// <param name="filter">(Optional) A valid OData filter string</param>
        /// <param name="includeSoftDeletedRows">(Optional, Default: false) Set 'true' to fetch soft-deleted rows</param>
        /// <returns>A single business object or Null</returns>
        public T? SelectOnlyResultOrNull<T>(string? partitionKey = null, string? rowKey = null, string? filter = null, bool includeSoftDeletedRows = false)
            where T : class
        {
            TypeMetadata metadata = TypeMetadata.Discover<T>();
            TableEntity? entity = Query(
                    metadata.TableName,
                    PrepareColumnsList(metadata, false),
                    PrepareFilterString(metadata.UseSoftDelete,
                        partitionKey,
                        rowKey,
                        filter,
                        includeSoftDeletedRows
                    )
                ).FirstOrDefault();
            return (entity != null) ? metadata.Transform<T>(entity) : null;
        }

        /// <summary>
        ///  Execute the query against the <see cref="TableClient"/> and retrieve the only result. If there were no results returned, returns NULL.
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="partitionKey">(Optional) Value of PartitionKey</param>
        /// <param name="rowKey">(Optional) Value of RowKey</param>
        /// <param name="filter">(Optional) A valid OData filter string</param>
        /// <param name="includeSoftDeletedRows">(Optional, Default: false) Set 'true' to fetch soft-deleted rows</param>
        /// <returns>A single business object or Null</returns>
        public T? SelectOnlyResultOrNull<T>(object? partitionKey = null, object? rowKey = null, string? filter = null, bool includeSoftDeletedRows = false)
            where T : class
            => SelectOnlyResultOrNull<T>(
                    (partitionKey == null) ? null : ReflectionUtils.GetAcceptableValue<string>(partitionKey),
                    (rowKey == null) ? null : ReflectionUtils.GetAcceptableValue<string>(rowKey),
                    filter,
                    includeSoftDeletedRows
                );


        /// <summary>
        ///  Execute the query against the <see cref="TableClient"/>
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="partitionKey">(Optional) Value of PartitionKey</param>
        /// <param name="rowKey">(Optional) Value of RowKey</param>
        /// <param name="filter">(Optional) A valid OData filter string</param>
        /// <param name="includeSoftDeletedRows">(Optional, Default: false) Set 'true' to fetch soft-deleted rows</param>
        /// <returns><see cref="IEnumerable{T}"/> collection of populated business object entities</returns>
        public IEnumerable<T> Select<T>(string? partitionKey = null, string? rowKey = null, string? filter = null, bool includeSoftDeletedRows = false)
            where T : class
            => ExecuteQueryImpl<T>(
                    partitionKey,
                    rowKey,
                    filter, includeSoftDeletedRows
                );

        /// <summary>
        ///  Execute the query against the <see cref="TableClient"/>
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="partitionKey">(Optional) Value of PartitionKey</param>
        /// <param name="rowKey">(Optional) Value of RowKey</param>
        /// <param name="filter">(Optional) A valid OData filter string</param>
        /// <param name="includeSoftDeletedRows">(Optional, Default: false) Set 'true' to fetch soft-deleted rows</param>
        /// <returns><see cref="IEnumerable{T}"/> collection of populated business object entities</returns>
        public IEnumerable<T> Select<T>(object? partitionKey = null, object? rowKey = null, string? filter = null, bool includeSoftDeletedRows = false)
            where T : class
            => ExecuteQueryImpl<T>(
                    (partitionKey == null) ? null : ReflectionUtils.GetAcceptableValue<string>(partitionKey),
                    (rowKey == null) ? null : ReflectionUtils.GetAcceptableValue<string>(rowKey),
                    filter, includeSoftDeletedRows
                );

        #endregion

        #region Execute Query Overloads

        /// <summary>
        ///  Execute the query against the <see cref="TableClient"/>
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="partitionKey">(Optional) Value of PartitionKey</param>
        /// <param name="rowKey">(Optional) Value of RowKey</param>
        /// <param name="filter">(Optional) A valid OData filter string</param>
        /// <param name="includeSoftDeletedRows">(Optional, Default: false) Set 'true' to fetch soft-deleted rows</param>
        /// <returns><see cref="IEnumerable{T}"/> collection of populated business object entities</returns>
        public IEnumerable<T> ExecuteQuery<T>(string? partitionKey = null, string? rowKey = null, string? filter = null, bool includeSoftDeletedRows = false)
            where T : class
            => ExecuteQueryImpl<T>(
                    partitionKey,
                    rowKey,
                    filter, includeSoftDeletedRows
                );

        /// <summary>
        ///  Execute the query against the <see cref="TableClient"/>
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="partitionKey">(Optional) Value of PartitionKey</param>
        /// <param name="rowKey">(Optional) Value of RowKey</param>
        /// <param name="filter">(Optional) A valid OData filter string</param>
        /// <param name="includeSoftDeletedRows">(Optional, Default: false) Set 'true' to fetch soft-deleted rows</param>
        /// <returns><see cref="IEnumerable{T}"/> collection of populated business object entities</returns>
        public IEnumerable<T> ExecuteQuery<T>(object? partitionKey = null, object? rowKey = null, string? filter = null, bool includeSoftDeletedRows = false)
            where T : class
            => ExecuteQueryImpl<T>(
                    (partitionKey == null) ? null : ReflectionUtils.GetAcceptableValue<string>(partitionKey),
                    (rowKey == null) ? null : ReflectionUtils.GetAcceptableValue<string>(rowKey),
                    filter, includeSoftDeletedRows
                );

        /// <summary>
        /// (Implementation) Execute the query against the <see cref="TableClient"/>
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="partitionKey">(Optional) Value of PartitionKey</param>
        /// <param name="rowKey">(Optional) Value of RowKey</param>
        /// <param name="filter">(Optional) A valid OData filter string</param>
        /// <param name="includeSoftDeletedRows">(Optional, Default: false) Set 'true' to fetch soft-deleted rows</param>
        /// <returns><see cref="IEnumerable{T}"/> collection of populated business object entities</returns>
        private IEnumerable<T> ExecuteQueryImpl<T>(string? partitionKey = null, string? rowKey = null, string? filter = null, bool includeSoftDeletedRows = false)
            where T : class
        {
            TypeMetadata metadata = TypeMetadata.Discover<T>();
            foreach (TableEntity entity in Query(metadata.TableName, PrepareColumnsList(metadata, false), PrepareFilterString(metadata.UseSoftDelete, partitionKey, rowKey, filter, includeSoftDeletedRows)))
            {
                yield return metadata.Transform<T>(entity);
            }
        }

        #endregion

        #region Table-level operations

        /// <summary>
        /// Drop table attached to the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        public void DropTable<T>() where T : class
            => DropTable(GetTableName<T>());

        /// <summary>
        /// Create table attached to the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        public void CreateTable<T>() where T : class
            => CreateTable(GetTableName<T>());

        /// <summary>
        /// Check if table attached to the <typeparamref name="T"/> exists.
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <returns>True if table exists</returns>
        public bool TableExists<T>() where T : class
            => TableExists(GetTableName<T>());


        /// <summary>
        /// Clear all rows from a table attached to the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        public void ClearTable<T>() where T : class
            => ClearTable(GetTableName<T>());

        /// <summary>
        /// Clear data for the specified <paramref name="partitionKey"/> from the table attached to the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="partitionKey">Partition key to clear data for</param>
        public void ClearPartition<T>(string partitionKey) where T : class
        {
            TypeMetadata info = TypeMetadata.Discover<T>();
            ClearPartition(info.TableName, partitionKey, info.UseSoftDelete);
        }

        /// <summary>
        /// Gets the name of the table from the <see cref="Attributes.TableAttribute"/> decorating the 
        /// object defined by <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of object to get table for.</typeparam>
        /// <returns>Name of the table as specified by the <see cref="Attributes.TableAttribute.TableName"/></returns>
        private static string GetTableName<T>() where T : class
            => TypeMetadata.Discover<T>().TableName;

        #endregion

    }
}