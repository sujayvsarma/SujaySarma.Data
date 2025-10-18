using System;
using System.Data;
using System.Text.Json;

using SujaySarma.Data.Core.Reflection;

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
                /*
                    Incoming column names maybe:

                        ------------------------------------------------------------------------------------------------------------
                        Probables                                                 Source of name
                        ------------------------------------------------------------------------------------------------------------
                        ColName, [ColName]                                      - user provided dataset.
                        Tab.ColName, [Tab].[ColName], Tab.[Colname]             - user or SqlQueryBuilder.
                        Sch.Tab.Col, [Sch].[Tab].[ColName], Sch.Tab.[ColName]   - user or SqlQueryBuilder.
                        [Sch.Tab.ColName]                                       - SqlQueryBuilder only!
                        ------------------------------------------------------------------------------------------------------------                        
                */


                string  rawColName              = member.Column.CreateQualifiedName().Replace("[", "").Replace("]", ""),
                        colWithTableName        = $"{ContainerTypeInfo.Container.Name.Replace("[", "").Replace("]", "")}.{rawColName}",
                        colWithSchemaTableName  = $"{ContainerTypeInfo.Container.CreateQualifiedName().Replace("[", "").Replace("]", "")}.{rawColName}";

                bool resultsetHasColumn = false;
                string discoveredColumnName = string.Empty;

                foreach(DataColumn col in row.Table.Columns)
                {
                    string sanitisedColName = col.ColumnName.Replace("[", "").Replace("]", "");
                    if (sanitisedColName.Equals(rawColName, StringComparison.InvariantCultureIgnoreCase) 
                        || sanitisedColName.Equals(colWithTableName, StringComparison.InvariantCultureIgnoreCase) 
                            || sanitisedColName.Equals(colWithSchemaTableName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        discoveredColumnName = col.ColumnName;
                        resultsetHasColumn = true;
                        break;
                    }
                }

                if (resultsetHasColumn)
                {
                    object? obj = row[discoveredColumnName];
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

        /// <summary>
        /// Transform a data row into an object of type <paramref name="targetType"/>.
        /// </summary>
        /// <param name="row"><see cref="T:System.Data.DataRow" /> containing the data to be transformed. Must contain column information and be attached to a table.</param>
        /// <param name="targetType">Type of .NET class, structure or record</param>
        /// <returns>Instance of type <paramref name="targetType"/></returns>
        public static object Transform(DataRow row, Type targetType)
        {
            if ((row.Table == null) || (row.Table.Columns.Count == 0))
            {
                throw new TypeLoadException($"The DataRow passed is not attached to a table, or the table has no columns defined. Object: '{targetType.Name}'");
            }

            ContainerTypeInfo ContainerTypeInfo = TypeDiscoveryFactory.Resolve(targetType);
            object instance = (Activator.CreateInstance(targetType) ?? throw new TypeLoadException($"Unable to create instance of type '{ContainerTypeInfo.Name}'."));
            foreach (MemberTypeInfo member in ContainerTypeInfo.Members.Values)
            {
                /*
                    Incoming column names maybe:

                        ------------------------------------------------------------------------------------------------------------
                        Probables                                                 Source of name
                        ------------------------------------------------------------------------------------------------------------
                        ColName, [ColName]                                      - user provided dataset.
                        Tab.ColName, [Tab].[ColName], Tab.[Colname]             - user or SqlQueryBuilder.
                        Sch.Tab.Col, [Sch].[Tab].[ColName], Sch.Tab.[ColName]   - user or SqlQueryBuilder.
                        [Sch.Tab.ColName]                                       - SqlQueryBuilder only!
                        ------------------------------------------------------------------------------------------------------------                        
                */


                string  rawColName              = member.Column.CreateQualifiedName().Replace("[", "").Replace("]", ""),
                        colWithTableName        = $"{ContainerTypeInfo.Container.Name.Replace("[", "").Replace("]", "")}.{rawColName}",
                        colWithSchemaTableName  = $"{ContainerTypeInfo.Container.CreateQualifiedName().Replace("[", "").Replace("]", "")}.{rawColName}";

                bool resultsetHasColumn = false;
                string discoveredColumnName = string.Empty;

                foreach (DataColumn col in row.Table.Columns)
                {
                    string sanitisedColName = col.ColumnName.Replace("[", "").Replace("]", "");
                    if (sanitisedColName.Equals(rawColName, StringComparison.InvariantCultureIgnoreCase)
                        || sanitisedColName.Equals(colWithTableName, StringComparison.InvariantCultureIgnoreCase)
                            || sanitisedColName.Equals(colWithSchemaTableName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        discoveredColumnName = col.ColumnName;
                        resultsetHasColumn = true;
                        break;
                    }
                }

                if (resultsetHasColumn)
                {
                    object? obj = row[discoveredColumnName];
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

            return instance;
        }
    }
}
