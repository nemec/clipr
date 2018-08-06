
using clipr.IOC;
using clipr.Triggers;
using System;
using System.Collections.Generic;

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

        /// <summary>
        /// The verbs leading up to this config (used mostly for help generation)
        /// </summary>
        string[] PrecursorVerbs { get; set; }
        
        void AppendTriggers(IEnumerable<ITerminatingTrigger> triggers);
    }

    internal class VerbParserConfig<TVerb> : ParserConfig, IVerbParserConfig where TVerb : class
    {
        // WARNING - if you modify this constructor, make sure to make the appropriate changes
        // to the method AttributeConfigParser.InitializeVerbs, which uses Activator.CreateInstance
        // to create newinstances of this class.
        public VerbParserConfig(
                RootApplicationMetadata metadata,
                string name,
                string description,
                LocalizationInfo localizationInfo,
                IParserConfig internalParserConfig,
                IValueStoreDefinition store,
                IParserSettings options,
                string[] precursorVerbs)
            : base(metadata, options, null)
        {
            Name = name;
            Description = description;
            LocalizationInfo = localizationInfo;
            InternalParserConfig = internalParserConfig;
            Store = store;

            LongNameArguments = InternalParserConfig.LongNameArguments;
            ShortNameArguments = InternalParserConfig.ShortNameArguments;
            PositionalArguments = InternalParserConfig.PositionalArguments;
            Verbs = InternalParserConfig.Verbs;
            PostParseMethods = InternalParserConfig.PostParseMethods;
            RequiredNamedArguments = InternalParserConfig.RequiredNamedArguments;
            PrecursorVerbs = precursorVerbs;
        }

        /// <inheritdoc/>
        public IValueStoreDefinition Store { get; set; }

        /// <inheritdoc/>
        public string[] PrecursorVerbs { get; set; }

        /// <inheritdoc/>
        private IParserConfig InternalParserConfig { get; set; }
    }
}
