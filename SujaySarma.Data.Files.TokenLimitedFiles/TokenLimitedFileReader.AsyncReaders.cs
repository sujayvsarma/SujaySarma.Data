using SujaySarma.Data.Core;
using SujaySarma.Data.Files.TokenLimitedFiles.Serialisation;

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace SujaySarma.Data.Files.TokenLimitedFiles
{
    // Asynchronous reader methods
    public sealed partial class TokenLimitedFileReader
    {

        /// <summary>
        /// Read and return a complete row.
        /// </summary>
        /// <returns>The row that was read, as an array of component fields. If no rows were read, it will be NULL.</returns>
        /// <remarks>
        ///     Each row of a token-limited flat-file may contain an irregular number of elements. Thus, each call to
        ///     this function may return a different length of fields. HOWEVER, we will always return at least the
        ///     number of elements as the previous read.
        /// 
        ///     An "empty" field (or a field that has no value with just two consecutive delimiters (eg: ",,") is returned
        ///     as a string.Empty and not as a NULL. A NULL is only returned if that field does not exist on the row.
        /// </remarks>
        public async Task<string?[]?> ReadRowAsync()
        {
            this.ThrowIfDisposed(isDisposed, nameof(TokenLimitedFileReader));
            if (EndOfStream)
            {
                return null;
            }

            _state.ResetFieldsForNewRow();
            char[] buffer = new char[1];

            while (! _reader.EndOfStream)
            {
                await _reader.ReadAsync(buffer, 0, 1);
                _state.CurrentCharacter = GetValidCharacterFromResult(buffer[0]);
                if (_state.CurrentCharacter != char.MinValue)
                {
                    _state.NextCharacter = GetValidCharacterFromResult(_reader.Peek());
                    if (_state.NextCharacter != char.MinValue)
                    {
                        ProcessCharacterResult nextOp = ProcessCharacter();
                        if (nextOp == ProcessCharacterResult.SkipNextCharacter)
                        {
                            // We cannot throw away the buffer in ReadAsync, so let this be sync!
                            _reader.Read();
                        }
                        else if (nextOp == ProcessCharacterResult.FinishRow)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            FlushFieldBufferToRow();
            if (_state.ThisRowFieldsRead <= 0)
            {
                return null;
            }

            _state.SizeRowBufferForMaximumFieldsRead();
            ++ROWS_READ;
            return _state.RowBuffer;
        }

        /// <summary>
        /// Read content from the <paramref name="reader" /> and return it as a <see cref="DataTable" />.
        /// </summary>
        /// <param name="reader">An initialised instance of a <see cref="TokenLimitedFileReader" /></param>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <returns>A DataTable containing zero or more rows of data, potentially with column definitions</returns>
        public static async Task<DataTable> ReadToTableAsync(TokenLimitedFileReader reader, string tableName = "Table 1")
        {
            DataTable table = new DataTable(tableName);
            while (!reader.EndOfStream)
            {
                ProcessReadRow(await reader.ReadRowAsync(), table, reader._options, reader.RowCount);
            }

            return table;
        }

        /// <summary>
        /// Read data from the provided <paramref name="stream" /> into a <see cref="DataTable" />
        /// </summary>
        /// <param name="stream">Stream to open the reader on</param>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <param name="options">Options for the reader</param>
        /// <returns>Data from the stream as a DataTable</returns>
        public static async Task<DataTable> ReadToTableAsync(Stream stream, TokenLimitedFileOptions options, string tableName = "Table 1")
        {
            DataTable tableAsync;
            using (TokenLimitedFileReader reader = new TokenLimitedFileReader(stream, options))
            {
                tableAsync = await ReadToTableAsync(reader, tableName);
            }

            return tableAsync;
        }

        /// <summary>
        /// Read data from the provided <paramref name="path" /> into a <see cref="DataTable" />
        /// </summary>
        /// <param name="path">File path to open the reader on</param>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <param name="options">Options for the reader</param>
        /// <returns>Data from the path as a DataTable</returns>
        public static async Task<DataTable> ReadToTableAsync(string path, TokenLimitedFileOptions options, string tableName = "Table 1")
        {
            DataTable tableAsync;
            using (TokenLimitedFileReader reader = new TokenLimitedFileReader(path, options))
            {
                tableAsync = await ReadToTableAsync(reader, tableName);
            }

            return tableAsync;
        }

        /// <summary>Get the data from a stream as an IEnumerable[T]</summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="stream">Stream to open the reader on</param>
        /// <param name="options">Options for the reader</param>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <param name="action">An action to perform on the DataTable before conversion to list</param>
        /// <returns>Data from the stream as an IEnumerable[T]</returns>
        public static async Task<IEnumerable<TObject>> ReadToEnumerableAsync<TObject>(Stream stream, TokenLimitedFileOptions options, string tableName = "Table 1", Action<DataTable>? action = null)
        {
            DataTable tableAsync = await ReadToTableAsync(stream, options, tableName);
            if (action != null)
            {
                action(tableAsync);
            }

            return TokenLimitedFileSerialiser.Transform<TObject>(tableAsync);
        }

        /// <summary>Get the data from a stream as an IEnumerable[T]</summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="path">Path to file (absolute preferred)</param>
        /// <param name="options">Options for the reader</param>
        /// <param name="tableName">Name of the table to set on the returned DataTable</param>
        /// <param name="action">An action to perform on the DataTable before conversion to list</param>
        /// <returns>Data from the stream as an IEnumerable[T]</returns>
        public static async Task<IEnumerable<TObject>> ReadToEnumerableAsync<TObject>(string path, TokenLimitedFileOptions options, string tableName = "Table 1", Action<DataTable>? action = null)
        {
            DataTable tableAsync = await ReadToTableAsync(path, options, tableName);
            if (action != null)
            {
                action(tableAsync);
            }

            return TokenLimitedFileSerialiser.Transform<TObject>(tableAsync);
        }



    }
}
