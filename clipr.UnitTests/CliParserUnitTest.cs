using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using clipr.Core;

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
                new CaseFoldingOptions(), ParserOptions.CaseInsensitive);
            parser.Parse("--Name timothy".Split());
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void CaseFolding_ParseLongArgWithWrongCaseWhenCaseSensitive_ThrowsParseException()
        {
            var opt = CliParser.Parse<CaseFoldingOptions>("--Name timothy".Split());
            Assert.AreEqual("timothy", opt.Name);
        }

        [TestMethod]
        public void CaseFolding_ParseShortArgWithWrongCaseWhenCaseInsensitive_CorrectlyParsesArgs()
        {
            var parser = new CliParser<CaseFoldingOptions>(
                new CaseFoldingOptions(), ParserOptions.CaseInsensitive);
            parser.Parse("-N timothy".Split());
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void CaseFolding_ParseShortArgWithWrongCaseWhenCaseSensitive_ThrowsParseException()
        {
            var opt = CliParser.Parse<CaseFoldingOptions>("-N timothy".Split());
            Assert.AreEqual("timothy", opt.Name);
        }

        [TestMethod]
        public void CaseFolding_ParseLongArgPrefixWithCorrectCaseWhenCaseSensitiveAndPartialMatch_CorrectlyParsesArgs()
        {
            var parser = new CliParser<CaseFoldingOptions>(
                new CaseFoldingOptions(), ParserOptions.NamedPartialMatch);
            parser.Parse("--na timothy".Split());
        }

        [TestMethod]
        public void CaseFolding_ParseLongArgPrefixWithWrongCaseWhenCaseInsensitiveAndPartialMatch_CorrectlyParsesArgs()
        {
            var parser = new CliParser<CaseFoldingOptions>(
                new CaseFoldingOptions(), ParserOptions.CaseInsensitive | ParserOptions.NamedPartialMatch);
            parser.Parse("--Na timothy".Split());
        }

        internal class IntTypeConversion
        {
            [NamedArgument('a')]
            public int Age { get; set; }
        }

        [TestMethod]
        public void IntTypeConversion_ParseShortArgOnInteger_ConvertsStringArgumentToInteger()
        {
            var opt = CliParser.Parse<IntTypeConversion>("-a 3".Split());
            Assert.AreEqual(3, opt.Age);
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
            var opt = CliParser.Parse<EnumTypeConversion>("-s fulltime".Split());
            Assert.AreEqual(EnumTypeConversion.EmploymentStatus.FullTime, opt.Employment);
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
            var opt = CliParser.Parse<MixedNamedAndPositional>("-n name pos".Split());
            Assert.AreEqual("name", opt.Named);
            Assert.AreEqual("pos", opt.Positional);
        }

        [TestMethod]
        public void ParseArguments_WithPositionalBeforeNamed_ParsesBothArguments()
        {
            var opt = CliParser.Parse<MixedNamedAndPositional>("pos -n name".Split());
            Assert.AreEqual("name", opt.Named);
            Assert.AreEqual("pos", opt.Positional);
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
                opt, ParserOptions.None, null).Parse("name".Split());
            Assert.AreEqual("name", opt.Value);
        }

        [TestMethod]
        [ExpectedException(typeof (ParserExit))]
        public void ParseArguments_WithVersionArgument_PrintsVersionAndExits()
        {
            var opt = new object();
            new CliParser<object>(opt).Parse("--version".Split());
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
            var obj = CliParser.Parse<InfiniteNamedWithPositional>(
                "-n one two three -- positional".Split());
            Assert.AreEqual("positional", obj.Positional);
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
            var obj = CliParser.Parse<InfiniteNamedWithOtherNamedFollowing>(
                "-n one two three --other final".Split());
            Assert.AreEqual("final", obj.Other);
        }

        [TestMethod]
        public void Parse_WithShortNamedAgumentFollowingNamedWithNoLimit_StopsParsingAtFirstDash()
        {
            var obj = CliParser.Parse<InfiniteNamedWithOtherNamedFollowing>(
                "-n one two three -o final".Split());
            Assert.AreEqual("final", obj.Other);
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
            var obj = CliParser.Parse<EnumerableArguments>(
                "-n 1 4 6 8".Split());
            Assert.AreEqual(4, obj.Numbers.Count());
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
            var obj = CliParser.Parse<StaticEnumerationOptions>(
                "-e first".Split());
            Assert.AreSame(SomeEnum.First, obj.Value);
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
            var obj = CliParser.Parse<StaticEnumerationOptions>(
                "-e first".Split());
            Assert.AreSame(SomeEnum.First, obj.Value);
        }

        [TestMethod]
        public void Parse_WithStaticEnumerationSubclassDefinedOnClass_ParsesIntoValue()
        {
            var obj = CliParser.Parse<StaticEnumerationOptions>(
                "-e second".Split());
            Assert.AreSame(SomeEnum.Second, obj.Value);
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
            var obj = CliParser.Parse<StaticEnumerationOptionsExplicit>(
                "-e first".Split());
            Assert.AreSame(SomeEnumNonStatic.First, obj.Value);
        }

        private class TraceOptions
        {
            [NamedArgument('t')]
            public TraceLevelOption TraceLevel { get; set; }

            [PositionalArgument(0, Constraint = NumArgsConstraint.AtLeast, NumArgs = 0)]
            public List<string> Files { get; set; }
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void Parse_WithInvalidEnumValue_ThrowsException()
        {
            var obj = CliParser.Parse<TraceOptions>("-tX error c:\\file.txt c:\\file2.txt".Split());
            Console.WriteLine(obj.TraceLevel);
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
    }
}
