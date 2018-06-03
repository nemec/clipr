using System;

namespace clipr.Sample
{
    class OnParseArgumentEvent
    {
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
            var cfg = new ParserSettings<OrderedArgs>();
            cfg.OnParseArgument = args =>
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
                return null;
            };

            var parser = new CliParser<OrderedArgs>(cfg);
            parser.Parse("--foo --bar 5 --function".Split(), new OrderedArgs());

            Console.WriteLine("--- Next ---");

            parser.Parse("--bar 3 --function --foo".Split(), new OrderedArgs());
        }
    }
}