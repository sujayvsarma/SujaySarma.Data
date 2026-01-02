#nullable enable
using System;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.Core.ReflectionUtilities;

namespace SujaySarma.Data.Core.Tests.ReflectionUtilities
{
    [TestClass]
    [TestCategory("Functional")]
    public class Extensions_ValueExtensionsTests
    {
        private sealed class PersonDto
        {
            public string? Name { get; set; }
            public int Age { get; set; }
        }

        private class ReflectionTestEntity
        {
            public int Age { get; set; }
            public string? Name;
            public static string? GlobalValue;
        }

        [TestMethod(DisplayName = "ValueExtensions: Check if value is NULL (including DBNull)")]
        public void IsNull_NullAndDbNull()
        {
            object? n = null;
            Assert.IsTrue(n.IsNull());

            object db = DBNull.Value;
            Assert.IsTrue(db.IsNull());

            object non = 5;
            Assert.IsFalse(non.IsNull());
        }

        [TestMethod(DisplayName = "ValueExtensions: Get default value for given type")]
        public void GetDefault_IntAndAbstractType()
        {
            var defInt = typeof(int).GetDefault();
            Assert.IsNotNull(defInt);
            Assert.IsInstanceOfType(defInt, typeof(int));
            Assert.AreEqual(0, (int)defInt!);

            // Abstract/system type should fallback to default (null)
            var defStream = typeof(System.IO.Stream).GetDefault();
            Assert.IsNull(defStream);
        }

        [TestMethod(DisplayName = "ValueExtensions: Convert between types: string->int, int->string, including NULL")]
        public void ConvertTo_StringToInt_And_IntToString_And_Null()
        {
            object? res = "123".ConvertTo(typeof(int));
            Assert.IsNotNull(res);
            Assert.IsInstanceOfType(res, typeof(int));
            Assert.AreEqual(123, (int)res!);

            object? s = 42.ConvertTo(typeof(string));
            Assert.IsNotNull(s);
            Assert.IsInstanceOfType(s, typeof(string));
            Assert.AreEqual("42", (string)s!);

            object? nul = ((object?)null).ConvertTo(typeof(int));
            Assert.IsNull(nul);
        }

        [TestMethod(DisplayName = "ValueExtensions: Convert between types: same target type, must return as-is")]
        public void ConvertTo_SameType_ReturnsSameInstance()
        {
            object o = 7;
            var converted = o.ConvertTo(typeof(int));
            Assert.AreSame(o, converted);
        }

        [TestMethod(DisplayName = "ValueExtensions: Convert between types: json input string to an object")]
        public void ConvertTo_JsonToObject()
        {
            var dto = new PersonDto { Name = "Alice", Age = 30 };
            string json = System.Text.Json.JsonSerializer.Serialize(dto);

            object? des = json.ConvertTo(typeof(PersonDto));
            Assert.IsNotNull(des);
            Assert.IsInstanceOfType(des, typeof(PersonDto));

            var p = (PersonDto)des!;
            Assert.AreEqual("Alice", p.Name);
            Assert.AreEqual(30, p.Age);
        }

        private enum SampleEnum { Zero = 0, One = 1 }

        [TestMethod(DisplayName = "ValueExtensions: Conert between types: int to Enum type")]
        public void ConvertTo_EnumDestinationWithInt_ReturnsIntPerImplementation()
        {
            object? res = 1.ConvertTo(typeof(SampleEnum));
            // Current implementation returns the incoming int when destination is enum and input is int
            Assert.IsNotNull(res);
            Assert.IsInstanceOfType(res, typeof(int));
            Assert.AreEqual(1, (int)res!);
        }

        [TestMethod(DisplayName = "ValueExtensions: Get/set values from/to class member properties and fields")]
        public void GetValueAndSetValue_PropertyAndField_Works()
        {
            var ent = new ReflectionTestEntity();

            PropertyInfo pi = typeof(ReflectionTestEntity).GetProperty(nameof(ReflectionTestEntity.Age))!;
            FieldInfo fi = typeof(ReflectionTestEntity).GetField(nameof(ReflectionTestEntity.Name))!;
            FieldInfo staticFi = typeof(ReflectionTestEntity).GetField(nameof(ReflectionTestEntity.GlobalValue))!;

            // Set property
            ent.SetValue(pi, 55);
            var ageVal = ent.GetValue(pi);
            Assert.IsNotNull(ageVal);
            Assert.IsInstanceOfType(ageVal, typeof(int));
            Assert.AreEqual(55, (int)ageVal!);

            // Set instance field
            ent.SetValue(fi, "Bob");
            var nameVal = ent.GetValue(fi);
            Assert.IsNotNull(nameVal);
            Assert.IsInstanceOfType(nameVal, typeof(string));
            Assert.AreEqual("Bob", (string)nameVal!);

            // Set static field using null instance
            Core.ReflectionUtilities.ValueExtensions.SetValue(null, staticFi, "GLOBAL");
            var g = Core.ReflectionUtilities.ValueExtensions.GetValue(null, staticFi);
            Assert.IsNotNull(g);
            Assert.IsInstanceOfType(g, typeof(string));
            Assert.AreEqual("GLOBAL", (string)g!);
        }

        [TestMethod(DisplayName = "ValueExtensions: Attempting to set property value on NULL parent should throw exception")]
        public void SetValue_Property_ThrowsOnNullInstance()
        {
            PropertyInfo pi = typeof(ReflectionTestEntity).GetProperty(nameof(ReflectionTestEntity.Age))!;
            Assert.Throws<ArgumentNullException>(() => Core.ReflectionUtilities.ValueExtensions.SetValue(null, pi, 10));
        }
    }
}