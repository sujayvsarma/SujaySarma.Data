using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection;
using System.Text.Json;

namespace SujaySarma.Data.Core.Reflection
{
    /// <summary>
    /// Functions that retrieve or set values using reflection
    /// </summary>
    public sealed class ReflectionUtils
    {

        /// <summary>
        /// Gets the value of the <paramref name="member"/> member from the <paramref name="instance"/>.
        /// </summary>
        /// <param name="instance">Instance of object to retrieve value for</param>
        /// <param name="member">The member to retrieve the value from</param>
        /// <returns>The value, which may be Null.</returns>
        public static object? GetValue(ref object? instance, ContainerMemberTypeInformation member)
        {
            object? value = null;
            if (member.FieldOrPropertyInfo is FieldInfo field)
            {
                value = field.GetValue(field.IsStatic ? null : instance);
            }
            else if (member.FieldOrPropertyInfo is PropertyInfo property)
            {
                // properties are never 'static'
                value = property.GetValue(instance);
            }

            // if value is null or default, try calling the default value function to get a better answer
            if ((value == null) || (value.Equals(GetDefaultValue(GetFieldOrPropertyDataType(member.FieldOrPropertyInfo)))))
            {
                if (member.ContainerMemberDefinition.DefaultValueProviderFunction != null)
                {
                    value = member.ContainerMemberDefinition.DefaultValueProviderFunction();
                }
            }

            return value;
        }

        /// <summary>
        /// Set the value of a property or field
        /// </summary>
        /// <param name="instance">Instance of object</param>
        /// <param name="member">Member property or field</param>
        /// <param name="value">Value to set</param>
        public static void SetValue(ref object instance, ContainerMemberTypeInformation member, object? value)
        {
            // if value is null or default, try calling the default value function to get a better answer
            if ((value == null) || (value == default))
            {
                if (member.ContainerMemberDefinition.DefaultValueProviderFunction != null)
                {
                    value = member.ContainerMemberDefinition.DefaultValueProviderFunction();
                }
            }

            if (member.FieldOrPropertyInfo is FieldInfo field)
            {
                value = ConvertValueIfRequired(value, field.FieldType);
                field.SetValue(instance, value);
            }
            else if (member.FieldOrPropertyInfo is PropertyInfo property)
            {
                value = ConvertValueIfRequired(value, property.PropertyType);
                property.SetValue(instance, value);
            }
        }

        /// <summary>
        /// Set the value of a property or field
        /// </summary>
        /// <param name="instance">Instance of object</param>
        /// <param name="field">Member property or field</param>
        /// <param name="value">Value to set</param>
        public static void SetValue(ref object instance, FieldInfo field, object? value)
        {
            value = ConvertValueIfRequired(value, field.FieldType);
            field.SetValue(instance, value);
        }

        /// <summary>
        /// Set the value of a property or field
        /// </summary>
        /// <param name="instance">Instance of object</param>
        /// <param name="property">Member property or field</param>
        /// <param name="value">Value to set</param>
        public static void SetValue(ref object instance, PropertyInfo property, object? value)
        {
            value = ConvertValueIfRequired(value, property.PropertyType);
            property.SetValue(instance, value);
        }

        /// <summary>
        /// Convert the given <paramref name="value"/> to the <paramref name="targetClrType"/> if a type-conversion is required.
        /// </summary>
        /// <param name="value">Value that might need a conversion</param>
        /// <param name="targetClrType">The target CLR type that we require</param>
        /// <returns>Original or converted value</returns>        
        [return: NotNullIfNotNull(nameof(value))]
        public static object? ConvertValueIfRequired(object? value, Type targetClrType)
        {
            if ((value == default) || (value == DBNull.Value))
            {
                return default;
            }

            Type sourceRegistryType = value.GetType();
            Type? actualClrType = Nullable.GetUnderlyingType(targetClrType);
            Type destinationClrType = actualClrType ?? targetClrType;

