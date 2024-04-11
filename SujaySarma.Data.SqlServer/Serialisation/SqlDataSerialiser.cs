using System;
using System.Data;
using System.Text.Json;

using SujaySarma.Data.Core.Reflection;

namespace SujaySarma.Data.SqlServer.Serialisation
{
    /// <summary>
    /// (ORM implementation) Transforms .NET classes, structures and records into Sql Server data rows and vice versa. 
    /// </summary>
    internal static class SqlDataSerialiser
    {

        /// <summary>
        /// Transform a data row into an object of type <typeparamref name="TObject"/>
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="row"><see cref="DataRow"/> containing the data to be transformed. Must contain column information and be attached to a table.</param>
        /// <returns>Instance of type <typeparamref name="TObject"/></returns>
        public static TObject Transform<TObject>(DataRow row)
        {
            if ((row.Table == default) || (row.Table.Columns.Count == 0))
            {
                throw new TypeLoadException($"The DataRow passed is not attached to a table, or the table has no schema. Object: '{typeof(TObject).Name}'");
            }

            ContainerTypeInformation metadata = TypeDiscoveryFactory.Resolve<TObject>();
            TObject instance = Activator.CreateInstance<TObject>() ?? throw new TypeLoadException($"Unable to create instance of type '{metadata.Name}'.");

            foreach (ContainerMemberTypeInformation member in metadata.Members.Values)
            {
                string columnName = member.ContainerMemberDefinition.CreateQualifiedName();
                if (row.Table.Columns.Contains(columnName))
                {
                    object? value = row[columnName];
                    if ((value is DBNull) || (value == DBNull.Value))
                    {
                        value = null;
                    }

                    if (member.ContainerMemberDefinition.AllowSerializationAsJson)
                    {
                        value = JsonSerializer.Deserialize($"{value ?? string.Empty}", SujaySarma.Data.Core.Reflection.ReflectionUtils.GetFieldOrPropertyDataType(member.FieldOrPropertyInfo));
                    }

                    SujaySarma.Data.Core.Reflection.ReflectionUtils.SetValue(instance, member, value);
                }
            }

            return instance;
        }

    }
}
