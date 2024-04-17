using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SujaySarma.Data.SqlServer.Fluid;

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// SqlTableContext. Insert|Update|Delete|Upsert
    /// </summary>
    public partial class SqlTableContext
    {
        /// <summary>
        /// Execute an INSERT
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="data">Collection of object instances to insert</param>
        /// <returns>Total number of rows inserted</returns>
        public async Task<int> InsertAsync<TObject>(params TObject[] data)
            => await ExecuteNonQueryAsync<TObject>(SqlStatementType.Insert, data.AsEnumerable());

        /// <summary>
        /// Execute an INSERT
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="data">Collection of object instances to insert</param>
        /// <param name="additionalData">Additional data to be updated</param>
        /// <returns>Total number of rows inserted</returns>
        public async Task<int> InsertAsync<TObject>(IEnumerable<TObject> data, Dictionary<string, object?>? additionalData = null)
            => await ExecuteNonQueryAsync<TObject>(SqlStatementType.Insert, data, additionalData);

        /// <summary>
        /// Execute an INSERT
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="builder">An instance of <see cref="SqlInsertBuilder{TTable}"/></param>
        /// <returns>Total number of rows inserted</returns>
        public async Task<int> InsertAsync<TObject>(SqlInsertBuilder<TObject> builder)
            => await ExecuteNonQueryAsync(builder.Build());


        /// <summary>
        /// Execute an UPDATE
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="data">Collection of object instances to update</param>
        /// <returns>Total number of rows updated</returns>
        public async Task<int> UpdateAsync<TObject>(params TObject[] data)
            => await ExecuteNonQueryAsync<TObject>(SqlStatementType.Update, data.AsEnumerable());

        /// <summary>
        /// Execute an UPDATE
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="data">Collection of object instances to update</param>
        /// <param name="additionalData">Additional data to be updated</param>
        /// <param name="additionalConditions">Additional conditions to check -- will be merged with 'AND'</param>
        /// <returns>Total number of rows updated</returns>
        public async Task<int> UpdateAsync<TObject>(IEnumerable<TObject> data, Dictionary<string, object?>? additionalData = null, List<string>? additionalConditions = null)
            => await ExecuteNonQueryAsync<TObject>(SqlStatementType.Update, data, additionalData, additionalConditions);

        /// <summary>
        /// Execute a MERGE (Upsert)
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="data">Collection of object instances to insert or update</param>
        /// <returns>Total number of rows inserted/updated</returns>
        public async Task<int> UpsertAsync<TObject>(params TObject[] data)
            => await ExecuteNonQueryAsync<TObject>(SqlStatementType.Upsert, data.AsEnumerable());

        /// <summary>
        /// Execute a MERGE (Upsert)
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="data">Collection of object instances to insert or update</param>
        /// <returns>Total number of rows inserted/updated</returns>
        public async Task<int> UpsertAsync<TObject>(IEnumerable<TObject> data)
            => await ExecuteNonQueryAsync<TObject>(SqlStatementType.Upsert, data);


        /// <summary>
        /// Execute a DELETE
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="data">Collection of object instances to delete</param>
        /// <returns>Total number of rows deleted</returns>
        public async Task<int> DeleteAsync<TObject>(params TObject[] data)
            => await ExecuteNonQueryAsync<TObject>(SqlStatementType.Delete, data.AsEnumerable());

        /// <summary>
        /// Execute a DELETE
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="data">Collection of object instances to delete</param>
        /// <param name="additionalConditions">Additional conditions to check -- will be merged with 'AND'</param>
        /// <returns>Total number of rows deleted</returns>
        public async Task<int> DeleteAsync<TObject>(IEnumerable<TObject> data, List<string>? additionalConditions = null)
            => await ExecuteNonQueryAsync<TObject>(SqlStatementType.Delete, data, AdditionalConditions: additionalConditions);
    }
}