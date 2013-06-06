using System.Reflection;

namespace clipr.Fluent
{
    public class PositionalList<TConfig, TArg> : PositionalBase<TConfig, PositionalList<TConfig, TArg>, TArg>
        where TConfig : class
    {
        public PositionalList(CliParser<TConfig> parser, PropertyInfo prop)
            : base(parser, prop)
        {
            Consumes = new Consumes<TConfig, PositionalList<TConfig, TArg>, TArg>(this);
        }

        public Consumes<TConfig, PositionalList<TConfig, TArg>, TArg> Consumes { get; private set; }
    }
}
