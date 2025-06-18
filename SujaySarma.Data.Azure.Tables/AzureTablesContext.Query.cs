using Azure.Data.Tables;

using SujaySarma.Data.Core;

using System.Collections.Generic;
using System.Text;


namespace SujaySarma.Data.Azure.Tables
{
    /// <summary>
    /// Query operations (sync)
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
        public List<TableEntity> ExecuteQueryRaw(string tableName, List<string> selectColumns, object? partitionKey = null, object? rowKey = null, string? filter = null)
            => ExecuteQueryImpl(tableName,
                    selectColumns,
                        (string?) ReflectionUtils.ConvertValueIfRequired(partitionKey, typeof(string)), 
                            (string?) ReflectionUtils.ConvertValueIfRequired(rowKey, typeof(string)), 
                                filter);

        /// <summary>
        /// Execute a query against the given table and return the raw table entities
        /// </summary>
        /// <param name="tableName">Name of the Azure Table to query</param>
        /// <param name="selectColumns">List of names of columns to select from the table</param>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        public List<TableEntity> ExecuteQueryRaw(string tableName, List<string> selectColumns, string? partitionKey = null, string? rowKey = null, string? filter = null)
            => ExecuteQueryImpl(tableName, selectColumns, partitionKey, rowKey, filter);

        #endregion

        #region Execute Query

        /// <summary>
        /// Implementation function for synchronous Query (Select) actions
        /// </summary>
        /// <param name="tableName">Name of the Azure Table being queried</param>
        /// <param name="includedColumns">List of names of table columns to include in the result</param>
        /// <param name="partitionKey">Partition key of the table data</param>
        /// <param name="rowKey">Row key for the table row (POINT query)</param>
        /// <param name="filter">Pre-composed string with query filter (Where clause) if applicable</param>
        /// <param name="includeSoftDeletedRows">When set, includes columns with IsDeleted=TRUE</param>
        /// <returns>Enumeration of resulting rows</returns>
        private List<TableEntity> ExecuteQueryImpl(string tableName, List<string> includedColumns, string? partitionKey = null, string? rowKey = null, string? filter = null, bool includeSoftDeletedRows = false)
        {
            StringBuilder stringBuilder = PrepareQueryFilterString(partitionKey, rowKey, filter, includeSoftDeletedRows);
            TableClient tableReference = GetTableReference(tableName);
            List<TableEntity> tableEntityList = new List<TableEntity>();

            foreach (TableEntity tableEntity in tableReference.Query<TableEntity>(stringBuilder.ToString(), null, includedColumns))
            {
                tableEntityList.Add(tableEntity);
            }

            return tableEntityList;
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
        private IEnumerable<TableEntity> ExecuteQueryImplYielder(string tableName, List<string> includedColumns, string? partitionKey = null, string? rowKey = null, string? filter = null, bool includeSoftDeletedRows = false)
        {
            StringBuilder stringBuilder = PrepareQueryFilterString(partitionKey, rowKey, filter, includeSoftDeletedRows);
            TableClient tableReference = GetTableReference(tableName);

            foreach (TableEntity tableEntity in tableReference.Query<TableEntity>(stringBuilder.ToString(), null, includedColumns))
            {
                yield return tableEntity;
            }
        }

        #endregion
    }
}
