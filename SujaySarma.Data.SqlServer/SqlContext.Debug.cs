using System;
using System.IO;

namespace SujaySarma.Data.SqlServer
{
    // This instance of the partial class provides methods for debugging the calls passed through the SqlTableContext class.
    public partial class SqlContext
    {
        /// <summary>
        /// Dump the sql to log if enabled.
        /// </summary>
        /// <param name="executingMethod">Name of the currently executing method.</param>
        /// <param name="sql">SQL to dump.</param>
        public SqlContext DebugWrite(string executingMethod, string sql)
        {
            if (_isDebuggingEnabled)
            {
                _debugDumpFile!.WriteLine($"{DateTime.Now:yyyy-MM-ddTHH:mm:ss}\t{executingMethod}\t{sql}");
            }
            return this;
        }

        /// <summary>
        /// Fluid-style method to enable debugging.
        /// </summary>
        /// <param name="debugFileName">(Optional) The filename to debug into. If NULL, creates a temporary file instead.</param>
        /// <returns>Self-instance.</returns>
        public SqlContext WithDebuggingOn(string? debugFileName = null)
        {
            _isDebuggingEnabled = true;

            if (string.IsNullOrWhiteSpace(debugFileName))
            {
                _debugDumpFile = new StreamWriter(Path.GetTempFileName(), false);
            }
            else
            {
                _debugDumpFile = new StreamWriter(debugFileName, true, System.Text.Encoding.UTF8);
            }

            return this;
        }

        /// <summary>
        /// Fluid-style method to disable debugging.
        /// </summary>
        /// <returns>Self-instance.</returns>
        public SqlContext WithDebuggingOff()
        {
            _isDebuggingEnabled = false;

            // Leave the file alone as we may need to write into it.

            return this;
        }

        /// <summary>
        /// This is called from the default constructor to setup the debug environment if required.
        /// </summary>
        private void CheckEnvironmentForDebugFlag()
        {
            if (Environment.GetEnvironmentVariable("SQLCONTEXT_DUMPSQL") == null)
            {
                WithDebuggingOn(Environment.GetEnvironmentVariable("SQLCONTEXT_DUMPSQLFILE"));
            }
        }


        private static bool _isDebuggingEnabled = false;
        private static StreamWriter? _debugDumpFile = null;
    }
}
