This is a collection of ORM libraries and the related unit test suites. Here is how the projects (or libraries) are organised:
- SujaySarma.Data.Core ("Core" for short) is the base library providing core functionality. All other ORM libraries in this solution (SujaySarma.Data.*) implement functionality and features using the API surface that Core provides.
- SujaySarma.Data.Azure.Tables enables interaction with Azure Storage's "Tables" API. It leverages the nuget library "Azure.Data.Tables" to interact with the Azure Storage Tables API. It aims to provide a near ADO/OleDB style interaction system for its upstream libraries and applications with paradigms such as connections and a query/statement API similar to ADO/OleDB.
- SujaySarma.Data.Files.TokenLimitedFiles enables interaction with token-delimited flatfile data (for example: comma-separated or "CSV" data). It supports custom tokens and is proud of its performance metrics (reading and parsing flatfiles with 84000 records with 24 fields each in under 200ms). This library breaks with ADO/OleDB semantics and provides a File I/O API like API surface.
- SujaySarma.Data.SqlServer wraps over System.Data.SqlClient (when targeting .NET 6.0) or Microsoft.Data.SqlClient (for .NET 8.0 upwards) to provide ORM capabilities with SQL Server backed data.
---
## Goals for each library

Here are some specific goals for each library in turn.

Library                                  | Goal
-----------------------------------------|---------------
SujaySarma.Data.Core                     | This is the lowest layer of our libraries. Performance, stability and API consistency are uncompromisingly important.
SujaySarma.Data.Files.TokenLimitedFiles  | There are many CSV parser/ORM libraries out there. What's our USP? PERFORMANCE. We implement many structures ourselves instead of using .NET provided ones -- for example, the TokenLimitedFileReader exclusively uses only primitive types (eg: string arrays instead of List<string>) for performance reasons. Missing performance targets is not acceptable.
SujaySarma.Data.SqlServer                | Parity with SQL Server features and capabilities -- but from an ORM perspective. What is possible to do with a T-SQL statement should be possible when using one of the "builders" provided. We provide two mechanisms for SQL interaction: SqlExecute is raw, a direct wrapped SQL Server interaction as found in legacy applications -- executes queries, statements and stored procedures. SqlContext wraps over SqlExecute and adds ORM-specific capabilities -- inputs and outputs are objects or object-collections.
---
## Coding guidelines

- Nullability is enabled at the project level for all project through the directive in the `.csproj` file. Parameters, Properties, Fields and return types that accept or process `NULL`s are explicitly marked with the `?`. This convention is maintained even for NULLable intrinsic types such as `string`.
	```c#
	string? Name;			// accepts NULL or is NULL-friendly.
	string Something;	// NULL is a problem!
	```

- All classes, enums, properties, fields and methods must have Xml comments. See comment guidelines below for further detail.
- All conditions must be paranthesis enclosed, even if .NET thinks it is not necessary. 
	```c#
	if (a == b || c != d) { ... }
	```
	must be written as:
	```c#
	if ((a == b) || (c != d)) { ... }
	```
- Conditional and loop statements must always enclose their sub-statements in curly braces.
	
	Wrong:
	```c#
	if (a == b)
		DoSomething();
	```
	Correct:
	```c#
	if (a == b)
	{
		DoSomething();	
	}
	```
- When returning a boolean value based on some other value, never do a return on the condition. Instead, use an explicit conditional statement to return an explicit `true` or `false` value.

	Do not do this:
	```c#
	return (a > b);
	```
	Instead, do this:
	```c#
	return ((a > b) ? true : false);
	```
- When new-ing something, always specify the type on the right-hand side of the statement.

	Do not do this:
	```c#
	List<string> items = new();
	```
	Instead, do this:
	```c#
	List<string> items = new List<string>();
	```
- Never use `var` to declare variables, even in loops. Always use the correct and actual type.

- When newer versions of C# provide newer ways to express a statement, do not prefer the new way just because it is new. Select the method that is: better expressive of intent, easier to maintain. This may mean we use different ways of expressing the same thing in different situations -- and that is okay.
- When writing a conditional statement, prefer assertive statements rather than negating statements.

	Instead of:
	```c#
	if (a != b)
	{
		DoThis();
	}
	else
	{
		DoSomethingElse();
	}
	```
	Prefer:
	```c#
	if (a == b)
	{
		DoSomethingElse();
	}
	else
	{
		DoThis();
	}
	```
	EXCEPTION TO RULE: Validation checks. Conditionals that examine validity of arguments/etc may be written in negating style.
- Other than because of validation, early exits from methods are fine. But balance such early exits against the actual work of the method. Perhaps a single conditional check and a unified method exit is better for readability and maintenance. 
---
## Unit Testing

All public API surface must be unit tested. There shall be 3 categories of unit tests:

1. Functionality, Test category name: "Functional".
2. Performance, Test category name: "Performance"
3. Negative -- exceptions, unexpected scenario, etc. - Test category name: "Negative"

### Unit testing guidelines:
- Unit tests are written with MSTest framework.
- Annotate test methods using "`[TestMethod]`" attribute.
- When methods are expected to throw exceptions, instead of the `[ExpectedException]` attribute annotation, use the `Assert.ThrowsExactly` assertion within the method.
- String returning API must be checked for:
	- Each expected component of string,
	- Position of component within the string,
	- The entire string.
- Unit tests must test validation logic within methods by providing invalid input:
	-  non-`NULL values
	- `NULL`
	- `default`
	- `default!`
---
## Code Comments
- All classes, enums, properties, fields and methods must have Xml comments.
- Public surface API comments must detail the purpose of the thing (method/property/etc).
- When something may be ambigious to the caller/user (eg: a parameter such as `Expression condition`), provide an example to illustrate the kind of values that would be successful.
- When a function returns `bool` or an `Enum` value, the `<returns>` comment should detail the circumstances when each value is selected for return. For example: `TRUE: when value is less than 40 characters. FALSE: otherwise.` -- unless that functionality is obvious.
- `remarks` and `example` tags should be avoided in comments as they are not visible in the `IntelliSense` tooltip for the item -- they will only be output to the documentation Xml file. True these are published to our GitHub repo, but that requires the user/developer to go there to look (it is not a ready in-workflow reference)!
- Prefer single line `//` comment style for comments that are only 1 or 2 lines long. For longer comments, use the block (`/*...*/`) style.
- Comments must always be positioned above the line(s) being talked about, never on the side.
	```c#
	// Write comment here for DoSomething.
	DoSomething();

	DoSomethingElse();		// Don't write comments here!
	```
	Only exception is a statement that's broken across lines and comments are written about each aspect of it:
	```c#
	if (
			(a == b)					// Alphabet is cooked!
			|| IsNothingAsItSeems()		// something is going on...
			&& WhatIf()					// what?
		)
	{
		//...
	}
	```
---
