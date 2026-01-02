#nullable enable
using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SujaySarma.Data.Core.ReflectionUtilities;

namespace SujaySarma.Data.Core.Tests.ReflectionUtilities
{
    [TestClass]
    [TestCategory("Functional")]   
    public class Extensions_ConvertBetweenDatesAndTimes
    {
        [TestMethod(DisplayName = "ConvertBetweenDatesAndTimes: DateTimeOffset to DateTime")]
        public void FromDateTimeOffset_ToDateTime()
        {
            DateTime dt = _dateTimeOffset.ToDateTime();
            Assert.AreEqual(dt, _dateTime, $"Expected: '{_dateTime:yyyy-MM-ddHH:mm:ss} UTC', Actual: '{dt:yyyy-MM-ddHH:mm:ss} UTC'");
        }

        [TestMethod(DisplayName = "ConvertBetweenDatesAndTimes: DateOnly to DateTime")]
        public void FromDateOnly_ToDateTime()
        {
            DateTime dt = _dateOnly.ToDateTime();
            Assert.AreEqual(dt, _dateTime, $"Expected: '{_dateTime:yyyy-MM-ddHH:mm:ss} UTC', Actual: '{dt:yyyy-MM-ddHH:mm:ss} UTC'");
        }

        [TestMethod(DisplayName = "ConvertBetweenDatesAndTimes: TimeOnly to DateTime")]
        public void FromTimeOnly_ToDateTime()
        {
            DateTime dt = _timeOnly.ToDateTime();
            Assert.AreEqual(dt, new DateTime(1, 1, 1, 0, 0, 0), $"Expected: '{_dateTime:yyyy-MM-ddHH:mm:ss} UTC', Actual: '{dt:yyyy-MM-ddHH:mm:ss} UTC'");
        }

        [TestMethod(DisplayName = "ConvertBetweenDatesAndTimes: DateTimeOffset to DateOnly")]
        public void FromDateTimeOffset_ToDateOnly()
        {
            DateOnly d = _dateTimeOffset.ToDateOnly();
            Assert.AreEqual(_dateOnly, d, $"Expected: '{_dateOnly:yyyy-MM-dd} UTC', Actual: '{d:yyyy-MM-dd} UTC'");
        }

        [TestMethod(DisplayName = "ConvertBetweenDatesAndTimes: DateTime to DateOnly")]
        public void FromDateTime_ToDateOnly()
        {
            DateOnly d = _dateTime.ToDateOnly();
            Assert.AreEqual(_dateOnly, d, $"Expected: '{_dateOnly:yyyy-MM-dd} UTC', Actual: '{d:yyyy-MM-dd} UTC'");
        }

        [TestMethod(DisplayName = "ConvertBetweenDatesAndTimes: TimeOnly to DateOnly")]
        public void FromTimeOnly_ToDateOnly()
        {
            DateOnly d = _timeOnly.ToDateOnly();
            Assert.AreEqual(new DateOnly(1, 1, 1), d, $"Expected: '{_dateOnly:yyyy-MM-dd} UTC', Actual: '{d:yyyy-MM-dd} UTC'");
        }

        [TestMethod(DisplayName = "ConvertBetweenDatesAndTimes: DateTimeOffset to TimeOnly")]
        public void FromDateTimeOffset_ToTimeOnly()
        {
            TimeOnly t = _dateTimeOffset.ToTimeOnly();
            Assert.AreEqual(_timeOnly, t, $"Expected: '{_timeOnly:HH:mm:ss} UTC', Actual: '{t:HH:mm:ss} UTC'");
        }

        [TestMethod(DisplayName = "ConvertBetweenDatesAndTimes: DateTime to TimeOnly")]
        public void FromDateTime_ToTimeOnly()
        {
            TimeOnly t = _dateTime.ToTimeOnly();
            Assert.AreEqual(_timeOnly, t, $"Expected: '{_timeOnly:HH:mm:ss} UTC', Actual: '{t:HH:mm:ss} UTC'");
        }

        [TestMethod(DisplayName = "ConvertBetweenDatesAndTimes: DateOnly to TimeOnly")]
        public void FromDateOnly_ToTimeOnly()
        {
            TimeOnly t = _dateOnly.ToTimeOnly();
            Assert.AreEqual(new TimeOnly(0, 0, 0), t, $"Expected: '{_timeOnly:HH:mm:ss} UTC', Actual: '{t:HH:mm:ss} UTC'");
        }

