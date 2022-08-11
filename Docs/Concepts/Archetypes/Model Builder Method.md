A Model Builder Method is a [[Method]] on an [[Archetype]] that produces a [[Model]] of the type expexted of the Archetype based on it's [[XBam]] configuration settings.

A Method is designated as a Model Builder Method if:
- It is Public
- It's name begins with "Make" or "make"
- It produces an IModel

You can also manually mark a Method as a Model Builder Method using the [[ModelBuilderMethodAttribute|ModelBuilderMethod]] attribute.