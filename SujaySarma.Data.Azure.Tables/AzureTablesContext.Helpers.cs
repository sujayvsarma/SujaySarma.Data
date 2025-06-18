using Azure.Data.Tables;

using SujaySarma.Data.Azure.Tables.Serialisation;
using SujaySarma.Data.Core;
using SujaySarma.Data.Core.Reflection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SujaySarma.Data.Azure.Tables
{
    // Declarations
    public partial class AzureTablesContext
    {
        /// <summary>Prepares the filter string for a Query operation</summary>
        /// <param name="partitionKey">Partition key of the table data</param>
        /// <param name="rowKey">Row key for the table row (POINT query)</param>
        /// <param name="filter">Pre-composed string with query filter (Where clause) if applicable</param>
        /// <param name="includeSoftDeletedRows">When set, includes columns with IsDeleted=TRUE</param>
        /// <returns>Composed filter string (returns a <see cref="StringBuilder" /> instead of a <see cref="string" /> for better memory management)</returns>
        private static StringBuilder PrepareQueryFilterString(string? partitionKey = null, string? rowKey = null, string? filter = null, bool includeSoftDeletedRows = false)
        {
            StringBuilder sb = new StringBuilder();

            if (includeSoftDeletedRows)
            {
                AppendCondition(sb, $"{Core.ReservedNames.IsDeleted} eq false");
            }

            if (!string.IsNullOrWhiteSpace(partitionKey))
            {
                AppendCondition(sb, $"PartitionKey eq '{partitionKey}'");
            }

            if (!string.IsNullOrWhiteSpace(rowKey))
            {
                AppendCondition(sb, $"RowKey eq '{rowKey}'");
            }

            if (!string.IsNullOrWhiteSpace(filter))
            {
                AppendCondition(sb, filter);
            }

            return sb;


            static void AppendCondition(StringBuilder sb, string condition)
            {
                if (sb.Length > 0)
                {
                    sb.Append(" and ");
                }
                sb.Append(condition);
            }
        }

        /// <summary>
        /// Prepares the list of columns to be included in the query
        /// </summary>
        /// <param name="container">Discovered type information of the top-level (Table) object</param>
        /// <returns>List of column names</returns>
        private static List<string> PrepareQueryColumnsList(ContainerTypeInfo container)
        {
            List<string> stringList = new List<string>();
            stringList.AddRange(ReservedNames.All);

            foreach (MemberTypeInfo memberTypeInformation in container.Members.Values)
            {
                stringList.Add(memberTypeInformation.Column.CreateQualifiedName());
            }

            return stringList;
        }

        /// <summary>Group the provided entities by PartitionKey</summary>
        /// <param name="entities">List of <see cref="TableEntity" /> objects</param>
        /// <param name="operationType">Type of non-query action</param>
        /// <param name="deleteAction">If a delete operation, whether to perform a soft or a hard delete</param>
        /// <returns>Dictionary where the Key is a PartitionKey and the Value is a collection of <see cref="TransactionBatchManager{TableEntity}" /> objects for that PartitionKey</returns>
        private static Dictionary<string, TransactionBatchManager<TableTransactionAction>> GroupEntitiesByPartitionKey(List<TableEntity> entities, TableTransactionActionType operationType, DeleteAction deleteAction = DeleteAction.NotApplicable)
        {
            Dictionary<string, TransactionBatchManager<TableTransactionAction>> dictionary = new Dictionary<string, TransactionBatchManager<TableTransactionAction>>();
            if (operationType != TableTransactionActionType.Delete)
            {
                deleteAction = DeleteAction.NotApplicable;
            }
            else
            {
                switch (deleteAction)
                {
                    case DeleteAction.SoftDelete:
                        operationType = TableTransactionActionType.UpdateMerge;
                        break;

                    case DeleteAction.HardDelete:
                        operationType = TableTransactionActionType.Delete;
                        break;
                }
            }

            foreach (TableEntity entity in entities)
            {
                string key = entity.PartitionKey.ToUpperInvariant();
                if (deleteAction == DeleteAction.SoftDelete)
                {
                    entity[Core.ReservedNames.IsDeleted] = true;
                }

                dictionary.TryAdd(key, new TransactionBatchManager<TableTransactionAction>(TRANSACTION_BATCH_SIZE));
                dictionary[key].Add(new TableTransactionAction(operationType, entity));
            }
            return dictionary;
        }

        /// <summary>
        /// Update the results information and the batch for any other error
        /// </summary>
        /// <param name="partitionKey">PartitionKey for the batch</param>
        /// <param name="exception">The <see cref="Exception" /></param>
        /// <param name="results">The current results info (<see cref="TransactionBatchManager{TableEntity}" />)</param>
        /// <param name="batch">The current transaction batch</param>
        private static void UpdateResultsAndBatch(string partitionKey, Exception exception, TransactionResult<TableEntity> results, List<TableTransactionAction> batch)
        {
            results.Messages.Add($"-> PartitionKey = '{partitionKey}'");
            results.Messages.Add(exception.Message);
            results.Messages.AddRange(batch.Select(x => $"--> RowKey = '{x.Entity.RowKey}'"));
            results.Failed += batch.Count;
            results.FailedEntities.AddRange(batch.Select(x => (TableEntity)x.Entity));
        }

        /// <summary>
        /// Update the results information and the batch for a <see cref="TableTransactionFailedException" /> error
        /// </summary>
        /// <param name="partitionKey">PartitionKey for the batch</param>
        /// <param name="ttfe">The <see cref="TableTransactionFailedException" /></param>
        /// <param name="results">The current results info (<see cref="TransactionBatchManager{TableEntity}" />)</param>
        /// <param name="batch">The current transaction batch</param>
        private static void UpdateResultsAndBatch(string partitionKey, TableTransactionFailedException ttfe, TransactionResult<TableEntity> results, List<TableTransactionAction> batch)
        {
            results.Messages.Add($"-> PartitionKey = '{partitionKey}'");
            results.Messages.Add(ttfe.Message);
            ++results.Failed;

            if ((!ttfe.FailedTransactionActionIndex.HasValue) || (batch.Count == 1))
            {
                results.Messages.Add($"--> RowKey = '{batch[0].Entity.RowKey}'");
                results.FailedEntities.Add((TableEntity)batch[0].Entity);

                batch.Clear();
            }
            else
            {
                if (!ttfe.FailedTransactionActionIndex.HasValue)
                {
                    return;
                }

                results.Messages.Add($"--> RowKey = '{batch[ttfe.FailedTransactionActionIndex.Value].Entity.RowKey}'");
                results.FailedEntities.Add((TableEntity)batch[ttfe.FailedTransactionActionIndex.Value].Entity);

                batch.RemoveAt(ttfe.FailedTransactionActionIndex.Value);
            }
        }


        /// <summary>
        /// Converts the transaction result from TableEntity format to the caller's object format.
        /// </summary>
        /// <typeparam name="TObject">Type of the caller's object.</typeparam>
        /// <param name="result">The transaction result containing TableEntity objects.</param>
        /// <returns>A transaction result containing objects of type <typeparamref name="TObject"/>.</returns>
        private static TransactionResult<TObject> ConvertTransactionResultToCallerFormats<TObject>(TransactionResult<TableEntity> result)
        {
            TransactionResult<TObject> converted = new TransactionResult<TObject>()
            {
                TotalEntities = result.TotalEntities,
                Passed = result.Passed,
                Failed = result.Failed
            };

            converted.Messages.AddRange(result.Messages);

            if ((result.Failed > 0) && (result.FailedEntities.Count > 0))
            {
                foreach (TableEntity failedEntity in result.FailedEntities)
                {
                    converted.FailedEntities.Add(AzureTablesSerialiser.Deserialise<TObject>(failedEntity));
                }
            }

            return converted;
        }

    }
}
