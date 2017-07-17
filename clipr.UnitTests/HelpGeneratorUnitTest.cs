using clipr.Core;
using clipr.Usage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

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
        public void Help_WithNoDescription_NoNullPointer()
        {

            var args = "--help".Split();
            var opts = new NoDescription();
            var parser = new CliParser<NoDescription>(
                ParserOptions.Default,
                new Help());

            var result = parser.Parse(args, opts);
            result.Handle(
                opt => Assert.Fail("Parse succeeded but trigger was expected."),
                t => Assert.IsInstanceOfType(t, typeof(AutomaticHelpGenerator<NoDescription>)),
                errs => Assert.Fail("Error occurred but trigger was expected."));
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
            
            var parser = new CliParser<OptionsWithLongDescription>();
            var gen = new AutomaticHelpGenerator<OptionsWithLongDescription>
            {
                DisplayWidth = 80
            };


            var help = gen.GetHelp(parser.BuildConfig()).Split('\n');

            Assert.AreEqual(expected, help[3]);
        }

        [TestMethod]
        public void Help_With40CharacterDisplayWidth_PrintsUpTo40Characters()
        {
            const string expected = " -n, --name  Lorem ipsum dolor sit amet,\r";
            
            var parser = new CliParser<OptionsWithLongDescription>();
            var gen = new AutomaticHelpGenerator<OptionsWithLongDescription>
            {
                DisplayWidth = 40
            };


            var help = gen.GetHelp(parser.BuildConfig()).Split('\n');

            Assert.AreEqual(expected, help[3]);
        }

        [ApplicationInfo(Description = "This is a set of options.")]
        public class RequiredNamedOptions
        {
            [NamedArgument('c', "confirm", Action = ParseAction.StoreTrue, Required = true,
                 Description = "Confirms that the action is intended.")]
            public bool Confirmed { get; set; }
        }

        [TestMethod]
        public void Help_WithRequiredNamedArgument_ShowsArgumentInRequiredSection()
        {
            const string expected = @"Usage: clipr [ -h|--help ] [ --version ] -c|--confirm

 This is a set of options.

Required Arguments:
 -c, --confirm  Confirms that the action is intended.

Optional Arguments:
 -h, --help     Display this help document.
 --version      Displays the version of the current executable.";
            
            var parser = new CliParser<RequiredNamedOptions>();
            var gen = new AutomaticHelpGenerator<RequiredNamedOptions>();

            var help = gen.GetHelp(parser.BuildConfig());

            Assert.AreEqual(expected, help);
        }

        internal class OptionsWithRequiredValue
        {
            [NamedArgument('a', "optiona", Required = true, Description = "The A option.")]
            public string Optiona { get; set; }

            [NamedArgument('b', "optionb", Required = false, Description = "The B option.")]
            public string Optionb { get; set; }
        }

        [TestMethod]
        public void Help_WithRequiredNamedArgumentAndValue_PlacesSpaceBetweenArgumentAndMetaVar()
        {
            const string expected = @"Usage: clipr [ -h|--help ] [ --version ] -a|--optiona A [ -b|--optionb B ]
Required Arguments:
 -a, --optiona  The A option.

Optional Arguments:
 -b, --optionb  The B option.
 -h, --help     Display this help document.
 --version      Displays the version of the current executable.";
            
            var parser = new CliParser<OptionsWithRequiredValue>();
            var gen = new AutomaticHelpGenerator<OptionsWithRequiredValue>();

            var help = gen.GetHelp(parser.BuildConfig());

            Assert.AreEqual(expected, help);
        }

        internal class OptionsWithVerbs
        {
            [Verb(Description = "A verb for names")]
            public Verb1 Verb1 { get; set; }

            [Verb(Description = "A verb for ages")]
            public Verb2 Verb2 { get; set; }
        }

        internal class Verb1
        {
            [NamedArgument('n', "name")]
            public string Name { get; set; }
        }

        internal class Verb2
        {
            [NamedArgument('a', "age")]
            public int Age { get; set; }
        }

        [TestMethod]
        public void Help_WithVerbs_AddsVerbDescriptionsToHelpOutput()
        {
            const string expected = @"Usage: clipr [ -h|--help ] [ --version ] <command>
Optional Arguments:
 -h, --help  Display this help document.
 --version   Displays the version of the current executable.

Commands:
 verb1       A verb for names
 verb2       A verb for ages";
            
            var parser = new CliParser<OptionsWithVerbs>();
            var gen = new AutomaticHelpGenerator<OptionsWithVerbs>();

            var help = gen.GetHelp(parser.BuildConfig());

            Assert.AreEqual(expected, help);
        }

        [TestMethod]
        public void Help_WithHelpOfVerbCalled_GeneratedHelpOutputForVerb()
        {
            const string expected = @"Usage: clipr verb1 [ -n|--name N ] [ -h|--help ]
Optional Arguments:
 -h, --help  Display this help document.
 -n, --name
";

            var sw = new StringWriter();
            var opt = new OptionsWithVerbs();
            var parser = new CliParser<OptionsWithVerbs>(new ParserOptions { OutputWriter = sw });

            parser.Parse("verb1 --help".Split(), opt);
            var actual = sw.ToString();

            Assert.AreEqual(expected, actual);
        }

        internal class OptionsWithNestedVerbs
        {
            [Verb(Description = "A verb for names")]
            public Verb1WithNested Verb1 { get; set; }
        }

        internal class Verb1WithNested
        {
            [NamedArgument('n', "name")]
            public string Name { get; set; }

            [Verb(Description = "A verb for ages")]
            public Verb2WithNested Verb2 { get; set; }
        }

        internal class Verb2WithNested
        {
            [NamedArgument('a', "age")]
            public int Age { get; set; }
        }

        [TestMethod]
        public void Help_WithNestedHelpOfVerbCalledOnBaseVerb_GeneratedHelpOutputForVerbWithCommands()
        {
            const string expected = @"Usage: clipr verb1 [ -n|--name N ] [ -h|--help ] <command>
Optional Arguments:
 -h, --help  Display this help document.
 -n, --name

Commands:
 verb2       A verb for ages
";

            var sw = new StringWriter();
            var opt = new OptionsWithNestedVerbs();
            var parser = new CliParser<OptionsWithNestedVerbs>(new ParserOptions { OutputWriter = sw });

            parser.Parse("verb1 --help".Split(), opt);
            var actual = sw.ToString();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Help_WithNestedHelpOfVerbCalled_GeneratedHelpOutputForVerb()
        {
            const string expected = @"Usage: clipr verb1 verb2 [ -a|--age A ] [ -h|--help ]
Optional Arguments:
 -a, --age
 -h, --help  Display this help document.
";

            var sw = new StringWriter();
            var opt = new OptionsWithNestedVerbs();
            var parser = new CliParser<OptionsWithNestedVerbs>(new ParserOptions { OutputWriter = sw });

            parser.Parse("verb1 verb2 --help".Split(), opt);
            var actual = sw.ToString();

            Assert.AreEqual(expected, actual);
        }

        // TODO GenerateUsage_WithStaticEnum_ListsEnumValues()
    }
}
