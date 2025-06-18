using SujaySarma.Data.Core;
using SujaySarma.Data.Core.Reflection;
using SujaySarma.Data.Files.TokenLimitedFiles.Attributes;

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SujaySarma.Data.Files.TokenLimitedFiles
{
    // Helper methods
    public sealed partial class TokenLimitedFileWriter
    {

        /// <summary>
        /// Build a complete row of column names (header row)
        /// </summary>
        /// <param name="table">A <see cref="T:System.Data.DataTable" /> containing the column definitions</param>
        /// <param name="delimiter">The delimiter character to be used</param>
        /// <param name="mustQuote">The option to force individual fields to be quoted. When false, only quotes if the <paramref name="delimiter" /> is a part of field-string</param>
        /// <returns>A <see cref="T:System.Text.StringBuilder" /> instance with the column-header row</returns>
        private static StringBuilder BuildRowForColumnHeaders(DataTable table, char delimiter, bool mustQuote)
        {
            StringBuilder sb = new StringBuilder();
            for (int index = 0; index < table.Columns.Count; ++index)
            {
                if (index > 0)
                {
                    sb.Append(delimiter);
                }

                if (table.Columns[index].ColumnName.Contains(delimiter) || mustQuote)
                {
                    sb.Append(QUOTE)
                        .Append(table.Columns[index].ColumnName)
                      .Append(QUOTE);
                }
                else
                {
                    sb.Append(table.Columns[index].ColumnName);
                }
            }
            return sb;
        }

        /// <summary>
        /// Build a complete row of column names (header row)
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="delimiter">The delimiter character to be used</param>
        /// <param name="mustQuote">The option to force individual fields to be quoted. When false, only quotes if the <paramref name="delimiter" /> is a part of field-string</param>
        /// <returns>A <see cref="T:System.Text.StringBuilder" /> instance with the column-header row</returns>
        private static StringBuilder BuildRowForColumnHeaders<TObject>(char delimiter, bool mustQuote)
            => BuildRowForColumnHeaders(typeof(TObject), delimiter, mustQuote);

        /// <summary>
        /// Build a complete row of column names (header row)
        /// </summary>
        /// <param name="type">Type of .NET class, structure or record</param>
        /// <param name="delimiter">The delimiter character to be used</param>
        /// <param name="mustQuote">The option to force individual fields to be quoted. When false, only quotes if the <paramref name="delimiter" /> is a part of field-string</param>
        /// <returns>A <see cref="T:System.Text.StringBuilder" /> instance with the column-header row</returns>
        private static StringBuilder BuildRowForColumnHeaders(Type type, char delimiter, bool mustQuote)
        {
            ContainerTypeInfo ContainerTypeInfo = TypeDiscoveryFactory.Resolve(type);
            StringBuilder sb = new StringBuilder();

            foreach (MemberTypeInfo memberTypeInformation in ContainerTypeInfo.Members.Values)
            {
                if (memberTypeInformation.Column is FileFieldAttribute memberDefinition)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(delimiter);
                    }

                    string qualifiedName = memberDefinition.CreateQualifiedName();
                    if (qualifiedName.Contains(delimiter) || mustQuote)
                    {
                        sb.Append(QUOTE)
                                .Append(qualifiedName)
                            .Append(QUOTE);
                    }
                    else
                    {
                        sb.Append(qualifiedName);
                    }
                }
            }

            return sb;
        }

        /// <summary>Build a complete row for a data row</summary>
        /// <param name="delimiter">The delimiter character to be used</param>
        /// <param name="mustQuote">The option to force individual fields to be quoted. When false, only quotes if the <paramref name="delimiter" /> is a part of field-string</param>
        /// <param name="dataElements">A collection of some nature of the data elements</param>
        /// <returns>A <see cref="T:System.Text.StringBuilder" /> instance with the data row</returns>
        private static StringBuilder BuildRowForDataRow(char delimiter, bool mustQuote, params object?[] dataElements)
            => BuildRowForDataRow(delimiter, mustQuote, (IEnumerable<object>)dataElements);

        /// <summary>Build a complete row for a data row</summary>
        /// <param name="delimiter">The delimiter character to be used</param>
        /// <param name="mustQuote">The option to force individual fields to be quoted. When false, only quotes if the <paramref name="delimiter" /> is a part of field-string</param>
        /// <param name="dataElements">A collection of some nature of the data elements</param>
        /// <returns>A <see cref="T:System.Text.StringBuilder" /> instance with the data row</returns>
        private static StringBuilder BuildRowForDataRow(char delimiter, bool mustQuote, IEnumerable<object?> dataElements)
        {
            StringBuilder sb = new StringBuilder();
            Type targetClrType = typeof(string);

            foreach (object? dataElement in dataElements)
            {
                if (sb.Length > 0)
                {
                    sb.Append(delimiter);
                }
                
                string str = (string?)ReflectionUtils.ConvertValueIfRequired(dataElement, targetClrType) ?? string.Empty;
                if (str.Contains(delimiter) || mustQuote)
                {
                    sb.Append(QUOTE)
                            .Append(str)
                        .Append(QUOTE);
                }
                else
                {
                    sb.Append(str);
                }
            }

            return sb;
        }
    }
}
