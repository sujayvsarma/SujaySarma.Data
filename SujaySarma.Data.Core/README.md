# SujaySarma.Data.Core

**Core reflection, attribute discovery, validation, data conversion, and ORM services for the SujaySarma.Data.* library ecosystem.**

[![NuGet](https://img.shields.io/nuget/v/SujaySarma.Data.Core.svg)](https://www.nuget.org/packages/SujaySarma.Data.Core)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

---

## Overview

`SujaySarma.Data.Core` is the foundational library for all `SujaySarma.Data.*` libraries. It provides a comprehensive set of attributes, reflection utilities, and ORM infrastructure that enable powerful object-relational mapping capabilities across various data backends including SQL Server, Azure Storage Tables, and flat files.

This library focuses on **performance**, **stability**, and **API consistency** as its core principles.

---

## Installation

```
$ dotnet add package SujaySarma.Data.Core
```


**NuGet Package:** [SujaySarma.Data.Core](https://www.nuget.org/packages/SujaySarma.Data.Core)

**Current Version:** `10.0.0.0`

**Target Frameworks:** `.NET 6.0`, `.NET 8.0`, `.NET 10.0`

---

## Features

- **Attribute-based ORM System** – Decorate your business entities with powerful attributes to enable automatic persistence
- **Type Discovery & Caching** – Fast metadata discovery with intelligent caching for performance
- **Data Conversion Utilities** – Extensive extension methods for type conversions, date/time handling, and data coercion
- **Batch Transaction Support** – Efficient batch processing for bulk operations
- **Dirty State Tracking** – Automatic change tracking for entities
- **Soft Delete Support** – Built-in support for soft-delete patterns
- **System-Populated Fields** – Automatic value generation for timestamps, GUIDs, and identity fields

---

## Quick Start

### 1. Decorate Your Entity

```c#
using SujaySarma.Data.Core.Attributes;
```

```c#
[PersistenceContainer(Name = "Users")]
public class User { 
	[PersistenceContainerMember(Name = "Id")] 
	[OrmPopulatedGuidField]
	public Guid Id { get; set; }

	[PersistenceContainerMember(Name = "Username")]
	public string Username { get; set; }

	[PersistenceContainerMember(Name = "Email")]
	public string Email { get; set; }

	[PersistenceContainerMember(Name = "CreatedDate")]
	[OrmPopulatedTimestampField]
	public DateTime CreatedDate { get; set; }

	[DirtyStateField]
	public bool IsDirty { get; set; }
}
```

### 2. Discover Type Metadata


```c#
using SujaySarma.Data.Core;
PersistenceContainerInfo containerInfo = TypeDiscoveryFactory.Resolve<User>(); 

// Access metadata about the User type, its members, and attributes
```


---

## API Reference

### Attributes

#### Container-Level Attributes
Attributes applied to classes, structs, or records:

| Attribute | Type | Purpose |
|-----------|------|---------|
| `IPersistenceContainer` | Interface | Marks an entity as ORM-enabled with metadata about backend storage |
| `PersistenceContainer` | Implementation | Concrete implementation of `IPersistenceContainer` |

#### Member-Level Attributes
Attributes applied to properties or fields:

| Attribute | Type | Purpose |
|-----------|------|---------|
| `IOrmField` | Interface | Base interface for all member attributes |
| `IPersistenceContainerMember` | Interface | Marks a member for hydration/dehydration with backend metadata |
| `PersistenceContainerMember` | Implementation | Concrete implementation of `IPersistenceContainerMember` |
| `ISystemPopulatedField` | Interface | Indicates the value is automatically supplied by the system |
| `IBackendSystemPopulatedField` | Interface | Value supplied by the backend (e.g., `IDENTITY` columns) |
| `IOrmPopulatedField` | Interface | Value supplied by the ORM (e.g., auto-generated GUIDs) |
| `OrmPopulatedGuidField` | Implementation | Automatically generates new `Guid` values |
| `OrmPopulatedTimestampField` | Implementation | Automatically generates `DateTime` timestamps |
| `IDirtyStateField` | Interface | Enables dirty state tracking for the entity |
| `DirtyStateField` | Implementation | Concrete implementation of `IDirtyStateField` |
| `ISoftDeleteRecords` | Interface | Enables soft-delete pattern support |

> **Note:** `OrmPopulatedGuidField`, `OrmPopulatedTimestampField`, and `DirtyStateField` are independent attributes that don't extend `PersistenceContainerMember`, allowing ORM libraries to handle them separately.

---

### Core Classes

#### `TypeDiscoveryFactory`

The primary entry point for metadata discovery. Analyzes types decorated with `IPersistenceContainer` and returns comprehensive metadata.

**Key Features:**
- Discovers entity metadata from attributes
- Caches results for performance
- Validates attribute configurations

**Usage:**

```c#
PersistenceContainerInfo info = TypeDiscoveryFactory.Resolve<MyEntity>(); 

// OR...

PersistenceContainerInfo info = TypeDiscoveryFactory.Resolve(typeof(MyEntity)); PersistenceContainerInfo info = TypeDiscoveryFactory.Resolve(myEntityInstance);
```


#### `BatchCollection`

Manages batch transactions for bulk operations with configurable batch sizes.

**Usage:**

```c#
BatchCollection<User> batches = new BatchCollection<User>(users, batchSize: 100); 
foreach ((Batch<User> batch, int batchIndex) in batches) 
{ 
	// Process each batch 
}
```


#### `Result`

Provides a consistent structure for returning transaction results.

---

### Extension Methods

The `ReflectionUtilities` namespace contains approximately 100 extension methods for:

- **Date/Time Conversions** – Convert between `DateTime`, `DateTimeOffset`, and various formats
- **Type Conversions & Coercion** – Safe type casting and conversion utilities
- **String Manipulation** – Common string operations for data processing
- **Collection Operations** – LINQ-style extensions for data collections
- **Validation Helpers** – Check nullability, emptiness, and validity

All extension methods are fully documented with XML comments.

---

## Advanced Usage

### Dirty State Tracking

```c#
[PersistenceContainer(Name = "Products")]
public class Product 
{ 
	[PersistenceContainerMember(Name = "Id")] 
	public int Id { get; set; }

	[PersistenceContainerMember(Name = "Name")]
	public string Name { get; set; }

	[DirtyStateField]
	public bool IsDirty { get; set; }
	// The ORM can check IsDirty to determine if INSERT/UPDATE/DELETE is needed
}
```


### Soft Delete Support

```c#
[PersistenceContainer(Name = "Orders")]
[ISoftDeleteRecords(DeletedFieldName = "IsDeleted")]
public class Order 
{ 
	[PersistenceContainerMember(Name = "Id")] 
	public int Id { get; set; }

	[PersistenceContainerMember(Name = "IsDeleted")]
	public bool IsDeleted { get; set; }
	// Soft-deleted records are excluded from queries unless explicitly requested
}
```


---

## Architecture

`SujaySarma.Data.Core` serves as the foundation for specialized data access libraries:

- **[SujaySarma.Data.SqlServer](https://github.com/sujayvsarma/SujaySarma.Data)** – SQL Server ORM with full T-SQL feature parity
- **[SujaySarma.Data.Files.TokenLimitedFiles](https://github.com/sujayvsarma/SujaySarma.Data)** – High-performance CSV/flat-file parser (84K records in <200ms)

---

## Version History

| Version | Release Date | Notes |
|---------|--------------|-------|
| `10.0.0.0` | Nov 28, 2025 | Complete rewrite – NOT backwards compatible |

---

## Requirements

- **.NET 6.0** or higher
- **C# 10.0** or higher (nullable reference types enabled)

---

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Submit a pull request with clear descriptions

For issues, feature requests, or feedback, please [create an issue](https://github.com/sujayvsarma/SujaySarma.Data/issues) on GitHub.

---

## License

This library is licensed under the [MIT License](LICENSE).
Copyright (c) 2025 and beyond, Sujay V. Sarma. All rights reserved.

---

## Author

**Sujay V. Sarma**

- GitHub: [@sujayvsarma](https://github.com/sujayvsarma)
- Repository: [SujaySarma.Data](https://github.com/sujayvsarma/SujaySarma.Data)

---

## Important Notes

> ⚠️ **Internal Members:** This library contains public members intended only for use by `SujaySarma.Data.*` implementation libraries. These are part of the internal implementation and should not be used directly by consumers. They are subject to change without notice.

> 📊 **Performance Focus:** As the foundational layer, this library prioritizes performance, stability, and API consistency above all else.

---