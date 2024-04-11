using System.Threading.Tasks;

using Microsoft.Data.SqlClient;

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// Utility functions for SqlTableContext
    /// </summary>
    public partial class SqlTableContext
    {

        /// <summary>
        /// Check if the target database and server can be connected to
        /// </summary>
        /// <returns>True if database and server can be connected to.</returns>
        public bool IsConnectable()
        {
            bool result = false;
            using (SqlConnection cn = new(_connectionString))
            {
                try
                {
                    cn.Open(SqlConnectionOverrides.OpenWithoutRetry);
                    result = true;
                }
                catch
                {
                    // the only error we can get here is a connection problem
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// Check if the target database and server can be connected to
        /// </summary>
        /// <returns>True if database and server can be connected to.</returns>
        public async Task<bool> IsConnectableAsync()
        {
            bool result = false;

            string connectionStringWithoutRetry = _connectionString + ";ConnectRetryCount=0";
            using (SqlConnection cn = new(connectionStringWithoutRetry))
            {
                try
                {
                    await cn.OpenAsync();
                    result = true;
                }
                catch
                {
                    // the only error we can get here is a connection problem
                    result = false;
                }
            }

            return result;
        }
    }
}
