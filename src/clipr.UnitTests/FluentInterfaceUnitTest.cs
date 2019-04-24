using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace clipr.UnitTests
{
    [TestClass]
    public class FluentInterfaceUnitTest
    {
        internal class FluentOptions
        {
            public int Verbosity { get; set; }

            public string OutputFile { get; set; }

            public List<int> Numbers { get; set; }
        }

        [TestMethod]
        public void FluentParser_WithConfiguration_CorrectlyParsesArguments()
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

            Assert.AreEqual("out.txt", opt.OutputFile);

            var sum = 0;
            foreach (var number in opt.Numbers)
            {
                sum += number;
            }
            Assert.AreEqual(18, sum);
        }

        internal class FluentOptionsWithVerb
        {
            public int NumCounters { get; set; }

            public FluentOptionsVerb AddInfo { get; set; }
        }

        internal class FluentOptionsVerb
        {
            public string Filename { get; set; }
        }

        [TestMethod]
        public void FluentParser_WithConfigurationVerb_CorrectlyParsesArguments()
        {
            var opt = new FluentOptionsWithVerb();

            var builder = new CliParserBuilder<FluentOptionsWithVerb>();
            builder
                .AddNamedArgument(c => c.NumCounters)
                .WithShortName();
            builder
                .AddVerb("add", c => c.AddInfo,
                    v => v.AddPositionalArgument(c => c.Filename));
            var parser = builder.BuildParser();
            parser.Parse("add myfile.txt".Split(), opt);

            Assert.AreEqual("myfile.txt", opt.AddInfo.Filename);
        }

        [TestMethod]
        public void Fluent_WithDictionaryBackend_ParsesKeysCorrectly()
        {
            var key = 10;
            var opt = new Dictionary<int, string>();
            var builder = new CliParserBuilder<Dictionary<int, string>>();
            builder.AddNamedArgument(c => c[key])
                  .WithShortName('n');

            var parser = builder.BuildParser();
            parser.Parse("-n frank".Split(), opt);

            Assert.AreEqual(1, opt.Count);
            Assert.AreEqual(10, opt.Keys.First());
            Assert.AreEqual(opt[10], "frank");
        }
    }
}
