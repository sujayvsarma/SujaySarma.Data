SujaySarma.Data.Azure.Tables
=========================
This is a .NET library for working with Azure Table Storage. It provides a set of classes and interfaces to help you manage and manipulate data in Azure Table Storage efficiently. Note that this library is a "core" library and is not intended for standalone use. It is designed to be used as a dependency in other projects that require data management capabilities with Azure Table Storage.

API
----
This library provides the following public-surface API:

**Attributes**

Name | Description
-----|------------
Table | An attribute that can be applied to classes to indicate that they represent a table in Azure Table Storage.
TableColumn | An attribute that can be applied to properties of a class to indicate that they represent a column in Azure Table Storage.
PartitionKey | An attribute that can be applied to a property of a class to indicate that it represents the partition key for the table.
RowKey | An attribute that can be applied to a property of a class to indicate that it represents the row key for the table.
ETag | An attribute that can be applied to a property of a class to indicate that it represents the ETag for the table entity.
Timestamp | An attribute that can be applied to a property of a class to indicate that it represents the timestamp for the table entity.

**How to use**
- Decorate your classes with the `TableAttribute` to indicate that they represent a table in Azure Table Storage.
- Decorate the properties or fields in the classes with the `TableColumnAttribute` to indicate that they represent columns in the table. If they represent one of the special columns such as a `PartitionKey`, `RowKey`, `Timestamp` or `ETag`, decorate them with the appropriate attribute from the Attributes table above.
- Use the `AzureTablesContext` class to interact with Azure Table Storage. This class provides methods for performing both DDL and DML operations on the tables and entities, in both synchronous and asynchronous modes.

---

This library contains other members marked "public" that are only intended for use by a library implementing a data access mechanism. These members are part of the internal implementation and should not be used directly by consumers of the library. They are subject to change without notice and may not be available in future versions of the library. Please see the code and documentation within SujaySarma.Data.* data access implementation libraries.*

---
Copyright (c) 2025 and beyond, Sujay V. Sarma. All rights reserved.

Licensed under the MIT License. See LICENSE file in the project root for full license information.

Library authored and maintained by: Sujay V. Sarma.

Issues/Feedback/Suggestions/Feature requests: Please create an issue on the GitHub repository.

---
