#### where T : SELF
This syntax is used to show that the generic value of the base class of a class should be the class itself.

For example: `Enumeration<T> where T : SELF`
```
class EnumType : Enumeration<EnumType>
```
Or;  `Archetype<M, A> where A : SELF`
```
class ArchetypeType : Archetype<ModelType, ArchetypeType>
```