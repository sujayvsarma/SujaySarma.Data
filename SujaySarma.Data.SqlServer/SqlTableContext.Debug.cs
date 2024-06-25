using System;

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// Debugging functionality. Since we distribute DLLs only in Release mode, these functions are 
    /// available and are fully-functional in RELEASE mode! Use with care!!!
    /// </summary>
    public partial class SqlTableContext
    {
        
        /// <summary>
        /// Dump the sql to console if enabled
        /// </summary>
        /// <param name="sql">SQL to dump</param>
        private static void DumpGeneratedSqlToConsole(string sql)
        {
            if (Environment.GetEnvironmentVariable(DUMP_SQL_FLAG) != null)
            {
                Console.WriteLine(sql);
            }
        }

        /// <summary>
        /// Enable the dump generated SQL to console flag
        /// </summary>
        public static void EnableDumpSqlToConsole()
        {
            Environment.SetEnvironmentVariable(DUMP_SQL_FLAG, "true");
        }

        /// <summary>
        /// Disable the dump generated SQL to console flag
        /// </summary>
        public static void DisableDumpSqlToConsole()
        {
            Environment.SetEnvironmentVariable(DUMP_SQL_FLAG, null);
        }

        /// <summary>
        /// Name of the SQL-debug environment variable. Value can be set to anything.
        /// </summary>
        public const string DUMP_SQL_FLAG = "SQLTABLECONTEXT_DUMPSQL";

    }
}
