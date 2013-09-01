using System;
using System.Collections.Generic;
using clipr.Fluent;

namespace clipr.Sample
{
    class Program
    {
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
            public IEnumerable<int> Numbers { get; set; } 
        }

        public class VerbTestOptions
        {
            public int NumCounters { get; set; }

            public VerbSubOptions AddInfo { get; set; }
        }

        public class VerbSubOptions
        {
            public string Filename { get; set; }
        }

        static void Attribute(string[] args)
        {
            var opt = CliParser.StrictParse<Options>(args);
            //var opt = CliParser.Parse<Options>(
            //    "-vvv output.txt 1 2 -1 7".Split());
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

        static void Fluent(string[] args)
        {
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
                .Parse(args);

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

        static void FluentWithVerb(string[] args)
        {
            var opt = new VerbTestOptions();
            new CliParser<VerbTestOptions>(opt)
                .HasNamedArgument(c => c.NumCounters)
                .WithShortName()
            .And
                .HasVerb("add", c => c.AddInfo,
                         new CliParser<VerbSubOptions>(new VerbSubOptions())
                             .HasPositionalArgument(c => c.Filename).And)
            .And
                .Parse(args);

            Console.WriteLine("Number of counters: {0}", opt.NumCounters);  // 3
            Console.WriteLine("File name to add: {0}", opt.AddInfo.Filename);  // oranges.txt
        }

        static void Main(string[] args)
        {
            FluentWithVerb("-n3 add oranges.txt".Split());
        }
    }
}
