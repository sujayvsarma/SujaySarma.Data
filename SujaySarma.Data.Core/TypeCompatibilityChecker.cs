
using System;
using System.Collections.Generic;
using System.Linq;

namespace SujaySarma.Data.Core
{
    /// <summary>
    /// Provides mechanisms to check compatibility of data types against different collections
    /// </summary>
    public static class TypeCompatibilityChecker
    {
        /// <summary>
        /// Check if the <paramref name="nullableType" /> is the nullable equivalent of <paramref name="nonNullableEquivalent" />.
        /// </summary>
        /// <param name="nullableType">A Nullable type</param>
        /// <param name="nonNullableEquivalent">A non-Nullable type</param>
        /// <returns>True or False</returns>
        /// <example>
        ///     if (typeof(int?).IsNullableEquivalentOf(typeof(int))) { /* ... */ }
        /// </example>
        public static bool IsNullableEquivalentOf(this Type nullableType, Type nonNullableEquivalent)
            => Type.Equals(Nullable.GetUnderlyingType(nullableType), nonNullableEquivalent);

        /// <summary>
        /// Check if the <paramref name="typeToCheck" /> type is among the <paramref name="supportedTypes" /> collection
        /// </summary>
        /// <param name="typeToCheck">The type to test</param>
        /// <param name="checkNullableEquivalents">When set, also compares against nullable equivalents of types in the <paramref name="supportedTypes" /> collection</param>
        /// <param name="supportedTypes">The collection of types we declare to support</param>
        /// <returns>True if <paramref name="typeToCheck" /> is among <paramref name="supportedTypes" /></returns>
        public static bool IsSupportedType(this Type typeToCheck, bool checkNullableEquivalents, params Type[] supportedTypes)
            => typeToCheck.IsSupportedType(checkNullableEquivalents, supportedTypes.ToList());

        /// <summary>
        /// Check if the <paramref name="typeToCheck" /> type is among the <paramref name="supportedTypes" /> collection
        /// </summary>
        /// <param name="typeToCheck">The type to test</param>
        /// <param name="checkNullableEquivalents">When set, also compares against nullable equivalents of types in the <paramref name="supportedTypes" /> collection</param>
        /// <param name="supportedTypes">The collection of types we declare to support</param>
        /// <returns>True if <paramref name="typeToCheck" /> is among <paramref name="supportedTypes" /></returns>
        public static bool IsSupportedType(this Type typeToCheck, bool checkNullableEquivalents, IEnumerable<Type> supportedTypes)
        {
            foreach (Type supportedType in supportedTypes)
            {
                if (Type.Equals(supportedType, typeToCheck) || (checkNullableEquivalents && typeToCheck.IsNullableEquivalentOf(supportedType)))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
