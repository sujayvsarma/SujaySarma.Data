# Getting started
---

*This document provides documentation on how to get started with this library.*

Write your .NET business object as a `class` or `record`. For example:

```
public class Person
{
	public string LastName { get; set; }

	public string FirstName { get; set; }

	public DateTime DateOfBirth { get; set; }

	//...
}
```

Let's say you want to store this object in an SQL Server named `People`, add the following to the top 
of your class file:

*IMPORTANT:* You must create the database and tables beforehand. The SDK will NOT automatically create or manage them for you 
(unlike the Azure Tables Client SDK) as SQL Server object structure is much more complex than we want to deal with in this library. 

```
using SujaySarma.Data.SqlServer.Attributes;
```

and add this attribute above your class declaration:

```
[Table("People")]
public class Person ...
```

You ideally need at least one primary key for this class/table. To specify these fields, add the relevant attributes from 
our `SujaySarma.Data.SqlServer.Attributes` namespace. Let's define our `LastName` property as the `Primary Key`:

```
[TableColumn("LastName", KeyBehaviour = KeyBehaviourEnum.PrimaryKey)]
public string LastName { get; set; }
```

Just for fun, let's add a property to this class named `Hobbies` as a `List<string>`. We cannot store such types directly 
in the underlying table. Therefore, we need to tell the SDK to magically serialize it to Json and store that Json in the table.

```
[TableColumn("Hobbies", JsonSerialize = true)]
public List<string> Hobbies { get; set; } = new();
```

The completed class definition becomes:

```
[Table("People")]
public class Person
{
	[TableColumn("LastName", KeyBehaviour = KeyBehaviourEnum.PrimaryKey)]
	public string LastName { get; set; }

	[TableColumn("FirstName")]
	public string FirstName { get; set; }

	[TableColumn("DateOfBirth")]
	public DateTime DateOfBirth { get; set; }

	[TableColumn("Hobbies", JsonSerialize = true)]
	public List<string> Hobbies { get; set; } = new();
}
```

Let's instantiate an item and provide some data:

```
Person p = new() 
{
	LastName = "Doe",
	FirstName = "John",
	DateOfBirth = new DateTime(2000, 1, 1),
	Hobbies = new() { "Painting", "Reading" }
};
```

Now, we need to store this in our tables. The library provides the class `SqlTableContext`, which provides a "connection-less" paradigm 
to operate against the underlying Tables API. Initialize it with the connection string from above.

```
SqlTableContext tablesContext = new("<connection string>");
```

Of course, you can store the connection string in your Environment Variables or an appsettings.json or the KeyVault or wherever and 
use it from there.

You can also use the fluid-style initializers:

1. To initialize with local system database: 

```
SqlTableContext tablesContext = SqlTableContext.WithLocalDatabase("TempDB");
```

2. To initialize with provided connection string:

```
SqlTableContext tablesContext = SqlTableContext.WithConnectionString("<connection string>");
```


Insert the item we created into this storage (the method is `async`):

```
await tablesContext.InsertAsync<Person>(p);
```

That's it. Open up the table in your SQL Server Studio, you will find the `People` table there with the data you 
set above. Notice how the value of the `Hobbies` column is the Json-serialized value of the .NET property.

To read it back:

**Directly with SQL query (NOT recommended!):**
```
Person? p2 = tablesContext.SelectOnlyResultOrNull<Person>("SELECT * FROM People WHERE ([LastName] = 'Doe');");
```

Note that since we are trying to read a single row back, we are using the `SelectOnlyResultOrNull()` method.

**Parameters:**
```
Person? p2 = tablesContext.Select<Person>(
  parameters: new Dictionary<string, object?>() 
  {
    { "LastName", "Doe" }
  });
```

When you query using parameters, you would need to correctly serialize the values based on how they are 
stored in the table. For example, if you have an `Enum` that is serialized as a `string`, if you do ---

```
{ "DayOfWeek", DayOfWeek.Wednesday }
```

The query will **FAIL** because the query will use the `int` value of Wednesday. To do this correctly, you would need to --

```
{ "DayOfWeek", DayOfWeek.Wednesday.ToString() }
```

The return of the `Select()` will be a `Null` if it could not find the information requested. Examine the values of the properties in `p2` to 
confirm that all the values you stored earlier have been retrieved correctly. 

## Asynchronous operations

The SDK supports `async` operations via the `xxxAsync()` methods such as `InsertAsync()` in the `SqlTableContext` class. 

---

## Some typical gotchas

- When storing arrays/lists/collections of Enums as serialized Json, ensure you set the `JsonConverter(typeof(JsonStringEnumConverter))` 
attribute on the `enum` **definition** and **not** on the property! You set the converter attribute on the property only if it is a 
single value (not an array/collection).

---

### Powerful features

- You can store almost any type into a table using this library. If it does not store properly, just add a `JsonSerialize = true` to the 
`TableColumn` attribute. 

Happy coding!
