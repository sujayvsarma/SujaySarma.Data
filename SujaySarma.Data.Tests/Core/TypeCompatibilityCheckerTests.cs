using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.Core;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SujaySarma.Data.Tests.Core
{
    [TestClass()]
    public class TypeCompatibilityCheckerTests
    {
        [TestMethod()]
        public void IsNullableEquivalentOfTestTrue()
        {
            Assert.IsTrue(typeof(int?).IsNullableEquivalentOf(typeof(int)));
        }

        [TestMethod()]
        public void IsNullableEquivalentOfTestFalse()
        {
            Assert.IsFalse(typeof(string).IsNullableEquivalentOf(typeof(int)));
        }

        [TestMethod()]
        public void IsSupportedTypeTestTrue()
        {
            Assert.IsTrue(typeof(DateTime).IsSupportedType(true, compatibleTypes));
        }

        [TestMethod()]
        public void IsSupportedTypeTestFalse()
        {
            Assert.IsFalse(typeof(StreamWriter).IsSupportedType(true, compatibleTypes));
        }

        private static readonly Type[] compatibleTypes = new Type[]
        {
            typeof(string),
            typeof(byte[]),
            typeof(bool),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(double),
            typeof(Guid),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong)
        };
    }
}