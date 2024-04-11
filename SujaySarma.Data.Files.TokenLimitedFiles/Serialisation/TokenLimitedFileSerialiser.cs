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
    internal static class TokenLimitedFileSerialiser
    {
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
    }
}
