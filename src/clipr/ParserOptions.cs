using clipr.Core;
using clipr.Triggers;
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
        /// An event that is fired every time an argument is parsed, just
        /// after it is added to the Options object.
        /// </summary>
        public event Action<object, ParseEventArgs> OnParseArgument;

        /// <summary>
        /// Events may only be sent by the class they're defined on,
        /// so use this class as a springboard.
        /// </summary>
        /// <param name="ctx">Used to disambiguate between multiple calls to ParseArguments</param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal ITerminatingTrigger SendArgument(IParsingContext ctx, string name, object value)
        {
            var hnd = OnParseArgument;
            if(hnd != null)
            {
                var arg = new ParseEventArgs(name, value);
                hnd.Invoke(ctx, arg);
                return arg.StopParsing;
            }
            return null;
        }

        /// <summary>
        /// The text writer where extension or trigger output should
        /// be written.
        /// </summary>
        public TextWriter OutputWriter { get; set; }
    }
}
