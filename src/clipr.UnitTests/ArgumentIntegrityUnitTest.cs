using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Text;

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
            var result = CliParser.Parse<DuplicateArgumentWhenCaseInsensitive>(
                "-n tim -N robert".Split());
            result.Handle(
                opt =>
                {
                    Assert.AreEqual("tim", opt.Name);
                    Assert.AreEqual("robert", opt.Neighbor);
                },
                trig =>
                {
                    Assert.Fail(trig.ToString());
                },
                errs =>
                {
                    var sb = new StringBuilder();
                    foreach (var err in errs)
                    {
                        sb.AppendLine(err.ToString());
                    }
                    Assert.Fail(sb.ToString());
                });
            
        }

        [TestMethod]
        public void ParseArgument_WithArgumentsOfDifferingCaseWhenCaseInsensitive_ThrowsException()
        {
            var parser = new CliParser<DuplicateArgumentWhenCaseInsensitive>(
                new ParserSettings<DuplicateArgumentWhenCaseInsensitive> { CaseInsensitive = true });
            var errs = parser.PerformAttributeIntegrityCheck();
            
            Assert.IsTrue(errs
                .OfType<DuplicateArgumentException>()
                .Any());
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
            var parser = new CliParser<DuplicateArguments>();
            var errs = parser.PerformAttributeIntegrityCheck();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
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
        public void ParseArguments_WithMultipleArgumentAttributes_ThrowsIntegrityException()
        {
            var parser = new CliParser<MutuallyExclusiveArgumentAttributes>();
            var errs = parser.PerformAttributeIntegrityCheck();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

#endregion

#region NumArgs cannot equal zero unless it's the lower bound

        internal class NamedArgumentExactCountLessThanOne
        {
            [NamedArgument('s', NumArgs = 0)]
            public List<string> Args { get; set; }
        }

        [TestMethod]
        public void NamedArgument_WithExactCountLessThanOneThrows_ExceptionOnInitialize()
        {
            var parser = new CliParser<NamedArgumentExactCountLessThanOne>();
            var errs = parser.PerformAttributeIntegrityCheck();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

        internal class NamedArgumentUpperBoundCountLessThanOne
        {
            [NamedArgument('s', NumArgs = 0, Constraint = NumArgsConstraint.AtMost)]
            public List<string> Args { get; set; }
        }

        [TestMethod]
        public void NamedArgument_WithUpperBoundCountLessThanOneThrows_ExceptionOnInitialize()
        {
            var parser = new CliParser<PositionalArgumentUpperBoundCountLessThanOne>();
            var errs = parser.PerformAttributeIntegrityCheck();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

        internal class PositionalArgumentExactCountLessThanOne
        {
            [PositionalArgument(0, NumArgs = 0)]
            public List<string> Args { get; set; }
        }

        [TestMethod]
        public void PositionalArgument_WithExactCountLessThanOneThrows_ExceptionOnInitialize()
        {
            var parser = new CliParser<PositionalArgumentExactCountLessThanOne>();
            var errs = parser.PerformAttributeIntegrityCheck();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

        internal class PositionalArgumentUpperBoundCountLessThanOne
        {
            [PositionalArgument(0, NumArgs = 0, Constraint = NumArgsConstraint.AtMost)]
            public List<string> Args { get; set; }
        }

        [TestMethod]
        public void PositionalArgument_WithUpperBoundCountLessThanOneThrows_ExceptionOnInitialize()
        {
            var parser = new CliParser<PositionalArgumentUpperBoundCountLessThanOne>();
            var errs = parser.PerformAttributeIntegrityCheck();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
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
        public void PositionalArgument_WithMultiplePositionalArgumentsAndFirstHasVarArgs_FailsValidation()
        {
            var parser = new CliParser<MultiplePositionalArgsWithMultipleValues>();
            var errs = parser.PerformAttributeIntegrityCheck();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

#endregion

#region Append/AppendConst implements IEnumerable<>.

        internal class ParseActionAppend
        {
            [PositionalArgument(0, Action = ParseAction.Append)]
            public string Name { get; set; }
        }

        [TestMethod]
        public void Argument_WithParseActionAppendAndDoesNotImplementIEnumerable_FailsValidation()
        {
            var parser = new CliParser<ParseActionAppend>();
            var errs = parser.PerformAttributeIntegrityCheck();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

        internal class ParseActionAppendConst
        {
            [PositionalArgument(0, Action = ParseAction.AppendConst)]
            public string Name { get; set; }
        }

        [TestMethod]
        public void Argument_WithParseActionAppendConstAndDoesNotImplementIEnumerable_FailsValidation()
        {
            var parser = new CliParser<ParseActionAppendConst>();
            var errs = parser.PerformAttributeIntegrityCheck();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

#endregion

#region Count implements int.

        internal class ParseActionCount
        {
            [PositionalArgument(0, Action = ParseAction.Append)]
            public string Name { get; set; }
        }

        [TestMethod]
        public void Argument_WithParseActionCountAndDoesNotImplementInt_FailsValidation()
        {
            var parser = new CliParser<ParseActionCount>();
            var errs = parser.PerformAttributeIntegrityCheck();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

#endregion

#region Short name has valid characters.

        internal class InvalidShortName
        {
            [NamedArgument('.')]
            public string A { get; set; }
        }

        [TestMethod]
        public void Argument_WithInvalidCharacterAsShortName_FailsValidation()
        {
            var parser = new CliParser<InvalidShortName>();
            var errs = parser.PerformAttributeIntegrityCheck();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

#endregion

#region Long name has valid characters.

        internal class InvalidLongName
        {
            [NamedArgument("nano.bot")]
            public string A { get; set; }
        }

        [TestMethod]
        public void Argument_WithInvalidCharacterAsLongName_FailsValidation()
        {
            var parser = new CliParser<InvalidLongName>();
            var errs = parser.PerformAttributeIntegrityCheck();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

#endregion

#region Const value's type convertible to property type.

        internal class ConstWithWrongType
        {
            [NamedArgument('a', Action = ParseAction.StoreConst, Const = "rah")]
            public int Age { get; set; }
        }

        [TestMethod]
        public void Argument_WithConstValueAsWrongType_FailsValidation()
        {
            var parser = new CliParser<ConstWithWrongType>();
            var errs = parser.PerformAttributeIntegrityCheck();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

#endregion

#region Positional arguments cannot store const.

        internal class PositionalArgumentWithConst
        {
            [PositionalArgument(0, Action = ParseAction.StoreConst, Const = "tim")]
            public string Name { get; set; }
        }

        [TestMethod]
        public void PositionalArgument_WithConstValue_FailsValidation()
        {
            var parser = new CliParser<PositionalArgumentWithConst>();
            var errs = parser.PerformAttributeIntegrityCheck();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

#endregion

#region Const True/False must be bool.

        internal class ConstTrueWithWrongType
        {
            [PositionalArgument(0, Action = ParseAction.StoreTrue)]
            public string Name { get; set; }
        }

        [TestMethod]
        public void Argument_WithConstTrueWithWrongType_FailsValidation()
        {
            var parser = new CliParser<ConstTrueWithWrongType>();
            var errs = parser.PerformAttributeIntegrityCheck();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

        internal class ConstFalseWithWrongType
        {
            [PositionalArgument(0, Action = ParseAction.StoreFalse)]
            public string Name { get; set; }
        }

        [TestMethod]
        public void Argument_WithConstFalseWithWrongType_FailsValidation()
        {
            var parser = new CliParser<ConstFalseWithWrongType>();
            var errs = parser.PerformAttributeIntegrityCheck();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

#endregion

#region Help short name validity.

        [TestMethod]
        public void Help_WithInvalidShortName_ThrowsException()
        {
            var help = new Usage.AutomaticHelpGenerator<object>();
            help.ShortName = '.';

            var parser = new CliParser<object>(help);
            var errs = parser.PerformAttributeIntegrityCheck();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

        [TestMethod]
        public void Help_WithInvalidShortNameAsDigit_ThrowsException()
        {
            var help = new Usage.AutomaticHelpGenerator<object>();
            help.ShortName = '1';

            var parser = new CliParser<object>(help);
            var errs = parser.PerformAttributeIntegrityCheck();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

#endregion

#region Help long name validity.

        [TestMethod]
        public void Help_WithInvalidLongName_ThrowsException()
        {
            var help = new Usage.AutomaticHelpGenerator<object>();
            help.LongName = "no.thing";

            var parser = new CliParser<object>(help);
            var errs = parser.PerformAttributeIntegrityCheck();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

        [TestMethod]
        public void Help_WithInvalidLongNameLength_ThrowsException()
        {
            var help = new Usage.AutomaticHelpGenerator<object>();
            help.LongName = "n";

            var parser = new CliParser<object>(help);
            var errs = parser.PerformAttributeIntegrityCheck();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

        [TestMethod]
        public void Help_WithInvalidLongNameEndingWithDash_ThrowsException()
        {
            var help = new Usage.AutomaticHelpGenerator<object>();
            help.LongName = "none-";

            var parser = new CliParser<object>(help);
            var errs = parser.PerformAttributeIntegrityCheck();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

        [TestMethod]
        public void Help_WithInvalidLongNameBeginningWithDigit_ThrowsException()
        {
            var help = new Usage.AutomaticHelpGenerator<object>();
            help.LongName = "1none";

            var parser = new CliParser<object>(help);
            var errs = parser.PerformAttributeIntegrityCheck();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

#endregion

#region Version short name validity.

        [TestMethod]
        [Ignore]
        public void Version_WithInvalidShortName_ThrowsException()
        {
            var help = new Usage.AutomaticHelpGenerator<object>();
            //help.Version.ShortName = '.';

            var errs = new CliParser<object>(help).PerformAttributeIntegrityCheck();
            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

        [TestMethod]
        [Ignore]
        public void Version_WithInvalidShortNameAsDigit_ThrowsException()
        {
            var help = new Usage.AutomaticHelpGenerator<object>();
            //help.Version.ShortName = '1';

            var errs = new CliParser<object>(help).PerformAttributeIntegrityCheck();
            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

#endregion

#region Version long name validity.

        [TestMethod]
        [Ignore]
        public void Version_WithInvalidLongName_ThrowsException()
        {
            // TODO better way to add/remove triggers
            var help = new Usage.AutomaticHelpGenerator<object>();
            //help.Version.LongName = "no.thing";
            var errs = new CliParser<object>(help).PerformAttributeIntegrityCheck();
            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

        [TestMethod]
        [Ignore]
        public void Version_WithInvalidLongNameLength_ThrowsException()
        {
            var help = new Usage.AutomaticHelpGenerator<object>();
            //help.Version.LongName = "n";
            var errs = new CliParser<object>(help).PerformAttributeIntegrityCheck();
            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

        [TestMethod]
        [Ignore]
        public void Version_WithInvalidLongNameEndingWithDash_ThrowsException()
        {
            var help = new Usage.AutomaticHelpGenerator<object>();
            //help.Version.LongName = "none-";

            var errs = new CliParser<object>(help).PerformAttributeIntegrityCheck();
            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

        [TestMethod]
        [Ignore]
        public void Version_WithInvalidLongNameBeginningWithDigit_ThrowsException()
        {
            var help = new Usage.AutomaticHelpGenerator<object>();
            //help.Version.LongName = "1none";

            var errs = new CliParser<object>(help).PerformAttributeIntegrityCheck();
            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

#endregion

#region Positional argument checks respect value Index

        internal class MultiplePositionalArgsDefinedOutOfOrder
        {
            [PositionalArgument(1, NumArgs = 1, Constraint = NumArgsConstraint.AtLeast)]
            public IEnumerable<string> Listy2 { get; set; }

            [PositionalArgument(0, NumArgs = 1)]
            public IEnumerable<string> Listy { get; set; }
        }

        [TestMethod]
        public void PositionalArgument_SpecifiedOutOfOrder_PassesValidation()
        {
            new CliParser<MultiplePositionalArgsDefinedOutOfOrder>();
        }

        internal class MultiplePositionalArgsDefinedSuperClass
        {
            [PositionalArgument(0, NumArgs = 1)]
            public IEnumerable<string> Listy { get; set; }
        }

        internal class MultiplePositionalArgsDefinedSubClass : MultiplePositionalArgsDefinedSuperClass
        {
            [PositionalArgument(1, NumArgs = 1, Constraint = NumArgsConstraint.AtLeast)]
            public IEnumerable<string> Listy2 { get; set; }
        }

        [TestMethod]
        public void PositionalArgument_SpecifiedInSubClass_PassesValidation()
        {
            new CliParser<MultiplePositionalArgsDefinedSubClass>();
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
        public void CheckIntegrity_WithConfigContainingBothPositionalAndVerbs_ThrowsException()
        {
            var parser = new CliParser<PositionalAndVerbOptions>();
            var errs = parser.PerformAttributeIntegrityCheck();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

#endregion

        [TestMethod]
        public void PositionalArgument_WithLowerBoundCountEqualsZero_ParsesNoArguments()
        {
            var opts = new PositionalArgumentUnitTest.PositionalArgumentLowerBoundCountEqualsZero();
            var arguments = new string[0];
            var parser = new CliParser<PositionalArgumentUnitTest.PositionalArgumentLowerBoundCountEqualsZero>();

            parser.Parse(arguments, opts);

            Assert.AreEqual(0, opts.Args.Count);
        }

        [TestMethod]
        public void PositionalArgument_WithLowerBoundCountEqualsZero_ParsesAllArguments()
        {
            var opts = new PositionalArgumentUnitTest.PositionalArgumentLowerBoundCountEqualsZero();
            var arguments = new[] { "first", "second" };
            var parser = new CliParser<PositionalArgumentUnitTest.PositionalArgumentLowerBoundCountEqualsZero>();

            parser.Parse(arguments, opts);

            Assert.AreEqual(2, opts.Args.Count);
        }

        [TestMethod]
        public void PositionalArgument_WithMultiplePositionalArgumentsAndLowerBoundCountEqualsZero_ParsesOtherArguments()
        {
            var opts = new PositionalArgumentUnitTest.MultiplePositionalArgumentLowerBoundCountEqualsZero();
            var arguments = new[] { "fixed" };
            var parser = new CliParser<PositionalArgumentUnitTest.MultiplePositionalArgumentLowerBoundCountEqualsZero>();

            parser.Parse(arguments, opts);

            Assert.AreEqual(0, opts.Args.Count);
        }

        [TestMethod]
        public void PositionalArgument_WithMultiplePositionalArgumentsAndLowerBoundCountEqualsZero_ParsesAllArguments()
        {
            var opts = new PositionalArgumentUnitTest.MultiplePositionalArgumentLowerBoundCountEqualsZero();
            var arguments = new[] { "fixed", "first", "second" };
            var parser = new CliParser<PositionalArgumentUnitTest.MultiplePositionalArgumentLowerBoundCountEqualsZero>();

            parser.Parse(arguments, opts);

            Assert.AreEqual(2, opts.Args.Count);
        }

        private class OptionsWithOptionalNonNullable
        {
            [NamedArgument('a', Constraint = NumArgsConstraint.Optional)]
            public int Age { get; set; }
        }

        [TestMethod]
        public void NamedArgument_WithOptionalConstraintButNonNullable_FailsIntegrityCheck()
        {
            var parser = new CliParser<OptionsWithOptionalNonNullable>();
            var errs = parser.PerformAttributeIntegrityCheck();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

        private class OptionsWithOptionalNullableReference
        {
            [NamedArgument('n', Constraint = NumArgsConstraint.Optional)]
            public string Name { get; set; }
        }

        [TestMethod]
        public void NamedArgument_WithOptionalConstraintButNullableReference_PassesIntegrityCheck()
        {
            var parser = new CliParser<OptionsWithOptionalNullableReference>();
            var errs = parser.PerformAttributeIntegrityCheck();

            Assert.IsTrue(!errs.Any());
        }

        // TODO check for case when property is localized but no ResourceType is provided by prop or its class
    }
}
