using System;
using System.IO;
using System.Text;

namespace SujaySarma.Data.SqlServer
{
    // Debugging functions
    public partial class SqlTableContext
    {
        /// <summary>
        /// Dump the sql to console if enabled
        /// </summary>
        /// <param name="executingMethod">Name of the currently executing method</param>
        /// <param name="sql">SQL to dump</param>
        private static void DebugWriteSql(string executingMethod, string sql)
        {
            if (Environment.GetEnvironmentVariable("SQLTABLECONTEXT_DUMPSQL") == null)
            {
                return;
            }

            StringBuilder formattedLog = new StringBuilder();
            formattedLog.Append($"{DateTime.Now:yyyy-MM-ddTHH:mm:ss}\t{executingMethod}\t{sql}");

            if (_sqlDumpFile != null)
            {
                using (StreamWriter sw = new StreamWriter(_sqlDumpFile, append: true))
                {
                    sw.WriteLine(formattedLog.ToString());
                }
            }
        }

        /// <summary>
        /// Enable the dump generated SQL to console flag
        /// </summary>
        public static void EnableSqlDebug()
        {
            Environment.SetEnvironmentVariable("SQLTABLECONTEXT_DUMPSQL", "true");
            _sqlDumpFile = Path.GetTempFileName();

            Console.WriteLine($"SQL Logging: Enabled!");
            Console.WriteLine($"SQL Log File: {_sqlDumpFile}");
        }

        /// <summary>
        /// Disable the dump generated SQL to console flag
        /// </summary>
        public static void DisableSqlDebug()
        {
            Environment.SetEnvironmentVariable("SQLTABLECONTEXT_DUMPSQL", null);
            _sqlDumpFile = null;

            Console.WriteLine($"SQL Logging: Disabled!");
        }

        /// <summary>
        /// Name of the SQL-debug environment variable. Value can be set to anything.
        /// </summary>
        public const string DUMP_SQL_FLAG = "SQLTABLECONTEXT_DUMPSQL";

        private static string? _sqlDumpFile = null;
    }
}
