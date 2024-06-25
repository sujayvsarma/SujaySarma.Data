using System;
using System.Collections.Generic;
using System.Linq;

using Azure.Data.Tables;

namespace SujaySarma.Data.Azure.Tables
{

    /*
        Table-level operations (Synchronous)
    */

    public partial class AzureTablesContext
    {

        /// <summary>
        /// List all tables
        /// </summary>
        /// <returns>List of names of all tables</returns>
        public List<string> ListTables() =>
            _serviceClient.Query().Select(t => t.Name).ToList();


        /// <summary>
        /// Drop table -- does not check if it already exists, may throw an error.
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
                // eat any error
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
                // eat any error
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

            // fetch only base Entity fields
            string filter = $"{ReservedNames.PartitionKey} eq '{partitionKey}'";
            List<string> columns = new List<string>() { ReservedNames.PartitionKey, ReservedNames.RowKey, ReservedNames.ETag };
            if (useSoftDelete)
            {
                // dont fetch already soft-deleted rows
                filter = $"{filter} and {SujaySarma.Data.Core.ReservedNames.IsDeleted} ne true";    // Allow for IsDeleted = NULL

                // ensure we fetch the soft-delete column if it should exist
                columns.Add(SujaySarma.Data.Core.ReservedNames.IsDeleted);
            }

            // Potentially huge number of records to clear. Let's use our auto-batching logic!
            ExecuteNonQueryImpl(tableName, 
                    ExecuteQueryImpl(tableName, columns, filter: filter), 
                    ((useSoftDelete == true) ? TableTransactionActionType.UpdateMerge : TableTransactionActionType.Delete),
                    
                    // We are already setting the right flag in the inline-IF in the previous parameter
                    DeleteAction.NotApplicable
                );
        }
    }
}