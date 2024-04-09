using System;
using System.Collections.Generic;
using System.Data;

using SujaySarma.Data.Core.Reflection;
using SujaySarma.Data.Files.TokenLimitedFiles.Attributes;

namespace SujaySarma.Data.Files.TokenLimitedFiles.Serialisation
{
    /// <summary>
    /// Serialise/Deserialise data between .NET and Flatfiles
    /// </summary>
    public static class TokenLimitedFileSerialiser
    {
        /// <summary>
        /// Transform a <typeparamref name="TObject"/> into a <see cref="DataTable"/>
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="obj">Instance of object</param>
        /// <returns>A <see cref="DataTable"/> with the type information and data. This will contain zero or one rows</returns>
        public static DataTable Transform<TObject>(TObject obj)
        {
            ContainerTypeInformation typeInfo = TypeDiscoveryFactory.Resolve<TObject>();
            DataTable table = CreateDatatableFromTypeInformation(typeInfo);

            if (obj != null)
            {
                DataRow row = table.NewRow();
                foreach (ContainerMemberTypeInformation member in typeInfo.Members.Values)
                {
                    if (member.ContainerMemberDefinition is FileFieldAttribute ffa)
                    {
                        row[ffa.CreateQualifiedName()] = ReflectionUtils.GetValue(obj, member);
                    }
                }

                table.Rows.Add(row);
            }

            return table;
        }

        /// <summary>
        /// Transform the contents of a <see cref="DataTable"/> into a List of <typeparamref name="TObject"/> instances.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="table">DataTable with column definitions and data rows</param>
        /// <returns>List of <typeparamref name="TObject"/> instances</returns>
        /// <exception cref="TypeLoadException">If an instance of <typeparamref name="TObject"/> cannot be created.</exception>
        public static List<TObject> Transform<TObject>(DataTable table)
        {
            ContainerTypeInformation typeInfo = TypeDiscoveryFactory.Resolve<TObject>();
            if (Activator.CreateInstance(typeof(TObject)) == null)
            {
                throw new TypeLoadException($"Cannot create instance of an object of type '{typeInfo.Name}'");
            }

            List<TObject> items = new List<TObject>();
            foreach (DataRow row in table.Rows)
            {
                TObject instance = (TObject?)Activator.CreateInstance(typeof(TObject))!;
                foreach (ContainerMemberTypeInformation member in typeInfo.Members.Values)
                {
                    if (member.ContainerMemberDefinition is FileFieldAttribute ffa)
                    {
                        ReflectionUtils.SetValue(instance, member, row[ffa.CreateQualifiedName()]);
                    }
                }
                items.Add(instance);
            }

            return items;
        }

        /// <summary>
        /// Transform the contents of the provided <paramref name="headerRow"/> and <paramref name="dataRow"/> into an instance of a <typeparamref name="TObject"/>.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="headerRow">String array of column names from the TSV's header row</param>
        /// <param name="dataRow">String array of data values from the TSV's row (as returned by <see cref="TokenLimitedFileReader.ReadRow"/> or <see cref="TokenLimitedFileReader.ReadRowAsync"/>)</param>
        /// <returns>An instance of type <typeparamref name="TObject"/> or Null if the datarow was a Null as well</returns>
        /// <exception cref="TypeLoadException">If an instance of <typeparamref name="TObject"/> cannot be created.</exception>
        public static TObject? Transform<TObject>(string?[] headerRow, string?[]? dataRow)
        {
            ContainerTypeInformation typeInfo = TypeDiscoveryFactory.Resolve<TObject>();
            if (dataRow == null)
            {
                return default;
            }

            TObject instance = (TObject?)Activator.CreateInstance(typeof(TObject)) ?? throw new TypeLoadException($"Cannot create instance of an object of type '{typeInfo.Name}'");
            foreach (ContainerMemberTypeInformation member in typeInfo.Members.Values)
            {
                if (member.ContainerMemberDefinition is FileFieldAttribute ffa)
                {
                    int columnIndex = findDataIndexFromColumnName(ffa.CreateQualifiedName());
                    columnIndex = ((columnIndex == -1) ? ffa.ColumnIndex : columnIndex);
                    if (columnIndex == -1)
                    {
                        continue;
                    }

                    ReflectionUtils.SetValue(instance, member, dataRow[columnIndex]);
                }
            }

            return instance;

            // Find the index of a column given its name
            int findDataIndexFromColumnName(string columnName)
            {
                for (int i = 0; i < headerRow.Length; i++)
                {
                    string? name = headerRow[i];
                    if ((!string.IsNullOrWhiteSpace(name)) && name.Equals(columnName))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }

        /// <summary>
        /// Creates a <see cref="DataTable"/> from the provided reflection-discovered type information
        /// </summary>
        /// <param name="typeInformation">Reflection-discovered type information</param>
        /// <returns>A <see cref="DataTable"/> with the populated type information</returns>
        private static DataTable CreateDatatableFromTypeInformation(ContainerTypeInformation typeInformation)
        {
            DataTable table = new DataTable(typeInformation.Name);
            foreach (ContainerMemberTypeInformation member in typeInformation.Members.Values)
            {
                DataColumn column = new DataColumn(member.ContainerMemberDefinition.CreateQualifiedName(), ReflectionUtils.GetFieldOrPropertyDataType(member.FieldOrPropertyInfo));
                table.Columns.Add(column);

                if ((member.ContainerMemberDefinition is FileFieldAttribute ffa) && (ffa.ColumnIndex >= 0))
                {
                    column.SetOrdinal(ffa.ColumnIndex);
                }
            }

            return table;
        }
    }
}
