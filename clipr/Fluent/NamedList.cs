using System.Reflection;

namespace clipr.Fluent
{
    public class NamedList<TConfig, TArg> : NamedBase<TConfig, NamedList<TConfig, TArg>, TArg>
        where TConfig : class 
    {
        internal NamedList(CliParser<TConfig> parser, PropertyInfo prop)
            : base(parser, prop)
        {
            Consumes = new Consumes<TConfig, NamedBase<TConfig, NamedList<TConfig, TArg>, TArg>, TArg>(this);
        }

        public Consumes<TConfig, NamedBase<TConfig, NamedList<TConfig, TArg>, TArg>, TArg> Consumes { get; private set; }

        public new NamedList<TConfig, TArg> AppendsValue()
        {
            return (NamedList<TConfig, TArg>)base.AppendsValue();
        }

        public new NamedList<TConfig, TArg> AppendsValue(TArg value)
        {
            return (NamedList<TConfig, TArg>)base.AppendsValue(value);
        }
    }
}
