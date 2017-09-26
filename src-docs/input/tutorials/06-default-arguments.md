Title: Default Argument Values
Description: How to set default values on an argument.
---

Set default values for a property in the config object's constructor. If a
value is provided on the command line, it will overwrite the default value.

```csharp
public class Options
{
    public Options()
    {
        OutputFile = "out.txt";
    }

    [PositionalArgument(0, MetaVar = "OUT",
        Description = "Output file.")]
    public string OutputFile { get; set; }
}
```