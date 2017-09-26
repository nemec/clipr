Title: Creating the Parser
Description: How to use the static factory CliParser class.
---

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