using System;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

using SujaySarma.Data.SqlServer.Builders;

#if NET7_0_OR_GREATER
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif

namespace SujaySarma.Data.SqlServer;

/// <summary>
/// Exposes methods that execute various types of SQL queries and statements asynchronously.
/// </summary>
public static class SqlExecuteAsync
{
    /// <summary>
    /// Execute a query and return the results as a <see cref="DataSet"/> of tables.
    /// </summary>
    /// <param name="connectionString">The connection string to connect to the target SQL Server.</param>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">The asynchronous operation cancellation token.</param>
    /// <returns>Results of the query execution as a <see cref="QueryResult"/> if successful. Otherwise returns a <see cref="ErrorResult"/>.</returns>
    public static async Task<Result> QueryAsync(string connectionString, StringBuilder query, CancellationToken cancellationToken)
    {
        string statement = query.ToString();
        if (string.IsNullOrWhiteSpace(statement))
        {
            return new ErrorResult(statement, new ArgumentException("Query cannot be null or empty."));
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            DataSet ds = new DataSet();
            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                await cn.OpenAsync(cancellationToken);
                using (SqlCommand cmd = cn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = statement;

                    Logger.DebugLog(nameof(QueryAsync), statement);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        int index = 0;
                        do
                        {
                            DataTable table = new DataTable($"Table{++index}");

                            // Load schema
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                table.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
                            }

                            // Read rows asynchronously
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                DataRow row = table.NewRow();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[i] = reader.GetValue(i);
                                }
                                table.Rows.Add(row);

                                cancellationToken.ThrowIfCancellationRequested();
                            }

                            ds.Tables.Add(table);

                        } while (await reader.NextResultAsync(cancellationToken));
                    }
                }
            }
            return new QueryResult(statement, ds);
        }
        catch (OperationCanceledException)
        {
            Logger.DebugLog(nameof(QueryAsync), "Operation cancelled by user.");
            return new CancelledResult(statement);
        }
        catch (Exception error)
        {
            Logger.DebugLog(nameof(QueryAsync), error.Message, error.ToString());
            return new ErrorResult(statement, error);
        }
    }

    /// <summary>
    /// Execute a query and return the results as a <see cref="DataSet"/> of tables.
    /// </summary>
    /// <param name="connectionString">The connection string to connect to the target SQL Server.</param>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">The asynchronous operation cancellation token.</param>
    /// <returns>Results of the query execution as a <see cref="QueryResult"/> if successful. Otherwise returns a <see cref="ErrorResult"/>.</returns>
    public static async Task<Result> QueryAsync(string connectionString, SqlQueryBuilder query, CancellationToken cancellationToken)
    {
        return await QueryAsync(connectionString, query.Build(), cancellationToken);
    }

    /// <summary>
    /// Execute a query and return binary (as a byte array) data.
    /// </summary>
    /// <param name="connectionString">The connection string to connect to the target SQL Server.</param>
    /// <param name="query">The query to execute.</param>
    /// <param name="length">The expected length of the binary data.</param>
    /// <param name="index">Index of the binary data into the resultant data stream.</param>
    /// <param name="offset">Offset of the binary data in the data stream.</param>
    /// <param name="cancellationToken">The asynchronous operation cancellation token.</param>
    /// <returns>Results of the execution: <see cref="QueryBinaryResult" /> if successful, otherwise <see cref="ErrorResult" />.</returns>
    public static async Task<Result> QueryBinaryAsync(string connectionString, StringBuilder query, int length, CancellationToken cancellationToken, int index = 0, long offset = 0L)
    {
        string statement = query.ToString();
        if (string.IsNullOrWhiteSpace(statement))
        {
            return new ErrorResult(statement, new ArgumentException("Query cannot be null or empty."));
        }

        if (length <= 0)
        {
            return new ErrorResult(statement, new ArgumentException("Expected length of binary data must be greater than zero."));
        }

        if (index < 0)
        {
            return new ErrorResult(statement, new ArgumentException("Index position of binary data in stream must be zero or greater."));
        }

        if (offset < 0)
        {
            return new ErrorResult(statement, new ArgumentException("Offset position of binary data in stream must be zero or greater."));
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            byte[] buffer = new byte[length];
            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                await cn.OpenAsync(cancellationToken);
                using (SqlCommand cmd = cn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = statement;

                    Logger.DebugLog(nameof(QueryBinaryAsync), statement);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken))
                    {
                        if (await reader.ReadAsync(cancellationToken))
                        {
                            // Read in chunks asynchronously using GetStream (available in .NET Standard 2.1+)
                            using (var stream = reader.GetStream(index))
                            {
                                if (offset > 0)
                                {
                                    stream.Seek(offset, System.IO.SeekOrigin.Begin);
                                }

                                int totalRead = 0;
                                while (totalRead < length)
                                {
                                    int bytesRead = await stream.ReadAsync(buffer.AsMemory(totalRead, length - totalRead), cancellationToken);
                                    if (bytesRead == 0)
                                    {
                                        break; // End of stream
                                    }
                                    totalRead += bytesRead;
                                }
                            }
                        }
                    }
                }
            }

            return new QueryBinaryResult(statement, buffer);
        }
        catch (OperationCanceledException)
        {
            Logger.DebugLog(nameof(QueryBinaryAsync), "Operation cancelled by user.");
            return new CancelledResult(statement);
        }
        catch (Exception error)
        {
            Logger.DebugLog(nameof(QueryBinaryAsync), error.Message, error.ToString());
            return new ErrorResult(statement, error);
        }
    }

    /// <summary>
    /// Execute a query and return binary (as a byte array) data.
    /// </summary>
    /// <param name="connectionString">The connection string to connect to the target SQL Server.</param>
    /// <param name="query">The query to execute.</param>
    /// <param name="length">The expected length of the binary data.</param>
    /// <param name="index">Index of the binary data into the resultant data stream.</param>
    /// <param name="offset">Offset of the binary data in the data stream.</param>
    /// <param name="cancellationToken">The asynchronous operation cancellation token.</param>
    /// <returns>Results of the execution: <see cref="QueryBinaryResult" /> if successful, otherwise <see cref="ErrorResult" />.</returns>
    public static async Task<Result> QueryBinaryAsync(string connectionString, SqlQueryBuilder query, int length, CancellationToken cancellationToken, int index = 0, long offset = 0L)
    {
        return await QueryBinaryAsync(connectionString, query.Build(), length, cancellationToken, index, offset);
    }

    /// <summary>
    /// Execute a query and returns the first column of the first row of data, ignoring everything else.
    /// </summary>
    /// <param name="connectionString">The connection string to connect to the target SQL Server.</param>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">The asynchronous operation cancellation token.</param>
    /// <returns>Results of the query execution as a <see cref="QueryResult"/> if successful. Otherwise returns a <see cref="ErrorResult"/>.</returns>
    public static async Task<Result> QueryScalarAsync(string connectionString, StringBuilder query, CancellationToken cancellationToken)
    {
        string statement = query.ToString();
        if (string.IsNullOrWhiteSpace(statement))
        {
            return new ErrorResult(statement, new ArgumentException("Query cannot be null or empty."));
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            object? result = null;
            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                await cn.OpenAsync(cancellationToken);
                using (SqlCommand cmd = cn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = statement;

                    Logger.DebugLog(nameof(QueryScalarAsync), statement);
                    result = await cmd.ExecuteScalarAsync(cancellationToken);
                }
            }

            return new QueryScalarResult(statement, result);
        }
        catch (OperationCanceledException)
        {
            Logger.DebugLog(nameof(QueryScalarAsync), "Operation cancelled by user.");
            return new CancelledResult(statement);
        }
        catch (Exception error)
        {
            Logger.DebugLog(nameof(QueryScalarAsync), error.Message, error.ToString());
            return new ErrorResult(statement, error);
        }
    }

    /// <summary>
    /// Execute a query and returns the first column of the first row of data, ignoring everything else.
    /// </summary>
    /// <param name="connectionString">The connection string to connect to the target SQL Server.</param>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">The asynchronous operation cancellation token.</param>
    /// <returns>Results of the query execution as a <see cref="QueryResult"/> if successful. Otherwise returns a <see cref="ErrorResult"/>.</returns>
    public static async Task<Result> QueryScalarAsync(string connectionString, SqlQueryBuilder query, CancellationToken cancellationToken)
    {
        return await QueryScalarAsync(connectionString, query.Build(), cancellationToken);
    }

    /// <summary>
    /// Execute a stored procedure or SQL function and return the results.
    /// </summary>
    /// <param name="connectionString">The connection string to connect to the target SQL Server.</param>
    /// <param name="procedureName">Name of the target stored procedure or SQL function.</param>
    /// <param name="inputParameters">Input parameters for the procedure or function.</param>
    /// <param name="outputParameters">Output parameters for the procedure or function.</param>
    /// <param name="cancellationToken">The asynchronous operation cancellation token.</param>
    /// <returns>Results of the execution. Will be a <see cref="ProcedureOrFunctionResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
    public static async Task<Result> ProcedureOrFunctionAsync(string connectionString, string procedureName, Dictionary<string, object?> inputParameters, Dictionary<string, object>? outputParameters, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(procedureName))
        {
            return new ErrorResult(procedureName, new ArgumentException("Name of the procedure cannot be null or empty."));
        }

        try
        {
            DataSet ds = new DataSet();
            int returnValue = 0;
            cancellationToken.ThrowIfCancellationRequested();

            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                await cn.OpenAsync(cancellationToken);
                using (SqlCommand cmd = cn.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = procedureName;

                    Logger.DebugLog(nameof(ProcedureOrFunctionAsync), procedureName,
                        Logger.SerialiseDictionary<string, object?>(inputParameters).ToString(),
                            ((outputParameters is null) ? string.Empty : Logger.SerialiseDictionary<string, object?>(outputParameters!).ToString()));

                    if ((inputParameters is not null) && (inputParameters.Count > 0))
                    {
                        foreach (KeyValuePair<string, object?> parameter in inputParameters)
                        {
                            if (string.IsNullOrWhiteSpace(parameter.Key))
                            {
                                return new ErrorResult(parameter.Key, new ArgumentException("Name of parameter cannot be null or empty."));
                            }

                            cmd.Parameters.Add(new SqlParameter((parameter.Key.StartsWith('@') ? parameter.Key : $"@{parameter.Key}"), parameter.Value)
                            {
                                Direction = ParameterDirection.Input
                            });
                        }
                    }

                    if ((outputParameters is not null) && (outputParameters.Count > 0))
                    {
                        foreach (KeyValuePair<string, object> parameter in outputParameters)
                        {
                            if (string.IsNullOrWhiteSpace(parameter.Key))
                            {
                                return new ErrorResult(parameter.Key, new ArgumentException("Name of parameter cannot be null or empty."));
                            }

                            if (parameter.Value is null)
                            {
                                throw new ArgumentException($"Value of [out] parameter [{parameter.Key}] cannot be passed as [NULL] since the underlying layer cannot guess the datatype.");
                            }

                            cmd.Parameters.Add(new SqlParameter((parameter.Key.StartsWith('@') ? parameter.Key : $"@{parameter.Key}"), parameter.Value)
                            {
                                Direction = ParameterDirection.Output
                            });
                        }
                    }

                    cmd.Parameters.Add(new SqlParameter("@returnValue", null)
                    {
                        Direction = ParameterDirection.ReturnValue
                    });

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken))
                    {
                        int index = 0;
                        do
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            DataTable table = new DataTable($"Table{++index}");

                            // Load schema
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                table.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
                            }

                            // Read rows
                            while (await reader.ReadAsync(cancellationToken))
                            {
                                DataRow row = table.NewRow();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[i] = reader.GetValue(i);
                                }
                                table.Rows.Add(row);

                                cancellationToken.ThrowIfCancellationRequested();
                            }

                            ds.Tables.Add(table);

                        } while (await reader.NextResultAsync(cancellationToken));
                    }

                    if (cmd.Parameters.Contains("@returnValue") && (cmd.Parameters["@returnValue"].Value is not null) && int.TryParse(cmd.Parameters["@returnValue"].Value.ToString(), out returnValue))
                    {
                        if (returnValue < 0)
                        {
                            throw new DataException($"Stored procedure returned {returnValue} (error condition) instead of throwing an exception.");
                        }
                    }

                    Dictionary<string, object?> returnedParameters = new Dictionary<string, object?>();
                    if ((outputParameters is not null) && (outputParameters.Count > 0))
                    {
                        foreach (KeyValuePair<string, object> parameter in outputParameters)
                        {
                            if (cmd.Parameters.Contains(parameter.Key))
                            {
                                returnedParameters.Add(parameter.Key, cmd.Parameters[parameter.Key].Value);
                            }
                        }
                    }

                    return new ProcedureOrFunctionResult(procedureName)
                    {
                        InputParameters = inputParameters,
                        ReturnParameters = (((outputParameters is not null) && (outputParameters.Count > 0)) ? returnedParameters : null),
                        Data = ds,
                        ReturnValue = returnValue
                    };
                }
            }
        }
        catch (OperationCanceledException)
        {
            Logger.DebugLog(nameof(ProcedureOrFunctionAsync), "Operation cancelled by user.");
            return new CancelledResult(procedureName);
        }
        catch (Exception error)
        {
            Logger.DebugLog(nameof(ProcedureOrFunctionAsync), error.Message, error.ToString());
            return new ErrorResult(procedureName, error);
        }
    }

    /// <summary>
    /// Execute a stored procedure or SQL function and return the results.
    /// </summary>
    /// <param name="connectionString">The connection string to connect to the target SQL Server.</param>
    /// <param name="execBuilder">Instance of a <see cref="SqlExecBuilder"/> to build the execution parameters.</param>
    /// <returns>Results of the execution. Will be a <see cref="ProcedureOrFunctionResult"/> if successful or a <see cref="ErrorResult"/> if there was an exception.</returns>
    public static async Task<Result> ProcedureOrFunctionAsync(string connectionString, SqlExecBuilder execBuilder)
    {
        try
        {
            DataSet ds = new DataSet();
            int returnValue = 0;

            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                cn.Open();
                using (SqlCommand cmd = execBuilder.Build())
                {
                    cmd.Connection = cn;

                    Dictionary<string, object?> inputParameters = new Dictionary<string, object?>();
                    foreach (SqlParameter param in execBuilder.InputParameters)
                    {
                        inputParameters.Add(param.ParameterName, param.Value);
                    }

                    Logger.DebugLog(nameof(ProcedureOrFunctionAsync), execBuilder.ProcedureName,
                        Logger.SerialiseDictionary(inputParameters).ToString());

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        int index = 0;
                        do
                        {
                            DataTable table = new DataTable($"Table{++index}");

                            // Load schema
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                table.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
                            }

                            // Read rows
                            while (reader.Read())
                            {
                                DataRow row = table.NewRow();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    row[i] = reader.GetValue(i);
                                }
                                table.Rows.Add(row);
                            }

                            ds.Tables.Add(table);
                        } while (reader.NextResult());
                    }

                    if (cmd.Parameters.Contains("@returnValue") && (cmd.Parameters["@returnValue"].Value is not null) && int.TryParse(cmd.Parameters["@returnValue"].Value.ToString(), out returnValue))
                    {
                        if (returnValue < 0)
                        {
                            throw new DataException($"Stored procedure returned {returnValue} (error condition) instead of throwing an exception.");
                        }
                    }

                    Dictionary<string, object?> returnedParameters = new Dictionary<string, object?>();
                    List<SqlParameter> outputParameters = execBuilder.OutputParameters;

                    if (outputParameters.Count > 0)
                    {
                        foreach (SqlParameter parameter in outputParameters)
                        {
                            if (cmd.Parameters.Contains(parameter.ParameterName))
                            {
                                returnedParameters.Add(parameter.ParameterName, cmd.Parameters[parameter.ParameterName].Value);
                            }
                        }
                    }

                    return new ProcedureOrFunctionResult(execBuilder.ProcedureName)
                    {
                        InputParameters = inputParameters,
                        ReturnParameters = ((outputParameters.Count > 0) ? returnedParameters : null),
                        Data = ds,
                        ReturnValue = returnValue
                    };
                }
            }
        }
        catch (Exception error)
        {
            Logger.DebugLog(nameof(ProcedureOrFunctionAsync), error.Message, error.ToString());
            return new ErrorResult(execBuilder.ProcedureName, error);
        }
    }

    /// <summary>
    /// Execute a non-query SQL statement or script (like INSERT, UPDATE, DELETE) and return the number of affected rows.
    /// </summary>
    /// <param name="connectionString">The connection string to connect to the target SQL Server.</param>
    /// <param name="script">The statement or script to execute.</param>
    /// <param name="cancellationToken">The asynchronous operation cancellation token.</param>
    /// <returns>Results of the query execution as a <see cref="NonQueryResult"/> if successful. Otherwise returns a <see cref="ErrorResult"/>.</returns>
    public static async Task<Result> NonQueryAsync(string connectionString, StringBuilder script, CancellationToken cancellationToken)
    {
        string statement = script.ToString();
        if (string.IsNullOrWhiteSpace(statement))
        {
            return new ErrorResult(statement, new ArgumentException("Statement or script cannot be null or empty."));
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (SqlConnection cn = new SqlConnection(connectionString))
            {
                await cn.OpenAsync(cancellationToken);
                using (SqlCommand cmd = cn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = statement;

                    Logger.DebugLog(nameof(NonQueryAsync), statement);

                    int count = await cmd.ExecuteNonQueryAsync(cancellationToken);

                    return new NonQueryResult(statement, count);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Logger.DebugLog(nameof(NonQueryAsync), "Operation cancelled by user.");
            return new CancelledResult(statement);
        }
        catch (Exception ex)
        {
            Logger.DebugLog(nameof(NonQueryAsync), ex.Message, ex.ToString());
            return new ErrorResult(statement, ex);
        }
    }

    /// <summary>
    /// Execute a non-query SQL statement or script (like INSERT, UPDATE, DELETE) and return the number of affected rows.
    /// </summary>
    /// <param name="connectionString">The connection string to connect to the target SQL Server.</param>
    /// <param name="script">The statement or script to execute.</param>
    /// <param name="cancellationToken">The asynchronous operation cancellation token.</param>
    /// <returns>Results of the query execution as a <see cref="NonQueryResult"/> if successful. Otherwise returns a <see cref="ErrorResult"/>.</returns>
    public static async Task<Result> NonQueryAsync(string connectionString, SqlStatementBuilder script, CancellationToken cancellationToken)
    {
        if (script is SqlQueryBuilder)
        {
            throw new ArgumentException("SQL SELECT queries cannot be executed as non-queries. Use the QueryAsync() method instead.");
        }

        return await NonQueryAsync(connectionString, script.Build(), cancellationToken);
    }
}
