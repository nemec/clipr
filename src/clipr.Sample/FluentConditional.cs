using System;

namespace clipr.Sample
{
    public class FluentConditional
    {
        public class ConditionalOptions
        {
            public string Filename { get; set; }

            public string Url { get; set; }
        }

        public static void Main(string destinationFromConfig, string[] args)
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
    }
}