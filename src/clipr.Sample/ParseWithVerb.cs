using System;

namespace clipr.Sample
{
    class ParseWithVerb
    {
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
    }
}