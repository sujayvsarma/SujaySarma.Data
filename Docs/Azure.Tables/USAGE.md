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

Let's say you want to store this object in an Azure Storage Table (or CosmosDB container) named `People`, add the following to the top 
of your class file:

```
using SujaySarma.Data.Azure.Tables.Attributes;
```

and add this attribute above your class declaration:

```
[Table("People")]
public class Person ...
```

For this table, Microsoft mandates that you define a `PartitionKey` and a `RowKey` that can be stored only as `string`s. There are also 
[restrictions](https://learn.microsoft.com/en-us/rest/api/storageservices/understanding-the-table-service-data-model#system-properties) 
on what characters are valid for these values.

To specify these fields, add the relevant attributes from our `SujaySarma.Data.Azure.Tables.Attributes` namespace. Let's define our 
`LastName` property as the `PartitionKey` and `FirstName` as the `RowKey`:

```
[PartitionKey]
public string LastName { get; set; }

[RowKey]
public string FirstName { get; set; }
```

We want to store the `DateOfBirth` in a table column named `DateOfBirth`:

```
[TableColumn("DateOfBirth")]
public DateTime DateOfBirth { get; set; }
```

Finally, let's add a property to this class named `Hobbies` as a `List<string>`. We cannot store such types directly in the underlying table. 
Therefore, we need to tell the SDK to magically serialize it to Json and store that Json in the table.

```
[TableColumn("Hobbies", JsonSerialize = true)]
public List<string> Hobbies { get; set; } = new();
```

The completed class definition becomes:

```
[Table("People")]
public class Person
{
	[PartitionKey]
	public string LastName { get; set; }

	[RowKey]
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

Now, we need to store this in our tables. If you do not have one already, head over to the Azure Portal and create a new 
Azure Storage account. Then from its Access Keys blade, copy the `connection string` under `key1`. The library provides exactly 
one class `AzureTablesContext`, which provides a connection-less paradigm to operate against the underlying Tables API. Initialize 
it with the connection string from above.

```
AzureTablesContext tablesContext = new("<connection string>");
```

Of course, you can store the connection string in your Environment Variables or an appsettings.json or the KeyVault or wherever and 
use it from there.

You can also use the fluid-style initializers:

1. To initialize with Development Storage (we are Azurite compatible!). Note that this will use HTTP (not httpS)!!!

```
AzureTablesContext tablesContext = AzureTablesContext.WithDevelopmentStorage();
```

2. To initialize with provided connection string:

```
AzureTablesContext tablesContext = AzureTablesContext.WithConnectionString("<connection string>");
```


Insert the item we created into this storage.

```
tablesContext.Insert<Person>(p);
```

That's it. If you go to the Storage Browser blade of the storage account, you will find the `People` table there with the data you 
set above. Notice how the value of the `Hobbies` column is the Json-serialized value of the .NET property.


To read it back:

```
Person? p2 = tablesContext.SelectOnlyResultOrNull<Person>(partitionKey: "Doe", rowKey: "John");
```

The return of that call will be a Null if it could not find the information requested. Examine the values of the properties in `p2` to 
confirm that all the values you stored earlier have been retrieved correctly. 

---

**NOTE:** The Tables API stores DateTime values only as UTC. The `SujaySarma.Data.Azure.Tables` (this) library automatically **hard-codes**  
all `DateTime`, `DateTimeOffset` values to UTC while storing them. The value will not change, but it will be dealt with as 'UTC'. If you 
perform Timezone conversions in your code, you will need to 'unconvert' the value returned from the table back to whatever its original 
zone was:

```
DateTime dt = DateTime.SpecifyKind(p2.DateOfBirth, DateTimeKind.Local);
```

---

## Asynchronous operations

As of v8.x, the SDK supports `async` operations via the new `xxxAsync()` methods such as `InsertAsync` in the `AzureTablesContext` class. However, 
do note that the Async fetch operations (eg: `SelectAsync<T>`) will return `IAsyncEnumerable<T>` and you need to use your own `async foreach()` for example 
to loop through and fetch the results. To help, we do provide two extension methods (they are in the `AzureTablesContextExtensions` class but will attach to 
any `IAsyncEnumerable<T>` instance):

1. `async Task<bool> AnyAsync<T>(this IAsyncEnumerable<T> e, Predicate<T>? validation = null)`

This method checks if any item in the `IAsyncEnumerable` matches the provided condition in the same way as the `System.Linq` method `Any()` works.

2. `async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> e)`

This method accepts an `IAsyncEnumerable<T>` and returns a `List<T>`. It performs the `await foreach()` for you.

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
