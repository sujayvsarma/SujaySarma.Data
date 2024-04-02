using System.Threading.Tasks;

using SujaySarma.Data.Core.Reflection;

namespace SujaySarma.Data.Azure.Tables
{

    /*
        Table-level operations using Generics (Synchronous)
    */

    public partial class AzureTablesContext
    {

        /// <summary>
        /// Drop table attached to the <typeparamref name="TObject"/>.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        public async Task DropTableAsync<TObject>() where TObject : class
            => await DropTableAsync(GetTableName<TObject>());

        /// <summary>
        /// Create table attached to the <typeparamref name="TObject"/>.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        public async Task CreateTableAsync<TObject>() where TObject : class
            => await DropTableAsync(GetTableName<TObject>());

        /// <summary>
        /// Check if table attached to the <typeparamref name="TObject"/> exists.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <returns>True if table exists</returns>
        public async Task<bool> TableExistsAsync<TObject>() where TObject : class
            => await TableExistsAsync(GetTableName<TObject>());


        /// <summary>
        /// Clear all rows from a table attached to the <typeparamref name="TObject"/>.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        public async Task ClearTableAsync<TObject>() where TObject : class
            => await ClearTableAsync(GetTableName<TObject>());

        /// <summary>
        /// Clear data for the specified <paramref name="partitionKey"/> from the table attached to the <typeparamref name="TObject"/>.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">Partition key to clear data for</param>
        public async Task ClearPartitionAsync<TObject>(string partitionKey) where TObject : class
        {
            ContainerTypeInformation info = TypeDiscoveryFactory.Resolve<TObject>();
            await ClearPartitionAsync(info.Name, partitionKey, info.ContainerDefinition.UseSoftDelete);
        }

    }
}