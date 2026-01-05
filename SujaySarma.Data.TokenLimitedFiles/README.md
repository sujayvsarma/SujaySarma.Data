# SujaySarma.Data.TokenLimitedFiles

This library provides mechanisms to read from and write data to token-delimited files -- such as comma, semi-colon, space, tab, etc separated flat text files. These files may have file extensions of `.csv` or `.txt`. In addition to disk files, the library also supports reading from and writing to streams (eg: HTTP file download streams, uploaded files from web-based forms, etc).

**Core reflection, attribute discovery, validation, data conversion, and ORM services for the SujaySarma.Data.* library ecosystem.**

[![NuGet](https://img.shields.io/nuget/v/SujaySarma.Data.TokenLimitedFiles.svg)](https://www.nuget.org/packages/SujaySarma.Data.TokenLimitedFiles)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

---

## Installation

```
$ dotnet add package SujaySarma.Data.TokenLimitedFiles
```


**NuGet Package:** [SujaySarma.Data.TokenLimitedFiles](https://www.nuget.org/packages/SujaySarma.Data.TokenLimitedFiles)

**Current Version:** `10.0.0.0`

**Target Frameworks:** `.NET 6.0`, `.NET 8.0`, `.NET 10.0`

---


## Performance

This library is highly performance optimised. Benchmark: Parse and correctly load a flat text file with 20,000 records in less than 1 second. This file contains a mix of good data, erroneous data, quoted, unquoted, badly quoted, wrongly quoted, etc. that interprets the RFC 4180 specification in both letter and spirit. Typically, this library surpasses this metric by finishing in less than 300ms.

## Index Convention

> **IMPORTANT:** All indexes and positions provided to any attribute, property/field or method in this library are **ONE (1) based**. This library expects all sequences for token-delimited files to be: 1, 2, 3.... This is a significant departure from common popular programming paradigms where indexes are ZERO (0) based (0, 1, 2...). Please be aware of this while using this library!

---

## Public API

### Attributes

The library provides attributes to decorate your business entities for ORM-style interaction with token-delimited files.

#### Container-Level Attributes

**`Flatfile`** - Marks a class, struct or record as being persisted to a flatfile.

**Constructors:**
- `Flatfile()` - Creates a flatfile attribute with index-based field references (use with `FlatfileField`).
- `Flatfile(uint headerRowIndex)` - Creates a flatfile attribute with name-based field references (use with `FlatfileNamedField`). The `headerRowIndex` parameter specifies the 1-based line number containing the header row.

**Properties:**
- `FieldReferenceMode` - Gets how fields are referenced (`Indexes` or `Names`).
- `HeaderLineNumber` - Gets the 1-based line number of the header row (0 if index-based).

#### Member-Level Attributes

**`FlatfileField`** - Marks a property or field as a field in a flatfile using position-based indexing.

**Constructor:**
- Not directly instantiable (use `FlatfileNamedField` for named fields).

**Properties:**
- `Position` - Gets the 1-based position of the field in the record.

**`FlatfileNamedField`** - Marks a property or field as a named field in a flatfile (requires header row).

**Constructor:**
- `FlatfileNamedField(string name, uint position)` - Creates a named field attribute.
  - `name` - The name of the field in the header row.
  - `position` - The 1-based position of the field.

---

### Core Classes

#### `TokenLimitedFileReader`

Reads token-delimited records from flatfiles or streams synchronously, compliant with RFC 4180.

**Constructors:**
- `TokenLimitedFileReader(Stream stream, char delimiter = ',', Encoding? encoding = null, bool leaveStreamOpen = false)`
- `TokenLimitedFileReader(string path, char delimiter = ',', Encoding? encoding = null)`

**Methods:**
- `ReaderExitReason TryReadRecord(out string[] record)` - Reads the next complete record/row. Returns the reason for exiting and outputs the record fields.
- `ReaderExitReason TryReadField(out string? field)` - Reads the next field from the current position. Returns the reason for exiting and outputs the field value.

**Properties:**
- `bool CanRead` - Gets whether the reader can still read from the stream.

**Implements:** `IDisposable`

---

#### `TokenLimitedFileWriter`

Writes token-delimited records to flatfiles or streams synchronously, compliant with RFC 4180.

**Constructors:**
- `TokenLimitedFileWriter(Stream stream, char delimiter = ',', string recordDelimiter = "\r\n", Encoding? encoding = null, bool leaveStreamOpen = false, bool writeEmptyRows = false)`
- `TokenLimitedFileWriter(string path, char delimiter = ',', string recordDelimiter = "\r\n", Encoding? encoding = null, FileMode mode = FileMode.CreateNew, bool writeEmptyRows = false)`

**Methods:**
- `bool TryWriteRecord(IEnumerable<string?> record)` - Writes a record from string values.
- `bool TryWriteRecord(IEnumerable<object?> record)` - Writes a record from object values (automatically serialized).
- `bool TryWriteField<T>(T? field)` - Writes a single field (automatically quoted if needed).

**Properties:**
- `bool CanWrite` - Gets whether the writer can still write to the stream.

**Implements:** `IDisposable`

---

#### `Serialiser`

Serializes business entities into string arrays for writing, and deserializes string arrays from files/streams back into business entities.

**Static Factory Methods:**
- `static Serialiser For<T>()` - Creates a serializer for type `T`.
- `static Serialiser For(Type type)` - Creates a serializer for the specified type.

**Methods:**
- `object Deserialise(string[] values)` - Deserializes data into a business entity instance.
- `string[] SerialiseEntity(object? entity)` - Serializes an entity into a string array ready for writing.
- `string[] SerialiseHeaders()` - Returns the header row as a string array (empty for index-based files).
- `static string SerialiseValue<T>(T? data, char fieldDelimiter = ',', string recordDelimiter = "\r\n")` - Serializes a single value into a properly quoted/escaped string.
- `Serialiser WithFieldDelimiter(char delimiter = ',')` - Sets the field delimiter (returns self for chaining).
- `Serialiser WithRecordDelimiter(string delimiter = "\r\n")` - Sets the record delimiter (returns self for chaining).

---

#### `TokenLimitedFileContext`

A unified context that reads and writes entities to token-delimited files/streams without needing to instantiate and manage multiple objects (Serialiser, Reader, Writer, etc).

**Static Factory Methods:**
- `static TokenLimitedFileContext For<T>()` - Creates a context for type `T`.
- `static TokenLimitedFileContext For(Type type)` - Creates a context for the specified type.

**Reading Operations:**
- `object? Read()` - Reads and deserializes the next record, returning the entity instance (or null if no more records).
- `void ReadPreamble()` - Reads the preamble/header row from the current position.

**Writing Operations:**
- `void Write(object? instance)` - Serializes and writes an entity record.
- `void WritePreamble()` - Writes the preamble/header row.

**Adding Reader:**
- `TokenLimitedFileContext AddReader(string path, char delimiter = ',', Encoding? encoding = null)`
- `TokenLimitedFileContext AddReader(Stream stream, char delimiter = ',', Encoding? encoding = null, bool leaveStreamOpen = false)`
- `TokenLimitedFileContext AddReader(TokenLimitedFileReader reader)`

**Adding Writer:**
- `TokenLimitedFileContext AddWriter(string path, char delimiter = ',', string recordDelimiter = "\r\n", Encoding? encoding = null, FileMode mode = FileMode.CreateNew, bool writeEmptyRows = false)`
- `TokenLimitedFileContext AddWriter(Stream stream, char delimiter = ',', string recordDelimiter = "\r\n", Encoding? encoding = null, bool leaveStreamOpen = false, bool writeEmptyRows = false)`
- `TokenLimitedFileContext AddWriter(TokenLimitedFileWriter writer)`

**Properties:**
- `bool CanRead` - Gets whether the reader can be read from.
- `bool CanWrite` - Gets whether the writer can be written to.

**Implements:** `IDisposable`

---

## Usage Patterns

### Low-Level API
Use `TokenLimitedFileReader` and `TokenLimitedFileWriter` directly for fine-grained control with raw `string[]` data.

### ORM-Style API
1. Decorate your entities with `[Flatfile]` and `[FlatfileField]` or `[FlatfileNamedField]` attributes.
2. Use `Serialiser` to convert between entities and `string[]` arrays.
3. Use `TokenLimitedFileReader`/`TokenLimitedFileWriter` to handle file I/O.

### Unified Context API
Use `TokenLimitedFileContext` for the simplest experience - it manages the reader, writer, and serializer automatically:

```c#
using var context = TokenLimitedFileContext.For<MyEntity>()
						.AddReader("data.csv") .AddWriter("output.csv");

context.ReadPreamble(); 
while (context.CanRead) 
{ 
	var entity = (MyEntity?)context.Read(); 
	if (entity != null) 
	{ 
		// Process entity 
		context.Write(entity); 
	} 
}
```

---

Copyright (c) 2025 and beyond, Sujay V. Sarma. All rights reserved.  
Licensed under the MIT License. See LICENSE file in the project root for full license information.  
Library authored and maintained by: Sujay V. Sarma.  
Issues/Feedback/Suggestions/Feature requests: Please create an issue on the GitHub repository.