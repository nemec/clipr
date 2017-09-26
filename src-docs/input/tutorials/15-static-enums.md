Title: Static Enumerations
Description: How to extend the concept of an enum to more complex uses.
---

Sometimes there is a need to add logic to the Enum type. Since .Net enums are
purely integers, the only alternative is to implement the so-called "Typesafe
Enum" pattern by creating a class with fields for each enum value, possibly
with subclasses defining value-specific behavior.

Since clipr cannot rely on a specific typesafe enum implementation, it defines
a common set of criteria for identifying and parsing them.

1. Enum values must be `public`, `static`, and `readonly` fields of a class.
2. Fields must be the same type as the underlying enum or a subclass.
3. Static enums may only be parsed by name: the name of the field. Parsing
  is case insensitive (using the Invariant culture).
4. The class must be tagged with the `clipr.StaticEnumerationAttribute`
  attribute. If the attribute cannot be added to the class directly (such as
  third party types), but the class follows (1) and (2), the attribute may also
  be applied to the option property.

Sample:

```csharp
[StaticEnumeration]
internal abstract class SomeEnum
{
    public static readonly SomeEnum First = new EnumSubclass();

    public abstract void DoSomeWork();

    public class EnumSubclass : SomeEnum
    {
        public override void DoSomeWork() { }
    }
}

internal class StaticEnumerationOptions
{
    [NamedArgument('e')]
    public SomeEnum Value { get; set; } 
}

internal class StaticEnumerationExplicitOptions
{
    [NamedArgument('e')]
    [StaticEnumeration]  // Allowed in case attr is not defined on SomeEnum
    public SomeEnum Value { get; set; } 
}

public static void Main()
{
  var obj = CliParser.Parse<StaticEnumerationOptions>(
      "-e first".Split());
  Assert.AreSame(SomeEnum.First, obj.Value);
}
```