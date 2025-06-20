using System;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Collections.Generic;

using SujaySarma.Data.SqlServer.Builders;
using SujaySarma.Data.SqlServer.Serialisation;

#if NET7_0_OR_GREATER
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// Provides synchronous and asynchronous Query methods
    /// </summary>
    public partial class SqlContext
    {
        /// <summary>
        /// Execute a query
        /// </summary>
        /// <param name="query">The query to execute</param>
        /// <returns>Results of the execution. Will be a <see cref="QueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public ExecutionResult Query(StringBuilder query)
        {
            string command = query.ToString();
            DataSet data = new DataSet();

            try
            {
                using (SqlConnection cn = new SqlConnection(_connectionString))
                {
                    cn.Open();

                    using (SqlCommand cmd = cn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = command;

                        DebugWrite(nameof(Query), command);

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(data);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorResult error = new ErrorResult()
                {
                    IsError = true,
                    Exception = ex
                };
                error.Messages.Add(ex.Message);
                return error;
            }

            return new QueryResult()
            {
                IsError = false,
                Query = command,
                Data = data
            };
        }

        /// <summary>
        /// Execute a query
        /// </summary>
        /// <param name="query">The query to execute</param>
        /// <returns>Results of the execution. Will be a <see cref="QueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public ExecutionResult Query(SqlQueryBuilder query)
        {
            return Query(query.Build());
        }

        /// <summary>
        /// Execute a query and return the results as an IEnumerable collection of .NET objects.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to return.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <returns>IEnumerable collection of objects.</returns>
        /// <exception cref="Exception">Thrown if the query returned an error condition. InnerException will contain the downstream exception thrown.</exception>
        public IEnumerable<TObject> QueryEnumerable<TObject>(StringBuilder query)
        {
            ExecutionResult result = Query(query);
            if (result.IsError)
            {
                ErrorResult error = (ErrorResult)result;
                throw new Exception(error.Messages[0], error.Exception);
            }

            QueryResult dataResult = (QueryResult)result;
            foreach(DataRow row in dataResult.Data.Tables[0].Rows)
            {
                yield return SqlDataSerialiser.Transform<TObject>(row);
            }
        }

        /// <summary>
        /// Execute a query and return the results as an IEnumerable collection of .NET objects.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to return.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <returns>IEnumerable collection of objects.</returns>
        /// <exception cref="Exception">Thrown if the query returned an error condition. InnerException will contain the downstream exception thrown.</exception>
        public IEnumerable<TObject> QueryEnumerable<TObject>(SqlQueryBuilder query)
        {
            return QueryEnumerable<TObject>(query.Build());
        }

        /// <summary>
        /// Execute a query and return the results as a List of .NET objects.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to return.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <returns>List of objects.</returns>
        /// <exception cref="Exception">Thrown if the query returned an error condition. InnerException will contain the downstream exception thrown.</exception>
        public List<TObject> QueryList<TObject>(StringBuilder query)
        {
            List<TObject> list = new List<TObject>();
            foreach(TObject obj in QueryEnumerable<TObject>(query))
            {
                list.Add(obj);
            }

            return list;
        }

        /// <summary>
        /// Execute a query and return the results as a List of .NET objects.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to return.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <returns>List of objects.</returns>
        /// <exception cref="Exception">Thrown if the query returned an error condition. InnerException will contain the downstream exception thrown.</exception>
        public List<TObject> QueryList<TObject>(SqlQueryBuilder query)
        {
            return QueryList<TObject>(query);
        }

        /// <summary>
        /// Execute a query and returns the only/first row as a <typeparamref name="TObject"/> instance. If no data was returned, returns a NULL.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to return.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <returns>Single instance of type <typeparamref name="TObject"/> or NULL.</returns>
        /// <exception cref="Exception">Thrown if the query returned an error condition. InnerException will contain the downstream exception thrown.</exception>
        public TObject? Query<TObject>(StringBuilder query)
        {
            ExecutionResult result = Query(query);
            if (result.IsError)
            {
                ErrorResult error = (ErrorResult)result;
                throw new Exception(error.Messages[0], error.Exception);
            }

            QueryResult dataResult = (QueryResult)result;
            if ((dataResult.Data.Tables.Count >= 1) && (dataResult.Data.Tables[0].Rows.Count >= 1))
            {
                return SqlDataSerialiser.Transform<TObject>(dataResult.Data.Tables[0].Rows[0]);
            }

            return default(TObject);
        }

        /// <summary>
        /// Execute a query and returns the only/first row as a <typeparamref name="TObject"/> instance. If no data was returned, returns a NULL.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to return.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <returns>Single instance of type <typeparamref name="TObject"/> or NULL.</returns>
        /// <exception cref="Exception">Thrown if the query returned an error condition. InnerException will contain the downstream exception thrown.</exception>
        public TObject? Query<TObject>(SqlQueryBuilder query)
        {
            return Query<TObject>(query.Build());
        }

        /// <summary>
        /// Execute a query
        /// </summary>
        /// <param name="query">The query to execute</param>
        /// <returns>Results of the execution. Will be a <see cref="QueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public async Task<ExecutionResult> QueryAsync(StringBuilder query)
        {
            string command = query.ToString();
            DataSet data = new DataSet();

            try
            {
                using (SqlConnection cn = new SqlConnection(_connectionString))
                {
                    await cn.OpenAsync();

                    using (SqlCommand cmd = cn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = command;

                        DebugWrite(nameof(QueryAsync), command);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            do
                            {
                                DataTable table = new DataTable();
                                table.Load(reader);
                                data.Tables.Add(table);

                            } while (await reader.NextResultAsync());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorResult error = new ErrorResult()
                {
                    IsError = true,
                    Exception = ex
                };
                error.Messages.Add(ex.Message);
                return error;
            }

            return new QueryResult()
            {
                IsError = false,
                Query = command,
                Data = data
            };
        }

        /// <summary>
        /// Execute a query
        /// </summary>
        /// <param name="query">The query to execute</param>
        /// <returns>Results of the execution. Will be a <see cref="QueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public async Task<ExecutionResult> QueryAsync(SqlQueryBuilder query)
        {
            return await QueryAsync(query.Build());
        }

        /// <summary>
        /// Execute a query and return the results as an IAsyncEnumerable collection of .NET objects.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to return.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <returns>IAsyncEnumerable collection of objects.</returns>
        /// <exception cref="Exception">Thrown if the query returned an error condition. InnerException will contain the downstream exception thrown.</exception>
        public async IAsyncEnumerable<TObject> QueryEnumerableAsync<TObject>(StringBuilder query)
        {
            ExecutionResult result = await QueryAsync(query);
            if (result.IsError)
            {
                ErrorResult error = (ErrorResult)result;
                throw new Exception(error.Messages[0], error.Exception);
            }

            QueryResult dataResult = (QueryResult)result;
            foreach (DataRow row in dataResult.Data.Tables[0].Rows)
            {
                yield return SqlDataSerialiser.Transform<TObject>(row);
            }
        }

        /// <summary>
        /// Execute a query and return the results as an IAsyncEnumerable collection of .NET objects.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to return.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <returns>IAsyncEnumerable collection of objects.</returns>
        /// <exception cref="Exception">Thrown if the query returned an error condition. InnerException will contain the downstream exception thrown.</exception>
        public async IAsyncEnumerable<TObject> QueryEnumerableAsync<TObject>(SqlQueryBuilder query)
        {
            await foreach (var item in QueryEnumerableAsync<TObject>(query.Build()))
            {
                yield return item;
            }
        }

        /// <summary>
        /// Execute a query and return the results as a List of .NET objects.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to return.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <returns>List of objects.</returns>
        /// <exception cref="Exception">Thrown if the query returned an error condition. InnerException will contain the downstream exception thrown.</exception>
        public async Task<List<TObject>> QueryListAsync<TObject>(StringBuilder query)
        {
            List<TObject> list = new List<TObject>();
            await foreach (TObject obj in QueryEnumerableAsync<TObject>(query))
            {
                list.Add(obj);
            }

            return list;
        }

        /// <summary>
        /// Execute a query and return the results as a List of .NET objects.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to return.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <returns>List of objects.</returns>
        /// <exception cref="Exception">Thrown if the query returned an error condition. InnerException will contain the downstream exception thrown.</exception>
        public async Task<List<TObject>> QueryListAsync<TObject>(SqlQueryBuilder query)
        {
            return await QueryListAsync<TObject>(query.Build());
        }

        /// <summary>
        /// Execute a query and returns the only/first row as a <typeparamref name="TObject"/> instance. If no data was returned, returns a NULL.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to return.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <returns>Single instance of type <typeparamref name="TObject"/> or NULL.</returns>
        /// <exception cref="Exception">Thrown if the query returned an error condition. InnerException will contain the downstream exception thrown.</exception>
        public async Task<TObject?> QueryAsync<TObject>(StringBuilder query)
        {
            ExecutionResult result = await QueryAsync(query);
            if (result.IsError)
            {
                ErrorResult error = (ErrorResult)result;
                throw new Exception(error.Messages[0], error.Exception);
            }

            QueryResult dataResult = (QueryResult)result;
            if ((dataResult.Data.Tables.Count >= 1) && (dataResult.Data.Tables[0].Rows.Count >= 1))
            {
                return SqlDataSerialiser.Transform<TObject>(dataResult.Data.Tables[0].Rows[0]);
            }

            return default(TObject);
        }

        /// <summary>
        /// Execute a query and returns the only/first row as a <typeparamref name="TObject"/> instance. If no data was returned, returns a NULL.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object to return.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <returns>Single instance of type <typeparamref name="TObject"/> or NULL.</returns>
        /// <exception cref="Exception">Thrown if the query returned an error condition. InnerException will contain the downstream exception thrown.</exception>
        public async Task<TObject?> QueryAsync<TObject>(SqlQueryBuilder query)
        {
            return await QueryAsync<TObject>(query.Build());
        }
    }
}
