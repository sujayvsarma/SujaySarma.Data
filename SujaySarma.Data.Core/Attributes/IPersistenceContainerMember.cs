using System;

namespace SujaySarma.Data.Core.Attributes;

/// <summary>
/// An interface that must be implemented by <see cref="Attribute"/> classes that define metadata about how 
/// the member properties and fields of an <see cref="IPersistenceContainer"/> decorated business 
/// entity (class, record or struct) is mapped to a backend storage mechanism such 
/// as a database table's column, a field in a flatfile, or a column on a sheet in a spreadsheet, etc.
/// </summary>
public interface IPersistenceContainerMember : IOrmField
{
    /// <summary>
    /// Name of the field/column in the table (or faux-table if the underlying system 
    /// does not use the concept of tables) connected to this entity property or field.
    /// </summary>
    string TableFieldName 
    { 
        get; init;
    }

    /// <summary>
    /// This function is called to retrieve the usable name for the container member. Implementing attributes can use it
    /// to apply prefixes, suffixes or even contextually modify the value of the <see cref="TableFieldName" /> property to
    /// provide a different or better name for the operation.
    /// </summary>
    /// <param name="quoteNames">Expected to be set by default, causes the returned name to be "quoted" appropriately for the platform, 
    /// for example: enclosed in [square brackets] on SQL Server.</param>
    /// <returns>The qualified or usable name to use for the container</returns>
    string CreateQualifiedName(bool quoteNames = true);

}
