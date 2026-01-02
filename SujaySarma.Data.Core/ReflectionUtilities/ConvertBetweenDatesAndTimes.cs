using System;
using System.Diagnostics.CodeAnalysis;

namespace SujaySarma.Data.Core.ReflectionUtilities;

/// <summary>
/// 
/// </summary>
public static class ConvertBetweenDatesAndTimes
{
    #region To DateTime

    /// <summary>
    /// Convert a <see cref="DateTimeOffset" /> value to a <see cref="DateTime" />.
    /// The DateTimeOffset's offset value is used to set the DateTime's Kind information to Utc or Local.
    /// </summary>
    /// <param name="dto">DateTimeOffset value.</param>
    /// <returns>A new instance of a <see cref="DateTime" /> set to the correct timezone.</returns>
    public static DateTime ToDateTime(this DateTimeOffset dto)
        => new DateTime(dto.Ticks, ((dto.Offset.TotalMinutes == 0.0) ? DateTimeKind.Utc : DateTimeKind.Local));

    /// <summary>
    /// Convert a <see cref="DateOnly" /> to a <see cref="DateTime" /> and sets the Time component to 00:00:00.
    /// </summary>
    /// <param name="date">The DateOnly value.</param>
    /// <param name="kind">Preference for the Kind of value. Default: UTC.</param>
    /// <returns>A new instance of a <see cref="DateTime" /> set to the preferred timezone.</returns>
    public static DateTime ToDateTime(this DateOnly date, DateTimeKind kind = DateTimeKind.Utc)
        => new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0, kind);

    /// <summary>
    /// Convert a <see cref="TimeOnly" /> to a <see cref="DateTime" /> setting the Date component to the default empty date of 01/01/0001.
    /// </summary>
    /// <param name="time">The TimeOnly value.</param>
    /// <param name="kind">Preference for the Kind of value. Default: UTC.</param>
    /// <returns>A new instance of a <see cref="DateTime" /> set to the preferred timezone.</returns>
    /// <remarks>
    ///     This function sets the date component to 01/01/0001 which is invalid per most if not all database systems and will fail validation
    ///     if the consumer code attempts to store this value as-is!
    /// </remarks>
    public static DateTime ToDateTime(this TimeOnly time, DateTimeKind kind = DateTimeKind.Utc)
        => new DateTime(time.Ticks, kind);

    #endregion

    #region To DateOnly

    /// <summary>
    /// Converts a <see cref="DateTimeOffset" /> to a <see cref="DateOnly" /> stripping away its Time component.
    /// </summary>
    /// <param name="dto">The DateTimeOffset value.</param>
    /// <returns>A new instance of <see cref="DateOnly" />.</returns>
    public static DateOnly ToDateOnly(this DateTimeOffset dto)
        => DateOnly.FromDateTime(dto.DateTime);

    /// <summary>
    /// Converts a <see cref="DateTime" /> to a <see cref="DateOnly" /> stripping away its Time component.
    /// </summary>
    /// <param name="dateTime">The DateTime value.</param>
    /// <returns>A new instance of <see cref="DateOnly" />.</returns>
    public static DateOnly ToDateOnly(this DateTime dateTime)
        => DateOnly.FromDateTime(dateTime);

    /// <summary>
    /// This is provided purely for completeness and is not a valid operation!
    /// </summary>
    /// <param name="_">The <see cref="TimeOnly" /> value.</param>
    /// <returns>A new <see cref="DateOnly" /> value that is 01/01/0001.</returns>
    public static DateOnly ToDateOnly(this TimeOnly _)
        => new DateOnly();

    #endregion

    #region To TimeOnly

    /// <summary>
    /// Converts a <see cref="DateTimeOffset" /> to a <see cref="TimeOnly" /> stripping away its Date component.
    /// </summary>
    /// <param name="dto">The DateTimeOffset value.</param>
    /// <returns>A new instance of <see cref="TimeOnly" />.</returns>
    public static TimeOnly ToTimeOnly(this DateTimeOffset dto)
        => TimeOnly.FromDateTime(dto.DateTime);

    /// <summary>
    /// Converts a <see cref="DateTime" /> to a <see cref="TimeOnly" /> stripping away its Date component.
    /// </summary>
    /// <param name="dateTime">The DateTime value.</param>
    /// <returns>A new instance of <see cref="TimeOnly" />.</returns>
    public static TimeOnly ToTimeOnly(this DateTime dateTime)
        => TimeOnly.FromDateTime(dateTime);

    /// <summary>
    /// This is provided purely for completeness and is not a valid operation!
    /// </summary>
    /// <param name="_">The <see cref="DateOnly" /> value.</param>
    /// <returns>A new <see cref="TimeOnly" /> value that is 00:00:00.</returns>
    public static TimeOnly ToTimeOnly(this DateOnly _)
        => new TimeOnly(0, 0, 0, 0);

    #endregion

    #region To DateTimeOffset

    /// <summary>
    /// Convert <see cref="DateTime" /> to a <see cref="DateTimeOffset" />.
    /// </summary>
    /// <param name="dateTime">The DateTime value. Offset depends on the value of "Kind" property.</param>
    /// <returns>A new instance of a <see cref="DateTimeOffset" />.</returns>
    public static DateTimeOffset ToDateTimeOffset(this DateTime dateTime)
        => new DateTimeOffset(
            ((dateTime.Kind == DateTimeKind.Unspecified)
                ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
                : dateTime)
           );

    /// <summary>
    /// Convert <see cref="DateOnly" /> to a <see cref="DateTimeOffset" />.
    /// </summary>
    /// <param name="date">The DateOnly value.</param>
    /// <returns>A new instance of a <see cref="DateTimeOffset" /> that is set to Utc.</returns>
    public static DateTimeOffset ToDateTimeOffset(this DateOnly date)
        => new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Convert <see cref="TimeOnly" /> to a <see cref="DateTimeOffset" />.
    /// </summary>
    /// <param name="time">The TimeOnly value.</param>
    /// <returns>A new instance of a <see cref="DateTimeOffset" /> that is set to Utc.</returns>
    public static DateTimeOffset ToDateTimeOffset(this TimeOnly time)
        => new DateTimeOffset(time.Ticks, TimeSpan.Zero);

    #endregion

    /// <summary>
    /// Try to convert the provided <paramref name="value"/> into the provided <paramref name="targetType"/> type. 
    /// The result is returned via the out parameter <paramref name="result"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="result">[out] The converted value. Maybe NULL (if function return is FALSE).</param>
    /// <returns>TRUE if conversion was successful (result will be non-NULL). FALSE if conversion failed (result will be NULL).</returns>
    public static bool TryConvert(object value, Type targetType, [NotNullWhen(true)] out object? result)
    {
        bool IsTarget<T>() => Type.Equals(targetType, typeof(T));

        result = (value) switch
        {
            // --- From DateTime -------------------------------------------
            DateTime dt when IsTarget<DateOnly>() => dt.ToDateOnly(),
            DateTime dt when IsTarget<TimeOnly>() => dt.ToTimeOnly(),
            DateTime dt when IsTarget<DateTimeOffset>() => dt.ToDateTimeOffset(),

            // --- From DateOnly -------------------------------------------
            DateOnly d when IsTarget<TimeOnly>() => d.ToTimeOnly(),
            DateOnly d when IsTarget<DateTimeOffset>() => d.ToDateTimeOffset(),
            DateOnly d when IsTarget<DateTime>() => d.ToDateTime(),

            // --- From TimeOnly -------------------------------------------
            TimeOnly t when IsTarget<DateOnly>() => t.ToDateOnly(),
            TimeOnly t when IsTarget<DateTimeOffset>() => t.ToDateTimeOffset(),
            TimeOnly t when IsTarget<DateTime>() => t.ToDateTime(),

            // --- From DateTimeOffset -------------------------------------------
            DateTimeOffset dto when IsTarget<DateOnly>() => dto.ToDateOnly(),
            DateTimeOffset dto when IsTarget<TimeOnly>() => dto.ToTimeOnly(),
            DateTimeOffset dto when IsTarget<DateTime>() => dto.ToDateTime(),

            // --- String Conversions (Parsing) ---
            // Uses 'when' clauses for conditional parsing logic
            string str when IsTarget<DateOnly>() && DateOnly.TryParse(str, out var d) => d,
            string str when IsTarget<TimeOnly>() && TimeOnly.TryParse(str, out var t) => t,
            string str when IsTarget<DateTime>() && DateTime.TryParse(str, out var dt) => dt,
            string str when IsTarget<DateTimeOffset>() && DateTimeOffset.TryParse(str, out var dto) => dto,

            // --- Long Conversions (Ticks/Construction) ---
            long num when IsTarget<DateOnly>() => ToDateOnly(new DateTime(num)),
            long num when IsTarget<TimeOnly>() => ToTimeOnly(new DateTime(num)),
            long num when IsTarget<DateTime>() => new DateTime(num),
            long num when IsTarget<DateTimeOffset>() => new DateTimeOffset(num, TimeSpan.Zero),

            _ => null
        };

        return ((result == null) ? false : true);
    }
}