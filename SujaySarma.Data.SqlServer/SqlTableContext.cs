using System;
using System.Collections.Generic;
using System.Data;

using Microsoft.Data.SqlClient;

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// Enables connection-less SQL Server table interaction
    /// </summary>
    public partial class SqlTableContext
    {

        /// <summary>
        /// Execute a stored procedure and fetch the results
        /// </summary>
        /// <param name="procedureName">Stored procedure to run</param>
        /// <param name="inParameters">[OPTIONAL] Dictionary of IN parameters (keys are parameter names, values are param values)</param>
        /// <param name="outParameters">[OPTIONAL] Dictionary of OUT parameters. If this dictionary is NULL, then no out parameters are retrieved</param>
        /// <param name="commandTimeout">[OPTIONAL] Timeout of command. Default of 30 seconds.</param>
        public StoredProcedureExecutionResult ExecuteStoredProcedure(string procedureName, Dictionary<string, object?>? inParameters = null, Dictionary<string, object?>? outParameters = null, int commandTimeout = 30)
        {
            StoredProcedureExecutionResult result = new StoredProcedureExecutionResult()
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

                using SqlConnection cn = new SqlConnection(_connectionString);
                cn.Open();

                using SqlCommand cmd = new SqlCommand(procedureName, cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = commandTimeout;

                if ((inParameters != null) && (inParameters.Count > 0))
                {
                    foreach (string key in inParameters.Keys)
                    {
                        cmd.Parameters.Add(CreateParameter(key, inParameters[key], ParameterDirection.Input));
                    }
                }

                if ((outParameters != null) && (outParameters.Count > 0))
                {
                    foreach (string key in outParameters.Keys)
                    {
                        if (outParameters[key] == null)
                        {
                            throw new ArgumentException(string.Format("Value of [out] parameter [{0}] cannot be passed as [NULL] since the underlying layer cannot guess the datatype.", key));
                        }

                        cmd.Parameters.Add(CreateParameter(key, outParameters[key], ParameterDirection.Output));
                    }
                }

                cmd.Parameters.Add(CreateParameter("@returnValue", null, ParameterDirection.ReturnValue));

                using SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(ds);

                if (cmd.Parameters["@returnvalue"].Value != null)
                {
                    if (int.TryParse(cmd.Parameters["@returnvalue"].Value.ToString(), out int ret))
                    {
                        result.ReturnValue = ret;

                        if (ret < 0)
                        {
                            result.IsError = true;
                            result.Messages = $"Stored procedure returned {ret} (error condition) instead of throwing an exception.";
                        }
                    }
                }

                if (outParameters != null)
                {
                    result.ReturnParameters = new Dictionary<string, object?>();
                    foreach (string key in outParameters.Keys)
                    {
                        if (cmd.Parameters[key] != null)
                        {
                            result.ReturnParameters.Add(key, cmd.Parameters[key].Value);
                        }
                    }
                }

                if (!result.IsError)
                {
                    NameTables(ds);
                    result.Results = ds;
                }
            }
            catch (Exception mainEx)
            {
                result.IsError = true;
                result.Messages = mainEx.Message;
                result.Exception = mainEx;
            }

            // result return
            return result;

            // create sql parameter (local helper)
            static SqlParameter CreateParameter(string name, object? value, ParameterDirection direction)
            {
                SqlParameter p = new SqlParameter((name.StartsWith('@') ? name : ("@" + name)), value)
                {
                    Direction = direction
                };
                return p;
            }

            // name the tables in the dataset (local helper)
            static DataSet NameTables(DataSet? dataSet)
            {
                if ((dataSet == null) || (dataSet == default) || (dataSet == default(DataSet)))
                {
                    return new DataSet();
                }

                if (dataSet.Tables.Count < 1)
                {
                    return dataSet;
                }

                for (int tableIndex = 0; tableIndex < dataSet.Tables.Count; tableIndex++)
                {
                    if (string.IsNullOrEmpty(dataSet.Tables[tableIndex].TableName))
                    {
                        dataSet.Tables[tableIndex].TableName = $"Table{tableIndex + 1}";
                    }
                }

                return dataSet;
            }
        }


        #region Constructors

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="connectionString">Connection string to use</param>
        public SqlTableContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Initialize - with local SQL Server, connected to TempDB
        /// </summary>
        public SqlTableContext()
            : this(LOCAL_SQL_SERVER)
        {
        }

        /// <summary>
        /// Return a context pointed to the local SQL Server and specified database
        /// </summary>
        /// <param name="databaseName">Name of database to connect to</param>
        /// <returns>Context pointed to <paramref name="databaseName"/> database</returns>
        public static SqlTableContext WithLocalDatabase(string databaseName)
            => new SqlTableContext(string.Format(CONNECTIONSTRING_FORMAT, databaseName));

        /// <summary>
        /// Return a context pointed to the server and database specified by the <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">Connection string to a SQL Server and database</param>
        /// <returns>Context pointed to <paramref name="connectionString"/> server and database</returns>
        public static SqlTableContext WithConnectionString(string connectionString)
            => new SqlTableContext(connectionString);

        #endregion

        private readonly string _connectionString;
        private const string LOCAL_SQL_SERVER = "Data Source=(local);Initial Catalog=tempdb;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=True;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True";
        private const string CONNECTIONSTRING_FORMAT = "Data Source=(local);Initial Catalog={0};Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=True;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True";
    }
}
