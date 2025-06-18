using System;
using System.Collections.Generic;


#if NET8_0_OR_GREATER
    // MemoryCache and DistributedCache are only available in .NET 8.0 and later
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Caching.Memory;
    using System.Text;
    using System.Text.Json;
#endif

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
        /// Local cache that maintains the type information
        /// </summary>
        private static readonly Dictionary<string, ContainerTypeInfo> _localCache;

#if NET8_0_OR_GREATER
        /// <summary>
        /// IMemoryCache that maintains the type information
        /// </summary>
        private static IMemoryCache? _memoryCache;
        
        /// <summary>
        /// IDistributedCache that maintains the type information
        /// </summary>
        private static IDistributedCache? _distributedCache;
        
        /// <summary>
        /// Names of the keys in the distributed cache
        /// </summary>
        private static readonly List<string> _distributedCacheKeyNames;
#endif

        /// <summary>
        /// An object for sync locking
        /// </summary>
        private static readonly object _syncLockObject = new object();

        /// <summary>
        /// Resolve the type information for the provided object
        /// </summary>
        /// <typeparam name="TObject">Type of .NET class, structure or record to resolve</typeparam>
        /// <returns>Type resolution information. Will be Null if appropriate attribute decorations were missing.</returns>
        public static ContainerTypeInfo Resolve<TObject>()
        {
            return Resolve(typeof(TObject));
        }

        /// <summary>
        /// Resolve the type information for the provided object
        /// </summary>
        /// <param name="type">Type to resolve</param>
        /// <returns>Type resolution information. Will be Null if appropriate attribute decorations were missing.</returns>
        public static ContainerTypeInfo Resolve(Type type)
        {
            ContainerTypeInfo? typeInfo = null;
            lock (_syncLockObject)
            {
                if (_localCache.TryGetValue(type.Name, out typeInfo))
                {
                    return typeInfo;
                }

#if NET8_0_OR_GREATER
                if ((_memoryCache != null) && CacheExtensions.TryGetValue(_memoryCache, type.Name, out typeInfo) && (typeInfo != null))
                {
                    return typeInfo;
                }

                if (_distributedCache != null)
                {
                    byte[]? cachedData = _distributedCache.Get(type.Name);
                    if (cachedData?.Length > 0)
                    {
                        typeInfo = JsonSerializer.Deserialize<ContainerTypeInfo>(Encoding.UTF8.GetString(cachedData));
                        if (typeInfo != null)
                        {
                            return typeInfo;
                        }
                    }
                }
#endif

                typeInfo = new ContainerTypeInfo(type);
                _localCache.Add(typeInfo.Name, typeInfo);

#if NET8_0_OR_GREATER
                IMemoryCache? memoryCache = _memoryCache;
                if (memoryCache != null)
                {
                    CacheExtensions.Set<ContainerTypeInfo>(memoryCache, typeInfo.Name, typeInfo);
                }

                if (_distributedCache != null)
                {
                    string str = JsonSerializer.Serialize<ContainerTypeInfo>(typeInfo);
                    _distributedCacheKeyNames.Add(typeInfo.Name);
                    DistributedCacheExtensions.Set(_distributedCache, typeInfo.Name, Encoding.UTF8.GetBytes(str));
                }
#endif
                return typeInfo;
            }
        }

        /// <summary>
        /// Clears our own type-resolution data from all caches
        /// </summary>
        public static void ResetCaches()
        {
            lock (_syncLockObject)
            {
#if NET8_0_OR_GREATER
                if (_memoryCache != null)
                {
                    foreach (string key in _localCache.Keys)
                    {
                        _memoryCache.Remove((object)key);
                    }
                }
                if (_distributedCache != null)
                {
                    foreach (string distributedCacheKeyName in _distributedCacheKeyNames)
                    {
                        _distributedCache.Remove(distributedCacheKeyName);
                    }
                    
                    _distributedCacheKeyNames.Clear();
                }
#endif
                
                _localCache.Clear();
            }
        }

#if NET8_0_OR_GREATER
        /// <summary>
        /// Adds an IMemoryCache to be used.
        /// </summary>
        /// <param name="memoryCache">Implementation of IMemoryCache. This must have been initialised and configured PRIOR to this injection!</param>
        public static void AddCache(IMemoryCache memoryCache)
        {
            _memoryCache = ((_memoryCache == null) ? memoryCache : throw new InvalidOperationException("A memory cache has already been added."));
        }

        /// <summary>
        /// Adds an IDistributedCache to be used.
        /// </summary>
        /// <param name="distributedCache">Implementation of IDistributedCache. This must have been initialised and configured PRIOR to this injection!</param>
        public static void AddCache(IDistributedCache distributedCache)
        {
            _distributedCache = ((_distributedCache == null) ? distributedCache : throw new InvalidOperationException("A distributed cache has already been added."));
        }
#endif

        /// <summary>
        /// Static initialise
        /// </summary>
        static TypeDiscoveryFactory()
        {
            _localCache = new Dictionary<string, ContainerTypeInfo>();

#if NET8_0_OR_GREATER
            _distributedCacheKeyNames = new List<string>();
#endif
        }
    }

}
