A Trait of an [[Archetype]] used to build a [[Child Archetype]] for each member of an [[Concepts/Enumerations/Enumeration]].

To use this Trait, implement the Interface: [[Archetype+2.IBuildOneForEach+1]], on the desired Archetype, providing the desired Enumeration type.

**NOTE:** ONLY Enumeration values that are loaded by the [[Loader]] will have associated Archetypes created at the moment. Lazy loaded Enumeration values will NOT have associated Archetypes created when the Loader is run.

TODO: Add an overrideable bool to this Interface to allow lazy initialization of an Archetype for an Enumeration when a new Lazy Enumeration Value is registered.