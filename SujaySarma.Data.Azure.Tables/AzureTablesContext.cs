using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Azure.Data.Tables;

using SujaySarma.Data.Azure.Tables.Attributes;
using SujaySarma.Data.Azure.Tables.Reflection;
using SujaySarma.Data.Core.Reflection;

namespace SujaySarma.Data.Azure.Tables
{

    /*
        This file has the common struff like constructors, common fields, etc
    */

    /// <summary>
    /// A completely connection-less approach to interacting with Azure Tables. Supports both 
    /// Azure Storage Tables and Azure Cosmos DB with Tables API.
    /// </summary>
    public partial class AzureTablesContext
    {

        #region Implementation functions

        /// <summary>
        /// (Async) Perform a batched transaction based non-query (IUD) operation
        /// </summary>
        /// <param name="tableName">Name of the table to perform the operation against</param>
        /// <param name="type">Type of IUD operation to perform</param>
        /// <param name="entities">Collection of entities to insert/update/delete</param>
        /// <param name="tableUsesSoftDelete">(Default: true) Set 'false' if the table does not use soft-delete</param>
        /// <param name="forDeletePerformHardDelete">(Default: false) Set 'true' if rows need to be actually (hard) deleted</param>
        /// <returns>A tuple with the operation statistics and failure messages</returns>
        private async Task<(long Passed, long Failed, List<string> Messages)> ExecuteNonQueryAsyncImpl(string tableName, OperationType type, IEnumerable<TableEntity> entities, bool tableUsesSoftDelete = true, bool forDeletePerformHardDelete = false)
        {
            // key is the PartitionKey
            Dictionary<string, List<TableTransactionAction>> transactionsByPartitionKey = new();

            TableTransactionActionType transactionType = type switch
            {
                OperationType.Insert => TableTransactionActionType.Add,
                OperationType.Update => TableTransactionActionType.UpdateMerge,
                OperationType.Upsert => TableTransactionActionType.UpsertMerge,
                OperationType.Delete when (tableUsesSoftDelete && (!forDeletePerformHardDelete)) => TableTransactionActionType.UpdateMerge,
                OperationType.Delete => TableTransactionActionType.Delete,
                _ => throw new System.NotImplementedException()
            };

            // group transactions by partition key
            foreach (TableEntity entity in entities)
            {
                string partitionKey = entity.PartitionKey.ToUpperInvariant();
                transactionsByPartitionKey.TryAdd(partitionKey, new());

                if ((type == OperationType.Delete) && tableUsesSoftDelete && (!forDeletePerformHardDelete))
                {
                    entity[SujaySarma.Data.Core.ReservedNames.IsDeleted] = true;
                }
                transactionsByPartitionKey[partitionKey].Add(new(transactionType, entity));
            }

            TableClient client = await GetTableReferenceAsync(tableName);

            long passed = 0, failed = 0;
            List<string> messages = new();
            int currentLoopStartIndex, batchSize = 0;
            foreach (string partitionKey in transactionsByPartitionKey.Keys)
            {
                currentLoopStartIndex = 0;
                while ((transactionsByPartitionKey[partitionKey].Count > 0) && (currentLoopStartIndex < transactionsByPartitionKey[partitionKey].Count))
                {
                    try
                    {
                        // transaction limit is 100 rows at a time
                        if (transactionsByPartitionKey[partitionKey].Count <= 100)
                        {
                            batchSize = transactionsByPartitionKey[partitionKey].Count;
                            await client.SubmitTransactionAsync(transactionsByPartitionKey[partitionKey]);

                            passed += batchSize;
                            transactionsByPartitionKey[partitionKey].Clear();
                            continue;
                        }

                        List<TableTransactionAction> batch = transactionsByPartitionKey[partitionKey].Skip(currentLoopStartIndex).Take(100).ToList();

                        batchSize = batch.Count;
                        await client.SubmitTransactionAsync(batch);

                        currentLoopStartIndex += batchSize;
                        passed += batchSize;
                    }
                    catch (TableTransactionFailedException ttfe) when ((ttfe.Status == (int)HttpStatusCode.Conflict) || (ttfe.Status == (int)HttpStatusCode.BadRequest))
                    {
                        List<string> logMessage = new()
                        {
                            ttfe.Message,
                            $"-> PartitionKey = '{partitionKey}'"
                        };

                        if ((ttfe.FailedTransactionActionIndex == null) || (transactionsByPartitionKey[partitionKey].Count == 1))
                        {
                            logMessage.Add($"--> RowKey = '{transactionsByPartitionKey[partitionKey][0].Entity.RowKey}'");
                            transactionsByPartitionKey[partitionKey].Clear();
                        }
                        else if (ttfe.FailedTransactionActionIndex != null)
                        {
                            // transaction fails at first failure. So we only get ONE index at a time.
                            logMessage.Add($"--> RowKey = '{transactionsByPartitionKey[partitionKey][ttfe.FailedTransactionActionIndex!.Value].Entity.RowKey}'");
                            transactionsByPartitionKey[partitionKey].RemoveAt(ttfe.FailedTransactionActionIndex!.Value);
                        }

                        messages.Add(string.Join("\r\n", logMessage));
                    }
                    catch (Exception e)
                    {
                        List<string> logMessage = new()
                        {
                            e.Message,
                            $"-> PartitionKey = '{partitionKey}'"
                        };

                        // exception is against all the rows of the batch
                        foreach (TableTransactionAction tta in transactionsByPartitionKey[partitionKey])
                        {
                            logMessage.Add($"--> RowKey = '{tta.Entity.RowKey}'");
                        }

                        messages.Add(string.Join("\r\n", logMessage));
                        failed += batchSize;
                        currentLoopStartIndex += batchSize;             // try the next set
                    }
                }
            }

            return (Passed: passed, Failed: failed, Messages: messages);
        }

