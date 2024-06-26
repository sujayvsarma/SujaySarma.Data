## SqlUpdateWithJoinsBuilder

This document details the use of the SqlUpdateWithJoinsBuilder statement builder introduced in version 8.8.0.

---
This statement builder helps build the UPDATE statement. It supports inserting data from another table (UPDATE x SET... FROM Table x JOIN ...). The SqlUpdateWithJoinsBuilder also lets you specify arbitrary data to update. Let us first look at an example of using the builder. Assume that any objects and properties/fields referenced have been defined elsewhere.

```c#
SqlUpdateWithJoinsBuilder
  .IntoTable<UserAccounts>()
    .Joins
      .Add<UserAccounts, UserAccountsBackup>( (ua, uab) => ua.Id == uab.Id )    // type not specified: INNER JOIN
    .Where
      .Add<UserAccountsBackup>( uab => uab.LastModified > '1999-12-31T23:59:59Z' )
    .WithAdditionalColumns(
      new Dictionary<string, object?>() {
        { "PasswordHash", "foo1234$##@"
      }
    )
```

Let's now look at what we did in more detail:

The builder is static in nature. So calling the `IntoTable()` method instantiates the builder and kicks off the building sequence -- this also selects the table that we are updating the value into. 

Then we add the relevant Joins and Where clause conditions. We may add additional values to the table that are not a part of the business object using the `WithAdditionalColumns` function. This function accepts a `Dictionary<string, object?>`. Values provided to the `WithAdditionalColumns` are NOT re-interpreted, meaning what you have in your code will match exactly the values added to the corresponding `UPDATE` statement -- there will be no magic changing `Enums` to `strings` and so on, you will need to do them on your own.

Now that the builder is complete, there are 3 ways of using it:

**Method 1: Generate the T-SQL and use it elswhere.**
All the builders have the method `Build` that returns the T-SQL as a string. Simply call `Build` at the end of the sequence to get this T-SQL and use it in your own SqlCommand.

```c#
SqlCommand cmd = new(SqlUpdateWithJoinsBuilder....Build());
```

**Method 2: Pass the builder as is to the SqlTableContext's Select method**
The SqlTableContext's methods have been updated to accept the relevant fluid builder. You can use the one for Select:

```c#
await tableContext.UpdateAsync<UserAccounts>(SqlUpdateWithJoinsBuilder...());
```

**Method 3: Use the extension method to call query directly**
Along with the fluid builders, we have also added relevant extension methods that attach to them. These extensions take a SqlTableContext as their parameter. Use this method to execute the query:

```c#
await SqlUpdateWithJoinsBuilder...().Execute<UserAccounts>(tableContext);
```

Similar to the SqlTableContext class' `ExecuteNonQueryAsync`, the `ExecuteAsync` extension method returns an `int` -- number of rows affected in the corresponding SQL Server tables.
