using System.Collections.Generic;
using System.Linq;

using SujaySarma.Data.WindowsRegistry.Serialisation;

namespace SujaySarma.Data.WindowsRegistry
{
    /* 
        Select (read) operations 

        NOTE: There are no Async methods because Windows Registry does not support them.
              (Though it would be possible to create contrived async methods using Task.Run() etc, 
              such implementations would negatively impact performance and reliability!)

    */
    public static partial class WindowsRegistryContext
    {

        /// <summary>
        /// Insert or update the data in <paramref name="instance"/> into the Windows Registry
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="instance">Instance of an object of type <typeparamref name="TObject"/> with data to be stored</param>
        public static void InsertOrUpdate<TObject>(TObject instance)
            => WindowsRegistrySerialiser.Serialise<TObject>(instance);

        /// <summary>
        /// Insert or update the data in <paramref name="instance"/> into the Windows Registry
        /// </summary>
        /// <param name="instance">Instance of an object with data to be stored</param>
        public static void InsertOrUpdate(object? instance)
            => WindowsRegistrySerialiser.Serialise(instance);

        /// <summary>
        /// Insert or update data in each item of the <paramref name="instances"/> into the Windows Registry
        /// </summary>
        /// <param name="instances">Instances of objects, typically each of a different type of .NET class, structure or record with data to be stored</param>
        public static void InsertOrUpdate(params object?[] instances)
            => InsertOrUpdate(instances.ToList());

        /// <summary>
        /// Insert or update data in each item of the <paramref name="instances"/> into the Windows Registry
        /// </summary>
        /// <param name="instances">Instances of objects, typically each of a different type of .NET class, structure or record with data to be stored</param>
        public static void InsertOrUpdate(IEnumerable<object?> instances)
        {
            foreach (object? item in instances)
            {
                WindowsRegistrySerialiser.Serialise(item);
            }
        }

        /// <summary>
        /// Delete the data in <paramref name="instance"/> from the Windows Registry
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <param name="instance">Instance of an object of type <typeparamref name="TObject"/> with data to be deleted</param>
        public static void Delete<TObject>(TObject instance)
            => WindowsRegistrySerialiser.Delete<TObject>(instance);

        /// <summary>
        /// Delete the data in <paramref name="instance"/> from the Windows Registry
        /// </summary>
        /// <param name="instance">Instance of an object with data to be deleted</param>
        public static void Delete(object? instance)
            => WindowsRegistrySerialiser.Delete(instance);

        /// <summary>
        /// Delete the data in <paramref name="instances"/> from the Windows Registry
        /// </summary>
        /// <param name="instances">Instances of objects, typically each of a different type of .NET class, structure or record with data to be deleted</param>
        public static void Delete(params object?[] instances)
            => Delete(instances.ToList());

        /// <summary>
        /// Delete the data in <paramref name="instances"/> from the Windows Registry
        /// </summary>
        /// <param name="instances">Instances of objects, typically each of a different type of .NET class, structure or record with data to be deleted</param>
        public static void Delete(IEnumerable<object?> instances)
        {
            foreach (object? item in instances)
            {
                WindowsRegistrySerialiser.Delete(item);
            }
        }

    }
}
