using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using SujaySarma.Data.Core.Reflection;
using SujaySarma.Data.Files.TokenLimitedFiles.Serialisation;

namespace SujaySarma.Data.Files.TokenLimitedFiles
{
    /*
        Implementation of Static methods
    */
    public sealed partial class TokenLimitedFileReader
    {
        /// <summary>
        /// Read content from the <paramref name="reader"/> and return it as a <see cref="DataTable">.
        /// </summary>
        /// <param name="reader">An initialised instance of a <see cref="TokenLimitedFileReader"/></param>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <param name="hasHeaderRow">Flag indicating if header rows are expected</param>
        /// <param name="headerRowIndex">If <paramref name="hasHeaderRow"/> is true, then the ONE-based row index from the start of the content in instance where the column names are present</param>
        /// <returns>A DataTable containing zero or more rows of data, potentially with column definitions</returns>
        public static DataTable ReadToTable(TokenLimitedFileReader reader, string tableName = "Table 1", bool hasHeaderRow = true, ulong headerRowIndex = 1)
        {
            DataTable table = new(tableName);
            while (!reader.EndOfStream)
            {
                string?[]? fileRow = reader.ReadRow();
                if (fileRow != null)
                {
                    if (hasHeaderRow && (reader.RowCount == headerRowIndex))
                    {
                        for (int i = 0; i < fileRow.Length; i++)
                        {
                            table.Columns.Add((string.IsNullOrWhiteSpace(fileRow[i]) ? $"Column {i}" : fileRow[i]), typeof(string));
                        }

                        continue;
                    }

                    DataRow newTableRow = table.NewRow();
                    for (int i = 0; i < fileRow.Length; i++)
                    {
                        try
                        {
                            newTableRow[i] = ReflectionUtils.ConvertValueIfRequired(fileRow[0], typeof(string));
                        }
                        catch
                        {
                            newTableRow[i] = fileRow[i]?.ToString();
                        }
                    }
                    table.Rows.Add(newTableRow);
                }
            }

            return table;
        }

        /// <summary>
        /// Read content from the <paramref name="reader"/> and return it as a <see cref="DataTable">.
        /// </summary>
        /// <param name="reader">An initialised instance of a <see cref="TokenLimitedFileReader"/></param>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <param name="hasHeaderRow">Flag indicating if header rows are expected</param>
        /// <param name="headerRowIndex">If <paramref name="hasHeaderRow"/> is true, then the ONE-based row index from the start of the content in instance where the column names are present</param>
        /// <returns>A DataTable containing zero or more rows of data, potentially with column definitions</returns>
        public static async Task<DataTable> ReadToTableAsync(TokenLimitedFileReader reader, string tableName = "Table 1", bool hasHeaderRow = true, ulong headerRowIndex = 1)
        {
            DataTable table = new(tableName);
            while (!reader.EndOfStream)
            {
                string?[]? fileRow = await reader.ReadRowAsync();
                if (fileRow != null)
                {
                    if (hasHeaderRow && (reader.RowCount == headerRowIndex))
                    {
                        for (int i = 0; i < fileRow.Length; i++)
                        {
                            table.Columns.Add((string.IsNullOrWhiteSpace(fileRow[i]) ? $"Column {i}" : fileRow[i]), typeof(string));
                        }

                        continue;
                    }

                    DataRow newTableRow = table.NewRow();
                    for (int i = 0; i < fileRow.Length; i++)
                    {
                        try
                        {
                            newTableRow[i] = ReflectionUtils.ConvertValueIfRequired(fileRow[0], typeof(string));
                        }
                        catch
                        {
                            newTableRow[i] = fileRow[i]?.ToString();
                        }
                    }
                    table.Rows.Add(newTableRow);
                }
            }

            return table;
        }

