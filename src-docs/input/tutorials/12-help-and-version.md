Title: Help and Version Information
---

By default, Reflection is used to generate automatic help and version
information based on the ArgumentAttributes you apply to the option class.
Take the example above, this is the generated help documentation:

```
$ .\Clipr.Sample.exe --help
usage: clipr.Sample [ -v|--verbose ] OUT N ...

    This is a set of options.

positional arguments:
    N              Numbers to sum.
    OUT            Output file.

optional arguments:
    -h, --help     Display this help document.
    --version      Display version information.
    -v, --verbose  Increase the verbosity of the output.
```

You may also programmatically generate the help and usage for a class:

```csharp
var parser = new CliParser<Options>(new Options());
var help = new AutomaticHelpGenerator<Options>();
// Gets the usage message, a short summary of available arguments.
Console.WriteLine(help.GetUsage(parser.Config));
// Gets the full help message with argument descriptions.
Console.WriteLine(help.GetHelp(parser.Config));
```

Version information is similarly auto-generated from the version string
compiled into the application using this library. In the future there
will be an easy way to specify the version manually, but until then you'll
have to implement the `IVersion` interface yourself and replace the `Version`
property within the `IHelpGenerator`.