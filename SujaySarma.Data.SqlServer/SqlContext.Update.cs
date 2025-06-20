using SujaySarma.Data.SqlServer.Builders;

using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// Provides synchronous and asynchronous shortcut methods to Update data
    /// </summary>
    public partial class SqlContext
    {

        /// <summary>
        /// Update a single element of data.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to Update.</typeparam>
        /// <param name="data">Instance of object with data to Update.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public ExecutionResult Update<TObject>(TObject data)
        {
            StringBuilder script = SqlUpdateBuilder.For<TObject>().Values(data).Build();
            return ExecuteNonQuery(script);
        }

        /// <summary>
        /// Update a collection of data.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to Update.</typeparam>
        /// <param name="data">Instances of objects with data to Update.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public ExecutionResult Update<TObject>(IEnumerable<TObject> data)
        {
            StringBuilder script = SqlUpdateBuilder.For<TObject>().Values(data).Build();
            return ExecuteNonQuery(script);
        }

        /// <summary>
        /// Update a collection of data.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to Update.</typeparam>
        /// <param name="data">Instances of objects with data to Update.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public ExecutionResult Update<TObject>(params TObject[] data)
        {
            return Update<TObject>((IEnumerable<TObject>)data);
        }

        /// <summary>
        /// Execute a pre-built Update script.
        /// </summary>
        /// <param name="script">Script to Update data.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public ExecutionResult Update(SqlUpdateBuilder script)
        {
            return ExecuteNonQuery(script);
        }

        /// <summary>
        /// Update a single element of data.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to Update.</typeparam>
        /// <param name="data">Instance of object with data to Update.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public async Task<ExecutionResult> UpdateAsync<TObject>(TObject data)
        {
            StringBuilder script = SqlUpdateBuilder.For<TObject>().Values(data).Build();
            return await ExecuteNonQueryAsync(script);
        }

        /// <summary>
        /// Update a collection of data.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to Update.</typeparam>
        /// <param name="data">Instances of objects with data to Update.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public async Task<ExecutionResult> UpdateAsync<TObject>(IEnumerable<TObject> data)
        {
            StringBuilder script = SqlUpdateBuilder.For<TObject>().Values(data).Build();
            return await ExecuteNonQueryAsync(script);
        }

        /// <summary>
        /// Update a collection of data.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to Update.</typeparam>
        /// <param name="data">Instances of objects with data to Update.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public async Task<ExecutionResult> UpdateAsync<TObject>(params TObject[] data)
        {
            return await UpdateAsync<TObject>((IEnumerable<TObject>)data);
        }

        /// <summary>
        /// Execute a pre-built Update script.
        /// </summary>
        /// <param name="script">Script to Update data.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public async Task<ExecutionResult> UpdateAsync(SqlUpdateBuilder script)
        {
            return await ExecuteNonQueryAsync(script);
        }


    }
}
