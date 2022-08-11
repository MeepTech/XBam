An [[Archetype]] Identity is a Special Type of [[Concepts/Enumerations/Enumeration|Enumeration]] where each [[Enumeration Value]] is used to identify and index a different Archetype. 

Every Archetype has it's own unique Archetype Identity Value.

It is suggested that you place the Archetype Identity inside of the Archetype class it represents in a static context, like so:
```
public class Item.Type : Archetype<Item, Item.Type> {

	public static Identity Id {
		get;
	}

	...
}
```
As in the example above, it is also suggested you use your Archetype specific Identity class ([[Archetype+2.Identity]]), or create your own Identity sub-class for the current [[Archetype Tree]].