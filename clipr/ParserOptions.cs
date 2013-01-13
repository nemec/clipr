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
        CaseInsensitive
    }
}
