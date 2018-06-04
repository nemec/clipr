using System;

namespace clipr.Sample
{
    public class ReusableCliParser
    {
        public class Options
        {
            [NamedArgument('n', "name")]
            public string Name { get; set; }

            [NamedArgument('a', "age")]
            public int Age { get; set; }
        }

        public static void Main()
        {
            var parser = new CliParser<Options>();

            var obj1 = new Options();
            parser.Parse("--name tim -a 26".Split(), obj1);

            var obj2 = new Options();
            parser.Parse("--age 23 --name frank".Split(), obj2);

            Console.WriteLine("{0}: {1}", obj1.Name, obj1.Age);
            Console.WriteLine("{0}: {1}", obj2.Name, obj2.Age);
        }
    }
}