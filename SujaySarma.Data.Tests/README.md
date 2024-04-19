# Sujay Sarma's SQL Server Client SDK
---

(SQL Server, SQL Express, LocalDB, SQL Azure, SQL Server on Linux compatible)

Library                      | Current version
-----------------------------|------------------
SujaySarma.Data.SqlServer    | Version 8.9.7

## NuGet Package
https://www.nuget.org/packages/SujaySarma.Data.SqlServer

## Source code
https://github.com/sujayvsarma/SujaySarma.Data.SqlServer

### Changelog

Version | Changes
--------|----------
8.9.7 | Addresses security vulnerability in Microsoft.Data.SqlClient
8.9.6 | Adds support for .NET 6.0+
8.9.0 | Bug fixes, Adds `Enable|DisableDebugging()`
8.8.5 | Bug fixes, chain Join/Where/OrderBy statements, removes need for `qb.Select<T>()`
8.8.0 | Fluid builders, support for Lambda expressions
8.7.5 | Nullability bug fix in `ExecuteScalarAsync()`.
8.7.0 | SQL Injection mitigation for `Select()` methods.
8.5.0 | Added `UpsertAsync()` and `SQLTABLECONTEXT_DUMPSQL` env variable support. 
8.2.0 | New method `ExecuteStoredProcedure`. Other performance improvements and bug fixes.
8.0.0 | Initial version.

See [Change log](https://github.com/sujayvsarma/SujaySarma.Data.SqlServer/blob/master/CHANGELOG.md) for more details.

---

## About this library
This library simplifies writing data storage and retrieval code against databases hosted on Microsoft SQL Server technologies. You 
no longer need to use cumbersome frameworks like EntityFramework (EF) to simplify or automate your database interaction and ORM process.

This library is built on the same lines as my popular [Azure Tables Client SDK](https://www.nuget.org/packages/SujaySarma.Data.Azure.Tables/) and 
offers a highly simplified and super-performant structure.

## Dependencies
This package depends on the 'Microsoft.Data.SqlClient' library. And uses 'System.Data' and 'System.Reflection' extensively.

## Dependability and Trustworthiness

- The codebase is mostly the same as used in the 'Azure Tables Client SDK' (see link above).
- Given that the Azure Tables Client SDK is heavily performance optimized, all the same learnings have been directly applied here.
- Source code is available for your perusal on the GitHub page linked.

## Usage

For usage instructions, please refer to [this document](https://github.com/sujayvsarma/SujaySarma.Data.SqlServer/blob/master/USAGE.md)
Happy coding!

