using Azure;
using Azure.Data.Tables;

using SujaySarma.Data.Azure.Tables.Attributes;
using SujaySarma.Data.Azure.Tables.Reflection;
using SujaySarma.Data.Core;
using SujaySarma.Data.Core.Reflection;

using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SujaySarma.Data.Azure.Tables.Serialisation
{
    /// <summary>
    /// Serialise/Deserialise data between the .NET and Azure Tables
    /// </summary>
    internal static class AzureTablesSerialiser
    {
        /// <summary>
        /// Read an <see cref="TableEntity" /> and populate information into a .NET class, record or struct.
        /// </summary>
        /// <typeparamref name="TObject">Type of a class, record or struct</typeparamref>
        /// <param name="entity">Azure Tables Entity</param>
        public static TObject Deserialise<TObject>(TableEntity entity)
        {
            return (TObject)Deserialise(entity, typeof(TObject));
        }

        /// <summary>
        /// Read an <see cref="TableEntity" /> and populate information into a .NET class, record or struct.
        /// </summary>
        /// <param name="entity">Azure Tables Entity</param>
        /// <param name="targetType">Type of the class, record or struct to instantiate/populate</param>
        public static object Deserialise(TableEntity entity, Type targetType)
        {
            object instance = Activator.CreateInstance(targetType) ?? throw new TypeLoadException($"Cannot create an instance of type '{targetType.Name}'.");
            foreach (KeyValuePair<string, MemberTypeInfo> member in TypeDiscoveryFactory.Resolve(targetType).Members)
            {
                if (member.Value.Column is PartitionKeyAttribute)
                {
                    ReflectionUtils.SetValue(ref instance, member.Value, entity.PartitionKey);
                }
                else if (member.Value.Column is RowKeyAttribute)
                {
                    ReflectionUtils.SetValue(ref instance, member.Value, entity.RowKey);
                }
                else if (member.Value.Column is ETagAttribute)
                {
                    ReflectionUtils.SetValue(ref instance, member.Value, entity.ETag);
                }
                else if (member.Value.Column is TimestampAttribute)
                {
                    ReflectionUtils.SetValue(ref instance, member.Value, entity.Timestamp);
                }
                else if (member.Value.Column is TableColumnAttribute memberDefinition)
                {
                    object? obj = entity[memberDefinition.CreateQualifiedName()];
                    if (memberDefinition.SerialiseAsJson && (obj is string str) && (!string.IsNullOrWhiteSpace(str)))
                    {
                        obj = JsonSerializer.Deserialize(
                                    str,
                                    ReflectionUtils.GetFieldOrPropertyDataType(member.Value.FieldOrPropertyInfo)
                                );
                    }
                    ReflectionUtils.SetValue(ref instance, member.Value, obj);
                }
            }
            return instance;
        }

        /// <summary>
        /// Serialise the given <paramref name="instance" /> data into a <see cref="TableEntity" />.
        /// </summary>
        /// <param name="instance">A non-Null instance of a .NET class, record or struct to serialise</param>
        /// <param name="metadataOnly">When set, does not serialise (custom) columns. This is used during DELETE operations when the entire object is not required.</param>
        /// <param name="setSoftDeleteValue">The value to set into the 'IsDeleted' column</param>
        /// <returns>The serialised instance of a <see cref="TableEntity" /></returns>
        public static TableEntity? Serialise(object? instance, bool metadataOnly = false, bool setSoftDeleteValue = false)
        {
            if (instance == null)
            {
                return null;
            }

            ContainerTypeInfo ContainerTypeInfo = TypeDiscoveryFactory.Resolve(instance.GetType());
            TableEntity tableEntity = new TableEntity();
            if (ContainerTypeInfo.Container.UseSoftDelete)
            {
                tableEntity[Core.ReservedNames.IsDeleted] = setSoftDeleteValue;
            }

            foreach (MemberTypeInfo member in ContainerTypeInfo.Members.Values)
            {
                object? obj = ReflectionUtils.GetValue(ref instance, member);
                if (member.Column is PartitionKeyAttribute pk)
                {
                    string? str = (string?)ReflectionUtilsExtension.EnsureAzureTablesCompatibleValue(obj, typeof(string), false);
                    tableEntity.PartitionKey = ((!string.IsNullOrWhiteSpace(str)) ? str : throw new ArgumentNullException($"PartitionKey '{pk.Name}' cannot contain Null, Empty or whitespace value"));
                }
                else if (member.Column is RowKeyAttribute rk)
                {
                    string? str = (string?)ReflectionUtilsExtension.EnsureAzureTablesCompatibleValue(obj, typeof(string), false);
                    tableEntity.RowKey = ((!string.IsNullOrWhiteSpace(str)) ? str : throw new ArgumentNullException($"RowKey '{rk.Name}' cannot contain Null, Empty or whitespace value"));
                }
                else if (member.Column is ETagAttribute eTag)
                {
                    string? str = (string?)ReflectionUtilsExtension.EnsureAzureTablesCompatibleValue(obj, typeof(string), false);
                    tableEntity.ETag = ((!string.IsNullOrWhiteSpace(str)) ? new ETag(str) : throw new ArgumentNullException($"ETag '{member.Name}' cannot contain Null, Empty or whitespace value"));
                }
                else if (!metadataOnly && member.Column is TableColumnAttribute col)
                {
                    tableEntity[col.CreateQualifiedName()] = ReflectionUtilsExtension.EnsureAzureTablesCompatibleValue(obj);
                }
            }
            return tableEntity;
        }

        /// <summary>
        /// Serialise a collection of <paramref name="objects" /> into a collection of <see cref="TableEntity" />
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="objects">The collection of .NET classes, structures or records to serialise</param>
        /// <param name="metadataOnly">When set, does not serialise (custom) columns. This is used during DELETE operations when the entire object is not required.</param>
        /// <param name="setSoftDeleteValue">The value to set into the 'IsDeleted' column</param>
        /// <returns>The serialised collection of <see cref="TableEntity" /></returns>
        public static List<TableEntity> Serialise<TObject>(IEnumerable<TObject> objects, bool metadataOnly = false, bool setSoftDeleteValue = false)
        {
            Type type = typeof(TObject);
            ContainerTypeInfo ContainerTypeInfo = TypeDiscoveryFactory.Resolve(type) 
                                                    ?? throw new TypeLoadException($"The type '{type.Name}' does not seem to have the appropriate attribute decorations.");
            
            List<TableEntity> tableEntityList = new List<TableEntity>();
            foreach (TObject o in objects)
            {
                TableEntity tableEntity = new TableEntity();
                if (ContainerTypeInfo.Container.UseSoftDelete)
                {
                    tableEntity[Core.ReservedNames.IsDeleted] = setSoftDeleteValue;
                }

                object? instance = o;
                foreach (MemberTypeInfo member in ContainerTypeInfo.Members.Values)
                {
                    object? obj = ReflectionUtils.GetValue(ref instance, member);
                    if (member.Column is PartitionKeyAttribute pk)
                    {
                        string? str = (string?)ReflectionUtilsExtension.EnsureAzureTablesCompatibleValue(obj, typeof(string), false);
                        tableEntity.PartitionKey = ((!string.IsNullOrWhiteSpace(str)) ? str : throw new ArgumentNullException($"PartitionKey '{pk.Name}' cannot contain Null, Empty or whitespace value"));
                    }
                    else if (member.Column is RowKeyAttribute rk)
                    {
                        string? str = (string?)ReflectionUtilsExtension.EnsureAzureTablesCompatibleValue(obj, typeof(string), false);
                        tableEntity.RowKey = ((!string.IsNullOrWhiteSpace(str)) ? str : throw new ArgumentNullException($"RowKey '{rk.Name}' cannot contain Null, Empty or whitespace value"));
                    }
                    else if (member.Column is ETagAttribute eTag)
                    {
                        string? str = (string?)ReflectionUtilsExtension.EnsureAzureTablesCompatibleValue(obj, typeof(string), false);
                        tableEntity.ETag = ((!string.IsNullOrWhiteSpace(str)) ? new ETag(str) : throw new ArgumentNullException($"ETag '{member.Name}' cannot contain Null, Empty or whitespace value"));
                    }
                    else if (!metadataOnly && member.Column is TableColumnAttribute col)
                    {
                        tableEntity[col.CreateQualifiedName()] = ReflectionUtilsExtension.EnsureAzureTablesCompatibleValue(obj);
                    }
                }
                tableEntityList.Add(tableEntity);
            }
            return tableEntityList;
        }
    }

}
