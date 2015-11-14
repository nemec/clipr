using clipr.Core;
using clipr.Usage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace clipr.UnitTests
{
    class NoDescription
    {
        [NamedArgument("name")]
        public string Name { get; set; }
    }

    class Help: AutomaticHelpGenerator<NoDescription>
    {
        public Help()
        {
            ShortName = null;
            LongName = "help";
        }
    }

    [TestClass]
    public class HelpGeneratorUnitTest
    {
        [TestMethod]
        [ExpectedException(typeof(ParserExit))]
        public void Help_WithNoDescription_NoNullPointer()
        {

            var args = "--help".Split();
            var opt = new NoDescription();
            var parser = new CliParser<NoDescription>(
                opt,
                ParserOptions.None,
                new Help());
            parser.Parse(args);
        }
        
        [StaticEnumeration]
        internal class MyEnum
        {
            [EnumerationDescription("Some enum one")]
            public static readonly MyEnum First = new MyEnum();

            [EnumerationDescription("Some enum two")]
            public static readonly MyEnum Second = new MyEnum();
        }

        internal class StaticEnumOptions
        {
            public MyEnum Value { get; set; }
        }

        // TODO GenerateUsage_WithStaticEnum_ListsEnumValues()
    }
}
