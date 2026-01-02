using SujaySarma.Data.Core.ReflectionUtilities;

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace SujaySarma.Data.Core.TypeDiscovery;

/// <summary>
/// Discover and manage metadata about business entity types, members and 
/// their mappings to the backend datastorage containers and container members.
/// </summary>
public static class TypeDiscoveryFactory
{
    /// <summary>
    /// Try to resolve <typeparamref name="T"/> and discover its ORM-related metadata.
    /// </summary>
    /// <typeparam name="T">The <see cref="Type"/> to resolve/discover.</typeparam>
    /// <param name="persistenceContainerInfo">The ORM-related metadata if found, otherwise NULL.</param>
    /// <param name="options">Type discovery options and defaults.</param>
    /// <returns>TRUE if the <typeparamref name="T"/> is a valid ORM-able business entity type, otherwise FALSE.</returns>
    public static bool TryResolve<T>([NotNullWhen(true)] out PersistenceContainerInfo? persistenceContainerInfo, TypeDiscoveryOptions? options = null)
        => TryResolve(typeof(T), out persistenceContainerInfo, options);

    /// <summary>
    /// Try to resolve the <see cref="Type"/> of the business entity <paramref name="obj"/> instance and discover its ORM-related metadata.
    /// </summary>
    /// <param name="obj">Instance of an object the caller is interested to ORM.</param>
    /// <param name="persistenceContainerInfo">The ORM-related metadata if found, otherwise NULL.</param>
    /// <param name="options">Type discovery options and defaults.</param>
    /// <returns>TRUE if the the <see cref="Type"/> of the business entity <paramref name="obj"/> instance is a valid ORM-able business entity type, otherwise FALSE.</returns>
    public static bool TryResolve(object? obj, [NotNullWhen(true)] out PersistenceContainerInfo? persistenceContainerInfo, TypeDiscoveryOptions? options = null)
    {
        if (obj is null)
        {
            throw new ArgumentNullException(nameof(obj), "Cannot be NULL.");
        }

        return TryResolve(obj.GetType(), out persistenceContainerInfo, options);
    }

    /// <summary>
    /// Try to resolve <paramref name="type"/> and discover its ORM-related metadata.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to resolve/discover.</param>
    /// <param name="persistenceContainerInfo">The ORM-related metadata if found, otherwise NULL.</param>
    /// <param name="options">Type discovery options and defaults.</param>
    /// <returns>TRUE if the <paramref name="type"/> is a valid ORM-able business entity type and metadata was retrieved, otherwise FALSE.</returns>
    public static bool TryResolve(Type type, [NotNullWhen(true)] out PersistenceContainerInfo? persistenceContainerInfo, TypeDiscoveryOptions? options = null)
    {
        persistenceContainerInfo = null;

        // Not types we want to use for ORM! (early exit!!)
        if ((!type.IsClassRecordOrStruct()) || type.IsGenericTypeDefinition)
        {
            return false;
        }

        bool wasCachedResult = false;

        SemaphoreSlim semaphore = _typeLocks.GetOrAdd(type, new SemaphoreSlim(1, 1));
        try
        {
            semaphore.Wait();
            
            // Check if is already in cache and exit early.
            wasCachedResult = _cache.TryGetValue(type, out persistenceContainerInfo);
            if (!wasCachedResult)
            {
                // For the alias. 
                // NOTE: DO NOT decrement this again as other threads might have changed it again.

                if (_tableIndex == (uint.MaxValue - 1))
                {
                    throw new InvalidOperationException("TypeDiscoveryFactory has exhausted its table index limit.");
                }

                uint tableIndex = Interlocked.Increment(ref _tableIndex);

                // Discovery...
                persistenceContainerInfo = new PersistenceContainerInfo(type, options ?? TypeDiscoveryOptions.Default, $"[T{tableIndex}]");

                // There's no chance of this failing because we are in a Type-lock,
                // guaranteeing that only *we* can operate on this Type.
                _cache.TryAdd(type, persistenceContainerInfo);
            }
        }
        finally
        {
            semaphore.Release();
        }

        if (wasCachedResult && (persistenceContainerInfo is not null))
        {
            // perform any required checks on a cached result

            if (options is not null)
            {
                // Check if discovered options and current requirements can satisfy this request.
                if (!(persistenceContainerInfo.IsDiscoveredWithEquivalentOptions(options) || persistenceContainerInfo.Satisifes(options.Value)))
                {
                    persistenceContainerInfo = null;
                    return false;
                }
            }
        }

        return (persistenceContainerInfo is not null);
    }

    /// <summary>
    /// A completely lock-free code path that only validates that the provided <paramref name="type"/> meets all 
    /// requirements including the (optionally) provided <paramref name="options"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to validate.</param>
    /// <param name="options">Type discovery options and defaults.</param>
    /// <returns>TRUE if the <paramref name="type"/> is a valid ORM-able business entity type, otherwise FALSE.</returns>
    public static bool TryValidate(Type type, TypeDiscoveryOptions? options = null)
    {
        return PersistenceContainerInfo.ValidateForOrm(type, options ?? _defaultDiscoveryOptions);
    }

    /// <summary>
    /// Initialise the factory. This automatically spins off an asynchronous type discovery 
    /// routine that goes through each assembly loaded for the application and caches 
    /// ORM discoveries.
    /// </summary>
    static TypeDiscoveryFactory()
    {
        _typeLocks = new ConcurrentDictionary<Type, SemaphoreSlim>();
        _cache = new ConcurrentDictionary<Type, PersistenceContainerInfo>();
        _defaultDiscoveryOptions = TypeDiscoveryOptions.Default;
    }

    // LOCKING STRATEGY:
    // - Each Type gets its own SemaphoreSlim in _typeLocks
    // - All _cache access for a Type happens within that Type's semaphore
    // - This allows parallel discovery of different types while preventing
    //   duplicate work and race conditions for the same type
    private static ConcurrentDictionary<Type, PersistenceContainerInfo> _cache;
    private static ConcurrentDictionary<Type, SemaphoreSlim> _typeLocks;
    private static TypeDiscoveryOptions _defaultDiscoveryOptions;

    // This will be serially incremented (never decremented) until uint.MaxValue!
    private static uint _tableIndex = 0;
}
