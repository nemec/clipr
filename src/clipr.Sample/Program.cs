using System;
using System.Collections.Generic;
using clipr.Usage;
using clipr.Utils;
using System.Globalization;
using System.Threading;

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

            new CliParserBuilder<Options>()
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

        static void FluentWithVerb(string[] args)
        {
            var opt = new VerbTestOptions();
            new CliParserBuilder<VerbTestOptions>()
                .HasNamedArgument(c => c.NumCounters)
                .WithShortName()
            .And
                .HasVerb("add", c => c.AddInfo,
                         new CliParserBuilder<VerbSubOptions>()
                             .HasPositionalArgument(c => c.Filename).And)
            .And.Parser
                .Parse(args, opt);

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
            var builder = new CliParserBuilder<ConditionalOptions>();

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

            builder.Parser.Parse(args, opt);
            Console.WriteLine("Filename: {0}", opt.Filename);
            Console.WriteLine("Url: {0}", opt.Url);
        }

        public static void DictBackedConfiguration(string[] args)
        {
            var opt = new Dictionary<string, string>();
            var builder = new CliParserBuilder<Dictionary<string, string>>();
            builder.HasNamedArgument(c => c["name"])
                  .WithShortName('n');

            builder.Parser.Parse(args, opt);

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
            var builder = new CliParserBuilder<Dictionary<int, object>>();
            builder.HasNamedArgument(c => c[key])
                  .WithShortName('n');

            builder.Parser.Parse(args, opt);

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
            var result = CliParser.Parse<CustomDateTimeOptions>(args);
            result.Handle(
                opt => Console.WriteLine(opt.CurrentDate[1]),
                t => { },
                e => { });
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
            var result = CliParser.Parse<RequiredNamedArgument>(args);
            result.Handle(
                opt => Console.WriteLine(opt.CurrentDate),
                t => { },
                e => { });
            // >>> 
        }

        public class LocalizationOptions
        {
            [NamedArgument("turnonthepower", Description = "Set power level to 9001", Action = ParseAction.StoreTrue)]
            public bool TurnOnThePower { get; set; }

            [PositionalArgument(0, Description = "Files to add to the thing", Constraint = NumArgsConstraint.AtLeast, NumArgs = 1)]
            public IList<string> FilesToAdd { get; set; }

            [NamedArgument('s', Description = "Start date")]
            public DateTime StartDate { get; set; }

            [NamedArgument('c', Description = "A cool counter")]
            public double MyCounter { get; set; }
        }

        static void PrintLocalizedHelp()
        {
            var culture = Thread.CurrentThread.CurrentUICulture;

            var opt = new LocalizationOptions();
            var parser = new CliParser<LocalizationOptions>();
            var help = new AutomaticHelpGenerator<LocalizationOptions>();

            parser.Parse("--turnonthepower -s 3/12/2016 -c 2.3 file1.txt".Split(), opt);
            Console.WriteLine(opt.StartDate.Month); // 3
            Console.WriteLine(opt.MyCounter);  // 2.3

            Console.WriteLine(help.GetHelp(parser.BuildConfig()));

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("es-MX");

            var optMx = new LocalizationOptions();
            var parserMx = new CliParser<LocalizationOptions>();

            parserMx.Parse("--turnonthepower -s 12/3/2016 -c 2.3 file1.txt".Split(), optMx);
            Console.WriteLine(optMx.StartDate.Month);  // 3
            Console.WriteLine(optMx.MyCounter);  // 2.3

            Console.WriteLine(help.GetHelp(parserMx.BuildConfig()));

            Thread.CurrentThread.CurrentUICulture = new CultureInfo("es-ES");

            var optEs = new LocalizationOptions();
            var parserEs = new CliParser<LocalizationOptions>();

            parserEs.Parse("--turnonthepower -s 12/3/2016 -c 2,3 file1.txt".Split(), optEs);
            Console.WriteLine(optEs.StartDate.Month);  // 3
            Console.WriteLine(optEs.MyCounter);  // 2.3

            Console.WriteLine(help.GetHelp(parserEs.BuildConfig()));

            Thread.CurrentThread.CurrentUICulture = culture;
        }

        public class OptsWithVerb
        {
            [Verb]
            public OptVerb Download { get; set; }
        }

        public class OptVerb
        {
            [NamedArgument("delete", Action = ParseAction.StoreTrue)]
            public bool Delete { get; set; }
        }

        public static void DoVerb()
        {
            var result = CliParser.Parse<OptsWithVerb>("download --delete".Split());
            result.Handle(
                opt => Console.WriteLine(opt.Download.Delete),
                t => { },
                e => { });
        }

        public class OptsWithPwMasking
        {
            [PromptIfValueMissing(MaskInput = true)]
            [NamedArgument('p', "password")]
            public string Password { get; set; }
        }

        public static void DoPwMasking()
        {
            var result = CliParser.Parse<OptsWithPwMasking>("-p".Split());
            result.Handle(
                opt => Console.WriteLine(opt.Password),
                t => { },
                e => { });
        }

        public class OptsWithPwMaskingAndPositional
        {
            [PromptIfValueMissing(MaskInput = true)]
            [NamedArgument('p', "password")]
            public string Password { get; set; }

            [PositionalArgument(0)]
            public string Name { get; set; }
        }

        public static void DoPwMaskingAndPositional()
        {
            var result = CliParser.Parse<OptsWithPwMaskingAndPositional>("-p - test".Split());
            result.Handle(
                opt => Console.WriteLine(opt.Password),
                t => { },
                e => { });
        }

        public enum Color
        {
            Red,
            Green,
            Blue
        }


        [StaticEnumeration]
        internal abstract class ColorEnum
        {
            public static readonly ColorEnum Red = new EnumValue(Color.Red);


            public class EnumValue : ColorEnum
            {
                public EnumValue(Color value)
                {
                    Value = value;
                }

                public Color Value { get; private set; }

                public override string ToString()
                {
                    return Value.ToString();
                }
            }
        }

        internal class StaticEnumListOptions
        {
            [NamedArgument('c', "colors", Action = ParseAction.Append, Constraint = NumArgsConstraint.AtLeast, Const = 1)]
            public IList<ColorEnum> Colors { get; set; }
        }

        public static void ParseStaticEnumList()
        {
            var result = CliParser.Parse<StaticEnumListOptions>("-c Red".Split());
            result.Handle(
                opt =>
                {
                    foreach (var color in opt.Colors)
                        Console.WriteLine("Color: {0}", color);
                },
                t => { },
                e => { });
            
        }

        public class OrderedArgs
        {
            [NamedArgument('f', "foo", Action = ParseAction.StoreTrue)]
            public bool Foo { get; set; }

            [NamedArgument('b', "bar")]
            public string Bar { get; set; }

            [NamedArgument("function", Action = ParseAction.StoreTrue)]
            public bool Function { get; set; }
        }

        public static void ParseArgsWithEvent()
        {
            var cfg = new ParserOptions();
            cfg.OnParseArgument += (ctx, args) =>
            {
                switch (args.ArgumentName)
                {
                    case "Foo":
                        Console.WriteLine("foo");
                        break;
                    case "Bar":
                        Console.WriteLine("bar " + args.Value.ToString());
                        break;
                    case "Function":
                        Console.WriteLine("function");
                        break;
                }
            };

            var parser = new CliParser<OrderedArgs>(cfg);
            parser.Parse("--foo --bar 5 --function".Split(), new OrderedArgs());

            Console.WriteLine("--- Next ---");

            parser.Parse("--bar 3 --function --foo".Split(), new OrderedArgs());
        }
        

        static void Main()
        {
            //FluentWithVerb("-n3 add oranges.txt".Split());
            //FluentConditional("http", "-u http://file".Split());
            //DictBackendMethodConfig("-n frank".Split());
            //CustomDateTime("-d 20140730 2013-09-10".Split());
            //ParseRequiredNamedArgument("-c -d 10/13/2010".Split());
            //DoPwMaskingAndPositional();
            //ParseStaticEnumList();
            ParseArgsWithEvent();
        }
    }
}