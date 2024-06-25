using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
        /// <returns>A DataTable containing zero or more rows of data, potentially with column definitions</returns>
        public static DataTable ReadToTable(TokenLimitedFileReader reader, string tableName = "Table 1")
        {
            DataTable table = new DataTable(tableName);
            while (!reader.EndOfStream)
            {
                string?[]? fileRow = reader.ReadRow();

                ProcessReadRow(fileRow, table, reader._options, reader.RowCount);
            }

            return table;
        }

        /// <summary>
        /// Read content from the <paramref name="reader"/> and return it as a <see cref="DataTable">.
        /// </summary>
        /// <param name="reader">An initialised instance of a <see cref="TokenLimitedFileReader"/></param>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <returns>A DataTable containing zero or more rows of data, potentially with column definitions</returns>
        public static async Task<DataTable> ReadToTableAsync(TokenLimitedFileReader reader, string tableName = "Table 1")
        {
            DataTable table = new DataTable(tableName);
            while (!reader.EndOfStream)
            {
                string?[]? fileRow = await reader.ReadRowAsync();

                ProcessReadRow(fileRow, table, reader._options, reader.RowCount);
            }

            return table;
        }

        /// <summary>
        /// Process the row that was read and populate the table with its data
        /// </summary>
        /// <param name="fileRow">String array read from file</param>
        /// <param name="table">DataTable instance to populate with the row or header information</param>
        /// <param name="options">Copy of the <see cref="TokenLimitedFileOptions"/> as being used by the active <see cref="TokenLimitedFileReader"/></param>
        /// <param name="currentRowCount">Number of rows ALREADY processed</param>
        private static void ProcessReadRow(string?[]? fileRow, DataTable table, TokenLimitedFileOptions options, ulong currentRowCount)
        {
            if (fileRow != null)
            {
                if (options.HasHeaderRow && (currentRowCount == options.HeaderRowIndex))
                {
                    for (int i = 0; i < fileRow.Length; i++)
                    {
                        table.Columns.Add((string.IsNullOrWhiteSpace(fileRow[i]) ? $"Column {i}" : fileRow[i]), typeof(string));
                    }
                }
                else
                {
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
        }

        /// <summary>
        /// Read data from the provided <paramref name="stream"> into a <see cref="DataTable"/>
        /// </summary>
        /// <param name="stream">Stream to open the reader on</param>
        /// <param name="options">Options for the reader</param>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <returns>Data from the stream as a DataTable</returns>
        public static DataTable ReadToTable(Stream stream, TokenLimitedFileOptions options, string tableName = "Table 1")
        {
            using TokenLimitedFileReader reader = new TokenLimitedFileReader(stream, options);
            return ReadToTable(reader, tableName);
        }

        /// <summary>
        /// Read data from the provided <paramref name="stream"> into a <see cref="DataTable"/>
        /// </summary>
        /// <param name="path">File path to open the reader on</param>
        /// <param name="options">Options for the reader</param>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <returns>Data from the stream as a DataTable</returns>
        public static DataTable ReadToTable(string path, TokenLimitedFileOptions options, string tableName = "Table 1")
        {
            using TokenLimitedFileReader reader = new TokenLimitedFileReader(path, options);
            return ReadToTable(reader, tableName);
        }

        /// <summary>
        /// Read data from the provided <paramref name="stream"> into a <see cref="DataTable"/>
        /// </summary>
        /// <param name="stream">Stream to open the reader on</param>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <param name="options">Options for the reader</param>
        /// <returns>Data from the stream as a DataTable</returns>
        public static async Task<DataTable> ReadToTableAsync(Stream stream, TokenLimitedFileOptions options, string tableName = "Table 1")
        {
            using TokenLimitedFileReader reader = new TokenLimitedFileReader(stream, options);
            return await ReadToTableAsync(reader, tableName);
        }

        /// <summary>
        /// Read data from the provided <paramref name="stream"> into a <see cref="DataTable"/>
        /// </summary>
        /// <param name="path">File path to open the reader on</param>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <param name="options">Options for the reader</param>
        /// <returns>Data from the stream as a DataTable</returns>
        public static async Task<DataTable> ReadToTableAsync(string path, TokenLimitedFileOptions options, string tableName = "Table 1")
        {
            using TokenLimitedFileReader reader = new TokenLimitedFileReader(path, options);
            return await ReadToTableAsync(reader, tableName);
        }

        /// <summary>
        /// Get the data from a stream as an IEnumerable[T]
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="stream">Stream to open the reader on</param>
        /// <param name="options">Options for the reader</param>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <param name="action">An action to perform on the DataTable before conversion to list</param>
        /// <returns>Data from the stream as an IEnumerable[T]</returns>
        public static IEnumerable<TObject> ReadToEnumerable<TObject>(Stream stream, TokenLimitedFileOptions options, string tableName = "Table 1", Action<DataTable>? action = null)
        {
            DataTable table = ReadToTable(stream, options, tableName);
            action?.Invoke(table);

            return TokenLimitedFileSerialiser.Transform<TObject>(table);
        }

        /// <summary>
        /// Get the data from a stream as an IEnumerable[T]
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="path">Path to file (absolute preferred)</param>
        /// <param name="options">Options for the reader</param>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <param name="action">An action to perform on the DataTable before conversion to list</param>
        /// <returns>Data from the stream as an IEnumerable[T]</returns>
        public static IEnumerable<TObject> ReadToEnumerable<TObject>(string path, TokenLimitedFileOptions options, string tableName = "Table 1", Action<DataTable>? action = null)
        {
            DataTable table = ReadToTable(path, options, tableName);
            action?.Invoke(table);

            return TokenLimitedFileSerialiser.Transform<TObject>(table);
        }

        /// <summary>
        /// Get the data from a stream as an IEnumerable[T]
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="stream">Stream to open the reader on</param>
        /// <param name="options">Options for the reader</param>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <param name="action">An action to perform on the DataTable before conversion to list</param>
        /// <returns>Data from the stream as an IEnumerable[T]</returns>
        public static async Task<IEnumerable<TObject>> ReadToEnumerableAsync<TObject>(Stream stream, TokenLimitedFileOptions options, string tableName = "Table 1", Action<DataTable>? action = null)
        {
            DataTable table = await ReadToTableAsync(stream, options, tableName);
            action?.Invoke(table);

            return TokenLimitedFileSerialiser.Transform<TObject>(table);
        }

        /// <summary>
        /// Get the data from a stream as an IEnumerable[T]
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="path">Path to file (absolute preferred)</param>
        /// <param name="options">Options for the reader</param>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <param name="action">An action to perform on the DataTable before conversion to list</param>
        /// <returns>Data from the stream as an IEnumerable[T]</returns>
        public static async Task<IEnumerable<TObject>> ReadToEnumerableAsync<TObject>(string path, TokenLimitedFileOptions options, string tableName = "Table 1", Action<DataTable>? action = null)
        {
            DataTable table = await ReadToTableAsync(path, options, tableName);
            action?.Invoke(table);

            return TokenLimitedFileSerialiser.Transform<TObject>(table);
        }
    }
}