using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Json;

using SujaySarma.Data.Core.Attributes;
using SujaySarma.Data.Core.TypeDiscovery;

namespace SujaySarma.Data.Core.ReflectionUtilities;

/// <summary>
/// Extension methods that make working with object(?) values easier. Most methods 
/// in this class are NULL-aware and NULL-friendly.
/// </summary>
public static class ValueExtensions
{
    #region --- Nulls and Defaults ---

    /// <summary>
    /// Check if <paramref name="value"/> is really NULL. Includes checks for DBNull and variants.
    /// </summary>
    /// <param name="value">Value to test.</param>
    /// <returns>True if <paramref name="value"/> is NULL or DBNull.</returns>
    public static bool IsNull(this object? value)
        => ((value == null) || (value is DBNull) || (value == DBNull.Value));

    /// <summary>
    /// Get the default value of the provided <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to get a default value for.</param>
    /// <returns>Default value instance of the <see cref="Type"/> of <paramref name="type"/>.</returns>
    public static object? GetDefault(this Type type)
    {
        try
        {
            return Activator.CreateInstance(type);
        }
        catch
        {
            // try fallback
            return default;
        }
    }

    #endregion

    #region --- Value conversions ---

    /// <summary>
    /// Convert the given <paramref name="value" /> to the <paramref name="targetClrType" /> if a type-conversion is required.
    /// </summary>
    /// <param name="value">Value that might need a conversion.</param>
    /// <param name="targetClrType">The target CLR type that we require.</param>
    /// <returns>Original or converted value.</returns>
    public static object? ConvertTo(this object? value, Type targetClrType)
    {
        if (value.IsNull())
        {
            return null;
        }

        Type type = value!.GetType();
        Type destinationType = targetClrType.IfNullableGetActualType();

        if (Type.Equals(type, destinationType))
        {
            return value;
        }

        object knownNonNullValue = value!;
        type = knownNonNullValue.GetType();

        if ((knownNonNullValue is string input) && (input == string.Empty) && (destinationType != typeof(string)))
        {
            // change it to non-NULL default.
            return default!;
        }

        // Try an available type converter.
        TypeConverter converter = TypeDescriptor.GetConverter(destinationType);
        if ((converter is not null) && converter.CanConvertFrom(type))
        {
            return converter.ConvertFrom(knownNonNullValue);
        }

        // Handle ToString().
        if (Type.Equals(destinationType, typeof(string)))
        {
            return $"{knownNonNullValue}";
        }

        // Handle Enums (if incoming value is an INT).
        if (destinationType.IsEnum && (knownNonNullValue is int enumAsInt))
        {
            // Enums can be assigned integer values directly.
            return enumAsInt;
        }

        // Conversions between date/time/offset values.
        if (Type.Equals(destinationType, typeof(DateOnly)) || Type.Equals(destinationType, typeof(TimeOnly))
            || Type.Equals(destinationType, typeof(DateTime)) || Type.Equals(destinationType, typeof(DateTimeOffset)))
        {
            if (ConvertBetweenDatesAndTimes.TryConvert(knownNonNullValue, destinationType, out object? result))
            {
                return result;
            }
        }

        // Try using TryParse or Parse if available (prefer TryParse over Parse)
        MethodInfo[] methods = destinationType.GetMethods(BindingFlags.Static | BindingFlags.Public);
        if ((methods != null) && (methods.Length != 0))
        {
            foreach (MethodInfo methodInfo in methods)
            {
                if (methodInfo.Name.Equals("TryParse"))
                {
                    ParameterInfo? parameter = methodInfo.GetParameters()?[0];
                    if ((parameter != null) && Type.Equals(parameter.ParameterType, type))
                    {
                        object?[] objArray = new object?[2] { knownNonNullValue, null };
                        bool? tryParseResult = (bool?)methodInfo.Invoke(null, objArray);

                        return (((!tryParseResult.HasValue) || (!tryParseResult.Value)) ? null : objArray[1]);
                    }
                }
                else if (methodInfo.Name.Equals("Parse"))
                {
                    ParameterInfo? parameter = methodInfo.GetParameters()?[0];
                    if ((parameter != null) && Type.Equals(parameter.ParameterType, type))
                    {
                        return methodInfo.Invoke(null, new object?[1] { knownNonNullValue });
                    }
                }
            }
        }

        // If value is a string, but destinationType is not, json-deserialise?
        if ((knownNonNullValue is string json)
            && ((json.StartsWith('{') && json.EndsWith('}')) || (json.StartsWith('[') && json.EndsWith(']'))))
        {
            try
            {
                return JsonSerializer.Deserialize(json, destinationType) ?? default;
            }
            catch
            {
                // We don't know what to do now!
            }
        }

