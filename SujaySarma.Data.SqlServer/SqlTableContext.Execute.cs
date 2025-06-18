using SujaySarma.Data.SqlServer.Serialisation;

using System;
using System.Collections.Generic;
using System.Data;

#if NET6_0
using System.Data.SqlClient;
#elif NET8_0_OR_GREATER
using Microsoft.Data.SqlClient;
#endif

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SujaySarma.Data.SqlServer
{
    // Execute functions and overloads
    public partial class SqlTableContext
    {
        /// <summary>
        /// Execute a non-query SQL statement
        /// </summary>
        /// <param name="sql">SQL statement to execute</param>
        /// <returns>Number of rows affected</returns>
        public async Task<int> ExecuteNonQueryAsync(string sql)
        {
            int affectedRows = 0;
            using (SqlConnection cn = new SqlConnection(this._connectionString))
            {
                cn.Open();

                using (SqlCommand cmd = cn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sql;

                    DebugWriteSql("ExecuteNonQueryAsync()", sql);

                    affectedRows = await cmd.ExecuteNonQueryAsync();
                }
            }
            return affectedRows;
        }

        /// <summary>
        /// Execute a non-query operation
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="operationType">Type of operation</param>
        /// <param name="data">Instances of objects to insert/update/delete</param>
        /// <param name="AdditionalData">Additional data to be updated (for INSERT and UPDATE only!)</param>
        /// <param name="AdditionalConditions">Additional conditions to check (for UPDATE and DELETE only!) -- will be merged with 'AND'</param>
        /// <returns>Total number of rows affected in the backing data store</returns>
        /// <remarks>
        ///     This method is **PRIVATE** because there is no advantage to calling this over
        ///     the wrapped 'InsertAsync[T]()', 'UpdateAsync[T]()', 'DeleteAsync[T]()' functions
        /// </remarks>
        private async Task<int> ExecuteNonQueryAsync<TObject>(SqlStatementType operationType, IEnumerable<TObject> data, Dictionary<string, object?>? AdditionalData = null, List<string>? AdditionalConditions = null)
        {
            int totalAffectedRows = 0;

            if (data.Any<TObject>())
            {
                using (SqlConnection cn = new SqlConnection(this._connectionString))
                {
                    cn.Open();
                    using (SqlCommand cmd = cn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        foreach (TObject instance in data)
                        {
                            SqlCommand sqlCommand = cmd;
                            string str;

                            switch (operationType)
                            {
                                case SqlStatementType.Insert:
                                    str = SQLScriptGenerator.GetInsertStatement<TObject>(instance, AdditionalData);
                                    break;

                                case SqlStatementType.Update:
                                    str = SQLScriptGenerator.GetUpdateStatement<TObject>(instance, AdditionalData, AdditionalConditions);
                                    break;

                                case SqlStatementType.Upsert:
                                    str = SQLScriptGenerator.GetMergeStatement<TObject>(instance);
                                    break;

                                case SqlStatementType.Delete:
                                    str = SQLScriptGenerator.GetDeleteStatement<TObject>(instance, AdditionalConditions);
                                    break;

                                default:
                                    throw new NotSupportedException("'operationType' must be INSERT, UPDATE or DELETE.");
                            }

                            sqlCommand.CommandText = str;
                            DebugWriteSql("ExecuteNonQueryAsync()", cmd.CommandText);

                            int thisCallAffectedRows = totalAffectedRows;
                            totalAffectedRows = thisCallAffectedRows + (await cmd.ExecuteNonQueryAsync());
                        }
                    }
                }
            }
            return totalAffectedRows;
        }

        /// <summary>
        /// Execute a query yielding an enumeration of object instances
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="query">Query to run on the SQL Server</param>
        /// <returns>Enumeration of object instances</returns>
        public IEnumerable<TObject> ExecuteQuery<TObject>(SqlCommand query)
        {
            foreach (DataRow executeQueryRow in ExecuteQueryRows(query))
            {
                yield return SqlDataSerialiser.Transform<TObject>(executeQueryRow);
            }
        }

        /// <summary>
        /// Execute a query yielding an enumeration of object instances
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="query">Query to run on the SQL Server</param>
        /// <returns>Enumeration of object instances</returns>
        public IEnumerable<TObject> ExecuteQuery<TObject>(string query)
            => ExecuteQuery<TObject>(new SqlCommand(query));

        /// <summary>
        /// Execute a query yielding an enumeration of data rows
        /// </summary>
        /// <param name="query">Query to run on the SQL Server</param>
        /// <returns>Enumeration of DataRows</returns>
        public IEnumerable<DataRow> ExecuteQueryRows(SqlCommand query)
        {
            DataTable dataTable = ExecuteQueryTable(query);
            if ((dataTable.Columns.Count > 0) && (dataTable.Rows.Count > 0))
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    yield return row;
                }
            }
        }

        /// <summary>
        /// Execute a query yielding an enumeration of data rows
        /// </summary>
        /// <param name="query">Query to run on the SQL Server</param>
        /// <returns>Enumeration of DataRows</returns>
        public IEnumerable<DataRow> ExecuteQueryRows(string query)
            => ExecuteQueryRows(new SqlCommand(query));

        /// <summary>
        /// Execute a query yielding a complete DataTable
        /// </summary>
        /// <param name="query">Query to run on the SQL Server</param>
        /// <returns>DataTable</returns>
        public DataTable ExecuteQueryTable(SqlCommand query)
        {
            DataTable dataTable = new DataTable();
            using (SqlConnection sqlConnection = new SqlConnection(this._connectionString))
            {
                sqlConnection.Open();
                query.Connection = sqlConnection;

                DebugWriteSql("ExecuteQueryTable()", query.CommandText);
                using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(query))
                {
                    sqlDataAdapter.Fill(dataTable);
                }
            }

            return dataTable;
        }

        /// <summary>
        /// Execute a query yielding a complete DataTable
        /// </summary>
        /// <param name="query">Query to run on the SQL Server</param>
        /// <returns>DataTable</returns>
        public DataTable ExecuteQueryTable(string query)
            => ExecuteQueryTable(new SqlCommand(query));

        /// <summary>
        /// Execute a query yielding a DataSet
        /// </summary>
        /// <param name="query">Query to run on the SQL Server</param>
        /// <returns>DataSet</returns>
        public DataSet ExecuteQueryTables(string query)
        {
            DataSet dataSet = new DataSet();
            using (SqlConnection sqlConnection = new SqlConnection(this._connectionString))
            {
                sqlConnection.Open();
                using (SqlCommand command = sqlConnection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = query;

                    DebugWriteSql("ExecuteQueryTables()", command.CommandText);
                    using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(command))
                    {
                        sqlDataAdapter.Fill(dataSet);
                    }
                }
            }

            return dataSet;
        }

        /// <summary>
        /// Get a single value output (instead of a statement). This is similar to the ExecuteScalar statement.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET property, field or variable the scalar data will be used to populate</typeparam>
        /// <param name="query">SELECT query to run</param>
        /// <param name="commandTimeout">[OPTIONAL] Timeout of command. Default of 30 seconds.</param>
        /// <returns>The single scalar value -- will return 'default(T)' and not NULL!</returns>
        public async Task<TObject?> ExecuteScalarAsync<TObject>(string query, int commandTimeout = 30)
        {
            try
            {
                using (SqlConnection cn = new SqlConnection(this._connectionString))
                {
                    cn.Open();
                    using (SqlCommand cmd = cn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = query;
                        cmd.CommandTimeout = commandTimeout;

                        DebugWriteSql("ExecuteScalarAsync()", cmd.CommandText);
                        object? obj = await cmd.ExecuteScalarAsync();
                        return (((obj == null) || (obj is DBNull) || (obj == DBNull.Value))
                            ? default
                            : (TObject?)Core.ReflectionUtils.ConvertValueIfRequired(obj, typeof(TObject)));
                    }
                }
            }
            catch
            {
            }

            return default;
        }



        /// <summary>
        /// Get binary data out of a SQL table
        /// </summary>
        /// <param name="query">SELECT query to run</param>
        /// <param name="expectedLength">Expected length of data</param>
        /// <param name="commandTimeout">[OPTIONAL] Timeout of command. Default of 30 seconds</param>
        /// <returns>Byte array containing the binary content requested or empty byte array. If there was a problem will return an empty array instead of throwing an exception.</returns>
        public byte[] ExecuteSelectBinaryContent(string query, int expectedLength, int commandTimeout = 30)
        {
            byte[] buffer = new byte[expectedLength];
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(this._connectionString))
                {
                    sqlConnection.Open();
                    using (SqlCommand command = sqlConnection.CreateCommand())
                    {
                        command.CommandType = CommandType.Text;
                        command.CommandText = query;
                        command.CommandTimeout = commandTimeout;

                        DebugWriteSql("ExecuteSelectBinaryContent()", command.CommandText);
                        using (SqlDataReader sqlDataReader = command.ExecuteReader(CommandBehavior.SequentialAccess))
                        {
                            if (sqlDataReader.Read())
                            {
                                sqlDataReader.GetBytes(0, 0L, buffer, 0, expectedLength);
                            }
                        }
                    }
                }
            }
            catch
            {
                buffer = Array.Empty<byte>();
            }

            return buffer;
        }


        /// <summary>
        /// Execute a stored procedure and fetch the results
        /// </summary>
        /// <param name="procedureName">Stored procedure to run</param>
        /// <param name="inParameters">[OPTIONAL] Dictionary of IN parameters (keys are parameter names, values are param values)</param>
        /// <param name="outParameters">[OPTIONAL] Dictionary of OUT parameters. If this dictionary is NULL, then no out parameters are retrieved</param>
        /// <param name="commandTimeout">[OPTIONAL] Timeout of command. Default of 30 seconds.</param>
        public StoredProcedureExecutionResult ExecuteStoredProcedure(string procedureName, Dictionary<string, object?>? inParameters = null, Dictionary<string, object?>? outParameters = null, int commandTimeout = 30)
        {
            StoredProcedureExecutionResult procedureExecutionResult = new StoredProcedureExecutionResult()
            {
                IsError = false,
                Messages = null,
                Exception = null,
                Results = null,
                ProcedureName = procedureName,
                ReturnParameters = new Dictionary<string, object?>(),
                ReturnValue = 0
            };

            try
            {
                DataSet ds = new DataSet();
                using (SqlConnection sqlConnection = new SqlConnection(this._connectionString))
                {
                    sqlConnection.Open();

                    using (SqlCommand sqlCommand = new SqlCommand(procedureName, sqlConnection))
                    {
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.CommandTimeout = commandTimeout;

                        if ((inParameters != null) && (inParameters.Count > 0))
                        {
                            foreach (string key in inParameters.Keys)
                            {
                                sqlCommand.Parameters.Add(CreateParameter(key, inParameters[key], ParameterDirection.Input));
                            }
                        }

                        if ((outParameters != null) && (outParameters.Count > 0))
                        {
                            foreach (string key in outParameters.Keys)
                            {
                                if (outParameters[key] == null)
                                {
                                    throw new ArgumentException($"Value of [out] parameter [{key}] cannot be passed as [NULL] since the underlying layer cannot guess the datatype.");
                                }

                                sqlCommand.Parameters.Add(CreateParameter(key, outParameters[key], ParameterDirection.Output));
                            }
                        }

                        // add always!
                        sqlCommand.Parameters.Add(CreateParameter("@returnValue", null, ParameterDirection.ReturnValue));

                        using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand))
                        {
                            sqlDataAdapter.Fill(ds);

                            if ((sqlCommand.Parameters["@returnvalue"].Value != null) && int.TryParse(sqlCommand.Parameters["@returnvalue"].Value.ToString(), out int result))
                            {
                                procedureExecutionResult.ReturnValue = result;
                                if (result < 0)
                                {
                                    procedureExecutionResult.IsError = true;
                                    procedureExecutionResult.Messages = $"Stored procedure returned {result} (error condition) instead of throwing an exception.";
                                }
                            }

                            if (outParameters != null)
                            {
                                procedureExecutionResult.ReturnParameters = new Dictionary<string, object?>();
                                foreach (string key in outParameters.Keys)
                                {
                                    if (sqlCommand.Parameters[key] != null)
                                    {
                                        procedureExecutionResult.ReturnParameters.Add(key, sqlCommand.Parameters[key].Value);
                                    }
                                }
                            }

                            if (!procedureExecutionResult.IsError)
                            {
                                NameTables(ds);
                                procedureExecutionResult.Results = ds;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                procedureExecutionResult.IsError = true;
                procedureExecutionResult.Messages = ex.Message;
                procedureExecutionResult.Exception = ex;
            }

            // RETURN!
            return procedureExecutionResult;


            // helper function: create parameter
            static SqlParameter CreateParameter(string name, object? value, ParameterDirection direction)
            {
                SqlParameter parameter = new SqlParameter(name.StartsWith('@') ? name : "@" + name, value)
                {
                    Direction = direction
                };
                return parameter;
            }

            // helper function: name tables in the dataset
            static DataSet NameTables(DataSet? ds)
            {
                ds ??= new DataSet();
                if (ds.Tables.Count > 1)
                {
                    for (int index = 0; index < ds.Tables.Count; ++index)
                    {
                        if (string.IsNullOrEmpty(ds.Tables[index].TableName))
                        {
                            ds.Tables[index].TableName = $"Table{index + 1}";
                        }
                    }
                }

                return ds;
            }
        }
    }
}
