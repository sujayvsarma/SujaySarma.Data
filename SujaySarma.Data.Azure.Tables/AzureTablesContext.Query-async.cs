using System.Collections.Generic;
using System.Threading.Tasks;

using Azure.Data.Tables;

using SujaySarma.Data.Azure.Tables.Serialisation;
using SujaySarma.Data.Core.Reflection;

namespace SujaySarma.Data.Azure.Tables
{

    /*
        Query operations (Asynchronous)
    */

    public partial class AzureTablesContext
    {

        /// <summary>
        /// Execute a query against the given table and return the raw table entities
        /// </summary>
        /// <param name="tableName">Name of the Azure Table to query</param>
        /// <param name="selectColumns">List of names of columns to select from the table</param>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        public async Task<List<TableEntity>> ExecuteQueryRawAsync(string tableName, List<string> selectColumns, string? partitionKey = null, string? rowKey = null, string? filter = null)
        {
            // For the public surface, deleted rows will never be returned.
            return await ExecuteQueryAsyncImpl(tableName, selectColumns, partitionKey, rowKey, filter, false);
        }

        /// <summary>
        /// Execute a query against the table attached to <typeparamref name="TObject"/> and return the raw table entities
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <returns>List of <see cref="TableEntity"/> objects</returns>
        public async Task<List<TableEntity>> ExecuteQueryRawAsync<TObject>(string? partitionKey = null, string? rowKey = null, string? filter = null)
        {
            ContainerTypeInformation typeInfo = TypeDiscoveryFactory.Resolve<TObject>();
            return await ExecuteQueryRawAsync(typeInfo.ContainerDefinition.Name, 
                    PrepareQueryColumnsList(typeInfo), 
                    partitionKey, 
                    rowKey, 
                    filter
                );
        }

        /// <summary>
        /// Execute a query against the table attached to <typeparamref name="TObject"/> and returns hydrated .NET entitites
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <returns>List of <typeparamref name="TObject"/> objects</returns>
        public async Task<List<TObject>> ExecuteQueryAsync<TObject>(string? partitionKey = null, string? rowKey = null, string? filter = null)
        {
            ContainerTypeInformation typeInfo = TypeDiscoveryFactory.Resolve<TObject>();

            // We use the Yield return variant here to improve memory management and performance
            IAsyncEnumerable<TableEntity> entities = ExecuteQueryAsyncImplYielder(
                    typeInfo.ContainerDefinition.Name, 
                    PrepareQueryColumnsList(typeInfo), 
                    partitionKey, 
                    rowKey, 
                    filter
                );

            List<TObject> items = new List<TObject>();
            await foreach(TableEntity entity in entities)
            {
                items.Add(AzureTablesSerialiser.Deserialise<TObject>(entity));
            }

            return items;
        }


        /// <summary>
        /// Execute a query against the given table and return the raw table entities
        /// </summary>
        /// <param name="tableName">Name of the Azure Table to query</param>
        /// <param name="selectColumns">List of names of columns to select from the table</param>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        public async Task<List<TableEntity>> ExecuteQueryRawAsync(string tableName, List<string> selectColumns, object? partitionKey = null, object? rowKey = null, string? filter = null)
        {
            // For the public surface, deleted rows will never be returned.
            return await ExecuteQueryAsyncImpl(
                    tableName, 
                    selectColumns, 
                    (string?)ReflectionUtils.ConvertValueIfRequired(partitionKey, typeof(string)), 
                    (string?)ReflectionUtils.ConvertValueIfRequired(rowKey, typeof(string)), 
                    filter, 
                    false
                );
        }

        /// <summary>
        /// Execute a query against the table attached to <typeparamref name="TObject"/> and return the raw table entities
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <returns>List of <see cref="TableEntity"/> objects</returns>
        public async Task<List<TableEntity>> ExecuteQueryRawAsync<TObject>(object? partitionKey = null, object? rowKey = null, string? filter = null)
        {
            ContainerTypeInformation typeInfo = TypeDiscoveryFactory.Resolve<TObject>();
            return await ExecuteQueryRawAsync(typeInfo.ContainerDefinition.Name,
                    PrepareQueryColumnsList(typeInfo),
                    (string?)ReflectionUtils.ConvertValueIfRequired(partitionKey, typeof(string)),
                    (string?)ReflectionUtils.ConvertValueIfRequired(rowKey, typeof(string)),
                    filter
                );
        }

