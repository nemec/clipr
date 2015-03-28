using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace clipr.UnitTests
{
    [TestClass]
    public class NamedArgumentUnitTest
    {
        internal class NamedArgumentCount
        {
            [NamedArgument('v', "verbose", Action = ParseAction.Count)]
            public int Verbosity { get; set; }
        }

        [TestMethod]
        public void Argument_WithParseActionCount_AccumulatesConsecutiveArguments()
        {
            var opt = CliParser.Parse<NamedArgumentCount>("-vvv".Split());
            Assert.AreEqual(3, opt.Verbosity);
        }

        [TestMethod]
        public void Argument_WithParseActionCount_AccumulatesSeparateArguments()
        {
            var opt = CliParser.Parse<NamedArgumentCount>("-v -v -v".Split());
            Assert.AreEqual(3, opt.Verbosity);
        }

        [TestMethod]
        public void Argument_WithParseActionCount_AccumulatesShortAndLongArguments()
        {
            var opt = CliParser.Parse<NamedArgumentCount>("-v --verbose".Split());
            Assert.AreEqual(2, opt.Verbosity);
        }

        internal class NamedArgumentWithOption
        {
            [NamedArgument('n', "name")]
            public string Name { get; set; }
        }

        [TestMethod]
        public void Argument_WithLongOptionAndProvidedWithEqualsSign_StoresValue()
        {
            var opt = CliParser.Parse<NamedArgumentWithOption>("--name=tim".Split());
            Assert.AreEqual("tim", opt.Name);
        }

        [TestMethod]
        public void Argument_WithLongOption_StoresValue()
        {
            var opt = CliParser.Parse<NamedArgumentWithOption>("--name tim".Split());
            Assert.AreEqual("tim", opt.Name);
        }

        [TestMethod]
        public void Argument_WithShortOption_StoresValue()
        {
            var opt = CliParser.Parse<NamedArgumentWithOption>("-n tim".Split());
            Assert.AreEqual("tim", opt.Name);
        }

        [TestMethod]
        public void Argument_WithShortOptionAndNoSpace_StoresValue()
        {
            var opt = CliParser.Parse<NamedArgumentWithOption>("-ntim".Split());
            Assert.AreEqual("tim", opt.Name);
        }

        internal class OptionGroup
        {
            [NamedArgument('a', Action = ParseAction.StoreTrue)]
            public bool A { get; set; }

            [NamedArgument('b', Action = ParseAction.StoreTrue)]
            public bool B { get; set; }

            [NamedArgument('c', Action = ParseAction.StoreTrue)]
            public bool C { get; set; }

        }

        [TestMethod]
        public void Argument_WithOptionGroup_ParsesAllOptions()
        {
            var opt = CliParser.Parse<OptionGroup>("-abc".Split());
            Assert.IsTrue(opt.A && opt.B && opt.C);
        }

        internal class NegativeValue
        {
            [NamedArgument('n')]
            public int Value { get; set; }
        }

        [TestMethod]
        public void Argument_WithNegativeNumber_ParsesNumberAsValue()
        {
            var opt = CliParser.Parse<NegativeValue>("-n -1".Split());
            Assert.AreEqual(-1, opt.Value);
        }

        [TestMethod]
        public void Argument_WithNegativeNumberAndNoSpace_ParsesNumberAsValue()
        {
            var opt = CliParser.Parse<NegativeValue>("-n-1".Split());
            Assert.AreEqual(-1, opt.Value);
        }

        internal class PositionalNegative
        {
            [PositionalArgument(0)]
            public int Value { get; set; }
        }

        [TestMethod]
        public void Argument_WithPositionalNegativeNumber_ParsesNumberAsValue()
        {
            var opt = CliParser.Parse<PositionalNegative>("-1".Split());
            Assert.AreEqual(-1, opt.Value);
        }

        internal class NamedArgumentWithHyphen
        {
            [NamedArgument("full-name")]
            public string Name { get; set; }
        }

        [TestMethod]
        public void Argument_WithHyphenInNamedArgument_ParsesArgumentValue()
        {
            var opt = CliParser.Parse<NamedArgumentWithHyphen>("--full-name tim".Split());
            Assert.AreEqual("tim", opt.Name);
        }

        internal class NamedArgumentWithRequired
        {
            [NamedArgument('c', "celery", Required = true)]
            public string Celery { get; set; }
        }

        [TestMethod]
        public void Argument_WithRequiredLongNamedArgumentProvided_ParsesArgumentValue()
        {
            var opt = CliParser.Parse<NamedArgumentWithRequired>("--celery puppy".Split());
            Assert.AreEqual("puppy", opt.Celery);
        }

        [TestMethod]
        public void Argument_WithRequiredShortNamedArgumentProvided_ParsesArgumentValue()
        {
            var opt = CliParser.Parse<NamedArgumentWithRequired>("-c puppy".Split());
            Assert.AreEqual("puppy", opt.Celery);
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void Argument_WithRequiredNamedArgumentMissing_ThrowsException()
        {
            CliParser.Parse<NamedArgumentWithRequired>(new string[0]);
        }

        internal class MutuallyExclusive
        {
            [NamedArgument('a')]
            [MutuallyExclusiveGroup("something", Required = true)]
            public string A { get; set; }

            [NamedArgument('b')]
            [MutuallyExclusiveGroup("something")]
            public string B { get; set; }

            [NamedArgument('c')]
            public string C { get; set; }
        }

        [TestMethod]
        public void MutuallyExclusive_ArgumentsDoNotViolateExclusivity_ParsesSuccessfully()
        {
            var opt = CliParser.Parse<MutuallyExclusive>("-a rudolph -c other".Split());
            Assert.AreEqual("rudolph", opt.A);
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void MutuallyExclusive_ArgumentsViolateExclusivity_ThrowsParseException()
        {
            CliParser.Parse<MutuallyExclusive>("-a one -b two -c other".Split());
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void MutuallyExclusive_ArgumentsDoNotIncludeRequiredGroup_ThrowsParseException()
        {
            CliParser.Parse<MutuallyExclusive>("-c three".Split());
        }

        internal class NamedArgumentLowerBoundCountEqualsZero
        {
            public NamedArgumentLowerBoundCountEqualsZero()
            {
                Args = new List<string>();
            }

            [NamedArgument('s', NumArgs = 0, Constraint = NumArgsConstraint.AtLeast)]
            public List<string> Args { get; set; }
        }

        [TestMethod]
        public void NamedArgument_WithLowerBoundCountEqualsZero_ParsesNoArguments()
        {
            var opts = new NamedArgumentLowerBoundCountEqualsZero();
            var arguments = new string[0];
            var parser = new CliParser<NamedArgumentLowerBoundCountEqualsZero>(opts);

            parser.Parse(arguments);

            Assert.AreEqual(0, opts.Args.Count);
        }

        [TestMethod]
        public void NamedArgument_WithLowerBoundCountEqualsZero_ParsesAllArguments()
        {
            var opts = new NamedArgumentLowerBoundCountEqualsZero();
            var arguments = new[] { "-s", "first", "second" };
            var parser = new CliParser<NamedArgumentLowerBoundCountEqualsZero>(opts);

            parser.Parse(arguments);

            Assert.AreEqual(2, opts.Args.Count);
        }
    }
}
