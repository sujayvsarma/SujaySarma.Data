using System;

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// Debug time only functionality
    /// </summary>
    public partial class SqlTableContext
    {
        
        /// <summary>
        /// Dump the sql to console if enabled
        /// </summary>
        /// <param name="sql">SQL to dump</param>
        private static void DumpGeneratedSqlToConsole(string sql)
        {
#if DEBUG
            if (Environment.GetEnvironmentVariable(DUMP_SQL_FLAG) != null)
            {
                Console.WriteLine(sql);
            }
#endif
        }

        /// <summary>
        /// Name of the SQL-debug environment variable. Value can be set to anything.
        /// </summary>
        public const string DUMP_SQL_FLAG = "SQLTABLECONTEXT_DUMPSQL";

    }
}
