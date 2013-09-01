using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using clipr.Utils;

// ReSharper disable ObjectCreationAsStatement
// ReSharper disable UseObjectOrCollectionInitializer

namespace clipr.UnitTests
{
    [TestClass]
    public class ArgumentIntegrityUnitTest
    {
        #region Duplicate argument check.

        internal class DuplicateArgumentWhenCaseInsensitive
        {
            [NamedArgument('n')]
            public string Name { get; set; }

            [NamedArgument('N')]
            public string Neighbor { get; set; }
        }

        [TestMethod]
        public void ParseArgument_WithArgumentsOfDifferingCaseWhenCaseSensitive_ParsesArguments()
        {
            var opt = CliParser.Parse<DuplicateArgumentWhenCaseInsensitive>(
                "-n tim -N robert".Split());
            Assert.AreEqual("tim", opt.Name);
            Assert.AreEqual("robert", opt.Neighbor);
        }

        [TestMethod]
        public void ParseArgument_WithArgumentsOfDifferingCaseWhenCaseInsensitive_ThrowsException()
        {
            var opt = new DuplicateArgumentWhenCaseInsensitive();
            var parser = new CliParser<DuplicateArgumentWhenCaseInsensitive>(
                opt, ParserOptions.CaseInsensitive);

            AssertEx.Throws<DuplicateArgumentException>(() => parser.Parse("-n tim -n robert".Split()));
        }

        internal class DuplicateArguments
        {
            [NamedArgument('n')]
            public string Name { get; set; }

            [NamedArgument('n')]
            public string Neighbor { get; set; }
        }

        [TestMethod]
        public void ParseArgument_WithDuplicateArguments_ThrowsException()
        {
            var parser = new CliParser<DuplicateArguments>(new DuplicateArguments());
            AssertEx.Throws<DuplicateArgumentException>(
                () => parser.Parse("-n orange".Split()));
        }

        #endregion

        #region Mutually Exclusive check.

        internal class MutuallyExclusiveArgumentAttributes
        {
            [NamedArgument('n')]
            [PositionalArgument(0)]
            public string Name { get; set; }
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void ParseArguments_WithMultipleArgumentAttributes_ThrowsIntegrityException()
        {
            new CliParser<MutuallyExclusiveArgumentAttributes>(
                new MutuallyExclusiveArgumentAttributes());
        }

        #endregion

        #region NumArgs cannot equal zero.

        internal class ArgumentCountLessThanOne
        {
            [NamedArgument('s', NumArgs = 0)]
            public List<string> Args { get; set; }
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void Argument_WithCountLessThanOneThrows_ExceptionOnInitialize()
        {
            new CliParser<ArgumentCountLessThanOne>(
                new ArgumentCountLessThanOne());
        }

        #endregion

        #region Last positional has varargs only.

        internal class MultiplePositionalArgsWithMultipleValues
        {
            [PositionalArgument(0, NumArgs = 1, Constraint = NumArgsConstraint.AtLeast)]
            public IEnumerable<string> Listy { get; set; }

            [PositionalArgument(1, NumArgs = 2)]
            public IEnumerable<string> Listy2 { get; set; }
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void PositionalArgument_WithMultiplePositionalArgumentsAndFirstHasVarArgs_FailsValidation()
        {
            new CliParser<MultiplePositionalArgsWithMultipleValues>(
                new MultiplePositionalArgsWithMultipleValues());
        }

        #endregion

        #region Append/AppendConst implements IEnumerable<>.

        internal class ParseActionAppend
        {
            [PositionalArgument(0, Action = ParseAction.Append)]
            public string Name { get; set; }
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void Argument_WithParseActionAppendAndDoesNotImplementIEnumerable_FailsValidation()
        {
            new CliParser<ParseActionAppend>(new ParseActionAppend());
        }

