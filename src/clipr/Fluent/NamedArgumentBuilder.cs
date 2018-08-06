using clipr.Arguments;
using clipr.Core;
using System;
using System.Linq;

namespace clipr.Fluent
{
    public class NamedArgumentBuilder<TArg>
    {
        private readonly NamedArgument _arg;

        private readonly Type _defaultLocalizeResourceType;

        private readonly string _defaultLocalizeResourceName;

        private bool HasDefaultName { get; set; }

        internal NamedArgumentBuilder(
            NamedArgument arg, Type defaultLocalizeResourceType, string defaultLocalizeResourceName)
        {
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

        public NamedArgumentBuilder<TArg> WithMetaVar(string metavar)
        {
            _arg.MetaVar = metavar;
            return this;
        }

        public NamedArgumentBuilder<TArg> WithDescription(string description)
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
        public NamedArgumentBuilder<TArg> WithLocalizedDescription()
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
        public NamedArgumentBuilder<TArg> WithLocalizedDescription(
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
        public NamedArgumentBuilder<TArg> WithLocalizedDescription(
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
        public NamedArgumentBuilder<TArg> WithLocalizedDescription(
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
