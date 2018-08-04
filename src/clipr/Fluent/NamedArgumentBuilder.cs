using clipr.Arguments;
using System;
using System.Linq;

namespace clipr.Fluent
{
    public class NamedArgumentBuilder<TArg>
    {
        private readonly NamedArgument _arg;

        private bool HasDefaultName { get; set; }

        internal NamedArgumentBuilder(NamedArgument arg)
        {
            _arg = arg;
        }

        /// <summary>
        /// Delete the 'default' short and long name if explicitly set.
        /// </summary>
        private void ClearArgumentNamesIfNeeded()
        {
            if (!HasDefaultName) return;

            _arg.ShortName = null;
            _arg.LongName = null;
            HasDefaultName = true;
        }

        /// <summary>
        /// Use the property's lowercased first character as the short
        /// argument name.
        /// </summary>
        /// <returns></returns>
        public NamedArgumentBuilder<TArg> WithShortName()
        {
            return WithShortName(_arg.Name.First());
        }

        /// <summary>
        /// Use the given character as the short argument name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public NamedArgumentBuilder<TArg> WithShortName(char name)
        {
            ClearArgumentNamesIfNeeded();
            _arg.ShortName = Char.ToLowerInvariant(name);
            return this;
        }

        /// <summary>
        /// Use the property's lowercased name as the long argument name.
        /// </summary>
        /// <returns></returns>
        public NamedArgumentBuilder<TArg> WithLongName()
        {
            return WithLongName(_arg.Name.ToLowerInvariant());
        }

        /// <summary>
        /// Use the given name as the long argument name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public NamedArgumentBuilder<TArg> WithLongName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            ClearArgumentNamesIfNeeded();
            _arg.LongName = name.ToLowerInvariant();
            return this;
        }

        public NamedArgumentBuilder<TArg> HasMetaVar(string metavar)
        {
            _arg.MetaVar = metavar;
            return this;
        }

        public NamedArgumentBuilder<TArg> HasDescription(string description)
        {
            _arg.Description = description;
            return this;
        }

        public NamedArgumentBuilder<TArg> StoresValue(TArg obj)
        {
            _arg.Action = ParseAction.StoreConst;
            _arg.Const = obj;
            return this;
        }

        public NamedArgumentBuilder<TArg> ConsumesOptionalValue()
        {
            _arg.Constraint = NumArgsConstraint.Optional;
            return this;
        }

        public NamedArgumentBuilder<TArg> IsRequired()
        {
            _arg.Required = true;
            return this;
        }

        public NamedArgumentBuilder<TArg> PromptIfValueIsMissing(
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

        // internal because it is restricted only to bool arg types
        // check fluentextensions.cs for specialization extensions
        internal NamedArgumentBuilder<TArg> StoresTrue()
        {
            _arg.Action = ParseAction.StoreTrue;
            return this;
        }

        // internal because it is restricted only to bool arg types
        // check fluentextensions.cs for specialization extensions
        internal NamedArgumentBuilder<TArg> StoresFalse()
        {
            _arg.Action = ParseAction.StoreFalse;
            return this;
        }

        // internal because it is restricted only to int arg types
        // check fluentextensions.cs for specialization extensions
        internal NamedArgumentBuilder<TArg> CountsInvocations()
        {
            _arg.Action = ParseAction.Count;
            return this;
        }
    }
}
