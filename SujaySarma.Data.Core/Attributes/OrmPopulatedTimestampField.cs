using System;

namespace SujaySarma.Data.Core.Attributes;

/// <summary>
/// An <see cref="Attribute"/> to be applied to <see cref="DateTime" />-typed member properties/fields of business entities. This 
/// tells the ORM system that if the value of that property/field is NULL or <see cref="DateTime.MinValue"/> at the time of persistence 
/// (inserts and updates), the ORM will need to set a value (a new DateTime value) to it prior to persistence.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class OrmPopulatedTimestampField : Attribute, IOrmPopulatedField
{
    /// <summary>
    /// Indication of whether the DateTime to be set must be in 
    /// Local time or UTC time.
    /// </summary>
    public DateTimeKind Kind
    {
        get; init;
    }

    /// <summary>
    /// When set, the Time portion of the DateTime value is 
    /// set to 00:00:00.
    /// </summary>
    public bool ZeroOutTime
    {
        get; set;
    }

    /// <summary>
    /// An <see cref="Attribute"/> to be applied to <see cref="DateTime" />-typed member properties/fields of business entities. This 
    /// tells the ORM system that if the value of that property/field is NULL or <see cref="DateTime.MinValue"/> at the time of persistence 
    /// (inserts and updates), the ORM will need to set a value (a new DateTime value) to it prior to persistence.
    /// </summary>
    /// <param name="kind">Indication of whether the DateTime to be set must be in Local time or UTC time.</param>
    /// <param name="zeroOutTime">When set, the Time portion of the DateTime value is set to 00:00:00.</param>
    public OrmPopulatedTimestampField(DateTimeKind kind, bool zeroOutTime = false)
    {
        if ((!Enum.IsDefined(typeof(DateTimeKind), kind)) || (kind == DateTimeKind.Unspecified))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), $"Value of '{nameof(kind)}' must be either Local or Utc.");
        }

        Kind = kind;
        ZeroOutTime = zeroOutTime;
    }

    /// <inheritdoc />
    public object? GetOrmPopulatedValue(object? currentValue)
    {
        DateTime dt = (Kind) switch
        {
            DateTimeKind.Utc => DateTime.UtcNow,
            DateTimeKind.Local => DateTime.Now,

            // We will never reach here, but to satify the compiler!
            _ => throw new ArgumentOutOfRangeException(nameof(Kind), $"Value of '{nameof(Kind)}' must be either Local or Utc.")
        };

        if (ZeroOutTime)
        {
            dt = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, Kind);
        }

        return dt;
    }
}