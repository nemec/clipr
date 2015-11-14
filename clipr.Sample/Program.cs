using System;
using System.Collections.Generic;
using clipr.Usage;
using clipr.Utils;
using System.Globalization;

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

            new CliParserBuilder<Options>(opt)
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
            .And.Parser
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
            new CliParserBuilder<VerbTestOptions>(opt)
                .HasNamedArgument(c => c.NumCounters)
                .WithShortName()
            .And
                .HasVerb("add", c => c.AddInfo,
                         new CliParserBuilder<VerbSubOptions>(new VerbSubOptions())
                             .HasPositionalArgument(c => c.Filename).And)
            .And.Parser
                .Parse(args);

            Console.WriteLine("Number of counters: {0}", opt.NumCounters);  // 3
            Console.WriteLine("File name to add: {0}", opt.AddInfo.Filename);  // oranges.txt
        }

        public class ConditionalOptions
        {
            public string Filename { get; set; }

            public string Url { get; set; }
        }

        public static void FluentConditional(string destinationFromConfig, string[] args)
        {
            var opt = new ConditionalOptions();
            var builder = new CliParserBuilder<ConditionalOptions>(opt);

            switch (destinationFromConfig)
            {
                case "file":
                    builder.HasNamedArgument(c => c.Filename)
                          .WithShortName('f');
                    break;
                //case "http":
                default:
                    builder.HasNamedArgument(c => c.Url)
                          .WithShortName('u');
                    break;
            }

            builder.Parser.Parse(args);
            Console.WriteLine("Filename: {0}", opt.Filename);
            Console.WriteLine("Url: {0}", opt.Url);
        }

        public static void DictBackedConfiguration(string[] args)
        {
            var opt = new Dictionary<string, string>();
            var builder = new CliParserBuilder<Dictionary<string, string>>(opt);
            builder.HasNamedArgument(c => c["name"])
                  .WithShortName('n');

            builder.Parser.Parse(args);

            Console.WriteLine("Parsed Keys:");
            foreach (var kv in opt)
            {
                Console.WriteLine("\t{0}: {1}", kv.Key, kv.Value);
            }
        }

        public static void DictBackendMethodConfig(string[] args)
        {
            const int key = 1;
            var opt = new Dictionary<int, object>();
            var builder = new CliParserBuilder<Dictionary<int, object>>(opt);
            builder.HasNamedArgument(c => c[key])
                  .WithShortName('n');

            builder.Parser.Parse(args);

            Console.WriteLine("Parsed Keys:");
            foreach (var kv in opt)
            {
                Console.WriteLine("\t{0}: {1}", kv.Key, kv.Value);
            }
        }

        public class CustomDateTimeConverter : StringTypeConverter<DateTime>
        {
            private static readonly string[] Formats = new[]{ "yyyyMMdd", "yyyy/MM/dd", "yyyy-MM-dd" };

            public override DateTime ConvertFrom(System.Globalization.CultureInfo culture, string value)
            {
                return DateTime.ParseExact(value, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None);
            }
            public override bool IsValid(string value)
            {
                DateTime date;
                return DateTime.TryParseExact(value, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
            }
        }

        [ApplicationInfo(Description = "This is a set of options.")]
        public class CustomDateTimeOptions
        {
            [System.ComponentModel.TypeConverter(typeof(CustomDateTimeConverter))]
            [NamedArgument('d', "date", Action = ParseAction.Append,
                           Description = "Store the date.")]
            public List<DateTime> CurrentDate { get; set; }
        }

        static void CustomDateTime(string[] args)
        {
            var opt = CliParser.Parse<CustomDateTimeOptions>(args);
            Console.WriteLine(opt.CurrentDate[1]);
            // >>> 
        }

        public class RequiredNamedArgument
        {
            [NamedArgument('d', "date", Required = true,
                           Description = "Store the date.")]
            public string CurrentDate { get; set; }

            [NamedArgument('c', Action = ParseAction.StoreTrue,
                Description = "Do some other thing with cool results.")]
            public bool Other { get; set; }
        }

        static void ParseRequiredNamedArgument(string[] args)
        {
            var opt = CliParser.Parse<RequiredNamedArgument>(args);
            Console.WriteLine(opt.CurrentDate);
            // >>> 
        }


        static void Main()
        {
            FluentWithVerb("-n3 add oranges.txt".Split());
            FluentConditional("http", "-u http://file".Split());
            //DictBackendMethodConfig("-n frank".Split());
            //CustomDateTime("-d 20140730 2013-09-10".Split());
            ParseRequiredNamedArgument("-c -d 10/13/2010".Split());

            var parser = new CliParser<RequiredNamedArgument>(new RequiredNamedArgument());
            var help = new AutomaticHelpGenerator<RequiredNamedArgument>();
            Console.WriteLine(help.GetHelp(parser.Config));
        }
    }
}
