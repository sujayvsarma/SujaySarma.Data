using System;
using System.Linq;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.Core.Attributes;
using SujaySarma.Data.Core.ReflectionUtilities;
using SujaySarma.Data.Core.Tests.Objects;
using SujaySarma.Data.Core.TypeDiscovery;

namespace SujaySarma.Data.Core.Tests.TypeDiscovery;

[TestClass]
[TestCategory("Functional")]
public sealed class TypeDiscoveryTests
{
    private OrmReadyClass? instance;

    [TestInitialize]
    public void TestInit()
    {
        instance = new OrmReadyClass();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        instance = null;
    }

    [TestMethod(DisplayName = "TypeDiscoveryFactory: TryResolve(type): default options")]
    public void TestTypeDiscoveryWithDefaultOptions()
    {
        if (!TypeDiscoveryFactory.TryResolve(typeof(OrmReadyClass), out PersistenceContainerInfo? pci))
        {
            Assert.Fail("Resolution of 'OrmReady' failed.");
        }

        Assert.IsTrue(Type.Equals(pci.EntityType, typeof(OrmReadyClass)));
        Assert.IsTrue(Type.Equals(pci.PersistenceInfo.GetType(), typeof(PersistenceContainer)));
        Assert.IsFalse(pci.Attributes.Any(a => a.GetType().IsOrIsDerivedFrom(typeof(IPersistenceContainer))), $"Attributes returned: [{pci.Attributes.Count}] (correct: zero)");
        Assert.AreEqual("[T1]", pci.ReferenceAlias, $"Actual alias: [{pci.ReferenceAlias}] (correct: 'T0')");
        Assert.HasCount(4, pci.Members, $"Found count: {pci.Members.Count}/3");
    }

    [TestMethod(DisplayName = "TypeDiscoveryFactory: TryResolve(instance): default options")]
    public void TestTypeDiscoveryOfInstanceWithDefaultOptions()
    {
        if (!TypeDiscoveryFactory.TryResolve(instance, out PersistenceContainerInfo? pci))
        {
            Assert.Fail("Resolution of 'OrmReady' failed.");
        }

        Assert.IsTrue(Type.Equals(pci.EntityType, typeof(OrmReadyClass)));
        Assert.IsTrue(Type.Equals(pci.PersistenceInfo.GetType(), typeof(PersistenceContainer)));
        Assert.IsFalse(pci.Attributes.Any(a => a.GetType().IsOrIsDerivedFrom(typeof(IPersistenceContainer))), $"Attributes returned: [{pci.Attributes.Count}] (correct: zero)");
        Assert.AreEqual("[T1]", pci.ReferenceAlias, $"Actual alias: [{pci.ReferenceAlias}] (correct: 'T0')");
        Assert.HasCount(4, pci.Members, $"Found count: {pci.Members.Count}/3");
    }

    [TestMethod(DisplayName = "TypeDiscoveryFactory: (Multithreaded) TryResolve(type) & TryResolve(instance): default options")]
    public void TestMultiThreadedTypeRetrieval()
    {
        const int totalThreads = 100_000;
        int passCount = 0;
        try
        {
            for (int i = 0; i < totalThreads; i++)
            {
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    if ((i % 2) == 0)
                    {
                        if (TypeDiscoveryFactory.TryResolve(instance, out _))
                        {
                            Interlocked.Increment(ref passCount);
                        }
                    }
                    else
                    {
                        if (TypeDiscoveryFactory.TryResolve(typeof(OrmReadyClass), out _))
                        {
                            Interlocked.Increment(ref passCount);
                        }
                    }
                });
            }

            while (ThreadPool.PendingWorkItemCount > 0)
            {
                Thread.Sleep(10);
            }
        }
        catch
        {
            // Threads locked, probably?
            Assert.Fail("Threads probably locked etc? Test failed!");
        }

        Assert.AreEqual(0, ThreadPool.PendingWorkItemCount, $"All threads completed ({ThreadPool.PendingWorkItemCount}).");
        Assert.AreEqual(totalThreads, passCount, $"All threads were successful ({passCount}/{totalThreads}).");
    }

    [TestMethod(DisplayName = "TypeDiscoveryFactory: Reflection: Value retrieval (instance).")]
    public void TestCorrectValueRetrieval()
    {
        if (! TypeDiscoveryFactory.TryResolve(instance, out PersistenceContainerInfo? containerInfo))
        {
            Assert.Fail("Could not retrieve PCI.");
        }

        int foundProperties = 0;
        foreach(PersistenceContainerMemberInfo member in containerInfo.Members)
        {
            switch (member.Member.Name)
            {
                case "Id":
                    ++foundProperties;
                    Guid? id = (Guid?)instance.GetValue(member, true);
                    Assert.IsNotNull(id, $"Guid value: {id}.");
                    Assert.AreNotEqual(Guid.Empty, id, $"Guid value: {id}.");
                    break;

                case "Name":
                    ++foundProperties;
                    string? s = (string?)instance.GetValue(member);
                    Assert.IsNotNull(s, $"Name: {s}.");
                    Assert.AreEqual("Sujay Sarma", s, $"Name: {s}.");
                    break;

                case "LastModified":
                    ++foundProperties;
                    DateTime? dt = (DateTime?)instance.GetValue(member);
                    Assert.IsNotNull(dt, $"LastModified: {dt:yyyy-MM-dd HH:mm:ss}.");
                    Assert.AreEqual(DateTimeKind.Utc, dt.Value.Kind, $"LastModified: {dt.Value.Kind}.");
                    break;

                case "_internalField":
                    ++foundProperties;
                    int? i = (int?)instance.GetValue(member);
                    Assert.IsNotNull(i, $"_internalField: {i}");
                    Assert.AreEqual(99, i.Value, $"_internalField: {i}");
                    break;
            }
        }

        Assert.AreEqual(4, foundProperties, $"Found members: ({foundProperties}/4).");
    }

}
