using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

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
        /// Implementation function - actually write the records to the stream
        /// </summary>
        /// <param name="writer">Instance of TokenLimitedFileWriter that will perform the writing</param>
        /// <param name="table"><see cref="DataTable"/> containing the information to write</param>
        /// <param name="quoteAllStrings">When set, quotes all string values</param>
        private static void WriteRecordsImpl(TokenLimitedFileWriter writer, DataTable table, bool quoteAllStrings = true)
        {
            // Because we are working with strings, we will do a better job doing the quote-handling "in-house" than 
            // calling the other WriteRecordsImpl with the header and data rows.

            string?[]? header = new string[table.Columns.Count];
            for (int i = 0; i < table.Columns.Count; i++)
            {
                header[i] = (quoteAllStrings ? $"\"{table.Columns[i].ColumnName}\"" : table.Columns[i].ColumnName);
            }
            writer.Write(header);

            string? colData;
            for (int r = 0; r < table.Rows.Count; r++)
            {
                string?[]? data = new string[table.Columns.Count];

                for (int c = 0; c < table.Columns.Count; c++)
                {
                    if (table.Columns[c].DataType == typeof(string))
                    {
                        colData = table.Rows[r][c] as string;
                    }
                    else
                    {
                        colData = (string?)ReflectionUtils.ConvertValueIfRequired(table.Rows[r][c], typeof(string));
                    }

                    if ((colData != default) && (quoteAllStrings || (colData.Contains(writer.Delimiter))))
                    {
                        colData = $"\"{colData}\"";
                    }

                    data[c] = colData;
                }

                writer.Write(data);
            }
        }


        /// <summary>
        /// Write a single row of something into the flatfile
        /// </summary>
        /// <param name="writer">Instance of TokenLimitedFileWriter that will perform the writing</param>
        /// <param name="row">Single row of strings to write in</param>
        /// <param name="quoteAllStrings">When set, quotes all string values</param>
        private static void WriteRecordsImpl(TokenLimitedFileWriter writer, string?[] row, bool quoteAllStrings = true)
        {
            if (quoteAllStrings)
            {
                for (int i = 0; i < row.Length; i++)
                {
                    row[i] = $"\"{row[i]}\"";
                }
            }
            else
            {
                for (int i = 0; i < row.Length; i++)
                {
                    if ((row[i] != null) && row[i]!.Contains(writer.Delimiter))
                    {
                        row[i] = $"\"{row[i]}\"";
                    }
                }
            }

            writer.Write(row);
        }

        /// <summary>
        /// Write record from DataTable to the stream
        /// </summary>
        /// <param name="table">DataTable with records to write</param>
        /// <param name="stream">Existing stream to open the writer on</param>
        /// <param name="encoding">Specific encoding</param>
        /// <param name="bufferSize">Minimum stream buffer size</param>
        /// <param name="leaveStreamOpen">Set to dispose the stream when this object is disposed</param>
        /// <param name="quoteAllStrings">Set to quote all string values in the output</param>
        /// <returns>Number of records written</returns>
        public static ulong WriteRecords(DataTable table, Stream stream, Encoding? encoding = default, int bufferSize = -1, bool leaveStreamOpen = false, bool quoteAllStrings = true)
        {
            using TokenLimitedFileWriter writer = new TokenLimitedFileWriter(stream, encoding, bufferSize, leaveStreamOpen);
            WriteRecordsImpl(writer, table, quoteAllStrings);
            return writer.ROWS_WRITTEN;
        }

        /// <summary>
        /// Write record from DataTable to the file at the path
        /// </summary>
        /// <param name="table">DataTable with records to write</param>
        /// <param name="path">Path to the disk file to open the reader on</param>
        /// <param name="encoding">Specific encoding</param>
        /// <param name="leaveStreamOpen">Set to dispose the stream when this object is disposed</param>
        /// <param name="quoteAllStrings">Set to quote all string values in the output</param>
        /// <returns>Number of records written</returns>
        public static ulong WriteRecords(DataTable table, string path, Encoding? encoding = default, bool leaveStreamOpen = false, bool quoteAllStrings = true)
        {
            using TokenLimitedFileWriter writer = new TokenLimitedFileWriter(path, encoding, leaveStreamOpen);
            WriteRecordsImpl(writer, table, quoteAllStrings);
            return writer.ROWS_WRITTEN;
        }

        /// <summary>
        /// Write record from object to the file at the path
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="list">List of item to convert to record</param>
        /// <param name="path">Path to the disk file to open the reader on</param>
        /// <param name="encoding">Specific encoding</param>
        /// <param name="leaveStreamOpen">Set to dispose the stream when this object is disposed</param>
        /// <param name="quoteAllStrings">Set to quote all string values in the output</param>
        /// <returns>Number of records written</returns>
        public static ulong WriteRecords<TObject>(List<TObject> list, string path, Encoding? encoding = default, bool leaveStreamOpen = false, bool quoteAllStrings = true)
        {
            using TokenLimitedFileWriter writer = new TokenLimitedFileWriter(path, encoding, leaveStreamOpen);

            ContainerTypeInformation typeInfo = TypeDiscoveryFactory.Resolve<TObject>();
            string?[] row = new string[typeInfo.Members.Count];
            int columnIndex = 0;

            // Write the headers
            foreach (ContainerMemberTypeInformation member in typeInfo.Members.Values)
            {
                if (member.ContainerMemberDefinition is FileFieldAttribute ffa)
                {
                    row[columnIndex++] = ffa.CreateQualifiedName();
                }
            }
            WriteRecordsImpl(writer, row, quoteAllStrings);
           
            foreach(TObject item in list)
            {
                Array.Clear(row);

                foreach (ContainerMemberTypeInformation member in typeInfo.Members.Values)
                {
                    if (member.ContainerMemberDefinition is FileFieldAttribute ffa)
                    {
                        row[columnIndex++] = (string?)ReflectionUtils.ConvertValueIfRequired(ReflectionUtils.GetValue(item, member), typeof(string));
                    }
                }
                WriteRecordsImpl(writer, row, quoteAllStrings);
            }

            return writer.ROWS_WRITTEN;
        }

        /// <summary>
        /// Write record from object to the file at the path
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="list">List of item to convert to record</param>
        /// <param name="stream">Existing stream to open the writer on</param>
        /// <param name="encoding">Specific encoding</param>
        /// <param name="leaveStreamOpen">Set to dispose the stream when this object is disposed</param>
        /// <param name="quoteAllStrings">Set to quote all string values in the output</param>
        /// <returns>Number of records written</returns>
        public static ulong WriteRecords<TObject>(List<TObject> list, Stream stream, Encoding? encoding = default, bool leaveStreamOpen = false, bool quoteAllStrings = true)
        {
            using TokenLimitedFileWriter writer = new TokenLimitedFileWriter(stream, encoding, leaveStreamOpen: leaveStreamOpen);

            ContainerTypeInformation typeInfo = TypeDiscoveryFactory.Resolve<TObject>();
            string?[] row = new string[typeInfo.Members.Count];
            int columnIndex = 0;

            // Write the headers
            foreach (ContainerMemberTypeInformation member in typeInfo.Members.Values)
            {
                if (member.ContainerMemberDefinition is FileFieldAttribute ffa)
                {
                    row[columnIndex++] = ffa.CreateQualifiedName();
                }
            }
            WriteRecordsImpl(writer, row, quoteAllStrings);

            foreach (TObject item in list)
            {
                Array.Clear(row);

                foreach (ContainerMemberTypeInformation member in typeInfo.Members.Values)
                {
                    if (member.ContainerMemberDefinition is FileFieldAttribute ffa)
                    {
                        row[columnIndex++] = (string?)ReflectionUtils.ConvertValueIfRequired(ReflectionUtils.GetValue(item, member), typeof(string));
                    }
                }
                WriteRecordsImpl(writer, row, quoteAllStrings);
            }

            return writer.ROWS_WRITTEN;
        }


        /// <summary>
        /// Write record from object to the file at the path
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="path">Path to the disk file to open the reader on</param>
        /// <param name="encoding">Specific encoding</param>
        /// <param name="leaveStreamOpen">Set to dispose the stream when this object is disposed</param>
        /// <param name="quoteAllStrings">Set to quote all string values in the output</param>
        /// <param name="list">List of item to convert to record</param>
        /// <returns>Number of records written</returns>
        public static ulong WriteRecords<TObject>(string path, Encoding? encoding = default, bool leaveStreamOpen = false, bool quoteAllStrings = true, params TObject[] list)
            => WriteRecords<TObject>(list.ToList(), path, encoding, leaveStreamOpen, quoteAllStrings);

        /// <summary>
        /// Write record from object to the file at the path
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="stream">Existing stream to open the writer on</param>
        /// <param name="encoding">Specific encoding</param>
        /// <param name="leaveStreamOpen">Set to dispose the stream when this object is disposed</param>
        /// <param name="quoteAllStrings">Set to quote all string values in the output</param>
        /// <param name="list">List of item to convert to record</param>
        /// <returns>Number of records written</returns>
        public static ulong WriteRecords<TObject>(Stream stream, Encoding? encoding = default, bool leaveStreamOpen = false, bool quoteAllStrings = true, params TObject[] list)
            => WriteRecords<TObject>(list.ToList(), stream, encoding, leaveStreamOpen, quoteAllStrings);

    }
}