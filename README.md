clipr: A Command Line Interface ParseR for .Net 3.5+ and .Net Core
===============================================

Created by [Dan Nemec](https://github.com/nemec)

[![NuGet](https://img.shields.io/nuget/dt/clipr.svg)](https://www.nuget.org/packages/clipr/)
[![NuGet](https://img.shields.io/nuget/v/clipr.svg)](https://www.nuget.org/packages/clipr/)
[![NuGet](https://img.shields.io/nuget/vpre/clipr.svg)](https://www.nuget.org/packages/clipr/)

This command line parser library is very much inspired by the
[argparse](http://docs.python.org/2/library/argparse.html) library
for Python. It's one of the easiest parser libraries I've used while
still offering a comprehensive set of powerful features.

I've borrowed and ported many of its features to C# taking advantage of
C#'s static typing features where it makes sense. Thanks to the powerful
nature of the Reflection and TypeDescriptor assemblies, it's quite easy
to take the arguments already sent to any Console project and fill a
class with those values, even if they contain custom Types. Here's a quick
but powerful example of the objects this library enables you to build:

```csharp
[ApplicationInfo(Description = "This is a set of options.")]
public class Options
{
    [NamedArgument('v', "verbose", Action = ParseAction.Count,
        Description = "Increase the verbosity of the output.")]
    public int Verbosity { get; set; }

    [PositionalArgument(0, MetaVar = "OUT",
        Description = "Output file.")]
    public string OutputFile { get; set; }

    [PositionalArgument(1, MetaVar = "N",
        NumArgs = 1,
        Constraint = NumArgsConstraint.AtLeast,
        Description = "Numbers to sum.")]
    public List<int> Numbers { get; set; } 
}

static void Main()
{
    var result = CliParser.Parse<Options>(
        "-vvv output.txt 1 2 -1 7".Split());
    result.Handle(
        opt =>
		{
			Console.WriteLine(opt.Verbosity);
			// >>> 3

			Console.WriteLine(opt.OutputFile);
			// >>> output.txt

			var sum = 0;
			foreach (var number in opt.Numbers)
			{
				sum += number;
			}
			Console.WriteLine(sum);
			// >>> 9
		},
        t => Assert.Fail("Trigger {0} executed.", t),
        e => Assert.Fail("Error parsing arguments."));
}
```

## Changelog

### Master

* Remove exception throwing when an error occurs. (#31)
* Change parsing return type to a union indicating whether parsing
	succeeded, a trigger was hit, or an error occurred. (#34)
* Add a "tokenizer" that turns a regular string into a string
	array similar to that of `Main(string[] args)`.
* Fix minor help page formatting errors.
* Add Verbs/Commands to help page.
* Add the ability to configure the TextWriter for help and version output
  so it is no longer writing only to `Console.Error`.
* Allow each verb to inherit the `--help` trigger so that you can get detailed
  help information for verbs.
* Remove exception throwing when an error occurs. (#31)
* Change parsing return type to a union indicating whether parsing
	succeeded, a trigger was hit, or an error occurred. (#34)
* Add a "tokenizer" that turns a regular string into a string
	array similar to that of `Main(string[] args)`.
* Add `DisplayHelp` method to `CliParser` to make it easy for developers to
	display the help message on demand.
* Add the ability to provide arguments to PostParse methods via dependency injection.
	By default, parameterless methods are supported, but you may also set the
	`PostParseDependencyFactory` property on the `ParserSettings` with a custom
	factory (or a configured instance of `IOC.SimpleObjectFactory`), similar to
	the way verb injection is handled.
* Add a `AttributeValidator` with a few validation attributes built in.
	* `clipr.Validation.AllowedRangeAttribute`: Restricts an primitive integral/floating point
		option to a range of values.
	* `clipr.Validation.DirectoryExistsAttribute`: Applied to a file path, this validates that the
		path exists and is a directory.
	* `clipr.Validation.FileExistsAttribute`: Applied to a file path, this validates that the
		path exists and is a file.

### 2017-07-13 1.6.0.1

* Fix a bug in parsing consecutive Optional values.
* Fix minor resx issues.

### 2017-01-02 1.6.0

* Add ability to mask password.
* Add "optional" value constraint for reference or nullable types.
* Add section in help generation for required named arguments.
* Fixed bug in .Net Core localization resource name.

[Full Changelog](CHANGELOG.md)

## Terminology

Note that all terminology refers to UNIX/Linux style arguments, which is the
style used by this library. Windows style (`/arg`) and Powershell (`-Arg`)
are very similar, but do not distinguish between 'short' and 'long' arguments.

    ┌─ Shell Marker
    │      ┌─ Application Executable
    │      │             ┌─ Verb                           ┌─ Positional Argument Value 
    ↓      ↓             ↓                                 ↓
    $ application.exe download --outfile data.txt -p 80 google.com
	                               ↑        ↑      ↑
								   │        │      └─ Short Named Argument
								   │        └─ Named Argument Value
								   └─ Long Named Argument

* Shell Marker: this symbol is often seen paired with a command line, it
	indicates that the following text is a command that can be written in
	the terminal/console. The character `$` often represents a command run
	as a normal user while the character `#` indicates the command should
	be run under an Administrator or root account.
* Application Executable: this is the name of your CLI application, or the
	full path if the application is not available to the current folder.
* Verb: a verb is the name often given to a "bare" value added to the beginning
	of the command line. Since it often represents an action, the word is
	typically a 'verb', although it can be any word. It can also be used
	to group related arguments together. In clipr, verbs are optional and
	they can be nested to allow multiple verbs in a single command.
* Named Argument: a named argument refers to arguments that are given
	an explicit name on the command line. Because each is referred to by name,
	they are often optional and may be put in any order inside the command. When 
	a named argument (short or long) has no value, it is often called a 'flag'.
* Named Argument Value: the value for a named argument follows immediately after
	the corresponding argument. This allows you to easily identify which value
	belongs to an argument. Some named arguments support multiple values; this
	varies with each command.
* Long Named Argument: this is a named argument that is prefixed by a double dash
	and always contains at least two or more characters in the name.
* Short Named Argument: this is a named argument that is prefixed by a single dash.
	It always consists of a single character and, if it is a flag (no value), you
	may combine	multiple short named arguments together after a single dash
	(e.g. `-sap` is the same as `-s -a -p` - the single leading dash ensures the
	flags are not mistaken for a long named argument).
* Positional Argument: in contrast to a named argument, a positional argument
	is one that is not prefixed by a name. To resolve ambiguity, positional
	arguments are given a specific order by the application developer.

## CliParser vs. CliParser<>

If you don't need any special options or custom help generators,
the static CliParser class is the easiest way to initialize the
parser. Behind the scenes it will create the parser and begin parsing
the arguments passed in. If the destination type has a parameterless
construtor, you don't even need to set up the object first! It can `new()`
up an object for you and spit it out after parsing is complete, ready to
be used.

## Parse vs. Tryparse vs. StrictParse

There are three ways to parse a list of arguments. The former, `Parse()`, will
attempt to parse the input arguments and throw a ParseException if something
went wrong while parsing or a ParserExit exception if the help or version
information were triggered and printed to the console.

The `TryParse()` method is similar to the typical TryParse methods found
on integers and datetimes. It returns a boolean value of true if parsing
succeeded, false otherwise. There is one overload that lets you input an
instance of the type you want to parse (in cases where the constructor takes
parameters), but the other overload uses the `out` keyword to construct
a new instance of the type before parsing. If parsing fails, that instance
will be null.

The `StrictParse()` method was made for a very specific use case -- most
applications that parse arguments, when they encounter an invalid argument
or some other error, will print help / usage information and immediately
quit, letting the user correct her mistakes and rerun the program. In that
spirit, the `StrictParse()` method will not throw any exceptions
(if you see one, report it on the Github page). Instead, it will print the
error message and the one-line usage documentation, then terminate using
`Environment.Exit`. Note that your program **will not have the opportunity
to clean up** when that happens. If you've allocated any unmanaged resouces 
or left some important files in a half-written state, unpredictible things
may happen. Of course, parsing arguments is usually the first thing you do
in `Main` so it's not usually going to be an issue, but `Environment.Exit`
is not the cleanest form of flow control so I feel it deserves a mention.

# Integrity Checking

One of the most important non-functional features of a
library like this is making sure that you, the developer, don't have to
test the input with various garbage to make sure you've implemented the
library correctly. In that spirit, I've done my best to sanity check the
Attributes you place on your options class when the parser is initialized
(before any arguments are passed to it). You shouldn't have to wait until
a user passes the wrong arguments in to discover that you've defined a
duplicate argument short name or that the constant values you're storing
aren't actually convertible to the property type.

In order to use Integrity Checking you must call one of two validation 
methods:

1. The `ValidateAttributeConfig` method returns an array of Exceptions that
	list the various Integrity issues with your configuration. You may
	inspect that collection and handle them accordingly.

    var parser = new CliParser<Options>();
    var errs = parser.ValidateAttributeConfig();
	// do something with errors

2. The `EnsureValidAttributeConfig` method throws an AggregateException
	when any integrity issues are found. The expectation is that this
	exception remains *unhandled* so that, during development, any
	configuration issues are immediately found and handled. Since this
	method does not rely upon runtime input, there is no danger in
	leaving it in when shipping, but for performance reasons you may
	wish to exclude the validation from Release builds.

	var parser = new CliParser<Options>();
    parser.EnsureValidAttributeConfig();

# Features

## Named and Positional Arguments

There are two types of arguments that may be defined: Named and Positional
arguments.

* Named arguments are those set off by short names or long names
  (such as `-v` or `--verbose`). They are optional (in that the parser 
  will not show an error if one is not specified) unless explicitly
  marked as required and may
  be given in any order (eg. the parser does not care if you use
  `-v --input file.txt` or `--input file.txt -v`).
* Positional arguments, on the other hand, are always required and must be
  given in the correct order (indicated by the Index property when defining
  the argument). Since they have a defined order, there is no need to tag
  them with short names or long names: just put them in the argument list
  and they will be parsed automatically.
* While positional arguments are required to be given in *order*, they may
  be freely intermixed with named arguments. For example, the following are
  all valid ways to call the example above and will give the same output:

      clipr.Sample.exe -v -v out.txt 2 3
      clipr.Sample.exe out.txt -v -v 2 3
      clipr.Sample.exe --verbose out.txt 2 -v 3

* Since short arguments must be a single character, any short argument
  with an action that does not consume values (StoreConst, StoreTrue,
  StoreFalse, AppendConst, Count) may be combined together. `-vvs` is
  functionally the same as `-v -v -s`.
* Similarly, short arguments *with* values may be input without a space
  between the flag and the first value (`-fmyfilename.txt`).

## Multiple Action Types

Storing argument values isn't enough. There are a number of actions that
can be performed when the user specifies a named argument.

* Store: Exactly what you'd expect, this action stores the value(s) given
  after the argument in a property and is the default action.
* StoreConst: Instead of letting the user choose which value is stored, a value
  stored in the Attribute's Const property will be used. Intended to avoid
  `if(opts.FlagWasSet){ opts.SomeOtherProperty = MyConstantValue; }` --
  when parsing is finished, you should be ready to roll! Additionally, as long
  as the value stored in Const is convertible to the property type (through
  TypeDescriptor magic, more on that later), the property will be filled when
  the flag is set. Since Attribute properties may only be filled with
  compile-time constants, this allows you to use the same string serialization
  tricks for const arguments that you can use to transform argument strings
  into custom type objects.
* StoreTrue: Shortcut for constructing a boolean StoreConst. Lets you
  specify command line flags that consume no values (eg. `myprog.exe -v`).
* StoreFalse: The opposite of StoreTrue. Since booleans are value types with
  a default value of false already, it's worth noting here that nullable
  boolean properties work just fine, too (`bool? MyFlag { get; set; }`), which
  can be used to detect the presence of the "false" flag.
* Append: Like Store, but if a user specifies the flag more than once the
  parsed values are appended to the existing ones rather than replacing them
  (eg. `myprog.exe -f file1.txt -f file2.txt`). The backing property must
  be some sort of IList.
* AppendConst: Combine StoreConst and Append and you've got it.
* Count: Counts the number of times the flag was specified as an argument.
  Good for specifying a "level" (like verbosity). There is no way to limit
  the number of times a user specifies the argument.

## Variable/Optional Argument Count

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

## Default Argument Values

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
    
## Force Positional Argument Parsing

If, for any reason, you want the parser to stop parsing named arguments
and count the rest as positional arguments, use a `--`. This is useful
in cases where you want a positional argument that begins with a `-`
(`./prog.exe -- --sometext--`) or when a named argument would otherwise
consume your positional arguments as one of its own
(`./prog.exe --consumes-optional-value -- positional`).

## Password Masking

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
var result = CliParser.Parse<Options>(new[] {"-s"});
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
var result = CliParser.Parse<OptionsWithPositional>(new[] {"-s - Nemec"});
```

Prints:

    Input a value for -s:

This feature only applies in a specific set of circumstances:
* Can only annotate a `NamedArgument` (not a `PositionalArgument`).
* The `ParseAction` must be `ParseAction.Store` (the default).

## Argument Validation

Validation allows you to restrict the values passed in to your
program more tightly than the .NET type system allows. This includes
rules like "Make sure this file exists" or "the integer must be within
a range of values". There are two built-in validators and an interface
that allows you to build your own custom validator or plug in an existing
library of your choice, such as `FluentValidator`.

The validator can be accessed or replaced by using the `Validator` property
on the `clipr.CliParser<T>` object and it is automatically executed during
parsing. By default, the parser's validator is assigned to the singleton
`clipr.Validation.AttributeValidator.Default`. This allows you to easily
plug in additional `ValidationAttribute`s, as will be explained below.

### Using the AttributeValidator

The `clipr.Validation.AttributeValidator` is the default validator applied
to a parser (except when using the fluent `CliParserBuilder`, which has no
default validator). Its rules are defined as Attributes (derived from the
`clipr.Validation.ValidationAttribute` class) that are applied to
individual properties in your parser configuration, next to the
`NamedArgument` or `PositionalArgument` attributes. There are three built-in
validation attributes:
	* `clipr.Validation.AllowedRangeAttribute`: Restricts an primitive integral/floating point
		option to a range of values.
	* `clipr.Validation.DirectoryExistsAttribute`: Applied to a file path, this validates that the
		path exists and is a directory.
	* `clipr.Validation.FileExistsAttribute`: Applied to a file path, this validates that the
		path exists and is a file.

Here is an example of applying the `FileExistsAttribute`:

```csharp
public class FileExistsOptions
{
    [FileExists]
    [NamedArgument('c', "config")]
    public string ConfigurationFile { get; set; }
}

public void Main()
{
	var opt = new FileExistsOptions();
	var parser = new CliParser<FileExistsOptions>();

	parser.Parse("-c testdirectory\\testfile.txt".Split(), opt);

	Console.WriteLine(File.ReadAllText(opt.ConfigurationFile));
}
```

Additional custom attribute validations can be defined by implementing a derived
class from `clipr.Validation.ValidationAttribute`. The first method, `CanHandleType`,
allows you to specify which .NET types are supported by the validator. If a developer
tries to apply a validator to the wrong type, you can return a descriptive error message.
The second is `ValidateMember`, which is executed against the class member (e.g. property)
value - due to the limitations of Attributes you will need to cast it to the appropriate type
before validating.

```csharp
public class CustomValidationAttribute : ValidationAttribute
{
    public override bool CanHandleType(Type type, out string errorMessage)
    {
		// Perform type checking and return a descriptive error message.
        if(type != typeof(string))
        {
            errorMessage = "Only System.System.String can be validated by this attribute.";
            return false;
        }
        errorMessage = null;
        return true;
    }

    public override bool ValidateMember(object member, out Exception error)
    {
		// cast and perform some validation on the member object.
		// return 'true' if validation passes, 'false' if it fails.
		// if validation fails, you can set the 'error' with a descriptive Exception.
    }
}
```

Your custom validation attributes may be injected alongside the ones that are built-in
by calling the following method:

```csharp
clipr.Validation.AttributeValidator.Default.AddAttributeValidationType<CustomAttribute>();
// or
clipr.Validation.AttributeValidator.Default.AddAttributeValidationType(typeof(CustomAttribute));
```

If you would like to disable the built-in validation attributes entirely, simply create a new,
 empty `AttributeValidator` and replace the existing one:

 ```csharp
 var validator = new AttributeValidator();
 validator.AddAttributeValidationType<CustomAttribute>();

 var options = new Options();
 var parser = new CliParser<Options>();
 parser.Validator = validator;

 parser.Parse(args, options);  // your CustomAttribute validation is executed here
 ```

### Using the BasicParseValidator

TODO see `clipr.Validation.BasicParseValidator`.

 ```csharp
 var validator = new BasicParseValidator();
 validator.AddRule(o => {
 	 if(o.MyAge < 18) return new ValidationFailure("MyAge", "Must be over 18");
	 return null;
 })

 var options = new Options();
 var parser = new CliParser<Options>();
 parser.Validator = validator;

 parser.Parse(args, options);  // your custom rules are executed here
 ```

### Building Your Own Validator

You may integrate third-party validators or build your own by implementing
the `clipr.Validation.IParseValidator` interface. Simply replace the built-in
validator with your own befor parsing.

 ```csharp
 var validator = new MyCustomValidator();

 var options = new Options();
 var parser = new CliParser<Options>();
 parser.Validator = validator;

 parser.Parse(args, options);  // your custom rules are executed here
 ```

## Parsed Argument Event

Inside the `ParserOptions` used to configure the parser, there is an event
named `OnParseArgument`. This event is fired after each argument is parsed
and contains the parsed value and name of the Property that the value was
stored in. Generally this event should not be needed, but there are some
cases where it is helpful to know the order in which arguments are parsed,
and this will allow the developer to access that order.

## Verbs

Verbs are the name given to a set of initial arguments that conditionally
parse a set of configuration options based on the given verb. Think
`git add` and `git commit`. Each has a different set of options that's
used in a different way. Each verb class is a valid configuration class,
so verbs are basically a way to combine multiple sets of configurations,
conditionally, into one. Imagine that in addition to `./git`, there was
also `./git-add` and `./git-commit` each with its own configuration classes.
With clipr, you can reuse those individual classes in `./git` as verbs.

By default, the parser can automatically create an instance of each verb
type as long as it has a parameterless default constructor. For more
complex verb types, the `CliParser` constructor accepts an implementation of
the `clipr.IOC.IObjectFactory` interface and will delegate to that interface
when it needs to create a verb. Your choice of IOC Container can also be
hooked into this Interface with an adapter. Also provided is a
`clipr.IOC.SimpleObjectFactory` implementation that allows you to define
a factory for each verb type in a collection initializer. The type in
the initializer is optional and, if missing, will be inferred from the
factory's return type.

```csharp
var factory = new SimpleObjectFactory
{
	{ () => new GitAdd(".") }
	{ typeof(GitCommit), () => new GitCommit() }
}
```

```csharp
public class GitOptions
{
	[Verb("add")]
	public GitAdd Add { get; set; }

	[Verb]  // The lowercased property name is used when no name is provided
	public GitCommit Commit { get; set; }
}
```

Some notes on verbs:

  * A configuration class cannot contain both positional parameters and
    verbs (although the verb itself may define its own positional parameters).
  * Verbs may be nested arbitrarily deep, so long as you adhere to the
    above requirement (although it's not recommended that you nest too
    deeply).
  * Multiple verb attributes may be registered for the same verb. These
    will act like aliases (`svn co` vs. `svn checkout`).
  * PostParse methods 

## Post-Parse Triggers

Using the PostParseAttribute you can mark parameterless methods to be
automatically run once parsing is completed. When PostParse methods are
nested within Verbs, they will be executed in order from innermost class
to outermost, which means that whenever a PostParse method is executed, the
configuration class *and* all its verbs will be fully initialized by that
point.

## Generated Help and Version Information

By default, Reflection is used to generate automatic help and version
information based on the ArgumentAttributes you apply to the option class.
Take the example above, this is the generated help documentation:

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

You may also programmatically generate the help and usage for a class:

```csharp
var parser = new CliParser<Options>(new Options());
var help = new AutomaticHelpGenerator<Options>();
// Gets the usage message, a short summary of available arguments.
Console.WriteLine(help.GetUsage(parser.BuildConfig()));
// Gets the full help message with argument descriptions.
Console.WriteLine(help.GetHelp(parser.BuildConfig()));
```

Version information is similarly auto-generated from the version string
compiled into the application using this library. In the future there
will be an easy way to specify the version manually, but until then you'll
have to implement the `IVersion` interface yourself and replace the `Version`
property within the `IHelpGenerator`.

## Localization

clipr supports localization, both of the help UI generated by
`AutomaticHelpGenerator` and the options themselves. Only the description
of an argument is localizable - the long name of an argument cannot
be localized to prevent issues if a shell script that called your application
were run on a PC under a different locale.

Currently, the UI is only localized in three languages: English, Spanish, 
German, and Portuguese. If you're fluent in another language or see a problem
with the existing translations, please consider adding a translation! More
details can be found in [issue #28](https://github.com/nemec/clipr/issues/28).

To localize an argument description, apply the
`clipr.Attributes.LocalizeAttribute` attribute to the property. Localization
requires a strongly-typed Resources class, provided as the `ResourceType`
property to the attribute. If this attribute is applied to the enclosing class,
the `ResourceType` will be inherited by any properties within unless otherwise
specified. If not provided, the `ResourceName` defaults to 'ClassName' if
applied to a class or 'ClassName_PropertyName' if applied to a property.

```csharp
[Localize(ResourceType = typeof(Properties.Resources))]
public class LocalizationOptions
{
    [Localize]  // Resource Name defaults to LocalizationOptions_TurnOnThePower
    [NamedArgument("turnonthepower", Action = ParseAction.StoreTrue)]
    public bool TurnOnThePower { get; set; }

    [Localize("FileToAdd", typeof(Properties.Resources))]
    [PositionalArgument(0)]
    public string FileToAdd { get; set; }
}
```

## TypeDescriptor / TypeConverter

Custom types may be used as config values, but only if a 
[TypeConverter](http://msdn.microsoft.com/en-us/library/ayybcxe5.aspx) is
defined for that type. Since TypeConverters are big, messy beasts, and
for purposes of this library there is only one conversion case
(string -> MyType), the `clipr.Utils` namespace contains a
`StringTypeConverter` that simplifies the conversion interface. After
implementing the converter, tag your custom class with a
[TypeConverterAttribute](http://msdn.microsoft.com/en-us/library/system.componentmodel.typeconverterattribute.aspx)
to register the converter with .Net.

You may also attach a TypeConverter attribute to a Property on the class,
which allows targeted conversion. It also allows you to apply custom
converters to types that you do not own, like built-in classes.

## Static Enumerations

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
  result.Handle(
      opt => Assert.AreSame(SomeEnum.First, opt.Value),
      t => Assert.Fail("Trigger {0} executed.", t),
      e => Assert.Fail("Error parsing arguments."));
}
```

## Fluent Interface

Instead of attributes, the parser may be configured using a fluent interface.
There are five branches off of the parser to configure new arguments:
`AddNamedArgument`, `AddNamedArgumentList`, `AddPositionalArgument`,
`AddPositionalArgumentList`, and `AddVerb`. The argument instances returned
from each of these may be used to configure the individual argument

```csharp
var opt = new Options();

var builder = new CliParserBuilder<Options>();
builder
    .AddNamedArgument(o => o.Verbosity)
    .WithShortName('v')
    .CountsInvocations();
builder
    .AddNamedArgument(o => o.OutputFile)
    .WithShortName();
builder.AddPositionalArgumentList(o => o.Numbers)
    .HasDescription("These are numbers")
    .ConsumesAtLeast(1);

var parser = builder.BuildParser();
parser.Parse("-vvv -o out.txt 3 4 5 6".Split(), opt);

Console.WriteLine(opt.Verbosity);
// >>> 3

Console.WriteLine(opt.OutputFile);
// >>> output.txt
    
var sum = 0;
foreach (var number in opt.Numbers)
{
    sum += number;
}
Console.WriteLine(sum);
// >>> 9
```

You may also add a Verb to the parser config with this syntax:

```csharp
var opt = new Options();

var builder = new CliParserBuilder<Options>();
builder
    .AddNamedArgument(c => c.NumCounters)
    .WithShortName();
builder
    .AddVerb("add", c => c.AddInfo,
        v => v.AddPositionalArgument(c => c.Filename));
var parser = builder.BuildParser();
parser.Parse("add myfile.txt".Split(), opt);

Console.WriteLine(opt.AddInfo.Filename);
// myfile.txt
```

## Dictionary Backend

In addition to binding to a class (via properties), it is also possible
to bind to keys in a dictionary by specifying the indexer (plus key name)
in lieu of a property. The downside is that this requires all keys and 
all values to share the same type, respectively (the TypeConverter will
leave the values in a Dictionary<string, object> as strings). Once you get
past that, what this does is let you *dynamically* define the available
configuration properties at runtime (which is as good as you're going to
get, given that .Net currently doesn't allow ExpandoObject or dynamic
in Expressions).

```csharp
var key = 1;
var opt = new Dictionary<int, string>();
var builder = new CliParserBuilder<Dictionary<int, string>>();
builder.AddNamedArgument(c => c[key])
      .WithShortName('n');

var parser = builder.BuildParser();
parser.Parse("-n frank".Split(), opt);

Console.WriteLine("Parsed Keys:");
foreach (var kv in opt)
{
    Console.WriteLine("\t{0}: {1}", kv.Key, kv.Value);
}
// Parsed Keys:
//    1: frank
```

Notes:

* Any type with an indexer will work, it doesn't have to be a Dictionary.
* Non-constant key expressions will be evaluated at configuration time
  (such as dict[MyProperty] or dict[MethodCall()]), so variables may
  also be used as keys without issue.
* Since they're evaluated in configuration (rather than parsing), *any*
  type may be used as the indexer key, even if it's not convertible
  from a string.
