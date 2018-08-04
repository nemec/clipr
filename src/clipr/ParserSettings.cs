using clipr.Core;
using clipr.IOC;
using clipr.Triggers;
using clipr.Usage;
using System;
using System.Collections.Generic;
using System.IO;

namespace clipr
{
    /// <summary>
    /// Extra options that may be supplied to the parser.
    /// </summary>
    public class ParserSettings<TConfig> : IParserSettings
    {
        public ParserSettings()
        {
            IncludeHelpTriggerInVerbs = true;
            OutputWriter = Console.Error;
            VerbFactory = new ParameterlessVerbFactory();
            HelpGenerator = new AutomaticHelpGenerator<TConfig>();
            VersionGenerator = new ExecutingAssemblyVersion();
            CustomTriggers = new List<ITerminatingTrigger>();
        }

        public static readonly ParserSettings<TConfig> Default = new ParserSettings<TConfig>();

        /// <summary>
        /// Punctuation character prefixed to short and long argument
        /// names. Usually a hyphen (-).
        /// </summary>
        /// <exception cref="ArgumentIntegrityException">
        /// Character is not valid punctuation.
        /// </exception>
        public char ArgumentPrefix
        {
            get { return _argumentPrefix; }
            set
            {
                if (!Char.IsPunctuation(value))
                {
                    throw new ArgumentIntegrityException(
                        "Argument prefix must be a punctuation character.");
                }
                _argumentPrefix = value;
            }
        }
        private char _argumentPrefix = '-';

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
        /// Generates help documentation for this parser.
        /// </summary>
        public IHelpGenerator HelpGenerator { get; set; }

        /// <summary>
        /// Generates a version string
        /// </summary>
        public IVersion VersionGenerator { get; set; }

        /// <summary>
        /// Additional triggers beyond Help and Version that will be used when parsing
        /// </summary>
        public List<ITerminatingTrigger> CustomTriggers { get; set; }

        /// <summary>
        /// Include the help trigger automatically in verbs.
        /// </summary>
        public bool IncludeHelpTriggerInVerbs { get; set; }

        /// <summary>
        /// An event that is fired every time an argument is parsed, just
        /// after it is added to the Options object. If you would like to
        /// stop parsing in response to an event, return a non-null
        /// ITerminating trigger from the event handler. To continue
        /// parsing, simply return null.
        /// </summary>
        public Func<ParseEventArgs, ITerminatingTrigger> OnParseArgument { get; set; }

        /// <summary>
        /// The text writer where extension or trigger output should
        /// be written.
        /// </summary>
        public TextWriter OutputWriter { get; set; }

        /// <summary>
        /// The IOC factory used to generate necessary Verb objects.
        /// </summary>
        public IVerbFactory VerbFactory { get; set; }
    }
}
