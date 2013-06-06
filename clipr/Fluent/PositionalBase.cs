using System.Reflection;
using clipr.Arguments;

namespace clipr.Fluent
{
    public abstract class PositionalBase<TConfig, TPositional, TValue>
        : ArgumentBase<TConfig, TPositional, TValue>
        where TPositional : PositionalBase<TConfig, TPositional, TValue>
        where TConfig : class 
    {
        internal PositionalArgument Arg { get; set; }

        internal override BaseArgument BaseArgument { get { return Arg; } }

        protected PositionalBase(CliParser<TConfig> parser, PropertyInfo prop)
            : base(parser)
        {
            Arg = Arg = new PositionalArgument(prop);
        } 
    }
}
