using SujaySarma.Data.Core;

using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SujaySarma.Data.Files.TokenLimitedFiles
{
    // Synchronous writers
    public sealed partial class TokenLimitedFileWriter
    {

        #region Static Methods

        /// <summary>
        /// Write a single row for a single-item object
        /// </summary>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter" /></param>
        /// <param name="row">A <see cref="DataRow" /> containing the data to be written out</param>
        public static void WriteRow(TokenLimitedFileWriter writer, DataRow row)
        {
            StringBuilder stringBuilder = BuildRowForDataRow(writer._options.Delimiter, writer._options.ForceQuoteStrings, row.ItemArray);
            writer.Write(stringBuilder.ToString());
        }

        /// <summary>
        /// Write a single row for a single-item object
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter" /></param>
        /// <param name="instance">Instance of an object item with data</param>
        public static void WriteRow<TObject>(TokenLimitedFileWriter writer, TObject instance)
        {
            StringBuilder stringBuilder = BuildRowForDataRow(writer._options.Delimiter, writer._options.ForceQuoteStrings, instance);
            writer.Write(stringBuilder.ToString());
        }

        /// <summary>
        /// Writes the header row for the provided object type
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter" /></param>
        public static void WriteHeader<TObject>(TokenLimitedFileWriter writer)
        {
            StringBuilder stringBuilder = BuildRowForColumnHeaders<TObject>(writer._options.Delimiter, writer._options.ForceQuoteStrings);
            writer.Write(stringBuilder.ToString());
        }

        /// <summary>
        /// Write the header row for the provided table
        /// </summary>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter" /></param>
        /// <param name="table">A <see cref="DataTable" /> containing column information</param>
        public static void WriteHeader(TokenLimitedFileWriter writer, DataTable table)
        {
            StringBuilder stringBuilder = BuildRowForColumnHeaders(table, writer._options.Delimiter, writer._options.ForceQuoteStrings);
            writer.Write(stringBuilder.ToString());
        }

        /// <summary>
        /// Write an entire table to the flatfile
        /// </summary>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter" /></param>
        /// <param name="table">A <see cref="DataTable" /> containing the column-header information and data rows to write</param>
        public static void WriteTable(TokenLimitedFileWriter writer, DataTable table)
        {
            if (writer._options.HasHeaderRow)
            {
                WriteHeader(writer, table);
            }

            foreach (DataRow row in table.Rows)
            {
                WriteRow(writer, row);
            }
        }

        /// <summary>
        /// Write an entire object to the flatfile
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter" /></param>
        /// <param name="instance">Instance of a <typeparamref name="TObject" /></param>
        public static void WriteObject<TObject>(TokenLimitedFileWriter writer, TObject instance)
        {
            if (writer._options.HasHeaderRow)
            {
                WriteHeader<TObject>(writer);
            }

            WriteRow<TObject>(writer, instance);
        }

        /// <summary>
        /// Write an entire object to the flatfile
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter" /></param>
        /// <param name="instances">Instances of a <typeparamref name="TObject" /></param>
        public static void WriteObjects<TObject>(TokenLimitedFileWriter writer, params TObject[] instances)
            // IEnumerable cast is needed to avoid ambiguity with params overload
            => WriteObjects<TObject>(writer, (IEnumerable<TObject>)instances);

        /// <summary>
        /// Write an entire object to the flatfile
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter" /></param>
        /// <param name="instances">Instances of a <typeparamref name="TObject" /></param>
        public static void WriteObjects<TObject>(TokenLimitedFileWriter writer, IEnumerable<TObject> instances)
        {
            if (writer._options.HasHeaderRow)
            {
                WriteHeader<TObject>(writer);
            }

            foreach (TObject instance in instances)
            {
                WriteRow<TObject>(writer, instance);
            }
        }

        #endregion

        #region Instance Methods

        /// <summary>Write a complete row</summary>
        /// <param name="row">A single row of data</param>
        public void Write(string?[]? row)
        {
            this.ThrowIfDisposed(isDisposed, nameof(TokenLimitedFileWriter));

            if (row == null)
            {
                WriteNewLine();
                return;
            }

            for (int index = 0; index < row.Length; ++index)
            {
                string? rowField = row[index];
                if (rowField != null)
                {
                    if (rowField.Contains(' '))
                    {
                        // Ensure quoted correctly
                        _writer.Write(
                                $"{((rowField[0] == QUOTE) ? string.Empty : QUOTE)}{rowField}{((rowField[^1] == QUOTE) ? string.Empty : QUOTE)}"
                            );
                    }
                    else
                    {
                        _writer.Write(rowField);
                    }
                }

                if (index < row.Length - 1)
                {
                    _writer.Write(Delimiter);
                }
            }

            WriteNewLine();
            ++ROWS_WRITTEN;
        }

        /// <summary>Write a complete row</summary>
        /// <param name="row">A string of arbitrary information</param>
        public void Write(string? row)
        {
            this.ThrowIfDisposed(isDisposed, nameof(TokenLimitedFileWriter));

            if (row == null)
            {
                WriteNewLine();
                return;
            }

            _writer.Write(row);
            WriteNewLine();
            ++ROWS_WRITTEN;

        }

        /// <summary>Write a newline to the stream</summary>
        public void WriteNewLine()
            => _writer.WriteLine();

        #endregion
    }
}
