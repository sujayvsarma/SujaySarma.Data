using SujaySarma.Data.Core.ReflectionUtilities;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SujaySarma.Data.SqlServer;

/// <summary>
/// Performs query and operation logging.
/// </summary>
internal static class Logger
{
    /// <summary>
    /// Write to the debug log (if debugging is enabled).
    /// </summary>
    /// <param name="content">Content or message to log.</param>
    public static void DebugLog(params string[] content)
    {
        if (_isDebuggingEnabled && (_debuggingLogFile != null))
        {
            _debuggingLogFile.Write($"{DateTime.Now:yyyy-MM-ddTHH:mm:ss}");
            foreach (string item in content)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    _debuggingLogFile.Write("\r\n>>\t");
                    _debuggingLogFile.Write(item);
                }
            }
            _debuggingLogFile.WriteLine();
        }
    }

    /// <summary>
    /// Serialise a dictionary to a string.
    /// </summary>
    /// <typeparam name="TKey">Type of the key of the dictionary.</typeparam>
    /// <typeparam name="TValue">Type of values of the dictionary.</typeparam>
    /// <param name="dictionary">The dictionary to serialise.</param>
    /// <returns>An instance of a <see cref="StringBuilder"/> populated with the serialised key/pair values from the <paramref name="dictionary"/>.</returns>
    public static StringBuilder SerialiseDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        where TKey : notnull
    {
        StringBuilder sb = new StringBuilder();

        foreach(KeyValuePair<TKey, TValue> pair in dictionary)
        {
            sb.AppendLine($"Key: {pair.Key}, Value: {(pair.Value.IsNull() ? string.Empty : pair.Value)}");
        }

        return sb;
    }

    /// <summary>
    /// Turns debugging off. If it was previously turned on, flushes the content 
    /// to the file and closes it.
    /// </summary>
    public static void EndDebugging()
    {
        _isDebuggingEnabled = false;
        if (_debuggingLogFile != null)
        {
            _debuggingLogFile.Flush();
            _debuggingLogFile.Dispose();
            _debuggingLogFile = null;
        }
    }

    /// <summary>
    /// Turn debugging on and open a <see cref="StreamWriter"/> to write into 
    /// the configured log file.
    /// </summary>
    /// <param name="logFilePath">Absolute path to the logfile to write into.</param>
    /// <returns>Instance of self.</returns>
    public static void BeginDebugging(string logFilePath)
    {
        _isDebuggingEnabled = true;
        _debuggingLogFile = new StreamWriter(logFilePath, true, Encoding.UTF8);
    }

    /// <summary>
    /// Static initialiser.
    /// </summary>
    static Logger()
    {
        if ((Environment.GetEnvironmentVariable("SQLCONTEXT_DUMPSQL") != null))
        {
            string logfile = Environment.GetEnvironmentVariable("SQLCONTEXT_DUMPSQLFILE")
                ?? Path.GetTempFileName();

            BeginDebugging(logfile);
        }
        else
        {
            EndDebugging();
        }
    }

    private static bool _isDebuggingEnabled = false;
    private static StreamWriter? _debuggingLogFile = null;
}
