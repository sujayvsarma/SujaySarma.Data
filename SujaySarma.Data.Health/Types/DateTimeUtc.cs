using System;
using System.Collections.Immutable;

namespace SujaySarma.Data.Health.Types
{
    /// <summary>
    /// Date/time set into this type of object will always be in UTC.
    /// </summary>
    public class DateTimeUtc
    {

        /// <summary>
        /// Year
        /// </summary>
        public int Year => _value.Year;

        /// <summary>
        /// Month
        /// </summary>
        public int Month => _value.Month;

        /// <summary>
        /// Day
        /// </summary>
        public int Day => _value.Day;

        /// <summary>
        /// Hour
        /// </summary>
        public int Hour => _value.Hour;

        /// <summary>
        /// Minute
        /// </summary>
        public int Minute => _value.Minute;

        /// <summary>
        /// Second
        /// </summary>
        public int Second => _value.Second;

        /// <summary>
        /// Milliseconds
        /// </summary>
        public int Millisecond => _value.Millisecond;

        /// <summary>
        /// Day of week
        /// </summary>
        public DayOfWeek DayOfWeek => _value.DayOfWeek;

        /// <summary>
        /// Initialise
        /// </summary>
        public DateTimeUtc()
        {
            _value = DateTime.UtcNow;
        }


        /// <summary>
        /// Add seconds
        /// </summary>
        /// <param name="seconds">Number of seconds to add</param>
        /// <returns>NEW instance with the increment applied</returns>
        public DateTimeUtc AddSeconds(double seconds)
        {
            DateTime v = _value.AddSeconds(seconds);
            return v;
        }

        /// <summary>
        /// Add minutes
        /// </summary>
        /// <param name="minutes">Number of minutes to add</param>
        /// <returns>NEW instance with the increment applied</returns>
        public DateTimeUtc AddMinutes(double minutes)
        {
            DateTime v = _value.AddMinutes(minutes);
            return v;
        }

        /// <summary>
        /// Add hours
        /// </summary>
        /// <param name="hours">Number of hours to add</param>
        /// <returns>NEW instance with the increment applied</returns>
        public DateTimeUtc AddHours(double hours)
        {
            DateTime v = _value.AddHours(hours);
            return v;
        }

        /// <summary>
        /// Add days
        /// </summary>
        /// <param name="days">Number of days to add</param>
        /// <returns>NEW instance with the increment applied</returns>
        public DateTimeUtc AddDays(double days)
        {
            DateTime v = _value.AddDays(days);
            return v;
        }

        /// <summary>
        /// Add months
        /// </summary>
        /// <param name="months">Number of months to add</param>
        /// <returns>NEW instance with the increment applied</returns>
        public DateTimeUtc AddMonths(int months)
        {
            DateTime v = _value.AddMonths(months);
            return v;
        }

        /// <summary>
        /// Add years
        /// </summary>
        /// <param name="years">Number of years to add</param>
        /// <returns>NEW instance with the increment applied</returns>
        public DateTimeUtc AddYears(int years)
        {
            DateTime v = _value.AddYears(years);
            return v;
        }

        /// <summary>
        /// Create a standardised string representation of the datetime value.
        /// </summary>
        /// <returns>String representation of the date/time</returns>
        public override string ToString()
            => $"{_value:yyyy-MM-ddTHH:mm:ss.msZ}";

        /// <summary>
        /// Compares if the value of this instance is equal to the value of the provided instance
        /// </summary>
        /// <param name="obj">Instance to compare against</param>
        /// <returns>True if the values of both instances are the same</returns>
        public override bool Equals(object? obj)
            => ((obj != null) && ((obj is DateTimeUtc d) && (d._value == this._value)) || ((obj is DateTime dt) && (dt == this._value)));

        /// <summary>
        /// Get the object's hashcode
        /// </summary>
        /// <returns>Hashcode</returns>
        public override int GetHashCode()
            => _value.GetHashCode();

        private DateTime _value;

        /// <summary>
        /// Assign a DateTime to a DateTimeUtc
        /// </summary>
        /// <param name="value">DateTime value to assign</param>
        public static implicit operator DateTimeUtc(DateTime value)
        {
            DateTimeUtc utc = new DateTimeUtc();

            switch (value.Kind)
            {
                case DateTimeKind.Utc:
                    utc._value = value;
                    break;

                case DateTimeKind.Local:
                case DateTimeKind.Unspecified:
                    utc._value = TimeZoneInfo.ConvertTimeToUtc(value);
                    break;
            }

            return utc;
        }

