using System;
using System.Collections.Generic;
using System.Linq;

using SujaySarma.Data.Core.Reflection;
using SujaySarma.Data.WindowsRegistry.Attributes;

namespace SujaySarma.Data.WindowsRegistry
{
    /* 
        Utility functions to manage registry keys
    */
    public static partial class WindowsRegistryContext
    {

        /// <summary>
        /// Deletes an entire key (and subtrees) for the container pointed to by <typeparamref name="TObject"/>
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        public static void DropKey<TObject>()
            => DropKey(typeof(TObject));

        /// <summary>
        /// Deletes an entire key (and subtrees) for the container pointed to by <paramref name="type"/>
        /// </summary>
        /// <param name="type">Type of .NET class, structure or record</param>
        public static void DropKey(Type type)
        {
            ContainerTypeInformation metadata = TypeDiscoveryFactory.Resolve(type) ?? throw new TypeLoadException($"Type '{type.Name}' is not appropriately decorated.");
            if (metadata.ContainerDefinition is RegistryKeyNameAttribute kna)
            {
                kna.ParentKey.DeleteSubKeyTree(kna.CreateQualifiedName());
            }
        }

        /// <summary>
        /// Deletes an entire key (and subtrees) for the container pointed to by each <paramref name="types"/>
        /// </summary>
        /// <param name="types">Types of .NET class, structure or record</param>
        public static void DropKeys(params Type[] types)
            => DropKeys(types.ToList());

        /// <summary>
        /// Deletes an entire key (and subtrees) for the container pointed to by each <paramref name="types"/>
        /// </summary>
        /// <param name="types">Types of .NET class, structure or record</param>
        public static void DropKeys(IEnumerable<Type> types)
        {
            foreach (Type type in types)
            {
                DropKey(type);
            }
        }
    }
}
