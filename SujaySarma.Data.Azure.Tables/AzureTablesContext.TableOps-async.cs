using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Azure.Data.Tables;
using Azure.Data.Tables.Models;

namespace SujaySarma.Data.Azure.Tables
{

    /*
        Table-level operations (Asynchronous)
    */

    public partial class AzureTablesContext
    {

        /// <summary>
        /// List all tables
        /// </summary>
        /// <returns>List of names of all tables</returns>
        public async Task<List<string>> ListTablesAsync()
        {
            List<string> tableNames = new List<string>();
            await foreach (TableItem table in _serviceClient.QueryAsync())
            {
                tableNames.Add(table.Name);
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
                // eat any error
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
                // eat any error
            }
        }

        /// <summary>
        /// Check if table exists
        /// </summary>
        /// <param name="tableName">Name of table to check</param>
        public async Task<bool> TableExistsAsync(string tableName)
        {
            await foreach(TableItem table in _serviceClient.QueryAsync($"TableName eq '{tableName}'"))
            {
                if (table.Name.Equals(tableName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

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
            if (await TableExistsAsync(tableName))
            {
                await _serviceClient.DeleteTableAsync(tableName);

                // wait for table to disappear, can take at least 5secs
                while (await TableExistsAsync(tableName))
                {
                    System.Threading.Thread.Sleep(1000);
                }

                await _serviceClient.CreateTableAsync(tableName);
            }
        }

        /// <summary>
        /// Clear data for a specified <paramref name="partitionKey"/>
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

            // fetch only base Entity fields
            string filter = $"{ReservedNames.PartitionKey} eq '{partitionKey}'";
            List<string> columns = new List<string>() { ReservedNames.PartitionKey, ReservedNames.RowKey, ReservedNames.ETag };
            if (useSoftDelete)
            {
                // dont fetch already soft-deleted rows
                filter = $"{filter} and {SujaySarma.Data.Core.ReservedNames.IsDeleted} ne true";        // allow for IsDeleted = NULL

                // ensure we fetch the soft-delete column if it should exist
                columns.Add(SujaySarma.Data.Core.ReservedNames.IsDeleted);
            }

            // Potentially huge number of records to clear. Let's use our auto-batching logic!
            await ExecuteNonQueryAsyncImpl(tableName, 
                    await ExecuteQueryAsyncImpl(tableName, columns, filter: filter), 
                    ((useSoftDelete == true) ? TableTransactionActionType.UpdateMerge : TableTransactionActionType.Delete),
                    
                    // We are already setting the right flag in the inline-IF in the previous parameter
                    DeleteAction.NotApplicable
                );
        }
    }
}