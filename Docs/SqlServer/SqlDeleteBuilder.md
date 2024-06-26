## SqlDeleteBuilder

This document details the use of the SqlDeleteBuilder statement builder introduced in version 8.8.0.

---
This statement builder helps build the DELETE statement. It supports deleting data using a JOIN (DELETE FROM... JOIN...). Let us first look at an example of using the builder. Assume that any objects and properties/fields referenced have been defined elsewhere.

```c#
SqlDeleteBuilder<UserAccounts>
  .Begin()
/*
    .Joins
      .Add<A, B>(....)
*/
  .Where<UserAccounts>( (ua) => ua.AccountStatus == AccountStatus.Deleted )
```

Let's now look at what we did in more detail:

The builder is static in nature. So calling the `Begin()` method instantiates the builder and kicks off the building sequence -- the table is the one passed into the `T` parameter of `SqlDeleteBuilder`. Just like the `SqlQueryBuilder`, we can specify any number of Joins and Where clauses. 

Now that the builder is complete, there are 3 ways of using it:

**Method 1: Generate the T-SQL and use it elswhere.**
All the builders have the method `Build` that returns the T-SQL as a string. Simply call `Build` at the end of the sequence to get this T-SQL and use it in your own SqlCommand.

```c#
SqlCommand cmd = new(SqlDeleteBuilder....Build());
```

**Method 2: Pass the builder as is to the SqlTableContext's Select method**
The SqlTableContext's methods have been updated to accept the relevant fluid builder. You can use the one for Select:

```c#
await tableContext.DeleteAsync<UserAccounts>(SqlDeleteBuilder...());
```

**Method 3: Use the extension method to call query directly**
Along with the fluid builders, we have also added relevant extension methods that attach to them. These extensions take a SqlTableContext as their parameter. Use this method to execute the query:

```c#
await SqlDeleteBuilder...().Execute<UserAccounts>(tableContext);
```

Similar to the SqlTableContext class' `ExecuteNonQueryAsync`, the `ExecuteAsync` extension method returns an `int` -- number of rows affected in the corresponding SQL Server tables.
