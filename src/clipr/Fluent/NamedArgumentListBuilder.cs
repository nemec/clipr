using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using clipr.Arguments;

namespace clipr.Fluent
{
    public class NamedArgumentListBuilder<TArg>
    {
        private readonly NamedArgument _arg;

        private bool HasDefaultName { get; set; }

        internal NamedArgumentListBuilder(NamedArgument arg)
        {
            _arg.Action = ParseAction.Append;
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
        public NamedArgumentListBuilder<TArg> WithShortName()
        {
            return WithShortName(_arg.Name.First());
        }

        /// <summary>
        /// Use the given character as the short argument name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public NamedArgumentListBuilder<TArg> WithShortName(char name)
        {
            ClearArgumentNamesIfNeeded();
            _arg.ShortName = Char.ToLowerInvariant(name);
            return this;
        }

        /// <summary>
        /// Use the property's lowercased name as the long argument name.
        /// </summary>
        /// <returns></returns>
        public NamedArgumentListBuilder<TArg> WithLongName()
        {
            return WithLongName(_arg.Name.ToLowerInvariant());
        }

        /// <summary>
        /// Use the given name as the long argument name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public NamedArgumentListBuilder<TArg> WithLongName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            ClearArgumentNamesIfNeeded();
            _arg.LongName = name.ToLowerInvariant();
            return this;
        }

        /// <summary>
        /// Specify a meta-variable name. Used in help documents to
        /// show where the value should be added.
        /// </summary>
        /// <param name="metavar"></param>
        /// <returns></returns>
        public NamedArgumentListBuilder<TArg> HasMetaVar(string metavar)
        {
            _arg.MetaVar = metavar;
            return this;
        }

        /// <summary>
        /// Specify a description for the argument.
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public NamedArgumentListBuilder<TArg> HasDescription(string description)
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
        public NamedArgumentListBuilder<TArg> AppendsValue(TArg value)
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
        public NamedArgumentListBuilder<TArg> ConsumesAtLeast(uint numArguments)
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
        public NamedArgumentListBuilder<TArg> ConsumesAtMost(uint numArguments)
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
        public NamedArgumentListBuilder<TArg> ConsumesExactly(uint numArguments)
        {
            _arg.Constraint = NumArgsConstraint.Exactly;
            _arg.NumArgs = numArguments;
            return this;
        }

        /// <summary>
        /// Marks this named argument as required.
        /// </summary>
        /// <returns></returns>
        public NamedArgumentListBuilder<TArg> IsRequired()
        {
            _arg.Required = true;
            return this;
        }

        public NamedArgumentListBuilder<TArg> PromptIfValueIsMissing(
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
