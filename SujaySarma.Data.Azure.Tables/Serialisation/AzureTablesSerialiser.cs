using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

using Azure;
using Azure.Data.Tables;

using SujaySarma.Data.Azure.Tables.Attributes;
using SujaySarma.Data.Azure.Tables.Reflection;
using SujaySarma.Data.Core.Reflection;

namespace SujaySarma.Data.Azure.Tables.Serialisation
{
    /// <summary>
    /// Serialise/Deserialise data between the .NET and Azure Tables
    /// </summary>
    internal static class AzureTablesSerialiser
    {
        /// <summary>
        /// Read an <see cref="TableEntity"> and populate information into a .NET class, record or struct.
        /// </summary>
        /// <typeparamref name="T">Type of a class, record or struct</typeparamref>
        /// <param name="entity">Azure Tables Entity</param>
        public static T Deserialise<T>(TableEntity entity)
        {
            return (T)Deserialise(entity, typeof(T));
        }

        /// <summary>
        /// Read an <see cref="TableEntity"> and populate information into a .NET class, record or struct.
        /// </summary>
        /// <param name="entity">Azure Tables Entity</param>
        /// <param name="targetType">Type of the class, record or struct to instantiate/populate</param>
        public static object Deserialise(TableEntity entity, Type targetType)
        {
            object instance = Activator.CreateInstance(targetType)
                                ?? throw new TypeLoadException($"Cannot create an instance of type '{targetType.Name}'.");

            ContainerTypeInformation typeInfo = TypeDiscoveryFactory.Resolve(targetType)
                                ?? throw new TypeLoadException($"The type '{targetType.Name}' does not seem to have the appropriate attribute decorations.");


            foreach (KeyValuePair<string, ContainerMemberTypeInformation> member in typeInfo.Members)
            {
                if (member.Value.ContainerMemberDefinition is PartitionKeyAttribute)
                {
                    ReflectionUtils.SetValue(instance, member.Value, entity.PartitionKey);
                }
                else if (member.Value.ContainerMemberDefinition is RowKeyAttribute)
                {
                    ReflectionUtils.SetValue(instance, member.Value, entity.RowKey);
                }
                else if (member.Value.ContainerMemberDefinition is ETagAttribute)
                {
                    ReflectionUtils.SetValue(instance, member.Value, entity.ETag);
                }
                else if (member.Value.ContainerMemberDefinition is TimestampAttribute)
                {
                    ReflectionUtils.SetValue(instance, member.Value, entity.Timestamp);
                }
                else if (member.Value.ContainerMemberDefinition is TableColumnAttribute tableColumn)
                {
                    object? value = entity[tableColumn.CreateQualifiedName()];
                    if (tableColumn.AllowSerializationAsJson)
                    {
                        if ((value is string str) && (!string.IsNullOrWhiteSpace(str)))
                        {
                            value = JsonSerializer.Deserialize(str, member.Value.FieldOrPropertyInfo.MemberType switch
                            {
                                MemberTypes.Field => (member.Value.FieldOrPropertyInfo as FieldInfo)!.FieldType,
                                MemberTypes.Property => (member.Value.FieldOrPropertyInfo as PropertyInfo)!.PropertyType
                            });
                        }
                    }
                    ReflectionUtils.SetValue(instance, member.Value, value);
                }
            }

            return instance;
        }

        /// <summary>
        /// Serialise the given <paramref name="instance"/> data into a <see cref="TableEntity"/>.
        /// </summary>
        /// <param name="instance">A non-Null instance of a .NET class, record or struct to serialise</param>
        /// <param name="metadataOnly">When set, does not serialise (custom) columns. This is used during DELETE operations when the entire object is not required.</param>
        /// <param name="setSoftDeleteValue">The value to set into the 'IsDeleted' column</param>
        /// <returns>The serialised instance of a <see cref="TableEntity"/></returns>
        public static TableEntity Serialise(object instance, bool metadataOnly = false, bool setSoftDeleteValue = false)
        {
            Type targetType = instance.GetType();
            ContainerTypeInformation typeInfo = TypeDiscoveryFactory.Resolve(targetType)
                                ?? throw new TypeLoadException($"The type '{targetType.Name}' does not seem to have the appropriate attribute decorations.");

            TableEntity entity = new TableEntity();
            if (typeInfo.ContainerDefinition.UseSoftDelete)
            {
                entity[SujaySarma.Data.Core.ReservedNames.IsDeleted] = setSoftDeleteValue;
            }

            foreach (KeyValuePair<string, ContainerMemberTypeInformation> member in typeInfo.Members)
            {
                object? value = ReflectionUtils.GetValue(instance, member.Value);

                if (member.Value.ContainerMemberDefinition is PartitionKeyAttribute pk)
                {
                    string? pkVal = (string?)ReflectionUtilsExtension.EnsureAzureTablesCompatibleValue(value, typeof(string), false)
                                        ?? ((pk.DefaultValueProviderFunction != null) ? (string)pk.DefaultValueProviderFunction() : null);

                    if (string.IsNullOrWhiteSpace(pkVal))
                    {
                        throw new ArgumentNullException($"PartitionKey '{pk.Name}' cannot contain Null, Empty or whitespace value");
                    }

                    entity.PartitionKey = pkVal;
                }
                else if (member.Value.ContainerMemberDefinition is RowKeyAttribute rk)
                {
                    string? rkVal = (string?)ReflectionUtilsExtension.EnsureAzureTablesCompatibleValue(value, typeof(string), false)
                                        ?? ((rk.DefaultValueProviderFunction != null) ? (string)rk.DefaultValueProviderFunction() : null);

                    if (string.IsNullOrWhiteSpace(rkVal))
                    {
                        throw new ArgumentNullException($"RowKey '{rk.Name}' cannot contain Null, Empty or whitespace value");
                    }

                    entity.RowKey = rkVal;
                }
                else if (member.Value.ContainerMemberDefinition is ETagAttribute eTag)
                {
                    string? etVal = (string?)ReflectionUtilsExtension.EnsureAzureTablesCompatibleValue(value, typeof(string), false)
                                        ?? ((eTag.DefaultValueProviderFunction != null) ? (string)eTag.DefaultValueProviderFunction() : null);

                    if (string.IsNullOrWhiteSpace(etVal))
                    {
                        throw new ArgumentNullException($"ETag '{eTag.Name}' cannot contain Null, Empty or whitespace value");
                    }

                    entity.ETag = new ETag(etVal);
                }
                // We don't serialise LastModified because that field is not "writeable"
                else if ((!metadataOnly) && member.Value.ContainerMemberDefinition is TableColumnAttribute tableColumn)
                {
                    entity[tableColumn.CreateQualifiedName()] = ReflectionUtilsExtension.EnsureAzureTablesCompatibleValue(value, jsonSerialiseIfNot: true);
                }
            }

            return entity;
        }

