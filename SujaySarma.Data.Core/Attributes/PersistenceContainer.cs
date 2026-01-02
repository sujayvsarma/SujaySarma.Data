using System;

namespace SujaySarma.Data.Core.Attributes;

/// <summary>
/// An <see cref="Attribute" /> for business entity classes, structs or records to map them to backend datastorage 
/// containers like database tables.
/// </summary>
public class PersistenceContainer : Attribute, IPersistenceContainer
{
    /// <summary>
    /// Name of the table (or faux-table if the underlying system does not 
    /// use the concept of tables) connected to this business entity.
    /// </summary>
    public string TableName
    {
        get; init;
    }

    /// <summary>
    /// This function is called to retrieve the usable name for the container. Implementing attributes can use it
    /// to apply prefixes, suffixes or even contextually modify the value of the <see cref="TableName" /> property to
    /// provide a different or better name for the operation.
    /// </summary>
    /// <param name="quoteNames">Expected to be set by default, causes the returned name to be "quoted" appropriately for the platform, 
    /// for example: enclosed in [square brackets] on SQL Server.</param>
    /// <returns>The qualified or usable name to use for the container</returns>
    public virtual string CreateQualifiedName(bool quoteNames = true)
        => (quoteNames ? $"[{TableName}]" : TableName);

    /// <summary>
    /// An <see cref="Attribute" /> for business entity classes, structs or records to map them to backend datastorage 
    /// containers like database tables.
    /// </summary>
    /// <param name="tableName">Name of the table (or faux-table if the underlying system does not 
    /// use the concept of tables) connected to this business entity.</param>
    public PersistenceContainer(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentNullException(nameof(tableName), "Name of table cannot be null or empty");
        }

        TableName = tableName;
    }
}