using System;
using System.Collections.Generic;

namespace clipr.Sample {
    public class BasicExample
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

            var builder = new CliParserBuilder<Options>();
            builder
                .AddNamedArgument(o => o.Verbosity)
                .WithShortName('v')
                .CountsInvocations();
            builder
                .AddNamedArgument(o => o.OutputFile)
                .WithShortName();
            builder
                .AddPositionalArgumentList(o => o.Numbers)
                .WithDescription("These are numbers")
                .ConsumesAtLeast(1);

            builder.BuildParser()
                .Parse(args, opt);

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
    }
}