        /// <summary>
        /// Read data from the provided <paramref name="stream"> into a <see cref="DataTable"/>
        /// </summary>
        /// <param name="stream">Stream to open the reader on</param>
        /// <param name="hasHeaderRow">Set to TRUE if the stream has a header row</param>
        /// <param name="headerRowIndex">If <paramref name="hasHeaderRow"/> is TRUE, this should contain a 1-based index [starting from current stream-pos] of the row where the header row lives.</param>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <param name="encoding">Specific encoding. NULL to auto-detect</param>
        /// <param name="autoDetectEncoding">If set, auto-detects</param>
        /// <param name="bufferSize">Minimum stream buffer size</param>
        /// <param name="leaveStreamOpen">Set to dispose the stream when this object is disposed</param>
        /// <returns>Data from the stream as a DataTable</returns>
        public static DataTable ReadToTable(Stream stream, bool hasHeaderRow = true, ulong headerRowIndex = 1, string tableName = "Table 1", Encoding? encoding = default, bool autoDetectEncoding = true, int bufferSize = -1, bool leaveStreamOpen = false)
        {
            using TokenLimitedFileReader reader = new TokenLimitedFileReader(stream, encoding, autoDetectEncoding, bufferSize, leaveStreamOpen);
            return ReadToTable(reader, tableName, hasHeaderRow, headerRowIndex);
        }

        /// <summary>
        /// Read data from the provided <paramref name="stream"> into a <see cref="DataTable"/>
        /// </summary>
        /// <param name="path">File path to open the reader on</param>
        /// <param name="hasHeaderRow">Set to TRUE if the stream has a header row</param>
        /// <param name="headerRowIndex">If <paramref name="hasHeaderRow"/> is TRUE, this should contain a 1-based index [starting from current stream-pos] of the row where the header row lives.</param>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <param name="encoding">Specific encoding. NULL to auto-detect</param>
        /// <param name="autoDetectEncoding">If set, auto-detects</param>
        /// <param name="leaveStreamOpen">Set to dispose the stream when this object is disposed</param>
        /// <returns>Data from the stream as a DataTable</returns>
        public static DataTable ReadToTable(string path, bool hasHeaderRow = true, ulong headerRowIndex = 1, string tableName = "Table 1", Encoding? encoding = default, bool autoDetectEncoding = true, bool leaveStreamOpen = false)
        {
            using TokenLimitedFileReader reader = new TokenLimitedFileReader(path, encoding, autoDetectEncoding, leaveStreamOpen);
            return ReadToTable(reader, tableName, hasHeaderRow, headerRowIndex);
        }

        /// <summary>
        /// Read data from the provided <paramref name="stream"> into a <see cref="DataTable"/>
        /// </summary>
        /// <param name="stream">Stream to open the reader on</param>
        /// <param name="hasHeaderRow">Set to TRUE if the stream has a header row</param>
        /// <param name="headerRowIndex">If <paramref name="hasHeaderRow"/> is TRUE, this should contain a 1-based index [starting from current stream-pos] of the row where the header row lives.</param>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <param name="encoding">Specific encoding. NULL to auto-detect</param>
        /// <param name="autoDetectEncoding">If set, auto-detects</param>
        /// <param name="bufferSize">Minimum stream buffer size</param>
        /// <param name="leaveStreamOpen">Set to dispose the stream when this object is disposed</param>
        /// <returns>Data from the stream as a DataTable</returns>
        public static async Task<DataTable> ReadToTableAsync(Stream stream, bool hasHeaderRow = true, ulong headerRowIndex = 1, string tableName = "Table 1", Encoding? encoding = default, bool autoDetectEncoding = true, int bufferSize = -1, bool leaveStreamOpen = false)
        {
            using TokenLimitedFileReader reader = new TokenLimitedFileReader(stream, encoding, autoDetectEncoding, bufferSize, leaveStreamOpen);
            return await ReadToTableAsync(reader, tableName, hasHeaderRow, headerRowIndex);
        }