        /// <summary>
        /// Assign a DateTimeUtc to a DateTime. Will be assigned as a UTC date/time.
        /// </summary>
        /// <param name="value">DateTimeUtc value to assign</param>
        public static implicit operator DateTime(DateTimeUtc value)
            => new DateTime(
                value._value.Year, value._value.Month, value._value.Day,
                    value._value.Hour, value._value.Minute, value._value.Second,
                        DateTimeKind.Utc);

        /// <summary>
        /// Convert the given DateTimeUtc into a DateOnly structure
        /// </summary>
        /// <param name="value">DateTimeUtc value to assign</param>
        public static implicit operator DateOnly(DateTimeUtc value)
            => new DateOnly(value._value.Year, value._value.Month, value._value.Day);

        /// <summary>
        /// Convert the DateOnly structure to a DateTimeUtc
        /// </summary>
        /// <param name="value">DateOnly value to convert</param>
        public static implicit operator DateTimeUtc(DateOnly value)
        {
            DateTimeUtc v = value.ToDateTime(new TimeOnly(0, 0, 0, 0), DateTimeKind.Utc);
            return v;
        }

        /// <summary>
        /// Convert the given DateTimeUtc into a TimeOnly structure
        /// </summary>
        /// <param name="value">DateTimeUtc value to assign</param>
        public static implicit operator TimeOnly(DateTimeUtc value)
            => new TimeOnly(value._value.Hour, value._value.Minute, value._value.Second, value._value.Millisecond);

        /// <summary>
        /// Convert the given TimeOnly structure to a DateTimeUtc
        /// </summary>
        /// <param name="value">TimeOnly to convert</param>
        public static implicit operator DateTimeUtc(TimeOnly value)
        {
            DateTime n = DateTime.UtcNow;
            DateTimeUtc v = new DateTime(n.Year, n.Month, n.Day, value.Hour, value.Minute, value.Second, value.Millisecond, DateTimeKind.Utc);
            return v;
        }

        /// <summary>
        /// Compare if the left-side is smaller than the right-side
        /// </summary>
        /// <param name="left">Left-side</param>
        /// <param name="right">Right-side</param>
        /// <returns>True if left-side is smaller than the right-side</returns>
        public static bool operator <(DateTimeUtc left, DateTimeUtc right)
            => (left._value < right._value);

        /// <summary>
        /// Compare if the left-side is larger than the right-side
        /// </summary>
        /// <param name="left">Left-side</param>
        /// <param name="right">Right-side</param>
        /// <returns>True if left-side is smaller than the right-side</returns>
        public static bool operator >(DateTimeUtc left, DateTimeUtc right)
            => (right < left);

        /// <summary>
        /// Compare if the left-side is smaller than or equal to the right-side
        /// </summary>
        /// <param name="left">Left-side</param>
        /// <param name="right">Right-side</param>
        /// <returns>True if left-side is smaller than or equal to the right-side</returns>
        public static bool operator <=(DateTimeUtc left, DateTimeUtc right)
            => (left._value <= right._value);

        /// <summary>
        /// Compare if the left-side is larger than or equal to the right-side
        /// </summary>
        /// <param name="left">Left-side</param>
        /// <param name="right">Right-side</param>
        /// <returns>True if left-side is smaller than or equal to the right-side</returns>
        public static bool operator >=(DateTimeUtc left, DateTimeUtc right)
            => (right <= left);

        /// <summary>
        /// Compare if both values are equal
        /// </summary>
        /// <param name="left">Left-side</param>
        /// <param name="right">Right-side</param>
        /// <returns>True if both values are equal</returns>
        public static bool operator ==(DateTimeUtc left, DateTimeUtc right)
            => (left._value == right._value);

        /// <summary>
        /// Compare if both values are unequal
        /// </summary>
        /// <param name="left">Left-side</param>
        /// <param name="right">Right-side</param>
        /// <returns>True both values are unequal</returns>
        public static bool operator !=(DateTimeUtc left, DateTimeUtc right)
            => !(right == left);
    }
}
