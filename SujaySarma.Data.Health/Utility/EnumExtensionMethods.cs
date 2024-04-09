using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SujaySarma.Data.Health.Utility
{
    /// <summary>
    /// Defines methods that can be used with Enum types
    /// </summary>
    public static class EnumExtensionMethods
    {

        /// <summary>
        /// Deconstructs a flag composed of multiple values into a list of its components.
        /// </summary>
        /// <typeparam name="TEnum">A type of Enum</typeparam>
        /// <param name="flags">The composite flag value</param>
        /// <returns>List of component flag values</returns>
        public static List<TEnum> ResolveFlagMembers<TEnum>(TEnum flags)
            where TEnum : Enum
        {
            List<TEnum> components = new List<TEnum>();
            foreach (TEnum value in Enum.GetValues(typeof(TEnum)))
            {
                if (flags.HasFlag(value))
                {
                    components.Add(value);
                }
            }
            return components;
        }


        /// <summary>
        /// Get the annotated name and description for the provided enum value
        /// </summary>
        /// <typeparam name="TEnum">A type of Enum</typeparam>
        /// <param name="value">Value to get name and description for</param>
        /// <returns>A typed-Tuple with the name and description values. Missing items will be string.Empty</returns>
        public static (string Name, string Description) GetNameAndDescription<TEnum>(TEnum value)
            where TEnum : Enum
        {
            return GetNameAndDescription(typeof(TEnum), value);
        }

        /// <summary>
        /// Get the annotated name and description for the provided enum value
        /// </summary>
        /// <param name="TEnum">Type of Enum</param>
        /// <param name="value">Value to get name and description for</param>
        /// <returns>A typed-Tuple with the name and description values. Missing items will be string.Empty</returns>
        public static (string Name, string Description) GetNameAndDescription(Type TEnum, object value)
        {
            string v = $"{value}";

            FieldInfo? fieldInfo = TEnum.GetField(v);
            if (fieldInfo == default)
            {
                return (Name: "Invalid", Description: "Invalid value assignment");
            }

            DisplayAttribute? attrib = fieldInfo.GetCustomAttribute<DisplayAttribute>();
            string name = Enum.GetName(TEnum, value) ?? string.Empty, description = string.Empty;
            if (attrib != null)
            {
                if (attrib.Name != null)
                {
                    name = attrib.Name;
                }

                if (attrib.Description != null)
                {
                    description = attrib.Description;
                }
            }

            return (Name: name, Description: description);
        }
    }
}
