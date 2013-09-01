clipr: A Command Line Interface ParseR for .Net 3.5+
===============================================

Created by [Dan Nemec](http://github.com/nemec)

We're on [NuGet](https://nuget.org/packages/clipr)!

This project was originally written for .Net 4.5, but I realized that not
everyone is up to speed with the latest .Net version yet. It's now
retargeted to 3.5 with two sets of extension methods that emulate the features
I took advantage of in 4.5 that aren't available in previous versions.

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

    [Command(Description = "This is a set of options.")]
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
        var opt = CliParser.Parse<Options>(
            "-vvv output.txt 1 2 -1 7".Split());
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
    }

##Changelog

###2013-01-16 

* Added TryParse method
* Renamed ParseStrict to StrictParse (for consistency's sake)
* Reordered any public parse methods where T object came first 
  (also to retain consistency across the public API).

###2013-01-13

* Initial release with automated help generation.

##CliParser vs. CliParser<>

If you don't need any special options or custom help generators,
the static CliParser class is the easiest way to initialize the
parser. Behind the scenes it will create the parser and begin parsing
the arguments passed in. If the destination type has a parameterless
construtor, you don't even need to set up the object first! It can `new()`
up an object for you and spit it out after parsing is complete, ready to
be used.

##Parse vs. Tryparse vs. StrictParse

There are two ways to parse a list of arguments. The former, `Parse()`, will
attempt to parse the input arguments and throw a ParseException if something
went wrong while parsing or a ParserExit exception if the help or version
information were triggered and printed to the console. If successful argument
parsing isn't required to run your application, this is probably the correct
method to use.

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
to clean up** when that happens. If you've allocated any resouces or left
some important files in a half-written state, unpredictible things may happen.
Of course, parsing arguments is usually the first thing you do in `Main` so
it's not usually going to be an issue, but `Environment.Exit` is not the
cleanest form of flow control so I feel it deserves a mention.

#Integrity Checking

One of the most important non-functional features of a
library like this is making sure that you, the developer, don't have to
test the input with various garbage to make sure you've implemented the
library correctly. In that spirit, I've done my best to sanity check the
Attributes you place on your options class when the parser is created
(before any arguments are passed to it). You shouldn't have to wait until
a user passes the wrong arguments in to discover that you've defined a
duplicate argument short name or that the constant values you're storing
aren't actually convertible to the property type.

#Features

##Named and Positional Arguments

There are two types of arguments that may be defined: Named and Positional
arguments.

* Named arguments are those set off by short names or long names
  (such as `-v` or `--verbose`). They are always optional
  (in that the parser will not show an error if one is not specified) and may
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

##Multiple Action Types

Storing argument values isn't enough. There are a number of actions that
can be performed when the user specifies an argument.

* Store: Exactly what you'd expect, this action stores the value(s) given
  after the argument in a property.
* StoreConst: Instead letting the user choose which value is stored, a value
  stored in the Attribute's Const property will be used. Intended to avoid
  the `if(opts.FlagWasSet){ opts.SomeOtherProperty = MyConstantValue; }` --
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
  a default value of false, it's worth noting here that nullable boolean
  properties work just fine here, too (`bool? MyFlag { get; set; }`).
* Append: Like Store, but if a user specifies the flag more than once the
  parsed values are appended to the existing ones rather than replacing them
  (eg. `myprog.exe -f file1.txt -f file2.txt`).
* AppendConst: Combine StoreConst and Append and you've got it. It's a
  pretty simple concept.
* Count: Counts the number of times the flag was specified as an argument.
  Good for specifying a "level" (like verbosity).

##Variable Argument Count

In addition to choosing an action for each argument, you can specify
*how many* values each argument can consume. The first part is the NumArgs
property, which gives the value limit for the argument. The second part,
constraints, comes in three flavors: Exact, AtLeast, and AtMost. Any named
argument may take exactly the number of values specified or use that as
the minimum or maximum number consumed. Since positional arguments are not
separated in any discernable way only the *last* positional argument,
by Index, may use the constraints AtLease or AtMost. All previous positional
arguments must consume an exact number of values.

####Force Positional Argument Parsing

If, for any reason, you want the parser to stop parsing named arguments
and count the rest as positional arguments, use a `--`. This is useful
in cases where you want a positional argument that begins with a `-`
(`./prog.exe -- -o --continue`) or when a named argument would otherwise
consume your positional arguments as one of its own
(`./prog.exe --consumes-optional-value -- positional`).

##Mutually Exclusive Arguments

Named arguments can be given a MutuallyExclusiveGroupAttribute. If multiple
named arguments belong to the same group and the user tries to specify more
than one, a parser error is generated. Groups can also be marked as Required.
If at least one MutuallyExclusiveGroupAttribute for a group is required and
the user does *not* provide one of the member arguments, an error is also
generated.

##Verbs

Verbs are the name given to a set of initial arguments that conditionally
parse a set of configuration options based on the given verb. Think
`git add` and `git commit`. Each has a different set of options that's
used in a different way. Each verb class is a valid configuration class,
so verbs are basically a way to combine multiple sets of configurations,
conditionally, into one. Imagine that in addition to `./git`, there was
also `./git-add` and `./git-commit` each with its own configuration classes.
With clipr, you can reuse those individual classes in `./git` as verbs.

Some notes on verbs:

  * A configuration class cannot contain both positional parameters and
    verbs (although the verb itself may define its own positional parameters).
  * Verbs may be nested arbitrarily deep, so long as you adhere to the
    above requirement (although it's not recommended that you nest too
    deeply).
  * Multiple verb attributes may be registered for the same verb. These
    will act like aliases (`svn co` vs. `svn checkout`).
  * PostParse methods will be executed in order from innermost to outermost,
    which means that whenever a PostParse method is executed, the
    configuration class *and* all its verbs will be fully initialized by
    that point.

##Post-Parse Triggers

Using the PostParseAttribute you can mark parameterless methods to be
automatically run once parsing is completed. At present, this is not a
terribly useful feature but in the future these command classes will be
able to be nested as sub-commands (think `svn checkout`). Each sub-command
will then be able to trigger a different feature depending on which
sub-command was provided.

##Generated Help and Version Information

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

Version information is similarly auto-generated from the version string
compiled into the application using this library. In the future there
will be an easy way to specify the version manually, but until then you'll
have to implement the `IVersion` interface yourself and replace the `Version`
property within the `IHelpGenerator`.

##Fluent Interface

Instead of attributes, the parser may be configured using a fluent interface.

    var opt = new Options();

    new CliParser<Options>(opt)
        .HasNamedArgument(o => o.Verbosity)
            .WithShortName('v')
            .CountsInvocations()
    .And
        .HasNamedArgument(o => o.OutputFile)
            .WithShortName()
    .And
        .HasPositionalArgumentList(o => o.Numbers)
            .HasDescription("These are numbers.")
            .Consumes.AtLeast(1)
    .And
        .Parse("-vvv -o out.txt 3 4 5 6".Split());

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

You may also add a Verb to the parser config with this syntax:

    var opt = new Options();
    new CliParser<Options>(opt)
        .HasVerb("add", c => c.AddVerb,
                  // Note that in the Fluent Interface, you're nesting parsers
                  // Theoretically this means you can nest an 
                  // Attribute-configured parser inside a Fluent parser
                  // (although you cannot do the opposite, due to limitations
                  // with Attributes).
                  new CliParser<AddFileOptions>(new AddFileOptions())
                      .HasPositionalArgument(c => c.Filename)
                      .And)  // A necessary evil if defining inline.
    .And
        .Parse("add myfile.txt");

    Console.WriteLine(opt.AddVerb);
    // myfile.txt

##TODO

Render help information for verbs