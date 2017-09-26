Title: Variable Argument Counts
Description: Dealing with arguments that can contain multiple values.
---

In addition to choosing an action for each argument, you can specify
*how many* values each argument can consume. The first part is the NumArgs
property, which gives the value limit for the argument. The second part,
constraints, comes in four flavors: Exact, AtLeast, AtMost, and Optional. Any
named argument may take exactly the number of values specified or use that as
the minimum or maximum number consumed. The final constraint, Optional, can be
set on a reference type or Nullable property and it indicates that a value
for the argument can be provided or left off. If the argument is given without
a value, the value will be copied from the `Const` property on the argument.
For more complex types, `Const` may be set to a string and the value will be
converted to the destination type in the same way other argument values are 
converted.

```csharp
[NamedArgument('s', "server", Constraint = NumArgsConstraint.Optional, Const = 1234)]
public int? Server { get; set; }
```

You may also use Optional arguments to define boolean flags that can easily be scripted (explicitly set to true or false).

```csharp
private class Options
{
	[NamedArgument('f', "flag",
	   Constraint = NumArgsConstraint.Optional,
	   Action = ParseAction.Store,
	   Const = true)]
	public bool Flag { get; set; }

}
```

And call it with any of the following:

    -f
    -f true
    -f false

Since positional arguments are not
delimited in any discernable way only the *last* positional argument,
by Index, may use the constraints AtLeast or AtMost. All previous positional
arguments must consume an exact number of values.