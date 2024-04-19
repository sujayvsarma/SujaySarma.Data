using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azure.Data.Tables;

using SujaySarma.Data.Azure.Tables.Serialisation;
using SujaySarma.Data.Core;
using SujaySarma.Data.Core.Reflection;

namespace SujaySarma.Data.Azure.Tables
{

    /*

        DML (insert, update, delete, merge, etc) operations. Asynchronous.

    */

    public partial class AzureTablesContext
    {

        /// <summary>
        /// Insert a single item into an Azure Table
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="obj">Instance of object with the data to insert</param>
        /// <returns>Results of the transaction</returns>
        public async Task<TransactionResult<TObject>> InsertAsync<TObject>(TObject obj)
        {
            if (obj == null)
            {
                return TransactionResult<TObject>.Default;
            }

            ContainerTypeInformation typeInfo = TypeDiscoveryFactory.Resolve<TObject>() ?? throw new TypeLoadException($"Type '{typeof(TObject).Name}' is not appropriately decorated.");
            TableEntity entity = AzureTablesSerialiser.Serialise(obj, false);

            TransactionResult<TableEntity> execResults = await ExecuteNonQueryAsyncImpl(
                    typeInfo.ContainerDefinition.Name,
                    new List<TableEntity>(1) { entity },
                    TableTransactionActionType.Add,
                    DeleteAction.NotApplicable
                );

            TransactionResult<TObject> result = new TransactionResult<TObject>()
            {
                Passed = execResults.Passed,
                Failed = execResults.Failed,
                TotalEntities = 1
            };
            result.Messages.AddRange(execResults.Messages);
            if (execResults.Failed == 1)
            {
                result.FailedEntities.Add(obj);
            }
            return result;
        }

        /// <summary>
        /// Insert a collection of items into an Azure Table
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="objects">Collection of items to insert</param>
        /// <returns>Results of the transaction</returns>
        public async Task<TransactionResult<TObject>> InsertAsync<TObject>(IEnumerable<TObject> objects)
        {
            ContainerTypeInformation typeInfo = TypeDiscoveryFactory.Resolve<TObject>() ?? throw new TypeLoadException($"Type '{typeof(TObject).Name}' is not appropriately decorated."); 
            TransactionResult<TableEntity> execResults = await ExecuteNonQueryAsyncImpl(
                    typeInfo.ContainerDefinition.Name,
                    AzureTablesSerialiser.Serialise<TObject>(objects, false, false),
                    TableTransactionActionType.Add,
                    DeleteAction.NotApplicable
                );

            TransactionResult<TObject> result = new TransactionResult<TObject>()
            {
                Passed = execResults.Passed,
                Failed = execResults.Failed,
                TotalEntities = execResults.TotalEntities
            };
            result.Messages.AddRange(execResults.Messages);
            if (execResults.Failed > 0)
            {
                Type tObject = typeof(TObject);
                foreach (TableEntity entity in execResults.FailedEntities)
                {
                    result.FailedEntities.Add((TObject)AzureTablesSerialiser.Deserialise(entity, tObject));
                }
            }

            return result;
        }

        /// <summary>
        /// Insert a collection of items into an Azure Table
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="objects">Collection of items to insert</param>
        /// <returns>Results of the transaction</returns>
        public async Task<TransactionResult<TObject>> InsertAsync<TObject>(params TObject[] objects)
            => await InsertAsync<TObject>(objects.ToList());

        /// <summary>
        /// Update a single item into an Azure Table
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="obj">Instance of object with the data to update</param>
        /// <param name="mode">Mode of update</param>
        /// <returns>Results of the transaction</returns>
        public async Task<TransactionResult<TObject>> UpdateAsync<TObject>(TObject obj, UpdateModes mode = UpdateModes.Merge)
        {
            if (obj == null)
            {
                return TransactionResult<TObject>.Default;
            }

            ContainerTypeInformation typeInfo = TypeDiscoveryFactory.Resolve<TObject>() ?? throw new TypeLoadException($"Type '{typeof(TObject).Name}' is not appropriately decorated.");
            TableEntity entity = AzureTablesSerialiser.Serialise(obj, false);

            TableTransactionActionType tta = mode switch
            {
                UpdateModes.Merge => TableTransactionActionType.UpdateMerge,
                UpdateModes.Replace => TableTransactionActionType.UpdateReplace,
                UpdateModes.InsertIfMissingOrMerge => TableTransactionActionType.UpsertMerge,
                UpdateModes.InsertIfMissingOrReplace => TableTransactionActionType.UpsertReplace
            };

            TransactionResult<TableEntity> execResults = await ExecuteNonQueryAsyncImpl(
                    typeInfo.ContainerDefinition.Name,
                    new List<TableEntity>(1) { entity },
                    tta,
                    DeleteAction.NotApplicable
                );

            TransactionResult<TObject> result = new TransactionResult<TObject>()
            {
                Passed = execResults.Passed,
                Failed = execResults.Failed,
                TotalEntities = 1
            };
            result.Messages.AddRange(execResults.Messages);
            if (execResults.Failed == 1)
            {
                result.FailedEntities.Add(obj);
            }
            return result;
        }

