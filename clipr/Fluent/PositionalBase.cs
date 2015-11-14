using clipr.Arguments;
using clipr.Core;

namespace clipr.Fluent
{
    public abstract class PositionalBase<TConfig, TPositional, TValue>
        : ArgumentBase<TConfig, TPositional, TValue>
        where TPositional : PositionalBase<TConfig, TPositional, TValue>
        where TConfig : class 
    {
        internal PositionalArgument Arg { get; set; }

        internal override BaseArgument BaseArgument { get { return Arg; } }

        protected PositionalBase(CliParserBuilder<TConfig> parser, IValueStoreDefinition store)
            : base(parser)
        {
            Arg = Arg = new PositionalArgument(store);
        } 
    }
}
