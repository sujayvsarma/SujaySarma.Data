
#if NET7_0_OR_GREATER
using Microsoft.Data.SqlClient;
#else
using System.Data.SqlClient;
#endif

using System.Threading.Tasks;

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// Enables "connection-less" SQL Server table interactivity experience.
    /// </summary>
    public partial class SqlContext
    {

        #region Utility Methods

        /// <summary>
        /// Check if the target database and server can be connected to.
        /// </summary>
        /// <returns>True if database and server can be connected to.</returns>
        public bool IsConnectable()
        {
            using (SqlConnection cn = new SqlConnection(this._connectionString))
            {
                try
                {
#if NET7_0_OR_GREATER
                    cn.Open(SqlConnectionOverrides.OpenWithoutRetry);
#else
                    cn.Open();
#endif
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Check if the target database and server can be connected to.
        /// </summary>
        /// <returns>True if database and server can be connected to.</returns>
        public async Task<bool> IsConnectableAsync()
        {
            bool flag = false;
            using (SqlConnection cn = new SqlConnection(this._connectionString + ";ConnectRetryCount=0"))
            {
                try
                {
#if NET7_0_OR_GREATER
                    await cn.OpenAsync(SqlConnectionOverrides.OpenWithoutRetry, new System.Threading.CancellationToken());
#else
                    await cn.OpenAsync();
#endif
                    flag = true;
                }
                catch
                {
                    flag = false;
                }
            }
            return flag;
        }

        #endregion

        #region Constructors -- PRIVATE

        /// <summary>
        /// Initialise with the provided connection string.
        /// </summary>
        /// <param name="connectionString">Connection string to use.</param>
        private SqlContext(string connectionString)
            : this()
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Initialize - with local SQL Server, connected to TempDB. Uses integrated security.
        /// </summary>
        private SqlContext()
        {
            _connectionString = default!;

            // Check if env. debugging flag is ON or OFF and set up....
            CheckEnvironmentForDebugFlag();
        }

        #endregion

        /// <summary>
        /// Initialise using integrated security information.
        /// </summary>
        /// <param name="serverName">The name or address (maybe domain or IP) of the server, Eg: "sqlserver01", "sqlserver01.xyz.databases.db", "123.456.789.012".</param>
        /// <param name="serverPortNumber">The port number the target SQL Server listens to. Default: 1433.</param>
        /// <param name="databaseName">Name of database to connect to.</param>
        /// <param name="connectionTimeout">Connection timeout in seconds. Default: 60 (seconds).</param>
        /// <param name="persistSecurityInfo">If set, saves connection security (credentials) across calls allowing you to retrieve them later from the SqlConnection object's properties. Default: FALSE.</param>
        /// <param name="allowPooling">If set, allows connection pooling for better performance. Default: TRUE.</param>
        /// <param name="allowMultipleResults">If set, allows multiple tables to be returned through query execution. Default: TRUE.</param>
        /// <param name="encryption">If set, turns on encrypted data transfer between this client and SQL Server. Default: TRUE.</param>
        /// <param name="trustServerCertificate">If set, trusts the SQL Server's server certificate. Default: TRUE.</param>
        /// <returns>A usable SqlContext instance.</returns>
        public static SqlContext WithIntegratedSecurity(string serverName = "(local)", uint serverPortNumber = 1433, string databaseName = "TempDB", uint connectionTimeout = 60, bool persistSecurityInfo = false, bool allowPooling = true, bool allowMultipleResults = true, bool encryption = true, bool trustServerCertificate = true)
        {
            SqlServerConnectionStringBuilder builder = SqlServerConnectionStringBuilder
                .TargetDataSource(serverName, serverPortNumber)
                    .UsesIntegratedSecurity()
                        .SetDatabase(databaseName)
                            .WithConnectionTimeout(connectionTimeout);

            if (persistSecurityInfo)
            {
                builder.AllowSecurityInfoPersistence();
            }

            if (!allowPooling)
            {
                builder.DisableConnectionPooling();
            }

            if (!allowMultipleResults)
            {
                builder.DisableMultipleResultsets();
            }

            if (!encryption)
            {
                builder.DisableEncryptedTransfers();
            }

            if (!trustServerCertificate)
            {
                builder.DoNOTTrustServerCertificate();
            }

            return new SqlContext(builder.Build());
        }

        /// <summary>
        /// Initialise using username/password based credential security information.
        /// </summary>
        /// <param name="userName">The username/User ID to use to login.</param>
        /// <param name="password">The password (in clear text!) to use to login.</param>
        /// <param name="serverName">The name or address (maybe domain or IP) of the server, Eg: "sqlserver01", "sqlserver01.xyz.databases.db", "123.456.789.012".</param>
        /// <param name="serverPortNumber">The port number the target SQL Server listens to. Default: 1433.</param>
        /// <param name="databaseName">Name of database to connect to.</param>
        /// <param name="connectionTimeout">Connection timeout in seconds. Default: 60 (seconds).</param>
        /// <param name="persistSecurityInfo">If set, saves connection security (credentials) across calls allowing you to retrieve them later from the SqlConnection object's properties. Default: FALSE.</param>
        /// <param name="allowPooling">If set, allows connection pooling for better performance. Default: TRUE.</param>
        /// <param name="allowMultipleResults">If set, allows multiple tables to be returned through query execution. Default: TRUE.</param>
        /// <param name="encryption">If set, turns on encrypted data transfer between this client and SQL Server. Default: TRUE.</param>
        /// <param name="trustServerCertificate">If set, trusts the SQL Server's server certificate. Default: TRUE.</param>
        /// <returns>A usable SqlContext instance.</returns>
        public static SqlContext WithPasswordCredential(string userName, string password, string serverName = "(local)", uint serverPortNumber = 1433, string databaseName = "TempDB", uint connectionTimeout = 60, bool persistSecurityInfo = false, bool allowPooling = true, bool allowMultipleResults = true, bool encryption = true, bool trustServerCertificate = true)
        {
            SqlServerConnectionStringBuilder builder = SqlServerConnectionStringBuilder
                .TargetDataSource(serverName, serverPortNumber)
                    .UseLoginCredentials(userName, password)
                        .SetDatabase(databaseName)
                            .WithConnectionTimeout(connectionTimeout);

            if (persistSecurityInfo)
            {
                builder.AllowSecurityInfoPersistence();
            }

            if (!allowPooling)
            {
                builder.DisableConnectionPooling();
            }

            if (!allowMultipleResults)
            {
                builder.DisableMultipleResultsets();
            }

            if (!encryption)
            {
                builder.DisableEncryptedTransfers();
            }

            if (!trustServerCertificate)
            {
                builder.DoNOTTrustServerCertificate();
            }

            return new SqlContext(builder.Build());
        }

        private readonly string _connectionString;
    }
}
