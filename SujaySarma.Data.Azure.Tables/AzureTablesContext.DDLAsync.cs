using Azure.Data.Tables;
using Azure.Data.Tables.Models;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SujaySarma.Data.Azure.Tables
{
    // Async Table-level operations
    public partial class AzureTablesContext
    {

        /// <summary>
        /// List all tables
        /// </summary>
        /// <returns>List of names of all tables</returns>
        public async Task<List<string>> ListTablesAsync()
        {
            List<string> tableNames = new List<string>();
            await foreach (TableItem tableItem in _serviceClient.QueryAsync())
            {
                tableNames.Add(tableItem.Name);
            }

            return tableNames;
        }

        /// <summary>
        /// Drop table
        /// </summary>
        /// <param name="tableName">Name of table to drop</param>
        public async Task DropTableAsync(string tableName)
        {
            try
            {
                await _serviceClient.DeleteTableAsync(tableName);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Create a table (only if it does not exist already)
        /// </summary>
        /// <param name="tableName">Name of table to create</param>
        public async Task CreateTableAsync(string tableName)
        {
            try
            {
                await _serviceClient.CreateTableIfNotExistsAsync(tableName);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Check if table exists
        /// </summary>
        /// <param name="tableName">Name of table to check</param>
        public async Task<bool> TableExistsAsync(string tableName)
            => await _serviceClient.QueryAsync($"TableName eq '{tableName}'").AnyAsync();

        /// <summary>
        /// Clear all rows from a table -- works by dropping the table. Caller must ensure that
        /// nothing else tries to write to the table until this call returns.
        /// </summary>
        /// <param name="tableName">Name of table to clear</param>
        /// <remarks>
        ///     This method will sleep for 1000ms between table deletes
        /// </remarks>
        public async Task ClearTableAsync(string tableName)
        {
            if (!await TableExistsAsync(tableName))
            {
                return;
            }

            await DropTableAsync(tableName);
            while (await TableExistsAsync(tableName) == true)
            {
                await Task.Delay(1000);
            }
            
            await CreateTableAsync(tableName);
        }

        /// <summary>
        /// Clear data for a specified <paramref name="partitionKey" />
        /// </summary>
        /// <param name="tableName">Name of table</param>
        /// <param name="partitionKey">Partition key to clear data for</param>
        /// <param name="useSoftDelete">Use soft delete. If not set, hard-deletes the row</param>
        public async Task ClearPartitionAsync(string tableName, string partitionKey, bool useSoftDelete = true)
        {
            if (string.IsNullOrWhiteSpace(partitionKey))
            {
                throw new ArgumentNullException(nameof(partitionKey));
            }

            string filter = $"{ReservedNames.PartitionKey} eq '{partitionKey}'";
            List<string> includedColumns = new List<string>()
            {
                ReservedNames.PartitionKey,
                ReservedNames.RowKey,
                ReservedNames.ETag
            };

            if (useSoftDelete)
            {
                filter = $"{filter} and {Core.ReservedNames.IsDeleted} ne true";
                includedColumns.Add(Core.ReservedNames.IsDeleted);
            }

            await ExecuteNonQueryAsyncImpl(
                tableName, 
                    await ExecuteQueryAsyncImpl(tableName, includedColumns, filter: filter), 
                        ((useSoftDelete == true) ? TableTransactionActionType.UpdateMerge : TableTransactionActionType.Delete));
        }

    }
}
