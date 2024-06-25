using System;
using System.Collections.Generic;
using System.Drawing;

using SujaySarma.Data.Core.Reflection;

namespace SujaySarma.Data.Tests.Core.Reflection
{
    [TestClass()]
    public class ReflectionUtilsTests
    {
        [TestMethod()]
        public void GetValueTest()
        {
            Console.WriteLine("class");
            foreach (ContainerMemberTypeInformation member in _classMetadata.Members.Values)
            {
                try
                {
                    object? value = ReflectionUtils.GetValue(ref _classTarget!, member);
                    Console.WriteLine(value);
                }
                catch
                {
                    Assert.Fail($"Failed to retrieve member {member.Name} in {_classMetadata.Name}");
                }
            }

            Console.WriteLine("struct");
            foreach (ContainerMemberTypeInformation member in _structMetadata.Members.Values)
            {
                try
                {
                    object? value = ReflectionUtils.GetValue(ref _structTarget!, member);
                    Console.WriteLine(value);
                }
                catch
                {
                    Assert.Fail($"Failed to retrieve member {member.Name} in {_structMetadata.Name}");
                }
            }

            Console.WriteLine("record");
            foreach (ContainerMemberTypeInformation member in _recordMetadata.Members.Values)
            {
                try
                {
                    object? value = ReflectionUtils.GetValue(ref _recordTarget!, member);
                    Console.WriteLine(value);
                }
                catch
                {
                    Assert.Fail($"Failed to retrieve member {member.Name} in {_recordMetadata.Name}");
                }
            }
        }

        [TestMethod()]
        public void SetValueCommonTest()
        {
            string value = Guid.NewGuid().ToString("d");
            
            ReflectionUtils.SetValue(ref _classTarget!, _classMetadata.Members["Value1"], value);
            Assert.AreEqual(((ReflectionTestsClassTarget)_classTarget).Value1, value);

            ReflectionUtils.SetValue(ref _structTarget!, _structMetadata.Members["Value1"], value);
            Assert.AreEqual(((ReflectionTestsStructTarget)_structTarget).Value1, value);

            ReflectionUtils.SetValue(ref _recordTarget!, _recordMetadata.Members["Value1"], value);
            Assert.AreEqual(((ReflectionTestsRecordTarget)_recordTarget).Value1, value);
        }

        [TestMethod()]
        public void SetValuePropertyTest()
        {
            List<int> value = new List<int>() { 1, 2, 3 };

            ReflectionUtils.SetValue(ref _classTarget!, _classMetadata.Members["Value2"], value);
            Assert.AreEqual(((ReflectionTestsClassTarget)_classTarget).Value2, value);

            ReflectionUtils.SetValue(ref _recordTarget!, _recordMetadata.Members["Value2"], value);
            Assert.AreEqual(((ReflectionTestsRecordTarget)_recordTarget).Value2, value);

            // Cannot do this for Struct target, because it does not have "properties"
        }

        [TestMethod()]
        public void SetValueFieldTest()
        {
            List<int> value = new List<int>() { 1, 2, 3 };

            ReflectionUtils.SetValue(ref _structTarget!, _structMetadata.Members["Value2"], value);
            Assert.AreEqual(((ReflectionTestsStructTarget)_structTarget).Value2, value);

            // Cannot do for Class/Record targets, because they do not have "fields" in this UT
        }

        [TestMethod()]
        public void ConvertValueIfRequiredTest()
        {
            Guid g = Guid.NewGuid();
            Guid result_g = (Guid)ReflectionUtils.ConvertValueIfRequired(g.ToString("d"), typeof(Guid));
            Assert.AreEqual(result_g, g);

            int i = 10;
            int result_i = (int)ReflectionUtils.ConvertValueIfRequired(i.ToString(), typeof(int));
            Assert.AreEqual(result_i, i);

            string json = "[1, 2, 3]";
            List<int> result_json = (List<int>)ReflectionUtils.ConvertValueIfRequired(json, typeof(List<int>));
            Assert.IsTrue((result_json.Count == 3) && (result_json[0] == 1) && (result_json[1] == 2) && (result_json[2] == 3));
        }

        [TestMethod()]
        public void GetFieldOrPropertyDataTypeTest()
        {
            Type type = ReflectionUtils.GetFieldOrPropertyDataType(_classMetadata.Members["Value1"].FieldOrPropertyInfo);
            Assert.IsTrue(type == typeof(string));
        }

        [TestMethod()]
        public void ConvertToTest()
        {
            // Primitive types
            string v_1 = "1";
            int result_v1 = (int)ReflectionUtils.ConvertTo(typeof(int), v_1);
            Assert.AreEqual(v_1, result_v1.ToString());

            // Enums
            string v_2 = "Monday";
            DayOfWeek result_v2 = (DayOfWeek)ReflectionUtils.ConvertTo(typeof(DayOfWeek), v_2);
            Assert.AreEqual(v_2, result_v2.ToString());

            // Color (since we have a specific converter if it is a Hex value)
            int v_3 = 0xfcfcfc;
            Color result_v3 = (Color)ReflectionUtils.ConvertTo(typeof(Color), v_3);
            Assert.AreEqual(v_3, result_v3.ToArgb());

            // DateTime... (this runs through our DateTimeUtilities class)
            DateTime v_4 = DateTime.Now;
            DateTimeOffset result_v4 = (DateTimeOffset)ReflectionUtils.ConvertTo(typeof(DateTimeOffset), v_4);
            Assert.AreEqual(v_4, result_v4.DateTime);

            // Something with a Parse/TryParse...
            string v_5 = Guid.NewGuid().ToString("d");
            Guid result_v5 = (Guid)ReflectionUtils.ConvertTo(typeof(Guid), v_5);
            Assert.AreEqual(v_5, result_v5.ToString("d"));

            // Json
            string json = "[1, 2, 3]";
            List<int> result_json = (List<int>)ReflectionUtils.ConvertTo(typeof(List<int>), json);
            Assert.IsTrue((result_json.Count == 3) && (result_json[0] == 1) && (result_json[1] == 2) && (result_json[2] == 3));
        }


        public ReflectionUtilsTests()
        {
            _classTarget = new ReflectionTestsClassTarget();
            _structTarget = new ReflectionTestsStructTarget();
            _recordTarget = new ReflectionTestsRecordTarget(Guid.Empty, DateTime.MinValue, DateTime.MinValue);

            _classMetadata = TypeDiscoveryFactory.Resolve<ReflectionTestsClassTarget>();
            _recordMetadata = TypeDiscoveryFactory.Resolve<ReflectionTestsRecordTarget>();
            _structMetadata = TypeDiscoveryFactory.Resolve<ReflectionTestsStructTarget>();
        }

        private object _classTarget;
        private object _structTarget;
        private object _recordTarget;

        private readonly ContainerTypeInformation _classMetadata, _recordMetadata, _structMetadata;
    }
}