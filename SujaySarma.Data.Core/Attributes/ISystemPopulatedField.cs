using System;

namespace SujaySarma.Data.Core.Attributes;

/// <summary>
/// An common interface implemented by <see cref="Attribute"/>s on member properties/fields of business entity 
/// classes/structs/fields to indicate they are populated by the system (application, ORM or backend datastorage) 
/// at the time of persistence and hence would not be initialised.
/// </summary>
public interface ISystemPopulatedField : IOrmField
{
}
