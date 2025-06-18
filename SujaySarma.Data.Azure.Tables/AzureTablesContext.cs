using Azure.Data.Tables;

namespace SujaySarma.Data.Azure.Tables
{
    /// <summary>
    /// A completely connection-less approach to interacting with Azure Tables. Supports both
    /// Azure Storage Tables and Azure Cosmos DB with Tables API.
    /// </summary>
    public partial class AzureTablesContext
    {
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
            => new AzureTablesContext();

        /// <summary>
        /// Get a context based on the provided connection string
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <returns>AzureTablesContext</returns>
        public static AzureTablesContext WithConnectionString(string connectionString)
            => new AzureTablesContext(connectionString);

        private readonly string _connectionString;
        private readonly TableServiceClient _serviceClient;

        /// <summary>
        /// Well-known connection string SHORTCUT to use the Azure Storage Emulator (Azurite).
        /// </summary>
        private const string UseDevelopmentStorage = "UseDevelopmentStorage=true";

        /// <summary>
        /// Well-known connection string with well-known username and password for Azure Storage (Development Storage -- Azurite / Emulator).
        /// </summary>
        private const string DevelopmentStorageConnectionString = "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";

        /// <summary>
        /// Size of a transactional batch. This is hard-coded at Azure Tables end.
        /// </summary>
        private static readonly int TRANSACTION_BATCH_SIZE = 100;


        /// <summary>
        /// Type of delete to perform
        /// </summary>
        private enum DeleteAction
        {
            /// <summary>
            /// Not a DELETE operation
            /// </summary>
            NotApplicable,

            /// <summary>
            /// SOFT-delete (sets the IsDeleted column)
            /// </summary>
            SoftDelete,

            /// <summary>
            /// HARD-delete
            /// </summary>
            HardDelete
        }
    }

}
