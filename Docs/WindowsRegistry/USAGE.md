# Usage instructions

This library provides 3 attributes to be used as follows:

**RegistryKeyNameAttribute**

This is a "container" attribute and configures the registry key the information contained in the .NET class, structure or record is stored into or retrieved from.

**RegistryValueNameAttribute**

Inherits from the "container member" attribute and configures the registry value (value under a Key) contained a .NET property or field.

The final attribute **RegistryValueNamePairAttribute** 

Is a special (container member) attribute that lets you transfer values between structures like a KeyValuePair<,> or a Dictionary<,> and the Windows Registry, where you don't want to, or cannot, define value names for each contained element. While performing ORM, the library will always process the connected property or field LAST (after all other properties and fields have been processed). The behaviour is thus:

- on READ: After all other values have been read as per the `RegistryValueNameAttribute`s, if a property or field with the `RegistryValueNamePairAttribute` is present, any remaining values under the `RegistryKeyNameAttribute` key being processed is populated into the property/field with the `RegistryValueNamePairAttribute` decoration.
  
- on WRITE: The contents of the property/field decorated with the `RegistryValueNamePairAttribute` is written out to the Registry directly. The Key of the KeyValuePair or Dictionary becomes the value's name in the registry.

*For this reason, there can only be ONE property or field in any class decoratedw with a `RegistryValueNamePairAttribute`. If you have any more, the library will scream at you at runtime.*

## Operations:
Use the functions within the `WindowsRegistryContext` class to trigger the (de-)serialisation operations. Unlike other libraries, this one does not provide any Async methods because Windows Registry does not support them. Though it would be possible to create contrived async methods using Task.Run() etc, such implementations would negatively impact performance and reliability!
