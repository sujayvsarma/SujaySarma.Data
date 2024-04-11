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
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">Collection of object instances to insert</param>
        /// <returns>Total number of rows inserted</returns>
        public async Task<int> InsertAsync<T>(params T[] data) where T : class
            => await ExecuteNonQueryAsync<T>(SqlStatementType.Insert, data.AsEnumerable());

        /// <summary>
        /// Execute an INSERT
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">Collection of object instances to insert</param>
        /// <param name="additionalData">Additional data to be updated</param>
        /// <returns>Total number of rows inserted</returns>
        public async Task<int> InsertAsync<T>(IEnumerable<T> data, Dictionary<string, object?>? additionalData = null) where T : class
            => await ExecuteNonQueryAsync<T>(SqlStatementType.Insert, data, additionalData);

        /// <summary>
        /// Execute an INSERT
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="builder">An instance of <see cref="SqlInsertBuilder{TTable}"/></param>
        /// <returns>Total number of rows inserted</returns>
        public async Task<int> InsertAsync<T>(SqlInsertBuilder<T> builder) where T : class
            => await ExecuteNonQueryAsync(builder.Build());


        /// <summary>
        /// Execute an UPDATE
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">Collection of object instances to update</param>
        /// <returns>Total number of rows updated</returns>
        public async Task<int> UpdateAsync<T>(params T[] data) where T : class
            => await ExecuteNonQueryAsync<T>(SqlStatementType.Update, data.AsEnumerable());

        /// <summary>
        /// Execute an UPDATE
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">Collection of object instances to update</param>
        /// <param name="additionalData">Additional data to be updated</param>
        /// <param name="additionalConditions">Additional conditions to check -- will be merged with 'AND'</param>
        /// <returns>Total number of rows updated</returns>
        public async Task<int> UpdateAsync<T>(IEnumerable<T> data, Dictionary<string, object?>? additionalData = null, List<string>? additionalConditions = null) where T : class
            => await ExecuteNonQueryAsync<T>(SqlStatementType.Update, data, additionalData, additionalConditions);

        /// <summary>
        /// Execute a MERGE (Upsert)
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">Collection of object instances to insert or update</param>
        /// <returns>Total number of rows inserted/updated</returns>
        public async Task<int> UpsertAsync<T>(params T[] data) where T : class
            => await ExecuteNonQueryAsync<T>(SqlStatementType.Upsert, data.AsEnumerable());

        /// <summary>
        /// Execute a MERGE (Upsert)
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">Collection of object instances to insert or update</param>
        /// <returns>Total number of rows inserted/updated</returns>
        public async Task<int> UpsertAsync<T>(IEnumerable<T> data) where T : class
            => await ExecuteNonQueryAsync<T>(SqlStatementType.Upsert, data);


        /// <summary>
        /// Execute a DELETE
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">Collection of object instances to delete</param>
        /// <returns>Total number of rows deleted</returns>
        public async Task<int> DeleteAsync<T>(params T[] data) where T : class
            => await ExecuteNonQueryAsync<T>(SqlStatementType.Delete, data.AsEnumerable());

        /// <summary>
        /// Execute a DELETE
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">Collection of object instances to delete</param>
        /// <param name="additionalConditions">Additional conditions to check -- will be merged with 'AND'</param>
        /// <returns>Total number of rows deleted</returns>
        public async Task<int> DeleteAsync<T>(IEnumerable<T> data, List<string>? additionalConditions = null) where T : class
            => await ExecuteNonQueryAsync<T>(SqlStatementType.Delete, data, AdditionalConditions: additionalConditions);
    }
}