using System;
using System.IO;

namespace clipr
{
    /// <summary>
    /// Extra options that may be supplied to the parser.
    /// </summary>
    public class ParserOptions
    {
        public ParserOptions()
        {
            IncludeHelpTriggerInVerbs = true;
            OutputWriter = Console.Error;
        }

        public static readonly ParserOptions Default = new ParserOptions();

        /// <summary>
        /// Make short and long argument comparisons case insensitive.
        /// </summary>
        public bool CaseInsensitive { get; set; }

        /// <summary>
        /// Partially match unambiguous long named arguments.
        /// E.g. "--che" will match the named argument "checkout" as
        /// long as no other named argument begins with "che".
        /// </summary>
        public bool NamedPartialMatch { get; set; }

        /// <summary>
        /// Include the help trigger automatically in verbs.
        /// </summary>
        public bool IncludeHelpTriggerInVerbs { get; set; }

        /// <summary>
        /// The text writer where extension or trigger output should
        /// be written.
        /// </summary>
        public TextWriter OutputWriter { get; set; }
    }
}
