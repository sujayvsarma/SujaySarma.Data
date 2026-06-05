using System;

namespace SujaySarma.Data.TokenLimitedFiles.Attributes;

/// <summary>
/// Mark the member property/field as a reference to a sub-entity.
/// <para>
///     For example, if you have an entity named <b>Contact</b> and an address for that contact, 
///     instead of flattening all the address fields into the <b>Contact</b> structure, you may use principles of encapsulation 
///     to create an <b>Address</b> class/struct/record. To (de)serialise <b>Address</b> while operating on <b>Contact</b>, you would 
///     decorate <b>Contact.Address</b> (a member property/field within the Contact class) with the <see cref="FlatfileRefStructure"/> attribute 
///     as shown in the below code example:
/// </para>
/// <para>
///     <code>
/// [FlatFile]
/// public class Contact {
/// 
///     [FlatFileRefStructure]
///     public Address Address { get; set; }
/// 
/// }
/// </code>
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class FlatfileRefStructure : FlatfileField
{

    /// <summary>
    /// An optional prefix string that is applied to all member properties/fields of this sub structure 
    /// during serialisation -- the prefix shall also be expected during deserialisation!
    /// <para>
    ///     When <b>NULL</b>, no prefixes are added to or expected from the flatfile.
    /// </para>
    /// </summary>
    public string? Prefix
    {
        get;
        init;

    } = null;


    /// <summary>
    /// Mark the member property/field as a reference to a sub-entity.
    /// </summary>
    public FlatfileRefStructure()
        : base(string.Empty, 0)
    {
    }

    /// <summary>
    /// Returns the name enclosed in double quotes.
    /// </summary>
    /// <param name="quoteNames">(Not used)</param>
    /// <returns>Name enclosed in double quotes.</returns>
    /// <exception cref="InvalidOperationException">Always throws this exception. This function should never be called for this attribute.</exception>
    public override string CreateQualifiedName(bool quoteNames = true)
        => throw new InvalidOperationException();
}
