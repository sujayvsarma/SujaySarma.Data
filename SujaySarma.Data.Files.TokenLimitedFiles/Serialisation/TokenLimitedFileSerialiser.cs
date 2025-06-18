using SujaySarma.Data.Core.Reflection;
using SujaySarma.Data.Files.TokenLimitedFiles.Attributes;

using System;
using System.Collections.Generic;
using System.Data;

namespace SujaySarma.Data.Files.TokenLimitedFiles.Serialisation
{
    /// <summary>
    /// Serialise/Deserialise data between .NET and Flatfiles
    /// </summary>
    internal static class TokenLimitedFileSerialiser
    {
        /// <summary>
        /// Transform the contents of a <see cref="T:System.Data.DataTable" /> into a List of <typeparamref name="TObject" /> instances.
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="table">DataTable with column definitions and data rows</param>
        /// <returns>List of <typeparamref name="TObject" /> instances</returns>
        /// <exception cref="T:System.TypeLoadException">If an instance of <typeparamref name="TObject" /> cannot be created.</exception>
        public static List<TObject> Transform<TObject>(DataTable table)
        {
            ContainerTypeInfo ContainerTypeInfo = TypeDiscoveryFactory.Resolve<TObject>();
            if (Activator.CreateInstance<TObject>() == null)
            {
                throw new TypeLoadException($"Cannot create instance of an object of type '{ContainerTypeInfo.Name}'");
            }

            List<TObject> objectList = new List<TObject>();
            foreach (DataRow row in table.Rows)
            {
                // No need to double-check/throw as we have already validated it in the above IF...
                object instance = Activator.CreateInstance(typeof(TObject))!;

                foreach (MemberTypeInfo member in ContainerTypeInfo.Members.Values)
                {
                    if (member.Column is FileFieldAttribute memberDefinition)
                    {
                        Core.ReflectionUtils.SetValue(ref instance, member, row[memberDefinition.CreateQualifiedName()]);
                    }
                }
                objectList.Add((TObject)instance);
            }

            return objectList;
        }
    }

}
