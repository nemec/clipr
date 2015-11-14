using clipr.Core;

namespace clipr.Fluent
{
    public class PositionalList<TConfig, TArg> : PositionalBase<TConfig, PositionalList<TConfig, TArg>, TArg>
        where TConfig : class
    {
        public PositionalList(CliParserBuilder<TConfig> parser, IValueStoreDefinition store)
            : base(parser, store)
        {
            Consumes = new Consumes<TConfig, PositionalList<TConfig, TArg>, TArg>(this);
        }

        public Consumes<TConfig, PositionalList<TConfig, TArg>, TArg> Consumes { get; private set; }
    }
}