        // This will fail on the caller, let them deal with it.
        return knownNonNullValue;
    }

    #endregion

    #region --- Property/field Get Value ---

    /// <summary>
    /// Retrieve the raw value of a property or field <paramref name="member"/> from the provided object <paramref name="instance"/>.
    /// </summary>
    /// <param name="instance">Instance of the entity object to retrieve value from. Cannot be NULL if retrieving the value of a static <see cref="FieldInfo"/>.</param>
    /// <param name="member">A valid instance of a <see cref="PersistenceContainerMemberInfo"/> (Property or Field) to retrieve the value from.</param>
    /// <param name="useAutoPopulate">When set, and if a ISystemPopulatedField attribute is annotated on the member, the value is retrieved via auto-population.</param>
    /// <returns>The value of the requested property or field, may be NULL.</returns>
    public static object? GetValue(this object? instance, PersistenceContainerMemberInfo member, bool useAutoPopulate = true)
    {
        return GetValue(instance, member.Member, useAutoPopulate);
    }

    /// <summary>
    /// Retrieve the raw value of a property or field <paramref name="member"/> from the provided object <paramref name="instance"/>.
    /// </summary>
    /// <param name="instance">Instance of the entity object to retrieve value from. Cannot be NULL if retrieving the value of a static <see cref="FieldInfo"/>.</param>
    /// <param name="member">A valid instance of a <see cref="PropertyInfo"/> (Property) or a <see cref="FieldInfo"/> (Field) to retrieve the value from.</param>
    /// <param name="useAutoPopulate">When set, and if a ISystemPopulatedField attribute is annotated on the member, the value is retrieved via auto-population.</param>
    /// <returns>The value of the requested property or field, may be NULL.</returns>
    public static object? GetValue(this object? instance, MemberInfo member, bool useAutoPopulate = true)
    {
        switch (member)
        {
            case PropertyInfo pi:
                return instance.GetValue(pi, useAutoPopulate);

            case FieldInfo fi:
                return instance.GetValue(fi, useAutoPopulate);

            default:
                throw new NotSupportedException($"GetValue was called on a '{member.MemberType}' type member. Only 'Property' and 'Field' members are supported for this call.");
        }
    }

    /// <summary>
    /// Retrieve the raw value of a property <paramref name="property"/> from the provided object <paramref name="instance"/>.
    /// </summary>
    /// <param name="instance">Instance of the entity object to retrieve value from. Can be NULL for static properties (eg: DateTime.UtcNow, string.Empty)</param>
    /// <param name="property">A valid instance of a <see cref="PropertyInfo"/> (Property) to retrieve the value from.</param>
    /// <param name="useAutoPopulate">When set, and if a ISystemPopulatedField attribute is annotated on the member, the value is retrieved via auto-population.</param>
    /// <returns>The value of the requested property, may be NULL.</returns>
    public static object? GetValue(this object? instance, PropertyInfo property, bool useAutoPopulate = true)
    {
        object? value = property.GetValue(instance);
        if (useAutoPopulate && property.TryGetAttribute(typeof(IOrmPopulatedField), out Attribute? irm))
        {
            value = ((IOrmPopulatedField)irm).GetOrmPopulatedValue(value);
        }

        return value;
    }

    /// <summary>
    /// Retrieve the raw value of a field <paramref name="field"/> from the provided object <paramref name="instance"/>.
    /// </summary>
    /// <param name="instance">Instance of the entity object to retrieve value from. Can be NULL when retrieving values of static fields.</param>
    /// <param name="field">A valid instance of a <see cref="FieldInfo"/> (field) to retrieve the value from.</param>
    /// <param name="useAutoPopulate">When set, and if a ISystemPopulatedField attribute is annotated on the member, the value is retrieved via auto-population.</param>
    /// <returns>The value of the requested property, may be NULL.</returns>
    public static object? GetValue(this object? instance, FieldInfo field, bool useAutoPopulate = true)
    {
        if ((instance is null) && (!field.IsStatic))
        {
            throw new ArgumentNullException(nameof(instance), $"Object instance cannot be NULL when a instance field value is to be retrieved.");
        }

        object? value = field.GetValue(field.IsStatic ? null : instance);
        if (useAutoPopulate && field.TryGetAttribute(typeof(IOrmPopulatedField), out Attribute? irm))
        {
            value = ((IOrmPopulatedField)irm).GetOrmPopulatedValue(value);
        }

        return value;
    }

    #endregion

    #region --- Property/field Set Value ---

