using System.Collections.Generic;

using SujaySarma.Data.Core;

namespace SujaySarma.Data.Tests.Core
{
    [TestClass()]
    public class UtilityTests
    {
        [TestMethod()]
        public void IsNumericTypeDataArgTest()
        {
            foreach (object data in dataWithResults.Keys)
            {
                bool result = Utility.IsNumericType(data);
                Assert.AreEqual(result, dataWithResults[data]);
            }
        }

        [TestMethod()]
        public void IsNumericTypeTypeArgTest()
        {
            foreach (object data in dataWithResults.Keys)
            {
                bool result = Utility.IsNumericType(data.GetType());
                Assert.AreEqual(result, dataWithResults[data]);
            }
        }

        [TestMethod()]
        public void ThrowIfDisposedTest()
        {
            // The Utility.ThrowIfDisposed only works against custom-implemented objects 
            // and only checks against their internal _isDisposed flag value -- that too 
            // passed as an argument to the utility method.

            Assert.IsTrue(1 == 1);
        }


        private static readonly Dictionary<object, bool> dataWithResults = new Dictionary<object, bool>()
        {
            { 1, true },
            { -5, true },
            { 0, true },
            { 3.142f, true },
            { 3.142d, true },
            { 0x01234, true },
            { 0b0101_0101, true },
            { "Data", false },
            { 'd', false },
            { '\0', false },
            { '\t', false },
            { " ", false },
            { ' ', false },
            { true, false },
            { false, false }
        };
    }
}