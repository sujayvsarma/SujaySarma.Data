using System;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.Core.Attributes;
using SujaySarma.Data.Core.Tests.Objects;
using SujaySarma.Data.Core.TypeDiscovery;

namespace SujaySarma.Data.Core.Tests.TypeDiscovery;

[TestClass]
[TestCategory("Functional")]
public class PersistenceContainerInfoTests
{
    private OrmReadyClass? instance;
    private PersistenceContainerInfo? containerInfo;
    private TypeDiscoveryOptions options;

    [TestInitialize]
    public void TestInit()
    {
        instance = new OrmReadyClass();
        options = new TypeDiscoveryOptions()
        {
            EntityMustImplement = (new List<Type>()).AsReadOnly(),
            MustHaveAtLeastOneMember = true,
            PersistenceContainerAttributeRestriction = typeof(PersistenceContainer),
            PersistenceContainerMemberAttributeRestriction = typeof(PersistenceContainerMember)
        };

        TypeDiscoveryFactory.TryResolve(instance, out containerInfo, options);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        instance = null;
    }

    [TestMethod(DisplayName = "PersistenceContainerInfo: Correct population")]
    public void PersistenceContainerInfoPopulation()
    {
        Assert.IsNotNull(containerInfo);
        Assert.IsTrue(Type.Equals(typeof(OrmReadyClass), containerInfo.EntityType));
        Assert.IsTrue(containerInfo.PersistenceInfo.GetType().IsAssignableFrom(typeof(PersistenceContainer)));
        Assert.AreEqual("[T1]", containerInfo.ReferenceAlias);

        Assert.IsGreaterThanOrEqualTo<int>(0, containerInfo.Attributes.Count);
        Assert.HasCount(4, containerInfo.Members);
    }

