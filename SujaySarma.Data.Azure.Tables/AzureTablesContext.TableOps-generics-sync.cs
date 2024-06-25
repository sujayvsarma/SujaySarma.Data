using System;

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
        public void DropTable<TObject>()
            => DropTable(GetTableName<TObject>());

        /// <summary>
        /// Create table attached to the <typeparamref name="TObject"/>.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        public void CreateTable<TObject>()
            => CreateTable(GetTableName<TObject>());

        /// <summary>
        /// Check if table attached to the <typeparamref name="TObject"/> exists.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <returns>True if table exists</returns>
        public bool TableExists<TObject>()
            => TableExists(GetTableName<TObject>());


        /// <summary>
        /// Clear all rows from a table attached to the <typeparamref name="TObject"/>.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        public void ClearTable<TObject>()
            => ClearTable(GetTableName<TObject>());

        /// <summary>
        /// Clear data for the specified <paramref name="partitionKey"/> from the table attached to the <typeparamref name="TObject"/>.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="partitionKey">Partition key to clear data for</param>
        public void ClearPartition<TObject>(string partitionKey)
        {
            ContainerTypeInformation info = TypeDiscoveryFactory.Resolve<TObject>();
            ClearPartition(info.Name, partitionKey, info.ContainerDefinition.UseSoftDelete);
        }

    }
}