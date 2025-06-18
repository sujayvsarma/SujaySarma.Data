using SujaySarma.Data.Core.Reflection;

using System;
using System.Data;
using System.Text.Json;

namespace SujaySarma.Data.SqlServer.Serialisation
{
    /// <summary>
    /// (ORM implementation) Transforms .NET classes, structures and records into Sql Server data rows and vice versa.
    /// </summary>
    internal class SqlDataSerialiser
    {
        /// <summary>
        /// Transform a data row into an object of type <typeparamref name="TObject" />
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="row"><see cref="T:System.Data.DataRow" /> containing the data to be transformed. Must contain column information and be attached to a table.</param>
        /// <returns>Instance of type <typeparamref name="TObject" /></returns>
        public static TObject Transform<TObject>(DataRow row)
        {
            if ((row.Table == null) || (row.Table.Columns.Count == 0))
            {
                throw new TypeLoadException($"The DataRow passed is not attached to a table, or the table has no columns defined. Object: '{typeof(TObject).Name}'");
            }

            ContainerTypeInfo ContainerTypeInfo = TypeDiscoveryFactory.Resolve<TObject>();
            object instance = (Activator.CreateInstance<TObject>() ?? throw new TypeLoadException($"Unable to create instance of type '{ContainerTypeInfo.Name}'."));
            foreach (MemberTypeInfo member in ContainerTypeInfo.Members.Values)
            {
                string qualifiedName = member.Column.CreateQualifiedName();
                if (row.Table.Columns.Contains(qualifiedName))
                {
                    object? obj = row[qualifiedName];
                    if ((obj is DBNull) || (obj == DBNull.Value))
                    {
                        obj = null;
                    }

                    if (member.Column.SerialiseAsJson)
                    {
                        obj = JsonSerializer.Deserialize($"{obj ?? (object)string.Empty}", Core.ReflectionUtils.GetFieldOrPropertyDataType(member.FieldOrPropertyInfo));
                    }

                    Core.ReflectionUtils.SetValue(ref instance, member, obj);
                }
            }

            return (TObject)instance;
        }
    }
}
