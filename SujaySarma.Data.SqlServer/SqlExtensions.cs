using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.Core.TypeDiscovery;
using SujaySarma.Data.SqlServer.Attributes;

using System;
using System.Collections.Generic;

namespace SujaySarma.Data.SqlServer;

/// <summary>
/// SQL Server specific extension methods
/// </summary>
internal static class SqlExtensions
{

    /// <summary>
    /// Returns the SQL data type best matching the provided CLR type
    /// </summary>
    /// <param name="clrType">CLR type</param>
    /// <returns>SQL data type as a string</returns>
    public static string GetSqlTypeForClrType(this Type clrType)
        => (SqlClrTypeMapping.TryGetValue(clrType, out string? str) ? str : "nvarchar");

    /// <summary>
    /// Get the value correctly formatted and appropriately quoted (and escaped) for use in a SQL statement.
    /// </summary>
    /// <param name="clrValue">Value from the CLR object.</param>
    /// <param name="quotedStrings">When true, returns strings in quoted form.</param>
    /// <returns>Correctly quoted and formatted value to be used in a SQL statement.</returns>
    public static string GetSQLStringValue(this object? clrValue, bool quotedStrings = true)
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
        return QuoteStringValueIfRequired(clrValue.ToString()!, quotedStrings);
    }

    /// <summary>
    /// Helper to quote and escape strings if required.
    /// </summary>
    /// <param name="value">String value.</param>
    /// <param name="quotedStrings">When true, returns strings in quoted form.</param>
    /// <returns>Quoted and escaped string.</returns>
    public static string QuoteStringValueIfRequired(this string value, bool quotedStrings)
    {
        if (quotedStrings)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "''";
            }
            else
            {
                if ((value[0] is not '\'') && (value[^1] is not '\''))
                {
                    return $"'{value.Replace("'", "''")}'";
                }
            }
        }

        return value;
    }

    /// <summary>
    /// Enclose an identifier in square-brackets if not already done so. 
    /// Follows SQL Server's quoting/escaping rules to do so.
    /// </summary>
    /// <param name="identifier">Name of a table, column, etc.</param>
    /// <returns>The quoted identifer.</returns>
    public static string EnsureIdentifierIsQuoted(this string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return identifier;
        }

        // Already properly quoted? Return as-is.
        if ((identifier.Length >= 2) && (identifier[0] is '[') && (identifier[^1] is ']'))
        {
            return identifier;
        }

        // Process character by character to escape brackets and quote the identifier.
        System.Text.StringBuilder builder = new System.Text.StringBuilder(identifier.Length + 10);
        builder.Append('[');

        for (int i = 0; i < identifier.Length; i++)
        {
            char c = identifier[i];

            if ((c is '[') || (c is ']'))
            {
                // Escape brackets by doubling them
                builder.Append(c);
                builder.Append(c);
            }
            else
            {
                builder.Append(c);
            }
        }

        builder.Append(']');
        return builder.ToString();
    }

    /// <summary>
    /// Applies the provided <paramref name="prefix"/> to every string in <paramref name="list"/>. The caller 
    /// may use either the returned list or the <paramref name="list"/> (both will be identical).
    /// </summary>
    /// <param name="list">The list of strings to apply prefixes to.</param>
    /// <param name="prefix">The prefix to apply.</param>
    /// <returns>List with prefixes applied. The caller may use either the returned list or the <paramref name="list"/> (both will be identical).</returns>
    public static List<string> ApplyPrefix(this List<string> list, string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            throw new ArgumentNullException(nameof(prefix));
        }
        
        for (int i = 0; i < list.Count; i++)
        {
            list[i] = $"{prefix}{list[i]}";
        }

        return list;
    }


    /// <summary>
    /// Get the type discovery options for SQL Server specific attribute annotated entities.
    /// </summary>
    /// <returns>The applicable <see cref="TypeDiscoveryOptions"/>.</returns>
    public static TypeDiscoveryOptions GetSqlServerTypeDiscoveryOptions()
    {
        if (_options is null)
        {
            _options = new TypeDiscoveryOptions()
            {
                MustHaveAtLeastOneMember = true,
                PersistenceContainerAttributeRestriction = typeof(SqlTable),
                PersistenceContainerMemberAttributeRestriction = typeof(SqlTableColumn)
            };
        }

        return _options.Value;
    }
    private static TypeDiscoveryOptions? _options = null;

    /// <summary>
    /// Validate that the type can be resolved and returns the metadata.
    /// </summary>
    /// <param name="type">The type to resolve.</param>
    /// <returns>The metadata as a <see cref="PersistenceContainerInfo"/>.</returns>
    public static PersistenceContainerInfo RetrievePersistenceContainerInfoOrThrowException(this Type type)
    {
        if (TypeDiscoveryFactory.TryResolve(type, out PersistenceContainerInfo? pci, GetSqlServerTypeDiscoveryOptions()) && (pci is not null))
        {
            return pci;
        }

        throw new TypeLoadException($"The type '{type.GetUsableTypeName()}': is not valid for this operation.");
    }

    /// <summary>
    /// Validates that the type is valid for ORM operations. Throws an exception if not.
    /// </summary>
    /// <param name="type">The type to validate.</param>
    public static void ValidateForOrmWithException(this Type type)
    {
        if (!TypeDiscoveryFactory.TryValidate(type, GetSqlServerTypeDiscoveryOptions()))
        {
            throw new TypeLoadException($"The type '{type.GetUsableTypeName()}': is not valid for this operation.");
        }
    }


    /// <summary>
    /// Type conversion cache used by <see cref="GetSQLStringValue(object?, bool)" />.
    /// </summary>
    private static readonly Dictionary<Type, Func<object, string>> TypeConversionCache = new()
        {
            { typeof(bool), value => (bool)value ? "1" : "0" },
            { typeof(byte[]), value => "0x" + Convert.ToHexString((byte[])value) },
            { typeof(char), value => QuoteStringValueIfRequired(value!.ToString()!, true) },
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
            { typeof(string), value => QuoteStringValueIfRequired(value.ToString()!, true) },
            { typeof(DateTime), value => $"'{(DateTime)value:yyyy-MM-dd HH:mm:ss.fff}'" },
            { typeof(Guid), value => $"'{value}'" }
        };

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

}
