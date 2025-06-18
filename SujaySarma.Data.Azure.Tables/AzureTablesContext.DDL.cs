using Azure.Data.Tables;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SujaySarma.Data.Azure.Tables
{
    // Table-level operations
    public partial class AzureTablesContext
    {
        /// <summary>Get reference to a table</summary>
        /// <param name="tableName">Name of table to connect to</param>
        /// <param name="doNotCreate">If NOT set (default), will create the table if it does not exist</param>
        /// <returns></returns>
        private TableClient GetTableReference(string tableName, bool doNotCreate = false)
        {
            TableClient tableReference = new TableClient(_connectionString, tableName);
            if (!doNotCreate)
            {
                tableReference.CreateIfNotExists();
            }
            return tableReference;
        }

        /// <summary>
        /// List all tables
        /// </summary>
        /// <returns>List of names of all tables</returns>
        public List<string> ListTables()
            => _serviceClient.Query().Select(t => t.Name).ToList();

        /// <summary>
        /// Drop table -- does not check if it already exists.
        /// </summary>
        /// <param name="tableName">Name of table to drop</param>
        public void DropTable(string tableName)
        {
            try
            {
                _serviceClient.DeleteTable(tableName);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Create a table (only if it does not exist already)
        /// </summary>
        /// <param name="tableName">Name of table to create</param>
        public void CreateTable(string tableName)
        {
            try
            {
                _serviceClient.CreateTableIfNotExists(tableName);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Check if table exists
        /// </summary>
        /// <param name="tableName">Name of table to check</param>
        public bool TableExists(string tableName) 
            => _serviceClient.Query($"TableName eq '{tableName}'").Any();

        /// <summary>
        /// Clear all rows from a table -- works by dropping the table. Caller must ensure that
        /// nothing else tries to write to the table until this call returns.
        /// </summary>
        /// <param name="tableName">Name of table to clear</param>
        /// <remarks>
        ///     This method will sleep for 1000ms between table deletes
        /// </remarks>
        public void ClearTable(string tableName)
        {
            if (!TableExists(tableName))
            {
                return;
            }
            DropTable(tableName);

            // Wait for table to actually disappear
            while (TableExists(tableName))
            {
                Thread.Sleep(1000);
            }

            // We can re-create it now.
            CreateTable(tableName);
        }

        /// <summary>
        /// Clear data for a specified <paramref name="partitionKey" />
        /// </summary>
        /// <param name="tableName">Name of table</param>
        /// <param name="partitionKey">Partition key to clear data for</param>
        /// <param name="useSoftDelete">Use soft delete. If not set, hard-deletes the row</param>
        public void ClearPartition(string tableName, string partitionKey, bool useSoftDelete = true)
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

            ExecuteNonQueryImpl(
                tableName,
                    ExecuteQueryImpl(tableName, includedColumns, filter: filter),
                        (useSoftDelete ? TableTransactionActionType.UpdateMerge : TableTransactionActionType.Delete));
        }
    }
}
