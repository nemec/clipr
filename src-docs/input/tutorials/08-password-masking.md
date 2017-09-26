Title: Password Masking
Description: Understanding how to accept sensitive input through the command line.
---

In some cases, it's useful to offer the ability for a user to "mask" their
password, or provide it separately from the command line in order to
prevent the password from appearing in console history or screenshots.
This feature can be enabled on `NamedArgument`s by annotating the property
with a `PromptValueIfMissingAttribute`. You may also, optionally, choose to
mask the password (so nothing shows on the screen when you type) or not.
If the password is provided in the command line arguments, the value will
used without prompting. If missing, the console will prompt for the missing
input. Additionally, a `SignalString` can be provided to explicitly indicate
that a value should be prompted. This resolves ambiguity in cases where
a positional argument/verb could be mistaken for a value.
Defaults to a dash `-`.

```csharp
public class Options
{
    [PromptIfValueMissing(MaskInput = true)]
    [NamedArgument('s', "secret")]
    public string SecretValue { get; set; }
}
```

```csharp
var opt = CliParser.Parse<Options>(new[] {"-s"});
```

```csharp
public class OptionsWithPositional
{
    [PromptIfValueMissing(MaskInput = true)]
    [NamedArgument('s', "secret")]
    public string SecretValue { get; set; }

	[PositionalArgument(0)]
	public string Name { get; set; }
}
```

```csharp
var opt = CliParser.Parse<OptionsWithPositional>(new[] {"-s - Nemec"});
```

Prints:

    Input a value for -s:

This feature only applies in a specific set of circumstances:
* Can only annotate a `NamedArgument` (not a `PositionalArgument`).
* The `ParseAction` must be `ParseAction.Store` (the default).