Enumerations are instances of a given base class with limited Key-Based options. Depending on how the [[Enumeration Base Class]] is set up, new [[Enumeration Value|values]] can be added to the Enumeration Type's collection lazily after startup as well.
# Features
- **Works Like C#'s Enum type:** The Enumeration provides an alternative to the basic C# `enum` functionality. Unlike the base `enum` types, Enumerations can be extended and hold additional data fields.
- **Modular Modibility:** The creator of an Enumeration Base Class can decide if modders are able to create more Enumeration Values of the given Enumeration Type, and can also decide if a modder can extend their Enumeration to add additional functionality.
- **Internal vs External Ids**: Enumeration Values have both External and Internal Ids. The external Id is a unique value of any type desired by the Enumeration's Creator and can be customized using the virtual [[UniqueIdCreationLogic]] function. The internal Id is an integer value that is unique for each Enumeration Value for an Enumeration Base Type, but is not guarenteed to stay considtent between runs of the [[Loader]].
- **Static Access**: Enumerations can be easily accessed from a static or global context using the Universe object, or the `All` property of the [[Enumeration Base Class]] itself.
# Accessing
Enumeration Values can be accessed in a few different ways other than their initial Declarations:
## All Property
All [[Enumeration Base Class]]es have a static `All` property which is defined as an IEnumerable containing all of the currently registered and loaded Enumerations that inherit from the Base Class.
## Universe Object
If you have access to the [[Universe]] the Enumeration Type belongs to, you can use the `Enumerations` property of the Universe to access data about the given Enumerations within that universe.
# Loading
## Loading at Startup
The [[Loader]] automatically initializes certain Enumeration Values at startup. For an Enumeration Value to be initialized at startup, it must be created on a public static property of a class that inherts from either [[Enumeration+1]], or a public class that includes the Attrubute: [[BuildAllDeclaredEnumValuesOnInitialLoadAttrubute]].
## Lazy Loading
Instances of Enumeration Values declared outside of the classes specified above are only loaded and registered when they are first called/constructed.
### Reflection Based Lazy Loading
Enumerations support certail Framerworks that use Reflection based Loading (Such as EFCore and Newtonsoft.Json) via a private/default Parameterless constructor by passing `null` into the Base Constructor. 

Enumerations with `null` passed to their Base Constructor will not be registered to the [[Enumeration Collection]] until the ExternalId value is set via Reflection.
# Serialization
By default, Enumerations are serialized into a single string contaning their key and the [[Universe]] identifier (if one exists) seperated by an `@` symbol.
## JSON
JSON Serialization is included by default to any [[Enumeration+1]], or collection of [[Enumeration+1]] based properties of any JSON Serializeable type.
## EFCore
***Note: EFCore serialization requires the library: [[Meep.Tech.Data.EfCore]].***

The [[SetUpEcsbamModels]] extension function automatically applies the [[EnumerationToKeyStringConverter]] to all fields that extend from [[Enumeration+1]], and creates a compound ValueConverter for collections of Enumerations based on the same basic single Enumeration ValueConverter. The collection converter results in a JSON array of key+Universe strings by default.
## How To Override Default Serialization
### Json
You can set a custom Json Conversion using the Newtonsoft JsonConverter Attribute
### EFCore
Any of the [[UseCustomEfCoreConverterAttribute]]s can be used.