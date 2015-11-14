using clipr.Core;

namespace clipr.Fluent
{
    public class Named<TConfig, TArg> : NamedBase<TConfig, Named<TConfig, TArg>, TArg>
        where TConfig : class
    {
        internal Named(CliParserBuilder<TConfig> parser, IValueStoreDefinition store)
            : base(parser, store)
        {
        }

        public Named<TConfig, TArg> StoresValue(TArg obj)
        {
            Arg.Action = ParseAction.StoreConst;
            Arg.Const = obj;
            return this;
        }
    }
}
