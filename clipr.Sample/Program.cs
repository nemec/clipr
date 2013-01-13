using System;
using System.Collections.Generic;

namespace clipr.Sample
{
    class Program
    {

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
    }
}
