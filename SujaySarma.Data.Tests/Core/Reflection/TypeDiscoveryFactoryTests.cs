using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

using SujaySarma.Data.Core.Reflection;

namespace SujaySarma.Data.Tests.Core.Reflection
{
    [TestClass()]
    public class TypeDiscoveryFactoryTests
    {
        [TestMethod()]
        public void ResolveByGenericsTest()
        {
            ContainerTypeInformation? metadata1 = TypeDiscoveryFactory.Resolve<ReflectionTestsClassTarget>();
            ContainerTypeInformation? metadata2 = TypeDiscoveryFactory.Resolve<ReflectionTestsStructTarget>();
            ContainerTypeInformation? metadata3 = TypeDiscoveryFactory.Resolve<ReflectionTestsRecordTarget>();

            Assert.IsTrue(
                    (metadata1 != null) && (metadata2 != null) && (metadata3 != null) 
                    && (metadata1.Members.Count == 5) && (metadata2.Members.Count == 5) && (metadata3.Members.Count == 5)
                );
        }

        [TestMethod()]
        public void ResolveByTypeArgumentTest()
        {
            ContainerTypeInformation? metadata1 = TypeDiscoveryFactory.Resolve(typeof(ReflectionTestsClassTarget));
            ContainerTypeInformation? metadata2 = TypeDiscoveryFactory.Resolve(typeof(ReflectionTestsStructTarget));
            ContainerTypeInformation? metadata3 = TypeDiscoveryFactory.Resolve(typeof(ReflectionTestsRecordTarget));

            Assert.IsTrue(
                    (metadata1 != null) && (metadata2 != null) && (metadata3 != null)
                    && (metadata1.Members.Count == 5) && (metadata2.Members.Count == 5) && (metadata3.Members.Count == 5)
                );
        }

        [TestMethod()]
        public void ResetCachesTest()
        {
            // We have no way to externally examine its caches.
            // Only this should not fail!
            TypeDiscoveryFactory.ResetCaches();
            Assert.IsTrue(1 == 1);
        }

        [TestMethod()]
        public void AddIMemoryCacheTest()
        {
            // We have no way to externally examine its caches.
            // Only this should not fail!

            TestMemoryCache cache = new TestMemoryCache();
            TypeDiscoveryFactory.AddCache(cache);
            Assert.IsTrue(1 == 1);
        }

        [TestMethod()]
        public void AddIDistributedCacheTest()
        {
            // We have no way to externally examine its caches.
            // Only this should not fail!
            TestDistributedCache cache = new TestDistributedCache();
            TypeDiscoveryFactory.AddCache(cache);
            Assert.IsTrue(1 == 1);
        }


        public TypeDiscoveryFactoryTests()
        {
            _classTarget = new ReflectionTestsClassTarget();
            _structTarget = new ReflectionTestsStructTarget();
            _recordTarget = new ReflectionTestsRecordTarget(Guid.Empty, DateTime.MinValue, DateTime.MinValue);
        }

        private readonly ReflectionTestsClassTarget _classTarget;
        private readonly ReflectionTestsStructTarget _structTarget;
        private readonly ReflectionTestsRecordTarget _recordTarget;


        // Dummy!
        private class TestDistributedCache : IDistributedCache
        {
            public byte[]? Get(string key)
            {
                return Array.Empty<byte>();
            }

            public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
            {
                return Task.FromResult<byte[]?>(Array.Empty<byte>());
            }

            public void Refresh(string key)
            {
            }

            public Task RefreshAsync(string key, CancellationToken token = default)
            {
                return Task.CompletedTask;
            }

            public void Remove(string key)
            {
            }

            public Task RemoveAsync(string key, CancellationToken token = default)
            {
                return Task.CompletedTask;
            }

            public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
            {
            }

            public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
            {
                return Task.CompletedTask;
            }
        }



        // Dummy!
        private class TestMemoryCache : IMemoryCache
        {
            public ICacheEntry CreateEntry(object key)
            {
                return (ICacheEntry)key;
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public void Remove(object key)
            {
                throw new NotImplementedException();
            }

            public bool TryGetValue(object key, out object? value)
            {
                value = null;
                return false;
            }
        }
    }
}