        /// <summary>
        /// Serialise a collection of <paramref name="objects"/> into a collection of <see cref="TableEntity"/>
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="objects">The collection of .NET classes, structures or records to serialise</param>
        /// <param name="metadataOnly">When set, does not serialise (custom) columns. This is used during DELETE operations when the entire object is not required.</param>
        /// <param name="setSoftDeleteValue">The value to set into the 'IsDeleted' column</param>
        /// <returns>The serialised collection of <see cref="TableEntity"/></returns>
        public static List<TableEntity> Serialise<TObject>(IEnumerable<TObject> objects, bool metadataOnly = false, bool setSoftDeleteValue = false)
        {
            Type targetType = typeof(TObject);
            ContainerTypeInformation typeInfo = TypeDiscoveryFactory.Resolve(targetType)
                                ?? throw new TypeLoadException($"The type '{targetType.Name}' does not seem to have the appropriate attribute decorations.");

            List<TableEntity> entities = new List<TableEntity>();
            foreach (TObject instance in objects)
            {
                TableEntity entity = new TableEntity();
                if (typeInfo.ContainerDefinition.UseSoftDelete)
                {
                    entity[SujaySarma.Data.Core.ReservedNames.IsDeleted] = setSoftDeleteValue;
                }

                foreach (KeyValuePair<string, ContainerMemberTypeInformation> member in typeInfo.Members)
                {
                    object? value = ReflectionUtils.GetValue(instance, member.Value);

                    if (member.Value.ContainerMemberDefinition is PartitionKeyAttribute pk)
                    {
                        string? pkVal = (string?)ReflectionUtilsExtension.EnsureAzureTablesCompatibleValue(value, typeof(string), false)
                                            ?? ((pk.DefaultValueProviderFunction != null) ? (string)pk.DefaultValueProviderFunction() : null);

                        if (string.IsNullOrWhiteSpace(pkVal))
                        {
                            throw new ArgumentNullException($"PartitionKey '{pk.Name}' cannot contain Null, Empty or whitespace value");
                        }

                        entity.PartitionKey = pkVal;
                    }
                    else if (member.Value.ContainerMemberDefinition is RowKeyAttribute rk)
                    {
                        string? rkVal = (string?)ReflectionUtilsExtension.EnsureAzureTablesCompatibleValue(value, typeof(string), false)
                                            ?? ((rk.DefaultValueProviderFunction != null) ? (string)rk.DefaultValueProviderFunction() : null);

                        if (string.IsNullOrWhiteSpace(rkVal))
                        {
                            throw new ArgumentNullException($"RowKey '{rk.Name}' cannot contain Null, Empty or whitespace value");
                        }

                        entity.RowKey = rkVal;
                    }
                    else if (member.Value.ContainerMemberDefinition is ETagAttribute eTag)
                    {
                        string? etVal = (string?)ReflectionUtilsExtension.EnsureAzureTablesCompatibleValue(value, typeof(string), false)
                                            ?? ((eTag.DefaultValueProviderFunction != null) ? (string)eTag.DefaultValueProviderFunction() : null);

                        if (string.IsNullOrWhiteSpace(etVal))
                        {
                            throw new ArgumentNullException($"ETag '{eTag.Name}' cannot contain Null, Empty or whitespace value");
                        }

                        entity.ETag = new ETag(etVal);
                    }
                    // We don't serialise LastModified because that field is not "writeable"
                    else if ((!metadataOnly) && member.Value.ContainerMemberDefinition is TableColumnAttribute tableColumn)
                    {
                        entity[tableColumn.CreateQualifiedName()] = ReflectionUtilsExtension.EnsureAzureTablesCompatibleValue(value, jsonSerialiseIfNot: true);
                    }
                }

                entities.Add(entity);
            }

            return entities;
        }
    }
}
