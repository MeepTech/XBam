Archetypes are the heart and soul of [[XBam]]. They are [[Flyweight Data]] Stores and [[Factory|Factories]] that produce [[Model]]s. An Archetype's purpose is to hold Static/Immutable data and logic, and to produce Models via [[Builder]]s using a provided collection of [[Parameter]]s.

By default, Archetypes act like Singletons; Only one archetype of each Class Type is allowed to exist at any given time.

For an aternative to Archetypes that don't need to produce Models, See [[Concepts/Enumerations/Enumeration|Enumeration]]s
# Features and Rules
- All Archetypes Must Inherit from [[Archetype+2]].
-  Each archetype has a unique [[Archetype Identity]].
- Each Archetype acts like a Factory that can produce a Model based on the [[Base Model Type]].
- Archetypes can be extended into [[Archetype Tree]]s via inheritance.
	- The Child Archetypes have the potential to produce different types of Models that inherit from the [[Base Model Type]]. 
- Archetypes can be easily accessed from anywhere via their Class, making them a good store of accessable Flyweight Data for the Models they produce.
- The coder of each Archetype can determine if those Archetypes can be extended, and how moddable and extensible those Child Archetypes can be.
- All Archetypes are owned by, and stored in, a specific Root [[Archetype Collection]] based on their [[Base Archetype]].
	- Archetypes can be added to other Archetype Collections as well