        /// <summary>
        /// Update a collection of items into an Azure Table
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="objects">Collection of items to update</param>
        /// <param name="mode">Mode of update</param>
        /// <returns>Results of the transaction</returns>
        public async Task<TransactionResult<TObject>> UpdateAsync<TObject>(IEnumerable<TObject> objects, UpdateModes mode = UpdateModes.Merge)
        {
            ContainerTypeInformation typeInfo = TypeDiscoveryFactory.Resolve<TObject>() ?? throw new TypeLoadException($"Type '{typeof(TObject).Name}' is not appropriately decorated.");

            TableTransactionActionType tta = mode switch
            {
                UpdateModes.Merge => TableTransactionActionType.UpdateMerge,
                UpdateModes.Replace => TableTransactionActionType.UpdateReplace,
                UpdateModes.InsertIfMissingOrMerge => TableTransactionActionType.UpsertMerge,
                UpdateModes.InsertIfMissingOrReplace => TableTransactionActionType.UpsertReplace
            };

            TransactionResult<TableEntity> execResults = await ExecuteNonQueryAsyncImpl(
                    typeInfo.ContainerDefinition.Name,
                    AzureTablesSerialiser.Serialise<TObject>(objects, false, false),
                    tta,
                    DeleteAction.NotApplicable
                );

            TransactionResult<TObject> result = new TransactionResult<TObject>()
            {
                Passed = execResults.Passed,
                Failed = execResults.Failed,
                TotalEntities = execResults.TotalEntities
            };
            result.Messages.AddRange(execResults.Messages);
            if (execResults.Failed > 0)
            {
                Type tObject = typeof(TObject);
                foreach (TableEntity entity in execResults.FailedEntities)
                {
                    result.FailedEntities.Add((TObject)AzureTablesSerialiser.Deserialise(entity, tObject));
                }
            }

            return result;
        }

        /// <summary>
        /// Update a collection of items into an Azure Table
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="mode">Mode of update</param>
        /// <param name="objects">Collection of items to update</param>
        /// <returns>Results of the transaction</returns>
        public async Task<TransactionResult<TObject>> UpdateAsync<TObject>(UpdateModes mode = UpdateModes.Merge, params TObject[] objects)
            => await UpdateAsync<TObject>(objects.ToList(), mode);

        /// <summary>
        /// Delete a single item into an Azure Table
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="obj">Instance of object with the data to delete</param>
        /// <param name="permanentDelete">When set, performs a hard delete. When not set and the underlying table is not configured for soft-deletion, a hard delete will be performed nonetheless.</param>
        /// <returns>Results of the transaction</returns>
        public async Task<TransactionResult<TObject>> DeleteAsync<TObject>(TObject obj, bool permanentDelete = false)
        {
            if (obj == null)
            {
                return TransactionResult<TObject>.Default;
            }

            ContainerTypeInformation typeInfo = TypeDiscoveryFactory.Resolve<TObject>() ?? throw new TypeLoadException($"Type '{typeof(TObject).Name}' is not appropriately decorated.");
            TableEntity entity = AzureTablesSerialiser.Serialise(obj, false);

            TransactionResult<TableEntity> execResults = await ExecuteNonQueryAsyncImpl(
                    typeInfo.ContainerDefinition.Name,
                    new List<TableEntity>(1) { entity },
                    TableTransactionActionType.Delete,
                    ((permanentDelete == true) ? DeleteAction.HardDelete : DeleteAction.SoftDelete)
                );

            TransactionResult<TObject> result = new TransactionResult<TObject>()
            {
                Passed = execResults.Passed,
                Failed = execResults.Failed,
                TotalEntities = 1
            };
            result.Messages.AddRange(execResults.Messages);
            if (execResults.Failed == 1)
            {
                result.FailedEntities.Add(obj);
            }
            return result;
        }

