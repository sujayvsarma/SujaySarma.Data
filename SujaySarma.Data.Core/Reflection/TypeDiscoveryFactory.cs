using System;
using System.Collections.Generic;
using System.Text.Json;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace SujaySarma.Data.Core.Reflection
{
    /// <summary>
    /// Helps discover the members, attributes and other metadata about the object/type in question.
    /// </summary>
    /// <remarks>
    ///     This class uses an in-built cache to speed up repeat lookups. 
    /// </remarks>
    public static class TypeDiscoveryFactory
    {
        /// <summary>
        /// Resolve the type information for the provided object
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record to resolve</typeparam>
        /// <returns>Type resolution information. Will be Null if appropriate attribute decorations were missing.</returns>
        public static ContainerTypeInformation Resolve<TObject>()
            => Resolve(typeof(TObject));

        /// <summary>
        /// Resolve the type information for the provided object
        /// </summary>
        /// <param name="type">Type to resolve</param>
        /// <returns>Type resolution information. Will be Null if appropriate attribute decorations were missing.</returns>
        public static ContainerTypeInformation Resolve(Type type)
        {
            ContainerTypeInformation information = default!;

            lock (_syncLockObject)
            {
                if (_localCache.TryGetValue(type.Name, out ContainerTypeInformation? info1))
                {
                    information = info1;
                }
                else if ((_memoryCache != null) && _memoryCache.TryGetValue(type.Name, out ContainerTypeInformation? info2))
                {
                    // Not Null because TryGetValue would have returned TRUE therefore!
                    information = info2!;
                }
                else if (_distributedCache != null)
                {
                    // This is the longest operation in this function
                    byte[]? data = _distributedCache.Get(type.Name);

                    if ((data != null) && (data.Length > 0))
                    {
                        string json = System.Text.Encoding.UTF8.GetString(data);
                        ContainerTypeInformation? info3 = JsonSerializer.Deserialize<ContainerTypeInformation>(json);
                        if (info3 != null)
                        {
                            information = info3;
                        }
                    }
                }
                else
                {
                    ContainerTypeInformation? info4 = new ContainerTypeInformation(type);
                    if (info4 != null)
                    {
                        _localCache.Add(information.Name, info4);

                        _memoryCache?.Set<ContainerTypeInformation>(info4.Name, info4);

                        if (_distributedCache != null)
                        {
                            string json = JsonSerializer.Serialize<ContainerTypeInformation>(info4);
                            _distributedCache.Set(info4.Name, System.Text.Encoding.UTF8.GetBytes(json));
                        }
                    }
                }                
            }

            return information;
        }

        /// <summary>
        /// Clears our own type-resolution data from all caches
        /// </summary>
        public static void ResetCaches()
        {
            lock (_syncLockObject)
            {
                if (_memoryCache != null)
                {
                    foreach (string key in _localCache.Keys)
                    {
                        _memoryCache.Remove(key);
                    }
                }

                if (_distributedCache != null)
                {
                    foreach (string key in _localCache.Keys)
                    {
                        _distributedCache.Remove(key);
                    }
                }

                _localCache.Clear();
            }            
        }


        /// <summary>
        /// Adds an IMemoryCache to be used.
        /// </summary>
        /// <param name="memoryCache">Implementation of IMemoryCache. This must have been initialised and configured PRIOR to this injection!</param>
        public static void AddCache(IMemoryCache memoryCache)
        {
            if (_memoryCache != null)
            {
                throw new InvalidOperationException("A memory cache has already been added.");
            }
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Adds an IDistributedCache to be used.
        /// </summary>
        /// <param name="distributedCache">Implementation of IDistributedCache. This must have been initialised and configured PRIOR to this injection!</param>
        public static void AddCache(IDistributedCache distributedCache)
        {
            if (_distributedCache != null)
            {
                throw new InvalidOperationException("A distributed cache has already been added.");
            }
            _distributedCache = distributedCache;
        }

        /// <summary>
        /// Static initialise
        /// </summary>
        static TypeDiscoveryFactory()
        {
            _syncLockObject = new object();

            _localCache = new Dictionary<string, ContainerTypeInformation>();
        }

        /// <summary>
        /// Local cache that maintains the type information
        /// </summary>
        private static readonly Dictionary<string, ContainerTypeInformation> _localCache;

        /// <summary>
        /// IMemoryCache that maintains the type information
        /// </summary>
        private static IMemoryCache? _memoryCache = null;

        /// <summary>
        /// IDistributedCache that maintains the type information
        /// </summary>
        private static IDistributedCache? _distributedCache = null;

        /// <summary>
        /// An object for sync locking
        /// </summary>
        private static readonly object _syncLockObject;
    }
}
