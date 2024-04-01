# Sujay Sarma's Azure Tables Client SDK 
---

(Azure Storage Tables, Azure Development Storage, Azurite &amp; CosmosDB compatible)

Library                      | Current version
-----------------------------|------------------
SujaySarma.Data.Azure.Tables | Version 9.0.0


> Version 9.0.0 is a revamped version that takes on a dependency on SujaySarma.Data.Core.


## NuGet Package
https://www.nuget.org/packages/SujaySarma.Data.Azure.Tables

## About this library
Microsoft keeps changing the library names, calling conventions and provided APIs, too frequently for developers to have a stable and 
consistent environment to code against. This library is an attempt by me to provide both myself and other developers in the same boat 
overcome those limitations while writing maintainable and performant code.

While you can just use this library as a wrapper for Microsoft's `Azure.Data.Tables` library, the **true power** of this library is 
when you use its powerful ORM capabilities. Transform your .NET business objects (`class`es or `record`s) in the blink of an eye to 
`TableEntity`-ies and vice-versa. When you do so, you also get to leverage the powerful type-transformation and built-in Json serialization 
capabilities.

## Dependencies
This package only depends on whatever is the current Microsoft library of the day that provides the client APIs to interact with 
Azure Storage Tables and CosmosDB. Right now, that library is 

- [Azure.Data.Tables](https://github.com/Azure/azure-sdk-for-net/tree/Azure.Data.Tables_12.7.1/sdk/tables/Azure.Data.Tables)

## Dependability and Trustworthiness

- All the code is MIT licensed and open source. Examine every aspect of what the library does.
- I use this library in tons of my applications, tools, etc. You can expect bugs to be swiftly fixed.
- Enhancements happen all the time.
- If you need help or spot an issue, file a ticket on the Issues section.

## Usage
Please see [this guide](https://github.com/SujaySarma.Data/_docs/Azure.Tables/USAGE.md) for more details.

Happy coding!

