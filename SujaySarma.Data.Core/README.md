SujaySarma.Data.Core
==================
This is a .NET library for working with data in a structured way. It provides a set of classes and interfaces to help you manage and manipulate data efficiently. Note that this library is a "core" library and is not intended for standalone use. It is designed to be used as a dependency in other projects that require data management capabilities.

API
----
This library provides the following public-surface API:

**Attributes**

Name | Description
-----|------------
IContainerAttribute | An interface for implementing an attribute that can be applied to classes to indicate that they are containers for data.
IContainerMemberAttribute | An interface for implementing an attribute that can be applied to members of a class to indicate that they are part of a data container.
ContainerAttribute | A concrete implementation (but still a "base" class) for an attribute that can be applied to classes to indicate that they are containers for data.
ContainerMemberAttribute | A concrete implementation (but still a "base" class) for an attribute that can be applied to members of a class to indicate that they are part of a data container.
DateTimeAuditMemberAttribute | Apply this attribute to a class/struct/record property/field to indicate that it is a date/time audit member. This will automatically set the value to the current date/time when the object is created or updated.
GuidPrimaryKeyMemberAttribute | Apply this attribute to a class/struct/record property/field to indicate that it is a GUID primary key member. This will automatically set the value to a new GUID when the object is created.

**Constants**

Name | Description
-----|------------
DataModificationInclusionBehaviour | An Enum that defines the inclusion behavior for data modifications. It can be used to specify whether to include or exclude certain modifications when processing data.
EnumSerializationBehaviour | An Enum that defines the serialization behavior for enums. It can be used to specify how enums should be serialized when converting to and from JSON or other formats.

**Transactions**

Name | Description
-----|------------
TransactionResult | A class that represents the result of a transaction. It contains information about whether the transaction was successful, any errors that occurred, and any data that was returned as part of the transaction.
TransactionBatchManager | A class that manages a batch of transactions. It provides methods for adding, committing, and rolling back transactions as needed.

---

This library contains other members marked "public" that are only intended for use by a library implementing a data access mechanism. These members are part of the internal implementation and should not be used directly by consumers of the library. They are subject to change without notice and may not be available in future versions of the library. Please see the code and documentation within SujaySarma.Data.* data access implementation libraries.*

---
Copyright (c) 2025 and beyond, Sujay V. Sarma. All rights reserved.

Licensed under the MIT License. See LICENSE file in the project root for full license information.

Library authored and maintained by: Sujay V. Sarma.

Issues/Feedback/Suggestions/Feature requests: Please create an issue on the GitHub repository.

---
