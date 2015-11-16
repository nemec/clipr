using System.Collections.Generic;
using clipr.Arguments;
using clipr.Core;
using clipr.Triggers;

namespace clipr.Fluent
{
    internal class FluentParserConfig<TConfig> : ParserConfig<TConfig> where TConfig : class
    {
        public FluentParserConfig(ParserOptions options, IEnumerable<ITerminatingTrigger> triggers)
            : base(options, triggers)
        {
            PendingNamedArguments = new List<NamedArgument>();
            /*
             * CliParser<Config>
             *  .HasNamedOption(c => c.Verbose)
             *      .WithLongName("verb")
             *      .WithShortName() // lower case v by defaults
             *  .And
             *   .HasPositionalOption(c => c.OutputFile)
             *      .HasMetaVar("out")
             *      .HasDescription("Some text.")
             *      .IsInMutuallyExclusiveGroup("grp")
             *      .Consumes.AtMost(10) // .AtLeast .Exactly
             *      .StoresValue(new object())
             *      .AppendsValue()
             *      .AppendsValue("something")
             *      .CountsInvocations()
             *      .StoresTrue()
             *      .StoresFalse()
             *  .And
             *   .HasVerb("add", c => c.AddInfo)
             * */
        }

        private List<NamedArgument> PendingNamedArguments { get; set; }

        internal void ProcessArguments()
        {
            foreach (var arg in PendingNamedArguments)
            {
                if (arg.ShortName.HasValue)
                {
                    ShortNameArguments.Add(arg.ShortName.Value, arg);
                }
                if (arg.LongName != null)
                {
                    LongNameArguments.Add(arg.LongName, arg);
                }
                if (arg.Required)
                {
                    RequiredNamedArguments.Add(arg.Name);
                }
            }
        }

        public void Add<TNamed, TArg>(NamedBase<TConfig, TNamed, TArg> named) 
            where TNamed : NamedBase<TConfig, TNamed, TArg>
        {
            PendingNamedArguments.Add(named.Arg);
        }

        public void Add<TPositional, TArg>(PositionalBase<TConfig, TPositional, TArg> positional)
            where TPositional : PositionalBase<TConfig, TPositional, TArg>
        {
            PositionalArguments.Add(positional.Arg);
        }
    }
}
