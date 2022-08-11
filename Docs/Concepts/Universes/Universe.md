A Universe organizes and contains references to all [[Concepts/Enumerations/Enumeration|Enumerations]], [[Archetype]]s, [[Model]]s, and [[Component]]s registered by a [[Loader]] in [[XBam]].

It is not usually reccomended, but a runtime can host multiple Universes at once.

# Access
## Default Universe
The first Universe created is automatically set as the Default Universe of the static [[Archetypes]] class, and is set as the Default Universe of the runtime. A runtime can have a seperate default universe for [[Archetype]]s, [[Model]]s, and [[Component]]s using the static [[Archetypes]], [[Models]], and [[Components]] classes if the user wants. By default, [[Models]] and [[Components]] use the same default universe assinged to [[Archetypes]].`DefaultUniverse`.
# Registering An Enum Or Archetype To An Alternate Universe
By default, if you pass `null` into the `Universe` property of an [[Archetype]] of [[Concepts/Enumerations/Enumeration|Enumeration]]'s constructor it will register that type to the Default Universe. Passing in an alternate universe object to these constructors will cause these types to be registered to the alternate universe instead.