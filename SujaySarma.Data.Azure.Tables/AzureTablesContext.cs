using System;

using Azure.Data.Tables;

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

        /// <summary>
        /// Gets the name of the table from the <see cref="Attributes.TableAttribute"/> decorating the 
        /// object defined by <typeparamref name="TObject"/>.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <returns>Name of the table as specified by the "TableAttribute.TableName" property</returns>
        private static string GetTableName<TObject>()
            => TypeDiscoveryFactory.Resolve<TObject>().Name;


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
            _serviceClient = new TableServiceClient(connectionString);
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