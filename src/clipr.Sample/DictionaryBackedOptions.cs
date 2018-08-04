using System;
using System.Collections.Generic;

namespace clipr.Sample
{
    public class DictionaryBackedOptions
    {
        public static void DictBackedConfiguration(string[] args)
        {
            var opt = new Dictionary<string, string>();
            var builder = new CliParserBuilder<Dictionary<string, string>>();
            builder.AddNamedArgument(c => c["name"])
                .WithShortName('n');

            builder.BuildParser().Parse(args, opt);

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
            builder.AddNamedArgument(c => c[key])
                .WithShortName('n');

            builder.BuildParser().Parse(args, opt);

            Console.WriteLine("Parsed Keys:");
            foreach (var kv in opt)
            {
                Console.WriteLine("\t{0}: {1}", kv.Key, kv.Value);
            }
        }
    }
}