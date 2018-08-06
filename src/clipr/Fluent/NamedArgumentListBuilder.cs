using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using clipr.Arguments;
using clipr.Core;

namespace clipr.Fluent
{
    public class NamedArgumentListBuilder<TArg>
    {
        private readonly NamedArgument _arg;

        private readonly Type _defaultLocalizeResourceType;

        private readonly string _defaultLocalizeResourceName;

        private bool HasDefaultName { get; set; }

        internal NamedArgumentListBuilder(
            NamedArgument arg, Type defaultLocalizeResourceType, string defaultLocalizeResourceName)
        {
            _arg.Action = ParseAction.Append;
            _arg = arg;
            _defaultLocalizeResourceType = defaultLocalizeResourceType;
            _defaultLocalizeResourceName = defaultLocalizeResourceName;
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
        public NamedArgumentListBuilder<TArg> WithMetaVar(string metavar)
        {
            _arg.MetaVar = metavar;
            return this;
        }

        /// <summary>
        /// Specify a description for the argument.
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public NamedArgumentListBuilder<TArg> WithDescription(string description)
        {
            _arg.Description = description;
            return this;
        }

        /// <summary>
        /// Localize the description of this argument using resource files.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Thrown when the <see cref="CliParserBuilder{TConf}"/> does not
        /// have a localization class defined. To set one, call
        /// <code>builder.Localize(typeof(ResourceType))</code> with your
        /// resx resource type.
        /// </exception>
        /// <returns></returns>
        public NamedArgumentListBuilder<TArg> WithLocalizedDescription()
        {
            return WithLocalizedDescription(null, _defaultLocalizeResourceName);
        }

        /// <summary>
        /// Localize the description of this argument using resource files.
        /// </summary>
        /// <param name="invariantDescription">
        /// The default description used under the invariant locale, or when
        /// the description is needed in an untranslated locale.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when the <see cref="CliParserBuilder{TConf}"/> does not
        /// have a localization class defined. To set one, call
        /// <code>builder.Localize(typeof(ResourceType))</code> with your
        /// resx resource type.
        /// </exception>
        /// <returns></returns>
        public NamedArgumentListBuilder<TArg> WithLocalizedDescription(
            string invariantDescription)
        {
            return WithLocalizedDescription(invariantDescription, _defaultLocalizeResourceName);
        }

        /// <summary>
        /// Localize the description of this argument using resource files.
        /// </summary>
        /// <param name="invariantDescription">
        /// The default description used under the invariant locale, or when
        /// the description is needed in an untranslated locale.
        /// </param>
        /// <param name="resourceName">
        /// The name of the resx resource that holds the translation.
        /// </param>
        /// <returns></returns>
        public NamedArgumentListBuilder<TArg> WithLocalizedDescription(
            string invariantDescription, string resourceName)
        {
            if (_defaultLocalizeResourceType is null)
            {
                throw new ArgumentException(
                    "If you do not explicitly specify a Resource Type on this argument, one must be " +
                    "set on the builder by calling 'builder.Localize(typeof(ResourceType))'");
            }
            return WithLocalizedDescription(invariantDescription, _defaultLocalizeResourceType, resourceName);
        }

        /// <summary>
        /// Localize the description of this argument using resource files.
        /// </summary>
        /// <param name="invariantDescription">
        /// The default description used under the invariant locale, or when
        /// the description is needed in an untranslated locale.
        /// </param>
        /// <param name="resourceType">
        /// The type containing the resources for this item.
        /// </param>
        /// <param name="resourceName">
        /// The name of the resx resource that holds the translation.
        /// </param>
        /// <returns></returns>
        public NamedArgumentListBuilder<TArg> WithLocalizedDescription(
            string invariantDescription, Type resourceType, string resourceName)
        {
            _arg.Description = invariantDescription;
            _arg.LocalizationInfo = new LocalizationInfo
            {
                ResourceType = resourceType,
                ResourceName = resourceName
            };
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
