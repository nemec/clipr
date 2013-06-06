using System.Reflection;

namespace clipr.Fluent
{
    public class Positional<TConf, TArg> : PositionalBase<TConf, Positional<TConf, TArg>, TArg>
        where TConf : class
    {
        internal Positional(CliParser<TConf> parser, PropertyInfo prop)
            : base(parser, prop)
        {
        } 
    }
}
