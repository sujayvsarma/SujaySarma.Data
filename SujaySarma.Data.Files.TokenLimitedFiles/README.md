SujaySarma.Data.Files.TokenLimitedFiles
=========================
This library makes it easy to work with token-limited flatfiles like: .CSV, .TSV, and .TXT files.

API
----
This library provides the following public-surface API:

**Attributes**

Name | Description
-----|------------
FileField | Attribute to indicate that a property or field in a class represents a column in a flatfile.

**How to use**
- Decorate your classes with the `Container` attribute to indicate that the class requires token-limited flatfile (de-)serialisation support.
- Decorate the properties or fields in the classes with the `FileField` attribute to indicate that they represent columns in the flatfile. 
- Use the `TokenLimitedFileReader` to read flatfiles into your classes; and the `TokenLimitedFileWriter` to write out your classes to flatfiles.
- You may use the `TokenLimitedFileContext` to perform "connection-less" read/write operations.

```
NOTE: The TokenLimitedFileWriter or the TokenLimitedFileContext class methods do NOT offer any means to edit a line "in place". If you make changes even to a single field, you will need to write out the entire file. Support for in-place edits are NOT planned and are NOT part of any proposed future feature-set.
```

---

This library contains other members marked "public" that are only intended for use by a library implementing a data access mechanism. These members are part of the internal implementation and should not be used directly by consumers of the library. They are subject to change without notice and may not be available in future versions of the library. Please see the code and documentation within SujaySarma.Data.* data access implementation libraries.*

---
Copyright (c) 2025 and beyond, Sujay V. Sarma. All rights reserved.

Licensed under the MIT License. See LICENSE file in the project root for full license information.

Library authored and maintained by: Sujay V. Sarma.

Issues/Feedback/Suggestions/Feature requests: Please create an issue on the GitHub repository.

---
