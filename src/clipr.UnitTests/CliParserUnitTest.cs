using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using clipr.Core;
using clipr.Usage;
using clipr.Validation;

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
                new ParserSettings<CaseFoldingOptions> { CaseInsensitive = true });
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
                new ParserSettings<CaseFoldingOptions> { CaseInsensitive = true });
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
                new ParserSettings<CaseFoldingOptions> { NamedPartialMatch = true });
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
                new ParserSettings<CaseFoldingOptions> { CaseInsensitive = true, NamedPartialMatch = true });
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
            var opts = new ParserSettings<NullUsageAndVersion>();
            opts.HelpGenerator = null;
            opts.VersionGenerator = null;
            new CliParser<NullUsageAndVersion>(opts).Parse("name".Split(), opt);
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

        internal class EnumerableArgumentsDefaultConstraint
        {
            [NamedArgument('n')]
            public IEnumerable<int> Numbers { get; set; }
        }

        [TestMethod]
        public void Parse_WithIEnumerableNamedArgumentDefaultConstraint_ParsesIntoIEnumerable()
        {
            var result = CliParser.Parse<EnumerableArgumentsDefaultConstraint>(
                "-n 1 4 6 8".Split());
            result.Handle(
                opt => Assert.AreEqual(4, opt.Numbers.Count()),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments: {0}", e));
        }

        internal class ListArgumentsDefaultConstraint
        {
            [NamedArgument('n')]
            public IList<int> Numbers { get; set; }
        }

        [TestMethod]
        public void Parse_WithIListNamedArgumentDefaultConstraint_ParsesIntoIEnumerable()
        {
            var result = CliParser.Parse<ListArgumentsDefaultConstraint>(
                "-n 1 4 6 8".Split());
            result.Handle(
                opt => Assert.AreEqual(4, opt.Numbers.Count()),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments: {0}", e));
        }

        internal class BooleanValueWithDefaultAction
        {
            [NamedArgument('t')]
            public bool IsTrue { get; set; }
        }

        public void Parse_WithBooleanDefaultAction_SetsTrue()
        {
            var result = CliParser.Parse<BooleanValueWithDefaultAction>(
                "-t".Split());
            result.Handle(
                opt => Assert.IsTrue(opt.IsTrue),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments: {0}", e));
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

        internal class ValidationWithFlagRule
        {
            [NamedArgument('r')]
            public bool Recursive { get; set; }

            [NamedArgument('f')]
            public bool Force { get; set; }

            [NamedArgument("no-preserve-root")]
            public bool NoPreserveRoot { get; set; }

            [PositionalArgument(0)]
            public IList<string> Filenames { get; set; }
        }

        internal class ValidationWithFlagRuleComparer : IEqualityComparer<ValidationWithFlagRule>
        {
            public bool Equals(ValidationWithFlagRule x, ValidationWithFlagRule y)
            {
                if (x == null || y == null) return ReferenceEquals(x, y);

                if (x.Force != y.Force ||
                    x.NoPreserveRoot != y.NoPreserveRoot ||
                    x.Recursive != y.Recursive)
                {
                    return false;
                }

                return x.Filenames.SequenceEqual(y.Filenames);
            }

            public int GetHashCode(ValidationWithFlagRule obj)
            {
                var seed = (obj.Force ? 3 : 0) ^
                    (obj.NoPreserveRoot ? 5 : 0) ^
                    (obj.Recursive ? 7 : 0);
                if (obj.Filenames == null) return seed;
                return obj.Filenames.Aggregate(seed, (prior, next) => prior ^ next.GetHashCode());
            }
        }

        [TestMethod]
        public void Validate_WithMissingFlag_ThrowsError()
        {
            var expected = new ValidationFailure(
                "Filenames",
                "When deleting the root directory '/' you must pass the parameter --no-preserve-root");

            var validator = new BasicParseValidator<ValidationWithFlagRule>();
            validator.AddRule(o =>
                    o.Filenames.Any(f => f == "/") && !o.NoPreserveRoot
                ? expected
                : null);

            var parser = new CliParser<ValidationWithFlagRule>();
            parser.Validator = validator;
            var obj = new ValidationWithFlagRule();


            var result = parser.Parse("-rf / bin".Split(), obj);

            result.Handle(
                opt => Assert.Fail("Parsing validation should throw an error."),
                trigger => Assert.Fail("No triggers should be thrown"),
                err => Assert.AreEqual(expected, err.FirstOrDefault())
            );
        }

        [TestMethod]
        public void Validate_WithFlag_ReturnsSuccess()
        {
            var expected = new ValidationWithFlagRule
            {
                Filenames = new List<string> { "/", "bin" },
                NoPreserveRoot = true,
                Recursive = true,
                Force = true
            };

            var validator = new BasicParseValidator<ValidationWithFlagRule>();
            validator.AddRule(o =>
                    o.Filenames.Any(f => f == "/") && !o.NoPreserveRoot
                ? new ValidationFailure(
                    "Filenames",
                    "When deleting the root directory '/' you must pass the parameter --no-preserve-root")
                : null);

            var parser = new CliParser<ValidationWithFlagRule>();
            parser.Validator = validator;
            var obj = new ValidationWithFlagRule();


            var result = parser.Parse("-rf --no-preserve-root / bin".Split(), obj);

            result.Handle(
                opt => Assert.IsTrue(new ValidationWithFlagRuleComparer().Equals(expected, opt)),
                trigger => Assert.Fail("No triggers should be thrown"),
                err => Assert.Fail("Validation should succeed")
            );
        }

        internal class ValidationMutuallyExclusive
        {
            [NamedArgument('a', "ServerAddress")]
            public string ServerAddress { get; set; }

            [NamedArgument('u', "Username")]
            public string Username { get; set; }

            [NamedArgument('p', "Password")]
            public string Password { get; set; }

            [NamedArgument('f', "File", Constraint = NumArgsConstraint.Optional, Const = "settings.json")]
            public string SettingsFile { get; set; }
        }

        internal class ExclusiveValidator : IParseValidator<ValidationMutuallyExclusive>
        {
            public ValidationResult Validate(ValidationMutuallyExclusive obj)
            {
                var errs = new List<ValidationFailure>();
                if (obj.SettingsFile != null)
                {
                    if (obj.ServerAddress != null)
                    {
                        errs.Add(new ValidationFailure(
                            nameof(ValidationMutuallyExclusive.ServerAddress),
                            "Must be null if a settings file is provided."));
                    }
                    if (obj.Username != null)
                    {
                        errs.Add(new ValidationFailure(
                            nameof(ValidationMutuallyExclusive.Username),
                            "Must be null if a settings file is provided."));
                    }
                    if (obj.Password != null)
                    {
                        errs.Add(new ValidationFailure(
                            nameof(ValidationMutuallyExclusive.Password),
                            "Must be null if a settings file is provided."));
                    }
                }
                else
                {
                    if (obj.ServerAddress == null)
                    {
                        errs.Add(new ValidationFailure(
                            nameof(ValidationMutuallyExclusive.ServerAddress),
                            "Server address must be provided."));
                    }
                    if (obj.Username == null)
                    {
                        errs.Add(new ValidationFailure(
                            nameof(ValidationMutuallyExclusive.Username),
                            "Username must be provided."));
                    }
                    if (obj.Password == null)
                    {
                        errs.Add(new ValidationFailure(
                            nameof(ValidationMutuallyExclusive.Password),
                            "Password must be provided."));
                    }
                }
                return new ValidationResult(errs);
            }
        }

        [TestMethod]
        public void Validate_WithMutuallyExclusiveOptionsViolateSettings_FailsValidation()
        {
            var expected = "Must be null if a settings file is provided.";

            var parser = new CliParser<ValidationMutuallyExclusive>();
            parser.Validator = new ExclusiveValidator();
            var obj = new ValidationMutuallyExclusive();

            var actual = parser.Parse(@"-f c:\myfile.conf -a 127.0.0.1".Split(), obj);

            actual.Handle(
                opt => Assert.Fail("Parsing should fail validation"),
                trigger => Assert.Fail("No triggers should be launched"),
                errs => Assert.AreEqual(
                    (errs.First() as ValidationFailure).ErrorMessage,
                    expected)
            );
        }

        [TestMethod]
        public void Validate_WithMutuallyExclusiveOptionsMissingRequired_FailsValidation()
        {
            var expected = "Password must be provided.";

            var parser = new CliParser<ValidationMutuallyExclusive>();
            parser.Validator = new ExclusiveValidator();
            var obj = new ValidationMutuallyExclusive();

            var actual = parser.Parse(@"-a 127.0.0.1 -u frankie".Split(), obj);

            actual.Handle(
                opt => Assert.Fail("Parsing should fail validation"),
                trigger => Assert.Fail("No triggers should be launched"),
                errs => Assert.AreEqual(
                    (errs.First() as ValidationFailure).ErrorMessage,
                    expected)
            );
        }

        [TestMethod]
        public void Validate_WithMutuallyExclusiveOptionsAndSettingsFile_PassesValidation()
        {
            var expected = @"c:\myfile.conf";

            var parser = new CliParser<ValidationMutuallyExclusive>();
            parser.Validator = new ExclusiveValidator();
            var obj = new ValidationMutuallyExclusive();

            var actual = parser.Parse(@"-f c:\myfile.conf".Split(), obj);

            actual.Handle(
                opt => Assert.AreEqual(expected, opt.SettingsFile),
                trigger => Assert.Fail("No triggers should be launched"),
                errs => Assert.Fail(errs.First().ToString())
            );
        }

        [TestMethod]
        public void Validate_WithMutuallyExclusiveOptionsAndCliSettings_PassesValidation()
        {
            var expectedAddr = "127.0.0.1";
            var expectedUser = "frankie";
            var expectedPass = "love123";

            var parser = new CliParser<ValidationMutuallyExclusive>();
            parser.Validator = new ExclusiveValidator();
            var obj = new ValidationMutuallyExclusive();

            var actual = parser.Parse(@"-a 127.0.0.1 -u frankie -plove123".Split(), obj);

            actual.Handle(
                opt =>
                {
                    Assert.AreEqual(expectedAddr, opt.ServerAddress);
                    Assert.AreEqual(expectedUser, opt.Username);
                    Assert.AreEqual(expectedPass, opt.Password);
                },
                trigger => Assert.Fail("No triggers should be launched"),
                errs => Assert.Fail(errs.First().ToString())
            );
        }
    }
}
