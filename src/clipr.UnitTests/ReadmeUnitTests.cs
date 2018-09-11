using System;
using System.Collections.Generic;
using System.IO;
using clipr.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace clipr.UnitTests
{
    /// <summary>
    /// Unit tests for all examples in the README
    /// </summary>
    [TestClass]
    public class ReadmeUnitTests
    {
        [ApplicationInfo(Description = "This is a set of options.")]
        public class Options
        {
            [NamedArgument('v', "verbose", Action = ParseAction.Count,
                Description = "Increase the verbosity of the output.")]
            public int Verbosity { get; set; }

            [PositionalArgument(0, MetaVar = "OUT",
                Description = "Output file.")]
            public string OutputFile { get; set; }

            [PositionalArgument(1, MetaVar = "N",
                NumArgs = 1,
                Constraint = NumArgsConstraint.AtLeast,
                Description = "Numbers to sum.")]
            public List<int> Numbers { get; set; }
        }

        [TestMethod]
        public void ParseExample_WithMultipleArgumentsAndAttributeConfig_ParsesVerbosity()
        {
            var result = CliParser.Parse<Options>(
                "-vvv output.txt 1 2 -1 7".Split());
            result.Handle(
                opt => Assert.AreEqual(3, opt.Verbosity),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void ParseExample_WithMultipleArgumentsAndAttributeConfig_ParsesOutputFile()
        {
            var result = CliParser.Parse<Options>(
                "-vvv output.txt 1 2 -1 7".Split());
            result.Handle(
                opt => Assert.AreEqual("output.txt", opt.OutputFile),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void ParseExample_WithMultipleArgumentsAndAttributeConfig_ParsesNumbers()
        {
            var numbers = new[] { 1, 2, -1, 7 };
            var result = CliParser.Parse<Options>(
                "-vvv output.txt 1 2 -1 7".Split());
            result.Handle(
                opt => CollectionAssert.AreEqual(numbers, opt.Numbers),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        public class FluentOptions
        {
            public int Verbosity { get; set; }

            public string OutputFile { get; set; }

            public List<int> Numbers { get; set; }
        }

        [TestMethod]
        public void ParseExample_WithMultipleArgumentsAndFluentConfig_ParsesVerbosity()
        {
            var opt = new FluentOptions();
            var builder = new CliParserBuilder<FluentOptions>();
            builder
                .AddNamedArgument(o => o.Verbosity)
                .WithShortName('v')
                .CountsInvocations();
            builder
                .AddNamedArgument(o => o.OutputFile)
                .WithShortName();
            builder.AddPositionalArgumentList(o => o.Numbers)
                .WithDescription("These are numbers")
                .ConsumesAtLeast(1);

            var parser = builder.BuildParser();
                
            parser.Parse("-vvv -o out.txt 3 4 5 6".Split(), opt);

            Assert.AreEqual(3, opt.Verbosity);
        }

        [TestMethod]
        public void ParseExample_WithMultipleArgumentsAndFluentConfig_ParsesOutputFile()
        {
            var opt = new FluentOptions();
            var builder = new CliParserBuilder<FluentOptions>();
            builder
                .AddNamedArgument(o => o.Verbosity)
                .WithShortName('v')
                .CountsInvocations();
            builder
                .AddNamedArgument(o => o.OutputFile)
                .WithShortName();
            builder.AddPositionalArgumentList(o => o.Numbers)
                .WithDescription("These are numbers")
                .ConsumesAtLeast(1);

            var parser = builder.BuildParser();

            parser.Parse("-vvv -o out.txt 3 4 5 6".Split(), opt);

            Assert.AreEqual("out.txt", opt.OutputFile);
        }

        [TestMethod]
        public void ParseExample_WithMultipleArgumentsAndFluentConfig_ParsesNumbers()
        {
            var numbers = new[] { 3, 4, 5, 6 };

            var opt = new FluentOptions();
            var builder = new CliParserBuilder<FluentOptions>();
            builder
                .AddNamedArgument(o => o.Verbosity)
                .WithShortName('v')
                .CountsInvocations();
            builder
                .AddNamedArgument(o => o.OutputFile)
                .WithShortName();
            builder.AddPositionalArgumentList(o => o.Numbers)
                .WithDescription("These are numbers")
                .ConsumesAtLeast(1);

            var parser = builder.BuildParser();

            parser.Parse("-vvv -o out.txt 3 4 5 6".Split(), opt);

            CollectionAssert.AreEqual(numbers, opt.Numbers);
        }

        [TestMethod]
        public void ParseExample_WithDynamicArgumentsAndFluentConfig_ParsesValue()
        {
            var expected = "frank";
            var key = 1;
            var opt = new Dictionary<int, string>();
            var builder = new CliParserBuilder<Dictionary<int, string>>();
            builder.AddNamedArgument(c => c[key])
                  .WithShortName('n');

            var parser = builder.BuildParser();
            parser.Parse("-n frank".Split(), opt);

            Assert.AreEqual(expected, opt[key]);
        }

        [StaticEnumeration]
        internal abstract class SomeEnum
        {
            public static readonly SomeEnum First = new EnumSubclass();

            public abstract void DoSomeWork();

            public class EnumSubclass : SomeEnum
            {
                public override void DoSomeWork() { }
            }
        }

        internal class StaticEnumerationOptions
        {
            [NamedArgument('e')]
            public SomeEnum Value { get; set; }
        }

        internal class StaticEnumerationExplicitOptions
        {
            [NamedArgument('e')]
            [StaticEnumeration]  // Allowed in case attr is not defined on SomeEnum
            public SomeEnum Value { get; set; }
        }

        [TestMethod]
        public void Parse_WithStaticEnumeration_ParsesValue()
        {
            var result = CliParser.Parse<StaticEnumerationOptions>(
                "-e first".Split());
            result.Handle(
                opt => Assert.AreSame(SomeEnum.First, opt.Value),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Parse_WithDictionaryBackend_ParsesToDictionary()
        {
            var key = 1;
            var opt = new Dictionary<int, string>();
            var builder = new CliParserBuilder<Dictionary<int, string>>();
            builder.AddNamedArgument(c => c[key])
                    .WithShortName('n');

            var parser = builder.BuildParser();
                
            parser.Parse("-n frank".Split(), opt);

            Assert.AreEqual("frank", opt[key]);
        }

        public class FileExistsOptions
        {
            [FileExists]
            [NamedArgument('c', "config")]
            public string ConfigurationFile { get; set; }
        }

        [TestMethod]
        public void Parse_WithFileExists_ValidatesThatFileExists()
        {
            var opt = new FileExistsOptions();
            var parser = new CliParser<FileExistsOptions>();

            parser.Parse("-c testdirectory\\testfile.txt".Split(), opt);

            Assert.AreEqual("hello world", File.ReadAllText(opt.ConfigurationFile));
        }
    }
}
