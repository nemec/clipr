using clipr.Core;
using clipr.Triggers;
using System;
using System.Collections.Generic;

namespace clipr.Fluent
{
    internal class FluentParserConfig<TConf> : ParserConfig where TConf : class
    {
        public FluentParserConfig(
            RootApplicationMetadata metadata,
            string applicationDescription,
            IParserSettings options, 
            IEnumerable<ITerminatingTrigger> triggers)
            : base(metadata, options, triggers)
        {
            Name = metadata.ApplicationName;
            Description = applicationDescription;
            /*
             * var b = new CliParserBuilder<Options>();
             * b.AddNamedOption(o => o.Verbose)
             *      .WithLongName("verb")
             *      .WithShortName(); // lower case v by defaults
             * b.AddVerb(o => o.AddInfo)
             *      .WithName("add")
             *      .HasMetaVar("out")
             *      .HasDescription("Some text.")
             *      .ConsumesAtMost(10) // AtLeast, Exactly
             *      .StoresValue(new object())
             *      .AppendsValue()
             *      .AppendsValue("something")
             *      .CountsInvocations()
             *      .StoresTrue()
             *      .StoresFalse();
             * var parser = b.Build();
             * */
        }
    }
}
