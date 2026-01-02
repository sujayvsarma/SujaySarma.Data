using System;

namespace SujaySarma.Data.Core.Attributes;

/// <summary>
/// An <see cref="Attribute"/> for the member properties and fields of a business entity class/struct/record to 
/// map them to a backend storage's fields, such as to a database table's fields/columns.
/// </summary>
public class PersistenceContainerMember : Attribute, IPersistenceContainerMember
{
    /// <summary>
    /// Name of the field/column in the table (or faux-table if the underlying system 
    /// does not use the concept of tables) connected to this entity property or field.
    /// </summary>
    public string TableFieldName
    {
        get; init;
    }

    /// <summary>
    /// If the value of the underlying member property or field is an Enum, 
    /// this property indicates how it should be serialised to the backend data store.
    /// </summary>
    public EnumSerialisationStrategy IfEnumSerialiseAs
    {
        get; init;

    } = EnumSerialisationStrategy.AsInteger;

    /// <summary>
    /// This function is called to retrieve the usable name for the container member. Implementing attributes can use it
    /// to apply prefixes, suffixes or even contextually modify the value of the <see cref="TableFieldName" /> property to
    /// provide a different or better name for the operation.
    /// </summary>
    /// <param name="quoteNames">Expected to be set by default, causes the returned name to be "quoted" appropriately for the platform, 
    /// for example: enclosed in [square brackets] on SQL Server.</param>
    /// <returns>The qualified or usable name to use for the container</returns>
    public virtual string CreateQualifiedName(bool quoteNames = true)
        => (quoteNames ? $"[{TableFieldName}]" : TableFieldName);


    /// <summary>
    /// An <see cref="Attribute"/> for the member properties and fields of a business entity class/struct/record to 
    /// map them to a backend storage's fields, such as to a database table's fields/columns.
    /// </summary>
    /// <param name="tableFieldName">Name of the field/column in the table (or faux-table if the underlying system 
    /// does not use the concept of tables) connected to this entity property or field.</param>
    public PersistenceContainerMember(string tableFieldName)
    {
        if (string.IsNullOrWhiteSpace(tableFieldName))
        {
            throw new ArgumentNullException(nameof(tableFieldName), "Name of the table field cannot be null or empty.");
        }

        TableFieldName = tableFieldName;
    }

    /// <summary>
    /// The serialisation strategy if the value is an Enum.
    /// </summary>
    public enum EnumSerialisationStrategy
    {
        /// <summary>
        /// As integer.
        /// </summary>
        AsInteger = 0,

        /// <summary>
        /// As its string representation.
        /// </summary>
        AsString = 1
    }
}