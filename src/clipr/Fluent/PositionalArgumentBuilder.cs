using clipr.Arguments;
using clipr.Core;
using System;
using System.Linq;

namespace clipr.Fluent
{
    public class PositionalArgumentBuilder<TArg>
    {
        private readonly PositionalArgument _arg;

        private readonly Type _defaultLocalizeResourceType;

        private readonly string _defaultLocalizeResourceName;

        private bool HasDefaultName { get; set; }

        internal PositionalArgumentBuilder(
            PositionalArgument arg, Type defaultLocalizeResourceType, string defaultLocalizeResourceName)
        {
            _arg = arg;
            _defaultLocalizeResourceType = defaultLocalizeResourceType;
            _defaultLocalizeResourceName = defaultLocalizeResourceName;
        }

        public PositionalArgumentBuilder<TArg> WithMetaVar(string metavar)
        {
            _arg.MetaVar = metavar;
            return this;
        }

        public PositionalArgumentBuilder<TArg> WithDescription(string description)
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
        public PositionalArgumentBuilder<TArg> WithLocalizedDescription()
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
        public PositionalArgumentBuilder<TArg> WithLocalizedDescription(
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
        public PositionalArgumentBuilder<TArg> WithLocalizedDescription(
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
        public PositionalArgumentBuilder<TArg> WithLocalizedDescription(
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
