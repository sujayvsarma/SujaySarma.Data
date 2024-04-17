using System;

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
        /// Select and deserialise data for a single object pointed to by the type declaration for <typeparamref name="TObject"/>
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record</typeparam>
        /// <returns>Populated object instance</returns>
        public static TObject Select<TObject>()
            => WindowsRegistrySerialiser.Deserialise<TObject>();

        /// <summary>
        /// Select and deserialise data for a single object pointed to by the type declaration <paramref name="type"/>
        /// </summary>
        /// <param name="type">Type of .NET class, structure or record</param>
        /// <returns>Populated object instance</returns>
        public static object? Select(Type type)
            => WindowsRegistrySerialiser.Deserialise(type);

    }
}
