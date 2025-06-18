using SujaySarma.Data.Core;

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace SujaySarma.Data.Files.TokenLimitedFiles
{
    // Asynchronous writers
    public sealed partial class TokenLimitedFileWriter
    {

        #region Static Methods

        /// <summary>
        /// Write a single row for a single-item object
        /// </summary>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter" /></param>
        /// <param name="row">A <see cref="DataRow" /> containing the data to be written out</param>
        public static async Task WriteRowAsync(TokenLimitedFileWriter writer, DataRow row)
            => await writer.WriteAsync(BuildRowForDataRow(writer._options.Delimiter, writer._options.ForceQuoteStrings, row.ItemArray).ToString());

        /// <summary>
        /// Write a single row for a single-item object
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter" /></param>
        /// <param name="instance">Instance of an object item with data</param>
        public static async Task WriteRowAsync<TObject>(TokenLimitedFileWriter writer, TObject instance)
            => await writer.WriteAsync(BuildRowForDataRow(writer._options.Delimiter, writer._options.ForceQuoteStrings, new TObject[] { instance }).ToString());

        /// <summary>
        /// Writes the header row for the provided object type
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter" /></param>
        public static async Task WriteHeaderAsync<TObject>(TokenLimitedFileWriter writer)
            => await writer.WriteAsync(BuildRowForColumnHeaders<TObject>(writer._options.Delimiter, writer._options.ForceQuoteStrings).ToString());

        /// <summary>
        /// Write the header row for the provided table
        /// </summary>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter" /></param>
        /// <param name="table">A <see cref="DataTable" /> containing column information</param>
        public static async Task WriteHeaderAsync(TokenLimitedFileWriter writer, DataTable table)
            => await writer.WriteAsync(BuildRowForColumnHeaders(table, writer._options.Delimiter, writer._options.ForceQuoteStrings).ToString());

        /// <summary>
        /// Write an entire table to the flatfile
        /// </summary>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter" /></param>
        /// <param name="table">A <see cref="DataTable" /> containing the column-header information and data rows to write</param>
        public static async Task WriteTableAsync(TokenLimitedFileWriter writer, DataTable table)
        {
            if (writer._options.HasHeaderRow)
            {
                await WriteHeaderAsync(writer, table);
            }

            foreach (DataRow row in table.Rows)
            {
                await WriteRowAsync(writer, row);
            }
        }

        /// <summary>
        /// Write an entire object to the flatfile
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter" /></param>
        /// <param name="instance">Instance of a <typeparamref name="TObject" /></param>
        public static async Task WriteObjectAsync<TObject>(TokenLimitedFileWriter writer, TObject instance)
        {
            if (writer._options.HasHeaderRow)
            {
                await WriteHeaderAsync<TObject>(writer);
            }

            await WriteRowAsync<TObject>(writer, instance);
        }

        /// <summary>
        /// Write an entire object to the flatfile
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter" /></param>
        /// <param name="instances">Instances of a <typeparamref name="TObject" /></param>
        public static async Task WriteObjectsAsync<TObject>(TokenLimitedFileWriter writer, params TObject[] instances)
            => await WriteObjectsAsync<TObject>(writer, (IEnumerable<TObject>)instances);

        /// <summary>
        /// Write an entire object to the flatfile
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter" /></param>
        /// <param name="instances">Instances of a <typeparamref name="TObject" /></param>
        public static async Task WriteObjectsAsync<TObject>(TokenLimitedFileWriter writer, IEnumerable<TObject> instances)
        {
            if (writer._options.HasHeaderRow)
            {
                await WriteHeaderAsync<TObject>(writer);
            }

            foreach (TObject instance in instances)
            {
                await WriteRowAsync<TObject>(writer, instance);
            }
        }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Write a complete row
        /// </summary>
        /// <param name="row">A single row of data</param>
        public async Task WriteAsync(string?[]? row)
        {
            this.ThrowIfDisposed(isDisposed, nameof(TokenLimitedFileWriter));

            if (row == null)
            {
                await WriteNewLineAsync();
                return;
            }

            for (int h = 0; h < row.Length; ++h)
            {
                string? element = row[h];
                if (element != null)
                {
                    if (element.Contains(' '))
                    {
                        await _writer.WriteAsync(
                            $"{((element[0] != QUOTE) ? QUOTE.ToString() : string.Empty)}{element}{((element[^1] != QUOTE) ? QUOTE.ToString() : string.Empty)}"
                        );
                    }
                    else
                    {
                        await _writer.WriteAsync(element);
                    }
                }
                if (h < (row.Length - 1))
                {
                    await _writer.WriteAsync(Delimiter);
                }
            }

            await WriteNewLineAsync();
            ++ROWS_WRITTEN;

        }

        /// <summary>
        /// Write a complete row
        /// </summary>
        /// <param name="row">A string of arbitrary information</param>
        public async Task WriteAsync(string? row)
        {
            this.ThrowIfDisposed(isDisposed, nameof(TokenLimitedFileWriter));

            if (row == null)
            {
                await WriteNewLineAsync();
                return;
            }

            await _writer.WriteAsync(row);
            await WriteNewLineAsync();
            ++ROWS_WRITTEN;
        }

        /// <summary>
        /// Write a newline to the stream
        /// </summary>
        public async Task WriteNewLineAsync() 
            => await _writer.WriteLineAsync();

        #endregion

    }
}