        /// <summary>
        /// Perform a batched transaction based non-query (IUD) operation
        /// </summary>
        /// <param name="tableName">Name of the table to perform the operation against</param>
        /// <param name="type">Type of IUD operation to perform</param>
        /// <param name="entities">Collection of entities to insert/update/delete</param>
        /// <param name="tableUsesSoftDelete">(Default: true) Set 'false' if the table does not use soft-delete</param>
        /// <param name="forDeletePerformHardDelete">(Default: false) Set 'true' if rows need to be actually (hard) deleted</param>
        /// <returns>A tuple with the operation statistics and failure messages</returns>
        private (long Passed, long Failed, List<string> Messages) ExecuteNonQueryImpl(string tableName, OperationType type, IEnumerable<TableEntity> entities, bool tableUsesSoftDelete = true, bool forDeletePerformHardDelete = false)
        {
            // key is the PartitionKey
            Dictionary<string, List<TableTransactionAction>> transactionsByPartitionKey = new();

            TableTransactionActionType transactionType = type switch
            {
                OperationType.Insert => TableTransactionActionType.Add,
                OperationType.Update => TableTransactionActionType.UpdateMerge,
                OperationType.Upsert => TableTransactionActionType.UpsertMerge,
                OperationType.Delete when (tableUsesSoftDelete && (!forDeletePerformHardDelete)) => TableTransactionActionType.UpdateMerge,
                OperationType.Delete => TableTransactionActionType.Delete,
                _ => throw new System.NotImplementedException()
            };

            // group transactions by partition key
            foreach (TableEntity entity in entities)
            {
                string partitionKey = entity.PartitionKey.ToUpperInvariant();
                transactionsByPartitionKey.TryAdd(partitionKey, new());

                if ((type == OperationType.Delete) && tableUsesSoftDelete && (!forDeletePerformHardDelete))
                {
                    entity[SujaySarma.Data.Core.ReservedNames.IsDeleted] = true;
                }
                transactionsByPartitionKey[partitionKey].Add(new(transactionType, entity));
            }

            TableClient client = GetTableReference(tableName);

            long passed = 0, failed = 0;
            List<string> messages = new();
            int currentLoopStartIndex, batchSize = 0;
            foreach (string partitionKey in transactionsByPartitionKey.Keys)
            {
                currentLoopStartIndex = 0;
                while ((transactionsByPartitionKey[partitionKey].Count > 0) && (currentLoopStartIndex < transactionsByPartitionKey[partitionKey].Count))
                {
                    try
                    {
                        // transaction limit is 100 rows at a time
                        if (transactionsByPartitionKey[partitionKey].Count <= 100)
                        {
                            batchSize = transactionsByPartitionKey[partitionKey].Count;
                            client.SubmitTransaction(transactionsByPartitionKey[partitionKey]);

                            passed += batchSize;
                            transactionsByPartitionKey[partitionKey].Clear();
                            continue;
                        }

                        List<TableTransactionAction> batch = transactionsByPartitionKey[partitionKey].Skip(currentLoopStartIndex).Take(100).ToList();

                        batchSize = batch.Count;
                        client.SubmitTransaction(batch);

                        currentLoopStartIndex += batchSize;
                        passed += batchSize;
                    }
                    catch (TableTransactionFailedException ttfe) when ((ttfe.Status == (int)HttpStatusCode.Conflict) || (ttfe.Status == (int)HttpStatusCode.BadRequest))
                    {
                        List<string> logMessage = new()
                        {
                            ttfe.Message,
                            $"-> PartitionKey = '{partitionKey}'"
                        };

                        if ((ttfe.FailedTransactionActionIndex == null) || (transactionsByPartitionKey[partitionKey].Count == 1))
                        {
                            logMessage.Add($"--> RowKey = '{transactionsByPartitionKey[partitionKey][0].Entity.RowKey}'");
                            transactionsByPartitionKey[partitionKey].Clear();
                        }
                        else if (ttfe.FailedTransactionActionIndex != null)
                        {
                            // transaction fails at first failure. So we only get ONE index at a time.
                            logMessage.Add($"--> RowKey = '{transactionsByPartitionKey[partitionKey][ttfe.FailedTransactionActionIndex!.Value].Entity.RowKey}'");
                            transactionsByPartitionKey[partitionKey].RemoveAt(ttfe.FailedTransactionActionIndex!.Value);
                        }

                        messages.Add(string.Join("\r\n", logMessage));
                        failed++;
                    }
                    catch (Exception e)
                    {
                        List<string> logMessage = new()
                        {
                            e.Message,
                            $"-> PartitionKey = '{partitionKey}'"
                        };

                        // exception is against all the rows of the batch
                        foreach (TableTransactionAction tta in transactionsByPartitionKey[partitionKey])
                        {
                            logMessage.Add($"--> RowKey = '{tta.Entity.RowKey}'");
                        }

                        messages.Add(string.Join("\r\n", logMessage));
                        failed += batchSize;
                        currentLoopStartIndex += batchSize;             // try the next set
                    }
                }
            }


            return (Passed: passed, Failed: failed, Messages: messages);
        }

