using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SujaySarma.Data.Tests.Core
{
    [TestClass()]
    public class DateTimeUtilitiesTests
    {
        [TestMethod()]
        public void ConvertDateTimeOffsetToDateTimeTest()
        {
            DateTime now = DateTime.Now;
            DateTimeOffset dto = new DateTimeOffset(now, TimeSpan.FromMinutes(330));
            DateTime result = DateTimeUtilities.ConvertDateTimeOffsetToDateTime(dto);
            
            Assert.IsTrue(result == now);
        }

        [TestMethod()]
        public void ConvertDateOnlyToDateTimeTest()
        {
            DateTime now = DateTime.Now;
            DateOnly date = new DateOnly(now.Year, now.Month, now.Day);
            DateTime result = DateTimeUtilities.ConvertDateOnlyToDateTime(date, DateTimeKind.Local);
            
            Assert.IsTrue((result.Year == now.Year) && (result.Month == now.Month) && (result.Day == now.Day));
        }

        [TestMethod()]
        public void ConvertTimeOnlyToDateTimeTest()
        {
            DateTime now = DateTime.Now;
            TimeOnly time = new TimeOnly(
                    now.Hour, 
                    now.Minute, 
                    now.Second,
                    now.Millisecond
#if NET7_0_OR_GREATER
                    , now.Microsecond
#endif
                );

            DateTime result = DateTimeUtilities.ConvertTimeOnlyToDateTime(time, DateTimeKind.Local);

            Assert.IsTrue(
                    (result.Hour == now.Hour) && (result.Minute == now.Minute) && (result.Second == now.Second) && (result.Millisecond == now.Millisecond)
#if NET7_0_OR_GREATER
                    && (result.Microsecond == now.Microsecond)
#endif
                );
        }

        [TestMethod()]
        public void ConvertDateTimeOffsetToDateOnlyTest()
        {
            DateTime now = DateTime.Now;
            DateTimeOffset dto = new DateTimeOffset(now);

            DateOnly date = DateTimeUtilities.ConvertDateTimeOffsetToDateOnly(dto);
            Assert.IsTrue((date.Year == now.Year) && (date.Month == now.Month) && (date.Day == now.Day));
        }

        [TestMethod()]
        public void ConvertDateTimeToDateOnlyTest()
        {
            DateTime now = DateTime.Now;
            DateOnly date = DateTimeUtilities.ConvertDateTimeToDateOnly(now);
            Assert.IsTrue((date.Year == now.Year) && (date.Month == now.Month) && (date.Day == now.Day));
        }

        [TestMethod()]
        public void ConvertTimeOnlyToDateOnlyTest()
        {
            TimeOnly time = new TimeOnly();
            DateTimeUtilities.ConvertTimeOnlyToDateOnly(time);
            
            Assert.IsTrue(1 == 1);
        }

        [TestMethod()]
        public void ConvertDateTimeOffsetToTimeOnlyTest()
        {
            DateTime now = DateTime.Now;
            DateTimeOffset dto = new DateTimeOffset(now);

            TimeOnly result = DateTimeUtilities.ConvertDateTimeOffsetToTimeOnly(dto);
            Assert.IsTrue(
                    (result.Hour == now.Hour) && (result.Minute == now.Minute) && (result.Second == now.Second) && (result.Millisecond == now.Millisecond)
#if NET7_0_OR_GREATER
                    && (result.Microsecond == now.Microsecond)
#endif
                );
        }

        [TestMethod()]
        public void ConvertDateTimeToTimeOnlyTest()
        {
            DateTime now = DateTime.Now;
            TimeOnly result = DateTimeUtilities.ConvertDateTimeToTimeOnly(now);
            Assert.IsTrue(
                    (result.Hour == now.Hour) && (result.Minute == now.Minute) && (result.Second == now.Second) && (result.Millisecond == now.Millisecond)
#if NET7_0_OR_GREATER
                    && (result.Microsecond == now.Microsecond)
#endif
                );
        }

        [TestMethod()]
        public void ConvertDateOnlyToTimeOnlyTest()
        {
            DateTimeUtilities.ConvertDateOnlyToTimeOnly(new DateOnly());
            Assert.IsTrue(1 == 1);
        }

        [TestMethod()]
        public void ConvertDateTimeToDateTimeOffsetTest()
        {
            DateTime dateTime = DateTime.Now;
            DateTimeOffset result = DateTimeUtilities.ConvertDateTimeToDateTimeOffset(dateTime);

            Assert.IsTrue(result.Ticks == dateTime.Ticks);
        }

        [TestMethod()]
        public void ConvertDateOnlyToDateTimeOffsetTest()
        {
            DateTime now = DateTime.Now;
            DateOnly date = new DateOnly(now.Year, now.Month, now.Day);
            DateTimeOffset result = DateTimeUtilities.ConvertDateOnlyToDateTimeOffset(date);

            Assert.IsTrue((result.Year == date.Year) && (result.Month == date.Month) && (result.Day == date.Day));
        }

        [TestMethod()]
        public void ConvertTimeOnlyToDateTimeOffsetTest()
        {
            DateTime now = DateTime.Now;
            TimeOnly time = new TimeOnly(now.Hour, now.Minute, now.Second, now.Millisecond
#if NET7_0_OR_GREATER
                    , now.Microsecond
#endif
                );

            DateTimeOffset result = DateTimeUtilities.ConvertTimeOnlyToDateTimeOffset(time);
            Assert.IsTrue(result.Ticks == time.Ticks);
        }

        [TestMethod()]
        public void TryConvertTest()
        {
            DateTime now = DateTime.Now;
            Dictionary<Type, object> typesWithValues = new Dictionary<Type, object>()
            {
                { typeof(DateTime), now },
                { typeof(DateOnly), DateOnly.FromDateTime(now) },
                { typeof(TimeOnly), TimeOnly.FromDateTime(now) },
                { typeof(DateTimeOffset), new DateTimeOffset(now) }
            };
            List<Type> targetTypes = new List<Type>()
            {
                typeof(DateTime),
                typeof(DateOnly),
                typeof(TimeOnly),
                typeof(DateTimeOffset)
            };

            foreach(Type type in typesWithValues.Keys)
            {
                foreach(Type targetType in targetTypes)
                {
                    if (type == targetType)
                    {
                        continue;
                    }

                    Assert.IsTrue(DateTimeUtilities.TryConvert(typesWithValues[type], targetType, out object? _));
                }
            }
        }
    }
}