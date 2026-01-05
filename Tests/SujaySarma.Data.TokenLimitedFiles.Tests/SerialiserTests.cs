using System;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.TokenLimitedFiles.Tests.Objects;

namespace SujaySarma.Data.TokenLimitedFiles.Tests;

[TestClass]
public class SerialiserTests
{

    [TestMethod(DisplayName = "Serialiser: Serialise value")]
    [TestCategory("Functional")]
    public void SerialiseValueTest()
    {
        OrmReadyClass instance = new OrmReadyClass();
        Serialiser serialiser = Serialiser.For(typeof(OrmReadyClass));
        Assert.AreEqual("\"Sujay Sarma\"", Serialiser.SerialiseValue(instance.Name));
    }

    [TestMethod(DisplayName = "Serialiser: Duplicate 'Position' values in header")]
    [TestCategory("Functional")]
    public void SerialiseHeadersDuplicatePositionsTest()
    {
        // This does not throw or return any errors. Duplicate positions are acceptable!
        Serialiser.For(typeof(OrmUnreadyClass));
    }

    [TestMethod(DisplayName = "Serialiser: Gaps in 'Position' values in header")]
    [TestCategory("Functional")]
    public void SerialiseHeadersGappedPositionsTest()
    {
        Serialiser serialiser = Serialiser.For(typeof(OrmReadyClassWithGaps));
        string[] data = serialiser.SerialiseHeaders();

        Assert.HasCount(8, data);
        Assert.AreEqual("\"Id\"", data[0]);
        Assert.AreEqual("\"Name\"", data[1]);
        Assert.AreEqual("\"\"", data[2]);
        Assert.AreEqual("\"\"", data[3]);
        Assert.AreEqual("\"LastModified\"", data[4]);
        Assert.AreEqual("\"\"", data[5]);
        Assert.AreEqual("\"\"", data[6]);
        Assert.AreEqual("\"InternalField\"", data[7]);
    }

    [TestMethod(DisplayName = "Serialiser: Serialise an entity with named headers")]
    [TestCategory("Functional")]
    public void SerialiseEntityNamedHeadersTest()
    {
        OrmReadyClass entity = new OrmReadyClass();
        Serialiser serialiser = Serialiser.For(typeof(OrmReadyClass));        

        string[] headers = serialiser.SerialiseHeaders();
        Assert.HasCount(4, headers);
        Assert.AreEqual("\"Id\"", headers[0]);
        Assert.AreEqual("\"Name\"", headers[1]);
        Assert.AreEqual("\"LastModified\"", headers[2]);
        Assert.AreEqual("\"InternalField\"", headers[3]);

        string[] values = serialiser.SerialiseEntity(entity);
        Assert.HasCount(4, values);
        Assert.AreNotEqual(Guid.Empty.ToString("d"), values[0]);
        Console.WriteLine($">> Autogen Id: {values[0]}");
        Assert.AreEqual("\"Sujay Sarma\"", values[1]);
        Assert.AreNotEqual($"{DateTime.MinValue}", values[2]);
        Console.WriteLine($">> Autogen LastModified: {values[2]}");
        Assert.AreEqual("99", values[3]);
    }

    [TestMethod(DisplayName = "Serialiser: Serialise an entity with indexed fields")]
    [TestCategory("Functional")]
    public void SerialiseEntityIndexedFieldsTest()
    {
        OrmReadyClassWithGaps entity = new OrmReadyClassWithGaps();
        Serialiser serialiser = Serialiser.For(typeof(OrmReadyClassWithGaps));

        string[] headers = serialiser.SerialiseHeaders();
        Assert.HasCount(8, headers);
        Assert.AreEqual("\"Id\"", headers[0]);
        Assert.AreEqual("\"Name\"", headers[1]);
        Assert.AreEqual("\"\"", headers[2]);
        Assert.AreEqual("\"\"", headers[3]);
        Assert.AreEqual("\"LastModified\"", headers[4]);
        Assert.AreEqual("\"\"", headers[5]);
        Assert.AreEqual("\"\"", headers[6]);
        Assert.AreEqual("\"InternalField\"", headers[7]);

        string[] values = serialiser.SerialiseEntity(entity);
        Assert.HasCount(8, values);
        Assert.AreNotEqual(Guid.Empty.ToString("d"), values[0]);
        Console.WriteLine($">> Autogen Id: {values[0]}");
        Assert.AreEqual("\"Sujay Sarma\"", values[1]);
        Assert.AreEqual("\"\"", values[2]);
        Assert.AreEqual("\"\"", values[3]);
        Assert.AreNotEqual($"{DateTime.MinValue}", values[4]);
        Console.WriteLine($">> Autogen LastModified: {values[4]}");
        Assert.AreEqual("\"\"", values[5]);
        Assert.AreEqual("\"\"", values[6]);
        Assert.AreEqual("99", values[7]);
    }

    [TestMethod(DisplayName = "Serialiser: De-serialise an entity with named headers")]
    [TestCategory("Functional")]
    public void DeserialiseEntityNamedFieldsTest()
    {
        Serialiser serialiser = Serialiser.For(typeof(OrmReadyClass));

        string[] headers = serialiser.SerialiseHeaders();
        Assert.HasCount(4, headers);
        Assert.AreEqual("\"Id\"", headers[0]);
        Assert.AreEqual("\"Name\"", headers[1]);
        Assert.AreEqual("\"LastModified\"", headers[2]);
        Assert.AreEqual("\"InternalField\"", headers[3]);

        string[] values = new string[]
        {
            "1a5a9bb2-2639-4a1b-b8f5-b930dcd9bb4a",
            "Test Name",
            "31-12-2025 23:59:59",
            "-1"
        };

        object instance = serialiser.Deserialise(values);
        Assert.IsNotNull(instance);
        Assert.IsTrue((instance.GetType() == typeof(OrmReadyClass)));

        OrmReadyClass orc = (OrmReadyClass)instance;
        Assert.AreEqual(values[0], typeof(OrmReadyClass).GetProperty("Id", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(orc)?.ToString());
        Assert.AreEqual(values[1], orc.Name);
        Assert.AreEqual(values[2], $"{orc.LastModified}");
        Assert.AreEqual(values[3], orc._internalField.ToString());
    }

}
