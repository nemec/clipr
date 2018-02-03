using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

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
            var result = CliParser.Parse<NamedArgumentCount>("-vvv".Split());
            result.Handle(
                opt => Assert.AreEqual(3, opt.Verbosity),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Argument_WithParseActionCount_AccumulatesSeparateArguments()
        {
            var result = CliParser.Parse<NamedArgumentCount>("-v -v -v".Split());
            result.Handle(
                opt => Assert.AreEqual(3, opt.Verbosity),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Argument_WithParseActionCount_AccumulatesShortAndLongArguments()
        {
            var result = CliParser.Parse<NamedArgumentCount>("-v --verbose".Split());
            result.Handle(
                opt => Assert.AreEqual(2, opt.Verbosity),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        internal class NamedArgumentWithOption
        {
            [NamedArgument('n', "name")]
            public string Name { get; set; }
        }

        [TestMethod]
        public void Argument_WithLongOptionAndProvidedWithEqualsSign_StoresValue()
        {
            var result = CliParser.Parse<NamedArgumentWithOption>("--name=tim".Split());
            result.Handle(
                opt => Assert.AreEqual("tim", opt.Name),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Argument_WithLongOption_StoresValue()
        {
            var result = CliParser.Parse<NamedArgumentWithOption>("--name tim".Split());
            result.Handle(
                opt => Assert.AreEqual("tim", opt.Name),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Argument_WithShortOption_StoresValue()
        {
            var result = CliParser.Parse<NamedArgumentWithOption>("-n tim".Split());
            result.Handle(
                opt => Assert.AreEqual("tim", opt.Name),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Argument_WithShortOptionAndNoSpace_StoresValue()
        {
            var result = CliParser.Parse<NamedArgumentWithOption>("-ntim".Split());
            result.Handle(
                opt => Assert.AreEqual("tim", opt.Name),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
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
            var result = CliParser.Parse<OptionGroup>("-abc".Split());
            result.Handle(
                opt => Assert.IsTrue(opt.A && opt.B && opt.C),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        internal class NegativeValue
        {
            [NamedArgument('n')]
            public int Value { get; set; }
        }

        [TestMethod]
        public void Argument_WithNegativeNumber_ParsesNumberAsValue()
        {
            var result = CliParser.Parse<NegativeValue>("-n -1".Split());
            result.Handle(
                opt => Assert.AreEqual(-1, opt.Value),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Argument_WithNegativeNumberAndNoSpace_ParsesNumberAsValue()
        {
            var result = CliParser.Parse<NegativeValue>("-n-1".Split());
            result.Handle(
                opt => Assert.AreEqual(-1, opt.Value),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        internal class PositionalNegative
        {
            [PositionalArgument(0)]
            public int Value { get; set; }
        }

        [TestMethod]
        public void Argument_WithPositionalNegativeNumber_ParsesNumberAsValue()
        {
            var result = CliParser.Parse<PositionalNegative>("-1".Split());
            result.Handle(
                opt => Assert.AreEqual(-1, opt.Value),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        internal class NamedArgumentWithHyphen
        {
            [NamedArgument("full-name")]
            public string Name { get; set; }
        }

        [TestMethod]
        public void Argument_WithHyphenInNamedArgument_ParsesArgumentValue()
        {
            var result = CliParser.Parse<NamedArgumentWithHyphen>("--full-name tim".Split());
            result.Handle(
                opt => Assert.AreEqual("tim", opt.Name),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        internal class NamedArgumentWithRequired
        {
            [NamedArgument('c', "celery", Required = true)]
            public string Celery { get; set; }
        }

        [TestMethod]
        public void Argument_WithRequiredLongNamedArgumentProvided_ParsesArgumentValue()
        {
            var result = CliParser.Parse<NamedArgumentWithRequired>("--celery puppy".Split());
            result.Handle(
                opt => Assert.AreEqual("puppy", opt.Celery),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Argument_WithRequiredShortNamedArgumentProvided_ParsesArgumentValue()
        {
            var result = CliParser.Parse<NamedArgumentWithRequired>("-c puppy".Split());
            result.Handle(
                opt => Assert.AreEqual("puppy", opt.Celery),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Argument_WithRequiredNamedArgumentMissing_ThrowsException()
        {
            var result = CliParser.Parse<NamedArgumentWithRequired>(new string[0]);
            result.Handle(
                opt => Assert.Fail("Parse returned a value when it should have returned an error"),
                t => Assert.Fail("Trigger {0} executed.", t),
                errs => Assert.IsTrue(errs
                    .OfType<ParseException>()
                    .Any()));
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
            var parser = new CliParser<NamedArgumentLowerBoundCountEqualsZero>();

            parser.Parse(arguments, opts);

            Assert.AreEqual(0, opts.Args.Count);
        }

        [TestMethod]
        public void NamedArgument_WithLowerBoundCountEqualsZero_ParsesAllArguments()
        {
            var opts = new NamedArgumentLowerBoundCountEqualsZero();
            var arguments = new[] { "-s", "first", "second" };
            var parser = new CliParser<NamedArgumentLowerBoundCountEqualsZero>();

            parser.Parse(arguments, opts);  // TODO a factory instead of an instance?

            Assert.AreEqual(2, opts.Args.Count);
        }

        internal class NamedArgumentWithPromptNoMasking
        {
            [PromptIfValueMissing]
            [NamedArgument('p', "password")]
            public string Password { get; set; }
        }

        internal class NamedArgumentWithPromptWithMasking
        {
            [PromptIfValueMissing(MaskInput = true)]
            [NamedArgument('p', "password")]
            public string Password { get; set; }
        }

        [Ignore]
        [TestMethod]
        public void Argument_WithLongOptionAndPromptNoMasking_GetsValueFromStdin()
        {
            // TODO inject pw into stdin
            //const string password = "Changeme123";
            var result = CliParser.Parse<NamedArgumentWithPromptNoMasking>("--password".Split());
            result.Handle(
                opt => Assert.AreEqual("Changeme123", opt.Password),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [Ignore]
        [TestMethod]
        public void Argument_WithLongOptionAndPromptWithMasking_GetsValueFromStdin()
        {
            // TODO inject pw into stdin
            // TODO verify stdout doesn't contain password?
            //const string password = "Changeme123";
            var result = CliParser.Parse<NamedArgumentWithPromptWithMasking>("--name=tim".Split());
            result.Handle(
                opt => Assert.AreEqual("Changeme123", opt.Password),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [Ignore]
        [TestMethod]
        public void Argument_WithLongOptionAndPromptWithMasking_ActuallyMasksDisplay()
        {
            // TODO inject pw into stdin
            // TODO verify stdout doesn't contain password?
            //const string password = "Changeme123";
            var result = CliParser.Parse<NamedArgumentWithPromptWithMasking>("--name=tim".Split());
            result.Handle(
                opt => Assert.AreEqual("Changeme123", opt.Password),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }
    }
}