        /// <summary>
        /// Execute a query against the table attached to <typeparamref name="TObject"/> and returns hydrated .NET entitites
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <returns>List of <typeparamref name="TObject"/> objects</returns>
        public async Task<List<TObject>> ExecuteQueryAsync<TObject>(object? partitionKey = null, object? rowKey = null, string? filter = null)
        {
            ContainerTypeInformation typeInfo = TypeDiscoveryFactory.Resolve<TObject>();

            // We use the Yield return variant here to improve memory management and performance
            IAsyncEnumerable<TableEntity> entities = ExecuteQueryAsyncImplYielder(
                    typeInfo.ContainerDefinition.Name,
                    PrepareQueryColumnsList(typeInfo),
                    (string?)ReflectionUtils.ConvertValueIfRequired(partitionKey, typeof(string)),
                    (string?)ReflectionUtils.ConvertValueIfRequired(rowKey, typeof(string)),
                    filter
                );

            List<TObject> items = new List<TObject>();
            await foreach (TableEntity entity in entities)
            {
                items.Add(AzureTablesSerialiser.Deserialise<TObject>(entity));
            }

            return items;
        }

        /// <summary>
        /// Execute the query against the <see cref="TableClient"/>
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <returns>List of <typeparamref name="TObject"/> objects</returns>
        public async Task<List<TObject>> SelectAsync<TObject>(string? partitionKey = null, string? rowKey = null, string? filter = null)            
            => await ExecuteQueryAsync<TObject>(
                    partitionKey,
                    rowKey,
                    filter
                );

        /// <summary>
        /// Execute the query against the <see cref="TableClient"/>
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <returns>List of <typeparamref name="TObject"/> objects</returns>
        public async Task<List<TObject>> SelectAsync<TObject>(object? partitionKey = null, object? rowKey = null, string? filter = null)            
            => await ExecuteQueryAsync<TObject>(
                    (string?)ReflectionUtils.ConvertValueIfRequired(partitionKey, typeof(string)),
                    (string?)ReflectionUtils.ConvertValueIfRequired(rowKey, typeof(string)),
                    filter
                );

        /// <summary>
        /// Execute the query against the <see cref="TableClient"/> and retrieve the only result. If there were no results returned, returns NULL.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">(Optional) Value of PartitionKey</param>
        /// <param name="rowKey">(Optional) Value of RowKey</param>
        /// <param name="filter">(Optional) A valid OData filter string</param>
        /// <returns>A single business object or Null</returns>
        public async Task<TObject?> SelectOnlyResultOrNullAsync<TObject>(string? partitionKey = null, string? rowKey = null, string? filter = null)            
        {
            List<TObject> data = await SelectAsync<TObject>(partitionKey, rowKey, filter);
            if (data.Count == 0)
            {
                return default(TObject);
            }
            return data[0];
        }

        /// <summary>
        /// Execute the query against the <see cref="TableClient"/> and retrieve the only result. If there were no results returned, returns NULL.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">(Optional) Value of PartitionKey</param>
        /// <param name="rowKey">(Optional) Value of RowKey</param>
        /// <param name="filter">(Optional) A valid OData filter string</param>
        /// <returns>A single business object or Null</returns>
        public async Task<TObject?> SelectOnlyResultOrNullAsync<TObject>(object? partitionKey = null, object? rowKey = null, string? filter = null)            
            => await SelectOnlyResultOrNullAsync<TObject>(
                    (string?)ReflectionUtils.ConvertValueIfRequired(partitionKey, typeof(string)),
                    (string?)ReflectionUtils.ConvertValueIfRequired(rowKey, typeof(string)),
                    filter                    
                );
    }
}