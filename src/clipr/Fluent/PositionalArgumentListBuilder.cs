using clipr.Arguments;

namespace clipr.Fluent
{
    public class PositionalArgumentListBuilder<TArg>
    {
        private readonly PositionalArgument _arg;

        internal PositionalArgumentListBuilder(PositionalArgument arg)
        {
            _arg = arg;
            _arg.Action = ParseAction.Append;
        }

        public PositionalArgumentListBuilder<TArg> HasMetaVar(string metavar)
        {
            _arg.MetaVar = metavar;
            return this;
        }

        public PositionalArgumentListBuilder<TArg> HasDescription(string description)
        {
            _arg.Description = description;
            return this;
        }

        /// <summary>
        /// Indicates that this argument appends a constant value to the
        /// option property each time it is specified instead of a
        /// value that is provided on the command line.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public PositionalArgumentListBuilder<TArg> AppendsValue(TArg value)
        {
            _arg.Action = ParseAction.AppendConst;
            _arg.Const = value;
            return this;
        }

        /// <summary>
        /// Indicates that this argument consumes multiple values with a lower bound.
        /// </summary>
        /// <param name="numArguments"></param>
        /// <returns></returns>
        public PositionalArgumentListBuilder<TArg> ConsumesAtLeast(uint numArguments)
        {
            _arg.Constraint = NumArgsConstraint.AtLeast;
            _arg.NumArgs = numArguments;
            return this;
        }

        /// <summary>
        /// Indicates that this argument consumes multiple values with an upper bound.
        /// </summary>
        /// <param name="numArguments"></param>
        /// <returns></returns>
        public PositionalArgumentListBuilder<TArg> ConsumesAtMost(uint numArguments)
        {
            _arg.Constraint = NumArgsConstraint.AtMost;
            _arg.NumArgs = numArguments;
            return this;
        }

        /// <summary>
        /// Indicates that this argument consumes an exact number of values.
        /// </summary>
        /// <param name="numArguments"></param>
        /// <returns></returns>
        public PositionalArgumentListBuilder<TArg> ConsumesExactly(uint numArguments)
        {
            _arg.Constraint = NumArgsConstraint.Exactly;
            _arg.NumArgs = numArguments;
            return this;
        }

        public PositionalArgumentListBuilder<TArg> PromptIfValueIsMissing(
            bool maskInput = false, string signalString = null)
        {
            var prompt = new PromptIfValueMissing
            {
                Enabled = true,
                MaskInput = maskInput
            };
            if (signalString != null)
            {
                prompt.SignalString = signalString;
            }
            _arg.PromptIfValueMissing = prompt;
            return this;
        }
    }
}
