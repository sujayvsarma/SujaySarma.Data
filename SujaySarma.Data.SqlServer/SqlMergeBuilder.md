# SqlMergeBuilder<TTarget> - Comprehensive Guide

**A fluent builder for SQL Server MERGE statements with full T-SQL feature parity.**

---

## Overview

The `SqlMergeBuilder<TTarget>` provides a type-safe, fluent API for constructing complex SQL Server MERGE statements. The MERGE statement is one of SQL Server's most powerful but complex features, allowing you to synchronize two tables by performing INSERT, UPDATE, or DELETE operations in a single atomic statement based on matching conditions.

This builder handles all the complexity of MERGE syntax while providing compile-time type safety and IntelliSense support.

---

## Table of Contents

1. [Why Use MERGE?](#why-use-merge)
2. [Basic Concepts](#basic-concepts)
3. [Getting Started](#getting-started)
4. [Builder Pattern Overview](#builder-pattern-overview)
5. [Complete API Reference](#complete-api-reference)
6. [Common Patterns](#common-patterns)
7. [Advanced Scenarios](#advanced-scenarios)
8. [Best Practices](#best-practices)
9. [Performance Considerations](#performance-considerations)
10. [Troubleshooting](#troubleshooting)

---

## Why Use MERGE?

The MERGE statement (also known as "UPSERT") combines INSERT, UPDATE, and DELETE operations into a single atomic statement. It's ideal for:

- **Data Synchronization** - Syncing staging tables with production tables
- **ETL Processes** - Loading data from external sources
- **Incremental Updates** - Applying changes from change tracking tables
- **Conditional Logic** - Complex business rules based on data comparison
- **Performance** - Single statement vs multiple round trips

---

## Basic Concepts

### The MERGE Statement Structure

```sql
MERGE [target_table] 
	AS T 
	USING [source_table] AS S ON (T.Id = S.Id) 
	WHEN MATCHED [AND condition] THEN 
		UPDATE SET T.Column = S.Column 
	WHEN NOT MATCHED BY TARGET [AND condition] THEN 
		INSERT (columns) VALUES (values) 
	WHEN NOT MATCHED BY SOURCE [AND condition] THEN 
		DELETE;
```


### Key Components

| Component | Purpose |
|-----------|---------|
| **Target Table** | The table being modified (`TTarget`) |
| **Source Table** | The table providing comparison data (`TSource`) |
| **ON Clause** | Join condition between target and source |
| **WHEN MATCHED** | Action when rows exist in both tables |
| **WHEN NOT MATCHED BY TARGET** | Action when row exists only in source (INSERT) |
| **WHEN NOT MATCHED BY SOURCE** | Action when row exists only in target (UPDATE/DELETE) |

---

## Getting Started

### Simple Example: Sync Products

```csharp
using SujaySarma.Data.SqlServer.Builders; 
using System; 
using System.Collections.Generic; 
using System.Linq.Expressions;

// Entities 
[SqlTable("Products", Schema = "dbo")] 
public class Product 
{ 
	[SqlTablePrimaryKeyColumn("Id")] 
	public int Id { get; set; }

	[SqlTableColumn("Name")]
	public string Name { get; set; }

	[SqlTableColumn("Price")]
	public decimal Price { get; set; }

	[SqlTableColumn("LastModified")]
	public DateTime LastModified { get; set; }
}

[SqlTable("ProductUpdates", Schema = "staging")]
public class ProductUpdate 
{ 
	[SqlTableColumn("Id")] 
	public int Id { get; set; }

	[SqlTableColumn("Name")]
	public string Name { get; set; }

	[SqlTableColumn("Price")]
	public decimal Price { get; set; }
}
```

***Build a MERGE statement***

```csharp
SqlMergeBuilder<Product> merge = SqlMergeBuilder<Product>.Create() 
	.UsingTable<ProductUpdate>((target, source) => target.Id == source.Id) 
		.BeginMatches() 
			.WhenMatched() 
				.Update() 
					.Set(new Dictionary<string, Expression<Func<ProductUpdate, object>>> 
						{ 
							{ "Name", s => s.Name }, 
							{ "Price", s => s.Price }, 
							{ "LastModified", s => DateTime.UtcNow } 
						})
			.WhenNotMatchedByTarget() 
				.Insert() 
					.Set(new Dictionary<string, Expression<Func<ProductUpdate, object>>> 
						{ 
							{ "Id", s => s.Id }, 
							{ "Name", s => s.Name }, 
							{ "Price", s => s.Price }, 
							{ "LastModified", s => DateTime.UtcNow } 
						})
		.EndMatches();

string sql = merge.Build().ToString();
```

***Generated SQL***
```sql
MERGE [dbo].[Products] WITH (HOLDLOCK) AS [t0] 
	USING [staging].[ProductUpdates] AS [t1] ON ([t0].[Id] = [t1].[Id]) 
		WHEN MATCHED THEN 
			UPDATE SET 
				[t0].[Name] = [t1].[Name], [t0].[Price] = [t1].[Price], [t0].[LastModified] = GETUTCDATE() 
		WHEN NOT MATCHED BY TARGET THEN 
			INSERT ([t0].[Id], [t0].[Name], [t0].[Price], [t0].[LastModified]) 
				VALUES ([t1].[Id], [t1].[Name], [t1].[Price], GETUTCDATE());
```


---

## Builder Pattern Overview

The `SqlMergeBuilder` uses a fluent, type-safe builder pattern with distinct phases:

```
Create() 
	↓ Using 
		↓ BeginMatches() 
			↓ When 
				↓ ↓ 
				Set() 
		↓ EndMatches() 
	↓ [Optional: WithOutput()] 
↓ Build()
```


---

## Complete API Reference

### 1. Initialization

#### `Create()`

```c#
public static UsingBuilder<TTarget> Create( SqlHint lockingHint = SqlHint.HoldLock, uint? top = null, bool topIsPercent = false )
```

**Parameters:**
- `lockingHint` - Lock hint for target table (default: `HOLDLOCK`). Recommended: `HOLDLOCK`, `TABLOCK`, `UPDLOCK`, `XLOCK`
- `top` - Limit affected rows
- `topIsPercent` - If true, `top` is interpreted as percentage (must be ≤ 100)

**Returns:** `UsingBuilder<TTarget>` to continue building

**Example:**

```c#
// Default (HOLDLOCK) 
var merge = SqlMergeBuilder<Product>.Create();

// Custom locking 
var merge = SqlMergeBuilder<Product>.Create(SqlHint.TabLock);

// With TOP 
var merge = SqlMergeBuilder<Product>.Create(top: 1000);

// With TOP percent 
var merge = SqlMergeBuilder<Product>.Create(top: 10, topIsPercent: true);
```

> ⚠️ **Important:** MERGE statements have potential for concurrency issues. Always use appropriate locking hints!

---

### 2. Source Specification - `UsingBuilder<TTarget>`

#### `UsingTable<TSource>()`

```c#
public MatchBuilder<TSource> UsingTable<TSource>( Expression<Func<TTarget, TSource, bool>> condition )
```


Specifies the source table and join condition.

**Parameters:**
- `condition` - Expression defining how target and source match

**Returns:** `MatchBuilder<TSource>`

**Example:**

```c#
merge.UsingTable<ProductUpdate>((target, source) => target.Id == source.Id)
```

#### `UsingQuery<TSource>()`

```c#
public MatchBuilder<TSource> UsingQuery<TSource>( SqlQueryBuilder query, Expression<Func<TTarget, TSource, bool>> condition )
```

Uses a SELECT query as the source instead of a table.

**Parameters:**
- `query` - A `SqlQueryBuilder` instance with SELECT statement
- `condition` - Expression defining how target and source match

**Returns:** `MatchBuilder<TSource>`

**Example:**

```c#
SqlQueryBuilder sourceQuery = SqlQueryBuilder.From<ProductUpdate>() 
	.Where<ProductUpdate>(p => p.IsActive);

merge.UsingQuery<ProductUpdate>(sourceQuery, (target, source) => target.Id == source.Id)
```


---

### 3. Match Definition - `MatchBuilder<TSource>`

#### `BeginMatches()`

Begins defining WHEN clauses. Can only be called once.

**Returns:** `WhenBuilder<TTarget, TSource>`


---

### 4. Conditional Actions - `WhenBuilder<TTarget, TSource>`

#### `WhenMatched()`

Defines action when row exists in both target and source.

**Parameters:**
- `condition` - Optional additional filter beyond the ON clause

(allows `.Update()` or `.Delete()`)

**Rules:**
- Only **ONE** unconditional `WHEN MATCHED` allowed
- Multiple conditional `WHEN MATCHED` clauses allowed
- Actions: UPDATE or DELETE

**Examples:**
```c#
// Unconditional update 
	.WhenMatched() 
		.Update() 
			.Set(...)

// Conditional update - only if source is newer 
	.WhenMatched((t, s) => t.LastModified < s.LastModified) 
		.Update() 
			.Set(...)

// Conditional delete - soft delete scenario 
	.WhenMatched((t, s) => !s.IsActive) 
		.Delete()
```

#### `WhenNotMatchedByTarget()`


Defines action when row exists in source but not in target (INSERT).

**Parameters:**
- `condition` - Optional filter (typically filters on source)

(allows `.Insert()`)

**Rules:**
- Only **ONE** unconditional `WHEN NOT MATCHED BY TARGET` allowed
- Multiple conditional clauses allowed
- Action: INSERT only

**Examples:**

```c#
// Unconditional insert 
	.WhenNotMatchedByTarget() 
		.Insert() 
			.Set(...)

// Conditional insert - only active records 
	.WhenNotMatchedByTarget((_, s) => s.IsActive) 
		.Insert() 
			.Set(...)
```

#### `WhenNotMatchedBySource()`


Defines action when row exists in target but not in source.

**Parameters:**
- `condition` - Optional filter (typically filters on target)

(allows `.Update()` or `.Delete()`)

**Rules:**
- Only **ONE** unconditional `WHEN NOT MATCHED BY SOURCE` allowed
- Multiple conditional clauses allowed
- Actions: UPDATE or DELETE

**Examples:**

```c#
// Delete orphaned records 
	.WhenNotMatchedBySource() 
		.Delete()

// Archive instead of delete 
	.WhenNotMatchedBySource((t, _) => !t.IsArchived) 
		.Update() 
			.Set(new Dictionary<string, Expression<Func<TTarget, object>>> 
				{ 
					{ "IsArchived", _ => true } 
				})
```

## Special behavior of Delete() action

If target table uses soft-delete (`SqlTableWithSoftDelete`), automatically generates: 

```sql
UPDATE SET [<SoftDeleteColumn>] = 1
```

instead of a 

```sql
DELETE
```

---

### 5. Completion

#### `EndMatches()`

Completes the WHEN clause definitions.

---

### 6. OUTPUT Clause (Optional)

#### `WithOutput()`

Begins building an OUTPUT clause to capture affected rows.

#### `OutputBuilder` Methods

| Method | Purpose |
|--------|---------|
| `AddActionColumn(string? alias = null)` | Adds `$action` column (shows INSERT/UPDATE/DELETE) |
| `AddTable<TTable>(Expression<...>? columnSelector = null)` | Adds columns from a table in the MERGE |
| `AddInserted(params IEnumerable<string> columnNames)` | Adds columns from INSERTED table |
| `AddInserted(Dictionary<string, string> columnNamesWithAliases)` | Adds INSERTED columns with aliases |
| `AddDeleted(params IEnumerable<string> columnNames)` | Adds columns from DELETED table |
| `AddDeleted(Dictionary<string, string> columnNamesWithAliases)` | Adds DELETED columns with aliases |
| `ToTable<TOutput>()` | Redirects output to an entity-mapped table |
| `ToTable(string tableName)` | Redirects output to specified table |
| `EndOutput()` | Completes OUTPUT clause |

**Example:**

```c#
	.EndMatches() 
		.WithOutput() 
			.AddActionColumn("Action")				// adds the "$action" special column aliased as "Action".
			.AddTable<Product>()					// adds all columns from the table backing the `Product` entity.
			.AddInserted("Id", "Name", "Price")		// Adds the specified columns from the INSERTED table.
			.AddDeleted("Id", "Name")				// Adds the specified columns from the DELETED table.
			.ToTable("AuditLog")					// Sends all output to the "@AuditLog" table.
		.EndOutput()
```

> NOTE: The ToTable does not support temporary tables (`#table`) or table variables (`@table`).

The SQL generated from the above OUTPUT clause would look like:
(new lines added for reading clarity)

```sql
OUTPUT 
	$action AS [Action],										--- $action column aliased.
		[Id], [Name], [Price], [IsActive],						--- from Products table.
		INSERTED.[Id], INSERTED.[Name], INSERTED.[Price],		--- specified columns from INSERTED table.
		DELETED.[Id], DELETED.[Name]							--- specified columns from DELETED table.
	INTO AuditLog												--- output sent to AuditLog table.
```

---

### 7. Build the Statement

#### `Build()`

Generates the final SQL MERGE statement.

**Returns:** `StringBuilder` containing the SQL

---

## Common Patterns

### Pattern 1: Sync Staging to Production

```c#
SqlMergeBuilder<Customer> merge = SqlMergeBuilder<Customer>.Create() 
	.UsingTable<CustomerStaging>((prod, stage) => prod.CustomerId == stage.CustomerId) 
		.BeginMatches() 
			// Update existing customers if data changed 
			.WhenMatched((p, s) => p.DataHash != s.DataHash) 
				.Update() 
					.Set( new Dictionary<string, Expression<Func<CustomerStaging, object>>> 
						{ 
							{ "Name", s => s.Name }, 
							{ "Email", s => s.Email }, 
							{ "Phone", s => s.Phone }, 
							{ "DataHash", s => s.DataHash } 
						}, 
						new Dictionary<string, Expression<Func<Customer, object>>> 
						{ 
							{ "LastModified", _ => DateTime.UtcNow } 
						}) 
			// Insert new customers 
			.WhenNotMatchedByTarget() 
				.Insert() 
					.Set( new Dictionary<string, Expression<Func<CustomerStaging, object>>> 
						{ 
							{ "CustomerId", s => s.CustomerId }, 
							{ "Name", s => s.Name }, 
							{ "Email", s => s.Email }, 
							{ "Phone", s => s.Phone }, 
							{ "DataHash", s => s.DataHash } 
						}, 
						new Dictionary<string, Expression<Func<Customer, object>>> 
						{ 
							{ "CreatedDate", _ => DateTime.UtcNow }, 
							{ "LastModified", _ => DateTime.UtcNow } 
						}) 
			// Archive removed customers 
			.WhenNotMatchedBySource((c, _) => !c.IsArchived) 
				.Update() 
					.Set( new Dictionary<string, Expression<Func<Customer, object>>> 
						{ 
							{ "IsArchived", _ => true }, 
							{ "ArchivedDate", _ => DateTime.UtcNow } 
						})
			.EndMatches();
```

### Pattern 2: Incremental Updates with Change Tracking

```c#
SqlQueryBuilder changesQuery = SqlQueryBuilder.From<ProductChanges>() .Where<ProductChanges>(c => c.ChangeDate > lastSyncDate);

SqlMergeBuilder<Product> merge = SqlMergeBuilder<Product>.Create() 
	.UsingQuery<ProductChanges>(changesQuery, (p, c) => p.ProductId == c.ProductId) 
		.BeginMatches() 
			.WhenMatched((p, c) => c.ChangeType == "UPDATE") 
				.Update() 
					.Set( new Dictionary<string, Expression<Func<ProductChanges, object>>> 
						{ 
							{ "Name", c => c.NewName }, 
							{ "Price", c => c.NewPrice } 
						})
			.WhenMatched((p, c) => c.ChangeType == "DELETE") 
				.Delete() 
					.WhenNotMatchedByTarget((_, c) => c.ChangeType == "INSERT") 
						.Insert() 
							.Set( new Dictionary<string, Expression<Func<ProductChanges, object>>> 
								{ 
									{ "ProductId", c => c.ProductId }, 
									{ "Name", c => c.NewName }, 
									{ "Price", c => c.NewPrice } 
								})
			.EndMatches();
```

### Pattern 3: Self-Join for Hierarchical Updates

```c#
SqlMergeBuilder<Employee> merge = SqlMergeBuilder<Employee>.Create()
	.UsingTable<Employee>((parent, child) => parent.EmployeeId == child.ManagerId) 
		.BeginMatches()
			.WhenMatched() 
				.Update() 
					.Set( new Dictionary<string, Expression<Func<Employee, object>>> 
						{ 
							{ "ManagerLevel", e => e.ManagerLevel + 1 } 
						})
			EndMatches();
```


---

## Advanced Scenarios

### Multiple Conditional WHEN Clauses

```c#
.BeginMatches() 
	// Priority 1: Update if source is newer and price changed significantly 
	.WhenMatched((t, s) => t.LastModified < s.LastModified && Math.Abs(t.Price - s.Price) > 10) 
		.Update() 
			.Set(...)
	// Priority 2: Update if just the name changed
	.WhenMatched((t, s) => t.Name != s.Name)
		.Update()
			.Set(...)

	// Priority 3: Deactivate if marked inactive in source
	.WhenMatched((t, s) => !s.IsActive)
		.Delete()

	// Priority 4: Insert new active records only
	.WhenNotMatchedByTarget((_, s) => s.IsActive && s.Price > 0)
		.Insert()
			.Set(...)
	.EndMatches();
```

### OUTPUT with Audit Trail

```c#
.EndMatches() 
	.WithOutput() 
		.AddActionColumn("Operation") 
			.AddInserted(new Dictionary<string, string> 
				{ { "Id", "NewId" }, { "Name", "NewName" }, { "Price", "NewPrice" } }) 
			.AddDeleted(new Dictionary<string, string> 
				{ { "Id", "OldId" }, { "Name", "OldName" }, { "Price", "OldPrice" } }) 
		.ToTable("ProductAuditLog") 
	.EndOutput()
```

### Complex Expressions

```c#
DateTime cutoffDate = DateTime.Now.AddMonths(-6);

	.Set( new Dictionary<string, Expression<Func<ProductUpdate, object>>> 
		{ 
			// Direct mapping 
			{ "Name", s => s.Name },

			// Conditional expression
			{ "Price", s => s.Price > 0 ? s.Price : 0.01m },
    
			// Math operations
			{ "DiscountedPrice", s => s.Price * 0.9m },
    
			// SQL Server functions (resolved server-side)
			{ "LastModified", s => DateTime.UtcNow },
			{ "UpdatedBy", s => "SYSTEM_USER" }
		},
		new Dictionary<string, Expression<Func<Product, object>>>
		{
			// Constant values
			{ "IsVerified", _ => true },
    
			// Using variables from scope
			{ "CutoffDate", _ => cutoffDate }
		}
	);
```


---

## Best Practices

### 1. Always Use Locking Hints

### 1. Always Use Locking Hints

```c#
// ✅ GOOD 
SqlMergeBuilder<Product>.Create(SqlHint.HoldLock)

// ⚠️ RISKY - default is HOLDLOCK, but be explicit 
SqlMergeBuilder<Product>.Create()
```

**Why:** MERGE statements are prone to race conditions and deadlocks without proper locking.

### 2. Order WHEN Clauses by Specificity

```c#
// ✅ GOOD - Most specific first 

	.WhenMatched((t, s) => t.LastModified < s.LastModified && t.Status != s.Status) 
		.Update().Set(...) 
	.WhenMatched((t, s) => t.LastModified < s.LastModified) 
		.Update().Set(...) 
	// catch-all
	.WhenMatched()  
		.Update().Set(...)
```

### Parallelism

- SQL Server may use parallel execution plans for MERGE
- Monitor with execution plans: `SET STATISTICS IO ON`

### Memory Grants

- Complex MERGE operations may request large memory grants
- Test with representative data volumes

---

## Requirements

- **SQL Server 2008+** (MERGE statement introduced in SQL Server 2008)
- **SujaySarma.Data.SqlServer 10.0.0.0+**
- **.NET 6.0+**

---

## See Also

- [Main SqlServer README](README.md)
- [SqlQueryBuilder Documentation](README.md#sqlquerybuilder)
- [SqlInsertBuilder Documentation](README.md#sqlinsertbuilder)
- [SqlUpdateBuilder Documentation](README.md#sqlupdatebuilder)
- [SqlDeleteBuilder Documentation](README.md#sqldeletebuilder)

---

## License

This library is licensed under the [MIT License](LICENSE).
Copyright (c) 2025 and beyond, Sujay V. Sarma. All rights reserved.

---