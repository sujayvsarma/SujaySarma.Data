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
    /// Provides synchronous and asynchronous Query methods that return a single raw/binary column/value (not entire row[s]!)
    /// </summary>
    public partial class SqlContext
    {

        /// <summary>
        /// Execute a query and return raw/binary data from (<paramref name="atPosition"/>, offset: <paramref name="offset"/>) position in the record and of <paramref name="expectedLength"/> bytes size.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="expectedLength">The expected maximum size of the data to read.</param>
        /// <param name="atPosition">Position of the column/data in the record buffer.</param>
        /// <param name="offset">Offset in bytes from <paramref name="atPosition"/> to start reading.</param>
        /// <returns>A byte[] of <paramref name="expectedLength"/> bytes size. Exceptions are re-thrown.</returns>
        public byte[] QueryBinary(StringBuilder query, int expectedLength, int atPosition = 0, long offset = 0L)
        {
            string command = query.ToString();
            byte[] buffer = new byte[expectedLength];

            using (SqlConnection cn = new SqlConnection(_connectionString))
            {
                cn.Open();

                using (SqlCommand cmd = cn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = command;

                    DebugWrite(nameof(QueryBinary), command);

                    using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
                    {
                        if (reader.Read())
                        {
                            reader.GetBytes(atPosition, offset, buffer, 0, expectedLength);
                        }
                    }
                }
            }

            return buffer;
        }

        /// <summary>
        /// Execute a query and return raw/binary data from (<paramref name="atPosition"/>, offset: <paramref name="offset"/>) position in the record and of <paramref name="expectedLength"/> bytes size.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="expectedLength">The expected maximum size of the data to read.</param>
        /// <param name="atPosition">Position of the column/data in the record buffer.</param>
        /// <param name="offset">Offset in bytes from <paramref name="atPosition"/> to start reading.</param>
        /// <returns>A byte[] of <paramref name="expectedLength"/> bytes size. Exceptions are re-thrown.</returns>
        public byte[] QueryBinary(SqlQueryBuilder query, int expectedLength, int atPosition = 0, long offset = 0L)
        {
            return QueryBinary(query.Build(), expectedLength, atPosition, offset);
        }

        /// <summary>
        /// Execute a query and return raw/binary data from (<paramref name="atPosition"/>, offset: <paramref name="offset"/>) position in the record and of <paramref name="expectedLength"/> bytes size.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="expectedLength">The expected maximum size of the data to read.</param>
        /// <param name="atPosition">Position of the column/data in the record buffer.</param>
        /// <param name="offset">Offset in bytes from <paramref name="atPosition"/> to start reading.</param>
        /// <returns>A byte[] of <paramref name="expectedLength"/> bytes size. Exceptions are re-thrown.</returns>
        public async Task<byte[]> QueryBinaryAsync(StringBuilder query, int expectedLength, int atPosition = 0, long offset = 0L)
        {
            string command = query.ToString();
            byte[] buffer = new byte[expectedLength];

            using (SqlConnection cn = new SqlConnection(_connectionString))
            {
                await cn.OpenAsync();

                using (SqlCommand cmd = cn.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = command;

                    DebugWrite(nameof(QueryBinary), command);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess))
                    {
                        if (await reader.ReadAsync())
                        {
                            reader.GetBytes(atPosition, offset, buffer, 0, expectedLength);
                        }
                    }
                }
            }

            return buffer;
        }

        /// <summary>
        /// Execute a query and return raw/binary data from (<paramref name="atPosition"/>, offset: <paramref name="offset"/>) position in the record and of <paramref name="expectedLength"/> bytes size.
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <param name="expectedLength">The expected maximum size of the data to read.</param>
        /// <param name="atPosition">Position of the column/data in the record buffer.</param>
        /// <param name="offset">Offset in bytes from <paramref name="atPosition"/> to start reading.</param>
        /// <returns>A byte[] of <paramref name="expectedLength"/> bytes size. Exceptions are re-thrown.</returns>
        public async Task<byte[]> QueryBinaryAsync(SqlQueryBuilder query, int expectedLength, int atPosition = 0, long offset = 0L)
        {
            return await QueryBinaryAsync(query.Build(), expectedLength, atPosition, offset);
        }

    }
}
