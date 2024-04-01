using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azure.Data.Tables;

using SujaySarma.Data.Azure.Tables.Reflection;

namespace SujaySarma.Data.Azure.Tables
{

    /*
        This file performs generics, Asynchronous operations
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
        public async Task<(long Passed, long Failed, List<string> Messages)> InsertAsync<T>(IEnumerable<T> data) where T : class
            => await ExecuteNonQueryAsync<T>(OperationType.Insert, data);

        /// <summary>
        /// Insert data
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">Collection of data items to insert</param>
        /// <returns>Statistics of the operation and the collection of error messages if any.</returns>
        public async Task<(long Passed, long Failed, List<string> Messages)> InsertAsync<T>(params T[] data) where T : class
            => await InsertAsync<T>(data.AsEnumerable<T>());

        /// <summary>
        /// Update data
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">Collection of data items to update</param>
        /// <returns>Statistics of the operation and the collection of error messages if any.</returns>
        public async Task<(long Passed, long Failed, List<string> Messages)> UpdateAsync<T>(IEnumerable<T> data) where T : class
            => await ExecuteNonQueryAsync<T>(OperationType.Update, data);

        /// <summary>
        /// Update data
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">Collection of data items to update</param>
        /// <returns>Statistics of the operation and the collection of error messages if any.</returns>
        public async Task<(long Passed, long Failed, List<string> Messages)> UpdateAsync<T>(params T[] data) where T : class
            => await UpdateAsync<T>(data.AsEnumerable<T>());

        /// <summary>
        /// Insert/Update data
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">Collection of data items to insert or update</param>
        /// <returns>Statistics of the operation and the collection of error messages if any.</returns>
        public async Task<(long Passed, long Failed, List<string> Messages)> UpsertAsync<T>(IEnumerable<T> data) where T : class
            => await ExecuteNonQueryAsync<T>(OperationType.Upsert, data);

        /// <summary>
        /// Insert/Update data
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">Collection of data items to insert or update</param>
        /// <returns>Statistics of the operation and the collection of error messages if any.</returns>
        public async Task<(long Passed, long Failed, List<string> Messages)> UpsertAsync<T>(params T[] data) where T : class
            => await UpsertAsync<T>(data.AsEnumerable<T>());

        /// <summary>
        /// Delete data
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">Collection of data items to delete</param>
        /// <param name="forceDelete">(Optional, Default: false) Set 'true' to perform a hard-delete on rows of tables that have the 'soft delete' flag set.</param>
        /// <returns>Statistics of the operation and the collection of error messages if any.</returns>
        public async Task<(long Passed, long Failed, List<string> Messages)> DeleteAsync<T>(IEnumerable<T> data, bool forceDelete = false) where T : class
            => await ExecuteNonQueryAsync<T>(OperationType.Delete, data, forceDelete);

        /// <summary>
        /// Delete data
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="forceDelete">(Optional, Default: false) Set 'true' to perform a hard-delete on rows of tables that have the 'soft delete' flag set.</param>
        /// <param name="data">Collection of data items to delete</param>
        /// <returns>Statistics of the operation and the collection of error messages if any.</returns>
        public async Task<(long Passed, long Failed, List<string> Messages)> DeleteAsync<T>(bool forceDelete = false, params T[] data) where T : class
            => await DeleteAsync<T>(data.AsEnumerable<T>(), forceDelete);

        /// <summary>
        /// Delete data
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="partitionKey">(Optional) Value of PartitionKey</param>
        /// <param name="rowKey">(Optional) Value of RowKey</param>
        /// <param name="filter">(Optional) A valid OData filter string</param>
        /// <param name="forceDelete">(Optional, Default: false) Set 'true' to perform a hard-delete on rows of tables that have the 'soft delete' flag set.</param>
        /// <returns>Statistics of the operation and the collection of error messages if any.</returns>
        public async Task<(long Passed, long Failed, List<string> Messages)> DeleteAsync<T>(string? partitionKey = null, string? rowKey = null, string? filter = null, bool forceDelete = false)
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
                return await ExecuteNonQueryAsyncImpl(
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
        public async Task<(long Passed, long Failed, List<string> Messages)> DeleteAsync<T>(object? partitionKey = null, object? rowKey = null, string? filter = null, bool forceDelete = false)
            where T : class
            => await DeleteAsync<T>(
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
        public async Task<(long Passed, long Failed, List<string> Messages)> ExecuteNonQueryAsync<T>(OperationType actionType, bool forDeletePerformHardDelete = false, params T[] data)
            where T : class
            => await ExecuteNonQueryAsync<T>(actionType, data.AsEnumerable<T>(), forDeletePerformHardDelete);

        /// <summary>
        /// Execute a non-query (insert/update/delete) operation
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="actionType">The insert/update/delete operation to perform</param>
        /// <param name="data">Collection of business objects to insert, update or delete</param>
        /// <param name="forDeletePerformHardDelete">(Optional, Default: false) Set 'true' to perform a hard-delete on rows of tables that have the 'soft delete' flag set.</param>
        /// <returns>Statistics of the operation and the collection of error messages if any.</returns>
        public async Task<(long Passed, long Failed, List<string> Messages)> ExecuteNonQueryAsync<T>(OperationType actionType, IEnumerable<T> data, bool forDeletePerformHardDelete = false)
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
                return await ExecuteNonQueryAsyncImpl(
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
        public async Task<T?> SelectOnlyResultOrNullAsync<T>(string? partitionKey = null, string? rowKey = null, string? filter = null, bool includeSoftDeletedRows = false)
            where T : class
        {
            TypeMetadata metadata = TypeMetadata.Discover<T>();
            IAsyncEnumerable<TableEntity> e = QueryAsync(
                    metadata.TableName,
                    PrepareColumnsList(metadata, false),
                    PrepareFilterString(metadata.UseSoftDelete,
                        partitionKey,
                        rowKey,
                        filter,
                        includeSoftDeletedRows
                    )
                );

            IAsyncEnumerator<TableEntity> en = e.GetAsyncEnumerator();
            while (await en.MoveNextAsync())
            {
                if ((en.Current != null) && (en.Current != default))
                {
                    return metadata.Transform<T>(en.Current);
                }
            }

            return null;
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
        public async Task<T?> SelectOnlyResultOrNullAsync<T>(object? partitionKey = null, object? rowKey = null, string? filter = null, bool includeSoftDeletedRows = false)
            where T : class
            => await SelectOnlyResultOrNullAsync<T>(
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
        /// <returns><see cref="IAsyncEnumerable{T}"/> collection of populated business object entities</returns>
        public async IAsyncEnumerable<T> SelectAsync<T>(string? partitionKey = null, string? rowKey = null, string? filter = null, bool includeSoftDeletedRows = false)
            where T : class
        {
            await foreach (T item in ExecuteQueryAsyncImpl<T>(
                    partitionKey,
                    rowKey,
                    filter, includeSoftDeletedRows
                ))
            {
                yield return item;
            }
        }

        /// <summary>
        ///  Execute the query against the <see cref="TableClient"/>
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="partitionKey">(Optional) Value of PartitionKey</param>
        /// <param name="rowKey">(Optional) Value of RowKey</param>
        /// <param name="filter">(Optional) A valid OData filter string</param>
        /// <param name="includeSoftDeletedRows">(Optional, Default: false) Set 'true' to fetch soft-deleted rows</param>
        /// <returns><see cref="IAsyncEnumerable{T}"/> collection of populated business object entities</returns>
        public async IAsyncEnumerable<T> SelectAsync<T>(object? partitionKey = null, object? rowKey = null, string? filter = null, bool includeSoftDeletedRows = false)
            where T : class
        {
            await foreach(T item in SelectAsync<T>(
                    (partitionKey == null) ? null : ReflectionUtils.GetAcceptableValue<string>(partitionKey),
                    (rowKey == null) ? null : ReflectionUtils.GetAcceptableValue<string>(rowKey),
                    filter, includeSoftDeletedRows
                ))
            {
                yield return item;
            }
        }

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
        /// <returns><see cref="IAsyncEnumerable{T}"/> collection of populated business object entities</returns>
        public async IAsyncEnumerable<T> ExecuteQueryAsync<T>(string? partitionKey = null, string? rowKey = null, string? filter = null, bool includeSoftDeletedRows = false)
            where T : class
        {
            await foreach(T item in ExecuteQueryAsyncImpl<T>(
                    partitionKey,
                    rowKey,
                    filter, includeSoftDeletedRows
                ))
            {
                yield return item;
            }
        }

        /// <summary>
        ///  Execute the query against the <see cref="TableClient"/>
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="partitionKey">(Optional) Value of PartitionKey</param>
        /// <param name="rowKey">(Optional) Value of RowKey</param>
        /// <param name="filter">(Optional) A valid OData filter string</param>
        /// <param name="includeSoftDeletedRows">(Optional, Default: false) Set 'true' to fetch soft-deleted rows</param>
        /// <returns><see cref="IAsyncEnumerable{T}"/> collection of populated business object entities</returns>
        public async IAsyncEnumerable<T> ExecuteQueryAsync<T>(object? partitionKey = null, object? rowKey = null, string? filter = null, bool includeSoftDeletedRows = false)
            where T : class
        {
            await foreach(T item in ExecuteQueryAsyncImpl<T>(
                    (partitionKey == null) ? null : ReflectionUtils.GetAcceptableValue<string>(partitionKey),
                    (rowKey == null) ? null : ReflectionUtils.GetAcceptableValue<string>(rowKey),
                    filter, includeSoftDeletedRows
                ))
            {
                yield return item;
            }
        }

        /// <summary>
        /// (Implementation) Execute the query against the <see cref="TableClient"/>
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="partitionKey">(Optional) Value of PartitionKey</param>
        /// <param name="rowKey">(Optional) Value of RowKey</param>
        /// <param name="filter">(Optional) A valid OData filter string</param>
        /// <param name="includeSoftDeletedRows">(Optional, Default: false) Set 'true' to fetch soft-deleted rows</param>
        /// <returns><see cref="IAsyncEnumerable{T}"/> collection of populated business object entities</returns>
        private async IAsyncEnumerable<T> ExecuteQueryAsyncImpl<T>(string? partitionKey = null, string? rowKey = null, string? filter = null, bool includeSoftDeletedRows = false)
            where T : class
        {           
            TypeMetadata metadata = TypeMetadata.Discover<T>();
            await foreach (TableEntity entity in QueryAsync(
                    metadata.TableName, 
                    PrepareColumnsList(metadata, false), 
                    PrepareFilterString(metadata.UseSoftDelete, partitionKey, rowKey, filter, includeSoftDeletedRows)))
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
        public async Task DropTableAsync<T>() where T : class
            => await DropTableAsync(GetTableName<T>());

        /// <summary>
        /// Create table attached to the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        public async Task CreateTableAsync<T>() where T : class
            => await CreateTableAsync(GetTableName<T>());

        /// <summary>
        /// Check if table attached to the <typeparamref name="T"/> exists.
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <returns>True if table exists</returns>
        public async Task TableExistsAsync<T>() where T : class
            => await TableExistsAsync(GetTableName<T>());

        /// <summary>
        /// Clear all rows from a table attached to the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        public async Task ClearTableAsync<T>() where T : class
            => await ClearTableAsync(GetTableName<T>());

        /// <summary>
        /// Clear data for the specified <paramref name="partitionKey"/> from the table attached to the <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="partitionKey">Partition key to clear data for</param>
        public async Task ClearPartitionAsync<T>(string partitionKey) where T : class
        {
            TypeMetadata info = TypeMetadata.Discover<T>();
            await ClearPartitionAsync(info.TableName, partitionKey, info.UseSoftDelete);
        }

        /// <summary>
        /// Get reference to a table
        /// </summary>
        /// <param name="tableName">Name of table to connect to</param>
        /// <param name="doNotCreate">If NOT set (default), will create the table if it does not exist</param>
        /// <returns></returns>
        private async Task<TableClient> GetTableReferenceAsync(string tableName, bool doNotCreate = false)
        {
            TableClient tableClient = new(_connectionString, tableName);
            if (!doNotCreate) { await tableClient.CreateIfNotExistsAsync(); }

            return tableClient;
        }

        #endregion
    }
}