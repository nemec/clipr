using System;
using clipr.Core;

namespace clipr.Arguments
{
    internal class NamedArgument : BaseArgument, INamedArgument
    {
        public new string Name
        {
            get
            {
                if (!String.IsNullOrEmpty(LongName))
                {
                    return LongName;
                }
                if (ShortName.HasValue && Char.IsLetterOrDigit(ShortName.Value))
                {
                    return ShortName.ToString();
                }
                return base.Name;
            }
            internal set
            {
                base.Name = value;
            }
        }

        public char? ShortName { get; set; }

        public string LongName { get; set; }

        public bool Required { get; set; }

        public PromptIfValueMissing PromptIfValueMissing { get; set; }

        public NamedArgument(IValueStoreDefinition store)
            : base(store)
        {
        }
    }
}
