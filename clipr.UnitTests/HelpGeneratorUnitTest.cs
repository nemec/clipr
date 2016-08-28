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

        public class OptionsWithLongDescription
        {
            [NamedArgument('n', "name", Description = @"Lorem ipsum dolor sit amet,
consectetur adipiscing elit. Donec eget nunc semper, cursus purus et, placerat
magna. Mauris porttitor ante sit amet erat consequat, in euismod velit
imperdiet. Cras placerat tempus nisl id lacinia. Nulla facilisi. Pellentesque
dignissim, eros pellentesque facilisis porta, ligula magna venenatis sem, sed
dignissim risus turpis sit amet augue. Ut tincidunt mi faucibus dictum posuere.
Nullam condimentum consectetur interdum.")]
            public string Name { get; set; }
        }

        [TestMethod]
        public void Help_With80CharacterDisplayWidth_PrintsUpTo80Characters()
        {
            const string expected = " -n, --name  Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec eget\r";

            var opt = new OptionsWithLongDescription();
            var parser = new CliParser<OptionsWithLongDescription>(opt);
            var gen = new AutomaticHelpGenerator<OptionsWithLongDescription>
            {
                DisplayWidth = 80
            };


            var help = gen.GetHelp(parser.Config).Split('\n');

            Assert.AreEqual(expected, help[3]);
        }

        [TestMethod]
        public void Help_With40CharacterDisplayWidth_PrintsUpTo40Characters()
        {
            const string expected = " -n, --name  Lorem ipsum dolor sit amet,\r";

            var opt = new OptionsWithLongDescription();
            var parser = new CliParser<OptionsWithLongDescription>(opt);
            var gen = new AutomaticHelpGenerator<OptionsWithLongDescription>
            {
                DisplayWidth = 40
            };


            var help = gen.GetHelp(parser.Config).Split('\n');

            Assert.AreEqual(expected, help[3]);
        }

        // TODO GenerateUsage_WithStaticEnum_ListsEnumValues()
    }
}
