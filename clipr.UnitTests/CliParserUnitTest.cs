using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using clipr.Core;
using clipr.Usage;

// ReSharper disable ObjectCreationAsStatement

namespace clipr.UnitTests
{
    [TestClass]
    public class CliParserUnitTest
    {
        internal class CaseFoldingOptions
        {
            [NamedArgument('n', "name")]
            public string Name { get; set; }
        }

        [TestMethod]
        public void CaseFolding_ParseLongArgWithWrongCaseWhenCaseInsensitive_CorrectlyParsesArgs()
        {
            var parser = new CliParser<CaseFoldingOptions>(
                new ParserOptions { CaseInsensitive = true });
            var result = parser.Parse("--Name timothy".Split(), new CaseFoldingOptions());
            result.Handle(
                opt => Assert.AreEqual("timothy", opt.Name),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void CaseFolding_ParseLongArgWithWrongCaseWhenCaseSensitive_ThrowsParseException()
        {
            var result = CliParser.Parse<CaseFoldingOptions>("--Name timothy".Split());
            result.Handle(
                opt => Assert.Fail("Parse succeeded but error was expected."),
                t => Assert.Fail("Trigger initiated but error was expected."),
                errs => Assert.IsTrue(errs
                .OfType<ParseException>()
                .Any()));
        }

        [TestMethod]
        public void CaseFolding_ParseShortArgWithWrongCaseWhenCaseInsensitive_CorrectlyParsesArgs()
        {
            var parser = new CliParser<CaseFoldingOptions>(
                new ParserOptions { CaseInsensitive = true });
            var result = parser.Parse("-N timothy".Split(), new CaseFoldingOptions());
            result.Handle(
                opt => Assert.AreEqual("timothy", opt.Name),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void CaseFolding_ParseShortArgWithWrongCaseWhenCaseSensitive_ThrowsParseException()
        {
            var result = CliParser.Parse<CaseFoldingOptions>("-N timothy".Split());
            result.Handle(
                opt => Assert.Fail("Parse succeeded but error was expected."),
                t => Assert.Fail("Trigger initiated but error was expected."),
                errs => Assert.IsTrue(errs
                .OfType<ParseException>()
                .Any()));
        }

        [TestMethod]
        public void CaseFolding_ParseLongArgPrefixWithCorrectCaseWhenCaseSensitiveAndPartialMatch_CorrectlyParsesArgs()
        {
            var parser = new CliParser<CaseFoldingOptions>(
                new ParserOptions { NamedPartialMatch = true });
            var result = parser.Parse("--na timothy".Split(), new CaseFoldingOptions());
            result.Handle(
                opt => Assert.AreEqual("timothy", opt.Name),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void CaseFolding_ParseLongArgPrefixWithWrongCaseWhenCaseInsensitiveAndPartialMatch_CorrectlyParsesArgs()
        {
            var parser = new CliParser<CaseFoldingOptions>(
                new ParserOptions { CaseInsensitive = true, NamedPartialMatch = true });
            var result = parser.Parse("--Na timothy".Split(), new CaseFoldingOptions());
            result.Handle(
                opt => Assert.AreEqual("timothy", opt.Name),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        internal class IntTypeConversion
        {
            [NamedArgument('a')]
            public int Age { get; set; }
        }

        [TestMethod]
        public void IntTypeConversion_ParseShortArgOnInteger_ConvertsStringArgumentToInteger()
        {
            var result = CliParser.Parse<IntTypeConversion>("-a 3".Split());
            result.Handle(
                opt => Assert.AreEqual(3, opt.Age),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        internal class EnumTypeConversion
        {
            [NamedArgument('s')]
            public EmploymentStatus Employment { get; set; }

            internal enum EmploymentStatus
            {
                Unemployed,
                PartTime,
                FullTime
            }
        }

        [TestMethod]
        public void EnumTypeConversion_ParseShortArgOnEnum_ConvertsStringArgumentToEnum()
        {
            var result = CliParser.Parse<EnumTypeConversion>("-s fulltime".Split());
            result.Handle(
                opt => Assert.AreEqual(EnumTypeConversion.EmploymentStatus.FullTime, opt.Employment),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        public class MixedNamedAndPositional
        {
            [NamedArgument('n')]
            public string Named { get; set; }

            [PositionalArgument(0)]
            public string Positional { get; set; }
        }

        [TestMethod]
        public void ParseArguments_WithNamedBeforePositional_ParsesBothArguments()
        {
            var result = CliParser.Parse<MixedNamedAndPositional>("-n name pos".Split());
            result.Handle(
                opt =>
                {
                    Assert.AreEqual("name", opt.Named);
                    Assert.AreEqual("pos", opt.Positional);
                },
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
            
        }

        [TestMethod]
        public void ParseArguments_WithPositionalBeforeNamed_ParsesBothArguments()
        {
            var result = CliParser.Parse<MixedNamedAndPositional>("pos -n name".Split());
            result.Handle(
                opt =>
                {
                    Assert.AreEqual("name", opt.Named);
                    Assert.AreEqual("pos", opt.Positional);
                },
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        internal class NullUsageAndVersion
        {
            [PositionalArgument(0)]
            public string Value { get; set; }
        }

        [TestMethod]
        public void ParseArguments_WithUsageAndVersionNull_DoesNotThrowException()
        {
            var opt = new NullUsageAndVersion();
            new CliParser<NullUsageAndVersion>(
                ParserOptions.Default, null).Parse("name".Split(), opt);
            Assert.AreEqual("name", opt.Value);
        }

        [TestMethod]
        public void ParseArguments_WithVersionArgument_PrintsVersionAndExits()
        {
            var opts = new object();
            var result = new CliParser<object>().Parse("--version".Split(), opts);
            result.Handle(
                opt => Assert.Fail("Parse succeeded but trigger was expected."),
                t => Assert.IsInstanceOfType(t, typeof(ExecutingAssemblyVersion)),
                errs => Assert.Fail("Error occurred but trigger was expected."));
        }

        [TestMethod]
        public void TryParse_WithInstanceAndValidArgs_ParsesArgs()
        {
            var obj = new CaseFoldingOptions();
            CliParser.TryParse("--name tim".Split(), obj);
            Assert.AreEqual("tim", obj.Name);
        }

        [TestMethod]
        public void TryParse_WithInstanceAndValidArgs_ReturnsTrue()
        {
            var obj = new CaseFoldingOptions();
            if (!CliParser.TryParse("--name tim".Split(), obj))
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TryParse_WithInstanceAndInvalidArgs_ReturnsFalse()
        {
            var obj = new CaseFoldingOptions();
            if (CliParser.TryParse("--ame tim".Split(), obj))
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TryParse_WithOutAndValidArgs_ParsesArgs()
        {
            CaseFoldingOptions obj;
            CliParser.TryParse("--name tim".Split(), out obj);
            Assert.AreEqual("tim", obj.Name);
        }

        [TestMethod]
        public void TryParse_WithOutAndValidArgs_ReturnsTrue()
        {
            CaseFoldingOptions obj;
            if (!CliParser.TryParse("--name tim".Split(), out obj))
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TryParse_WithOutAndInvalidArgs_ReturnsFalse()
        {
            CaseFoldingOptions obj;
            if (CliParser.TryParse("--ame tim".Split(), out obj))
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TryParse_WithOutAndInvalidArgs_SetsOutValueToNull()
        {
            CaseFoldingOptions obj;
            CliParser.TryParse("--ame tim".Split(), out obj);
            Assert.AreEqual(null, obj);
        }

        internal class InfiniteNamedWithPositional
        {
            [NamedArgument('n', 
                Constraint = NumArgsConstraint.AtLeast, NumArgs = 1)]
            public List<string> NamedArgs { get; set; }

            [PositionalArgument(0)]
            public string Positional { get; set; }
        }

        [TestMethod]
        public void Parse_WithPositionalAgumentFollowingNamedWithNoLimit_LeavesLastArgumentForPositional()
        {
            var result = CliParser.Parse<InfiniteNamedWithPositional>(
                "-n one two three -- positional".Split());
            result.Handle(
                opt => Assert.AreEqual("positional", opt.Positional),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        internal class InfiniteNamedWithOtherNamedFollowing
        {
            [NamedArgument('n',
                Constraint = NumArgsConstraint.AtLeast, NumArgs = 1)]
            public List<string> NamedArgs { get; set; }

            [NamedArgument('o', "other")]
            public string Other { get; set; }
        }

        [TestMethod]
        public void Parse_WithLongNamedAgumentFollowingNamedWithNoLimit_StopsParsingAtFirstDash()
        {
            var result = CliParser.Parse<InfiniteNamedWithOtherNamedFollowing>(
                "-n one two three --other final".Split());
            result.Handle(
                opt => Assert.AreEqual("final", opt.Other),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Parse_WithShortNamedAgumentFollowingNamedWithNoLimit_StopsParsingAtFirstDash()
        {
            var result = CliParser.Parse<InfiniteNamedWithOtherNamedFollowing>(
                "-n one two three -o final".Split());
            result.Handle(
                opt => Assert.AreEqual("final", opt.Other),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        internal class EnumerableArguments
        {
            [NamedArgument('n',
                Constraint = NumArgsConstraint.AtLeast, NumArgs = 1)]
            public IEnumerable<int> Numbers { get; set; } 
        }

        [TestMethod]
        public void Parse_WithIEnumerableNamedArgument_ParsesIntoIEnumerable()
        {
            var result = CliParser.Parse<EnumerableArguments>(
                "-n 1 4 6 8".Split());
            result.Handle(
                opt => Assert.AreEqual(4, opt.Numbers.Count()),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [StaticEnumeration]
        internal class SomeEnum
        {
            public static readonly SomeEnum First = new SomeEnum();
            public static readonly SomeEnum Second = new SomeEnum();
        }

        internal class StaticEnumerationOptions
        {
            [NamedArgument('e')]
            public SomeEnum Value { get; set; } 
        }

        [TestMethod]
        public void Parse_WithStaticEnumerationDefinedOnClass_ParsesIntoValue()
        {
            var result = CliParser.Parse<StaticEnumerationOptions>(
                "-e first".Split());
            result.Handle(
                opt => Assert.AreSame(SomeEnum.First, opt.Value),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [StaticEnumeration]
        internal class SomeEnumWithSubclass
        {
            public static readonly SomeEnumWithSubclass First = new EnumSubclass();
            public static readonly EnumSubclass Second = new EnumSubclass();

            public class EnumSubclass : SomeEnumWithSubclass 
            {
            }
        }

        internal class StaticEnumerationWithSubclassOptions
        {
            [NamedArgument('e')]
            public SomeEnum Value { get; set; } 
        }

        [TestMethod]
        public void Parse_WithStaticEnumerationSubclassCovariantDefinedOnClass_ParsesIntoValue()
        {
            var result = CliParser.Parse<StaticEnumerationOptions>(
                "-e first".Split());
            result.Handle(
                opt => Assert.AreSame(SomeEnum.First, opt.Value),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Parse_WithStaticEnumerationSubclassDefinedOnClass_ParsesIntoValue()
        {
            var result = CliParser.Parse<StaticEnumerationOptions>(
                "-e second".Split());
            result.Handle(
                opt => Assert.AreSame(SomeEnum.Second, opt.Value),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        internal class SomeEnumNonStatic
        {
            public static readonly SomeEnumNonStatic First = new SomeEnumNonStatic();
            public static readonly SomeEnumNonStatic Second = new SomeEnumNonStatic();
        }

        internal class StaticEnumerationOptionsExplicit
        {
            [StaticEnumeration]
            [NamedArgument('e')]
            public SomeEnumNonStatic Value { get; set; } 
        }

        [TestMethod]
        public void Parse_WithStaticEnumerationDefinedOnProperty_ParsesIntoValue()
        {
            var result = CliParser.Parse<StaticEnumerationOptionsExplicit>(
                "-e first".Split());
            result.Handle(
                opt => Assert.AreSame(SomeEnumNonStatic.First, opt.Value),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        private class TraceOptions
        {
            [NamedArgument('t')]
            public TraceLevelOption TraceLevel { get; set; }

            [PositionalArgument(0, Constraint = NumArgsConstraint.AtLeast, NumArgs = 0)]
            public List<string> Files { get; set; }
        }

        [TestMethod]
        public void Parse_WithInvalidEnumValue_ThrowsException()
        {
            CliParser.Parse<TraceOptions>("-tX error c:\\file.txt c:\\file2.txt".Split())
                .Handle(v => Assert.Fail("Value parsed, but should have thrown exception."),
                t => Assert.Fail("Trigger {0} executed.", t),
                errs => Assert.IsTrue(errs
                .OfType<ParseException>()
                .Any()));
        }

        [StaticEnumeration]
        public class TraceLevelOption
        {
            public static readonly TraceLevelOption Off = new TraceLevelOption(System.Diagnostics.TraceLevel.Off);
            public static readonly TraceLevelOption Error = new TraceLevelOption(System.Diagnostics.TraceLevel.Error);
            public static readonly TraceLevelOption Warning = new TraceLevelOption(System.Diagnostics.TraceLevel.Warning);
            public static readonly TraceLevelOption Info = new TraceLevelOption(System.Diagnostics.TraceLevel.Info);
            public static readonly TraceLevelOption Verbose = new TraceLevelOption(System.Diagnostics.TraceLevel.Verbose);

            private System.Diagnostics.TraceLevel value;

            private TraceLevelOption(System.Diagnostics.TraceLevel value)
            {
                this.value = value;
            }
        }

        private class OptionsWithOptionalValue
        {
            [NamedArgument('s', "server", Constraint = NumArgsConstraint.Optional, Const = 1234)]
            public int? Server { get; set; }
        }

        [TestMethod]
        public void Parse_WithOptionalValueAndValueProvided_SetsValue()
        {
            var result = CliParser.Parse<OptionsWithOptionalValue>("--server 123".Split());
            result.Handle(
                opt => Assert.AreEqual(123, opt.Server),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Parse_WithOptionalValueAndNoValueProvided_SetsConst()
        {
            var result = CliParser.Parse<OptionsWithOptionalValue>("--server".Split());
            result.Handle(
                opt => Assert.AreEqual(1234, opt.Server),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        private class OptionsWithMultipleOptionalValues
        {
            [NamedArgument('a', "ArgumentA",
               Constraint = NumArgsConstraint.Optional,
               Action = ParseAction.Store,
               Const = "MyConstValueA",
               Description = "Argument A")]
            public string A { get; set; }

            [NamedArgument('b', "ArgumentB",
                Constraint = NumArgsConstraint.Optional,
                Action = ParseAction.Store,
                Const = "MyConstValueB",
                Description = "Argument B")]
            public string B { get; set; }
        }

                [TestMethod]
        public void Parse_WithMultipleOptionalValuesAndNoValueProvided_SetsConstOnBoth()
        {
            const string expectedA = "MyConstValueA";
            const string expectedB = "MyConstValueB";
            var result = CliParser.Parse<OptionsWithMultipleOptionalValues>("-a -b".Split());
            result.Handle(
                opt =>
                {
                    Assert.AreEqual(expectedA, opt.A);
                    Assert.AreEqual(expectedB, opt.B);
                },
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }
    }
}
