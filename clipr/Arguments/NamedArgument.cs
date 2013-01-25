using System;
using System.Reflection;

namespace clipr.Arguments
{
    internal class NamedArgument : BaseArgument, INamedArgument
    {
        /// <summary>
        /// Single character name for the argument.
        /// </summary>
        public char? ShortName { get; set; }

        /// <summary>
        /// Longer, multi-character name for the argument.
        /// </summary>
        public string LongName { get; set; }

        /// <summary>
        /// An argument name suitable for displaying on a help page.
        /// 
        /// Defaults to either the short or long name.
        /// </summary>
        public override string MetaVar
        {
            get
            {
                if (!String.IsNullOrEmpty(base.MetaVar))
                {
                    return base.MetaVar;
                }
                if (ShortName != default(char))
                {
                    return ShortName.ToString();
                }
                if (!String.IsNullOrEmpty(LongName))
                {
                    return LongName;
                }
                
                return null;
            }
            set
            {
                base.MetaVar = value;
            }
        }

        public NamedArgument(PropertyInfo prop)
            : base(prop)
        {
        }

        internal override string GetArgumentDisplayName()
        {
            if (!String.IsNullOrEmpty(LongName))
            {
                return LongName;
            }
            if (ShortName.HasValue && Char.IsLetterOrDigit(ShortName.Value))
            {
                return ShortName.ToString();
            }
            return null;
        }
    }
}
