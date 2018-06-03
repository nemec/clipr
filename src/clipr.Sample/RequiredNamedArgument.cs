using System;

namespace clipr.Sample
{
    public class RequiredNamedArgument
    {
        public class RequiredNamedArgumentOptions
        {
            [NamedArgument('d', "date", Required = true,
                           Description = "Store the date.")]
            public string CurrentDate { get; set; }

            [NamedArgument('c', Action = ParseAction.StoreTrue,
                Description = "Do some other thing with cool results.")]
            public bool Other { get; set; }
        }

        public static void Main(string[] args)
        {
            var result = CliParser.Parse<RequiredNamedArgumentOptions>(args);
            result.Handle(
                opt => Console.WriteLine(opt.CurrentDate),
                t => { },
                e => { });
            // >>> 
        }
    }
}