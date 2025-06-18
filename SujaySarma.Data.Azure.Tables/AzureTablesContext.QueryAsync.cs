using Azure.Data.Tables;

using SujaySarma.Data.Core;

using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace SujaySarma.Data.Azure.Tables
{
    /// <summary>
    /// Query operations (async)
    /// </summary>
    public partial class AzureTablesContext
    {
        #region Execute Query (Raw)

        /// <summary>
        /// Execute a query against the given table and return the raw table entities
        /// </summary>
        /// <param name="tableName">Name of the Azure Table to query</param>
        /// <param name="selectColumns">List of names of columns to select from the table</param>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        public async Task<List<TableEntity>> ExecuteQueryRawAsync(string tableName, List<string> selectColumns, string? partitionKey = null, string? rowKey = null, string? filter = null)
            => await ExecuteQueryAsyncImpl(tableName, selectColumns, partitionKey, rowKey, filter);

        /// <summary>
        /// Execute a query against the given table and return the raw table entities
        /// </summary>
        /// <param name="tableName">Name of the Azure Table to query</param>
        /// <param name="selectColumns">List of names of columns to select from the table</param>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        public async Task<List<TableEntity>> ExecuteQueryRawAsync(string tableName, List<string> selectColumns, object? partitionKey = null, object? rowKey = null, string? filter = null)
            => await ExecuteQueryAsyncImpl(
                tableName,
                    selectColumns,
                        (string?)ReflectionUtils.ConvertValueIfRequired(partitionKey, typeof(string)),
                            (string?)ReflectionUtils.ConvertValueIfRequired(rowKey, typeof(string)),
                                filter);

        #endregion

        #region Execute Query

        /// <summary>
        /// Implementation function for asynchronous Query (Select) actions
        /// </summary>
        /// <param name="tableName">Name of the Azure Table being queried</param>
        /// <param name="includedColumns">List of names of table columns to include in the result</param>
        /// <param name="partitionKey">Partition key of the table data</param>
        /// <param name="rowKey">Row key for the table row (POINT query)</param>
        /// <param name="filter">Pre-composed string with query filter (Where clause) if applicable</param>
        /// <param name="includeSoftDeletedRows">When set, includes columns with IsDeleted=TRUE</param>
        /// <returns>Enumeration of resulting rows</returns>
        private async Task<List<TableEntity>> ExecuteQueryAsyncImpl(string tableName, List<string> includedColumns, string? partitionKey = null, string? rowKey = null, string? filter = null, bool includeSoftDeletedRows = false)
        {
            StringBuilder stringBuilder = PrepareQueryFilterString(partitionKey, rowKey, filter, includeSoftDeletedRows);
            TableClient tableReference = GetTableReference(tableName);
            List<TableEntity> tableEntities = new List<TableEntity>();
            TableClient tableClient = tableReference;

            await foreach (TableEntity tableEntity in tableClient.QueryAsync<TableEntity>(stringBuilder.ToString(), null, includedColumns))
            {
                tableEntities.Add(tableEntity);
            }

            return tableEntities;
        }

        /// <summary>
        /// (YIELD RETURN) Implementation function for synchronous Query (Select) actions
        /// </summary>
        /// <param name="tableName">Name of the Azure Table being queried</param>
        /// <param name="includedColumns">List of names of table columns to include in the result</param>
        /// <param name="partitionKey">Partition key of the table data</param>
        /// <param name="rowKey">Row key for the table row (POINT query)</param>
        /// <param name="filter">Pre-composed string with query filter (Where clause) if applicable</param>
        /// <param name="includeSoftDeletedRows">When set, includes columns with IsDeleted=TRUE</param>
        /// <returns>Yields an enumeration of resulting rows</returns>
        private async IAsyncEnumerable<TableEntity> ExecuteQueryAsyncImplYielder(string tableName, List<string> includedColumns, string? partitionKey = null, string? rowKey = null, string? filter = null, bool includeSoftDeletedRows = false)
        {
            StringBuilder stringBuilder = PrepareQueryFilterString(partitionKey, rowKey, filter, includeSoftDeletedRows);
            TableClient tableReference = GetTableReference(tableName);

            await foreach (TableEntity tableEntity in tableReference.QueryAsync<TableEntity>(stringBuilder.ToString(), null, includedColumns))
            {
                yield return tableEntity;
            }
        }

        #endregion
    }
}
