using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace clipr.UnitTests
{
    [TestClass]
    public class PostParseUnitTest
    {
        internal class ParserWithPostParse
        {
            [PositionalArgument(0)]
            public string Arg { get; set; }

            public bool PostParseRun { get; set; }

            [PostParse]
            public void SetPostParse()
            {
                PostParseRun = Arg != null;
            }
        }

        [TestMethod]
        public void PostParse_WithASinglePostParseMethod_RunsTheMethodAfterParsing()
        {
            var result = CliParser.Parse<ParserWithPostParse>("nothing".Split());
            result.Handle(
                opt => Assert.IsTrue(opt.PostParseRun),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        internal class PostParseWithParameters
        {
            [PositionalArgument(0)]
            public string Arg { get; set; }

            public int PostParseRun { get; set; }

            [PostParse]
            public void SetPostParse(int myFavoriteInt)
            {
                PostParseRun = myFavoriteInt;
            }
        }

        [TestMethod]
        public void PostParse_WithPostParseParametersAndDefaultInjector_ReturnsError()
        {
            var parser = new CliParser<PostParseWithParameters>();
            var errs = parser.ValidateAttributeConfig();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

        [TestMethod]
        public void PostParse_WithPostParseParametersAndSimpleInjector_InjectsObject()
        {
            const int expected = 10;
            var injector = new IOC.SimpleObjectFactory
            {
                {typeof(int), () => expected }
            };
            var settings = new ParserSettings<PostParseWithParameters>
            {
                PostParseDependencyFactory = injector
            };

            var opts = new PostParseWithParameters();
            var parser = new CliParser<PostParseWithParameters>(settings);
            var result = parser.Parse("nothing".Split(), opts);
            result.Handle(
                opt => Assert.IsTrue(opt.PostParseRun == expected),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail(
                    String.Format("Error parsing arguments. {0}", e.First())));
        }
    }
}
