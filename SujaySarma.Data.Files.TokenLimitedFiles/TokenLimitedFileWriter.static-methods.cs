using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SujaySarma.Data.Core.Reflection;
using SujaySarma.Data.Files.TokenLimitedFiles.Attributes;

namespace SujaySarma.Data.Files.TokenLimitedFiles
{
    /*
        Implementations of static methods
    */
    public sealed partial class TokenLimitedFileWriter
    {

        /// <summary>
        /// Write a single row for a single-item object
        /// </summary>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter"/></param>
        /// <param name="row">A <see cref="DataRow"/> containing the data to be written out</param>
        public static void WriteRow(TokenLimitedFileWriter writer, DataRow row)
        {
            StringBuilder dataRow = BuildRowForDataRow(writer._options.Delimiter, writer._options.ForceQuoteStrings, row.ItemArray);
            writer.Write(dataRow.ToString());
        }

        /// <summary>
        /// Write a single row for a single-item object
        /// </summary>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter"/></param>
        /// <param name="row">A <see cref="DataRow"/> containing the data to be written out</param>
        public static async Task WriteRowAsync(TokenLimitedFileWriter writer, DataRow row)
        {
            StringBuilder dataRow = BuildRowForDataRow(writer._options.Delimiter, writer._options.ForceQuoteStrings, row.ItemArray);
            await writer.WriteAsync(dataRow.ToString());
        }

        /// <summary>
        /// Write a single row for a single-item object
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter"/></param>
        /// <param name="instance">Instance of an object item with data</param>
        public static void WriteRow<TObject>(TokenLimitedFileWriter writer, TObject instance)
        {
            StringBuilder dataRow = BuildRowForDataRow(writer._options.Delimiter, writer._options.ForceQuoteStrings, instance);
            writer.Write(dataRow.ToString());
        }

        /// <summary>
        /// Write a single row for a single-item object
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter"/></param>
        /// <param name="instance">Instance of an object item with data</param>
        public static async Task WriteRowAsync<TObject>(TokenLimitedFileWriter writer, TObject instance)
        {
            StringBuilder dataRow = BuildRowForDataRow(writer._options.Delimiter, writer._options.ForceQuoteStrings, instance);
            await writer.WriteAsync(dataRow.ToString());
        }

        /// <summary>
        /// Writes the header row for the provided object type
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter"/></param>
        public static void WriteHeader<TObject>(TokenLimitedFileWriter writer)
        {
            StringBuilder headerRow = BuildRowForColumnHeaders<TObject>(writer._options.Delimiter, writer._options.ForceQuoteStrings);
            writer.Write(headerRow.ToString());
        }

        /// <summary>
        /// Writes the header row for the provided object type
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter"/></param>
        public static async Task WriteHeaderAsync<TObject>(TokenLimitedFileWriter writer)
        {
            StringBuilder headerRow = BuildRowForColumnHeaders<TObject>(writer._options.Delimiter, writer._options.ForceQuoteStrings);
            await writer.WriteAsync(headerRow.ToString());
        }

        /// <summary>
        /// Write the header row for the provided table
        /// </summary>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter"/></param>
        /// <param name="table">A <see cref="DataTable"/> containing column information</param>
        public static void WriteHeader(TokenLimitedFileWriter writer, DataTable table)
        {
            StringBuilder headerRow = BuildRowForColumnHeaders(table, writer._options.Delimiter, writer._options.ForceQuoteStrings);
            writer.Write(headerRow.ToString());
        }

        /// <summary>
        /// Write the header row for the provided table
        /// </summary>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter"/></param>
        /// <param name="table">A <see cref="DataTable"/> containing column information</param>
        public static async Task WriteHeaderAsync(TokenLimitedFileWriter writer, DataTable table)
        {
            StringBuilder headerRow = BuildRowForColumnHeaders(table, writer._options.Delimiter, writer._options.ForceQuoteStrings);
            await writer.WriteAsync(headerRow.ToString());
        }



        /// <summary>
        /// Write an entire table to the flatfile
        /// </summary>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter"/></param>
        /// <param name="table">A <see cref="DataTable"/> containing the column-header information and data rows to write</param>
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
        /// Write an entire table to the flatfile
        /// </summary>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter"/></param>
        /// <param name="table">A <see cref="DataTable"/> containing the column-header information and data rows to write</param>
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
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter"/></param>
        /// <param name="instance">Instance of a <typeparamref name="TObject"/></param>
        public static void WriteObject<TObject>(TokenLimitedFileWriter writer, TObject instance)
        {
            if (writer._options.HasHeaderRow)
            {
                WriteHeader<TObject>(writer);
            }

            WriteRow(writer, instance);
        }

        /// <summary>
        /// Write an entire object to the flatfile
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter"/></param>
        /// <param name="instance">Instance of a <typeparamref name="TObject"/></param>
        public static async Task WriteObjectAsync<TObject>(TokenLimitedFileWriter writer, TObject instance)
        {
            if (writer._options.HasHeaderRow)
            {
                await WriteHeaderAsync<TObject>(writer);
            }

            await WriteRowAsync(writer, instance);
        }

        /// <summary>
        /// Write an entire object to the flatfile
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter"/></param>
        /// <param name="instances">Instances of a <typeparamref name="TObject"/></param>
        public static void WriteObjects<TObject>(TokenLimitedFileWriter writer, params TObject[] instances)
            => WriteObjects<TObject>(writer, instances.ToList());