        /// <summary>
        /// Delete a collection of items into an Azure Table
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="objects">Collection of items to delete</param>
        /// <param name="permanentDelete">When set, performs a hard delete. When not set and the underlying table is not configured for soft-deletion, a hard delete will be performed nonetheless.</param>
        /// <returns>Results of the transaction</returns>
        public async Task<TransactionResult<TObject>> DeleteAsync<TObject>(IEnumerable<TObject> objects, bool permanentDelete = false)
        {
            ContainerTypeInformation typeInfo = TypeDiscoveryFactory.Resolve<TObject>() ?? throw new TypeLoadException($"Type '{typeof(TObject).Name}' is not appropriately decorated.");

            TransactionResult<TableEntity> execResults = await ExecuteNonQueryAsyncImpl(
                    typeInfo.ContainerDefinition.Name,
                    AzureTablesSerialiser.Serialise<TObject>(objects, false, false),
                    TableTransactionActionType.Delete,
                    ((permanentDelete == true) ? DeleteAction.HardDelete : DeleteAction.SoftDelete)
                );

            TransactionResult<TObject> result = new TransactionResult<TObject>()
            {
                Passed = execResults.Passed,
                Failed = execResults.Failed,
                TotalEntities = execResults.TotalEntities
            };
            result.Messages.AddRange(execResults.Messages);
            if (execResults.Failed > 0)
            {
                Type tObject = typeof(TObject);
                foreach (TableEntity entity in execResults.FailedEntities)
                {
                    result.FailedEntities.Add((TObject)AzureTablesSerialiser.Deserialise(entity, tObject));
                }
            }

            return result;
        }

        /// <summary>
        /// Delete a collection of items into an Azure Table
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="permanentDelete">When set, performs a hard delete. When not set and the underlying table is not configured for soft-deletion, a hard delete will be performed nonetheless.</param>
        /// <param name="objects">Collection of items to delete</param>
        /// <returns>Results of the transaction</returns>
        public async Task<TransactionResult<TObject>> DeleteAsync<TObject>(bool permanentDelete = false, params TObject[] objects)
            => await DeleteAsync<TObject>(objects.ToList(), permanentDelete);

