using SujaySarma.Data.Core.Reflection;

using System.Threading.Tasks;

namespace SujaySarma.Data.Azure.Tables
{
    // Async Table-level operations using Generics
    public partial class AzureTablesContext
    {
        /// <summary>
        /// Drop table attached to the <typeparamref name="TObject" />.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        public async Task DropTableAsync<TObject>() 
            => await DropTableAsync(GetTableName<TObject>());

        /// <summary>
        /// Create table attached to the <typeparamref name="TObject" />.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        public async Task CreateTableAsync<TObject>() 
            => await CreateTableAsync(GetTableName<TObject>());

        /// <summary>
        /// Check if table attached to the <typeparamref name="TObject" /> exists.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <returns>True if table exists</returns>
        public async Task<bool> TableExistsAsync<TObject>()
            => await TableExistsAsync(GetTableName<TObject>());


        /// <summary>
        /// Clear all rows from a table attached to the <typeparamref name="TObject" />.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        public async Task ClearTableAsync<TObject>() 
            => await ClearTableAsync(GetTableName<TObject>());

        /// <summary>
        /// Clear data for the specified <paramref name="partitionKey" /> from the table attached to the <typeparamref name="TObject" />.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">Partition key to clear data for</param>
        public async Task ClearPartitionAsync<TObject>(string partitionKey)
        {
            ContainerTypeInfo ContainerTypeInfo = TypeDiscoveryFactory.Resolve<TObject>();
            await ClearPartitionAsync(ContainerTypeInfo.Name, partitionKey, ContainerTypeInfo.Container.UseSoftDelete);
        }
    }
}
