using Azure.Data.Tables;

using SujaySarma.Data.Azure.Tables.Serialisation;
using SujaySarma.Data.Core;
using SujaySarma.Data.Core.Reflection;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SujaySarma.Data.Azure.Tables
{
    /// <summary>
    /// Handle UPDATE operations - both sync and async, generic and regular
    /// </summary>
    public partial class AzureTablesContext
    {

        #region Sync

        /// <summary>Update a single item into an Azure Table</summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="obj">Instance of object with the data to update</param>
        /// <param name="mode">Mode of update</param>
        /// <returns>Results of the transaction</returns>
        public TransactionResult<TObject> Update<TObject>(TObject obj, UpdateModes mode = UpdateModes.Merge)
        {
            if (obj == null)
            {
                return TransactionResult<TObject>.Default;
            }

            ContainerTypeInfo ContainerTypeInfo = TypeDiscoveryFactory.Resolve<TObject>();
            TableEntity tableEntity = AzureTablesSerialiser.Serialise(obj) 
                ?? throw new ArgumentNullException(nameof(obj), $"Failed to serialise object of type '{typeof(TObject).FullName}' into a TableEntity. Please ensure that the type is serialisable.");

            TransactionResult<TableEntity> result = ExecuteNonQueryImpl(ContainerTypeInfo.Container.Name, new List<TableEntity>(1) { tableEntity }, ConvertModeToTransactionType(mode));
            return ConvertTransactionResultToCallerFormats<TObject>(result);
        }

        /// <summary>Update a collection of items into an Azure Table</summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="objects">Collection of items to update</param>
        /// <param name="mode">Mode of update</param>
        /// <returns>Results of the transaction</returns>
        public TransactionResult<TObject> Update<TObject>(IEnumerable<TObject> objects, UpdateModes mode = UpdateModes.Merge)
        {
            ContainerTypeInfo ContainerTypeInfo = TypeDiscoveryFactory.Resolve<TObject>();

            TransactionResult<TableEntity> result = ExecuteNonQueryImpl(ContainerTypeInfo.Container.Name, AzureTablesSerialiser.Serialise(objects), ConvertModeToTransactionType(mode));
            return ConvertTransactionResultToCallerFormats<TObject>(result);
        }

        /// <summary>Update a collection of items into an Azure Table</summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="mode">Mode of update</param>
        /// <param name="objects">Collection of items to update</param>
        /// <returns>Results of the transaction</returns>
        public TransactionResult<TObject> Update<TObject>(UpdateModes mode = UpdateModes.Merge, params TObject[] objects)
            // IEnumerable cast is required as otherwise we would end up calling ourselves adnauseum!
            => Update<TObject>((IEnumerable<TObject>)objects, mode);

        #endregion

        #region Async

        /// <summary>Update a single item into an Azure Table</summary>
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

            ContainerTypeInfo ContainerTypeInfo = TypeDiscoveryFactory.Resolve<TObject>();
            TableEntity tableEntity = AzureTablesSerialiser.Serialise(obj) 
                                        ?? throw new ArgumentNullException(nameof(obj), $"Failed to serialise object of type '{typeof(TObject).FullName}' into a TableEntity. Please ensure that the type is serialisable.");

            TransactionResult<TableEntity> result = await ExecuteNonQueryAsyncImpl(
                                                        ContainerTypeInfo.Container.Name, 
                                                            new List<TableEntity>() { tableEntity }, 
                                                                ConvertModeToTransactionType(mode)
                                                        );
            
            return ConvertTransactionResultToCallerFormats<TObject>(result);
        }

        /// <summary>Update a collection of items into an Azure Table</summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="objects">Collection of items to update</param>
        /// <param name="mode">Mode of update</param>
        /// <returns>Results of the transaction</returns>
        public async Task<TransactionResult<TObject>> UpdateAsync<TObject>(IEnumerable<TObject> objects, UpdateModes mode = UpdateModes.Merge)
        {
            ContainerTypeInfo ContainerTypeInfo = TypeDiscoveryFactory.Resolve<TObject>();

            TransactionResult<TableEntity> result = await ExecuteNonQueryAsyncImpl(
                                                        ContainerTypeInfo.Container.Name, 
                                                            AzureTablesSerialiser.Serialise(objects), 
                                                                ConvertModeToTransactionType(mode)
                                                    );

            return ConvertTransactionResultToCallerFormats<TObject>(result);
        }

        /// <summary>Update a collection of items into an Azure Table</summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="mode">Mode of update</param>
        /// <param name="objects">Collection of items to update</param>
        /// <returns>Results of the transaction</returns>
        public async Task<TransactionResult<TObject>> UpdateAsync<TObject>(UpdateModes mode = UpdateModes.Merge, params TObject[] objects)
            // IEnumerable cast is required as otherwise we would end up calling ourselves adnauseum!
            => await UpdateAsync<TObject>((IEnumerable<TObject>)objects, mode);

        #endregion

        /// <summary>
        /// Converts an UpdateModes value to a TableTransactionActionType value.
        /// </summary>
        /// <param name="mode">UpdateModes value passed in by consumer caller function.</param>
        /// <returns>A TableTransactionActionType value to be passed to Azure Storage API.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the mode value is something other than an Update mode.</exception>
        private static TableTransactionActionType ConvertModeToTransactionType(UpdateModes mode)
            => mode switch
            {
                UpdateModes.Merge => TableTransactionActionType.UpdateMerge,
                UpdateModes.Replace => TableTransactionActionType.UpdateReplace,
                UpdateModes.InsertIfMissingOrMerge => TableTransactionActionType.UpsertMerge,
                UpdateModes.InsertIfMissingOrReplace => TableTransactionActionType.UpsertReplace,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), $"Unsupported value for UPDATE operation: '{Enum.GetName<UpdateModes>(mode)}'.")
            };

    }
}
