using System;

namespace SujaySarma.Data.Core.Attributes;

/// <summary>
/// An <see cref="Attribute"/> to be applied to <see cref="Guid" />-typed member properties/fields of business entities. This 
/// tells the ORM system that if the value of that property/field is NULL or an <see cref="Guid.Empty" /> guid at the time of persistence 
/// (inserts and updates), the ORM will need to set a value (a new Guid) to it prior to persistence. 
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class OrmPopulatedGuidField : Attribute, IOrmPopulatedField
{

    /// <summary>
    /// The mode of activation for the field's automatic population logic.
    /// </summary>
    public ActivateOn Activation
    {
        get; init;

    } = ActivateOn.Either;

    /// <inheritdoc />
    public object? GetOrmPopulatedValue(object? currentValue)
    {
        if ((currentValue is null) && Activation.HasFlag(ActivateOn.Nulls))
        {
            return Guid.NewGuid();
        }

        if ((currentValue is Guid g) && (g == Guid.Empty) && Activation.HasFlag(ActivateOn.Empty))
        {
            return Guid.NewGuid();
        }

        return currentValue;
    }

    /// <summary>
    /// An <see cref="Attribute"/> to be applied to <see cref="Guid" />-typed member properties/fields of business entities. This 
    /// tells the ORM system that if the value of that property/field is NULL or an <see cref="Guid.Empty" /> guid at the time of persistence 
    /// (inserts and updates), the ORM will need to set a value (a new Guid) to it prior to persistence. 
    /// </summary>
    /// <param name="activation">The mode of activation for the field's automatic population logic.</param>
    public OrmPopulatedGuidField(ActivateOn activation)
    {
        if (!Enum.IsDefined(activation))
        {
            throw new ArgumentOutOfRangeException(nameof(activation), "Value must belong to enumeration.");
        }

        Activation = activation;
    }

    /// <summary>
    /// The kind of values the field's autopopulation engages.
    /// </summary>
    [Flags]
    public enum ActivateOn
    {
        /// <summary>
        /// None -- (duh!) don't set this!
        /// </summary>
        None = 0,

        /// <summary>
        /// When the property/field is a NULL.
        /// </summary>
        Nulls = 1,

        /// <summary>
        /// When the property/field is an Guid.Empty.
        /// </summary>
        Empty = 2,

        /// <summary>
        /// When the property/field is either NULL or a Guid.Empty.
        /// </summary>
        Either = Nulls | Empty
    }
}
