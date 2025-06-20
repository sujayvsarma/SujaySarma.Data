using SujaySarma.Data.SqlServer.Builders;

using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// Provides synchronous and asynchronous shortcut methods to Delete data
    /// </summary>
    public partial class SqlContext
    {

        /// <summary>
        /// Delete a single element of data.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to Delete.</typeparam>
        /// <param name="data">Instance of object with data to Delete.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public ExecutionResult Delete<TObject>(TObject data)
        {
            StringBuilder script = SqlDeleteBuilder.From<TObject>().Where(data).Build();
            return ExecuteNonQuery(script);
        }

        /// <summary>
        /// Delete a collection of data.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to Delete.</typeparam>
        /// <param name="data">Instances of objects with data to Delete.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public ExecutionResult Delete<TObject>(IEnumerable<TObject> data)
        {
            StringBuilder script = new StringBuilder();
            foreach(TObject instance in data)
            {
                script.AppendLine(
                        SqlDeleteBuilder.From<TObject>().Where(instance).Build().ToString()
                    );
            }

            return ExecuteNonQuery(script);
        }

        /// <summary>
        /// Delete a collection of data.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to Delete.</typeparam>
        /// <param name="data">Instances of objects with data to Delete.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public ExecutionResult Delete<TObject>(params TObject[] data)
        {
            return Delete<TObject>((IEnumerable<TObject>)data);
        }

        /// <summary>
        /// Execute a pre-built Delete script.
        /// </summary>
        /// <param name="script">Script to Delete data.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public ExecutionResult Delete(SqlDeleteBuilder script)
        {
            return ExecuteNonQuery(script);
        }

        /// <summary>
        /// Delete a single element of data.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to Delete.</typeparam>
        /// <param name="data">Instance of object with data to Delete.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public async Task<ExecutionResult> DeleteAsync<TObject>(TObject data)
        {
            StringBuilder script = SqlDeleteBuilder.From<TObject>().Where(data).Build();
            return await ExecuteNonQueryAsync(script);
        }

        /// <summary>
        /// Delete a collection of data.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to Delete.</typeparam>
        /// <param name="data">Instances of objects with data to Delete.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public async Task<ExecutionResult> DeleteAsync<TObject>(IEnumerable<TObject> data)
        {
            StringBuilder script = new StringBuilder();
            foreach (TObject instance in data)
            {
                script.AppendLine(
                        SqlDeleteBuilder.From<TObject>().Where(instance).Build().ToString()
                    );
            }

            return await ExecuteNonQueryAsync(script);
        }

        /// <summary>
        /// Delete a collection of data.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to Delete.</typeparam>
        /// <param name="data">Instances of objects with data to Delete.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public async Task<ExecutionResult> DeleteAsync<TObject>(params TObject[] data)
        {
            return await DeleteAsync<TObject>((IEnumerable<TObject>)data);
        }

        /// <summary>
        /// Execute a pre-built Delete script.
        /// </summary>
        /// <param name="script">Script to Delete data.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public async Task<ExecutionResult> DeleteAsync(SqlDeleteBuilder script)
        {
            return await ExecuteNonQueryAsync(script);
        }


    }
}
