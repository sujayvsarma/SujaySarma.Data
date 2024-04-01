using System;
using System.Collections.Generic;
using System.Linq;

using Azure.Data.Tables;

using SujaySarma.Data.Azure.Tables.Reflection;

namespace SujaySarma.Data.Azure.Tables
{

    /*
        This file performs non-generics, synchronous operations
    */


    /// <summary>
    /// A completely connection-less approach to interacting with Azure Tables. Supports both 
    /// Azure Storage Tables and Azure Cosmos DB with Tables API.
    /// </summary>
    public partial class AzureTablesContext
    {
        #region Table-level operations

        /// <summary>
        /// List all tables
        /// </summary>
        /// <returns>List of names of all tables</returns>
        public IEnumerable<string> ListTables() =>
            _serviceClient.Query().Select(t => t.Name);


        /// <summary>
        /// Drop table -- does not check if it already exists, may throw an error.
        /// </summary>
        /// <param name="tableName">Name of table to drop</param>
        public void DropTable(string tableName)
            => _serviceClient.DeleteTable(tableName);

        /// <summary>
        /// Create a table (only if it does not exist already)
        /// </summary>
        /// <param name="tableName">Name of table to create</param>
        public void CreateTable(string tableName)
            => _serviceClient.CreateTableIfNotExists(tableName);


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
        public void ClearTable(string tableName)
        {
            if (_serviceClient.Query($"TableName eq '{tableName}'").Any())
            {
                _serviceClient.DeleteTable(tableName);

                // wait for table to disappear, can take at least 5secs
                while (TableExists(tableName))
                {
                    System.Threading.Thread.Sleep(1000);
                }

                _serviceClient.CreateTable(tableName);
            }
        }

        /// <summary>
        /// Clear data for a specified <paramref name="partitionKey"/>
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

            TableClient client = GetTableReference(tableName);
            List<TableTransactionAction> batch = new();

            // fetch only base Entity fields
            string filter = $"PartitionKey eq '{partitionKey}'";
            List<string> columns = new() { "PartitionKey", "RowKey", "ETag" };
            if (useSoftDelete)
            {
                // dont fetch already soft-deleted rows
                filter = $"{filter} and {TypeMetadata.ISDELETED_COLUMN_NAME} eq false";

                // ensure we fetch the soft-delete column if it should exist
                columns.Add(TypeMetadata.ISDELETED_COLUMN_NAME);
            }

            foreach (TableEntity e in client.Query<TableEntity>(filter, select: columns))
            {
                if (useSoftDelete)
                {
                    e[TypeMetadata.ISDELETED_COLUMN_NAME] = true;
                    batch.Add(new TableTransactionAction(TableTransactionActionType.UpdateMerge, e));
                }
                else
                {
                    batch.Add(new TableTransactionAction(TableTransactionActionType.Delete, e));
                }
            }

            client.SubmitTransaction(batch);
        }

        #endregion
    }
}