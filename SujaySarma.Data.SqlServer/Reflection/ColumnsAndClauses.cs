using SujaySarma.Data.Core.Constants;
using SujaySarma.Data.Core.Reflection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SujaySarma.Data.SqlServer.Reflection
{
    /// <summary>
    /// Extract sequences of columns or build clauses for a SQL query.
    /// </summary>
    public static class ColumnsAndClauses
    {

        /// <summary>
        /// Extract the names of all column-possible members from the specified type (eg: for a SELECT query).
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object.</typeparam>
        /// <param name="tableAliasName">When not-NULL, prefixes string as the table alias to each column (e.g.: useful in queries with joins).</param>
        /// <returns>List of column names. Will be EMPTY if no suitable columns were found.</returns>
        public static List<string> GetColumnsList<TObject>(string? tableAliasName = null)
            => GetColumnsList(TypeDiscoveryFactory.Resolve<TObject>(), tableAliasName);

        /// <summary>
        /// Extract the names of all column-possible members from the specified type (eg: for a SELECT query).
        /// </summary>
        /// <param name="typeInfo">Type-discovered reflection information about the .NET object.</param>
        /// <param name="tableAliasName">When not-NULL, prefixes string as the table alias to each column (e.g.: useful in queries with joins).</param>
        /// <returns>List of column names. Will be EMPTY if no suitable columns were found.</returns>
        public static List<string> GetColumnsList(ContainerTypeInfo typeInfo, string? tableAliasName = null)
        {
            List<string> columnNames = new List<string>();
            tableAliasName = SanitiseTableAliasName(tableAliasName);
            foreach (MemberTypeInfo member in typeInfo.Members.Values)
            {
                string sqlColumnName = $"{tableAliasName}[{member.Column.CreateQualifiedName()}]";
                columnNames.Add(sqlColumnName);
            }

            return columnNames;
        }

        /// <summary>
        /// Get a dictionary of column names and their values for use in SQL INSERT, UPDATE, DELETE or WHERE statements.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object passed as <paramref name="obj"/>.</typeparam>
        /// <param name="obj">Instance of .NET object of type <typeparamref name="TObject"/>.</param>
        /// <param name="statementType">Type of statement the pairs are required for.</param>
        /// <param name="typeInfo">Reflected type informatio for <typeparamref name="TObject"/>.</param>
        /// <param name="tableAliasName">When not-NULL, prefixes string as the table alias to each column (e.g.: useful in queries with joins).</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static Dictionary<string, string> GetColumnValuePairs<TObject>(this TObject obj, SqlStatementType statementType, ContainerTypeInfo typeInfo, string? tableAliasName = null)
        {
            Dictionary<string, string> columnValuePairs = new Dictionary<string, string>();
            if (obj == null)
            {
                return columnValuePairs;
            }

            if (statementType == SqlStatementType.Upsert)
            {
                throw new InvalidOperationException("For Upsert operations, call this function twice: once with (statementType = Insert), and again with (statementType = Update).");
            }

            tableAliasName = SanitiseTableAliasName(tableAliasName);
            object? refInstance = obj;

            foreach (MemberTypeInfo member in typeInfo.Members.Values)
            {
                switch (statementType)
                {
                    case SqlStatementType.Query:
                    case SqlStatementType.Delete:
                        if (member.Column.IsSearchKey)
                        {
                            KeyValuePair<string, string> columnWithValue = GetColumnNameWithValue(member, ref refInstance);
                            columnValuePairs.Add($"{tableAliasName}[{columnWithValue.Key}]", columnWithValue.Value);
                        }
                        break;

                    case SqlStatementType.Insert:
                        if (member.Column.IncludeFor.HasFlag(ColumnInclusionStrategy.Inserts))
                        {
                            KeyValuePair<string, string> columnWithValue = GetColumnNameWithValue(member, ref refInstance);
                            columnValuePairs.Add($"{tableAliasName}[{columnWithValue.Key}]", columnWithValue.Value);
                        }
                        break;

                    case SqlStatementType.Update:
                        if (member.Column.IncludeFor.HasFlag(ColumnInclusionStrategy.Updates))
                        {
                            KeyValuePair<string, string> columnWithValue = GetColumnNameWithValue(member, ref refInstance);
                            columnValuePairs.Add($"{tableAliasName}[{columnWithValue.Key}]", columnWithValue.Value);
                        }
                        break;
                }
            }
            return columnValuePairs;
        }

        /// <summary>
        /// Append a list of strings to the StringBuilder, separated by the specified separator.
        /// </summary>
        /// <param name="sb">Instance of a StringBuilder to append the <paramref name="list"/> strings into.</param>
        /// <param name="list">List of strings to append into the <paramref name="sb"/>.</param>
        /// <param name="separator">The character sequence to use as the string-concatenation separator.</param>
        /// <returns>Updated StringBuilder instance with the appended list.</returns>
        public static StringBuilder AppendStringList(this StringBuilder sb, IEnumerable<string> list, string separator = ",")
        {
            if ((list == null) || (!list.Any()))
            {
                return sb;
            }

            sb.Append(string.Join(separator, list.Select(x => x.Trim())));
            return sb;
        }

        /// <summary>
        /// Append a list of strings to the StringBuilder, separated by the specified separator.
        /// </summary>
        /// <param name="sb">Instance of a StringBuilder to append the <paramref name="list"/> strings into.</param>
        /// <param name="separator">The character sequence to use as the string-concatenation separator.</param>
        /// <param name="list">List of strings to append into the <paramref name="sb"/>.</param>
        /// <returns>Updated StringBuilder instance with the appended list.</returns>
        public static StringBuilder AppendStringList(this StringBuilder sb, string separator = ",", params string[] list)
            => AppendStringList(sb, (IEnumerable<string>)list, separator);

        /// <summary>
        /// Append a list of strings to the StringBuilder, separated by the specified separator.
        /// </summary>
        /// <param name="sb">Instance of a StringBuilder to append the <paramref name="list"/> strings into.</param>
        /// <param name="list">List of strings to append into the <paramref name="sb"/>.</param>
        /// <param name="separator">The character to use as the string-concatenation separator.</param>
        /// <returns>Updated StringBuilder instance with the appended list.</returns>
        public static StringBuilder AppendStringList(this StringBuilder sb, IEnumerable<string> list, char separator = ',')
        {
            if ((list == null) || (!list.Any()))
            {
                return sb;
            }

            sb.Append(string.Join(separator, list.Select(x => x.Trim())));
            return sb;
        }

        /// <summary>
        /// Append a list of strings to the StringBuilder, separated by the specified separator.
        /// </summary>
        /// <param name="sb">Instance of a StringBuilder to append the <paramref name="list"/> strings into.</param>
        /// <param name="separator">The character to use as the string-concatenation separator.</param>
        /// <param name="list">List of strings to append into the <paramref name="sb"/>.</param>
        /// <returns>Updated StringBuilder instance with the appended list.</returns>
        public static StringBuilder AppendStringList(this StringBuilder sb, char separator = ',', params string[] list)
            => AppendStringList(sb, (IEnumerable<string>)list, separator);

        /// <summary>
        /// Appends a collection of column names and their corresponding values to the provided StringBuilder, formatted as a comma-separated list of key-value pairs. 
        /// This method does NOT quote column names with [,], and column names are expected to be properly formatted already.
        /// </summary>
        /// <param name="sb">StringBuilder to append to.</param>
        /// <param name="columnsWithValues">Collection of column/value pairs to append.</param>
        /// <param name="separator">The character sequence to use as the string-concatenation separator.</param>
        /// <returns>The <paramref name="sb"/> with the added key-value pairs</returns>
        public static StringBuilder AppendColumnsWithValues(this StringBuilder sb, Dictionary<string, string> columnsWithValues, string separator = ",")
        {
            if ((columnsWithValues != null) && columnsWithValues.Any())
            {
                sb.Append(string.Join(separator, columnsWithValues.Select(kv => $"{kv.Key} = {kv.Value}")));
            }
            return sb;
        }

        /// <summary>
        /// Appends a collection of column names and their corresponding values to the provided StringBuilder, formatted as a comma-separated list of key-value pairs. 
        /// This method does NOT quote column names with [,], and column names are expected to be properly formatted already.
        /// </summary>
        /// <param name="sb">StringBuilder to append to.</param>
        /// <param name="columnsWithValues">Collection of column/value pairs to append.</param>
        /// <param name="separator">The character sequence to use as the string-concatenation separator.</param>
        /// <returns>The <paramref name="sb"/> with the added key-value pairs</returns>
        public static StringBuilder AppendColumnsWithValues(this StringBuilder sb, Dictionary<string, object?>? columnsWithValues, string separator = ",")
        {
            if ((columnsWithValues != null) && columnsWithValues.Any())
            {
                sb.Append(string.Join(separator, columnsWithValues.Select(kv => $"{kv.Key} = {ReflectionUtils.GetSQLStringValue(kv.Value)}")));
            }
            return sb;
        }
        
        /// <summary>
        /// Appends a collection of column names and their corresponding values to the provided StringBuilder, formatted as a comma-separated list of key-value pairs. 
        /// This method does NOT quote column names with [,], and column names are expected to be properly formatted already.
        /// </summary>
        /// <param name="sb">StringBuilder to append to.</param>
        /// <param name="columnsWithValues">Collection of column/value pairs to append.</param>
        /// <param name="separator">The character to use as the string-concatenation separator.</param>
        /// <returns>The <paramref name="sb"/> with the added key-value pairs</returns>
        public static StringBuilder AppendColumnsWithValues(this StringBuilder sb, Dictionary<string, string> columnsWithValues, char separator = ',')
        {
            if ((columnsWithValues != null) && columnsWithValues.Any())
            {
                sb.Append(string.Join(separator, columnsWithValues.Select(kv => $"{kv.Key} = {kv.Value}")));
            }
            return sb;
        }

        /// <summary>
        /// Appends a collection of column names and their corresponding values to the provided StringBuilder, formatted as a comma-separated list of key-value pairs. 
        /// This method does NOT quote column names with [,], and column names are expected to be properly formatted already.
        /// </summary>
        /// <param name="sb">StringBuilder to append to.</param>
        /// <param name="columnsWithValues">Collection of column/value pairs to append.</param>
        /// <param name="separator">The character to use as the string-concatenation separator.</param>
        /// <returns>The <paramref name="sb"/> with the added key-value pairs</returns>
        public static StringBuilder AppendColumnsWithValues(this StringBuilder sb, Dictionary<string, object?>? columnsWithValues, char separator = ',')
        {
            if ((columnsWithValues != null) && columnsWithValues.Any())
            {
                sb.Append(string.Join(separator, columnsWithValues.Select(kv => $"{kv.Key} = {ReflectionUtils.GetSQLStringValue(kv.Value)}")));
            }
            return sb;
        }


        /// <summary>
        /// Create a key/value pair representing a column name and its value for use in a SQL WHERE or UPDATE clause.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET object.</typeparam>
        /// <param name="member">Reflected type information about the property/field to extract the value from.</param>
        /// <param name="obj">Instance of object, should NOT be NULL.</param>
        /// <returns>Key/value pair containing the column name/value pair.</returns>
        private static KeyValuePair<string, string> GetColumnNameWithValue<TObject>(MemberTypeInfo member, ref TObject obj)
        {
            object? refInstance = obj;

            string sqlColumnName = member.Column.CreateQualifiedName();
            string value = ReflectionUtils.GetSQLStringValue(Core.ReflectionUtils.GetValue(ref refInstance, member));

            return new KeyValuePair<string, string>(sqlColumnName, value);
        }

        /// <summary>
        /// Ensure that the <paramref name="tableAliasName"/> is in a well-known form and format.
        /// </summary>
        /// <param name="tableAliasName">Table alias name passed in as argument.</param>
        /// <returns>Well-known form and format of the name: Empty string OR alias name enclosed in [,] and suffixed by a '.'</returns>
        private static string SanitiseTableAliasName(string? tableAliasName)
            => (string.IsNullOrWhiteSpace(tableAliasName) ? string.Empty : $"[{tableAliasName}].");

    }
}
