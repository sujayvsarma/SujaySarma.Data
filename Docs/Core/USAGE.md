# Usage instructions

This library provides the base attribute classes you can use to create your own decoration-attributes for the ORM layer. 

Base attribute types:

Attribute name | Purpose
---------------|------------------------
IContainerAttribute, ContainerAttribute | Define a data container, eg: a table in a database.
IContainerMemberAttribute, ContainerMemberAttribute | Define a member in a data container, eg: a column in a database table.

Then there are two specialised attribute classes:

- DateTimeAuditMemberAttribute
Derives from ContainerMemberAttribute, and adds a default value function that returns the current UTC DateTime. This is useful for data members such as "LastModified" timestamps.

- GuidPrimaryKeyMemberAttribute
Derives from ContainerMemberAttribute, and adds a default value function that returns a new Guid value. This is useful for "Id" type columns.

There are a few other constants (Enums), classes and utilities. Look through the codebase of the other libraries in this repo to know more about how, why and where to make use of them.
