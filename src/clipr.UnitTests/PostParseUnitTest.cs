using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