        /// <summary>
        /// Delete rows in an Azure Table matching the provided values or conditions
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <param name="permanentDelete">When set, performs a hard delete. When not set and the underlying table is not configured for soft-deletion, a hard delete will be performed nonetheless.</param>
        /// <returns>Results of the transaction</returns>
        public async Task<TransactionResult<TObject>> DeleteAsync<TObject>(string? partitionKey = null, string? rowKey = null, string? filter = null, bool permanentDelete = false)
        {
            ContainerTypeInformation typeInfo = TypeDiscoveryFactory.Resolve<TObject>() ?? throw new TypeLoadException($"Type '{typeof(TObject).Name}' is not appropriately decorated.");
            List<TableEntity> entities = new List<TableEntity>();
            TransactionResult<TObject> result = new TransactionResult<TObject>();
            Type tObject = typeof(TObject);
            TransactionResult<TableEntity> execResult;

            await foreach (TableEntity entity in ExecuteQueryAsyncImplYielder(typeInfo.ContainerDefinition.Name, PrepareQueryColumnsList(typeInfo), partitionKey, rowKey, filter, false))
            {
                entities.Add(entity);

                if (entities.Count == 100)
                {
                    execResult = await ExecuteNonQueryAsyncImpl(
                            typeInfo.ContainerDefinition.Name,
                            entities,
                            TableTransactionActionType.Delete,
                            ((permanentDelete == true) ? DeleteAction.HardDelete : DeleteAction.SoftDelete)
                        );

                    entities.Clear();

                    result.TotalEntities += execResult.TotalEntities;
                    result.Passed += execResult.Passed;
                    result.Failed += execResult.Failed;
                    result.Messages.AddRange(execResult.Messages);
                    if (execResult.Failed > 0)
                    {
                        foreach(TableEntity failedEntity in execResult.FailedEntities)
                        {
                            result.FailedEntities.Add((TObject)AzureTablesSerialiser.Deserialise(failedEntity, tObject));
                        }
                    }
                }
            }

            if (entities.Count > 0)
            {
                execResult = await ExecuteNonQueryAsyncImpl(
                        typeInfo.ContainerDefinition.Name,
                        entities,
                        TableTransactionActionType.Delete,
                        ((permanentDelete == true) ? DeleteAction.HardDelete : DeleteAction.SoftDelete)
                    );

                entities.Clear();

                result.TotalEntities += execResult.TotalEntities;
                result.Passed += execResult.Passed;
                result.Failed += execResult.Failed;
                result.Messages.AddRange(execResult.Messages);
                if (execResult.Failed > 0)
                {
                    foreach (TableEntity failedEntity in execResult.FailedEntities)
                    {
                        result.FailedEntities.Add((TObject)AzureTablesSerialiser.Deserialise(failedEntity, tObject));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Delete rows in an Azure Table matching the provided values or conditions
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <param name="permanentDelete">When set, performs a hard delete. When not set and the underlying table is not configured for soft-deletion, a hard delete will be performed nonetheless.</param>
        /// <returns>Results of the transaction</returns>
        public async Task<TransactionResult<TObject>> DeleteAsync<TObject>(object? partitionKey = null, object? rowKey = null, string? filter = null, bool permanentDelete = false)
            => await DeleteAsync<TObject>(
                    (string?)ReflectionUtils.ConvertValueIfRequired(partitionKey, typeof(string)),
                    (string?)ReflectionUtils.ConvertValueIfRequired(rowKey, typeof(string)),
                    filter,
                    permanentDelete
                );

        /// <summary>
        /// Delete rows in an Azure Table matching the provided values or conditions
        /// </summary>
        /// <param name="tableName">Name of the table to delete rows from</param>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <param name="permanentDelete">When set, performs a hard delete. When not set and the underlying table is not configured for soft-deletion, a hard delete will be performed nonetheless.</param>
        /// <returns>Results of the transaction</returns>
        public async Task<TransactionResult<TableEntity>> DeleteAsync(string tableName, string? partitionKey = null, string? rowKey = null, string? filter = null, bool permanentDelete = false)
        {
            List<TableEntity> entities = new List<TableEntity>();
            TransactionResult<TableEntity> result = new TransactionResult<TableEntity>();
            TransactionResult<TableEntity> execResult;

            await foreach (TableEntity entity in ExecuteQueryAsyncImplYielder(tableName, ReservedNames.All, partitionKey, rowKey, filter, false))
            {
                entities.Add(entity);

                if (entities.Count == 100)
                {
                    execResult = await ExecuteNonQueryAsyncImpl(
                            tableName,
                            entities,
                            TableTransactionActionType.Delete,
                            ((permanentDelete == true) ? DeleteAction.HardDelete : DeleteAction.SoftDelete)
                        );

                    entities.Clear();

                    result.TotalEntities += execResult.TotalEntities;
                    result.Passed += execResult.Passed;
                    result.Failed += execResult.Failed;
                    result.Messages.AddRange(execResult.Messages);
                    if (execResult.Failed > 0)
                    {
                        foreach (TableEntity failedEntity in execResult.FailedEntities)
                        {
                            result.FailedEntities.Add(failedEntity);
                        }
                    }
                }
            }

            if (entities.Count > 0)
            {
                execResult = await ExecuteNonQueryAsyncImpl(
                        tableName,
                        entities,
                        TableTransactionActionType.Delete,
                        ((permanentDelete == true) ? DeleteAction.HardDelete : DeleteAction.SoftDelete)
                    );

                entities.Clear();

                result.TotalEntities += execResult.TotalEntities;
                result.Passed += execResult.Passed;
                result.Failed += execResult.Failed;
                result.Messages.AddRange(execResult.Messages);
                if (execResult.Failed > 0)
                {
                    foreach (TableEntity failedEntity in execResult.FailedEntities)
                    {
                        result.FailedEntities.Add(failedEntity);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Delete rows in an Azure Table matching the provided values or conditions
        /// </summary>
        /// <param name="tableName">Name of the table to delete rows from</param>
        /// <param name="partitionKey">PartitionKey in the table</param>
        /// <param name="rowKey">RowKey in the table</param>
        /// <param name="filter">Pre-composed filter string (Where clause)</param>
        /// <param name="permanentDelete">When set, performs a hard delete. When not set and the underlying table is not configured for soft-deletion, a hard delete will be performed nonetheless.</param>
        /// <returns>Results of the transaction</returns>
        public async Task<TransactionResult<TableEntity>> DeleteAsync(string tableName, object? partitionKey = null, object? rowKey = null, string? filter = null, bool permanentDelete = false)
            => await DeleteAsync(
                    tableName,
                    (string?)ReflectionUtils.ConvertValueIfRequired(partitionKey, typeof(string)),
                    (string?)ReflectionUtils.ConvertValueIfRequired(rowKey, typeof(string)),
                    filter,
                    permanentDelete
                );

    }

}