            if (!sourceRegistryType.FullName!.Equals(destinationClrType.FullName, StringComparison.Ordinal))
            {
                // we need to convert
                return ConvertTo(destinationClrType, value);
            }
            return value;
        }

        /// <summary>
        /// Returns the data type of the Field or Property
        /// </summary>
        /// <param name="memberInfo">A <see cref="FieldInfo"/> or <see cref="PropertyInfo"/></param>
        /// <returns>The type of the underlying member. If the provided <paramref name="memberInfo"/> is not a <see cref="FieldInfo"/> or <see cref="PropertyInfo"/>, will return "<see cref="object"/>"</returns>
        public static Type GetFieldOrPropertyDataType(MemberInfo memberInfo)
        {
            if (memberInfo is FieldInfo field)
            {
                return field.FieldType;
            }
            else if (memberInfo is PropertyInfo property)
            {
                return property.PropertyType;
            }

            return typeof(object);
        }

        /// <summary>
        /// Convert between types
        /// </summary>
        /// <param name="destinationType">CLR Type of destination</param>
        /// <param name="value">The value to convert</param>
        /// <returns>The converted value</returns>
        public static object ConvertTo(Type destinationType, object value)
        {
            //NOTE: value is not null -- already been checked by caller before calling here
            TypeConverter converter = TypeDescriptor.GetConverter(destinationType);
            if ((converter != null) && converter.CanConvertFrom(value.GetType()))
            {
                return converter.ConvertFrom(value)!;
            }

            // type string is handled by the OOTB EnumConverter
            if (destinationType.IsEnum && (value is int intVal))
            {
                return intVal;
            }

            // type string is handled by the OOTB ColorConverter
            if ((destinationType == typeof(Color)) && (value is int colourHex))
            {
                return Color.FromArgb(colourHex);
            }

            // Pass it to our (rather verbose!) converter system
            if ((destinationType == typeof(DateOnly)) || (destinationType == typeof(TimeOnly)) || (destinationType == typeof(DateTime)) || (destinationType == typeof(DateTimeOffset)))
            {
                if (DateTimeUtilities.TryConvert(value, destinationType, out var result))
                {
                    return result;
                }
            }

            // see if type has a Parse or a TryParse static method
            MethodInfo[] methods = destinationType.GetMethods(BindingFlags.Public | BindingFlags.Static);
            if ((methods != null) && (methods.Length > 0))
            {
                Type sourceType = ((value == null) ? typeof(object) : value.GetType());
                foreach (MethodInfo m in methods)
                {
                    if (m.Name.Equals("Parse"))
                    {
                        ParameterInfo? p = m.GetParameters()?[0];
                        if ((p != null) && (p.ParameterType == sourceType))
                        {
                            return m.Invoke(null, new object?[] { value })!;
                        }
                    }
                    else if (m.Name.Equals("TryParse"))
                    {
                        ParameterInfo? p = m.GetParameters()?[0];
                        if ((p != null) && (p.ParameterType == sourceType))
                        {
                            object?[]? parameters = new object?[] { value, null };
                            bool? tpResult = (bool?)m.Invoke(null, parameters);
                            return ((tpResult.HasValue && (tpResult.Value == true)) ? parameters[1] : default!)!;
                        }
                    }
                }
            }

            // Fall through for Json-serialised data
            if (value is string potentialJson)
            {
                try
                {
                    // Jsonised?
                    return JsonSerializer.Deserialize(potentialJson, destinationType) ?? default!;
                }
                catch
                {
                    // this will throw below
                }
            }

            throw new TypeLoadException($"Could not find type converters for '{destinationType.Name}' type.");
        }

        /// <summary>
        /// Returns the default value of a Type
        /// </summary>
        /// <param name="type">Data type to get default value for</param>
        /// <returns>A new instance of the <paramref name="type"/></returns>
        private static object? GetDefaultValue(Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch
            {
                // Will fail for things like strings...
                return default;
            }
        }
    }
}
