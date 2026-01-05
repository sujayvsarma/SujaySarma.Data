# SujaySarma.Data.SqlServer

**ORM and fluent SQL builder for SQL Server with full T-SQL feature parity, without Entity Framework complexity.**

[![NuGet](https://img.shields.io/nuget/v/SujaySarma.Data.SqlServer.svg)](https://www.nuget.org/packages/SujaySarma.Data.SqlServer)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

---

## Overview

`SujaySarma.Data.SqlServer` provides a lightweight, high-performance ORM for SQL Server that focuses on T-SQL feature parity over abstraction. Build complex queries using fluent, type-safe builders or execute raw SQL with full control. Works with on-prem SQL Server, MSDE, LocalDB, named instances, SQL Azure, and SQL on Linux.

This library builds on **[SujaySarma.Data.Core](https://github.com/sujayvsarma/SujaySarma.Data)** and aims to provide everything possible with T-SQL through an intuitive API.

---

## Installation

```
$ dotnet add package SujaySarma.Data.SqlServer
```


**NuGet Package:** [SujaySarma.Data.SqlServer](https://www.nuget.org/packages/SujaySarma.Data.SqlServer)

**Current Version:** `10.0.0.0`

**Target Frameworks:** `.NET 6.0` (System.Data.SqlClient), `.NET 8.0+` (Microsoft.Data.SqlClient)

---

## Features

- **Fluent SQL Builders** – Type-safe query, insert, update, delete, and merge builders
- **Full T-SQL Support** – JOINs, CTEs, OUTPUT clauses, hints, TOP, DISTINCT, GROUP BY, and more
- **ORM Capabilities** – Attribute-based entity mapping with automatic serialization/deserialization
- **Connection String Builder** – Fluent API for building valid SQL Server connection strings
- **Raw SQL Execution** – Direct query execution with `SqlExecute` for complete control
- **Foreign Key Support** – Automatic JOIN generation for related entities
- **Soft Delete Support** – Built-in soft-delete pattern implementation
- **DDL Operations** – Create tables directly from entity definitions

> NOTE: The `SqlMergeBuilder` is a complicated builder and is documented in its own readme file here:
[SqlMergeBuilder](https://github.com/sujayvsarma/SujaySarma.Data/blob/main/SujaySarma.Data.SqlServer/SqlMergeBuilder.md)

---

## Quick Start

### 1. Decorate Your Entity

```c#
using SujaySarma.Data.SqlServer.Attributes; 
using SujaySarma.Data.Core.Attributes;

[SqlTable("Users", Schema = "dbo")] 
public class User 
{ 
	[SqlTablePrimaryKeyColumn("UserId")]
	[OrmPopulatedGuidField]
	public Guid UserId { get; set; }

	[SqlTableColumn("Username")]
	public string Username { get; set; }

	[SqlTableColumn("Email")]
	public string Email { get; set; }

	[SqlTableColumn("CreatedDate")]
	[OrmPopulatedTimestampField]
	public DateTime CreatedDate { get; set; }

	[SqlTablePopulatedColumn("LastLogin")]
	public DateTime? LastLogin { get; set; }
}
```


### 2. Build a Connection String

```c#
using SujaySarma.Data.SqlServer;
string connectionString = SqlConnectionStringBuilder
	.UsingServerAddress("localhost")
	.UsingDatabase("MyDatabase")
	.UsingCredential("username", "password")
	.EnableMultipleActiveResults()
	.EnableEncryption()
	.Build();
```


### 3. Query with SqlContext (ORM)

```c#
using SujaySarma.Data.SqlServer;

using (SqlContext context = SqlContext.Using(connectionString)) 
{ 
	// Query users 
	var users = context.Select<User>()
		.Where<User>(u => u.CreatedDate > DateTime.Now.AddDays(-30))
		.OrderByDESC<User>(u => u.CreatedDate) 
		.Execute();

	// Insert new user
	User newUser = new User
	{
		Username = "john.doe",
		Email = "john@example.com"
	};
	context.Insert(newUser);

	// Update existing user
	User existingUser = users.First();
	existingUser.Email = "newemail@example.com";
	context.Update(existingUser);

	// Delete user
	context.Delete(existingUser);
}
```


### 4. Build Queries with Fluent Builders

```c#
using SujaySarma.Data.SqlServer;

// Execute query 
Result queryResult = SqlExecute.Query(connectionString, "SELECT * FROM Users WHERE Active = 1"); 

if (queryResult is QueryResult qr) 
{ 
	DataSet data = qr.Data; 
	// Process data... 
}

// Execute stored procedure 
var inputParams = new Dictionary<string, object?> 
{ 
	{ "@UserId", userId }, 
	{ "@Status", "Active" } 
};

var outputParams = new Dictionary<string, object> 
{ 
	{ "@RecordCount", 0 } 
};

Result procResult = SqlExecute.ProcedureOrFunction( connectionString, "sp_UpdateUserStatus", inputParams, outputParams );
if (procResult is ProcedureOrFunctionResult pfr) 
{ 
	int returnValue = pfr.ReturnValue; 
	int recordCount = (int)pfr.ReturnParameters["@RecordCount"]; 
}

// Execute non-query (INSERT/UPDATE/DELETE) 
Result nonQueryResult = SqlExecute.NonQuery( connectionString, "DELETE FROM Users WHERE LastLogin < '2020-01-01'" );
if (nonQueryResult is NonQueryResult nqr) 
{ 
	int affectedRows = nqr.AffectedRowCount; 
}
```


---

## API Reference

### Attributes

#### Container-Level Attributes

| Attribute | Purpose |
|-----------|---------|
| `SqlTable` | Maps a class/struct/record to a SQL Server table with schema support |

**Properties:**
- `Schema` - The parent schema (default: `"dbo"`)

#### Member-Level Attributes

| Attribute | Purpose |
|-----------|---------|
| `SqlTableColumn` | Maps a property/field to a table column |
| `SqlTablePrimaryKeyColumn` | Marks a column as the PRIMARY KEY |
| `SqlTablePopulatedColumn` | Marks a column as populated by SQL Server (IDENTITY, DEFAULT, triggers, etc.) |
| `SqlTableForeignKeyColumn` | Marks a column as a FOREIGN KEY with automatic JOIN support |

**`SqlTableForeignKeyColumn` Constructors:**

```c#
// String-based FK (for dynamic references) 
SqlTableForeignKeyColumn( string columnName, string referencedTableName, string referencedColumnName, string referencedTableAlias )

// Type-safe FK (with automatic join generation) 
SqlTableForeignKeyColumn( string columnName, Type referencedTableEntity, string referencedPropertyOrField )
```


**Properties:**
- `Direction` - Gets the FK relationship direction (`ChildToParent` or `ParentToChild`)

---

### Core Classes

#### `SqlContext`

The primary ORM entry point for database operations.

**Initialization:**

- SqlContext Using(SqlConnectionStringBuilder builder) 
- SqlContext Using(string connectionString)

**Operations:**
- `Select<TEntity>()` - Query entities
- `Insert<TEntity>(entity)` - Insert entity
- `Update<TEntity>(entity)` - Update entity  
- `Delete<TEntity>(entity)` - Delete entity
- `Execute()` - Execute the built query

#### `SqlExecute`

Static class for executing raw SQL synchronously.

**Methods:**

- Result Query(string connectionString, StringBuilder query)
- Result Query(string connectionString, SqlQueryBuilder query)
- Result QueryScalar(string connectionString, StringBuilder query)
- Result QueryScalar(string connectionString, SqlQueryBuilder query)
- Result QueryBinary(string connectionString, StringBuilder query, int length, int index = 0, long offset = 0L)
- Result ProcedureOrFunction( string connectionString, string procedureName, Dictionary<string, object?> inputParameters, Dictionary<string, object>? outputParameters )
- Result ProcedureOrFunction(string connectionString, SqlExecBuilder execBuilder)
- Result NonQuery(string connectionString, StringBuilder script)
- Result NonQuery(string connectionString, SqlStatementBuilder script)


**Return Types:**
- `QueryResult` - Contains `DataSet` with query results
- `QueryScalarResult` - Contains single scalar value
- `QueryBinaryResult` - Contains binary data
- `ProcedureOrFunctionResult` - Contains procedure results, return value, and output parameters
- `NonQueryResult` - Contains affected row count
- `ErrorResult` - Contains exception information

---

### Fluent Builders


Note that all builders support the following methods as applicable:

***WHERE clauses:***
- `Where<TEntity>(Expression<Func<TEntity, bool>> condition)`
- `Where<T1, T2>(Expression<Func<T1, T2, bool>> condition)`
- `AndWhere<TEntity>(Expression<Func<TEntity, bool>> condition)` 
- `OrWhere<TEntity>(Expression<Func<TEntity, bool>> condition)`

**JOINs**
- `InnerJoin<T1, T2>(Expression<Func<T1, T2, bool>> condition, SqlHint hints)` 
- `LeftJoin<T1, T2>(Expression<Func<T1, T2, bool>> condition, SqlHint hints)` 
- `RightJoin<T1, T2>(Expression<Func<T1, T2, bool>> condition, SqlHint hints)` 
- `FullJoin<T1, T2>(Expression<Func<T1, T2, bool>> condition, SqlHint hints)` 
- `CrossJoin<T1, T2>(SqlHint hints)`

Except `SqlQueryBuilder`, the other builders support output of affected rows via methods:

Method | SqlInsertBuilder | SqlUpdateBuilder | SqlDeleteBuilder | [SqlMergeBuilder](https://github.com/sujayvsarma/SujaySarma.Data/blob/main/SujaySarma.Data.SqlServer/SqlMergeBuilder.md)
-------|------------------|------------------|------------------|------------------
OutputInserted() | ✓ | ✓ | - | ✓
OutputInserted(IEnumerable<string> columnNames) | ✓ | ✓ | - | ✓
OutputDeleted() | - | ✓ | ✓ | ✓
OutputDeleted(IEnumerable<string> columnNames) | - | ✓ | ✓ | ✓
OutputToTable<TEntity>() | ✓ | ✓ | ✓ | ✓
OutputToTable(string tableName) | ✓ | ✓ | ✓ | ✓

> `OutputInserted` causes the INSERTED table to be written, `OutputDeleted` causes the DELETED table to be written. When `OutputToTable` is specified, the rows are written to the specified table, otherwise they are returned as part of the resultset from the operation.

```c#
// INSERT with OUTPUT 
SqlInsertBuilder insert = SqlInsertBuilder.Into<User>() 
	.Value<User>(u => u.Username, "newuser") 
		.OutputInserted(new[] { "UserId", "CreatedDate" }) 
			.OutputToTable("@InsertedUsers");

// UPDATE with OUTPUT 
SqlUpdateBuilder update = SqlUpdateBuilder.Into<User>() 
	.Set(user) 
		.OutputUpdated()	// INSERTED.* and DELETED.* 
			.Where<User>(u => u.UserId == userId);

// DELETE with OUTPUT 
SqlDeleteBuilder delete = SqlDeleteBuilder.From<User>() 
	.OutputDeleted(new[] { "UserId", "Username" }) 
		.Where<User>(u => u.LastLogin < DateTime.Now.AddYears(-1));
```

Except `SqlMergeBuilder`, the other builders also support hints via method:

```c#
<builder>.With(SqlHint hints)
```

For `SqlMergeBuilder`, hints are specified in the initialiser:

```c#
SqlMergeBuilder.Using<TTable>.Create(SqlHint lockingHint = SqlHint.HoldLock, uint? top = null, bool topIsPercent = false);
-------------------------------------^^^^^^^^^^^^^^^^^^^^
```


#### `SqlQueryBuilder`

Builds SELECT statements with full T-SQL support.

**Initialization:**

```c#
SqlQueryBuilder queryBuilder = SqlQueryBuilder.From<TTable>();
```

**Clauses:**

- Select<TEntity>(Expression<Func<TEntity, object>>? columnSelector = null) 
- Top(uint count, bool isPercent = false) 
- Distinct() SqlQueryBuilder Into<TEntity>() 
- Into(string tableName)

**ORDER BY**
- OrderByASC<TEntity>(Expression<Func<TEntity, object>> columns)
- OrderByDESC<TEntity>(Expression<Func<TEntity, object>> columns) 

**GROUP BY**
- GroupByRollup<TEntity>(Expression<Func<TEntity, object>>? columns)
- GroupByCube<TEntity>(Expression<Func<TEntity, object>>? columns) 
- GroupByGroupingSets<TEntity>(Expression<Func<TEntity, object>>? columns) 
- GroupByEmpty<TEntity>()


**Overriding SOFT-DELETE to query for soft-deleted rows***
- IncludingDeletedRows()

```c#
queryBuilder.Build();
```


#### `SqlInsertBuilder`

Builds INSERT statements.

**Initialization:**

```c#
SqlInsertBuilder insertBuilder = SqlInsertBuilder.Into<TTable>();
```

**Methods:**

- Top(uint count, bool isPercent = false) 
- Value<TEntity>(Expression<Func<TEntity, object>> column, object value) 
- UsingDefaultValues() 
- From(SqlQueryBuilder query) 
- From(StringBuilder query, IEnumerable<string>? columnNames = null) 
- With(SqlHint hints)

```c#
insertBuilder.Build();
```

#### `SqlUpdateBuilder`

Builds UPDATE statements.

**Initialization:**

```c#
SqlUpdateBuilder updateBuilder = SqlUpdateBuilder.Into<TTable>();
```

***METHODS***

- Update<TEntity>(TEntity entity) 
- UpdateMany<TEntity>(params IEnumerable<TEntity> entities)  // Auto-generates WHERE on PK

- Top(uint count, bool isPercent = false) 
- Set<TEntity>( TEntity entity, Dictionary<string, Expression<Func<TEntity, object>>>? additionalValues = null ) 
- UpdateFrom<T1, T2>( Expression<Func<T1, T2, bool>> joinCondition, SqlHint hints, Dictionary<string, Expression<Func<T2, object>>> columnMappings ) 

```c#
updateBuilder.Build();
```

#### `SqlDeleteBuilder`

Builds DELETE statements.

**Initialization:**
```c#
SqlDeleteBuilder deleteBuilder = SqlDeleteBuilder.From<TTable>();
```

***METHODS***

- Delete<TEntity>(TEntity entity) 
- DeleteMany<TEntity>(params IEnumerable<TEntity> entities)  // Auto-generates WHERE on PK

- Top(uint count, bool isPercent = false) 


#### `SqlConnectionStringBuilder`

Fluent builder for SQL Server connection strings.

**Initialization:**

```c#
SqlConnectionStringBuilder builder = SqlConnectionStringBuilder.UsingServerAddress(string serverNameOrAddress);
```

***Methods***

- UsingDatabase(string database)
- UsingCredential(string userName, string password)
- UsingNamedInstance(string instanceName)
- UsingNamedPipes() 
- UsingPort(ushort port)

***Various flags***

> Flags that are not explicitly enabled using the below functions are left untouched, causing the connection to automatically use whatever is the default setting for that flag.

- EnableAllFlags() 
- DisableAllFlags()
- EnablePersistSecurityInfo()
- DisablePersistSecurityInfo()
- EnableConnectionPooling() 
- DisableConnectionPooling()
- EnableMultipleActiveResults() 
- DisableMultipleActiveResults()
- EnableEncryption()
- DisableEncryption() 
- EnableTrustServerCertificate() 
- DisableTrustServerCertificate()


```c#
builder.Build();
```


---

## Advanced Features

### Foreign Key Relationships with Auto-JOIN

Consider the object definitions:

```c#
[SqlTable("Orders", Schema = "dbo")] 
public class Order 
{ 
	[SqlTablePrimaryKeyColumn("OrderId")] 
	public int OrderId { get; set; }

	[SqlTableForeignKeyColumn("CustomerId", typeof(Customer), nameof(Customer.CustomerId))]
	public int CustomerId { get; set; }

	[SqlTableColumn("OrderDate")]
	public DateTime OrderDate { get; set; }
}

[SqlTable("Customers", Schema = "dbo")] 
public class Customer 
{ 
	[SqlTablePrimaryKeyColumn("CustomerId")] 
	public int CustomerId { get; set; }

	[SqlTableColumn("CustomerName")]
	public string CustomerName { get; set; }
}
```

When querying `Order`, the ORM can automatically generate JOINs to `Customer` based on the foreign key relationship:

```c#
// Automatically generates JOIN 

var query = SqlQueryBuilder.From<Order>()
			.Select<Order>()
			.InnerJoin<Order, Customer>((o, c) => o.CustomerId == c.CustomerId, SqlHint.None)
			.Where<Order>(o => o.OrderDate > DateTime.Now.AddDays(-30));
```

### Soft-delete support

"Soft-delete" is the mechanism of marking a row as deleted in the table without actually deleting it. The most common approach is to include a table-only column (typically a `boolean` type named `IsDeleted`) and marking this (as `TRUE`). While these rows exist on the table, they are never retrieved by any query or operation.

This library supports soft-delete natively via the `SqlTableWithSoftDelete` attribute at the container level:

```c#
using SujaySarma.Data.SqlServer.Attributes;

[SqlTableWithSoftDelete("Users", DeletedFieldName = "IsDeleted", Schema = "dbo")] 
public class User 
{	
	[SqlTablePrimaryKeyColumn("UserId")] 
	public Guid UserId { get; set; }

	[SqlTableColumn("Username")]
	public string Username { get; set; }

	// NOTE that the IsDeleted column is NOT a part of the entity definition!
}

// Automatically excludes soft-deleted records 
var activeUsers = SqlQueryBuilder.From<User>() 
	.Select<User>() 
		.Build(); 
		
/* Generates: "SELECT * FROM [dbo].[Users] WHERE [IsDeleted] = 0" */

// Include deleted records 
var allUsers = SqlQueryBuilder.From<User>() 
	.Select<User>() 
		.IncludingDeletedRows() 
			.Build(); 
			
/* Generates: "SELECT * FROM [dbo].[Users] */

```

### Complex queries with multiple JOINs

```c#

var query = SqlQueryBuilder.From<Order>() 
	.Select<Order>(o => new { o.OrderId, o.OrderDate }) 
		.InnerJoin<Order, Customer>((o, c) => o.CustomerId == c.CustomerId, SqlHint.None) 
			.LeftJoin<Order, OrderDetail>((o, od) => o.OrderId == od.OrderId, SqlHint.None) 
				.Where<Order>(o => o.OrderDate >= DateTime.Now.AddMonths(-3)) 
					.AndWhere<Customer>(c => c.Country == "USA") 
						.OrderByDESC<Order>(o => o.OrderDate) 
							.Top(100);
								.Build();
```


> NOTE: The `SqlMergeBuilder` is a complicated builder and is documented in its own readme file here:
[SqlMergeBuilder](https://github.com/sujayvsarma/SujaySarma.Data/blob/main/SujaySarma.Data.SqlServer/SqlMergeBuilder.md)

---

## Requirements

- **.NET 6.0** or higher
- **System.Data.SqlClient** (.NET 6.0) or **Microsoft.Data.SqlClient** (.NET 8.0+)
- **SujaySarma.Data.Core** 10.0.0.0 or higher

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

> 🎯 **T-SQL Parity:** This library aims to provide feature parity with T-SQL. If something is possible in T-SQL, it should be possible through this library's API.

> 🚀 **Version 10.0.0.0:** Complete rewrite – NOT backwards compatible with previous versions.

---