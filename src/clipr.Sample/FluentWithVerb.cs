using System;

namespace clipr.Sample
{
    public class FluentWithVerb
    {
        public class VerbTestOptions
        {
            public int NumCounters { get; set; }

            public VerbSubOptions AddInfo { get; set; }
        }

        public class VerbSubOptions
        {
            public string Filename { get; set; }
        }

        public static void Main(string[] args)
        {
            var opt = new VerbTestOptions();
            var builder = new CliParserBuilder<VerbTestOptions>();
            builder
                .AddNamedArgument(c => c.NumCounters)
                .WithShortName();
            builder
                .AddVerb("add", c => c.AddInfo,
                    v => v.AddPositionalArgument(c => c.Filename));
            var parser = builder.BuildParser();
            parser.Parse(args, opt);

            Console.WriteLine("Number of counters: {0}", opt.NumCounters);  // 3
            Console.WriteLine("File name to add: {0}", opt.AddInfo.Filename);  // oranges.txt
        }
    }
}