using System.Text;
using System.Threading.Tasks;
using System.Data;

using SujaySarma.Data.SqlServer.Builders;


#if NET7_0_OR_GREATER
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// Provides synchronous and asynchronous Query methods that return a single column/value (not entire row[s]!)
    /// </summary>
    public partial class SqlContext
    {

        /// <summary>
        /// Execute a query that results in a single column/field of data returned.
        /// </summary>
        /// <typeparam name="TType">Type of .NET value that would be returned.</typeparam>
        /// <param name="query">Query to execute.</param>
        /// <returns>A value of type <typeparamref name="TType"/> or NULL.</returns>
        public TType? QueryScalar<TType>(StringBuilder query)
        {
            string command = query.ToString();
            object? obj;

            using (SqlConnection cn = new SqlConnection(_connectionString))
            {
                cn.Open();

                using (SqlCommand cmd = cn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = command;

                    DebugWrite(nameof(QueryScalar), command);

                    obj = cmd.ExecuteScalar();
                }
            }

            return (TType?)Core.ReflectionUtils.ConvertValueIfRequired(obj, typeof(TType));
        }

        /// <summary>
        /// Execute a query that results in a single column/field of data returned.
        /// </summary>
        /// <typeparam name="TType">Type of .NET value that would be returned.</typeparam>
        /// <param name="query">Query to execute.</param>
        /// <returns>A value of type <typeparamref name="TType"/> or NULL.</returns>
        public TType? QueryScalar<TType>(SqlQueryBuilder query)
        {
            return QueryScalar<TType>(query.Build());
        }

        /// <summary>
        /// Execute a query that results in a single column/field of data returned.
        /// </summary>
        /// <typeparam name="TType">Type of .NET value that would be returned.</typeparam>
        /// <param name="query">Query to execute.</param>
        /// <returns>A value of type <typeparamref name="TType"/> or NULL.</returns>
        public async Task<TType?> QueryScalarAsync<TType>(StringBuilder query)
        {
            string command = query.ToString();
            object? obj;

            using (SqlConnection cn = new SqlConnection(_connectionString))
            {
                await cn.OpenAsync();

                using (SqlCommand cmd = cn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = command;

                    DebugWrite(nameof(QueryScalar), command);

                    obj = await cmd.ExecuteScalarAsync();
                }
            }

            return (TType?)Core.ReflectionUtils.ConvertValueIfRequired(obj, typeof(TType));
        }

        /// <summary>
        /// Execute a query that results in a single column/field of data returned.
        /// </summary>
        /// <typeparam name="TType">Type of .NET value that would be returned.</typeparam>
        /// <param name="query">Query to execute.</param>
        /// <returns>A value of type <typeparamref name="TType"/> or NULL.</returns>
        public async Task<TType?> QueryScalarAsync<TType>(SqlQueryBuilder query)
        {
            return await QueryScalarAsync<TType>(query.Build());
        }
    }
}