        /// <summary>
        /// Read data from the provided <paramref name="stream"> into a <see cref="DataTable"/>
        /// </summary>
        /// <param name="path">File path to open the reader on</param>
        /// <param name="hasHeaderRow">Set to TRUE if the stream has a header row</param>
        /// <param name="headerRowIndex">If <paramref name="hasHeaderRow"/> is TRUE, this should contain a 1-based index [starting from current stream-pos] of the row where the header row lives.</param>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <param name="encoding">Specific encoding. NULL to auto-detect</param>
        /// <param name="autoDetectEncoding">If set, auto-detects</param>
        /// <param name="leaveStreamOpen">Set to dispose the stream when this object is disposed</param>
        /// <returns>Data from the stream as a DataTable</returns>
        public static async Task<DataTable> ReadToTableAsync(string path, bool hasHeaderRow = true, ulong headerRowIndex = 1, string tableName = "Table 1", Encoding? encoding = default, bool autoDetectEncoding = true, bool leaveStreamOpen = false)
        {
            using TokenLimitedFileReader reader = new TokenLimitedFileReader(path, encoding, autoDetectEncoding, leaveStreamOpen);
            return await ReadToTableAsync(reader, tableName, hasHeaderRow, headerRowIndex);
        }

        /// <summary>
        /// Get the data from a stream as an IEnumerable[T]
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="stream">Stream to open the reader on</param>
        /// <param name="hasHeaderRow">Set to TRUE if the stream has a header row</param>
        /// <param name="headerRowIndex">If <paramref name="hasHeaderRow"/> is TRUE, this should contain a 1-based index [starting from current stream-pos] of the row where the header row lives.</param>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <param name="encoding">Specific encoding. NULL to auto-detect</param>
        /// <param name="autoDetectEncoding">If set, auto-detects</param>
        /// <param name="bufferSize">Minimum stream buffer size</param>
        /// <param name="leaveStreamOpen">Set to dispose the stream when this object is disposed</param>
        /// <param name="action">An action to perform on the DataTable before conversion to list</param>
        /// <returns>Data from the stream as an IEnumerable[T]</returns>
        public static IEnumerable<TObject> ReadToEnumerable<TObject>(Stream stream, bool hasHeaderRow = true, ulong headerRowIndex = 1, string tableName = "Table 1", Encoding? encoding = default, bool autoDetectEncoding = true, int bufferSize = -1, bool leaveStreamOpen = false, Action<DataTable>? action = null)
            where TObject : class, new()
        {
            DataTable table = ReadToTable(stream, hasHeaderRow, headerRowIndex, tableName, encoding, autoDetectEncoding, bufferSize, leaveStreamOpen);
            action?.Invoke(table);
            return TokenLimitedFileSerialiser.Transform<TObject>(table);
        }

        /// <summary>
        /// Get the data from a stream as an IEnumerable[T]
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="path">Path to file (absolute preferred)</param>
        /// <param name="hasHeaderRow">Set to TRUE if the stream has a header row</param>
        /// <param name="headerRowIndex">If <paramref name="hasHeaderRow"/> is TRUE, this should contain a 1-based index [starting from current stream-pos] of the row where the header row lives.</param>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <param name="encoding">Specific encoding. NULL for autodetect</param>
        /// <param name="autoDetectEncoding">If set, auto-detects</param>
        /// <param name="leaveStreamOpen">Set to dispose the stream when this object is disposed</param>
        /// <param name="action">An action to perform on the DataTable before conversion to list</param>
        /// <returns>Data from the stream as an IEnumerable[T]</returns>
        public static IEnumerable<TObject> ReadToEnumerable<TObject>(string path, bool hasHeaderRow = true, ulong headerRowIndex = 1, string tableName = "Table 1", Encoding? encoding = default, bool autoDetectEncoding = true, bool leaveStreamOpen = false, Action<DataTable>? action = null)
            where TObject : class, new()
        {
            DataTable table = ReadToTable(path, hasHeaderRow, headerRowIndex, tableName, encoding, autoDetectEncoding, leaveStreamOpen);
            action?.Invoke(table);
            return TokenLimitedFileSerialiser.Transform<TObject>(table);
        }
    }
}