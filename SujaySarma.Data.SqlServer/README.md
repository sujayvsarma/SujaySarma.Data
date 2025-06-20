SujaySarma.Data.SqlServer
=========================
This is a .NET library that helps you manipulate objects and data in SQL Server databases. It supports on-prem SQL Servers, MSDE, LocalDB, named instances, SQL Azure and SQL on Linux scenarios.

API
----
This library provides the following public-surface API:

**Attributes**

Name | Description
-----|------------
Table | Map a class, structure or record to a database table.
TableColumn | Map a property or field to a database table column.

In addition, this library supports creating a table for a .NET object. To support this, you would need to provide column type constraints (such as marking them as IDENTITY or providing precision/scale values). This is achieved through the following attributes:

Name | Description
-----|---------------
ColumnPrecision | Specifies the precision value.
ColumnScale | Specifies the scale of precision.
Identity | Marks a column as an IDENTITY column and provides the SEED and INCREMENT values.


**How to use**
- Decorate your classes with the `TableAttribute` to indicate that they represent a table in SQL Server.
- Decorate the properties or fields in the classes with the `TableColumnAttribute` to indicate that they represent columns in the table. If you need to support the SDK's CreateTable functionality, add the required attributes from the second table of attributes above.
- Use the `SqlContext` class to interact with SQL Server. This class provides methods for performing both DDL and DML operations on the tables, in both synchronous and asynchronous modes.

---

This library contains other members marked "public" that are only intended for use by a library implementing a data access mechanism. These members are part of the internal implementation and should not be used directly by consumers of the library. They are subject to change without notice and may not be available in future versions of the library. Please see the code and documentation within SujaySarma.Data.* data access implementation libraries.*

---
Copyright (c) 2025 and beyond, Sujay V. Sarma. All rights reserved.

Licensed under the MIT License. See LICENSE file in the project root for full license information.

Library authored and maintained by: Sujay V. Sarma.

Issues/Feedback/Suggestions/Feature requests: Please create an issue on the GitHub repository.

---