        internal class ParseActionAppendConst
        {
            [PositionalArgument(0, Action = ParseAction.AppendConst)]
            public string Name { get; set; }
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void Argument_WithParseActionAppendConstAndDoesNotImplementIEnumerable_FailsValidation()
        {
            new CliParser<ParseActionAppendConst>(new ParseActionAppendConst());
        }

        #endregion

        #region Count implements int.

        internal class ParseActionCount
        {
            [PositionalArgument(0, Action = ParseAction.Append)]
            public string Name { get; set; }
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void Argument_WithParseActionCountAndDoesNotImplementInt_FailsValidation()
        {
            new CliParser<ParseActionCount>(new ParseActionCount());
        }

        #endregion

        #region Short name has valid characters.

        internal class InvalidShortName
        {
            [NamedArgument('.')]
            public string A { get; set; }
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void Argument_WithInvalidCharacterAsShortName_FailsValidation()
        {
            new CliParser<InvalidShortName>(new InvalidShortName());
        }

        #endregion

        #region Long name has valid characters.

        internal class InvalidLongName
        {
            [NamedArgument("nano.bot")]
            public string A { get; set; }
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void Argument_WithInvalidCharacterAsLongName_FailsValidation()
        {
            new CliParser<InvalidLongName>(new InvalidLongName());
        }

        #endregion

        #region Const value's type convertible to property type.

        internal class ConstWithWrongType
        {
            [NamedArgument('a', Action = ParseAction.StoreConst, Const = "rah")]
            public int Age { get; set; }
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void Argument_WithConstValueAsWrongType_FailsValidation()
        {
            new CliParser<ConstWithWrongType>(new ConstWithWrongType());
        }

        #endregion

        #region Positional arguments cannot store const.

        internal class PositionalArgumentWithConst
        {
            [PositionalArgument(0, Action = ParseAction.StoreConst, Const = "tim")]
            public string Name { get; set; }
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void PositionalArgument_WithConstValue_FailsValidation()
        {
            new CliParser<PositionalArgumentWithConst>(new PositionalArgumentWithConst());
        }

        #endregion

        #region Const True/False must be bool.

        internal class ConstTrueWithWrongType
        {
            [PositionalArgument(0, Action = ParseAction.StoreTrue)]
            public string Name { get; set; }
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void Argument_WithConstTrueWithWrongType_FailsValidation()
        {
            new CliParser<ConstTrueWithWrongType>(new ConstTrueWithWrongType());
        }

        internal class ConstFalseWithWrongType
        {
            [PositionalArgument(0, Action = ParseAction.StoreFalse)]
            public string Name { get; set; }
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void Argument_WithConstFalseWithWrongType_FailsValidation()
        {
            new CliParser<ConstFalseWithWrongType>(new ConstFalseWithWrongType());
        }

        #endregion

        #region Help short name validity.

        [TestMethod]
        public void Help_WithInvalidShortName_ThrowsException()
        {
            var help = new Usage.AutomaticHelpGenerator<object>();
            help.ShortName = '.';

            AssertEx.ThrowsAggregateContaining<ArgumentIntegrityException>(
                            () => new CliParser<object>(new object(), help));
        }

        [TestMethod]
        public void Help_WithInvalidShortNameAsDigit_ThrowsException()
        {
            var help = new Usage.AutomaticHelpGenerator<object>();
            help.ShortName = '1';

            AssertEx.ThrowsAggregateContaining<ArgumentIntegrityException>(
                            () => new CliParser<object>(new object(), help));
        }

        #endregion

        #region Help long name validity.

        [TestMethod]
        public void Help_WithInvalidLongName_ThrowsException()
        {
            var help = new Usage.AutomaticHelpGenerator<object>();
            help.LongName = "no.thing";

            AssertEx.ThrowsAggregateContaining<ArgumentIntegrityException>(
                            () => new CliParser<object>(new object(), help));
        }

        [TestMethod]
        public void Help_WithInvalidLongNameLength_ThrowsException()
        {
            var help = new Usage.AutomaticHelpGenerator<object>();
            help.LongName = "n";

            AssertEx.ThrowsAggregateContaining<ArgumentIntegrityException>(
                () => new CliParser<object>(new object(), help));
        }

