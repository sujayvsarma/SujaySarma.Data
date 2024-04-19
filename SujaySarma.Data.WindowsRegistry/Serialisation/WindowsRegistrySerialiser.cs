using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.Json;

using Microsoft.Win32;

using SujaySarma.Data.Core.Reflection;
using SujaySarma.Data.WindowsRegistry.Attributes;

namespace SujaySarma.Data.WindowsRegistry.Serialisation
{
    /// <summary>
    /// Serialises and deserialises data between the Windows Registry 
    /// and your .NET class, structure or record.
    /// </summary>
    public static class WindowsRegistrySerialiser
    {

        /// <summary>
        /// Deserialise or retrieve the values from the registry pointed at by the <typeparamref name="TObject"/>.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <returns>Instance of type <typeparamref name="TObject"/> populated with data from the registry</returns>
        public static TObject Deserialise<TObject>()
        {
            ContainerTypeInformation metadata = TypeDiscoveryFactory.Resolve<TObject>() ?? throw new TypeLoadException($"Type '{typeof(TObject).Name}' is not appropriately decorated.");
            return (TObject)DeserialiseImpl(metadata, typeof(TObject));
        }

        /// <summary>
        /// Deserialise or retrieve the values from the registry pointed at by the type <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Type of .NET class, structure or recrd</param>
        /// <returns>Instance of type <paramref name="type"/> populated with data from the registry</returns>
        public static object Deserialise(Type type)
        {
            ContainerTypeInformation metadata = TypeDiscoveryFactory.Resolve(type) ?? throw new TypeLoadException($"Type '{type.Name}' is not appropriately decorated.");
            return DeserialiseImpl(metadata, type);
        }

        /// <summary>
        /// Implementation of deserialisation
        /// </summary>
        /// <param name="metadata">Type-discovered metadata about the .NET class, structure or record</param>
        /// <param name="type">The type of .NET class, structure or record (metadata does NOT contain this info!)</param>
        /// <returns>Instance a .NET class, structure or record populated with data from the registry</returns>
        private static object DeserialiseImpl(ContainerTypeInformation metadata, Type type)
        {
            ThrowIfNotRegistryBacked(metadata);
            object instance = Activator.CreateInstance(type)
                                ?? throw new TypeLoadException($"Unable to create an instance of type '{metadata.Name}'.");

            RegistryKey key = ((RegistryKeyNameAttribute)metadata.ContainerDefinition).Key;

            List<string> keyValueNames = new List<string>();
            keyValueNames.AddRange(key.GetValueNames());

            ContainerMemberTypeInformation? keyPairMember = null;
            Type? keyPairMemberType = null;

