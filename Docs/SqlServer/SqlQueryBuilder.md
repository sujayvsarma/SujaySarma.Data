## SqlQueryBuilder

This document details the use of the SqlQueryBuilder statement builder introduced in version 8.8.0.

---
This statement builder helps build the SELECT query statement. It supports querying from a single table, or multiple tables with JOINs. The SqlQueryBuilder also lets you specify your own column selections for the SELECT clause, and more. Let us first look at a simple example of using the builder. Assume that any objects and properties/fields referenced have been defined elsewhere.

```c#
SqlQueryBuilder
  .From<UserAccounts>()
    .Joins
      .Add<UserAccounts, UserRoles>( (ua, ur) => ua.Id == ur.UserAccountId, TypesOfJoinsEnum.Inner )       // INNER JOIN
      .Add<UserAccounts, UserPreferences ( (ua, up) => ua.Id == ur.UserAccountId, TypesOfJoinsEnum.Left )  // LEFT JOIN
    .Where
      .Add<UserAccounts>( (ua) =>
          (ua.UserName == loginModel.UserName)
          && (ua.Password == loginModel.Password)
          && (ua.AccountStatus == AccountStatus.Active)
      )
    .Select<UserAccounts>(ua => new { ua.Id, ua.PasswordLastChanged })
    .Select<UserRoles>(ur => new { ur.UserRoleName })
    .Select<UserPreferences>()
```

Let's now look at what we did in more detail:

The builder is static in nature. So calling the `From<T>()` method instantiates the builder and kicks off the building sequence. The object you select to use in the call to `From` is very important as this is the regarded as the primary table through the builder. You should use the same object that corresponds to the table name in `SELECT FROM **table**` that you would have in the equivalent T-SQL query.

`Joins` and `Where` are 'collection' properties of the builder. Use their respective `Add` functions to add the respective `JOIN` or `WHERE` clause/condition. You may add any number and combination of them as you need to. 

Finally, we have a set of calls to the `Select` function. The first one selects columns from the `UserAccounts` object, the second from `UserRoles` and the last one from `UserPreferences`. Note how the first two select only specific columns while the last one does not specify any property/fields -- this one selects all the properties/fields (and hence the corresponding columns) for that table.

Our Linq Lambda Expression Parser is smart enough to respect table serialization settings you have built into your business classes. For example, notice how in the `where` clause, we have this:

```c#
(ua.AccountStatus == AccountStatus.Active)
```

If we assume that the `UserAccounts` object defines the `AccountStatus` property as an `Enum` of type `AccountStatus`, and let's say you have this property decoration:

```c#
[TableColumn("AccountStatus", EnumSerializationBehaviour = EnumSerializationBehaviour.AsString)]
```

While parsing the expression, our translator will automatically create the clause as `AccountStatus = 'Active'` and not as `AccountStatus = 0`. 

Similarly, you would have noticed that our statement had this:

```c#
(ua.UserName == loginModel.UserName)
```

Here, `ua` is no doubt our `UserAccounts` business object. However, `loginModel` is a function scoped object, possibly a Model from a HTTP POST request. Our parser will automatically translate such references into runtime constants. Similarly, you may also use direct constants or implied constants with clauses such as ---

```c#
(ua.LastLogin < DateTime.UtcNow.AddDays(-15))
or
(ua.EmailAddress != string.Empty)
```

This magic is available throughout the builder system.

Now that the builder is complete, there are 3 ways of using it:

**Method 1: Generate the T-SQL and use it elswhere.**
All the builders have the method `Build` that returns the T-SQL as a string. Simply call `Build` at the end of the sequence to get this T-SQL and use it in your own SqlCommand.

```c#
SqlCommand cmd = new(SqlQueryBuilder....Build());
```

**Method 2: Pass the builder as is to the SqlTableContext's Select method**
The SqlTableContext's methods have been updated to accept the relevant fluid builder. You can use the one for Select:

```c#
tableContext.Select<UserAccounts>(SqlQueryBuilder...Select());
```

**Method 3: Use the extension method to call query directly**
Along with the fluid builders, we have also added relevant extension methods that attach to them. These extensions take a SqlTableContext as their parameter. Use this method to execute the query:

```c#
IEnumerable<UserAccounts> user = SqlQueryBuilder...Select().Query<UserAccounts>(tableContext);
```

For the SqlQueryBuilder, we have provided two extension methods -- `Query` returning an `IEnumerable<T>` and a `QueryOneOrNull` that returns a `T?`.

