
using clipr.IOC;

namespace clipr.Core
{
    /// <summary>
    /// Parsing config for a verb, includes a way to "set" the resulting
    /// verb on the original options object.
    /// </summary>
    public interface IVerbParserConfig: IParserConfig
    {
        /// <summary>
        /// Store the verb on the options objcet.
        /// </summary>
        IValueStoreDefinition Store { get; }
    }

    internal class VerbParserConfig<TVerb> : ParserConfig<TVerb>, IVerbParserConfig where TVerb : class 
    {
        public VerbParserConfig(IParserConfig internalParserConfig, IValueStoreDefinition store, ParserOptions options, IVerbFactory factory) 
            : base(options, null, factory)
        {
            InternalParserConfig = internalParserConfig;
            Store = store;

            LongNameArguments = InternalParserConfig.LongNameArguments;
            ShortNameArguments = InternalParserConfig.ShortNameArguments;
            PositionalArguments = InternalParserConfig.PositionalArguments;
            Verbs = InternalParserConfig.Verbs;
            PostParseMethods = InternalParserConfig.PostParseMethods;
            RequiredMutuallyExclusiveArguments = InternalParserConfig.RequiredMutuallyExclusiveArguments;
            RequiredNamedArguments = InternalParserConfig.RequiredNamedArguments;
        }

        public IValueStoreDefinition Store { get; set; }

        private IParserConfig InternalParserConfig { get; set; }
    }
}