    /// <summary>
    /// Set the value of a property or field <paramref name="member"/> from the provided object <paramref name="instance"/>.
    /// </summary>
    /// <param name="instance">Instance of the entity object to set value on. Cannot be NULL if setting the value of a static <see cref="FieldInfo"/>.</param>
    /// <param name="member">A valid instance of a <see cref="PersistenceContainerMemberInfo"/> (Property or Field) to set the value on.</param>
    /// <param name="value">The value to set, may be NULL.</param>
    public static void SetValue(this object? instance, PersistenceContainerMemberInfo member, object? value)
    {
        SetValue(instance, member.Member, value);
    }

    /// <summary>
    /// Set the value of a property or field <paramref name="member"/> from the provided object <paramref name="instance"/>.
    /// </summary>
    /// <param name="instance">Instance of the entity object to set value on. Cannot be NULL if setting the value of a static <see cref="FieldInfo"/>.</param>
    /// <param name="member">A valid instance of a <see cref="PropertyInfo"/> (Property) or a <see cref="FieldInfo"/> (Field) to set the value on.</param>
    /// <param name="value">The value to set, may be NULL.</param>
    public static void SetValue(this object? instance, MemberInfo member, object? value)
    {
        switch (member)
        {
            case PropertyInfo pi:
                SetValue(instance, pi, value);
                break;

            case FieldInfo fi:
                SetValue(instance, fi, value);
                break;

            default:
                throw new NotSupportedException($"SetValue was called on a '{member.MemberType}' type member. Only 'Property' and 'Field' members are supported for this call.");
        }
    }

    /// <summary>
    /// Set the value of a property <paramref name="property"/> on the provided object <paramref name="instance"/>.
    /// </summary>
    /// <param name="instance">Instance of the entity object to set value on. Cannot be NULL if setting the value of a static <see cref="FieldInfo"/>.</param>
    /// <param name="property">A valid instance of a <see cref="PropertyInfo"/> (Property) to set the value on.</param>
    /// <param name="value">The value to set, may be NULL.</param>
    public static void SetValue(this object? instance, PropertyInfo property, object? value)
    {
        if (instance is null)
        {
            throw new ArgumentNullException(nameof(instance), $"Object instance cannot be NULL when a property value is to be set.");
        }

        value = value.ConvertTo(property.PropertyType);
        property.SetValue(instance, value);
    }

    /// <summary>
    /// Set the value of a field <paramref name="field"/> on the provided object <paramref name="instance"/>.
    /// </summary>
    /// <param name="instance">Instance of the entity object to set value on. Cannot be NULL if setting the value of a static <see cref="FieldInfo"/>.</param>
    /// <param name="field">A valid instance of a <see cref="FieldInfo"/> (Field) to set the value on.</param>
    /// <param name="value">The value to set, may be NULL.</param>
    public static void SetValue(this object? instance, FieldInfo field, object? value)
    {
        if ((instance is null) && (!field.IsStatic))
        {
            throw new ArgumentNullException(nameof(instance), $"Object instance cannot be NULL when a instance field value is to be set.");
        }

        value = value.ConvertTo(field.FieldType);
        field.SetValue((field.IsStatic ? null : instance), value);
    }

    #endregion

    #region Miscellaneous functions

    /// <summary>
    /// Materialise an <see cref="IEnumerable{T}"/> into a <see cref="List{T}"/>, with options to skip NULL elements or throw exceptions on NULL elements.
    /// </summary>
    /// <typeparam name="TElement">Type of elements in <paramref name="source"/>.</typeparam>
    /// <param name="source">An enumeration of elements.</param>
    /// <param name="acceptNullElements">[optional] When set (default: TRUE), accepts NULLs as part of the sequence and adds them to the returned list. 
    /// Otherwise, (if <paramref name="throwExceptionOnNull"/> is FALSE) skips them.</param>
    /// <param name="throwExceptionOnNull">[optional] When set (default: FALSE), throws an <see cref="ArgumentNullException"/> when the <paramref name="source"/> 
    /// contains a NULL, causing the enumeration and materialisation to stop.</param>
    /// <returns>A <see cref="List{T}"/> of elements of type <typeparamref name="TElement"/>.</returns>
    public static List<TElement> Materialise<TElement>(this IEnumerable<TElement> source, bool acceptNullElements = true, bool throwExceptionOnNull = false)
    {
        if (source is List<TElement> list)
        {
            if (acceptNullElements)
            {
                return list;
            }

            // Need to filter out NULLs, fall through to loop below.
        }
        else
        {
            if (acceptNullElements)
            {
                return source.ToList<TElement>();
            }
        }

        List<TElement> results = new List<TElement>();
        foreach (TElement element in source)
        {
            if (element is null)
            {
                if (throwExceptionOnNull)
                {
                    throw new ArgumentNullException(nameof(source), $"Enumerable source returned a NULL element, not allowed by caller.");
                }

                continue;
            }

            results.Add(element);
        }

        return results;
    }

    #endregion
}
