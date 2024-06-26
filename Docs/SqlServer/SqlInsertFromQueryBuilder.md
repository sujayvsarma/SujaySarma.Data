## SqlInsertFromQueryBuilder

This document details the use of the SqlInsertFromQueryBuilder statement builder introduced in version 8.8.0.

---
This statement builder helps build the INSERT statement. It supports inserting data from another table (INSERT INTO ... SELECT). The SqlInsertFromQueryBuilder also lets you specify arbitrary data to insert. Let us first look at an example of using the builder. Assume that any objects and properties/fields referenced have been defined elsewhere.

```c#
SqlInsertFromQueryBuilder
  .IntoTable<UserAccounts>()
    .FromQuery(

      SqlQueryBuilder
        .From<UserAccountsBackup>()
        .Select()

    )
    .WithAdditionalColumns(
      new Dictionary<string, object?>() {
        { "PasswordHash", "foo1234$##@"
      }
    )
```

Let's now look at what we did in more detail:

The builder is static in nature. So calling the `IntoTable()` method instantiates the builder and kicks off the building sequence -- this also selects the table that we are inserting the value into. 

The call to `FromQuery` accepts a `SqlQueryBuilder` that defines the SELECT query we would use. We may add additional values to the table that are not a part of the business object using the `WithAdditionalColumns` function. This function accepts a `Dictionary<string, object?>`. Values provided to the `WithAdditionalColumns` are NOT re-interpreted, meaning what you have in your code will match exactly the values added to the corresponding `INSERT` statement -- there will be no magic changing `Enums` to `strings` and so on, you will need to do them on your own.

Now that the builder is complete, there are 3 ways of using it:

**Method 1: Generate the T-SQL and use it elswhere.**
All the builders have the method `Build` that returns the T-SQL as a string. Simply call `Build` at the end of the sequence to get this T-SQL and use it in your own SqlCommand.

```c#
SqlCommand cmd = new(SqlInsertFromQueryBuilder....Build());
```

**Method 2: Pass the builder as is to the SqlTableContext's Select method**
The SqlTableContext's methods have been updated to accept the relevant fluid builder. You can use the one for Select:

```c#
await tableContext.InsertAsync<UserAccounts>(SqlInsertFromQueryBuilder...());
```

**Method 3: Use the extension method to call query directly**
Along with the fluid builders, we have also added relevant extension methods that attach to them. These extensions take a SqlTableContext as their parameter. Use this method to execute the query:

```c#
await SqlInsertFromQueryBuilder...().Execute<UserAccounts>(tableContext);
```

Similar to the SqlTableContext class' `ExecuteNonQueryAsync`, the `ExecuteAsync` extension method returns an `int` -- number of rows affected in the corresponding SQL Server tables.

