using System;
using System.Diagnostics.CodeAnalysis;

namespace SujaySarma.Data.Core
{
    /// <summary>
    /// Provides functions (conversions) specific to Dates, Times and DateTimes and DateTimeOffsets.
    /// </summary>
    public static class DateTimeUtilities
    {
        #region *** To DateTime ***

        /// <summary>
        /// Convert a <see cref="DateTimeOffset"/> value to a <see cref="DateTime"/>. 
        /// The DateTimeOffset's offset value is used to set the DateTime's Kind information to Utc or Local.
        /// </summary>
        /// <param name="dto">DateTimeOffset value</param>
        /// <returns>A new instance of a <see cref="DateTime"/> set to the correct timezone</returns>
        public static DateTime ConvertDateTimeOffsetToDateTime(DateTimeOffset dto)
        {
            return new DateTime(dto.Ticks, ((dto.Offset.TotalMinutes == 0) ? DateTimeKind.Utc : DateTimeKind.Local));
        }

        /// <summary>
        /// Convert a <see cref="DateOnly"/> to a <see cref="DateTime"/> and sets the Time component to 00:00:00.
        /// </summary>
        /// <param name="date">The DateOnly value</param>
        /// <param name="kind">Preference for the Kind of value. Default: UTC</param>
        /// <returns>A new instance of a <see cref="DateTime"/> set to the preferred timezone</returns>
        public static DateTime ConvertDateOnlyToDateTime(DateOnly date, DateTimeKind kind = DateTimeKind.Utc)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0, kind);
        }

        /// <summary>
        /// Convert a <see cref="TimeOnly"/> to a <see cref="DateTime"/> setting the Date component to the default empty date of 01/01/0001.
        /// </summary>
        /// <param name="time">The DateOnly value</param>
        /// <param name="kind">Preference for the Kind of value. Default: UTC</param>
        /// <returns>A new instance of a <see cref="DateTime"/> set to the preferred timezone</returns>
        /// <remarks>
        ///     This function sets the date component to 01/01/0001 which is invalid per most if not all database systems and will fail validation 
        ///     if the consumer code attempts to store this value as-is!
        /// </remarks>
        public static DateTime ConvertTimeOnlyToDateTime(TimeOnly time, DateTimeKind kind = DateTimeKind.Utc)
        {
            return new DateTime(time.Ticks, kind);
        }

        #endregion

        #region *** To DateOnly ***

        /// <summary>
        /// Converts a <see cref="DateTimeOffset"/> to a <see cref="DateOnly"/> stripping away its Time component
        /// </summary>
        /// <param name="dto">The DateTimeOffset value</param>
        /// <returns>A new instance of <see cref="DateOnly"/></returns>
        public static DateOnly ConvertDateTimeOffsetToDateOnly(DateTimeOffset dto)
        {
            return DateOnly.FromDateTime(dto.DateTime);
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> to a <see cref="DateOnly"/> stripping away its Time component
        /// </summary>
        /// <param name="dateTime">The DateTime value</param>
        /// <returns>A new instance of <see cref="DateOnly"/></returns>
        public static DateOnly ConvertDateTimeToDateOnly(DateTime dateTime)
        {
            return DateOnly.FromDateTime(dateTime);
        }

        /// <summary>
        /// This is provided purely for completeness and is not a valid operation!
        /// </summary>
        /// <param name="_">The <see cref="TimeOnly"/> value</param>
        /// <returns>A new <see cref="DateOnly"/> value that is 01/01/0001</returns>
        public static DateOnly ConvertTimeOnlyToDateOnly(TimeOnly _)
        {
            return new DateOnly();
        }

        #endregion

        #region *** To TimeOnly ***

        /// <summary>
        /// Converts a <see cref="DateTimeOffset"/> to a <see cref="TimeOnly"/> stripping away its Date component
        /// </summary>
        /// <param name="dto">The DateTimeOffset value</param>
        /// <returns>A new instance of <see cref="TimeOnly"/></returns>
        public static TimeOnly ConvertDateTimeOffsetToTimeOnly(DateTimeOffset dto)
        {
            return TimeOnly.FromDateTime(dto.DateTime);
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> to a <see cref="TimeOnly"/> stripping away its Date component
        /// </summary>
        /// <param name="dateTime">The DateTime value</param>
        /// <returns>A new instance of <see cref="DateOnly"/></returns>
        public static TimeOnly ConvertDateTimeToTimeOnly(DateTime dateTime)
        {
            return TimeOnly.FromDateTime(dateTime);
        }

        /// <summary>
        /// This is provided purely for completeness and is not a valid operation!
        /// </summary>
        /// <param name="_">The <see cref="DateOnly"/> value</param>
        /// <returns>A new <see cref="TimeOnly"/> value that is 00:00:00</returns>
        public static TimeOnly ConvertDateOnlyToTimeOnly(DateOnly _)
        {
            return new TimeOnly(0, 0, 0, 0);
        }

#endregion

        #region *** To DateTimeOffset ***

        /// <summary>
        /// Convert <see cref="DateTime"/> to a <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <param name="dateTime">The DateTime value. Offset depends on the value of "Kind" property.</param>
        /// <returns>A new instance of a <see cref="DateTimeOffset"/></returns>
        public static DateTimeOffset ConvertDateTimeToDateTimeOffset(DateTime dateTime)
        {
            // Treat unspecified kinds as Utc
            if (dateTime.Kind == DateTimeKind.Unspecified)
            {
                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }            

            return new DateTimeOffset(dateTime);
        }

        /// <summary>
        /// Convert <see cref="DateOnly"/> to a <see cref="DateTimeOffset"/>
        /// </summary>
        /// <param name="date">The DateOnly value</param>
        /// <returns>A new instance of a <see cref="DateTimeOffset"/> that is set to Utc</returns>
        public static DateTimeOffset ConvertDateOnlyToDateTimeOffset(DateOnly date)
        {
            return new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero);
        }

        /// <summary>
        /// Convert <see cref="TimeOnly"/> to a <see cref="DateTimeOffset"/>
        /// </summary>
        /// <param name="time">The TimeOnly value</param>
        /// <returns>A new instance of a <see cref="DateTimeOffset"/> that is set to Utc</returns>
        public static DateTimeOffset ConvertTimeOnlyToDateTimeOffset(TimeOnly time)
        {
            return new DateTimeOffset(time.Ticks, TimeSpan.Zero);
        }


        #endregion

        /// <summary>
        /// Attempt to convert the given value to the provided type
        /// </summary>
        /// <param name="value">The value. May be one of: DateOnly, TimeOnly, DateTime or DateTimeOffset, a string or a long (Ticks)</param>
        /// <param name="targetType">Type of result: DateOnly, TimeOnly, DateTime or DateTimeOffset</param>
        /// <param name="result">[Out] Result of the conversion. Will be Null if the return is FALSE</param>
        /// <returns>True if conversion was successful</returns>
        public static bool TryConvert(object value, Type targetType, [NotNullWhen(true)] out object? result)
        {
            result = null;

            if (value is DateTime dt)
            {
                if (targetType ==  typeof(DateOnly))
                {
                    result = ConvertDateTimeToDateOnly(dt);
                }
                else if (targetType == typeof(TimeOnly))
                {
                    result = ConvertDateTimeToTimeOnly(dt);
                }
                else if (targetType == typeof(DateTimeOffset))
                {
                    result = ConvertDateTimeToDateTimeOffset(dt);
                }
            }
            else if (value is DateOnly date)
            {
                if (targetType == typeof(TimeOnly))
                {
                    result = ConvertDateOnlyToTimeOnly(date);
                }
                else if (targetType == typeof(DateTime))
                {
                    result = ConvertDateOnlyToDateTime(date);
                }
                else if (targetType == typeof(DateTimeOffset))
                {
                    result = ConvertDateOnlyToDateTimeOffset(date);
                }
            }
            else if (value is TimeOnly time)
            {
                if (targetType == typeof(DateOnly))
                {
                    result = ConvertTimeOnlyToDateOnly(time);
                }
                else if (targetType == typeof(DateTime))
                {
                    result = ConvertTimeOnlyToDateTime(time);
                }
                else if (targetType == typeof(DateTimeOffset))
                {
                    result = ConvertTimeOnlyToDateTimeOffset(time);
                }
            }
            else if (value is DateTimeOffset dto)
            {
                if (targetType == typeof(DateOnly))
                {
                    result = ConvertDateTimeOffsetToDateOnly(dto);
                }
                else if (targetType == typeof(TimeOnly))
                {
                    result = ConvertDateTimeOffsetToTimeOnly(dto);
                }
                else if (targetType == typeof(DateTime))
                {
                    result = ConvertDateTimeOffsetToDateTime(dto);
                }
            }
            else if (value is string str)
            {
                if (targetType == typeof(DateOnly))
                {
                    if (DateOnly.TryParse(str, out DateOnly date2))
                    {
                        result = date2;
                    }
                }
                else if (targetType == typeof(TimeOnly))
                {
                    if (TimeOnly.TryParse(str, out TimeOnly time2))
                    {
                        result = time2;
                    }
                }
                else if (targetType == typeof(DateTime))
                {
                    if (DateTime.TryParse(str, out DateTime dateTime2))
                    {
                        result = dateTime2;
                    }
                }
                else if (targetType == typeof(DateTimeOffset))
                {
                    if (DateTimeOffset.TryParse(str, out DateTimeOffset dto2))
                    {
                        result = dto2;
                    }
                }
            }
            else if (value is long ticks)
            {
                if (targetType == typeof(DateOnly))
                {
                    result = ConvertDateTimeToDateOnly(new DateTime(ticks));
                }
                else if (targetType == typeof(TimeOnly))
                {
                    result = ConvertDateTimeToTimeOnly(new DateTime(ticks));
                }
                else if (targetType == typeof(DateTime))
                {
                    result = new DateTime(ticks);
                }
                else if (targetType == typeof(DateTimeOffset))
                {
                    result = new DateTimeOffset(ticks, TimeSpan.Zero);
                }
            }

            return ((result == null) ? false : true);
        }
    }
}
