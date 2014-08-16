using System;
using clipr.Arguments;
using clipr.Core;

namespace clipr
{
    /// <summary>
    /// An argument triggered by either a short or long name.
    /// </summary>
    public class NamedArgumentAttribute : ArgumentAttribute, INamedArgument
    {
        /// <summary>
        /// Single character name for the argument.
        /// </summary>
        public char? ShortName { get; internal set; }

        /// <summary>
        /// Longer, multi-character name for the argument.
        /// </summary>
        public string LongName { get; internal set; }

        /// <inheritdoc/>
        public bool Required { get; set; }

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

        /// <summary>
        /// Create a new named argument with only a short name.
        /// </summary>
        /// <param name="shortName"></param>
        public NamedArgumentAttribute(char shortName)
        {
            ShortName = shortName;
        }

        /// <summary>
        /// Create a new named argument with only a long name.
        /// </summary>
        /// <param name="longName"></param>
        public NamedArgumentAttribute(string longName)
        {
            LongName = longName;
        }

        /// <summary>
        /// Create a new named argument with both a short and long name.
        /// </summary>
        /// <param name="shortName"></param>
        /// <param name="longName"></param>
        public NamedArgumentAttribute(char shortName, string longName)
        {
            ShortName = shortName;
            LongName = longName;
        }
    }
}
