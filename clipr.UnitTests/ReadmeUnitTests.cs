using System;
using System.Collections.Generic;
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
            var opt = CliParser.Parse<Options>(
                "-vvv output.txt 1 2 -1 7".Split());
            Assert.AreEqual(3, opt.Verbosity);
        }

        [TestMethod]
        public void ParseExample_WithMultipleArgumentsAndAttributeConfig_ParsesOutputFile()
        {
            var opt = CliParser.Parse<Options>(
                "-vvv output.txt 1 2 -1 7".Split());
            Assert.AreEqual("output.txt", opt.OutputFile);
        }

        [TestMethod]
        public void ParseExample_WithMultipleArgumentsAndAttributeConfig_ParsesNumbers()
        {
            var numbers = new[] { 1, 2, -1, 7 };
            var opt = CliParser.Parse<Options>(
                "-vvv output.txt 1 2 -1 7".Split());

            CollectionAssert.AreEqual(numbers, opt.Numbers);
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

            new CliParserBuilder<FluentOptions>(opt)
                .HasNamedArgument(o => o.Verbosity)
                    .WithShortName('v')
                    .CountsInvocations()
            .And
                .HasNamedArgument(o => o.OutputFile)
                    .WithShortName()
            .And
                .HasPositionalArgumentList(o => o.Numbers)
                    .HasDescription("These are numbers.")
                    .Consumes.AtLeast(1)
            .And.Parser
                .Parse("-vvv -o out.txt 3 4 5 6".Split());

            Assert.AreEqual(3, opt.Verbosity);
        }

        [TestMethod]
        public void ParseExample_WithMultipleArgumentsAndFluentConfig_ParsesOutputFile()
        {
            var opt = new FluentOptions();

            new CliParserBuilder<FluentOptions>(opt)
                .HasNamedArgument(o => o.Verbosity)
                    .WithShortName('v')
                    .CountsInvocations()
            .And
                .HasNamedArgument(o => o.OutputFile)
                    .WithShortName()
            .And
                .HasPositionalArgumentList(o => o.Numbers)
                    .HasDescription("These are numbers.")
                    .Consumes.AtLeast(1)
            .And.Parser
                .Parse("-vvv -o out.txt 3 4 5 6".Split());

            Assert.AreEqual("out.txt", opt.OutputFile);
        }

        [TestMethod]
        public void ParseExample_WithMultipleArgumentsAndFluentConfig_ParsesNumbers()
        {
            var numbers = new[] { 3, 4, 5, 6 };

            var opt = new FluentOptions();

            new CliParserBuilder<FluentOptions>(opt)
                .HasNamedArgument(o => o.Verbosity)
                    .WithShortName('v')
                    .CountsInvocations()
            .And
                .HasNamedArgument(o => o.OutputFile)
                    .WithShortName()
            .And
                .HasPositionalArgumentList(o => o.Numbers)
                    .HasDescription("These are numbers.")
                    .Consumes.AtLeast(1)
            .And.Parser
                .Parse("-vvv -o out.txt 3 4 5 6".Split());

            CollectionAssert.AreEqual(numbers, opt.Numbers);
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
            var obj = CliParser.Parse<StaticEnumerationOptions>(
                "-e first".Split());
            Assert.AreSame(SomeEnum.First, obj.Value);
        }

        [TestMethod]
        public void Parse_WithDictionaryBackend_ParsesToDictionary()
        {
            var key = 1;
            var opt = new Dictionary<int, string>();
            var builder = new CliParserBuilder<Dictionary<int, string>>(opt);
            builder.HasNamedArgument(c => c[key])
                    .WithShortName('n');

            builder.Parser.Parse("-n frank".Split());

            Assert.AreEqual("frank", opt[key]);
        }
    }
}
