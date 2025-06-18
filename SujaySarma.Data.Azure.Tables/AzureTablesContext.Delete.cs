using Azure.Data.Tables;

using SujaySarma.Data.Azure.Tables.Serialisation;
using SujaySarma.Data.Core;
using SujaySarma.Data.Core.Reflection;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SujaySarma.Data.Azure.Tables
{
    /// <summary>
    /// Handle DELETE operations - both sync and async, generic and regular
    /// </summary>
    public partial class AzureTablesContext
    {

        #region Sync

        /// <summary>Delete a single item into an Azure Table</summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="obj">Instance of object with the data to delete</param>
        /// <param name="permanentDelete">When set, performs a hard delete. When not set and the underlying table is not configured for soft-deletion, a hard delete will be performed nonetheless.</param>
        /// <returns>Results of the transaction</returns>
        public TransactionResult<TObject> Delete<TObject>(TObject obj, bool permanentDelete = false)
        {
            if (obj == null)
            {
                return TransactionResult<TObject>.Default;
            }

            ContainerTypeInfo ContainerTypeInfo = TypeDiscoveryFactory.Resolve<TObject>();
            TableEntity tableEntity = AzureTablesSerialiser.Serialise(obj)
                    ?? throw new ArgumentException("The object provided could not be serialised into a TableEntity. Please check the properties of the object and ensure that it is serialisable.", nameof(obj));

            DeleteAction action = GetSanitisedDeleteMode(permanentDelete);

            TransactionResult<TableEntity> result = ExecuteNonQueryImpl(
                ContainerTypeInfo.Container.Name,
                    new List<TableEntity>() { tableEntity },
                        TableTransactionActionType.Delete, action);

            return ConvertTransactionResultToCallerFormats<TObject>(result);
        }

        /// <summary>Delete a collection of items into an Azure Table</summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="objects">Collection of items to delete</param>
        /// <param name="permanentDelete">When set, performs a hard delete. When not set and the underlying table is not configured for soft-deletion, a hard delete will be performed nonetheless.</param>
        /// <returns>Results of the transaction</returns>
        public TransactionResult<TObject> Delete<TObject>(IEnumerable<TObject> objects, bool permanentDelete = false)
        {
            if ((objects == null) || ((objects is ICollection<TObject> collection) && (collection.Count == 0)))
            {
                return TransactionResult<TObject>.Default;
            }

            TransactionResult<TableEntity> result = ExecuteNonQueryImpl(
                TypeDiscoveryFactory.Resolve<TObject>().Container.Name,
                    AzureTablesSerialiser.Serialise(objects),
                        TableTransactionActionType.Delete,
                            GetSanitisedDeleteMode(permanentDelete));

            return ConvertTransactionResultToCallerFormats<TObject>(result);
        }

        /// <summary>Delete a collection of items into an Azure Table</summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="permanentDelete">When set, performs a hard delete. When not set and the underlying table is not configured for soft-deletion, a hard delete will be performed nonetheless.</param>
        /// <param name="objects">Collection of items to delete</param>
        /// <returns>Results of the transaction</returns>
        public TransactionResult<TObject> Delete<TObject>(bool permanentDelete = false, params TObject[] objects)
            // IEnumerable cast is required to call the other overload. Else we would recursively call ourselves adnauseum!
            => Delete<TObject>((IEnumerable<TObject>)objects, permanentDelete);

        /// <summary>
        /// Delete rows in an Azure Table matching the provided values or conditions
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <param name="permanentDelete">When set, performs a hard delete. When not set and the underlying table is not configured for soft-deletion, a hard delete will be performed nonetheless.</param>
        /// <returns>Results of the transaction</returns>
        public TransactionResult<TObject> Delete<TObject>(string? partitionKey = null, string? rowKey = null, string? filter = null, bool permanentDelete = false)
        {
            ContainerTypeInfo container = TypeDiscoveryFactory.Resolve<TObject>();
            List<TableEntity> entities = new List<TableEntity>();
            
            foreach (TableEntity tableEntity in ExecuteQueryImplYielder(container.Container.Name, PrepareQueryColumnsList(container), partitionKey, rowKey, filter))
            {
                entities.Add(tableEntity);
            }

            if (entities.Count == 0)
            {
                return TransactionResult<TObject>.Default;
            }

            return ConvertTransactionResultToCallerFormats<TObject>(
                    // We perform automatic batching of requests within ExecNonQueryImpl!
                    ExecuteNonQueryImpl(
                        container.Container.Name,
                            entities,
                                TableTransactionActionType.Delete,
                                    GetSanitisedDeleteMode(permanentDelete)
                    )
                );
        }

        /// <summary>
        /// Delete rows in an Azure Table matching the provided values or conditions
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <param name="permanentDelete">When set, performs a hard delete. When not set and the underlying table is not configured for soft-deletion, a hard delete will be performed nonetheless.</param>
        /// <returns>Results of the transaction</returns>
        public TransactionResult<TObject> Delete<TObject>(object? partitionKey = null, object? rowKey = null, string? filter = null, bool permanentDelete = false)
            => Delete<TObject>(
                (string?)ReflectionUtils.ConvertValueIfRequired(partitionKey, typeof(string)),
                    (string?)ReflectionUtils.ConvertValueIfRequired(rowKey, typeof(string)),
                        filter, permanentDelete);

        #endregion

        #region Async

        /// <summary>Delete a single item into an Azure Table</summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="obj">Instance of object with the data to delete</param>
        /// <param name="permanentDelete">When set, performs a hard delete. When not set and the underlying table is not configured for soft-deletion, a hard delete will be performed nonetheless.</param>
        /// <returns>Results of the transaction</returns>
        public async Task<TransactionResult<TObject>> DeleteAsync<TObject>(TObject obj, bool permanentDelete = false)
        {
            if (obj == null)
            {
                return TransactionResult<TObject>.Default;
            }

            ContainerTypeInfo ContainerTypeInfo = TypeDiscoveryFactory.Resolve<TObject>();
            TableEntity tableEntity = AzureTablesSerialiser.Serialise(obj)
                    ?? throw new ArgumentException("The object provided could not be serialised into a TableEntity. Please check the properties of the object and ensure that it is serialisable.", nameof(obj));

            return ConvertTransactionResultToCallerFormats<TObject>(
                await ExecuteNonQueryAsyncImpl(
                    ContainerTypeInfo.Container.Name,
                        new List<TableEntity>() { tableEntity },
                            TableTransactionActionType.Delete, GetSanitisedDeleteMode(permanentDelete))
            );
        }

        /// <summary>Delete a collection of items into an Azure Table</summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="objects">Collection of items to delete</param>
        /// <param name="permanentDelete">When set, performs a hard delete. When not set and the underlying table is not configured for soft-deletion, a hard delete will be performed nonetheless.</param>
        /// <returns>Results of the transaction</returns>
        public async Task<TransactionResult<TObject>> DeleteAsync<TObject>(IEnumerable<TObject> objects, bool permanentDelete = false)
        { 
            if ((objects == null) || ((objects is ICollection<TObject> collection) && (collection.Count == 0)))
            {
                return TransactionResult<TObject>.Default;
            }

            return ConvertTransactionResultToCallerFormats<TObject>(
                await ExecuteNonQueryAsyncImpl(
                    TypeDiscoveryFactory.Resolve<TObject>().Container.Name,
                        AzureTablesSerialiser.Serialise<TObject>(objects),
                            TableTransactionActionType.Delete, GetSanitisedDeleteMode(permanentDelete))
            );
        }

        /// <summary>Delete a collection of items into an Azure Table</summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="permanentDelete">When set, performs a hard delete. When not set and the underlying table is not configured for soft-deletion, a hard delete will be performed nonetheless.</param>
        /// <param name="objects">Collection of items to delete</param>
        /// <returns>Results of the transaction</returns>
        public async Task<TransactionResult<TObject>> DeleteAsync<TObject>(bool permanentDelete = false, params TObject[] objects)
            // The IEnumerable cast is required to call the other overload. Else we would recursively call ourselves adnauseum!
            => await DeleteAsync<TObject>((IEnumerable<TObject>)objects, permanentDelete);

        /// <summary>
        /// Delete rows in an Azure Table matching the provided values or conditions
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <param name="permanentDelete">When set, performs a hard delete. When not set and the underlying table is not configured for soft-deletion, a hard delete will be performed nonetheless.</param>
        /// <returns>Results of the transaction</returns>
        public async Task<TransactionResult<TObject>> DeleteAsync<TObject>(string? partitionKey = null, string? rowKey = null, string? filter = null, bool permanentDelete = false)
        {
            ContainerTypeInfo container = TypeDiscoveryFactory.Resolve<TObject>();
            List<TableEntity> entities = new List<TableEntity>();

            await foreach (TableEntity tableEntity in ExecuteQueryAsyncImplYielder(container.Container.Name, PrepareQueryColumnsList(container), partitionKey, rowKey, filter))
            {
                entities.Add(tableEntity);
            }

            if (entities.Count == 0)
            {
                return TransactionResult<TObject>.Default;
            }

            return ConvertTransactionResultToCallerFormats<TObject>(
                    // We perform automatic batching of requests within ExecNonQueryImpl !
                    await ExecuteNonQueryAsyncImpl(
                        container.Container.Name,
                            entities,
                                TableTransactionActionType.Delete,
                                    GetSanitisedDeleteMode(permanentDelete)
                    )
                );
        }

        /// <summary>
        /// Delete rows in an Azure Table matching the provided values or conditions
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <param name="permanentDelete">When set, performs a hard delete. When not set and the underlying table is not configured for soft-deletion, a hard delete will be performed nonetheless.</param>
        /// <returns>Results of the transaction</returns>
        public async Task<TransactionResult<TObject>> DeleteAsync<TObject>(object? partitionKey = null, object? rowKey = null, string? filter = null, bool permanentDelete = false)
            => await DeleteAsync<TObject>(
                (string?)ReflectionUtils.ConvertValueIfRequired(partitionKey, typeof(string)), 
                    (string?)ReflectionUtils.ConvertValueIfRequired(rowKey, typeof(string)), 
                        filter, permanentDelete);

        #endregion

        #region Conditional deletes

        /// <summary>
        /// Delete rows in an Azure Table matching the provided values or conditions
        /// </summary>
        /// <param name="tableName">Name of the table to delete rows from</param>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <param name="permanentDelete">When set, performs a hard delete. When not set and the underlying table is not configured for soft-deletion, a hard delete will be performed nonetheless.</param>
        /// <returns>Results of the transaction</returns>
        public TransactionResult<TableEntity> Delete(string tableName, string? partitionKey = null, string? rowKey = null, string? filter = null, bool permanentDelete = false)
        {
            List<TableEntity> entities = new List<TableEntity>();
            foreach (TableEntity tableEntity in ExecuteQueryImplYielder(tableName, ReservedNames.All, partitionKey, rowKey, filter))
            {
                entities.Add(tableEntity);
            }

            if (entities.Count == 0)
            {
                return TransactionResult<TableEntity>.Default;
            }

            return ExecuteNonQueryImpl(
                    tableName,
                        entities, 
                            TableTransactionActionType.Delete, 
                                GetSanitisedDeleteMode(permanentDelete)
                );
        }

        /// <summary>
        /// Delete rows in an Azure Table matching the provided values or conditions
        /// </summary>
        /// <param name="tableName">Name of the table to delete rows from</param>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <param name="permanentDelete">When set, performs a hard delete. When not set and the underlying table is not configured for soft-deletion, a hard delete will be performed nonetheless.</param>
        /// <returns>Results of the transaction</returns>
        public TransactionResult<TableEntity> Delete(string tableName, object? partitionKey = null, object? rowKey = null, string? filter = null, bool permanentDelete = false)
            => Delete(tableName, 
                    (string?)ReflectionUtils.ConvertValueIfRequired(partitionKey, typeof(string)), 
                        (string?)ReflectionUtils.ConvertValueIfRequired(rowKey, typeof(string)), 
                            filter, permanentDelete);

        /// <summary>
        /// Delete rows in an Azure Table matching the provided values or conditions
        /// </summary>
        /// <param name="tableName">Name of the table to delete rows from</param>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <param name="permanentDelete">When set, performs a hard delete. When not set and the underlying table is not configured for soft-deletion, a hard delete will be performed nonetheless.</param>
        /// <returns>Results of the transaction</returns>
        public async Task<TransactionResult<TableEntity>> DeleteAsync(string tableName, string? partitionKey = null, string? rowKey = null, string? filter = null, bool permanentDelete = false)
        {
            List<TableEntity> entities = new List<TableEntity>();
            await foreach (TableEntity tableEntity in ExecuteQueryAsyncImplYielder(tableName, ReservedNames.All, partitionKey, rowKey, filter))
            {
                entities.Add(tableEntity);
            }

            if (entities.Count == 0)
            {
                return TransactionResult<TableEntity>.Default;
            }

            return await ExecuteNonQueryAsyncImpl(
                    tableName,
                        entities,
                            TableTransactionActionType.Delete,
                                GetSanitisedDeleteMode(permanentDelete)
                );
        }

        /// <summary>
        /// Delete rows in an Azure Table matching the provided values or conditions
        /// </summary>
        /// <param name="tableName">Name of the table to delete rows from</param>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <param name="permanentDelete">When set, performs a hard delete. When not set and the underlying table is not configured for soft-deletion, a hard delete will be performed nonetheless.</param>
        /// <returns>Results of the transaction</returns>
        public async Task<TransactionResult<TableEntity>> DeleteAsync(string tableName, object? partitionKey = null, object? rowKey = null, string? filter = null, bool permanentDelete = false)
            => await DeleteAsync(tableName,
                    (string?)ReflectionUtils.ConvertValueIfRequired(partitionKey, typeof(string)),
                        (string?)ReflectionUtils.ConvertValueIfRequired(rowKey, typeof(string)),
                            filter, permanentDelete);

        #endregion


        /// <summary>
        /// Return a delete mode that is consistent with whether we are doing a permanent delete.
        /// </summary>
        /// <param name="isPermanentDelete">Is this a permanent delete?</param>
        /// <returns>Appropriate delete mode.</returns>
        private static DeleteAction GetSanitisedDeleteMode(bool isPermanentDelete)
            => (isPermanentDelete ? DeleteAction.HardDelete : DeleteAction.SoftDelete);

    }
}