        [TestMethod(DisplayName = "ConvertBetweenDatesAndTimes: DateTime to DateTimeOffset")]
        public void FromDateTime_ToDateTimeOffset()
        {
            DateTimeOffset dto = _dateTime.ToDateTimeOffset();
            Assert.AreEqual(_dateTimeOffsetUtc, dto, $"Expected: '{_dateTimeOffsetUtc:yyyy-MM-ddHH:mm:ss} UTC', Actual: '{dto:yyyy-MM-ddHH:mm:ss} UTC'");
        }

        [TestMethod(DisplayName = "ConvertBetweenDatesAndTimes: DateOnly to DateTimeOffset")]
        public void FromDateOnly_ToDateTimeOffset()
        {
            DateTimeOffset dto = _dateOnly.ToDateTimeOffset();
            Assert.AreEqual(_dateTimeOffset, dto, $"Expected: '{_dateTimeOffset:yyyy-MM-ddHH:mm:ss} UTC', Actual: '{dto:yyyy-MM-ddHH:mm:ss} UTC'");
        }

        [TestMethod(DisplayName = "ConvertBetweenDatesAndTimes: TimeOnly to DateTimeOffset")]
        public void FromTimeOnly_ToDateTimeOffset()
        {
            DateTimeOffset dto = _timeOnly.ToDateTimeOffset();
            Assert.AreEqual(new DateTimeOffset(1, 1, 1, 0, 0, 0, TimeSpan.FromMinutes(0)), dto, $"Expected: '{_dateTimeOffset:yyyy-MM-ddHH:mm:ss} UTC', Actual: '{dto:yyyy-MM-ddHH:mm:ss} UTC'");
        }

        [TestMethod(DisplayName = "ConvertBetweenDatesAndTimes: Common Entry Point: TryConvert(...)")]
        public void TryConvert()
        {
            Type[] types = new Type[] { typeof(DateTime), typeof(DateOnly), typeof(TimeOnly), typeof(DateTimeOffset) };
            Random random = new Random(int.MaxValue);
            int maxTypes = (types.Length - 1), from = random.Next(0, maxTypes);
            int to = 0, tries = 0;
            while ((to == from) && (tries < 3))
            {
                ++tries;
                to = random.Next(0, maxTypes);
            }

            if ((to == from) && (tries >= 3))
            {
                Assert.Fail("Could not acquire different from/to pairs within 3 tries.");
                return;
            }

            var value = new object();
            switch (from)
            {
                case 1:
                    value = _dateOnly;
                    break;

                case 2:
                    value = _timeOnly;
                    break;

                case 3:
                    value = _dateTimeOffset;
                    break;

                default:
                    value = _dateTime;
                    break;
            }

            if (!ConvertBetweenDatesAndTimes.TryConvert(value, types[to], out object? result))
            {
                Assert.Fail($"Type conversion failed between '{value.GetType()}' and '{types[to].Name}'.");
            }

            switch (to)
            {
                case 1:
                    Assert.AreEqual(_dateOnly, result, $"From type: '{value.GetType()}' to type 'DateOnly'.");
                    break;

                case 2:
                    Assert.AreEqual(_timeOnly, result, $"From type: '{value.GetType()}' to type 'TimeOnly'.");
                    break;

                case 3:
                    if (value.GetType() == typeof(TimeOnly))
                    {
                        Assert.AreEqual(new DateTimeOffset(1, 1, 1, 0, 0, 0, TimeSpan.FromMinutes(0)), result, $"From type: '{value.GetType()}' to type 'DateTimeOffset'.");
                    }
                    else
                    {
                        Assert.AreEqual(_dateTimeOffset, result, $"From type: '{value.GetType()}' to type 'DateTimeOffset'.");
                    }
                    break;

                default:
                    if (value.GetType() == typeof(TimeOnly))
                    {
                        Assert.AreEqual(new DateTime(1, 1, 1, 0, 0, 0), result, $"From type: '{value.GetType()}' to type 'DateTime'.");
                    }
                    else
                    {
                        Assert.AreEqual(_dateTime, result, $"From type: '{value.GetType()}' to type 'DateTime'.");
                    }
                    break;
            }
        }

        private static readonly DateTime _dateTime = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Local);
        private static readonly DateTimeOffset _dateTimeOffset = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.FromMinutes(0));
        private static readonly DateTimeOffset _dateTimeOffsetUtc = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.FromMinutes(330));
        private static readonly DateOnly _dateOnly = new DateOnly(2000, 1, 1);
        private static readonly TimeOnly _timeOnly = new TimeOnly(0, 0, 0);

    }
}