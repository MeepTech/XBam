Factories are classes that inherit from [[IFactory]]; Usually [[Archetype]]s. By default, The [[XBam]] [[Loader]] does not produce any Factories that are not also Archetypes. 
The purpose of a [[Factory]] is to produce a [[Model]] using a [[Builder]] or a set of [[Parameter]]s.

Factories in XBam use [[Make Function]]s to produce new Models using either a Builder or collection of Parameters.

By default, [[Component Builder Factory|Cponent Producing Factories]] and [[Model Builder Factory|Model Builder Factories]] expose access to default Make functions, while 
Archetypes(Which produce models) do not. 