    [TestMethod(DisplayName = "PersistenceContainerInfo: Check satisfies original options")]
    public void InternalSatisfiesOptionsOriginal()
    {
        MethodInfo? satisfiesMethod = typeof(PersistenceContainerInfo).GetMethod("Satisifes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.IsNotNull(satisfiesMethod);

        bool result = (bool?)satisfiesMethod.Invoke(containerInfo, new object?[] { options }) ?? false;
        Assert.IsTrue(result);
    }

    [TestMethod(DisplayName = "PersistenceContainerInfo: Check does not satisfy different options")]
    public void InternalSatisfiesOptionsDifferent()
    {
        MethodInfo? satisfiesMethod = typeof(PersistenceContainerInfo).GetMethod("Satisifes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.IsNotNull(satisfiesMethod);

        TypeDiscoveryOptions tdo = new TypeDiscoveryOptions()
        {
            EntityMustImplement = (new List<Type>() { typeof(IDisposable) }).AsReadOnly(),
            MustHaveAtLeastOneMember = false
        };

        bool result = (bool?)satisfiesMethod.Invoke(containerInfo, new object?[] { tdo }) ?? false;
        Assert.IsFalse(result);
    }

    [TestMethod(DisplayName = "PersistenceContainerInfo: Check does not satisfy default options")]
    public void InternalSatisfiesOptionsDefault()
    {
        MethodInfo? satisfiesMethod = typeof(PersistenceContainerInfo).GetMethod("Satisifes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.IsNotNull(satisfiesMethod);

        TypeDiscoveryOptions tdo = new TypeDiscoveryOptions();

        bool result = (bool?)satisfiesMethod.Invoke(containerInfo, new object?[] { tdo }) ?? false;
        Assert.IsFalse(result);
    }

    [TestMethod(DisplayName = "PersistenceContainerInfo: Retrieve attributes on entity class")]
    public void TestAttributeRetrieval()
    {
        Assert.IsNotNull(containerInfo);

        bool result = containerInfo.TryGetAttribute(typeof(System.ComponentModel.DataAnnotations.Schema.TableAttribute), out Attribute? attribute);
        Assert.IsTrue(result);
        Assert.IsNotNull(attribute);
        Assert.IsTrue(Type.Equals(typeof(System.ComponentModel.DataAnnotations.Schema.TableAttribute), attribute.GetType()));

        result = containerInfo.TryGetAttribute(typeof(PersistenceContainerMember), out attribute);
        Assert.IsFalse(result);
    }

    [TestMethod(DisplayName = "PersistenceContainerInfo: Enumerate members retrieved from entity class")]
    public void TestTryGetMember()
    {
        PersistenceContainerMemberInfo? memberInfo;

        Assert.IsNotNull(containerInfo);

        Assert.IsTrue(containerInfo.TryGetMember("Id", out memberInfo));
        Assert.AreEqual("Id", memberInfo.Member.Name);

        Assert.IsTrue(containerInfo.TryGetMember("Name", out memberInfo));
        Assert.AreEqual("Name", memberInfo.Member.Name);

        Assert.IsTrue(containerInfo.TryGetMember("LastModified", out memberInfo));
        Assert.AreEqual("LastModified", memberInfo.Member.Name);

        Assert.IsTrue(containerInfo.TryGetMember("_internalField", out memberInfo));
        Assert.AreEqual("_internalField", memberInfo.Member.Name);
    }

    [TestMethod(DisplayName = "PersistenceContainerInfo: Enumerate members (using persistence names) retrieved from entity class")]
    public void TestTryGetMemberByPersistenceName()
    {
        PersistenceContainerMemberInfo? memberInfo;

        Assert.IsNotNull(containerInfo);

        Assert.IsTrue(containerInfo.TryGetMemberByPersistenceColumnName("Id", out memberInfo));
        Assert.AreEqual("Id", memberInfo.Member.Name);

        Assert.IsTrue(containerInfo.TryGetMemberByPersistenceColumnName("Name", out memberInfo));
        Assert.AreEqual("Name", memberInfo.Member.Name);

        Assert.IsTrue(containerInfo.TryGetMemberByPersistenceColumnName("LastModified", out memberInfo));
        Assert.AreEqual("LastModified", memberInfo.Member.Name);

        Assert.IsTrue(containerInfo.TryGetMemberByPersistenceColumnName("InternalField", out memberInfo));
        Assert.AreEqual("_internalField", memberInfo.Member.Name);
    }

    [TestMethod(DisplayName = "PersistenceContainerInfo: Enumerate members (using annotated attributes) retrieved from entity class")]
    public void TryGetMembersWithAttributes()
    {
        PersistenceContainerMemberInfo[] memberInfos;

        Assert.IsNotNull(containerInfo);

        Assert.IsTrue(containerInfo.TryGetMembers(new Type[] { typeof(OrmPopulatedGuidField) }, true, out memberInfos));
        Assert.HasCount(1, memberInfos);

        Assert.IsTrue(containerInfo.TryGetMembers(new Type[] { typeof(OrmPopulatedTimestampField) }, true, out memberInfos));
        Assert.HasCount(1, memberInfos);

        Assert.IsFalse(containerInfo.TryGetMembers(new Type[] { typeof(System.ComponentModel.DataAnnotations.Schema.ColumnAttribute) }, true, out memberInfos));
        Assert.HasCount(0, memberInfos);
    }

    [TestMethod(DisplayName = "PersistenceContainerMemberInfo: Correct population")]
    public void PersistenceContainerMemberInfoPopulation()
    {
        Assert.IsNotNull(containerInfo);

        PersistenceContainerMemberInfo? memberInfo;
        bool result = containerInfo.TryGetMember("Id", out memberInfo);
        Assert.IsTrue(result);
        Assert.IsNotNull(memberInfo);

        Assert.AreEqual("Id", memberInfo.Member.Name);
        Assert.IsTrue(Type.Equals(memberInfo.PersistenceInfo.GetType(), typeof(PersistenceContainerMember)));
        Assert.IsGreaterThanOrEqualTo<int>(0, memberInfo.Attributes.Count);

        Assert.IsTrue(memberInfo.TryGetAttribute(typeof(OrmPopulatedGuidField), out Attribute? attribute));
        Assert.IsNotNull(attribute);
        Assert.IsTrue(Type.Equals(typeof(OrmPopulatedGuidField), attribute.GetType()));
    }

}