        /// <summary>
        /// Write an entire object to the flatfile
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter"/></param>
        /// <param name="instances">Instances of a <typeparamref name="TObject"/></param>
        public static void WriteObjects<TObject>(TokenLimitedFileWriter writer, IEnumerable<TObject> instances)
        {
            if (writer._options.HasHeaderRow)
            {
                WriteHeader<TObject>(writer);
            }

            foreach (TObject instance in instances)
            {
                WriteRow(writer, instance);
            }
        }

        /// <summary>
        /// Write an entire object to the flatfile
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter"/></param>
        /// <param name="instances">Instances of a <typeparamref name="TObject"/></param>
        public static async Task WriteObjectsAsync<TObject>(TokenLimitedFileWriter writer, params TObject[] instances)
            => await WriteObjectsAsync<TObject>(writer, instances.ToList());

        /// <summary>
        /// Write an entire object to the flatfile
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="writer">Instance of a <see cref="TokenLimitedFileWriter"/></param>
        /// <param name="instances">Instances of a <typeparamref name="TObject"/></param>
        public static async Task WriteObjectsAsync<TObject>(TokenLimitedFileWriter writer, IEnumerable<TObject> instances)
        {
            if (writer._options.HasHeaderRow)
            {
                await WriteHeaderAsync<TObject>(writer);
            }

            foreach (TObject instance in instances)
            {
                await WriteRowAsync(writer, instance);
            }
        }


        /// <summary>
        /// Build a complete row of column names (header row)
        /// </summary>
        /// <param name="table">A <see cref="DataTable"/> containing the column definitions</param>
        /// <param name="delimiter">The delimiter character to be used</param>
        /// <param name="mustQuote">The option to force individual fields to be quoted. When false, only quotes if the <paramref name="delimiter"/> is a part of field-string</param>
        /// <returns>A <see cref="StringBuilder"/> instance with the column-header row</returns>
        private static StringBuilder BuildRowForColumnHeaders(DataTable table, char delimiter, bool mustQuote)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(delimiter);
                }

                if (table.Columns[i].ColumnName.Contains(delimiter) || mustQuote)
                {
                    builder.Append($"\"{table.Columns[i].ColumnName}\"");
                }
                else
                {
                    builder.Append(table.Columns[i].ColumnName);
                }
            }

            return builder;
        }

        /// <summary>
        /// Build a complete row of column names (header row)
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="delimiter">The delimiter character to be used</param>
        /// <param name="mustQuote">The option to force individual fields to be quoted. When false, only quotes if the <paramref name="delimiter"/> is a part of field-string</param>
        /// <returns>A <see cref="StringBuilder"/> instance with the column-header row</returns>
        private static StringBuilder BuildRowForColumnHeaders<TObject>(char delimiter, bool mustQuote)
            => BuildRowForColumnHeaders(typeof(TObject), delimiter, mustQuote);

        /// <summary>
        /// Build a complete row of column names (header row)
        /// </summary>
        /// <param name="type">Type of .NET class, structure or record</param>
        /// <param name="delimiter">The delimiter character to be used</param>
        /// <param name="mustQuote">The option to force individual fields to be quoted. When false, only quotes if the <paramref name="delimiter"/> is a part of field-string</param>
        /// <returns>A <see cref="StringBuilder"/> instance with the column-header row</returns>
        private static StringBuilder BuildRowForColumnHeaders(Type type, char delimiter, bool mustQuote)
        {
            ContainerTypeInformation? metadata = TypeDiscoveryFactory.Resolve(type) ?? throw new TypeLoadException($"Type '{type.Name}' is not appropriately decorated."); ;
            StringBuilder builder = new StringBuilder();
            foreach (ContainerMemberTypeInformation member in metadata.Members.Values)
            {
                if (member.ContainerMemberDefinition is FileFieldAttribute ffa)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(delimiter);
                    }

                    string columnName = ffa.CreateQualifiedName();
                    if (columnName.Contains(delimiter) || mustQuote)
                    {
                        builder.Append($"\"{columnName}\"");
                    }
                    else
                    {
                        builder.Append(columnName);
                    }
                }
            }

            return builder;
        }


        /// <summary>
        /// Build a complete row for a data row
        /// </summary>
        /// <param name="delimiter">The delimiter character to be used</param>
        /// <param name="mustQuote">The option to force individual fields to be quoted. When false, only quotes if the <paramref name="delimiter"/> is a part of field-string</param>
        /// <param name="dataElements">A collection of some nature of the data elements</param>
        /// <returns>A <see cref="StringBuilder"/> instance with the data row</returns>
        private static StringBuilder BuildRowForDataRow(char delimiter, bool mustQuote, params object?[] dataElements)
            => BuildRowForDataRow(delimiter, mustQuote, dataElements.ToList());

        /// <summary>
        /// Build a complete row for a data row
        /// </summary>
        /// <param name="delimiter">The delimiter character to be used</param>
        /// <param name="mustQuote">The option to force individual fields to be quoted. When false, only quotes if the <paramref name="delimiter"/> is a part of field-string</param>
        /// <param name="dataElements">A collection of some nature of the data elements</param>
        /// <returns>A <see cref="StringBuilder"/> instance with the data row</returns>
        private static StringBuilder BuildRowForDataRow(char delimiter, bool mustQuote, IEnumerable<object?> dataElements)
        {
            StringBuilder builder = new StringBuilder();
            foreach(object? item in dataElements)
            {
                if (builder.Length > 0)
                {
                    builder.Append(delimiter);
                }

                string stringValue = (string?)ReflectionUtils.ConvertValueIfRequired(item, typeof(string)) ?? string.Empty;
                if (stringValue.Contains(delimiter) || mustQuote)
                {
                    builder.Append($"\"{stringValue}\"");
                }
                else
                {
                    builder.Append(stringValue);
                }
            }

            return builder;
        }
    }
}