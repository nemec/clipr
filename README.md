clipr: A Command Line Interface ParseR for .Net
===============================================

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

    public class Options
    {
        [NamedArgument('v', Action = ParseAction.Count)]
        public int Verbosity { get; set; }

        [PositionalArgument(0)]
        public string OutputFile { get; set; }

        [PositionalArgument(1,
            NumArgs = 1,
            Constraint = NumArgsConstraint.AtLeast)]
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

Integrity Checking
==================

One of the most important non-functional features of a
library like this is making sure that you, the developer, doesn't have to
test the input with various garbage to make sure you've implemented the
library correctly. In that spirit, I've done my best to sanity check the
Attributes you place on your options class when the parser is created
(before any arguments are passed to it). You shouldn't have to wait until
a user passes the wrong arguments in to discover that your list of numbers
isn't defined with the type `List<int>` or that your counter is actually
a `string` making any use of that argument throw an exception.