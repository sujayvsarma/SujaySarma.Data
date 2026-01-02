SujaySarma.Data.Core
==================
This library is used as a dependency by other libraries named with the prefix `SujaySarma.Data.*`. `SujaySarma.Data.Core` provides core reflection attributes, attribute discovery, validation, data conversion, population, hydration and evaporation services to the upstream consuming libraries.

## Version table

Version | NuGet URL | Package link
--------|-----------|--------------------
`10.0.0.0` | `https://www.nuget.org/packages/SujaySarma.Data.Core` | [NuGet](https://www.nuget.org/packages/SujaySarma.Data.Core)

Last updated: `Nov 28, 2025`

## API
The following public-surface API is exposed by this library:

### Attributes
SujaySarma.Data.Core provides fully implemented base and specialist attributes. Upstream libraries utilising the `SujaySarma.Data.*` namespace extend these attributes for their purposes. Business entities (classes, structs and records) and their members (properties and fields) in applications must be decorated by one of these attributes to engage the powerful ORM capabilities of these libraries.


#### Object/Entity or Container level
(These attributes are used at class, struct or record level)

Nature | Attribute | Purpose
-------|-----------|-------------
Interface | `IPersistenceContainer` | Marks the entity as something that can be hydrated and dehydrated by our ORM system, providing metadata about the backend storage mechanism.
Implementation | `PersistenceContainer` | A concrete implementation of `IPersistenceContainer`.


#### Object/Entity member or container member level
(These attributes are used at property or field level)

Nature | Attribute | Purpose
-------|-----------|---------------
Interface | `IOrmField` | A base interface extended by all other member property/field attributes.
Interface | `IPersistenceContainerMember` | Marks the entity member as a participant in the hydration/dehydration ORM process, providing metata about the backend storage field/column.
Implementation | `PersistenceContainerMember` | A concrete implemetnation of `IPersistenceContainerMember`.
Interface | `ISystemPopulatedField` | Marks that the value for the member would be supplied automatically by the system.
Interface | `IBackendSystemPopulatedField` | Inherits `ISystemPopulatedField` -- the backend database would provide a value for such fields. Eg: an `IDENTITY` column.
Interface | `IOrmPopulatedField` | Inherits `ISystemPopulatedField` -- the ORM library would provide a value for this field. Eg: a `LastModified` timestamp, or an `Id` that uses automatic `NewGuid` for new records.
Implementation | `OrmPopulatedGuidField` | Inherits `IOrmPopulatedField > ISystemPopulatedField` -- implements metadata to allow for automatic new `Guid` population for the field. Eg: `Id` that uses automatic `NewGuid` for new records.
Implementation | `OrmPopulatedTimestampField` | Inherits `IOrmPopulatedField > ISystemPopulatedField` -- implements metadata to allow for automatic new `DateTime` population for the field. Eg: `LastModified` that uses automatic `DateTime.(Utc)Now` for new records.
Interface | `IDirtyStateField` | Marks that this member enables the business entity track its own "dirty state". The ORM can automatically check the value of this field to decide whether to insert, update or delete it from the backend database system when requested. (**NOTE:** The field or property annotated with this attribute must be of `bool` type).
Implementation | `DirtyStateField` | A concrete implementation of `IDirtyStateField`.
Interface | `ISoftDeleteRecords` | Marks that the backing database table supports the concept of "soft deletes", and provides metadata to deal with that. Soft-deleted records are never returned in normal queries (unless through an override) though they may be used to filter records in other ways.


> **NOTE:** *that the attributes `OrmPopulatedGuidField`, `OrmPopulatedTimestampField` and `DirtyStateField` do not extend the `PersistenceContainerMember` attribute, and are instead independent attributes. This lets the implementing ORM library deal with them separately from the other persistence container members.*
---

## Entry point
The primary entry point for this library is the `TypeDiscoveryFactory` class. When provided a type through one of its three overloads, it returns a `PersistenceContainerInfo` structure -- returning useful metadata for the ORM system about the objects, its members and the attributes decorating them. Critically, the `TypeDiscoveryFactory` interacts with a class, structure or record that is anotated with an attribute inheriting from `IPersistenceContainer` and retrieves the entity object's members (properties and fields) only if they are anotated with an attribute inheriting from `IPersistenceContainerMember`.

Metadata discovered through the `TypeDiscoveryFactory` are cached by the `TypeDiscoveryFactory`, resulting in faster future lookups. 

---

## Extension functions
The `ReflectionUtilities` namespace provides about a hundred extension methods to perform common ORM and data related tasks such as Date/Time/DateTime/DateTimeOffset conversions, type conversions and coercion, etc. Each function is fully documented.

---

## Transaction batching

This library provides a mechanism to manage batch transactions in data applications. The `BatchCollection` class provides the ability to iterate through a configurable number of elements per batch. This is useful when interacting with systems such as `Azure Storage Tables` and `Microsoft SQL Server` that allow data to be inserted, updated or deleted in batches.

The `Result` class (not connected to the `BatchCollection`) provides a consistent way of returning results from an operation that leverages transactions whether through the `BatchCollection` or otherwise.

---

> **NOTE:** This library contains other members marked "public" that are only intended for use by a library implementing a data access mechanism. These members are part of the internal implementation and should not be used directly by consumers of the library. They are subject to change without notice and may not be available in future versions of the library. Please see the code and documentation within SujaySarma.Data.* data access implementation libraries.


---
Copyright (c) 2025 and beyond, Sujay V. Sarma. All rights reserved.
Licensed under the MIT License. See LICENSE file in the project root for full license information.
Library authored and maintained by: Sujay V. Sarma.
Issues/Feedback/Suggestions/Feature requests: Please create an issue on the GitHub repository.

---