        /// <summary>
        /// (Async) Perform a data select query
        /// </summary>
        /// <param name="tableName">Name of the table to query</param>
        /// <param name="columns">List of columns to fetch (Use <see cref="PrepareColumnsList(TypeMetadata, bool)"/> to create)</param>
        /// <param name="filterString">Filter string to execute (Use <see cref="PrepareFilterString(bool, string?, string?, string?, bool)"/> to prepare)</param>
        /// <returns><see cref="IEnumerable{TableEntity}"/> with the results.</returns>
        private async Task<List<TableEntity>> QueryAsync(string tableName, IEnumerable<string> columns, string? filterString = null)
        {
            IAsyncEnumerable<TableEntity> values = GetTableReference(tableName).QueryAsync<TableEntity>(filter: filterString, select: columns);

            // Convert IAsyncEnumerable to a regular List
            List<TableEntity> list = new();
            await foreach (TableEntity item in values)
            {
                list.Add(item);
            }

            return list;
        }

        /// <summary>
        /// Perform a data select query
        /// </summary>
        /// <param name="tableName">Name of the table to query</param>
        /// <param name="columns">List of columns to fetch (Use <see cref="PrepareColumnsList(TypeMetadata, bool)"/> to create)</param>
        /// <param name="filterString">Filter string to execute (Use <see cref="PrepareFilterString(bool, string?, string?, string?, bool)"/> to prepare)</param>
        /// <returns><see cref="IEnumerable{TableEntity}"/> with the results.</returns>
        private IEnumerable<TableEntity> Query(string tableName, IEnumerable<string> columns, string? filterString = null)
            => GetTableReference(tableName).Query<TableEntity>(filter: filterString, select: columns);

