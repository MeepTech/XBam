---
aliases: XBamf, ECSBAM, ECSBAMf
---
XBAM (or XBAMF) is short for E.C.S.B.A.M.F, which stands for: "Entity Component System Based Archetype Model Factories". XBam is a framework for managing Flyweight combined with Factory patterns for use in things like game dev, modable development, and static data management.

XBAM allows one to create complex branching trees of both [[Archetype]]s and [[Model]]s in order to manage things such as Item Types for a game or Command Types for a Scripting Language, while being able to also control how extensible each Archetype or Model within your application is.

The main Components of XBAM, and the goals they accomplish are:
- **[[Concepts/Enumerations/Enumeration|Enumerations]]:** accomplish the goal of extensible immutable static data storage and are statically accessable from anywhere.
- **[[Archetype]]s**: accomplish two main goals, and are statically accessible from anywhere (ex: `Animal.Types.Get<Cat.Type>()`). 
   - Static Flyweight Data and Logic Storage
   - Extensible Model Builder Factories
- **[[Model]]s:** accomplish the goal of extensible mutable data based entities.
- **[[Component]]s**: accomplish the goal of extensible modular mutable data storage for Models