using SujaySarma.Data.Core.Reflection;

using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Text.Json;

namespace SujaySarma.Data.Core
{
    /// <summary>
    /// Functions that retrieve or set values using reflection
    /// </summary>
    public sealed class ReflectionUtils
    {
        /// <summary>
        /// Gets the value of the <paramref name="member" /> member from the <paramref name="instance" />.
        /// </summary>
        /// <param name="instance">Instance of object to retrieve value for</param>
        /// <param name="member">The member to retrieve the value from</param>
        /// <returns>The value, which may be Null.</returns>
        public static object? GetValue(ref object? instance, MemberTypeInfo member)
        {
            object? obj = null;

            if (member.FieldOrPropertyInfo is FieldInfo field)
            {
                obj = field.GetValue(field.IsStatic ? null : instance);
            }
            else if (member.FieldOrPropertyInfo is PropertyInfo property)
            {
                obj = property.GetValue(instance);
            }

            if (((obj == null) || obj.Equals(GetDefaultValue(GetFieldOrPropertyDataType(member.FieldOrPropertyInfo))))
                    && (member.Column.DefaultValueProviderFunction != null))
            {
                obj = member.Column.DefaultValueProviderFunction();
            }

            return obj;
        }

        
        /// <summary>
        /// Sets the value of the specified member for the given instance using reflection.
        /// </summary>
        /// <param name="instance">The object instance from which the value is to be retrieved. Can be null for static members.</param>
        /// <param name="member">The member information containing details about the field or property to retrieve the value from.</param>
        /// <param name="value">The value of the specified member. If the value is null or matches the default value of the member's type, 
        /// and a default value provider function is defined, the default value from the provider function is returned.</param>
        public static void SetValue(ref object instance, MemberTypeInfo member, object? value)
        {
            if (((value == null) || value.Equals(GetDefaultValue(GetFieldOrPropertyDataType(member.FieldOrPropertyInfo))))
                    && (member.Column.DefaultValueProviderFunction != null))
            {
                value = member.Column.DefaultValueProviderFunction();
            }

            if (member.FieldOrPropertyInfo is FieldInfo field)
            {
                SetValue(ref instance, field, value);
            }
            else if (member.FieldOrPropertyInfo is PropertyInfo property)
            {
                SetValue(ref instance, property, value);
            }
        }

        /// <summary>
        /// Sets the value of the specified member for the given instance using reflection.
        /// </summary>
        /// <param name="instance">The object instance for which the value is to be set. Can be null for static members.</param>
        /// <param name="field">The member information containing details about the field to set the value for.</param>
        /// <param name="value">The value to set for the specified member. If the value is null or matches the default value 
        /// of the member's type, and a default value provider function is defined, the default value from the provider function is used.</param>
        public static void SetValue(ref object instance, FieldInfo field, object? value)
        {
            value = ConvertValueIfRequired(value, field.FieldType);
            field.SetValue(instance, value);
        }

        /// <summary>
        /// Sets the value of the specified member for the given instance using reflection.
        /// </summary>
        /// <param name="instance">The object instance for which the value is to be set. Can be null for static members.</param>
        /// <param name="property">The member information containing details about the property to set the value for.</param>
        /// <param name="value">The value to set for the specified member. If the value is null or matches the default value 
        /// of the member's type, and a default value provider function is defined, the default value from the provider function is used.</param>
        public static void SetValue(ref object instance, PropertyInfo property, object? value)
        {
            value = ConvertValueIfRequired(value, property.PropertyType);
            property.SetValue(instance, value);
        }

        /// <summary>
        /// Convert the given <paramref name="value" /> to the <paramref name="targetClrType" /> if a type-conversion is required.
        /// </summary>
        /// <param name="value">Value that might need a conversion</param>
        /// <param name="targetClrType">The target CLR type that we require</param>
        /// <returns>Original or converted value</returns>
        public static object? ConvertValueIfRequired(object? value, Type targetClrType)
        {
            if ((value == null) || (value is DBNull) || (value == DBNull.Value))
            {
                return null;
            }

            Type type = value.GetType();
            Type destinationType = (Nullable.GetUnderlyingType(targetClrType) ?? targetClrType);

