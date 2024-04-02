using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Azure.Data.Tables;

using SujaySarma.Data.Core;
using SujaySarma.Data.Core.Reflection;

namespace SujaySarma.Data.Azure.Tables
{

    /*
        Private implementations of operations
    */

    public partial class AzureTablesContext
    {

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
            StringBuilder filterClause = PrepareQueryFilterString(partitionKey, rowKey, filter, includeSoftDeletedRows);
            TableClient client = GetTableReference(tableName);

            List<TableEntity> tableEntities = new List<TableEntity>();
            await foreach(TableEntity entity in client.QueryAsync<TableEntity>(filter: filterClause.ToString(), select: includedColumns))
            {
                tableEntities.Add(entity);
            }

            return tableEntities;
        }

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
            StringBuilder filterClause = PrepareQueryFilterString(partitionKey, rowKey, filter, includeSoftDeletedRows);
            TableClient client = GetTableReference(tableName);

            List<TableEntity> tableEntities = new List<TableEntity>();
            foreach (TableEntity entity in client.Query<TableEntity>(filter: filterClause.ToString(), select: includedColumns))
            {
                tableEntities.Add(entity);
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
        private IEnumerable<TableEntity> ExecuteQueryImplYielder(string tableName, List<string> includedColumns, string? partitionKey = null, string? rowKey = null, string? filter = null, bool includeSoftDeletedRows = false)
        {
            StringBuilder filterClause = PrepareQueryFilterString(partitionKey, rowKey, filter, includeSoftDeletedRows);
            TableClient client = GetTableReference(tableName);

            foreach (TableEntity entity in client.Query<TableEntity>(filter: filterClause.ToString(), select: includedColumns))
            {
                yield return entity;
            }
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
            StringBuilder filterClause = PrepareQueryFilterString(partitionKey, rowKey, filter, includeSoftDeletedRows);
            TableClient client = GetTableReference(tableName);

            await foreach (TableEntity entity in client.QueryAsync<TableEntity>(filter: filterClause.ToString(), select: includedColumns))
            {
                yield return entity;
            }
        }

        /// <summary>
        /// Implementation function for Synchronous Non-Query (Insert, Update, Delete) actions
        /// </summary>
        /// <param name="tableName">Name of the table being acted on</param>
        /// <param name="entities">The pre-populated <see cref="TableEntity"/> records to insert, update or delete</param>
        /// <param name="operationType">Type of non-query action</param>
        /// <param name="deleteAction">If a delete operation, whether to perform a soft or a hard delete</param>
        /// <returns>Results of the operation</returns>
        private async Task<TransactionResult<TableEntity>> ExecuteNonQueryAsyncImpl(string tableName, List<TableEntity> entities, TableTransactionActionType operationType, DeleteAction deleteAction = DeleteAction.NotApplicable)
        {
            TransactionResult<TableEntity> results = new TransactionResult<TableEntity>()
            {
                TotalEntities = entities.Count
            };

            Dictionary<string, TransactionBatchManager<TableTransactionAction>> groupedByPartitionKey = GroupEntitiesByPartitionKey(entities, operationType, deleteAction);
            TableClient client = GetTableReference(tableName);

            foreach (string partitionKey in groupedByPartitionKey.Keys)
            {
                TransactionBatchManager<TableTransactionAction> groupForPartitionKey = groupedByPartitionKey[partitionKey];
                while (groupForPartitionKey.ItemsLeft > 0)
                {
                    List<TableTransactionAction> batch = groupForPartitionKey.GetNext();
                    while (batch.Count > 0)
                    {
                        try
                        {
                            await client.SubmitTransactionAsync(batch);

                            results.Passed += batch.Count;
                            batch.Clear();
                        }
                        catch (TableTransactionFailedException ttfe) when ((ttfe.Status == (int)HttpStatusCode.Conflict) || (ttfe.Status == (int)HttpStatusCode.BadRequest))
                        {
                            UpdateResultsAndBatch(partitionKey, ttfe, results, batch);
                        }
                        catch (Exception e)
                        {
                            // This is a more catastrophic failure for the entire batch
                            UpdateResultsAndBatch(partitionKey, e, results, batch);
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Implementation function for Synchronous Non-Query (Insert, Update, Delete) actions
        /// </summary>
        /// <param name="tableName">Name of the table being acted on</param>
        /// <param name="entities">The pre-populated <see cref="TableEntity"/> records to insert, update or delete</param>
        /// <param name="operationType">Type of non-query action</param>
        /// <param name="deleteAction">If a delete operation, whether to perform a soft or a hard delete</param>
        /// <returns>Results of the operation</returns>
        private TransactionResult<TableEntity> ExecuteNonQueryImpl(string tableName, List<TableEntity> entities, TableTransactionActionType operationType, DeleteAction deleteAction = DeleteAction.NotApplicable)
        {
            TransactionResult<TableEntity> results = new TransactionResult<TableEntity>()
            {
                TotalEntities = entities.Count
            };

            Dictionary<string, TransactionBatchManager<TableTransactionAction>> groupedByPartitionKey = GroupEntitiesByPartitionKey(entities, operationType, deleteAction);
            TableClient client = GetTableReference(tableName);

            foreach (string partitionKey in groupedByPartitionKey.Keys)
            {
                TransactionBatchManager<TableTransactionAction> groupForPartitionKey = groupedByPartitionKey[partitionKey];
                while (groupForPartitionKey.ItemsLeft > 0)
                {
                    List<TableTransactionAction> batch = groupForPartitionKey.GetNext();
                    while (batch.Count > 0)
                    {
                        try
                        {
                            client.SubmitTransaction(batch);

                            results.Passed += batch.Count;
                            batch.Clear();
                        }
                        catch (TableTransactionFailedException ttfe) when ((ttfe.Status == (int)HttpStatusCode.Conflict) || (ttfe.Status == (int)HttpStatusCode.BadRequest))
                        {
                            UpdateResultsAndBatch(partitionKey, ttfe, results, batch);
                        }
                        catch (Exception e)
                        {
                            // This is a more catastrophic failure for the entire batch
                            UpdateResultsAndBatch(partitionKey, e, results, batch);
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Update the results information and the batch for any other error
        /// </summary>
        /// <param name="partitionKey">PartitionKey for the batch</param>
        /// <param name="exception">The <see cref="Exception"/></param>
        /// <param name="results">The current results info (<see cref="TransactionBatchManager{TEntity}"/>)</param>
        /// <param name="batch">The current transaction batch</param>
        private void UpdateResultsAndBatch(string partitionKey, Exception exception, TransactionResult<TableEntity> results, List<TableTransactionAction> batch)
        {
            results.Messages.Add($"-> PartitionKey = '{partitionKey}'");
            results.Messages.Add(exception.Message);
            results.Messages.AddRange(batch.Select(x => $"--> RowKey = '{x.Entity.RowKey}'"));

            results.Failed += batch.Count;
            results.FailedEntities.AddRange(batch.Select(x => (TableEntity)x.Entity));
        }


        /// <summary>
        /// Update the results information and the batch for a <see cref="TableTransactionFailedException"/> error
        /// </summary>
        /// <param name="partitionKey">PartitionKey for the batch</param>
        /// <param name="ttfe">The <see cref="TableTransactionFailedException"/></param>
        /// <param name="results">The current results info (<see cref="TransactionBatchManager{TEntity}"/>)</param>
        /// <param name="batch">The current transaction batch</param>
        private void UpdateResultsAndBatch(string partitionKey, TableTransactionFailedException ttfe, TransactionResult<TableEntity> results, List<TableTransactionAction> batch)
        {
            results.Messages.Add($"-> PartitionKey = '{partitionKey}'");
            results.Messages.Add(ttfe.Message);

            // It is always ONE failed item
            results.Failed++;

            if ((!ttfe.FailedTransactionActionIndex.HasValue) || (batch.Count == 1))
            {
                // single item in transaction or entire thing failed: hopeless batch!
                results.Messages.Add($"--> RowKey = '{batch[0].Entity.RowKey}'");
                results.FailedEntities.Add((TableEntity)batch[0].Entity);

                batch.Clear();
            }
            else if (ttfe.FailedTransactionActionIndex.HasValue)
            {
                // The transaction will stop at each point of failure one at a time :-(
                // Remove the indicated failed item and try the batch again!

                results.Messages.Add($"--> RowKey = '{batch[ttfe.FailedTransactionActionIndex.Value].Entity.RowKey}'");
                results.FailedEntities.Add((TableEntity)batch[ttfe.FailedTransactionActionIndex.Value].Entity);

                batch.RemoveAt(ttfe.FailedTransactionActionIndex.Value);
            }
        }


        /// <summary>
        /// Group the provided entities by PartitionKey
        /// </summary>
        /// <param name="entities">List of <see cref="TableEntity"/> objects</param>
        /// <param name="operationType">Type of non-query action</param>
        /// <param name="deleteAction">If a delete operation, whether to perform a soft or a hard delete</param>
        /// <returns>Dictionary where the Key is a PartitionKey and the Value is a collection of <see cref="TransactionBatchManager{TEntity}"/> objects for that PartitionKey</returns>
        private Dictionary<string, TransactionBatchManager<TableTransactionAction>> GroupEntitiesByPartitionKey(List<TableEntity> entities, TableTransactionActionType operationType, DeleteAction deleteAction = DeleteAction.NotApplicable)
        {
            Dictionary<string, TransactionBatchManager<TableTransactionAction>> results = new Dictionary<string, TransactionBatchManager<TableTransactionAction>>();

            // Adjust:
            switch (deleteAction)
            {
                case DeleteAction.SoftDelete:
                    operationType = TableTransactionActionType.UpdateMerge;
                    break;

                case DeleteAction.HardDelete:
                    operationType = TableTransactionActionType.Delete;
                    break;
            }

            foreach (TableEntity entity in entities)
            {
                string partitionKey = entity.PartitionKey.ToUpperInvariant();
                results.TryAdd(partitionKey, new TransactionBatchManager<TableTransactionAction>(TRANSACTION_BATCH_SIZE));

                // do this here!
                if (deleteAction == DeleteAction.SoftDelete)
                {
                    entity[SujaySarma.Data.Core.ReservedNames.IsDeleted] = true;
                }

                results[partitionKey].Add(new TableTransactionAction(operationType, entity));
            }

            return results;
        }

        /// <summary>
        /// Prepares the filter string for a Query operation
        /// </summary>
        /// <param name="partitionKey">Partition key of the table data</param>
        /// <param name="rowKey">Row key for the table row (POINT query)</param>
        /// <param name="filter">Pre-composed string with query filter (Where clause) if applicable</param>
        /// <param name="includeSoftDeletedRows">When set, includes columns with IsDeleted=TRUE</param>
        /// <returns>Composed filter string (returns a <see cref="StringBuilder"/> instead of a <see cref="string"/> for better memory management)</returns>
        private StringBuilder PrepareQueryFilterString(string? partitionKey = null, string? rowKey = null, string? filter = null, bool includeSoftDeletedRows = false)
        {
            StringBuilder query = new StringBuilder();

            if (includeSoftDeletedRows)
            {
                query.Append($"{SujaySarma.Data.Core.ReservedNames.IsDeleted} eq false");
            }
            
            if (!string.IsNullOrWhiteSpace(partitionKey))
            {
                if (query.Length > 0)
                {
                    query.Append(" and ");
                }
                query.Append($"PartitionKey eq '{partitionKey}'");
            }
            if (!string.IsNullOrWhiteSpace(rowKey))
            {
                if (query.Length > 0)
                {
                    query.Append(" and ");
                }
                query.Append($"RowKey eq '{rowKey}'");
            }
            if (!string.IsNullOrWhiteSpace(filter))
            {
                if (query.Length > 0)
                {
                    query.Append(" and ");
                }
                query.Append($"{filter}");
            }

            return query;
        }

        /// <summary>
        /// Prepares the list of columns to be included in the query
        /// </summary>
        /// <param name="container">Discovered type information of the top-level (Table) object</param>
        /// <returns>List of column names</returns>
        private List<string> PrepareQueryColumnsList(ContainerTypeInformation container)
        {
            List<string> columnNames = new List<string>();
            columnNames.AddRange(ReservedNames.All);
            foreach(ContainerMemberTypeInformation member in container.Members.Values)
            {
                columnNames.Add(member.ContainerMemberDefinition.CreateQualifiedName());
            }

            return columnNames;
        }



        /// <summary>
        /// Type of delete to perform
        /// </summary>
        private enum DeleteAction
        {
            /// <summary>
            /// Not a DELETE operation
            /// </summary>
            NotApplicable = 0,

            /// <summary>
            /// Soft DELETE (sets the IsDeleted column)
            /// </summary>
            SoftDelete = 1,

            /// <summary>
            /// Performs a real DELETE
            /// </summary>
            HardDelete = 2
        }

        /// <summary>
        /// Size of a transactional batch. This is hard-coded at Azure Tables end.
        /// </summary>
        private static int TRANSACTION_BATCH_SIZE = 100;
    }
}