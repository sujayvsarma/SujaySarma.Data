using System;
using System.Diagnostics.CodeAnalysis;


namespace SujaySarma.Data.Core
{
    /// <summary>
    /// Provides functions (conversions) specific to Dates, Times and DateTimes and DateTimeOffsets.
    /// </summary>
    public static class DateTimeUtilities
    {
        /// <summary>
        /// Convert a <see cref="DateTimeOffset" /> value to a <see cref="DateTime" />.
        /// The DateTimeOffset's offset value is used to set the DateTime's Kind information to Utc or Local.
        /// </summary>
        /// <param name="dto">DateTimeOffset value</param>
        /// <returns>A new instance of a <see cref="DateTime" /> set to the correct timezone</returns>
        public static DateTime ConvertDateTimeOffsetToDateTime(DateTimeOffset dto)
            => new DateTime(dto.Ticks, ((dto.Offset.TotalMinutes == 0.0) ? DateTimeKind.Utc : DateTimeKind.Local));

        /// <summary>
        /// Convert a <see cref="DateOnly" /> to a <see cref="DateTime" /> and sets the Time component to 00:00:00.
        /// </summary>
        /// <param name="date">The DateOnly value</param>
        /// <param name="kind">Preference for the Kind of value. Default: UTC</param>
        /// <returns>A new instance of a <see cref="DateTime" /> set to the preferred timezone</returns>
        public static DateTime ConvertDateOnlyToDateTime(DateOnly date, DateTimeKind kind = DateTimeKind.Utc)
            => new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0, kind);

        /// <summary>
        /// Convert a <see cref="TimeOnly" /> to a <see cref="DateTime" /> setting the Date component to the default empty date of 01/01/0001.
        /// </summary>
        /// <param name="time">The TimeOnly value</param>
        /// <param name="kind">Preference for the Kind of value. Default: UTC</param>
        /// <returns>A new instance of a <see cref="DateTime" /> set to the preferred timezone</returns>
        /// <remarks>
        ///     This function sets the date component to 01/01/0001 which is invalid per most if not all database systems and will fail validation
        ///     if the consumer code attempts to store this value as-is!
        /// </remarks>
        public static DateTime ConvertTimeOnlyToDateTime(TimeOnly time, DateTimeKind kind = DateTimeKind.Utc)
            => new DateTime(time.Ticks, kind);

        /// <summary>
        /// Converts a <see cref="DateTimeOffset" /> to a <see cref="DateOnly" /> stripping away its Time component
        /// </summary>
        /// <param name="dto">The DateTimeOffset value</param>
        /// <returns>A new instance of <see cref="DateOnly" /></returns>
        public static DateOnly ConvertDateTimeOffsetToDateOnly(DateTimeOffset dto)
            => DateOnly.FromDateTime(dto.DateTime);

        /// <summary>
        /// Converts a <see cref="DateTime" /> to a <see cref="DateOnly" /> stripping away its Time component
        /// </summary>
        /// <param name="dateTime">The DateTime value</param>
        /// <returns>A new instance of <see cref="DateOnly" /></returns>
        public static DateOnly ConvertDateTimeToDateOnly(DateTime dateTime)
            => DateOnly.FromDateTime(dateTime);

        /// <summary>
        /// This is provided purely for completeness and is not a valid operation!
        /// </summary>
        /// <param name="_">The <see cref="TimeOnly" /> value</param>
        /// <returns>A new <see cref="DateOnly" /> value that is 01/01/0001</returns>
        public static DateOnly ConvertTimeOnlyToDateOnly(TimeOnly _)
            => new DateOnly();

        /// <summary>
        /// Converts a <see cref="DateTimeOffset" /> to a <see cref="TimeOnly" /> stripping away its Date component
        /// </summary>
        /// <param name="dto">The DateTimeOffset value</param>
        /// <returns>A new instance of <see cref="TimeOnly" /></returns>
        public static TimeOnly ConvertDateTimeOffsetToTimeOnly(DateTimeOffset dto)
            => TimeOnly.FromDateTime(dto.DateTime);

        /// <summary>
        /// Converts a <see cref="DateTime" /> to a <see cref="TimeOnly" /> stripping away its Date component
        /// </summary>
        /// <param name="dateTime">The DateTime value</param>
        /// <returns>A new instance of <see cref="TimeOnly" /></returns>
        public static TimeOnly ConvertDateTimeToTimeOnly(DateTime dateTime)
            => TimeOnly.FromDateTime(dateTime);

        /// <summary>
        /// This is provided purely for completeness and is not a valid operation!
        /// </summary>
        /// <param name="_">The <see cref="DateOnly" /> value</param>
        /// <returns>A new <see cref="TimeOnly" /> value that is 00:00:00</returns>
        public static TimeOnly ConvertDateOnlyToTimeOnly(DateOnly _)
            => new TimeOnly(0, 0, 0, 0);

        /// <summary>
        /// Convert <see cref="DateTime" /> to a <see cref="DateTimeOffset" />.
        /// </summary>
        /// <param name="dateTime">The DateTime value. Offset depends on the value of "Kind" property.</param>
        /// <returns>A new instance of a <see cref="DateTimeOffset" /></returns>
        public static DateTimeOffset ConvertDateTimeToDateTimeOffset(DateTime dateTime)
            => new DateTimeOffset(
                ((dateTime.Kind == DateTimeKind.Unspecified)
                    ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
                    : dateTime)
               );

