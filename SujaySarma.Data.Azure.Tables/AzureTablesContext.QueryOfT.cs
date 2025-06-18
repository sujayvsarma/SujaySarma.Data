using Azure.Data.Tables;

using SujaySarma.Data.Azure.Tables.Serialisation;
using SujaySarma.Data.Core;
using SujaySarma.Data.Core.Reflection;

using System.Collections.Generic;

namespace SujaySarma.Data.Azure.Tables
{
    /// <summary>
    /// Query operations (sync)
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
        /// <returns>List of <see cref="T:Azure.Data.Tables.TableEntity" /> objects</returns>
        public List<TableEntity> ExecuteQueryRaw<TObject>(string? partitionKey = null, string? rowKey = null, string? filter = null)
        {
            ContainerTypeInfo container = TypeDiscoveryFactory.Resolve<TObject>();
            return ExecuteQueryRaw(container.Container.Name, PrepareQueryColumnsList(container), partitionKey, rowKey, filter);
        }

        /// <summary>
        /// Execute a query against the table attached to <typeparamref name="TObject" /> and return the raw table entities
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <returns>List of <see cref="T:Azure.Data.Tables.TableEntity" /> objects</returns>
        public List<TableEntity> ExecuteQueryRaw<TObject>(object? partitionKey = null, object? rowKey = null, string? filter = null)
        {
            ContainerTypeInfo container = TypeDiscoveryFactory.Resolve<TObject>();
            return ExecuteQueryRaw(
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
        public List<TObject> ExecuteQuery<TObject>(string? partitionKey = null, string? rowKey = null, string? filter = null)
        {
            ContainerTypeInfo container = TypeDiscoveryFactory.Resolve<TObject>();
            IEnumerable<TableEntity> tableEntities = ExecuteQueryImplYielder(
                container.Container.Name, 
                    PrepareQueryColumnsList(container), 
                        partitionKey, rowKey, filter);

            List<TObject> objectList = new List<TObject>();
            foreach (TableEntity entity in tableEntities)
            {
                objectList.Add(AzureTablesSerialiser.Deserialise<TObject>(entity));
            }

            return objectList;
        }

        /// <summary>
        /// Execute a query against the table attached to <typeparamref name="TObject" /> and returns hydrated .NET entitites
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <returns>List of <typeparamref name="TObject" /> objects</returns>
        public List<TObject> ExecuteQuery<TObject>(object? partitionKey = null, object? rowKey = null, string? filter = null)
        {
            ContainerTypeInfo container = TypeDiscoveryFactory.Resolve<TObject>();
            IEnumerable<TableEntity> tableEntities = ExecuteQueryImplYielder(
                container.Container.Name, 
                    PrepareQueryColumnsList(container), 
                        (string?)ReflectionUtils.ConvertValueIfRequired(partitionKey, typeof(string)), 
                            (string?)ReflectionUtils.ConvertValueIfRequired(rowKey, typeof(string)), 
                                filter);

            List<TObject> objectList = new List<TObject>();
            foreach (TableEntity entity in tableEntities)
            {
                objectList.Add(AzureTablesSerialiser.Deserialise<TObject>(entity));
            }

            return objectList;
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
        public List<TObject> Select<TObject>(string? partitionKey = null, string? rowKey = null, string? filter = null)
            => ExecuteQuery<TObject>(partitionKey, rowKey, filter);

        /// <summary>
        /// Execute the query against the <see cref="TableClient" />
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <returns>List of <typeparamref name="TObject" /> objects</returns>
        public List<TObject> Select<TObject>(object? partitionKey = null, object? rowKey = null, string? filter = null)
            => ExecuteQuery<TObject>(
                (string?)ReflectionUtils.ConvertValueIfRequired(partitionKey, typeof(string)),
                    (string?)ReflectionUtils.ConvertValueIfRequired(rowKey, typeof(string)),
                        filter);

        #endregion

        #region Select Single/NULL

        /// <summary>
        /// Execute the query against the <see cref="T:Azure.Data.Tables.TableClient" /> and retrieve the only result. If there were no results returned, returns NULL.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">(Optional) Value of PartitionKey</param>
        /// <param name="rowKey">(Optional) Value of RowKey</param>
        /// <param name="filter">(Optional) A valid OData filter string</param>
        /// <returns>A single business object or Null</returns>
        public TObject? SelectOnlyResultOrNull<TObject>(string? partitionKey = null, string? rowKey = null, string? filter = null)
        {
            List<TObject> objectList = Select<TObject>(partitionKey, rowKey, filter);
            return ((objectList.Count == 0) ? default : objectList[0]);
        }

        /// <summary>
        /// Execute the query against the <see cref="T:Azure.Data.Tables.TableClient" /> and retrieve the only result. If there were no results returned, returns NULL.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">(Optional) Value of PartitionKey</param>
        /// <param name="rowKey">(Optional) Value of RowKey</param>
        /// <param name="filter">(Optional) A valid OData filter string</param>
        /// <returns>A single business object or Null</returns>
        public TObject? SelectOnlyResultOrNull<TObject>(object? partitionKey = null, object? rowKey = null, string? filter = null)
            => SelectOnlyResultOrNull<TObject>(
                (string?)ReflectionUtils.ConvertValueIfRequired(partitionKey, typeof(string)),
                    (string?)ReflectionUtils.ConvertValueIfRequired(rowKey, typeof(string)),
                        filter);

        #endregion

    }
}
