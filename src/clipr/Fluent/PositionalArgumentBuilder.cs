using clipr.Arguments;
using System;
using System.Linq;

namespace clipr.Fluent
{
    public class PositionalArgumentBuilder<TArg>
    {
        private readonly PositionalArgument _arg;

        private bool HasDefaultName { get; set; }

        internal PositionalArgumentBuilder(PositionalArgument arg)
        {
            _arg = arg;
        }

        public PositionalArgumentBuilder<TArg> HasMetaVar(string metavar)
        {
            _arg.MetaVar = metavar;
            return this;
        }

        public PositionalArgumentBuilder<TArg> HasDescription(string description)
        {
            _arg.Description = description;
            return this;
        }

        public PositionalArgumentBuilder<TArg> StoresValue(TArg obj)
        {
            _arg.Action = ParseAction.StoreConst;
            _arg.Const = obj;
            return this;
        }

        internal PositionalArgumentBuilder<TArg> StoresTrue()
        {
            _arg.Action = ParseAction.StoreTrue;
            return this;
        }

        internal PositionalArgumentBuilder<TArg> StoresFalse()
        {
            _arg.Action = ParseAction.StoreFalse;
            return this;
        }

        internal PositionalArgumentBuilder<TArg> CountsInvocations()
        {
            _arg.Action = ParseAction.Count;
            return this;
        }

        public PositionalArgumentBuilder<TArg> PromptIfValueIsMissing(
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
