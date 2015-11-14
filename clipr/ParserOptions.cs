using System;

namespace clipr
{
    /// <summary>
    /// Extra options that may be supplied to the parser.
    /// </summary>
    [Flags]
    public enum ParserOptions
    {
        /// <summary>
        /// Add no extra options.
        /// </summary>
        None = 0,

        /// <summary>
        /// Make short and long argument comparisons case insensitive.
        /// </summary>
        CaseInsensitive = 1,

        /// <summary>
        /// Partially match unambiguous long named arguments.
        /// E.g. "--che" will match the named argument "checkout" as
        /// long as no other named argument begins with "che".
        /// </summary>
        NamedPartialMatch = 2
    }
}
