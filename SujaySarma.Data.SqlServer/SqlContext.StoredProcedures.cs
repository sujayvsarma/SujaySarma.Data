using System;
using System.Threading.Tasks;
using System.Data;
using System.Collections.Generic;


#if NET7_0_OR_GREATER
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// Provides synchronous and asynchronous methods to execute stored procedures and functions
    /// </summary>
    public partial class SqlContext
    {

        /// <summary>
        /// Execute a stored procedure or SQL function and return the results.
        /// </summary>
        /// <param name="procOrFuncName">Name of the target stored procedure or SQL function.</param>
        /// <param name="inParams">Inward parameters.</param>
        /// <param name="outParams">Outward parameters.</param>
        /// <returns>Results of the execution. Will be a <see cref="ProcedureOrFunctionResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public ExecutionResult Execute(string procOrFuncName, Dictionary<string, object?>? inParams = null, Dictionary<string, object>? outParams = null)
        {
            DataSet ds = new DataSet();
            ProcedureOrFunctionResult result = new ProcedureOrFunctionResult()
            {
                IsError = false,
                Name = procOrFuncName,
                Data = ds
            };

            try
            {
                using (SqlConnection cn = new SqlConnection(_connectionString))
                {
                    cn.Open();

                    using (SqlCommand cmd = cn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = procOrFuncName;

                        DebugWrite(nameof(Execute), procOrFuncName);

                        if ((inParams != null) && (inParams.Count > 0))
                        {
                            foreach (KeyValuePair<string, object?> kvp in inParams)
                            {
                                cmd.Parameters.Add(CreateParameter(kvp.Key, kvp.Value, ParameterDirection.Input));
                            }
                        }

                        if ((outParams != null) && (outParams.Count > 0))
                        {
                            foreach (KeyValuePair<string, object> kvp in outParams)
                            {
                                if (kvp.Value == null)
                                {
                                    throw new ArgumentException($"Value of [out] parameter [{kvp.Key}] cannot be passed as [NULL] since the underlying layer cannot guess the datatype.");
                                }

                                cmd.Parameters.Add(CreateParameter(kvp.Key, kvp.Value, ParameterDirection.Output));
                            }
                        }

                        cmd.Parameters.Add(CreateParameter("@returnValue", null, ParameterDirection.ReturnValue));

                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(ds);
                        }

                        if ((cmd.Parameters["@returnValue"].Value != null) && int.TryParse(cmd.Parameters["@returnValue"].Value.ToString(), out int procReturnValue))
                        {
                            result.ReturnValue = procReturnValue;
                            if (procReturnValue < 0)
                            {
                                result.IsError = true;
                                result.Messages.Add($"Stored procedure returned {procReturnValue} (error condition) instead of throwing an exception.");
                            }
                        }

                        if ((outParams != null) && (outParams.Count > 0))
                        {
                            foreach (string paramName in outParams.Keys)
                            {
                                if (cmd.Parameters[paramName] != null)
                                {
                                    result.ReturnParameters.Add(paramName, cmd.Parameters[paramName].Value);
                                }
                            }
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

            return result;
        }

        /// <summary>
        /// Execute a stored procedure or SQL function and return the results.
        /// </summary>
        /// <param name="procOrFuncName">Name of the target stored procedure or SQL function.</param>
        /// <param name="inParams">Inward parameters.</param>
        /// <param name="outParams">Outward parameters.</param>
        /// <returns>Results of the execution. Will be a <see cref="ProcedureOrFunctionResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
        public async Task<ExecutionResult> ExecuteAsync(string procOrFuncName, Dictionary<string, object?>? inParams = null, Dictionary<string, object>? outParams = null)
        {
            DataSet ds = new DataSet();
            ProcedureOrFunctionResult result = new ProcedureOrFunctionResult()
            {
                IsError = false,
                Name = procOrFuncName,
                Data = ds
            };

            try
            {
                using (SqlConnection cn = new SqlConnection(_connectionString))
                {
                    await cn.OpenAsync();

                    using (SqlCommand cmd = cn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = procOrFuncName;

                        DebugWrite(nameof(Execute), procOrFuncName);

                        if ((inParams != null) && (inParams.Count > 0))
                        {
                            foreach (KeyValuePair<string, object?> kvp in inParams)
                            {
                                cmd.Parameters.Add(CreateParameter(kvp.Key, kvp.Value, ParameterDirection.Input));
                            }
                        }

                        if ((outParams != null) && (outParams.Count > 0))
                        {
                            foreach (KeyValuePair<string, object> kvp in outParams)
                            {
                                if (kvp.Value == null)
                                {
                                    throw new ArgumentException($"Value of [out] parameter [{kvp.Key}] cannot be passed as [NULL] since the underlying layer cannot guess the datatype.");
                                }

                                cmd.Parameters.Add(CreateParameter(kvp.Key, kvp.Value, ParameterDirection.Output));
                            }
                        }

                        cmd.Parameters.Add(CreateParameter("@returnValue", null, ParameterDirection.ReturnValue));

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            do
                            {
                                DataTable table = new DataTable();
                                table.Load(reader);
                                ds.Tables.Add(table);

                            } while (await reader.NextResultAsync());
                        }

                        if ((cmd.Parameters["@returnValue"].Value != null) && int.TryParse(cmd.Parameters["@returnValue"].Value.ToString(), out int procReturnValue))
                        {
                            result.ReturnValue = procReturnValue;
                            if (procReturnValue < 0)
                            {
                                result.IsError = true;
                                result.Messages.Add($"Stored procedure returned {procReturnValue} (error condition) instead of throwing an exception.");
                            }
                        }

                        if ((outParams != null) && (outParams.Count > 0))
                        {
                            foreach (string paramName in outParams.Keys)
                            {
                                if (cmd.Parameters[paramName] != null)
                                {
                                    result.ReturnParameters.Add(paramName, cmd.Parameters[paramName].Value);
                                }
                            }
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

            return result;
        }

        /// <summary>
        /// Create a new SqlParameter.
        /// </summary>
        /// <param name="name">Name of the parameter. If not already prefixed with '@', this function will do so.</param>
        /// <param name="value">Value of the parameter. Set NULL if this is a return/output parameter type.</param>
        /// <param name="direction">Direction (in/out/both) of the parameter.</param>
        /// <returns>The created SqlParameter.</returns>
        private static SqlParameter CreateParameter(string name, object? value, ParameterDirection direction)
        {
            return new SqlParameter(name.StartsWith('@') ? name : "@" + name, value)
            {
                Direction = direction
            };
        }

    }
}
