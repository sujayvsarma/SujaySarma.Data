using Azure;
using Azure.Data.Tables;

using SujaySarma.Data.Core;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SujaySarma.Data.Azure.Tables
{
    // Handles ExecNonQuery implementations
    public partial class AzureTablesContext
    {

        #region Sync

        /// <summary>
        /// Implementation function for Synchronous Non-Query (Insert, Update, Delete) actions
        /// </summary>
        /// <param name="tableName">Name of the table being acted on</param>
        /// <param name="entities">The pre-populated <see cref="TableEntity" /> records to insert, update or delete</param>
        /// <param name="operationType">Type of non-query action</param>
        /// <param name="deleteAction">If a delete operation, whether to perform a soft or a hard delete</param>
        /// <returns>Results of the operation</returns>
        private TransactionResult<TableEntity> ExecuteNonQueryImpl(string tableName, List<TableEntity> entities, TableTransactionActionType operationType, DeleteAction deleteAction = DeleteAction.NotApplicable)
        {
            TransactionResult<TableEntity> results = new TransactionResult<TableEntity>()
            {
                TotalEntities = entities.Count
            };

            if (operationType != TableTransactionActionType.Delete)
            {
                deleteAction = DeleteAction.NotApplicable;
            }

            Dictionary<string, TransactionBatchManager<TableTransactionAction>> dictionary = GroupEntitiesByPartitionKey(entities, operationType, deleteAction);
            TableClient tableReference = GetTableReference(tableName);
            foreach (string key in dictionary.Keys)
            {
                TransactionBatchManager<TableTransactionAction> transactionBatchManager = dictionary[key];
                while (transactionBatchManager.ItemsLeft > 0)
                {
                    List<TableTransactionAction> batch = transactionBatchManager.GetNext();
                    while (batch.Count > 0)
                    {
                        try
                        {
                            tableReference.SubmitTransaction(batch);
                            results.Passed += batch.Count;

                            batch.Clear();
                        }
                        catch (TableTransactionFailedException ex) when (((RequestFailedException)ex).Status == 409 || ((RequestFailedException)ex).Status == 400)
                        {
                            UpdateResultsAndBatch(key, ex, results, batch);
                        }
                        catch (Exception ex)
                        {
                            UpdateResultsAndBatch(key, ex, results, batch);
                        }
                    }
                }
            }
            return results;
        }

        #endregion

        #region Async

        /// <summary>
        /// Implementation function for Synchronous Non-Query (Insert, Update, Delete) actions
        /// </summary>
        /// <param name="tableName">Name of the table being acted on</param>
        /// <param name="entities">The pre-populated <see cref="TableEntity" /> records to insert, update or delete</param>
        /// <param name="operationType">Type of non-query action</param>
        /// <param name="deleteAction">If a delete operation, whether to perform a soft or a hard delete</param>
        /// <returns>Results of the operation</returns>
        private async Task<TransactionResult<TableEntity>> ExecuteNonQueryAsyncImpl(string tableName, List<TableEntity> entities, TableTransactionActionType operationType, DeleteAction deleteAction = DeleteAction.NotApplicable)
        {
            TransactionResult<TableEntity> results = new TransactionResult<TableEntity>()
            {
                TotalEntities = entities.Count
            };

            if (operationType != TableTransactionActionType.Delete)
            {
                deleteAction = DeleteAction.NotApplicable;
            }

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
                            Response<IReadOnlyList<Response>> response = await client.SubmitTransactionAsync(batch);
                            results.Passed += batch.Count;
                            batch.Clear();
                        }
                        catch (TableTransactionFailedException ex) when (((RequestFailedException)ex).Status == 409 || ((RequestFailedException)ex).Status == 400)
                        {
                            UpdateResultsAndBatch(partitionKey, ex, results, batch);
                        }
                        catch (Exception ex)
                        {
                            UpdateResultsAndBatch(partitionKey, ex, results, batch);
                        }
                    }

                    batch.Clear();
                }

                groupForPartitionKey.Clear();
            }

            return results;
        }

        #endregion

    }
}
