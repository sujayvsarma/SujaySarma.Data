## SqlInsertBuilder

This document details the use of the SqlInsertBuilder statement builder introduced in version 8.8.0.

---
This statement builder helps build the INSERT  statement. It supports inserting data from a single business object, or multiple business objects. The SqlQueryBuilder also lets you specify arbitrary data to insert, and more. Let us first look at an example of using the builder. Assume that any objects and properties/fields referenced have been defined elsewhere.

```c#
SqlInsertBuilder<UserAccounts>
  .Begin()
    .AddItems(validatedCreatUserAccountModel)
    .WithAdditionalColumns(
      new Dictionary<string, object?>() {
        { "PasswordHash", "foo1234$##@"
      }
    )
```

Let's now look at what we did in more detail:

The builder is static in nature. So calling the `Begin()` method instantiates the builder and kicks off the building sequence. The table being inserted into is the one corresponding to the business object used as the `T` parameter for `SqlInsertBuilder<T>`.

The call to `AddItems` accepts one or more instances of business objects. Above, we are only adding one item, but this can be a sequence of items in `param array` fashion or an `IEnumerable` structure containing them. We may add additional values to the table that are not a part of the business object using the `WithAdditionalColumns` function. This function accepts a `Dictionary<string, object?>`. Values provided to the `WithAdditionalColumns` are NOT re-interpreted, meaning what you have in your code will match exactly the values added to the corresponding `INSERT` statement -- there will be no magic changing `Enums` to `strings` and so on, you will need to do them on your own.

Now that the builder is complete, there are 3 ways of using it:

**Method 1: Generate the T-SQL and use it elswhere.**
All the builders have the method `Build` that returns the T-SQL as a string. Simply call `Build` at the end of the sequence to get this T-SQL and use it in your own SqlCommand.

```c#
SqlCommand cmd = new(SqlInsertBuilder....Build());
```

**Method 2: Pass the builder as is to the SqlTableContext's Select method**
The SqlTableContext's methods have been updated to accept the relevant fluid builder. You can use the one for Select:

```c#
await tableContext.InsertAsync<UserAccounts>(SqlInsertBuilder...());
```

**Method 3: Use the extension method to call query directly**
Along with the fluid builders, we have also added relevant extension methods that attach to them. These extensions take a SqlTableContext as their parameter. Use this method to execute the query:

```c#
await SqlInsertBuilder...().Execute<UserAccounts>(tableContext);
```

Similar to the SqlTableContext class' `ExecuteNonQueryAsync`, the `ExecuteAsync` extension method returns an `int` -- number of rows affected in the corresponding SQL Server tables.