        [TestMethod]
        public void Help_WithInvalidLongNameEndingWithDash_ThrowsException()
        {
            var help = new Usage.AutomaticHelpGenerator<object>();
            help.LongName = "none-";

            AssertEx.ThrowsAggregateContaining<ArgumentIntegrityException>(
                () => new CliParser<object>(new object(), help));
        }

        [TestMethod]
        public void Help_WithInvalidLongNameBeginningWithDigit_ThrowsException()
        {
            var help = new Usage.AutomaticHelpGenerator<object>();
            help.LongName = "1none";

            AssertEx.ThrowsAggregateContaining<ArgumentIntegrityException>(() =>
            {
                new CliParser<object>(new object(), help);
            });
        }

        #endregion

        #region Version short name validity.

        [TestMethod]
        [Ignore]
        [ExpectedException(typeof(ArgumentIntegrityException))]
        public void Version_WithInvalidShortName_ThrowsException()
        {
            var help = new Usage.AutomaticHelpGenerator<object>();
            //help.Version.ShortName = '.';

            AssertEx.ThrowsAggregateContaining<ArgumentIntegrityException>(
                () => new CliParser<object>(new object(), help));
        }

        [TestMethod]
        [Ignore]
        [ExpectedException(typeof(ArgumentIntegrityException))]
        public void Version_WithInvalidShortNameAsDigit_ThrowsException()
        {
            var help = new Usage.AutomaticHelpGenerator<object>();
            //help.Version.ShortName = '1';

            AssertEx.ThrowsAggregateContaining<ArgumentIntegrityException>(() =>
            {
                new CliParser<object>(new object(), help);
            });
        }

        #endregion

        #region Version long name validity.

        [TestMethod]
        [Ignore]
        [ExpectedException(typeof(ArgumentIntegrityException))]
        public void Version_WithInvalidLongName_ThrowsException()
        {
            var help = new Usage.AutomaticHelpGenerator<object>();
            //help.Version.LongName = "no.thing";

            new CliParser<object>(new object(), help);
        }

        [TestMethod]
        [Ignore]
        [ExpectedException(typeof(ArgumentIntegrityException))]
        public void Version_WithInvalidLongNameLength_ThrowsException()
        {
            var help = new Usage.AutomaticHelpGenerator<object>();
            //help.Version.LongName = "n";

            new CliParser<object>(new object(), help);
        }

        [TestMethod]
        [Ignore]
        [ExpectedException(typeof(ArgumentIntegrityException))]
        public void Version_WithInvalidLongNameEndingWithDash_ThrowsException()
        {
            var help = new Usage.AutomaticHelpGenerator<object>();
            //help.Version.LongName = "none-";

            new CliParser<object>(new object(), help);
        }

        [TestMethod]
        [Ignore]
        [ExpectedException(typeof(ArgumentIntegrityException))]
        public void Version_WithInvalidLongNameBeginningWithDigit_ThrowsException()
        {
            var help = new Usage.AutomaticHelpGenerator<object>();
            //help.Version.LongName = "1none";

            new CliParser<object>(new object(), help);
        }

        #endregion

        #region Config cannot contain both Positional and Verbs

        internal class PositionalAndVerbOptions
        {
            [PositionalArgument(0)]
            public string Name { get; set; }

            [Verb("sub")]
            public PositionalAndVerbSubOptions SubOptions { get; set; }
        }

        internal class PositionalAndVerbSubOptions
        {
            public string Age { get; set; }
        }

        [TestMethod]
        [ExpectedException(typeof (AggregateException))]
        public void CheckIntegrity_WithConfigContainingBothPositionalAndVerbs_ThrowsException()
        {
            new CliParser<PositionalAndVerbOptions>(
                new PositionalAndVerbOptions());
        }

        #endregion
    }
}
