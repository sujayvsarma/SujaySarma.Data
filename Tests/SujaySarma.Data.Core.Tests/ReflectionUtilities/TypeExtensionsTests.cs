#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.Core.ReflectionUtilities;

namespace SujaySarma.Data.Core.Tests.ReflectionUtilities
{
    [TestClass]
    [TestCategory("Functional")]
    public class Extensions_TypeExtensionsTests
    {
        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
        private sealed class MyTestAttribute : Attribute
        {
            public string Name { get; }
            public MyTestAttribute(string name) => Name = name;
        }

        [MyTest("on type")]
        private class AttributedClass
        {
            [MyTest("on property")]
            public int MyProperty { get; set; }

            [MyTest("on field")]
            public string MyField = "x";

            private int PrivateProp { get; set; }
        }

        private struct MyStruct { }
        private interface IMyInterface { }
        private delegate void MyDeleg();

        [TestMethod(DisplayName = "TypeExtensions: Get type name (AssemblyQualifiedName or FullName or just Name)")]
        public void GetWellQualifiedTypeName_ReturnsAssemblyQualifiedName()
        {
            string name = typeof(AttributedClass).GetUsableTypeName();
            Assert.AreEqual(typeof(AttributedClass).AssemblyQualifiedName, name);
        }

        [TestMethod(DisplayName = "TypeExtensions: If type is nullable, get underlying type")]
        public void IfNullableGetActualType_Behavior()
        {
            Assert.AreEqual(typeof(int), typeof(int?).IfNullableGetActualType());
            Assert.AreEqual(typeof(string), typeof(string).IfNullableGetActualType());
        }

        [TestMethod(DisplayName = "TypeExtensions: Check if is class, record or struct")]
        public void IsClassRecordOrStruct_VariousTypes()
        {
            Assert.IsTrue(typeof(AttributedClass).IsClassRecordOrStruct());
            Assert.IsTrue(typeof(MyStruct).IsClassRecordOrStruct());
            Assert.IsFalse(typeof(IMyInterface).IsClassRecordOrStruct());
            Assert.IsFalse(typeof(DayOfWeek).IsClassRecordOrStruct()); // enum
            Assert.IsFalse(typeof(int).IsClassRecordOrStruct()); // primitive value type
            Assert.IsFalse(typeof(MyDeleg).IsClassRecordOrStruct()); // delegate
        }

        [TestMethod(DisplayName = "TypeExtensions: Check if is Enumerable type")]
        public void IsEnumerableType_Various()
        {
            Assert.IsTrue(typeof(int[]).IsEnumerableType());
            Assert.IsTrue(typeof(List<string>).IsEnumerableType());
            Assert.IsTrue(typeof(Dictionary<int,int>).IsEnumerableType());
            Assert.IsFalse(typeof(string).IsEnumerableType());
            Type? nullType = null;
            Assert.IsFalse(Core.ReflectionUtilities.TypeExtensions.IsEnumerableType(nullType));
        }

        [TestMethod(DisplayName = "TypeExtensions: Check if is a numeric type")]
        public void IsNumericType_Various()
        {
            Assert.IsTrue(typeof(int).IsNumericType());
            Assert.IsTrue(typeof(double).IsNumericType());
            Assert.IsTrue(typeof(decimal).IsNumericType());
            // Implementation includes Math
            Assert.IsTrue(typeof(Math).IsNumericType());
            Assert.IsFalse(typeof(string).IsNumericType());
        }

        [TestMethod(DisplayName = "TypeExtensions: Check if two given types are nullable equivalents of each other")]
        public void IsNullable_And_IsNullableEquivalentOf()
        {
            Assert.IsTrue(typeof(int?).IsNullable());
            Assert.IsFalse(typeof(string).IsNullable()); // string? is not a Nullable<T>
            Assert.IsTrue(typeof(int?).IsNullableEquivalentOf(typeof(int)));
            Assert.IsFalse(typeof(int?).IsNullableEquivalentOf(typeof(long)));
        }