- By default, Archetypes can and should not be modified after the [[Loader]] is sealed.
	- This behavior can be overriden [[#Allowing Runtime Loading and Unloading|via a virtual boolean]], and Archetypes that extend that Archetype can be loaded and un-loaded from allowed sourced during runtime.
## Traits
There are a number of built in special 'Traits' that you can give to archetypes
- [[Splayable]]: Used to build an Archetype for each member of an [[Concepts/Enumerations/Enumeration|Enumeration]].
- [[Expose Builders]]: There are 3 Extension classes to [[Archetype+2]], Inheriting from these classes instead of [[Archetype+2]] exposes the desired built in [[Model Builder Method]]s which are Protected by default.
- [[#Branch Attribute|Branching]]: This trait makes this or any [[Child Archetype]] with a Class Declaration that is inside of a Model Class Declaration (as a Nested Type) produce that type of Model.
# The Archetype Build Proccess
By default, Archetypes are built by the [[Loader]] when it is run to initialize a [[Universe]].
This should usually be done at Start-up.
## Default Constructors
Archetypes need to have one of the following Constructors defined in order to be built at start up, otherwise an error will be thrown: 
```
protected Ctor() : base(Id, [Collection = null]) {...}
// or
Ctor() : base(Id, [Collection = null]) {...}
// or
protected Ctor(Identity Id, [Collection = null]) : base(Id ?? BaseId, [Collection]) {...}
```
In the final example, the Id will be passed in as null; so BaseId will be used as the Identity when the Archetype is auto-built for the first time.

## Ignored Archetype System Types
Abstract Classes, Classes that use The [[DoNotBuildInInitialLoadAttribute|DoNotBuildInInitialLoad Attribute]], and Classes that use or inhert the [[DoNotBuildThisOrChildrenInInitialLoadAttribute|DoNotBuildThisOrChildrenInInitialLoad Attribute]] are not built at runtime. 
**All other Archetype Classes That Match the Loader Settings are Built When the Loader is Run.**
# Creating Trees of Achetypes
Only [[Base Model Type]]s that extend [[Model+2]] can have branching trees of Archetypes. Alternately, [[Model+1]] types only have a single Root Archetype which acts as a [[Simple Builder Factory]] which can be customized to produce multiple Model Types if one desires.

[[Base Archetype]]s and their [[Child Archetype|children]] are organized into [[Archetype Tree]]s. The Base Archetype is the root of this tree, and all other Archetypes in this Tree/Family inherit from that Base Archetype. 
The Base Archetype has a [[Base Model Type]], which is the most basic type of [[Model]] that this Archetype can produce as a Factory. All Models produced by any Archetype in this family will inherit from this Base Model Type.
All classes that inhert from the Base Archetype are built during startup by the [[Loader]] and are included in the [[Archetype Collection]] for this Base Archetype.
By default, all Archetypes that inherit from the Base Archetype produce the same Base Model Type when used as a Factory.
## Changing The Model Type a Child Archetype Produces As A Factory.
[[Archetype Tree Branch|Branches]] of this Archetype Tree produce different types of Models than the Base Model Type.
### Branch Attribute
The Attrubute; [[BranchAttribute]], can be added to a Child Archetype's class if that Child Archetype is a nested class within another class that inherits from the [[Base Archetype]]'s [[Base Model Type]]. This will tell the [[Loader]] that the Archetype with the attribute should produce [[Model]]s of the outer type that extends the Base Model Type.

```
public class Item : Model<Item, Item.Type> {
	public class Type : Archetype<Item, Item.Type> {
		...
	}
}

public class Weapon : Item {
	[Branch]
	public class Type : Item.Type {
		...
	}
}
```
In this example, Weapon.Type will produce Weapon Models by default instead of Item Models.

### Overriding The Model Constructor
The model constructor an Archetype uses can either be overriden in the default [[Builder]] the Archetype uses, or in the [[Archetype]] itself.
#### In-Archetype Method
The virtual property [[ModelConstructor]] can be overriden in a Child Archetype class.

```
public class Weapon : Item {


	public class Type : Item.Type {
	
      	public override Func<IBuilder<Item>, Item> ModelConstructor
      		=> builder => new Weapon();
		
		...
	}
}
```
#### Builder Method 
# Accessing Archetypes
Archetypes can be accessed in a variety of ways, offering differing levels of 'security' and different guarentees.
## Via Identity
Archetypes are accessible via their registered [[Archetype Identity]]:
```
//if 
class Apple : Archetype<...> {
	public static Identity Id {get;}
		= new Identity("Apple");
		
	Apple(): base(Id) {}
}

// Then
// Accessible via it's registered Identity:
Apple apple = Apple.Id.Archetype as Apple
```
## Via Universe
Archetypes can be accessed via the [[Universe]] they are registered to:
```
// If:
Universe universe;
new Loader().Load(universe);

/// Then
universe.Archetypes.All["..."]
universe.Archetypes.Get(...)
```
## Static Access Via Default Universe
Archetypes in the [[Default Universe]] can be accessed via the static [[Code/Archetypes+1|Archetypes<>]] Class
```
Archetypes<TArchetype>._
// or
Archetypes<TArchetype>.Instance

// or via the Default Universe:
Archetypes.DefaultUniverse.Archetypes...
```
## Via Auto-Generated Collection
Each [[Base Archetype]] of an Archetype Tree has an [[Archetype Collection]] generated automatically.

This Archetype Collection can be accessed statically via a `Types` property on it's [[Base Model Type]].
```
// If:
class Item.Types : Archetype<Item, Item.Type> {...}

// Then
// Accessable via collection at Item.Types:
Item.Type itemType = Item.Types.Get(...)
```
# Moddability of Archetypes
The coder of an Archetype can decide if an archetype can be extended, and exactly how.
## Modability Via Inheritance
### Child Archetypes
To allow the creation of Child Archetypes by others, it's suggested you define the constructor of your Archetype like this:
```
protected ArchetypeName(Identity Id) : base (Id ?? BaseArchetypeId) {}
```
- `protected` Allows Child Archetype Types to pass their Id into the base Constructor of your Archetype Type.
- ` base (Id ?? BaseArchetypeId)` Non-Abstract Archetypes are built at runtime by default. This format passes in 

### Moddable Property Snippets
## Allowing Runtime Loading and Unloading
## Allowing Adding of Components