using System.Reflection;

namespace clipr.Fluent
{
    public class Named<TConfig, TArg> : NamedBase<TConfig, Named<TConfig, TArg>, TArg>
        where TConfig : class
    {
        internal Named(CliParser<TConfig> parser, PropertyInfo prop)
            : base(parser, prop)
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
