using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using clipr.Annotations;

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
            parser.Parse("--name timothy".Split());
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
            parser.Parse("-n timothy".Split());
        }

        [TestMethod]
        [ExpectedException(typeof(ParseException))]
        public void CaseFolding_ParseShortArgWithWrongCaseWhenCaseSensitive_ThrowsParseException()
        {
            var opt = CliParser.Parse<CaseFoldingOptions>("-N timothy".Split());
            Assert.AreEqual("timothy", opt.Name);
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

        internal class CustomTypeConversion
        {
            [NamedArgument("set")]
            public CustomType Custom { get; set; }

            [TypeConverter(typeof(CustomTypeConverter))]
            public class CustomType
            {
                public int Value { get; set; }
            }

            public class CustomTypeConverter : StringTypeConverter<CustomType>
            {
                public override CustomType ConvertFrom(CultureInfo culture, string value)
                {
                    var converted = int.Parse(value);
                    return new CustomType
                        {
                            Value = converted
                        };
                }

                public override bool IsValid(string value)
                {
                    int converted;
                    return int.TryParse(value, out converted);
                }
            }
        }

        [TestMethod]
        public void CustomTypeConversion_ParseShortArgOnEnum_ConvertsStringArgumentToCustomType()
        {
            var opt = CliParser.Parse<CustomTypeConversion>("--set=3".Split());
            Assert.AreEqual(3, opt.Custom.Value);
        }

        internal class ListTypeConversion
        {
            [NamedArgument("numbers", NumArgs = 2)]
            public List<int> Numbers { get; set; }
        }

        [TestMethod]
        public void ListTypeConversion_GivenMultipleIntegers_ConvertsAndAddsToList()
        {
            var expected = new List<int> {1, 2};
            var opt = CliParser.Parse<ListTypeConversion>("--numbers 1 2".Split());
            CollectionAssert.AreEqual(expected, opt.Numbers);
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
    }
}
