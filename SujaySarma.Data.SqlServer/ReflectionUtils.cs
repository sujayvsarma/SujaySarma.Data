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
        /// Get the value correctly formatted and appropriately quoted (and escaped) for use in a SQL statement.
        /// </summary>
        /// <param name="clrValue">Value from the CLR object</param>
        /// <param name="quotedStrings">When true, returns strings in quoted form</param>
        /// <returns>Correctly quoted and formatted value to be used in a SQL statement</returns>
        public static string GetSQLStringValue(object? clrValue, bool quotedStrings = true)
        {
            if ((clrValue == null) || (clrValue == default))
            {
                return "NULL";
            }

            if (clrValue is byte[] v5)
            {
                return $"0x{Convert.ToHexString(v5)}";
            }

            Type t = clrValue.GetType();
            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Boolean:
                    return $"{((bool)clrValue ? 1 : 0)}";

                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.SByte:
                case TypeCode.Single:
                    return $"{clrValue}";

                case TypeCode.Char:
                    return (quotedStrings ? $"'{clrValue}'" : $"{clrValue}");

                case TypeCode.String:
                    string s = (string)clrValue;
                    s = s.Replace("'", "''");
                    return (quotedStrings ? $"'{s}'" : $"{s}");

                case TypeCode.DateTime:
                    return (quotedStrings ? $"'{((DateTime)clrValue).ToUniversalTime():yyyy-MM-ddTHH:mm:ss}Z'" : $"{((DateTime)clrValue).ToUniversalTime():yyyy-MM-ddTHH:mm:ss}Z");
            }

            if (clrValue is DateOnly v1)
            {
                return (quotedStrings ? $"'{v1:yyyy-MM-dd}T00:00:00Z'" : $"{v1:yyyy-MM-dd}T00:00:00Z");
            }

            if (clrValue is TimeOnly v2)
            {
                return (quotedStrings ? $"'01-01-{DateTime.UtcNow.Year}T{v2:HH:mm:ss}Z'" : $"01-01-{DateTime.UtcNow.Year}T{v2:HH:mm:ss}Z");
            }

            if (clrValue is DateTimeOffset v3)
            {
                return (quotedStrings ? $"'{v3.UtcDateTime:yyyy-MM-ddTHH:mm:ss}Z'" : $"{v3.UtcDateTime:yyyy-MM-ddTHH:mm:ss}Z");
            }

            if (clrValue is Guid v4)
            {
                return (quotedStrings ? $"'{v4:d}'" : $"{v4:d}");
            }

            if (clrValue is IEnumerable e)
            {
                List<string> strings = new();
                foreach (object element in e)
                {
                    strings.Add(GetSQLStringValue(element));
                }
                return string.Join(",", strings);
            }

            throw new ArgumentException($"Cannot serialize clrValue of type '{t.Name}'.");
        }

        /// <summary>
        /// Returns the SQL data type best matching the provided CLR type
        /// </summary>
        /// <param name="clrType">CLR type</param>
        /// <returns>SQL data type as a string</returns>
        public static string GetSqlTypeForClrType(Type clrType)
        {
            if (SqlClrTypeMapping.TryGetValue(clrType, out string? sqlType))
            {
                return sqlType;
            }

            return "nvarchar";  // when all else fails
        }

        /// <summary>
        /// Type mapping dictionary used by <see cref="GetSqlTypeForClrType(Type)"/>
        /// </summary>
        private static readonly Dictionary<Type, string> SqlClrTypeMapping = new Dictionary<Type, string>()
        {
            { typeof(bool), "bit" },
            { typeof(byte[]), "varbinary" },
            { typeof(byte), "tinyint" },
            { typeof(sbyte), "tinyint" },
            { typeof(char), "nchar" },
            { typeof(decimal), "float" },
            { typeof(double), "decimal" },
            { typeof(float), "float" },
            { typeof(Guid), "uniqueidentifier" },
            { typeof(int), "int" },
            { typeof(uint), "int" },
            { typeof(long), "bigint" },
            { typeof(ulong), "bigint" },
            { typeof(short), "smallint" },
            { typeof(ushort), "smallint" },
            { typeof(string), "nvarchar" },
            { typeof(DateTime), "datetime" },
            { typeof(DateTimeOffset), "datetimeoffset" },
            { typeof(DateOnly), "smalldatetime" },
            { typeof(TimeOnly), "smalldatetime" },
            { typeof(TimeSpan), "datetimeoffset"}
        };

    }
}
