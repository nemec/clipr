using System;
using System.Collections.Generic;

namespace clipr.Sample
{
    class Program
    {
        public class Config
        {
            [NamedArgument('n', "name")]
            public string Name { get; set; }

            [PositionalArgument(0)]
            public List<string> Something { get; set; }

            [PositionalArgument(1, NumArgs = 3)]
            public List<string> SomethingElse { get; set; } 

            public Config()
            {
                Something = new List<string>();
                SomethingElse = new List<string>();
            }
        }

        static void Main(string[] args)
        {
            var conf = new Config();
            var parser = new CliParser<Config>(conf);

            parser.Parse("-n timothy".Split());

            Console.WriteLine(conf.Name);
        }
    }
}
