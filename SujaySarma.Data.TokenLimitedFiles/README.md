SujaySarma.Data.Files.TokenLimitedFiles
=========================
This library provides mechanisms to read from and write data to token delimited files -- such as comma, semi-colon, space, tab, etc seperated flat text files. These files may have file extensions of .csv or .txt. In addition to disk files, the library also supports reading from and writing to streams (eg: Http file download streams, Uploaded files from web-based forms, etc).

This library is highly performance optimised. My benchmark: Parse and correctly load a flat text file with 20,000 records in less than 1 second. This file contains a mix of good data, erroroneous data, quoted, unquoted, badly quoted, wrongly quoted, etc. that interprets the RFC specification in both letter and spirit. Typically, this library surpasses this metric by finishing in less than 300ms.

## API
The following public-surface API is exposed by this library:

> *IMPORTANT:*: All indexes and positions provided to any attribute, property/field or method in this library are ONE (1) based. This library expects all sequences for token-delimited files to be: 1, 2, 3.... This is a significant departure from common popular programming paradigms where indexes are ZERO (0) based (0, 1, 2...). Please be aware of this while using this library!

### Attributes
SujaySarma.Data.TokenLimitedFiles provides fully implemented specialist attributes.

#### Object/Entity or Container level
(These attributes are used at class, struct or record level)

Attribute | Purpose
----------|-------------
FlatFile  | Indicates that data from this entity will be serialised into a flatfile. The property `FieldReferenceMode` is important as the ORM system will consider only the type you have set!


#### Object/Entity member or container member level
(These attributes are used at property or field level)

Attribute | Purpose
----------|-------------
FlatFileField | Provides the one-based (1, 2, 3...) index of the field that serialises this property/field's data.
FlatFileNamedField | Provides the name of the field that serialises this property/field's data.


## Entry points
This library provides three primary entrypoints:

1. `TokenLimitedFileReader` - This class provides synchronous functionality with 1:1 parity, to read from the token delimited flatfiles and streams. 
2. `TokenLimitedFileWriter` - This class provides synchornous functionality with 1:1 parity, to write data to token-delimited flatfiles and streams.
3. Both above classes return or make use of `string[]`s for header and data information. To get an object's data or metadata (headers) into the expected `string[]`, use the `Serialiser` class's methods.

The `TokenLimitedFileContext` class reads and writes entities to token-delimited files/streams without needing to instantiate and manage multiple objects (Serialiser, Reader, Writer, etc). 


---
Copyright (c) 2025 and beyond, Sujay V. Sarma. All rights reserved.
Licensed under the MIT License. See LICENSE file in the project root for full license information.
Library authored and maintained by: Sujay V. Sarma.
Issues/Feedback/Suggestions/Feature requests: Please create an issue on the GitHub repository.

---
