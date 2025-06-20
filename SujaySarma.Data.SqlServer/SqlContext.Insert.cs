using SujaySarma.Data.SqlServer.Builders;

using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// Provides synchronous and asynchronous shortcut methods to INSERT data
    /// </summary>
    public partial class SqlContext
    {

        /// <summary>
        /// Insert a single element of data.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to insert.</typeparam>
        /// <param name="data">Instance of object with data to insert.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public ExecutionResult Insert<TObject>(TObject data)
        {
            StringBuilder script = SqlInsertBuilder.Into<TObject>().Values(data).Build();
            return ExecuteNonQuery(script);
        }

        /// <summary>
        /// Insert a collection of data.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to insert.</typeparam>
        /// <param name="data">Instances of objects with data to insert.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public ExecutionResult Insert<TObject>(IEnumerable<TObject> data)
        {
            StringBuilder script = SqlInsertBuilder.Into<TObject>().Values(data).Build();
            return ExecuteNonQuery(script);
        }

        /// <summary>
        /// Insert a collection of data.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to insert.</typeparam>
        /// <param name="data">Instances of objects with data to insert.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public ExecutionResult Insert<TObject>(params TObject[] data)
        {
            StringBuilder script = SqlInsertBuilder.Into<TObject>().Values(data).Build();
            return ExecuteNonQuery(script);
        }

        /// <summary>
        /// Execute a pre-built INSERT script.
        /// </summary>
        /// <param name="script">Script to INSERT data.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public ExecutionResult Insert(SqlInsertBuilder script)
        {
            return ExecuteNonQuery(script);
        }

        /// <summary>
        /// Insert a single element of data.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to insert.</typeparam>
        /// <param name="data">Instance of object with data to insert.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public async Task<ExecutionResult> InsertAsync<TObject>(TObject data)
        {
            StringBuilder script = SqlInsertBuilder.Into<TObject>().Values(data).Build();
            return await ExecuteNonQueryAsync(script);
        }

        /// <summary>
        /// Insert a collection of data.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to insert.</typeparam>
        /// <param name="data">Instances of objects with data to insert.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public async Task<ExecutionResult> InsertAsync<TObject>(IEnumerable<TObject> data)
        {
            StringBuilder script = SqlInsertBuilder.Into<TObject>().Values(data).Build();
            return await ExecuteNonQueryAsync(script);
        }

        /// <summary>
        /// Insert a collection of data.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to insert.</typeparam>
        /// <param name="data">Instances of objects with data to insert.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public async Task<ExecutionResult> InsertAsync<TObject>(params TObject[] data)
        {
            StringBuilder script = SqlInsertBuilder.Into<TObject>().Values(data).Build();
            return await ExecuteNonQueryAsync(script);
        }

        /// <summary>
        /// Execute a pre-built INSERT script.
        /// </summary>
        /// <param name="script">Script to INSERT data.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public async Task<ExecutionResult> InsertAsync(SqlInsertBuilder script)
        {
            return await ExecuteNonQueryAsync(script);
        }


    }
}
