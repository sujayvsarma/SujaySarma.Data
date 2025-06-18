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
    /// Handle INSERT operations - both sync and async, generic and regular
    /// </summary>
    public partial class AzureTablesContext
    {

        #region Sync        

        /// <summary>Insert a single item into an Azure Table</summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="obj">Instance of object with the data to insert</param>
        /// <returns>Results of the transaction</returns>
        public TransactionResult<TObject> Insert<TObject>(TObject obj)
        {
            if (obj == null)
            {
                return TransactionResult<TObject>.Default;
            }

            TableEntity entity = AzureTablesSerialiser.Serialise(obj) 
                                    ?? throw new ArgumentException("Object cannot be serialised to a TableEntity", nameof(obj));

            TransactionResult<TableEntity> result = ExecuteNonQueryImpl(TypeDiscoveryFactory.Resolve<TObject>().Container.Name, 
                new List<TableEntity>(1) { entity }, 
                    TableTransactionActionType.Add);

            return ConvertTransactionResultToCallerFormats<TObject>(result);
        }

        /// <summary>Insert a collection of items into an Azure Table</summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="objects">Collection of items to insert</param>
        /// <returns>Results of the transaction</returns>
        public TransactionResult<TObject> Insert<TObject>(IEnumerable<TObject> objects)
        {
            if (objects == null)
            {
                return TransactionResult<TObject>.Default;
            }

            List<TableEntity> tableEntities = AzureTablesSerialiser.Serialise(objects);
            if (tableEntities.Count == 0)
            {
                return TransactionResult<TObject>.Default;
            }

            TransactionResult<TableEntity> result = ExecuteNonQueryImpl(
                TypeDiscoveryFactory.Resolve<TObject>().Container.Name, 
                    tableEntities, 
                        TableTransactionActionType.Add);

            return ConvertTransactionResultToCallerFormats<TObject>(result);
        }

        /// <summary>Insert a collection of items into an Azure Table</summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="objects">Collection of items to insert</param>
        /// <returns>Results of the transaction</returns>
        public TransactionResult<TObject> Insert<TObject>(params TObject[] objects)
            // without the cast, we will end up calling ourselves recursively!
            => Insert<TObject>((IEnumerable<TObject>)objects);

        #endregion

        #region Async

        /// <summary>Insert a single item into an Azure Table</summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="obj">Instance of object with the data to insert</param>
        /// <returns>Results of the transaction</returns>
        public async Task<TransactionResult<TObject>> InsertAsync<TObject>(TObject obj)
        {
            if (obj == null)
            {
                return TransactionResult<TObject>.Default;
            }

            ContainerTypeInfo typeInfo = TypeDiscoveryFactory.Resolve<TObject>();
            TableEntity tableEntity = AzureTablesSerialiser.Serialise(obj) 
                                        ?? throw new ArgumentException("Object cannot be serialised to a TableEntity", nameof(obj));

            TransactionResult<TableEntity> result = await ExecuteNonQueryAsyncImpl(
                typeInfo.Container.Name,
                    new List<TableEntity>(1) { tableEntity }, 
                        TableTransactionActionType.Add);

            return ConvertTransactionResultToCallerFormats<TObject>(result);
        }

        /// <summary>Insert a collection of items into an Azure Table</summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="objects">Collection of items to insert</param>
        /// <returns>Results of the transaction</returns>
        public async Task<TransactionResult<TObject>> InsertAsync<TObject>(IEnumerable<TObject> objects)
        {
            List<TableEntity> entities = AzureTablesSerialiser.Serialise(objects);
            if (entities.Count == 0)
            {
                return TransactionResult<TObject>.Default;
            }

            TransactionResult<TableEntity> result = await ExecuteNonQueryAsyncImpl(
                TypeDiscoveryFactory.Resolve<TObject>().Container.Name, 
                    entities, 
                        TableTransactionActionType.Add);

            return ConvertTransactionResultToCallerFormats<TObject>(result);
        }

        /// <summary>Insert a collection of items into an Azure Table</summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="objects">Collection of items to insert</param>
        /// <returns>Results of the transaction</returns>
        public async Task<TransactionResult<TObject>> InsertAsync<TObject>(params TObject[] objects)
            // without the cast, we will end up calling ourselves recursively!
            => await InsertAsync<TObject>((IEnumerable<TObject>)objects);

        #endregion

    }
}
