using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace clipr.UnitTests
{
    [TestClass]
    public class PositionalArgumentUnitTest
    {
        #region Basic positional.

        public class OnePositionalArgument
        {
            [PositionalArgument(0)]
            public string Input { get; set; }
        }

        [TestMethod]
        public void Positional_WithOnePositionalArgument_ParsesArgument()
        {
            var result = CliParser.Parse<OnePositionalArgument>("filename".Split());
            result.Handle(
                opt => Assert.AreEqual("filename", opt.Input),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Positional_WithNegativeNumber_ParsesAsNegativeNumber()
        {
            var result = CliParser.Parse<OnePositionalArgument>("-1".Split());
            result.Handle(
                opt => Assert.AreEqual("-1", opt.Input),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Positional_WithPositionalArgumentAfterDoubleDash_ParsesPositionalArgument()
        {
            var result = CliParser.Parse<OnePositionalArgument>("-- filename".Split());
            result.Handle(
                opt => Assert.AreEqual("filename", opt.Input),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Positional_WithDoubleDashPositionalArgumentAfterDoubleDash_ParsesPositionalArgument()
        {
            var result = CliParser.Parse<OnePositionalArgument>("-- --".Split());
            result.Handle(
                opt => Assert.AreEqual("--", opt.Input),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Positional_WithPositionalArgumentThatLooksLikeLongNamedArgumentAfterDoubleDash_ParsesPositionalArgument()
        {
            var result = CliParser.Parse<OnePositionalArgument>("-- --argument".Split());
            result.Handle(
                opt => Assert.AreEqual("--argument", opt.Input),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Positional_WithPositionalArgumentThatLooksLikeShortNamedArgumentAfterDoubleDash_ParsesPositionalArgument()
        {
            var result = CliParser.Parse<OnePositionalArgument>("-- -o".Split());
            result.Handle(
                opt => Assert.AreEqual("-o", opt.Input),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Positional_WithValueBeginningWithHyphenForced_ParsesValueAsPositional()
        {
            var result = CliParser.Parse<OnePositionalArgument>("-- -hyphen".Split());
            result.Handle(
                opt => Assert.AreEqual("-hyphen", opt.Input),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }
        
        public class TwoPositionalArguments
        {
            [PositionalArgument(0)]
            public string Input { get; set; }

            [PositionalArgument(1)]
            public string Output { get; set; }
        }

        [TestMethod]
        public void Positional_WithTwoPositionalArguments_ParsesArgument()
        {
            var result = CliParser.Parse<TwoPositionalArguments>("input.txt output.txt".Split());
            result.Handle(
                opt => Assert.IsTrue(opt.Input == "input.txt" && opt.Output == "output.txt"),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        #endregion

        #region Argument with single value followed by argument with multiple values.

        public class VarargsLastPositionalArgument
        {
            [PositionalArgument(0)]
            public string Input { get; set; }

            [PositionalArgument(1, NumArgs = 2)]
            public List<string> Output { get; set; }

            public class EqualityComparer : IEqualityComparer<VarargsLastPositionalArgument>
            {

                public bool Equals(VarargsLastPositionalArgument x, VarargsLastPositionalArgument y)
                {
                    return x.Input == y.Input &&
                           x.Output.SequenceEqual(y.Output);
                }

                public int GetHashCode(VarargsLastPositionalArgument obj)
                {
                    return obj.Input.GetHashCode() +
                        obj.Output.GetHashCode();
                }
            }
        }

        [TestMethod]
        public void Positional_WithVarargsAsLastPositional_ParsesMultipleArgs()
        {
            var expected = new VarargsLastPositionalArgument
            {
                Input = "input.txt",
                Output = new List<string>
                {
                            "out1.txt",
                            "out2.txt"
                        }
            };
            var result = CliParser.Parse<VarargsLastPositionalArgument>(
                "input.txt out1.txt out2.txt".Split());
            result.Handle(
                opt => Assert.IsTrue(new VarargsLastPositionalArgument.EqualityComparer().Equals(expected, opt)),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        #endregion

        #region Argument with at most two values.

        internal class VarargMaxTwo
        {
            [PositionalArgument(0, NumArgs = 2, Constraint = NumArgsConstraint.AtMost)]
            public List<string> Args { get; set; }
        }

        [TestMethod]
        public void Positional_WithAtMostTwoValuesAndGivenOneValue_AddsValue()
        {
            var expected = new List<string> { "value" };
            var result = CliParser.Parse<VarargMaxTwo>("value".Split());
            result.Handle(
                opt => CollectionAssert.AreEqual(expected, opt.Args),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Positional_WithAtMostTwoValuesAndGivenTwoValues_AddsValues()
        {
            var expected = new List<string> { "value1", "value2" };
            var result = CliParser.Parse<VarargMaxTwo>("value1 value2".Split());
            result.Handle(
                opt => CollectionAssert.AreEqual(expected, opt.Args),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Positional_WithAtMostTwoValuesAndGivenThreeValues_ThrowsException()
        {
            var result = CliParser.Parse<VarargMaxTwo>("value1 value2 value3".Split());
            result.Handle(
                opt => Assert.Fail("Parse succeeded, but error was expected"),
                t => Assert.Fail("Trigger initiated, but error was expected."),
                errs => Assert.IsTrue(errs
                .OfType<ParseException>()
                .Any()));
        }

        #endregion

        #region Argument with exactly two values.

        internal class VarargExactlyTwo
        {
            [PositionalArgument(0, NumArgs = 2)]
            public List<string> Args { get; set; }
        }

        [TestMethod]
        public void Positional_WithExactlyTwoValuesAndGivenTwoValues_AddsValues()
        {
            var expected = new List<string> { "value1", "value2" };
            var result = CliParser.Parse<VarargExactlyTwo>("value1 value2".Split());
            result.Handle(
                opt => CollectionAssert.AreEqual(expected, opt.Args),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Positional_WithExactlyTwoValuesAndGivenOneValue_ThrowsException()
        {
            var result = CliParser.Parse<VarargExactlyTwo>("value1".Split());
            result.Handle(
                opt => Assert.Fail("Parse succeeded, but error was expected"),
                t => Assert.Fail("Trigger initiated, but error was expected."),
                errs => Assert.IsTrue(errs
                .OfType<ParseException>()
                .Any()));
        }

        [TestMethod]
        public void Positional_WithExactlyTwoValuesAndGivenThreeValues_ThrowsException()
        {
            var result = CliParser.Parse<VarargExactlyTwo>("value1 value2 value3".Split());
            result.Handle(
                opt => Assert.Fail("Parse succeeded, but error was expected"),
                t => Assert.Fail("Trigger initiated, but error was expected."),
                errs => Assert.IsTrue(errs
                .OfType<ParseException>()
                .Any()));
        }

        #endregion

        #region Argument with at least two values.

        internal class VarargAtLeastTwo
        {
            [PositionalArgument(0, NumArgs = 2, Constraint = NumArgsConstraint.AtLeast)]
            public List<string> Args { get; set; }
        }

        [TestMethod]
        public void Positional_WithAtLeastTwoValuesAndGivenOneValue_ThrowsException()
        {
            var result = CliParser.Parse<VarargAtLeastTwo>("value1".Split());
            result.Handle(
                opt => Assert.Fail("Parse succeeded, but error was expected"),
                t => Assert.Fail("Trigger initiated, but error was expected."),
                errs => Assert.IsTrue(errs
                .OfType<ParseException>()
                .Any()));
        }

        [TestMethod]
        public void Positional_WithAtLeastTwoValuesAndGivenTwoValues_AddsValues()
        {
            var expected = new List<string> { "value1", "value2" };
            var result = CliParser.Parse<VarargAtLeastTwo>("value1 value2".Split());
            result.Handle(
                opt => CollectionAssert.AreEqual(expected, opt.Args),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Positional_WithAtLeastTwoValuesAndGivenThreeValues_AddsValues()
        {
            var expected = new List<string> { "value1", "value2", "value3" };
            var result = CliParser.Parse<VarargAtLeastTwo>("value1 value2 value3".Split());
            result.Handle(
                opt => CollectionAssert.AreEqual(expected, opt.Args),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Positional_WithAtLeastTwoValuesAndGivenPositionalDelimter_AddsValues()
        {
            var expected = new List<string> { "value1", "--value2", "value3" };
            var result = CliParser.Parse<VarargAtLeastTwo>("-- value1 --value2 value3".Split());
            result.Handle(
                opt => CollectionAssert.AreEqual(expected, opt.Args),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }
        
        #endregion

        #region One positional argument with multiple values and another with variable values.

        internal class PositionalMultipleValues
        {
            [PositionalArgument(0, NumArgs = 3)]
            public List<int> Votes { get; set; }

            [PositionalArgument(1, Constraint = NumArgsConstraint.AtLeast)]
            public List<string> Names { get; set; } 
        }

        [TestMethod]
        public void PositionalArguments_WithFirstArgumentHavingMultipleValues_ParsesCorrectly()
        {
            var expectedVotes = new List<int> {10, 6, 8};
            var expectedNames = new List<string> {"Nancy", "Rick", "Tim"};
            var result = CliParser.Parse<PositionalMultipleValues>(
                "10 6 8 Nancy Rick Tim".Split());
            result.Handle(
                opt =>
                {
                    CollectionAssert.AreEqual(expectedVotes, opt.Votes);
                    CollectionAssert.AreEqual(expectedNames, opt.Names);
                },
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));

        }

        #endregion

        #region Positional arguments with lower bound of zero

        internal class PositionalArgumentLowerBoundCountEqualsZero
        {
            [PositionalArgument(0, NumArgs = 0, Constraint = NumArgsConstraint.AtLeast)]
            public List<string> Args { get; set; }
        }

        [TestMethod]
        public void PositionalArgument_WithLowerBoundCountEqualsZero_ParsesNoArguments1886105196()
        {
            var opts = new PositionalArgumentLowerBoundCountEqualsZero();
            var arguments = new string[0];
            var parser = new CliParser<PositionalArgumentLowerBoundCountEqualsZero>();

            parser.Parse(arguments, opts);

            Assert.AreEqual(0, opts.Args.Count);
        }

        [TestMethod]
        public void PositionalArgument_WithLowerBoundCountEqualsZero_ParsesAllArguments445959719()
        {
            var opts = new PositionalArgumentLowerBoundCountEqualsZero();
            var arguments = new[] { "first", "second" };
            var parser = new CliParser<PositionalArgumentLowerBoundCountEqualsZero>();

            parser.Parse(arguments, opts);

            Assert.AreEqual(2, opts.Args.Count);
        }

        internal class MultiplePositionalArgumentLowerBoundCountEqualsZero
        {
            [PositionalArgument(0)]
            public string FixedArg { get; set; }

            [PositionalArgument(1, NumArgs = 0, Constraint = NumArgsConstraint.AtLeast)]
            public List<string> Args { get; set; }
        }

        [TestMethod]
        public void PositionalArgument_WithMultiplePositionalArgumentsAndLowerBoundCountEqualsZero_ParsesOtherArguments2128719744()
        {
            var opts = new MultiplePositionalArgumentLowerBoundCountEqualsZero();
            var arguments = new[] { "fixed" };
            var parser = new CliParser<MultiplePositionalArgumentLowerBoundCountEqualsZero>();

            parser.Parse(arguments, opts);

            Assert.AreEqual(0, opts.Args.Count);
        }

        [TestMethod]
        public void PositionalArgument_WithMultiplePositionalArgumentsAndLowerBoundCountEqualsZero_ParsesAllArguments158709799()
        {
            var opts = new MultiplePositionalArgumentLowerBoundCountEqualsZero();
            var arguments = new[] { "fixed", "first", "second" };
            var parser = new CliParser<MultiplePositionalArgumentLowerBoundCountEqualsZero>();

            parser.Parse(arguments, opts);

            Assert.AreEqual(2, opts.Args.Count);
        }

        #endregion
    }
}
