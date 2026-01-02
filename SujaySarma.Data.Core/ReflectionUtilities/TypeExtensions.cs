using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SujaySarma.Data.Core.ReflectionUtilities;

/// <summary>
/// Extension methods that provide <see cref="Type"/> related functionality.
/// </summary>
public static class TypeExtensions
{
    #region --- Type Checks ---

    /// <summary>
    /// Get a usable name of the type. This is AssemblyQualifiedName if available, else FullName, else Name.
    /// </summary>
    /// <param name="type"><see cref="Type"/> to retrieve the name for.</param>
    /// <returns>String containing the AssemblyQualifiedName if available, else FullName, else Name.</returns>
    public static string GetUsableTypeName(this Type type)
        => type.FullName ?? type.Name;

    /// <summary>
    /// <see cref="Nullable"/> types hide the actual type behind a generic-like definition. If the 
    /// provided <paramref name="type"/> is nullable, retrieve the actual <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to investigate.</param>
    /// <returns>The actual Type. If <paramref name="type"/> is not nullable, then it is returned as-is.</returns>
    public static Type IfNullableGetActualType(this Type type)
    {
        return Nullable.GetUnderlyingType(type) ?? type;
    }

    /// <summary>
    /// Determine if the given <paramref name="type"/> is a class, record or struct.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to examine.</param>
    /// <returns>True if the <paramref name="type"/> is a class, record or struct.</returns>
    public static bool IsClassRecordOrStruct(this Type type)
    {
        if (type.IsInterface || type.IsEnum)
        {
            return false;
        }

        if (type.IsClass)
        {
            if (typeof(MulticastDelegate).IsAssignableFrom(type.BaseType))
            {
                return false;
            }

            // is a class or a record class.
            return true;
        }

        if (type.IsValueType)
        {
            if (type.IsPrimitive)
            {
                return false;
            }

            // is a struct or a record struct.
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check if the <paramref name="type"/> is an enumerable type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static bool IsEnumerableType(this Type? type)
    {
        if (type is null)
        {
            // NULLs are never enumerable!
            return false;
        }

        if (type == typeof(string))
        {
            // A string is enumerable (to chars), but we dont usually want to 
            // treat a string as a "collection". So, NAH!
            return false;
        }

        // Arrays - always enumerable!
        if (type.IsArray)
        {
            return true;
        }

        // Whether IEnumerable or IEnumerable<T>, they have to implement the 
        // GetEnumerator() -- which is also a check that C# itself makes!
        return ((type.GetMethod("GetEnumerator", BindingFlags.Public | BindingFlags.Instance) is not null)
            ? true : false);
    }

    /// <summary>
    /// Checks if the provided type is a numeric type. Numeric types are: int, short, long, etc. including their
    /// unsigned versions.
    /// </summary>
    /// <param name="type">Type to check.</param>
    /// <returns>True if is a numeric type.</returns>
    public static bool IsNumericType(this Type? type)
    {
        if (type is null)
        {
            return false;
        }

        return
            Type.Equals(type, typeof(Math))
            || Type.Equals(type, typeof(sbyte))
            || Type.Equals(type, typeof(byte))
            || Type.Equals(type, typeof(short))
            || Type.Equals(type, typeof(ushort))
            || Type.Equals(type, typeof(int))
            || Type.Equals(type, typeof(uint))
            || Type.Equals(type, typeof(long))
            || Type.Equals(type, typeof(ulong))
            || Type.Equals(type, typeof(float))
            || Type.Equals(type, typeof(float))
            || Type.Equals(type, typeof(Decimal))
            || Type.Equals(type, typeof(double));
    }

    /// <summary>
    /// Returns if the provided <paramref name="type"/> is a Nullable type. 
    /// NOTE: This WILL return FALSE for "string?"! This is by design!
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to investigate.</param>
    /// <returns>True if the type is NULL.</returns>
    public static bool IsNullable(this Type type)
    {
        return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    /// <summary>
    /// Check if the <paramref name="type" /> is the nullable equivalent of <paramref name="nonNullableEquivalent" />.
    /// </summary>
    /// <param name="type">A Nullable type.</param>
    /// <param name="nonNullableEquivalent">A non-Nullable type.</param>
    /// <returns>True or False.</returns>
    /// <example>
    ///     if (typeof(int?).IsNullableEquivalentOf(typeof(int))) { /* ... */ }
    /// </example>
    public static bool IsNullableEquivalentOf(this Type type, Type nonNullableEquivalent)
        => type.IsNullable() && Type.Equals(Nullable.GetUnderlyingType(type), nonNullableEquivalent);

    /// <summary>
    /// Checks if <paramref name="type"/> (the 'this' argument) is the same as, or a subclass of, <paramref name="baseType"/>.
    /// </summary>
    /// <param name="type">The type (the 'this' argument) to check for equality or inheritance.</param>
    /// <param name="baseType">The base type to compare against.</param>
    /// <returns>True if <paramref name="type"/> (the 'this' argument) is the same as or inherits from <paramref name="baseType"/>.</returns>
    public static bool IsOrIsDerivedFrom(this Type type, Type baseType)
    {
        // Remember: Animal.IsAssignableFrom(Dog)
        return baseType.IsAssignableFrom(type);
    }

    /// <summary>
    /// Check if the <paramref name="type" /> type is among the <paramref name="supportedTypes" /> collection.
    /// </summary>
    /// <param name="type">The type to test.</param>
    /// <param name="checkNullableEquivalents">When set, also compares against nullable equivalents of types in the <paramref name="supportedTypes" /> collection.</param>
    /// <param name="supportedTypes">The collection of types we declare to support.</param>
    /// <returns>True if <paramref name="type" /> is among <paramref name="supportedTypes" />.</returns>
    public static bool IsSupportedType(this Type type, bool checkNullableEquivalents, params Type[] supportedTypes)
        => type.IsSupportedType(checkNullableEquivalents, (IEnumerable<Type>)supportedTypes);

    /// <summary>
    /// Check if the <paramref name="type" /> type is among the <paramref name="supportedTypes" /> collection.
    /// </summary>
    /// <param name="type">The type to test.</param>
    /// <param name="checkNullableEquivalents">When set, also compares against nullable equivalents of types in the <paramref name="supportedTypes" /> collection.</param>
    /// <param name="supportedTypes">The collection of types we declare to support.</param>
    /// <returns>True if <paramref name="type" /> is among <paramref name="supportedTypes" />.</returns>
    public static bool IsSupportedType(this Type type, bool checkNullableEquivalents, IEnumerable<Type> supportedTypes)
    {
        foreach (Type supportedType in supportedTypes)
        {
            if (Type.Equals(supportedType, type) || (checkNullableEquivalents && type.IsNullableEquivalentOf(supportedType)))
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Extracts entity types from a generic expression type (e.g., Expression&lt;Func&lt;Person, bool&gt;&gt; → [Person, bool]).
    /// </summary>
    /// <param name="expressionType">The type of the expression (usually from expression.GetType()).</param>
    /// <returns>Array of entity types contained in the generic arguments, or empty array if not generic.</returns>
    public static Type[] GetEntityTypesFromExpression(this Type expressionType)
    {
        if (!expressionType.IsGenericType)
        {
            return new Type[] { expressionType };
        }

        Type[] genericArguments = expressionType.GetGenericArguments();

        // If we have one generic argument that is itself generic (e.g., Func<Person, bool>),
        // unwrap it to get the inner types
        if ((genericArguments.Length == 1) && genericArguments[0].IsGenericType)
        {
            return genericArguments[0].GetGenericArguments();
        }

        return genericArguments;
    }

    /// <summary>
    /// Finds a matching type from a collection of candidate types that is assignable from the target type.
    /// </summary>
    /// <param name="targetType">The type to match against (e.g., the declaring type of a member).</param>
    /// <param name="candidateTypes">Collection of types to search through.</param>
    /// <param name="matchedType">[out] The first matching type, or null if no match found.</param>
    /// <returns>True if a matching type was found.</returns>
    public static bool TryFindAssignableType(this Type targetType, Type[] candidateTypes, [NotNullWhen(true)] out Type? matchedType)
    {
        foreach (Type candidateType in candidateTypes)
        {
            if (targetType.IsAssignableFrom(candidateType))
            {
                matchedType = candidateType;
                return true;
            }
        }

        matchedType = null;
        return false;
    }


    #endregion

    #region --- Attributes ---

    /// <summary>
    /// Try to get an attribute of type <paramref name="attributeType"/> from the type <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type that may be anotated with the attribute.</param>
    /// <param name="attributeType">Type of attribute to find.</param>
    /// <param name="attribute">[out] The attribute if found, else NULL.</param>
    /// <returns>True if the attribute was found.</returns>
    public static bool TryGetAttribute(this Type type, Type attributeType, [NotNullWhen(true)] out Attribute? attribute)
    {
        object[] attributes = type.GetCustomAttributes(attributeType, inherit: true);
        if ((attributes.Length > 0) && attributes[0].GetType().IsOrIsDerivedFrom(attributeType))
        {
            attribute = (Attribute)attributes[0];
            return true;
        }

        attribute = null;
        return false;
    }

    /// <summary>
    /// Try to get an attribute of type <paramref name="attributeType"/> from the type <paramref name="memberInfo"/>.
    /// </summary>
    /// <param name="memberInfo">MemberInfo (PropertyInfo or FieldInfo) of a class/struct/record property or field 
    /// that may be anotated with the attribute.</param>
    /// <param name="attributeType">Type of attribute to find.</param>
    /// <param name="attribute">[out] The attribute if found, else NULL.</param>
    /// <returns>True if the attribute was found.</returns>
    public static bool TryGetAttribute(this MemberInfo memberInfo, Type attributeType, [NotNullWhen(true)] out Attribute? attribute)
    {
        object[] attributes = memberInfo.GetCustomAttributes(attributeType, inherit: true);
        if ((attributes.Length > 0) && attributes[0].GetType().IsOrIsDerivedFrom(attributeType))
        {
            attribute = (Attribute)attributes[0];
            return true;
        }

        attribute = null;
        return false;
    }

    /// <summary>
    /// Try to get an attribute of type <typeparamref name="T"/> from the type <paramref name="type"/>.
    /// </summary>
    /// <typeparam name="T">The type of attribute to find.</typeparam>
    /// <param name="type">The type that may be decorated with the attribute.</param>
    /// <param name="attribute">[out] The attribute if found, else NULL.</param>
    /// <returns>True if the attribute was found.</returns>
    public static bool TryGetAttribute<T>(this Type type, [NotNullWhen(true)] out T? attribute)
    {
        Type typeOfT = typeof(T);
        if (TryGetAttribute(type, typeOfT, out Attribute? attrib))
        {
            attribute = (T)(object)attrib;
            return true;
        }

        attribute = default(T);
        return false;
    }

    /// <summary>
    /// Try to get an attribute of type <typeparamref name="T"/> from the type <paramref name="memberInfo"/>.
    /// </summary>
    /// <typeparam name="T">The type of attribute to find.</typeparam>
    /// <param name="memberInfo">MemberInfo (PropertyInfo or FieldInfo) of a class/struct/record property or field 
    /// that may be decorated with the attribute.</param>
    /// <param name="attribute">[out] The attribute if found, else NULL.</param>
    /// <returns>True if the attribute was found.</returns>
    public static bool TryGetAttribute<T>(this MemberInfo memberInfo, [NotNullWhen(true)] out T? attribute)
    {
        Type typeOfT = typeof(T);
        if (TryGetAttribute(memberInfo, typeOfT, out Attribute? attrib))
        {
            attribute = (T)(object)attrib;
            return true;
        }

        attribute = default(T);
        return false;
    }

    #endregion

    #region --- Properties and fields ---

    /// <summary>
    /// Try to retrieve a specific property by name.
    /// </summary>
    /// <param name="type">The type containing the property.</param>
    /// <param name="name">Name of the property (case sensitive)</param>
    /// <param name="bindingFlags">BindingFlags to use to filter.</param>
    /// <param name="propertyInfo">[out] The PropertyInfo if found -- otherwise will be NULL.</param>
    /// <returns>True if the property was found.</returns>
    public static bool TryGetProperty(this Type type, string name, BindingFlags bindingFlags, [NotNullWhen(true)] out PropertyInfo? propertyInfo)
    {
        propertyInfo = type.GetProperty(name, bindingFlags);
        return (propertyInfo != null);
    }

    /// <summary>
    /// Try to retrieve a specific field by name.
    /// </summary>
    /// <param name="type">The type containing the field.</param>
    /// <param name="name">Name of the field (case sensitive)</param>
    /// <param name="bindingFlags">BindingFlags to use to filter.</param>
    /// <param name="fieldInfo">[out] The FieldInfo if found -- otherwise will be NULL.</param>
    /// <returns>True if the field was found.</returns>
    public static bool TryGetField(this Type type, string name, BindingFlags bindingFlags, [NotNullWhen(true)] out FieldInfo? fieldInfo)
    {
        fieldInfo = type.GetField(name, bindingFlags);
        return (fieldInfo != null);
    }

    /// <summary>
    /// Try to retrieve the data type of the member property or field represented by <paramref name="memberInfo"/>. 
    /// NOTE: We support **only** properties and fields!
    /// </summary>
    /// <param name="memberInfo">The <see cref="MemberInfo"/> metadata about a member property or field.</param>
    /// <param name="type">The data type of the property or field.</param>
    /// <returns>True if the type could be determined. False if not.</returns>
    public static bool TryGetPropertyOrFieldDataType(this MemberInfo memberInfo, [NotNullWhen(true)] out Type? type)
    {
        switch (memberInfo)
        {
            case PropertyInfo pi:
                type = pi.PropertyType;
                return true;

            case FieldInfo fi:
                type = fi.FieldType;
                return true;

            default:
                type = null;
                return false;
        }
    }

    #endregion
}
