using System;

namespace SujaySarma.Data.Core.Attributes;

/// <summary>
/// An interface that must be implemented by <see cref="Attribute"/> classes that define metadata indicating 
/// the connected business entity (class, record or struct) supports the concept of "soft deleting" (where records are 
/// not really deleted or only marked as deleted).
/// </summary>
public interface ISoftDeleteRecords
{
    /// <summary>
    /// Name of the backend table column used to hold the soft-deletion flag. 
    /// This value is never fetched as part of any normal query, but may be 
    /// overridden through supported method parameters or used to filter by 
    /// in queries, etc.
    /// </summary>
    string SoftDeleteTableColumnName
    {
        get; init;
    }

}