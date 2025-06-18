using SujaySarma.Data.Core;

using System;
using System.Text.Json;

namespace SujaySarma.Data.Azure.Tables.Reflection
{
    /// <summary>Extending the class provided in Core</summary>
    public class ReflectionUtilsExtension
    {
        /// <summary>
        /// List of types that are compatible with Azure Tables
        /// </summary>
        private static readonly Type[] compatibleTypes = new Type[11]
        {
            typeof (string),
            typeof (byte[]),
            typeof (bool),
            typeof (DateTime),
            typeof (DateTimeOffset),
            typeof (double),
            typeof (Guid),
            typeof (int),
            typeof (uint),
            typeof (long),
            typeof (ulong)
        };

        /// <summary>
        /// Ensures that the provided <paramref name="value" /> is compatible with Azure Tables
        /// </summary>
        /// <param name="value">Value to adjust</param>
        /// <param name="ensureType">When set, will check type compatibility with *only* this type</param>
        /// <param name="jsonSerialiseIfNot">When set, will Json-serialise incompatible types. Otherwise will call the ToString() on it.</param>
        /// <returns>The adjusted value. Type may be changed!</returns>
        public static object? EnsureAzureTablesCompatibleValue(object? value, Type? ensureType = null, bool jsonSerialiseIfNot = true)
        {
            if (value == null)
            {
                return value;
            }

            if (ensureType != null)
            {
                return ReflectionUtils.ConvertValueIfRequired(value, ensureType);
            }

            if (value.GetType().IsSupportedType(true, compatibleTypes))
            {
                return value;
            }

            return (jsonSerialiseIfNot ? JsonSerializer.Serialize(value) : value.ToString());
        }
    }
}
