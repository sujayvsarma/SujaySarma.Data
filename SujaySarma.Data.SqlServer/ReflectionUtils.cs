using System;
using System.Collections.Generic;

namespace SujaySarma.Data.SqlServer
{
    /// <summary>
    /// Provides tools to perform various actions on types and objects
    /// </summary>
    internal static class ReflectionUtils
    {
        /// <summary>
        /// Type mapping dictionary used by <see cref="GetSqlTypeForClrType(Type)" />
        /// </summary>
        private static readonly Dictionary<Type, string> SqlClrTypeMapping = new Dictionary<Type, string>()
        {
            { typeof (bool), "bit" },
            { typeof (byte[]), "varbinary" },
            { typeof (byte), "tinyint" },
            { typeof (sbyte), "tinyint" },
            { typeof (char), "nchar" },
            { typeof (Decimal), "float" },
            { typeof (double), "decimal" },
            { typeof (float), "float" },
            { typeof (Guid), "uniqueidentifier" },
            { typeof (int), "int" },
            { typeof (uint), "int" },
            { typeof (long), "bigint" },
            { typeof (ulong), "bigint" },
            { typeof (short), "smallint" },
            { typeof (ushort), "smallint" },
            { typeof (string), "nvarchar" },
            { typeof (DateTime), "datetime" },
            { typeof (DateTimeOffset), "datetimeoffset" },
            { typeof (DateOnly), "smalldatetime" },
            { typeof (TimeOnly), "smalldatetime" },
            { typeof (TimeSpan), "datetimeoffset" }
        };

        /// <summary>
        /// Type conversion cache used by <see cref="GetSQLStringValue(object?, bool)" />
        /// </summary>
        private static readonly Dictionary<Type, Func<object, string>> TypeConversionCache = new()
        {
            { typeof(bool), value => (bool)value ? "1" : "0" },
            { typeof(byte[]), value => "0x" + Convert.ToHexString((byte[])value) },
            { typeof(char), value => QuoteIfRequired(value!.ToString()!, true) },
            { typeof(sbyte), value => value.ToString()! },
            { typeof(byte), value => value.ToString()! },
            { typeof(short), value => value.ToString()! },
            { typeof(ushort), value => value.ToString()! },
            { typeof(int), value => value.ToString()! },
            { typeof(uint), value => value.ToString()! },
            { typeof(long), value => value.ToString()! },
            { typeof(ulong), value => value.ToString()! },
            { typeof(float), value => ((float)value).ToString("R") },
            { typeof(double), value => ((double)value).ToString("R") },
            { typeof(decimal), value => ((decimal)value).ToString("G") },
            { typeof(string), value => QuoteIfRequired(value.ToString()!, true) },
            { typeof(DateTime), value => $"'{(DateTime)value:yyyy-MM-dd HH:mm:ss.fff}'" },
            { typeof(Guid), value => $"'{value}'" }
        };

        /// <summary>
        /// Get the value correctly formatted and appropriately quoted (and escaped) for use in a SQL statement.
        /// </summary>
        /// <param name="clrValue">Value from the CLR object</param>
        /// <param name="quotedStrings">When true, returns strings in quoted form</param>
        /// <returns>Correctly quoted and formatted value to be used in a SQL statement</returns>
        public static string GetSQLStringValue(object? clrValue, bool quotedStrings = true)
        {
            if (clrValue is null)
            {
                return "NULL";
            }

            Type type = clrValue.GetType();

            if (TypeConversionCache.TryGetValue(type, out var converter))
            {
                return converter(clrValue);
            }

            // Fallback for unsupported types
            return QuoteIfRequired(clrValue.ToString()!, quotedStrings);
        }

        /// <summary>
        /// Returns the SQL data type best matching the provided CLR type
        /// </summary>
        /// <param name="clrType">CLR type</param>
        /// <returns>SQL data type as a string</returns>
        public static string GetSqlTypeForClrType(Type clrType)
            => (SqlClrTypeMapping.TryGetValue(clrType, out string? str) ? str : "nvarchar");

        /// <summary>
        /// Helper to quote and escape strings if required
        /// </summary>
        /// <param name="value">String value</param>
        /// <param name="quotedStrings">When true, returns strings in quoted form</param>
        /// <returns>Quoted and escaped string</returns>
        private static string QuoteIfRequired(string value, bool quotedStrings)
            => (quotedStrings ? $"'{value.Replace("'", "''")}'" : value);
    }
}