            return (!type.FullName!.Equals(destinationType.FullName, StringComparison.Ordinal) ? ConvertTo(destinationType, value) : value);
        }

        /// <summary>
        /// Returns the data type of the Field or Property
        /// </summary>
        /// <param name="memberInfo">A <see cref="FieldInfo" /> or <see cref="PropertyInfo" /></param>
        /// <returns>The type of the underlying member. If the provided <paramref name="memberInfo" /> is not a <see cref="FieldInfo" /> or <see cref="PropertyInfo" />, will return "<see cref="Object" />"</returns>
        public static Type GetFieldOrPropertyDataType(MemberInfo memberInfo)
        {
            return memberInfo switch
            {
                FieldInfo fieldInfo => fieldInfo.FieldType,
                PropertyInfo propertyInfo => propertyInfo.PropertyType,
                _ => typeof(object)
            };
        }

        /// <summary>Convert between types</summary>
        /// <param name="destinationType">CLR Type of destination</param>
        /// <param name="value">The value to convert</param>
        /// <returns>The converted value</returns>
        public static object? ConvertTo(Type destinationType, object value)
        {
            // Use a Type-Converter if available
            TypeConverter converter = TypeDescriptor.GetConverter(destinationType);
            if ((converter != null) && converter.CanConvertFrom(value.GetType()))
            {
                return converter.ConvertFrom(value);
            }

            // Enum conversion to numeric
            if (destinationType.IsEnum && (value is int enumNumericValue))
            {
                // We can assign a numeric value directly to an Enum, so we do not need to parse this and jump through validation hoops!
                return enumNumericValue;
            }

            // Color processing
            if (Type.Equals(destinationType, typeof(Color)) && (value is int colorNumericValue))
            {
                return Color.FromArgb(colorNumericValue);
            }

            // Date/Time stuff
            if ((Type.Equals(destinationType, typeof(DateOnly)) || Type.Equals(destinationType, typeof(TimeOnly))
                || Type.Equals(destinationType, typeof(DateTime)) || Type.Equals(destinationType, typeof(DateTimeOffset)))
                && DateTimeUtilities.TryConvert(value, destinationType, out object? dateTimeConvertedValue))
            {
                return dateTimeConvertedValue;
            }

            // Try using TryParse or Parse if available (prefer TryParse over Parse)
            MethodInfo[] methods = destinationType.GetMethods(BindingFlags.Static | BindingFlags.Public);
            if ((methods != null) && (methods.Length != 0))
            {
                Type type = ((value == null) ? typeof(object) : value.GetType());
                foreach (MethodInfo methodInfo in methods)
                {
                    if (methodInfo.Name.Equals("TryParse"))
                    {
                        ParameterInfo? parameter = methodInfo.GetParameters()?[0];
                        if ((parameter != null) && Type.Equals(parameter.ParameterType, type))
                        {
                            object?[] objArray = new object?[2] { value, null };
                            bool? tryParseResult = (bool?)methodInfo.Invoke(null, objArray);

                            return (((!tryParseResult.HasValue) || (!tryParseResult.Value)) ? null : objArray[1]);
                        }
                    }                    
                    else if (methodInfo.Name.Equals("Parse"))
                    {
                        ParameterInfo? parameter = methodInfo.GetParameters()?[0];
                        if ((parameter != null) && Type.Equals(parameter.ParameterType, type))
                        {
                            return methodInfo.Invoke(null, new object?[1] { value });
                        }
                    }
                }
            }

            // If what we have is a string, is it Json? If so, try deserialising it.
            if ((value is string str) && ((str.StartsWith('{') && str.EndsWith('}')) || (str.StartsWith('[') && str.EndsWith(']'))))
            {
                try
                {
                    return JsonSerializer.Deserialize(str, destinationType) ?? null;
                }
                catch
                {
                    // we don't know what this is supposed to be then? A regular string??
                }
            }

            throw new TypeLoadException($"Could not find type converters for '{destinationType.Name}' type.");
        }

        /// <summary>
        /// Returns the default value of a Type
        /// </summary>
        /// <param name="type">Data type to get default value for</param>
        /// <returns>A new instance of the <paramref name="type" /></returns>
        private static object? GetDefaultValue(Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch
            {
                // Couldnt create an instance, so probably NULL?
                return null;
            }
        }
    }

}
