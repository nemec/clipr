using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using clipr.IOC;
using clipr.Usage;
using System.Collections.Generic;
using System.Linq;

namespace clipr.UnitTests
{
    [TestClass]
    public class VerbUnitTest
    {
        public class Options
        {
            [Verb("add")]
            public AddVerb AddInfo { get; set; }
        }

        public class AddVerb
        {
            [PositionalArgument(0)]
            public string Filename { get; set; }
        }

        [TestMethod]
        public void Parse_WithSubcommand_InitializedSubcommandObject()
        {
            var result = CliParser.Parse<Options>("add file.txt".Split());
            result.Handle(
                opt => Assert.AreEqual("file.txt", opt.AddInfo.Filename),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        public class NamedOptions
        {
            [NamedArgument('v', Action = ParseAction.StoreTrue)]
            public bool Verbose { get; set; }

            [Verb("add")]
            public AddVerb AddInfo { get; set; }
        }

        [TestMethod]
        public void Parse_WithNamedArgumentAndVerbWithPositionalArgument_ParsesNamedArgumentsForTopLevelOptions()
        {
            var result = CliParser.Parse<NamedOptions>("-v add file.txt".Split());
            result.Handle(
                opt => Assert.IsTrue(opt.Verbose),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Parse_WithNamedArgumentAndVerbWithNamedArgument_LeavesVerbObjectNull()
        {
            var result = CliParser.Parse<NamedOptions>("-v".Split());
            result.Handle(
                opt => Assert.IsNull(opt.AddInfo),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Parse_WithNamedArgumentAndVerbWithPositionalArgument_ParsesNamedArgumentsForVerbOptions()
        {
            var result = CliParser.Parse<NamedOptions>("-v add file.txt".Split());
            result.Handle(
                opt => Assert.AreEqual("file.txt", opt.AddInfo.Filename),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        public class PostParseOuter
        {
            [NamedArgument('u')]
            public string Username { get; set; }

            [Verb("counter")]
            public PostParseInner Counter { get; set; }

            [PostParse]
            public void PostParse()
            {
                PostParseBuilder.Append(Username);
            }
        }

        public static StringBuilder PostParseBuilder { get; set; }

        public class PostParseInner
        {
            [NamedArgument('c')]
            public int Count { get; set; }

            [PostParse]
            public void PostParse()
            {
                PostParseBuilder.Append(Count);
            }
        }

        [TestMethod]
        public void Parse_WithNestedPostParseMethods_ExecutesMethodsFromInnerToOuterClass()
        {
            PostParseBuilder = new StringBuilder();

            CliParser.Parse<PostParseOuter>("-u frank counter -c 5".Split());

            Assert.AreEqual("5frank", PostParseBuilder.ToString());
        }

        public class NestedVerb1
        {
            [Verb("a")]
            public NestedVerb2 Verb { get; set; }
        }

        public class NestedVerb2
        {
            [Verb("b")]
            public NestedVerb3 Verb { get; set; }
        }

        public class NestedVerb3
        {
            [PositionalArgument(0)]
            public string Name { get; set; }
        }

        [TestMethod]
        public void Parse_WithMultiplyNestedVerbs_ParsesAllDepths()
        {
            var result = CliParser.Parse<NestedVerb1>("a b Horace".Split());
            result.Handle(
                opt => Assert.AreEqual("Horace", opt.Verb.Verb.Name),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        public class VerbOuterWithAliases
        {
            [Verb("co")]
            [Verb("checkout")]
            public VerbInnerWithAliases Checkout { get; set; }
        }

        public class VerbInnerWithAliases
        {
            [PositionalArgument(0)]
            public string Repo { get; set; }
        }

        [TestMethod]
        public void Parse_WithVerbTaggedWithTwoNamesAndFirstGivenAsArg_ParsesVerb()
        {
            var result = CliParser.Parse<VerbOuterWithAliases>("co myRepo".Split());
            result.Handle(
                opt => Assert.AreEqual("myRepo", opt.Checkout.Repo),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Parse_WithVerbTaggedWithTwoNamesAndSecondGivenAsArg_ParsesVerb()
        {
            var result = CliParser.Parse<VerbOuterWithAliases>("checkout myRepo".Split());
            result.Handle(
                opt => Assert.AreEqual("myRepo", opt.Checkout.Repo),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }


        public class OptionsWithDuplicateVerb
        {
            [Verb("add")]
            public AddVerb AddInfo { get; set; }


            [Verb("add")]
            public AddVerb AddInfo2 { get; set; }
        }



        [TestMethod]
        public void Parse_WithDuplicateVerbNames_ThrowsDuplicateVerbException()
        {
            var parser = new CliParser<OptionsWithDuplicateVerb>();
            var errs = parser.ValidateAttributeConfig();

            Assert.IsTrue(errs
                .OfType<DuplicateVerbException>()
                .Any());
        }


        public class OptionsWithNoDefaultVerbConstructor
        {
            [Verb("add")]
            public VerbWithNoDefaultConstructor AddInfo { get; set; }
        }

        public class VerbWithNoDefaultConstructor
        {
            public VerbWithNoDefaultConstructor(string fname)
            {
                Filename = fname;
            }

            [PositionalArgument(0)]
            public string Filename { get; set; }
        }

        [TestMethod]
        public void Parse_WithVerbHavingNoDefaultConstructor_ThrowsException()
        {
            var parser = new CliParser<OptionsWithNoDefaultVerbConstructor>();
            var errs = parser.ValidateAttributeConfig();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

        [TestMethod]
        public void Parse_WithFactoryAndVerbHavingNoDefaultConstructor_ParsesVerb()
        {
            const string expected = "myfile.txt";
            var opt = new OptionsWithNoDefaultVerbConstructor();
            var opts = new ParserSettings<OptionsWithNoDefaultVerbConstructor>
            {
                VerbFactory = new SimpleVerbfactory
                {
                    { typeof(VerbWithNoDefaultConstructor), () => new VerbWithNoDefaultConstructor("default.txt") }
                }
            };
            var parser = new CliParser<OptionsWithNoDefaultVerbConstructor>(opts);

            parser.Parse("add myfile.txt".Split(), opt);

            Assert.AreEqual(expected, opt.AddInfo.Filename);
        }

        public class OptionsWithVerbDefaultConstructor
        {
            [Verb("add")]
            public VerbWithDefaultConstructor AddInfo { get; set; }
        }

        public class VerbWithDefaultConstructor
        {
            [PositionalArgument(0)]
            public string Filename { get; set; }
        }

        [TestMethod]
        public void Parse_WithVerbHavingDefaultConstructor_ParsesVerb()
        {
            const string expected = "myfile.txt";
            var opt = new OptionsWithVerbDefaultConstructor();
            var parser = new CliParser<OptionsWithVerbDefaultConstructor>();

            parser.Parse("add myfile.txt".Split(), opt);

            Assert.AreEqual(expected, opt.AddInfo.Filename);
        }

        public class OptionsWithGitVerbs
        {
            [Verb]
            public GitAdd Add { get; set; }

            [Verb]
            public GitCommit Commit { get; set; }
        }

        public class GitAdd
        {
            [PositionalArgument(0, Constraint = NumArgsConstraint.AtLeast, NumArgs = 1)]
            public IEnumerable<string> Files { get; set; }
        }

        public class GitCommit
        {
            public GitCommit(string defaultCommitMessage)
            {
                CommitMessage = defaultCommitMessage;
            }

            [NamedArgument('m')]
            public string CommitMessage { get; set; }
        }

        [TestMethod]
        public void Parse_WithFactoryAndGitVerbsAdd_ParsesAddVerb()
        {
            string[] expected = { "myfile.txt", "otherfile.txt" };
            var opt = new OptionsWithGitVerbs();
            var opts = new ParserSettings<OptionsWithGitVerbs>
            {
                VerbFactory = new SimpleVerbfactory
                {
                    { () => new GitAdd() },
                    { typeof(GitCommit), () => new GitCommit("My default message") }
                }
            };
            var parser = new CliParser<OptionsWithGitVerbs>(opts);

            parser.Parse("add myfile.txt otherfile.txt".Split(), opt);
            var actual = opt.Add.Files.ToList();

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Parse_WithFactoryAndGitVerbsCommitAndNoMessage_ParsesCommitVerb()
        {
            const string expected = "My default message";
            var opt = new OptionsWithGitVerbs();
            var opts = new ParserSettings<OptionsWithGitVerbs>
            {
                VerbFactory = new SimpleVerbfactory
                {
                    { () => new GitAdd() },
                    { typeof(GitCommit), () => new GitCommit("My default message") }
                }
            };
            var parser = new CliParser<OptionsWithGitVerbs>(opts);

            parser.Parse("commit".Split(), opt);
            var actual = opt.Commit.CommitMessage;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Parse_WithFactoryAndGitVerbsCommitAndMessage_ParsesCommitVerb()
        {
            const string expected = "My message";
            var opt = new OptionsWithGitVerbs();
            var opts = new ParserSettings<OptionsWithGitVerbs>
            {
                VerbFactory = new SimpleVerbfactory
                {
                    { () => new GitAdd() },
                    { typeof(GitCommit), () => new GitCommit("My default message") }
                }
            };
            var parser = new CliParser<OptionsWithGitVerbs>(opts);

            parser.Parse(new[] { "commit", "-m", "My message" }, opt);
            var actual = opt.Commit.CommitMessage;

            Assert.AreEqual(expected, actual);
        }
    }
}
