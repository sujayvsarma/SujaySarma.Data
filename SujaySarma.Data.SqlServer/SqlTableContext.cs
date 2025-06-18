namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// Enables "connection-less" SQL Server table interactivity experience.
    /// </summary>
    public partial class SqlTableContext
    {

        /// <summary>Initialize</summary>
        /// <param name="connectionString">Connection string to use</param>
        public SqlTableContext(string connectionString) 
            => _connectionString = connectionString;

        /// <summary>
        /// Initialize - with local SQL Server, connected to TempDB
        /// </summary>
        public SqlTableContext()
          : this("Data Source=(local);Initial Catalog=tempdb;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=True;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True")
        {
        }

        /// <summary>
        /// Return a context pointed to the local SQL Server and specified database
        /// </summary>
        /// <param name="databaseName">Name of database to connect to</param>
        /// <returns>Context pointed to <paramref name="databaseName" /> database</returns>
        public static SqlTableContext WithLocalDatabase(string databaseName)
            => new SqlTableContext($"Data Source=(local);Initial Catalog={databaseName};Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=True;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True");

        /// <summary>
        /// Return a context pointed to the server and database specified by the <paramref name="connectionString" />.
        /// </summary>
        /// <param name="connectionString">Connection string to a SQL Server and database</param>
        /// <returns>Context pointed to <paramref name="connectionString" /> server and database</returns>
        public static SqlTableContext WithConnectionString(string connectionString)
            => new SqlTableContext(connectionString);

        private readonly string _connectionString;
        
    }
}
