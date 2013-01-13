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
            var opt = CliParser.Parse<OnePositionalArgument>("filename".Split());
            Assert.AreEqual("filename", opt.Input);
        }

        [TestMethod]
        public void Positional_WithNegativeNumber_ParsesAsNegativeNumber()
        {
            var opt = CliParser.Parse<OnePositionalArgument>("-1".Split());
            Assert.AreEqual("-1", opt.Input);
        }

        [TestMethod]
        public void Positional_WithValueBeginningWithHyphenForced_ParsesValueAsPositional()
        {
            var opt = CliParser.Parse<OnePositionalArgument>("-- -hyphen".Split());
            Assert.AreEqual("-hyphen", opt.Input);
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
            var opt = CliParser.Parse<TwoPositionalArguments>("input.txt output.txt".Split());
            Assert.IsTrue(opt.Input == "input.txt" && opt.Output == "output.txt");
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
            var opt = CliParser.Parse<VarargsLastPositionalArgument>(
                "input.txt out1.txt out2.txt".Split());
            Assert.IsTrue(new VarargsLastPositionalArgument.EqualityComparer().Equals(expected, opt));
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
            var opt = CliParser.Parse<VarargMaxTwo>("value".Split());
            CollectionAssert.AreEqual(expected, opt.Args);
        }

        [TestMethod]
        public void Positional_WithAtMostTwoValuesAndGivenTwoValues_AddsValues()
        {
            var expected = new List<string> { "value1", "value2" };
            var opt = CliParser.Parse<VarargMaxTwo>("value1 value2".Split());
            CollectionAssert.AreEqual(expected, opt.Args);
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void Positional_WithAtMostTwoValuesAndGivenThreeValues_ThrowsException()
        {
            CliParser.Parse<VarargMaxTwo>("value1 value2 value3".Split());
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
            var opt = CliParser.Parse<VarargExactlyTwo>("value1 value2".Split());
            CollectionAssert.AreEqual(expected, opt.Args);
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void Positional_WithExactlyTwoValuesAndGivenOneValue_ThrowsException()
        {
            CliParser.Parse<VarargExactlyTwo>("value1".Split());
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void Positional_WithExactlyTwoValuesAndGivenThreeValues_ThrowsException()
        {
            CliParser.Parse<VarargExactlyTwo>("value1 value2 value3".Split());
        }

        #endregion

        #region Argument with at least two values.

        internal class VarargAtLeastTwo
        {
            [PositionalArgument(0, NumArgs = 2, Constraint = NumArgsConstraint.AtLeast)]
            public List<string> Args { get; set; }
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void Positional_WithAtMostOneValueAndGivenOneValue_ThrowsException()
        {
            CliParser.Parse<VarargAtLeastTwo>("value1".Split());
        }

        [TestMethod]
        public void Positional_WithAtLeastOneValueAndGivenTwoValues_AddsValues()
        {
            var expected = new List<string> { "value1", "value2" };
            var opt = CliParser.Parse<VarargAtLeastTwo>("value1 value2".Split());
            CollectionAssert.AreEqual(expected, opt.Args);
        }

        [TestMethod]
        public void Positional_WithAtLeastOneValueAndGivenThreeValues_AddsValues()
        {
            var expected = new List<string> { "value1", "value2", "value3" };
            var opt = CliParser.Parse<VarargAtLeastTwo>("value1 value2 value3".Split());
            CollectionAssert.AreEqual(expected, opt.Args);
        }

        #endregion
    }
}
