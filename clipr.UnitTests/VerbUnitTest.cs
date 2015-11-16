using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var opt = CliParser.Parse<Options>("add file.txt".Split());

            Assert.AreEqual("file.txt", opt.AddInfo.Filename);
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
            var opt = CliParser.Parse<NamedOptions>("-v add file.txt".Split());

            Assert.IsTrue(opt.Verbose);
        }

        [TestMethod]
        public void Parse_WithNamedArgumentAndVerbWithNamedArgument_LeavesVerbObjectNull()
        {
            var opt = CliParser.Parse<NamedOptions>("-v".Split());

            Assert.IsNull(opt.AddInfo);
        }

        [TestMethod]
        public void Parse_WithNamedArgumentAndVerbWithPositionalArgument_ParsesNamedArgumentsForVerbOptions()
        {
            var opt = CliParser.Parse<NamedOptions>("-v add file.txt".Split());

            Assert.AreEqual("file.txt", opt.AddInfo.Filename);
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
            var obj = CliParser.Parse<NestedVerb1>("a b Horace".Split());

            Assert.AreEqual("Horace", obj.Verb.Verb.Name);
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
            var opt = CliParser.Parse<VerbOuterWithAliases>("co myRepo".Split());

            Assert.AreEqual("myRepo", opt.Checkout.Repo);
        }

        [TestMethod]
        public void Parse_WithVerbTaggedWithTwoNamesAndSecondGivenAsArg_ParsesVerb()
        {
            var opt = CliParser.Parse<VerbOuterWithAliases>("checkout myRepo".Split());

            Assert.AreEqual("myRepo", opt.Checkout.Repo);
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
            AssertEx.ThrowsAggregateContaining<DuplicateVerbException>(() => 
                CliParser.Parse<OptionsWithDuplicateVerb>("add myfile.txt".Split()));
        }


        public class OptionsWithNoDefaultConstructor
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
            AssertEx.ThrowsAggregateContaining<ArgumentIntegrityException>(() =>
                CliParser.Parse<OptionsWithNoDefaultConstructor>("add myfile.txt".Split()),
                "parameterless or default constructor");
        }
    }
}
