using System;
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
    /// Provides synchronous and asynchronous ExecuteNonQuery methods
    /// </summary>
    public partial class SqlContext
    {

        /// <summary>
        /// Execute a non-query SQL script.
        /// </summary>
        /// <param name="script">The SQL script to execute.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public ExecutionResult ExecuteNonQuery(StringBuilder script)
        {
            int count = -1;
            string command = script.ToString();

            try
            {
                using (SqlConnection cn = new SqlConnection(_connectionString))
                {
                    cn.Open();

                    using (SqlCommand cmd = cn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = command;

                        DebugWrite(nameof(ExecuteNonQuery), command);

                        count = cmd.ExecuteNonQuery();
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

            return new NonQueryResult()
            {
                IsError = false,
                RowsAffected = count,
                Text = command
            };
        }

        /// <summary>
        /// Executes a non-query SQL script built through a SQL Statement Builder instance.
        /// </summary>
        /// <param name="script">
        ///     Instance of a SQL statement builder. Allowed types: <see cref="SqlInsertBuilder"/>, <see cref="SqlUpdateBuilder"/>, <see cref="SqlDeleteBuilder"/>, 
        ///     <see cref="SqlMergeBuilder"/>.
        /// </param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public ExecutionResult ExecuteNonQuery(SqlStatementBuilder script)
        {
            if ((script is SqlQueryBuilder) || (script.GetType() == typeof(SqlStatementBuilder)))
            {
                throw new InvalidOperationException($"Cannot use ExecuteNonQuery on scripts of type '{nameof(SqlQueryBuilder)}' or the abstract class '{nameof(SqlStatementBuilder)}'.");
            }

            return ExecuteNonQuery(script.Build());
        }

        /// <summary>
        /// Execute a non-query SQL script.
        /// </summary>
        /// <param name="script">The SQL script to execute.</param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public async Task<ExecutionResult> ExecuteNonQueryAsync(StringBuilder script)
        {
            int count = -1;
            string command = script.ToString();

            try
            {
                using (SqlConnection cn = new SqlConnection(_connectionString))
                {
                    await cn.OpenAsync();

                    using (SqlCommand cmd = cn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = command;

                        DebugWrite(nameof(ExecuteNonQueryAsync), command);

                        count = await cmd.ExecuteNonQueryAsync();
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

            return new NonQueryResult()
            {
                IsError = false,
                RowsAffected = count,
                Text = command
            };
        }

        /// <summary>
        /// Executes a non-query SQL script built through a SQL Statement Builder instance.
        /// </summary>
        /// <param name="script">
        ///     Instance of a SQL statement builder. Allowed types: <see cref="SqlInsertBuilder"/>, <see cref="SqlUpdateBuilder"/>, <see cref="SqlDeleteBuilder"/>, 
        ///     <see cref="SqlMergeBuilder"/>.
        /// </param>
        /// <returns>Results of the execution. Will be a <see cref="NonQueryResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public async Task<ExecutionResult> ExecuteNonQueryAsync(SqlStatementBuilder script)
        {
            if ((script is SqlQueryBuilder) || (script.GetType() == typeof(SqlStatementBuilder)))
            {
                throw new InvalidOperationException($"Cannot use ExecuteNonQuery on scripts of type '{nameof(SqlQueryBuilder)}' or the abstract class '{nameof(SqlStatementBuilder)}'.");
            }

            return await ExecuteNonQueryAsync(script.Build());
        }
    }
}