        /// <summary>
        /// Create the string to be used for a query filter
        /// </summary>
        /// <param name="tableUsesSoftDelete">Set to 'true' if the target table has the IsDeleted flag to signal a soft delete</param>
        /// <param name="partitionKey">(Optional) value of Partition Key</param>
        /// <param name="rowKey">(Optional) value of Row Key</param>
        /// <param name="filter">(Optional) any OData filter string</param>
        /// <param name="fetchSoftDeletedRows">(Default: false) Set to 'true' if it is required to fetch hard-deleted rows as well</param>
        /// <returns>String containing an OData filter string</returns>
        private static string PrepareFilterString(bool tableUsesSoftDelete = true, string? partitionKey = null, string? rowKey = null, string? filter = null, bool fetchSoftDeletedRows = false)
        {
            StringBuilder query = new();
            if (tableUsesSoftDelete && (!fetchSoftDeletedRows))
            {
                query.Append($"{SujaySarma.Data.Core.ReservedNames.IsDeleted} eq false");
            }

            AppendQueryFilterIfNotNullEmptyOrWhitespace(query, ReservedNames.PartitionKey, partitionKey);
            AppendQueryFilterIfNotNullEmptyOrWhitespace(query, ReservedNames.RowKey, rowKey);
            AppendQueryFilterIfNotNullEmptyOrWhitespace(query, string.Empty, filter);           

            return query.ToString();

            // Append the strings to the StringBuilder instance:
            //  - queryBuilder : the StringBuilder instance
            //  - filterName   : name of the Azure Tables column (leave empty to use only 'value')
            //  - value        : the string to add
            static void AppendQueryFilterIfNotNullEmptyOrWhitespace(StringBuilder queryBuilder, string filterName, string? value)
            {
                if (! string.IsNullOrWhiteSpace(value))
                {
                    if (queryBuilder.Length > 0)
                    {
                        queryBuilder.Append(" and ");
                    }

                    if (string.IsNullOrWhiteSpace(filterName))
                    {
                        queryBuilder.Append($"{value}");
                    }
                    else
                    {
                        queryBuilder.Append($"{filterName} eq '{value}'");
                    }
                }
            }
        }

        /// <summary>
        /// Prepare the list of columns to be fetched
        /// </summary>
        /// <param name="metadata">Metadata retrieved by the reflector</param>
        /// <param name="minimalSurfaceOnly">(Default: false) Set to 'true' to retrieve only the minimal set of columns (i.e., base TableEntity fields)</param>
        /// <returns></returns>
        private static List<string> PrepareColumnsList(ContainerTypeInformation metadata, bool minimalSurfaceOnly = false)
        {
            List<string> columns = new();
            columns.AddRange(ReservedNames.All);

            if (!minimalSurfaceOnly)
            {
                foreach (ContainerMemberTypeInformation member in metadata.Members.Values)
                {
                    TableColumnAttribute? columnAttribute = member.FieldOrPropertyInfo.GetCustomAttribute<TableColumnAttribute>();
                    if ((columnAttribute != default) && (!string.IsNullOrWhiteSpace(columnAttribute.Name)))
                    {
                        columns.Add(columnAttribute.Name);
                    }
                }
            }

            return columns;
        }

        /// <summary>
        /// Get reference to a table
        /// </summary>
        /// <param name="tableName">Name of table to connect to</param>
        /// <param name="doNotCreate">If NOT set (default), will create the table if it does not exist</param>
        /// <returns></returns>
        private TableClient GetTableReference(string tableName, bool doNotCreate = false)
        {
            TableClient tableClient = new TableClient(_connectionString, tableName);
            if (!doNotCreate) { tableClient.CreateIfNotExists(); }

            return tableClient;
        }

        #endregion      

        #region Constructors

        /// <summary>
        /// Instantiate using Development Storage
        /// </summary>
        public AzureTablesContext()
            : this(DevelopmentStorageConnectionString)
        {
        }

        /// <summary>
        /// Instantiate using provided account name and secret
        /// </summary>
        /// <param name="accountName">Account Name</param>
        /// <param name="accountKey">Account Key/secret</param>
        public AzureTablesContext(string accountName, string accountKey)
            : this($"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey};EndpointSuffix=core.windows.net")
        {
        }

        /// <summary>
        /// Instantiate using provided connection string
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        public AzureTablesContext(string connectionString)
        {
            if (connectionString.Equals(UseDevelopmentStorage))
            {
                connectionString = DevelopmentStorageConnectionString;
            }

            _connectionString = connectionString;
            _serviceClient = new(connectionString);
        }

        /// <summary>
        /// Get a context based on the Development Storage
        /// </summary>
        /// <returns>AzureTablesContext</returns>
        public static AzureTablesContext WithDevelopmentStorage()
            => (new AzureTablesContext());

        /// <summary>
        /// Get a context based on the provided connection string
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <returns>AzureTablesContext</returns>
        public static AzureTablesContext WithConnectionString(string connectionString)
            => (new AzureTablesContext(connectionString));

        #endregion

        #region Private fields

        private readonly string _connectionString;
        private readonly TableServiceClient _serviceClient;
        private const string UseDevelopmentStorage = "UseDevelopmentStorage=true";
        private const string DevelopmentStorageConnectionString = "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";

        #endregion
    }
}