using System;
using System.Collections;
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

            // Handle byte[] specially, since it needs to be converted to hex format
            if (clrValue is byte[] inArray)
            {
                return "0x" + Convert.ToHexString(inArray);
            }

            Type type = clrValue.GetType();
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return $"{((bool)clrValue ? 1 : 0)}";

                case TypeCode.Char:
                    return quoteIfRequired($"{clrValue}", quotedStrings);

                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return $"{clrValue}";

                case TypeCode.DateTime:
                    return quoteIfRequired($"{((DateTime)clrValue).ToUniversalTime():yyyy-MM-ddTHH:mm:ss}Z", quotedStrings);

                case TypeCode.String:
                    string str = ((string)clrValue).Replace("'", "''");
                    return quoteIfRequired(str, quotedStrings);
            }

            switch (clrValue)
            {
                case DateOnly dateOnly:
                    return quoteIfRequired($"{dateOnly:yyyy-MM-dd}T00:00:00Z", quotedStrings);

                case TimeOnly timeOnly:
                    return quoteIfRequired($"01-01-{DateTime.UtcNow.Year}T{timeOnly:HH:mm:ss}Z", quotedStrings);

                case DateTimeOffset dateTimeOffset:
                    return quoteIfRequired($"{dateTimeOffset.UtcDateTime:yyyy-MM-ddTHH:mm:ss}Z", quotedStrings);

                case Guid guid:
                    return quoteIfRequired($"{guid:d}", quotedStrings);

                case IEnumerable enumerable:
                    List<string> values = new List<string>();
                    foreach (object item in enumerable)
                    {
                        values.Add(GetSQLStringValue(item, quotedStrings));
                    }
                    return  quoteIfRequired(string.Join(",", (IEnumerable<string>)values), quotedStrings);
            }

            throw new ArgumentException($"Cannot serialize clrValue of type '{type.Name}'.");


            // Nifty helper to reduce LOC
            static string quoteIfRequired(string val, bool requireQuotes)
            {
                if (requireQuotes)
                {
                    return $"'{val}'";
                }
                return val;
            }
        }

        /// <summary>
        /// Returns the SQL data type best matching the provided CLR type
        /// </summary>
        /// <param name="clrType">CLR type</param>
        /// <returns>SQL data type as a string</returns>
        public static string GetSqlTypeForClrType(Type clrType)
            => (SqlClrTypeMapping.TryGetValue(clrType, out string? str) ? str : "nvarchar");
    }
}