            foreach (ContainerMemberTypeInformation member in metadata.Members.Values)
            {
                if (member.ContainerMemberDefinition is RegistryValueNamePairAttribute vna)
                {
                    string valueName = vna.CreateQualifiedName();
                    if (keyValueNames.Any(k => k.Equals(valueName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        object keyValue = key.GetValue(valueName) ?? default!;
                        ReflectionUtils.SetValue(ref instance, member, keyValue);
                        keyValueNames.Remove(valueName);
                    }
                }
                else if (member.ContainerMemberDefinition is RegistryValueNamePairAttribute pair)
                {
                    if (keyPairMember != null)
                    {
                        throw new TypeLoadException($"Object '{metadata.Name}' has more than one property or field of type '{nameof(RegistryValueNamePairAttribute)}'. Only ONE is permitted.");
                    }

                    keyPairMember = member;
                    keyPairMemberType = ReflectionUtils.GetFieldOrPropertyDataType(member.FieldOrPropertyInfo);
                    if (keyPairMemberType.IsGenericType)
                    {
                        keyPairMemberType = keyPairMemberType.GetGenericTypeDefinition();
                        if ((keyPairMemberType == typeof(KeyValuePair<,>)) || (keyPairMemberType == typeof(Dictionary<,>)))
                        {
                            keyPairMember = member;
                        }
                    }
                }
            }

            if ((keyPairMember != null) && (keyPairMemberType != null))
            {
                if (keyPairMemberType == typeof(KeyValuePair<,>))
                {
                    FieldInfo? keyField = keyPairMemberType.GetField("Key");
                    FieldInfo? valueField = keyPairMemberType.GetField("Value");
                    if ((keyField != null) && (valueField != null))
                    {
                        string selectedKeyName = keyValueNames[0];
                        ReflectionUtils.SetValue(ref instance, keyField, selectedKeyName);

                        object? value = key.GetValue(selectedKeyName) ?? ((keyPairMember.ContainerMemberDefinition.DefaultValueProviderFunction != null) ? keyPairMember.ContainerMemberDefinition.DefaultValueProviderFunction() : null);
                        ReflectionUtils.SetValue(ref instance, valueField, value);
                    }
                }
                else if (keyPairMemberType == typeof(Dictionary<,>))
                {
                    // We need the generic defs from original type, not the TypeDef!
                    Type originalDictionaryType = ReflectionUtils.GetFieldOrPropertyDataType(keyPairMember.FieldOrPropertyInfo);
                    Type[] pairTypes = originalDictionaryType.GetGenericArguments();
                    Type keyType = pairTypes[0], valueType = pairTypes[1];

                    var dict = Activator.CreateInstance(originalDictionaryType);

                    MethodInfo dictionaryAddMethod = originalDictionaryType.GetMethod("Add")!;
                    foreach (string subKeyName in keyValueNames)
                    {
                        object value = key.GetValue(subKeyName) ?? default!;
                        _ = dictionaryAddMethod.Invoke(
                                dict,
                                new object?[]
                                {
                                    ReflectionUtils.ConvertValueIfRequired(subKeyName, keyType)!,
                                    ReflectionUtils.ConvertValueIfRequired(value, valueType)!
                                }
                            );
                    }

                    ReflectionUtils.SetValue(ref instance, keyPairMember, dict);
                }
            }

            return instance;
        }

        /// <summary>
        /// Saves the values in the <paramref name="instance"/>
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="instance">Instance of object with the data to be saved into the registry</param>
        public static void Serialise<TObject>(TObject instance)
        {
            if (instance == null)
            {
                // we cannot type-discover a null object!
                return;
            }

            ContainerTypeInformation metadata = TypeDiscoveryFactory.Resolve<TObject>() ?? throw new TypeLoadException($"Type '{typeof(TObject).Name}' is not appropriately decorated.");
            SerialiseImpl(metadata, instance);
        }

        /// <summary>
        /// Saves the values in the <paramref name="instance"/>
        /// </summary>
        /// <param name="instance">Instance of object with the data to be saved into the registry</param>
        public static void Serialise(object? instance)
        {
            if (instance == null)
            {
                // we cannot type-discover a null object!
                return;
            }

            ContainerTypeInformation metadata = TypeDiscoveryFactory.Resolve(instance.GetType()) ?? throw new TypeLoadException($"Type '{instance.GetType().Name}' is not appropriately decorated.");
            SerialiseImpl(metadata, instance);
        }

        /// <summary>
        /// Implementation function to serialise (persist) data
        /// </summary>
        /// <param name="metadata">Type-discovered information about the .NET class, structure or record</param>
        /// <param name="instance">Instance of object with the data to be saved into the registry</param>
        private static void SerialiseImpl(ContainerTypeInformation metadata, object instance)
        {
            ThrowIfNotRegistryBacked(metadata);
            RegistryKey key = ((RegistryKeyNameAttribute)metadata.ContainerDefinition).Key;
            foreach (ContainerMemberTypeInformation member in metadata.Members.Values)
            {
                object? memberValue = ReflectionUtils.GetValue(ref instance!, member);

                if (member.ContainerMemberDefinition is RegistryValueNameAttribute vna)
                {
                    PersistTo(key, vna.CreateQualifiedName(), memberValue);
                }
                else if (member.ContainerMemberDefinition is RegistryValueNamePairAttribute pair)
                {
                    // Easiest way is to get Json to do stuff for us
                    string jsonised = JsonSerializer.Serialize(memberValue);
                    Dictionary<string, object> dictised = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonised)!;

                    // Now we split strategy!
                    Type kvpOrDict = ReflectionUtils.GetFieldOrPropertyDataType(member.FieldOrPropertyInfo);
                    if (kvpOrDict.IsGenericType)
                    {
                        kvpOrDict = kvpOrDict.GetGenericTypeDefinition();
                        if (kvpOrDict == typeof(KeyValuePair<,>))
                        {
                            // We will have two entries:
                            string valueName = (string)dictised["Key"];
                            object? value = dictised["Value"];

                            PersistTo(key, valueName, value);
                        }
                        else if (kvpOrDict == typeof(Dictionary<,>))
                        {
                            foreach (KeyValuePair<string, object> item in dictised)
                            {
                                PersistTo(key, item.Key, item.Value);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Deletes the values that were inserted by the instance of <paramref name="instance"/>
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="instance">Instance of type <typeparamref name="TObject"/> with the data that is to be deleted. Data not matching these values will NOT be deleted.</param>
        public static void Delete<TObject>(TObject instance)
        {
            if (instance == null)
            {
                // we cannot type-discover a null object!
                return;
            }

            ContainerTypeInformation metadata = TypeDiscoveryFactory.Resolve<TObject>() ?? throw new TypeLoadException($"Type '{typeof(TObject).Name}' is not appropriately decorated.");
            DeleteImpl(metadata, instance);
        }

        /// <summary>
        /// Deletes the values that were inserted by the instance of <paramref name="instance"/>
        /// </summary>
        /// <param name="instance">Instance of a .NET class, structure or record with the data that is to be deleted. Data not matching these values will NOT be deleted.</param>
        public static void Delete(object? instance)
        {
            if (instance == null)
            {
                // we cannot type-discover a null object!
                return;
            }

            ContainerTypeInformation metadata = TypeDiscoveryFactory.Resolve(instance.GetType()) ?? throw new TypeLoadException($"Type '{instance.GetType().Name}' is not appropriately decorated.");
            DeleteImpl(metadata, instance);
        }


        /// <summary>
        /// Implementation of the Delete function.
        /// </summary>
        /// <param name="metadata">Type-discovered metadata about the type of <paramref name="instance"/></param>
        /// <param name="instance">Instance with information to delete</param>
        private static void DeleteImpl(ContainerTypeInformation metadata, object instance)
        {
            ThrowIfNotRegistryBacked(metadata);
            RegistryKeyNameAttribute keyAttribute = (RegistryKeyNameAttribute)metadata.ContainerDefinition;
            foreach (ContainerMemberTypeInformation member in metadata.Members.Values)
            {
                if (member.ContainerMemberDefinition is RegistryValueNameAttribute vna)
                {
                    keyAttribute.Key.DeleteValue(vna.CreateQualifiedName());
                }
                else if (member.ContainerMemberDefinition is RegistryValueNamePairAttribute pair)
                {
                    object? regKeyPairItemInstance = ReflectionUtils.GetValue(ref instance!, member);
                    if (regKeyPairItemInstance != null)
                    {
                        // Easiest way is to get Json to do stuff for us
                        string jsonised = JsonSerializer.Serialize(regKeyPairItemInstance);
                        Dictionary<string, object> dictised = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonised)!;

                        // Now we split strategy!
                        Type kvpOrDict = ReflectionUtils.GetFieldOrPropertyDataType(member.FieldOrPropertyInfo);
                        if (kvpOrDict.IsGenericType)
                        {
                            kvpOrDict = kvpOrDict.GetGenericTypeDefinition();
                            if (kvpOrDict == typeof(KeyValuePair<,>))
                            {
                                keyAttribute.Key.DeleteValue((string)dictised["Key"]);
                            }
                            else if (kvpOrDict == typeof(Dictionary<,>))
                            {
                                foreach (KeyValuePair<string, object> item in dictised)
                                {
                                    keyAttribute.Key.DeleteValue(item.Key);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Throws an exception if the metadata does not indicate that this is a registry-backed object.
        /// </summary>
        /// <param name="metadata">Reflected metadata about a .NET class, structure or record</param>
        private static void ThrowIfNotRegistryBacked(ContainerTypeInformation metadata)
        {
            if (metadata.ContainerDefinition is RegistryKeyNameAttribute)
            {
                return;
            }

            throw new InvalidOperationException($"Operation cannot be executed on an object that is not decorated with a '{nameof(RegistryKeyNameAttribute)}' attribute.");
        }

        /// <summary>
        /// Write the value to the registry
        /// </summary>
        /// <param name="key">The <see cref="RegistryKey"/> to write it to. Key must have been opened in Writable mode.</param>
        /// <param name="valueName">Name of the value within the <paramref name="key"/> to write to</param>
        /// <param name="value">The value to write. Can NOT be NULL!</param>
        private static void PersistTo(RegistryKey key, string valueName, object? value)
        {
            if (value == null)
            {
                // The registry cannot store NULLs, instead, we delete the value
                key.DeleteValue(valueName);
                return;
            }

            RegistryValueKind kind;
            if (value is string str)
            {
                kind = (str.Contains('%') ? RegistryValueKind.ExpandString : RegistryValueKind.String);
            }
            else if ((value is int) || (value is uint) || (value is short) || (value is ushort))
            {
                kind = RegistryValueKind.DWord;
            }
            else if ((value is long) || (value is ulong))
            {
                kind = RegistryValueKind.QWord;
            }
            else if (value is bool b)
            {
                kind = RegistryValueKind.String;
                value = (b ? "true" : "false");
            }
            else if ((value is DateTime) || (value is DateOnly) || (value is TimeOnly) || (value is DateTimeOffset) || (value is Guid))
            {
                kind = RegistryValueKind.String;
                value = value.ToString()!;
            }
            else if (value is Color color)
            {
                kind = RegistryValueKind.DWord;
                value = color.ToArgb();
            }
            else if (value is Enum)
            {
                // Serialise enums as strings
                kind = RegistryValueKind.String;
                value = value.ToString()!;
            }
            else
            {
                // What are we? Store as Json-serialised.
                kind = RegistryValueKind.String;
                value = JsonSerializer.Serialize(value: value, options: _jsonOptions);
            }

            key.SetValue(valueName, value, kind);
        }

        /// <summary>
        /// We use Json Serialisation in the methods above. This is a common set of options we pass in.
        /// </summary>
        static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions()
        {
            AllowTrailingCommas = false,
            WriteIndented = false
        };
    }
}