        /// <summary>
        /// Convert <see cref="DateOnly" /> to a <see cref="DateTimeOffset" />
        /// </summary>
        /// <param name="date">The DateOnly value</param>
        /// <returns>A new instance of a <see cref="DateTimeOffset" /> that is set to Utc</returns>
        public static DateTimeOffset ConvertDateOnlyToDateTimeOffset(DateOnly date)
            => new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero);

        /// <summary>
        /// Convert <see cref="TimeOnly" /> to a <see cref="DateTimeOffset" />
        /// </summary>
        /// <param name="time">The TimeOnly value</param>
        /// <returns>A new instance of a <see cref="DateTimeOffset" /> that is set to Utc</returns>
        public static DateTimeOffset ConvertTimeOnlyToDateTimeOffset(TimeOnly time)
            => new DateTimeOffset(time.Ticks, TimeSpan.Zero);

        /// <summary>
        /// Try to convert the provided <paramref name="value"/> into the provided <paramref name="targetType"/> type. 
        /// The result is returned via the out parameter <paramref name="result"/>
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="targetType">The type to convert to</param>
        /// <param name="result">[out] The converted value. Maybe NULL (if function return is FALSE)</param>
        /// <returns>TRUE if conversion was successful (result will be non-NULL). FALSE if conversion failed (result will be NULL).</returns>
        public static bool TryConvert(object value, Type targetType, [NotNullWhen(true)] out object? result)
        {
            result = null;
            switch (value)
            {
                case DateTime dateTime1:
                    if (Type.Equals(targetType, typeof(DateOnly)))
                    {
                        result = ConvertDateTimeToDateOnly(dateTime1);
                    }
                    else if (Type.Equals(targetType, typeof(TimeOnly)))
                    {
                        result = ConvertDateTimeToTimeOnly(dateTime1);
                    }
                    else if (Type.Equals(targetType, typeof(DateTimeOffset)))
                    {
                        result = ConvertDateTimeToDateTimeOffset(dateTime1);
                    }
                    break;

                case DateOnly _:
                    DateOnly dateOnly1 = (DateOnly)value;
                    if (Type.Equals(targetType, typeof(TimeOnly)))
                    {
                        result = ConvertDateOnlyToTimeOnly(dateOnly1);
                    }
                    else if (Type.Equals(targetType, typeof(DateTime)))
                    {
                        result = ConvertDateOnlyToDateTime(dateOnly1);
                    }
                    else if (Type.Equals(targetType, typeof(DateTimeOffset)))
                    {
                        result = ConvertDateOnlyToDateTimeOffset(dateOnly1);
                    }
                    break;

                case TimeOnly _:
                    TimeOnly timeOnly1 = (TimeOnly)value;
                    if (Type.Equals(targetType, typeof(DateOnly)))
                    {
                        result = ConvertTimeOnlyToDateOnly(timeOnly1);
                    }
                    else if (Type.Equals(targetType, typeof(DateTime)))
                    {
                        result = ConvertTimeOnlyToDateTime(timeOnly1);
                    }
                    else if (Type.Equals(targetType, typeof(DateTimeOffset)))
                    {
                        result = ConvertTimeOnlyToDateTimeOffset(timeOnly1);
                    }
                    break;

                case DateTimeOffset _:
                    DateTimeOffset dto = (DateTimeOffset)value;
                    if (Type.Equals(targetType, typeof(DateOnly)))
                    {
                        result = ConvertDateTimeOffsetToDateOnly(dto);
                    }
                    else if (Type.Equals(targetType, typeof(TimeOnly)))
                    {
                        result = ConvertDateTimeOffsetToTimeOnly(dto);
                    }
                    else if (Type.Equals(targetType, typeof(DateTime)))
                    {
                        result = ConvertDateTimeOffsetToDateTime(dto);
                    }
                    break;

                case string str:
                    if (Type.Equals(targetType, typeof(DateOnly)))
                    {
                        if (DateOnly.TryParse(str, out DateOnly dateOnly2))
                        {
                            result = dateOnly2;
                        }
                    }
                    else if (Type.Equals(targetType, typeof(TimeOnly)))
                    {
                        if (TimeOnly.TryParse(str, out TimeOnly timeOnly2))
                        {
                            result = timeOnly2;
                        }
                    }
                    else if (Type.Equals(targetType, typeof(DateTime)))
                    {
                        if (DateTime.TryParse(str, out DateTime dateTime))
                        {
                            result = dateTime;
                        }
                    }
                    else if (Type.Equals(targetType, typeof(DateTimeOffset)) && DateTimeOffset.TryParse(str, out DateTimeOffset dateTimeOffset))
                    {
                        result = dateTimeOffset;
                    }
                    break;

                case long num:
                    if (Type.Equals(targetType, typeof(DateOnly)))
                    {
                        result = ConvertDateTimeToDateOnly(new DateTime(num));
                    }
                    else if (Type.Equals(targetType, typeof(TimeOnly)))
                    {
                        result = ConvertDateTimeToTimeOnly(new DateTime(num));
                    }
                    else if (Type.Equals(targetType, typeof(DateTime)))
                    {
                        result = new DateTime(num);
                    }
                    else if (Type.Equals(targetType, typeof(DateTimeOffset)))
                    {
                        result = new DateTimeOffset(num, TimeSpan.Zero);
                    }
                    break;
            }

            return ((result == null) ? false : true);
        }
    }
}
