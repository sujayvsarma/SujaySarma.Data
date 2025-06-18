using Azure.Data.Tables;

using SujaySarma.Data.Azure.Tables.Serialisation;
using SujaySarma.Data.Core;
using SujaySarma.Data.Core.Reflection;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace SujaySarma.Data.Azure.Tables
{
    /// <summary>
    /// Query operations (sync and generic)
    /// </summary>
    public partial class AzureTablesContext
    {
        #region Execute Query (Raw)

        /// <summary>
        /// Execute a query against the table attached to <typeparamref name="TObject" /> and return the raw table entities
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <returns>List of <see cref="TableEntity" /> objects</returns>
        public async Task<List<TableEntity>> ExecuteQueryRawAsync<TObject>(string? partitionKey = null, string? rowKey = null, string? filter = null)
        {
            ContainerTypeInfo container = TypeDiscoveryFactory.Resolve<TObject>();
            return await ExecuteQueryRawAsync(
                container.Container.Name, 
                    PrepareQueryColumnsList(container), 
                        partitionKey, rowKey, filter);
        }

        /// <summary>
        /// Execute a query against the table attached to <typeparamref name="TObject" /> and return the raw table entities
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <returns>List of <see cref="TableEntity" /> objects</returns>
        public async Task<List<TableEntity>> ExecuteQueryRawAsync<TObject>(object? partitionKey = null, object? rowKey = null, string? filter = null)
        {
            ContainerTypeInfo container = TypeDiscoveryFactory.Resolve<TObject>();
            return await ExecuteQueryRawAsync(
                container.Container.Name, 
                    PrepareQueryColumnsList(container), 
                        (string?)ReflectionUtils.ConvertValueIfRequired(partitionKey, typeof(string)), 
                            (string?)ReflectionUtils.ConvertValueIfRequired(rowKey, typeof(string)), 
                                filter);
        }        

        #endregion

        #region Execute Query

        /// <summary>
        /// Execute a query against the table attached to <typeparamref name="TObject" /> and returns hydrated .NET entitites
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <returns>List of <typeparamref name="TObject" /> objects</returns>
        public async Task<List<TObject>> ExecuteQueryAsync<TObject>(object? partitionKey = null, object? rowKey = null, string? filter = null)
        {
            ContainerTypeInfo container = TypeDiscoveryFactory.Resolve<TObject>();
            IAsyncEnumerable<TableEntity> asyncEnumerable = ExecuteQueryAsyncImplYielder(
                container.Container.Name, 
                    PrepareQueryColumnsList(container), 
                        (string?)ReflectionUtils.ConvertValueIfRequired(partitionKey, typeof(string)), 
                            (string?)ReflectionUtils.ConvertValueIfRequired(rowKey, typeof(string)), 
                                filter);

            List<TObject> items = new List<TObject>();
            await foreach (TableEntity entity in asyncEnumerable)
            {
                items.Add(AzureTablesSerialiser.Deserialise<TObject>(entity));
            }

            return items;
        }

        /// <summary>
        /// Execute a query against the table attached to <typeparamref name="TObject" /> and returns hydrated .NET entitites
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <returns>List of <typeparamref name="TObject" /> objects</returns>
        public async Task<List<TObject>> ExecuteQueryAsync<TObject>(string? partitionKey = null, string? rowKey = null, string? filter = null)
        {
            ContainerTypeInfo container = TypeDiscoveryFactory.Resolve<TObject>();
            IAsyncEnumerable<TableEntity> asyncEnumerable = ExecuteQueryAsyncImplYielder(
                container.Container.Name, 
                    PrepareQueryColumnsList(container), 
                        partitionKey, rowKey, filter);

            List<TObject> items = new List<TObject>();
            await foreach (TableEntity entity in asyncEnumerable)
            {
                items.Add(AzureTablesSerialiser.Deserialise<TObject>(entity));
            }

            return items;
        }

        #endregion

        #region Select

        /// <summary>
        /// Execute the query against the <see cref="TableClient" />
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <returns>List of <typeparamref name="TObject" /> objects</returns>
        public async Task<List<TObject>> SelectAsync<TObject>(object? partitionKey = null, object? rowKey = null, string? filter = null)
            => await ExecuteQueryAsync<TObject>(
                    (string?)ReflectionUtils.ConvertValueIfRequired(partitionKey, typeof(string)),
                        (string?)ReflectionUtils.ConvertValueIfRequired(rowKey, typeof(string)),
                            filter);

        /// <summary>
        /// Execute the query against the <see cref="TableClient" />
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <returns>List of <typeparamref name="TObject" /> objects</returns>
        public async Task<List<TObject>> SelectAsync<TObject>(string? partitionKey = null, string? rowKey = null, string? filter = null)
            => await ExecuteQueryAsync<TObject>(partitionKey, rowKey, filter);

        #endregion

        #region Select Single/NULL

        /// <summary>
        /// Execute the query against the <see cref="TableClient" /> and retrieve the only result. If there were no results returned, returns NULL.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">(Optional) Value of PartitionKey</param>
        /// <param name="rowKey">(Optional) Value of RowKey</param>
        /// <param name="filter">(Optional) A valid OData filter string</param>
        /// <returns>A single business object or Null</returns>
        public async Task<TObject?> SelectOnlyResultOrNullAsync<TObject>(string? partitionKey = null, string? rowKey = null, string? filter = null)
        {
            List<TObject> objectList = await SelectAsync<TObject>(partitionKey, rowKey, filter);
            return ((objectList.Count != 0) ? objectList[0] : default);
        }

        /// <summary>
        /// Execute the query against the <see cref="TableClient" /> and retrieve the only result. If there were no results returned, returns NULL.
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
                            filter);

        #endregion
    }
}