        [TestMethod(DisplayName = "TypeExtensions: Check if Class B is or inherits from Class A")]
        public void IsOrIsDerivedFrom_And_IsSupportedType()
        {
            Assert.IsTrue(typeof(string).IsOrIsDerivedFrom(typeof(object)));
            Assert.IsTrue(typeof(System.IO.FileStream).IsOrIsDerivedFrom(typeof(System.IO.Stream)));

            var supported = new[] { typeof(int), typeof(string) };
            Assert.IsTrue(typeof(int).IsSupportedType(false, supported));
            Assert.IsTrue(typeof(int?).IsSupportedType(true, supported));
            Assert.IsFalse(typeof(int?).IsSupportedType(false, supported));
        }

        [TestMethod(DisplayName = "TypeExtensions: Get attributes annotated on a class")]
        public void TryGetAttribute_TypeAndMember_Versions()
        {
            // Type-level
            Assert.IsTrue(typeof(AttributedClass).TryGetAttribute(typeof(MyTestAttribute), out Attribute? a1));
            Assert.IsNotNull(a1);
            Assert.IsTrue(a1 is MyTestAttribute && ((MyTestAttribute)a1).Name == "on type");

            Assert.IsTrue(typeof(AttributedClass).TryGetAttribute<MyTestAttribute>(out var attrGeneric));
            Assert.IsNotNull(attrGeneric);
            Assert.AreEqual("on type", attrGeneric!.Name);

            // Property-level
            var pi = typeof(AttributedClass).GetProperty("MyProperty", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(pi);
            Assert.IsTrue(pi!.TryGetAttribute(typeof(MyTestAttribute), out Attribute? a2));
            Assert.IsTrue(a2 is MyTestAttribute && ((MyTestAttribute)a2).Name == "on property");

            Assert.IsTrue(pi.TryGetAttribute<MyTestAttribute>(out var propAttr));
            Assert.IsNotNull(propAttr);
            Assert.AreEqual("on property", propAttr!.Name);

            // Field-level
            var fi = typeof(AttributedClass).GetField("MyField", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(fi);
            Assert.IsTrue(fi!.TryGetAttribute(typeof(MyTestAttribute), out Attribute? a3));
            Assert.IsTrue(a3 is MyTestAttribute && ((MyTestAttribute)a3).Name == "on field");

            Assert.IsTrue(fi.TryGetAttribute<MyTestAttribute>(out var fieldAttr));
            Assert.IsNotNull(fieldAttr);
            Assert.AreEqual("on field", fieldAttr!.Name);
        }

        [TestMethod(DisplayName = "TypeExtensions: Get data type of a member property or field")]
        public void TryGetProperty_Field_And_TryGetPropertyOrFieldDataType()
        {
            // TryGetProperty
            Assert.IsTrue(typeof(AttributedClass).TryGetProperty("MyProperty", BindingFlags.Public | BindingFlags.Instance, out PropertyInfo? pi));
            Assert.IsNotNull(pi);
            Assert.AreEqual(typeof(int), pi!.PropertyType);

            // TryGetField
            Assert.IsTrue(typeof(AttributedClass).TryGetField("MyField", BindingFlags.Public | BindingFlags.Instance, out FieldInfo? fi));
            Assert.IsNotNull(fi);
            Assert.AreEqual(typeof(string), fi!.FieldType);

            // TryGetPropertyOrFieldDataType - property
            Assert.IsTrue(pi.TryGetPropertyOrFieldDataType(out Type? t1));
            Assert.AreEqual(typeof(int), t1);

            // TryGetPropertyOrFieldDataType - field
            Assert.IsTrue(fi.TryGetPropertyOrFieldDataType(out Type? t2));
            Assert.AreEqual(typeof(string), t2);

            // Unsupported member (constructor) should return false
            var ctor = typeof(AttributedClass).GetConstructor(Type.EmptyTypes);
            Assert.IsNotNull(ctor);
            Assert.IsFalse(ctor!.TryGetPropertyOrFieldDataType(out _));
        }
    }
}