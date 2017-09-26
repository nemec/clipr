using clipr.Arguments;

namespace clipr.Fluent
{
    public abstract class ArgumentBase<TConfig, TThis, TValue>
        where TThis : ArgumentBase<TConfig, TThis, TValue>
        where TConfig : class
    {
        internal abstract BaseArgument BaseArgument { get; }

        /// <summary>
        /// Finish configuring the current argument and return to the
        /// parser.
        /// </summary>
        public CliParserBuilder<TConfig> And { get; set; }

        protected ArgumentBase(CliParserBuilder<TConfig> parser)
        {
            And = parser;
        } 

        public TThis HasMetaVar(string metavar)
        {
            BaseArgument.MetaVar = metavar;
            return (TThis)this;
        }

        public TThis HasDescription(string description)
        {
            BaseArgument.Description = description;
            return (TThis)this;
        }

        public TThis AppendsValue()
        {
            BaseArgument.Action = ParseAction.Append;
            return (TThis)this;
        }

        public TThis AppendsValue(TValue value)
        {
            BaseArgument.Action = ParseAction.AppendConst;
            BaseArgument.Const = value;
            return (TThis)this;
        }
    }
}
