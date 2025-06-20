using System;
using System.Text;

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// Helps build connection strings to connect to a SQL Server.
    /// </summary>
    public class SqlServerConnectionStringBuilder
    {
        /// <summary>
        /// Finalise the connection string building process and return the usable connection string.
        /// </summary>
        /// <returns>The final usable connection string.</returns>
        public string Build()
        {
            StringBuilder builder = new StringBuilder();

            if (_useIntegratedSecurity)
            {
                builder.Append($"Server={_dataSource};Integrated Security=true;");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(_userName) || string.IsNullOrWhiteSpace(_password))
                {
                    throw new InvalidOperationException("Username and password must be set if not using integrated security.");
                }

                builder.Append($"Server={_dataSource};User ID={_userName};Password={_password};");
            }

            if (string.IsNullOrWhiteSpace(_databaseName))
            {
                _databaseName = "TempDB";
            }
            builder.Append($"Initial Catalog={_databaseName}")
                .Append($"Connect Timeout={_connectionTimeout}");

            builder.Append($"Persist Security Info={(_persistSecurityInfo ? "True" : "False")}")
                .Append($"TrustServerCertificate={(_trustServerCertificate ? "True" : "False")}")
                .Append($"Encrypt={(_allowEncryptedCommunication ? "True" : "False")}")
                .Append($"MultipleActiveResultSets={(_allowMultipleResultsets ? "True" : "False")}")
                .Append($"Pooling={(_allowConnectionPooling ? "True" : "False")}")
                ;

            return builder.ToString();
        }

        /// <summary>
        /// If called, will cause server certificates to not be trusted. This may result in connection errors unless you know what you are doing.
        /// </summary>
        /// <returns>Self-instance.</returns>
        public SqlServerConnectionStringBuilder DoNOTTrustServerCertificate()
        {
            _trustServerCertificate = false;
            return this;
        }

        /// <summary>
        /// If called, disables encrypted queries and result transfers over the wire. This will expose your database interactions to anybody that can listen in.
        /// </summary>
        /// <returns>Self-instance.</returns>
        public SqlServerConnectionStringBuilder DisableEncryptedTransfers()
        {
            _allowEncryptedCommunication = false;
            return this;
        }

        /// <summary>
        /// If called, disables returning of multiple results from query/non-query executions. Multiple results are required if a query or stored procedure execution 
        /// results in multiple "tables" being returned -- if this is turned off, then only the first one will be accessible.
        /// </summary>
        /// <returns>Self-instance.</returns>
        public SqlServerConnectionStringBuilder DisableMultipleResultsets()
        {
            _allowMultipleResultsets = false;
            return this;
        }

        /// <summary>
        /// Sets the connection timeout value. Default is 60 seconds.
        /// </summary>
        /// <param name="seconds">Connection time out in seconds.</param>
        /// <returns>Self-instance.</returns>
        public SqlServerConnectionStringBuilder WithConnectionTimeout(uint seconds)
        {
            _connectionTimeout = seconds;
            return this;
        }

        /// <summary>
        /// If called, sets Connection Pooling to FALSE. Otherwise this will default to TRUE.
        /// </summary>
        /// <returns></returns>
        public SqlServerConnectionStringBuilder DisableConnectionPooling()
        {
            _allowConnectionPooling = false;
            return this;
        }

        /// <summary>
        /// If called, sets Persist Security Info to TRUE. Otherwise this will default to FALSE.
        /// </summary>
        /// <returns>Self-instance.</returns>
        public SqlServerConnectionStringBuilder AllowSecurityInfoPersistence()
        {
            _persistSecurityInfo = true;
            return this;
        }

        /// <summary>
        /// Sets the target database to interact with. If not set, will be NULL and defaults to TempDB.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns>Self-instance.</returns>
        public SqlServerConnectionStringBuilder SetDatabase(string databaseName)
        {
            _databaseName = databaseName;
            return this;
        }

        /// <summary>
        /// Sets the connection to use a UserID/Password based login credential.
        /// </summary>
        /// <param name="userName">Username to use.</param>
        /// <param name="password">Password in clear text.</param>
        /// <returns>Self-instance.</returns>
        public SqlServerConnectionStringBuilder UseLoginCredentials(string userName, string password)
        {
            _useIntegratedSecurity = false;
            _userName = userName;
            _password = password;
            return this;
        }

        /// <summary>
        /// Set the connection to use trusted or integrated security (SSPI).
        /// </summary>
        /// <returns>Self-instance.</returns>
        public SqlServerConnectionStringBuilder UsesIntegratedSecurity()
        {
            _useIntegratedSecurity = true;
            _userName = null;
            _password = null;
            return this;
        }

        /// <summary>
        /// Creates a new instance of the builder and sets the target datasource
        /// </summary>
        /// <param name="serverAddress">The server's NETBIOS or IPv4 or IPv6 name or numerical address.</param>
        /// <param name="portNumber">The port number. Omitting it will set it to default port (1433)</param>
        /// <returns>The newly created instance</returns>
        public static SqlServerConnectionStringBuilder TargetDataSource(string serverAddress, uint portNumber = 1433)
        {
            SqlServerConnectionStringBuilder builder = new SqlServerConnectionStringBuilder($"{serverAddress},{portNumber}");
            return builder;
        }

        /// <summary>
        /// Constructor, private. Callers must use the static method(s) to create a new instance.
        /// </summary>
        /// <param name="dataSource">The target datasource.</param>
        private SqlServerConnectionStringBuilder(string dataSource)
        {
            _dataSource = dataSource;
        }

        private readonly string? _dataSource;
        
        private bool _useIntegratedSecurity = false;
        private string? _userName, _password;
        private string? _databaseName;
        
        private uint _connectionTimeout = 60;

        private bool _persistSecurityInfo = false;
        private bool _allowConnectionPooling = true;
        private bool _allowMultipleResultsets = true;
        private bool _allowEncryptedCommunication = true;
        private bool _trustServerCertificate = true;
    }
}
