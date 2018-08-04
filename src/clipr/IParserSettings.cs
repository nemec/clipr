using clipr.IOC;
using clipr.Triggers;
using clipr.Usage;
using System;
using System.Collections.Generic;
using System.IO;

namespace clipr
{
    public interface IParserSettings
    {
        /// <summary>
        /// Punctuation character prefixed to short and long argument
        /// names. Usually a hyphen (-).
        /// </summary>
        /// <exception cref="ArgumentIntegrityException">
        /// Character is not valid punctuation.
        /// </exception>
        char ArgumentPrefix { get; set; }

        /// <summary>
        /// Make short and long argument comparisons case insensitive.
        /// </summary>
        bool CaseInsensitive { get; set; }

        /// <summary>
        /// Partially match unambiguous long named arguments.
        /// E.g. "--che" will match the named argument "checkout" as
        /// long as no other named argument begins with "che".
        /// </summary>
        bool NamedPartialMatch { get; set; }

        /// <summary>
        /// Generates help documentation for this parser.
        /// </summary>
        IHelpGenerator HelpGenerator { get; set; }

        /// <summary>
        /// Generates a version string
        /// </summary>
        IVersion VersionGenerator { get; set; }

        /// <summary>
        /// Additional triggers beyond Help and Version that will be used when parsing
        /// </summary>
        List<ITerminatingTrigger> CustomTriggers { get; set; }

        /// <summary>
        /// Include the help trigger automatically in verbs.
        /// </summary>
        bool IncludeHelpTriggerInVerbs { get; set; }

        /// <summary>
        /// An event that is fired every time an argument is parsed, just
        /// after it is added to the Options object. If you would like to
        /// stop parsing in response to an event, return a non-null
        /// ITerminating trigger from the event handler. To continue
        /// parsing, simply return null.
        /// </summary>
        Func<ParseEventArgs, ITerminatingTrigger> OnParseArgument { get; set; }

        /// <summary>
        /// The text writer where extension or trigger output should
        /// be written.
        /// </summary>
        TextWriter OutputWriter { get; set; }

        /// <summary>
        /// The IOC factory used to generate necessary Verb objects.
        /// </summary>
        IVerbFactory VerbFactory { get; set; }
    }
}
