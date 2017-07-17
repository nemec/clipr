using System;
using System.Collections.Generic;
using clipr.Core;
using clipr.Triggers;
using clipr.Usage;
using clipr.Utils;
using clipr.IOC;
using System.Linq;

namespace clipr
{
    /// <summary>
    /// Static shortcuts for quickly parsing argument lists.
    /// </summary>
    public static class CliParser
    {

        /// <summary>
        /// <para>
        /// Parse the given argument list and return a new object
        /// containing the converted arguments.
        /// </para>
        /// <para>
        /// If parsing fails, error details are immediately written
        /// to the error console and help information is displayed.
        /// </para>
        /// <para>
        /// WARNING: This method may call <see cref="Environment.Exit"/> on
        /// error.
        /// </para>
        /// </summary>
        /// <param name="args">Argument list to parse.</param>
        /// <returns>
        /// A new object containing values parsed from the argument list.
        /// </returns>
        public static TS StrictParse<TS>(string[] args) where TS : class, new()
        {
            var obj = new TS();
            StrictParse(args, obj);
            return obj;
        }

        /// <summary>
        /// <para>
        /// Parse the given argument list.
        /// </para>
        /// <para>
        /// If parsing fails, error details is immediately written
        /// to the error console and help information is displayed.
        /// </para>
        /// <para>
        /// WARNING: This method may call <see cref="Environment.Exit"/> on
        /// error.
        /// </para>
        /// </summary>
        /// <param name="obj">Parsed arguments are stored here.</param>
        /// <param name="args">Argument list to parse.</param>
        public static void StrictParse<TS>(string[] args, TS obj) where TS : class
        {
            var parser = new CliParser<TS>();
            var errs = parser.ValidateAttributeConfig();
            if (errs.Any())
            {
                foreach (var err in errs)
                {
                    Console.Error.WriteLine(err);
                }
                Environment.Exit(2);
            }
            parser.StrictParse(args, obj);
        }

        /// <summary>
        /// Make an attempt to parse the arguments and return true if
        /// parsing was successful. If parsing completed, the
        /// <paramref name="obj"/> parameter will contain the constructed
        /// and parsed object. If parsing fails, the object will be null.
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <param name="args">Argument list to parse.</param>
        /// <param name="obj">Newly created destination object or null if parsing failed.</param>
        /// <returns>True if parsing succeeded.</returns>
        public static bool TryParse<TS>(string[] args, out TS obj) where TS : class, new()
        {
            obj = null;
            var tmpObj = new TS();
            if (TryParse(args, tmpObj))
            {
                obj = tmpObj;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Make an attempt to parse the arguments and return true if
        /// parsing was successful. If it returns false, the destination
        /// object may be left in an incomplete state.
        /// </summary>
        /// <typeparam name="TS"></typeparam>
        /// <param name="obj">Destination object.</param>
        /// <param name="args">Argument list to parse.</param>
        /// <returns>True if parsing succeeded.</returns>
        public static bool TryParse<TS>(string[] args, TS obj) where TS : class
        {
            var parser = new CliParser<TS>();
            return parser.TryParse(args, obj);
        }

        /// <summary>
        /// Parse the given argument list and return a new object
        /// containing the converted arguments.
        /// </summary>
        /// <param name="args">Argument list to parse.</param>
        /// <returns>
        /// A new object containing values parsed from the argument list.
        /// </returns>
        public static ParseResult<TS> Parse<TS>(string[] args) where TS : class, new()
        {
            var obj = new TS();
            return Parse(args, obj);
        }

        /// <summary>
        /// Parse the given argument list.
        /// </summary>
        /// <param name="args">Argument list to parse.</param>
        /// <param name="obj">Parsed arguments will be store here.</param>
        public static ParseResult<TS> Parse<TS>(string[] args, TS obj) where TS : class
        {
            var parser = new CliParser<TS>();
            return parser.Parse(args, obj);
        }
    }

    /// <summary>
    /// Parses a set of argument strings into an object.
    /// </summary>
    /// <typeparam name="TConf">Object type being deserialized.</typeparam>
    public class CliParser<TConf> where TConf : class
    {
        #region Public Properties

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

        #endregion

        #region Private Properties

        internal const int ErrorExitCode = 2;

        private IParserConfig Config { get; set; }

        private ParserOptions Options { get; set; }

        private IHelpGenerator HelpGenerator { get; set; }

        private IEnumerable<ITerminatingTrigger> Triggers { get; set; }

        private IVerbFactory Factory { get; set; }

        #endregion

        #region ctors

        /// <summary>
        /// Create a new parser with the default usage generator.
        /// </summary>
        public CliParser()
            : this(ParserOptions.Default, new AutomaticHelpGenerator<TConf>())
        {
        }

        /// <summary>
        /// Create a new parser with a set of options and the
        /// default usage generator.
        /// </summary>
        /// <param name="options">Extra options for the parser.</param>
        public CliParser(ParserOptions options)
            : this(options, new AutomaticHelpGenerator<TConf>())
        {
        }

        /// <summary>
        /// Create a new parser with a custom usage generator.
        /// </summary>
        /// <param name="usageGenerator">
        /// Generates help documentation for this parser.
        /// </param>
        public CliParser(IHelpGenerator usageGenerator)
            : this(ParserOptions.Default, usageGenerator)
        {
        }

        /// <summary>
        /// Create a new parser with a set of options and a custom
        /// usage generator.
        /// </summary>
        /// <param name="options">Extra options for the parser.</param>
        /// <param name="usageGenerator">
        /// Generates help documentation for this parser.
        /// </param>
        public CliParser(ParserOptions options, IHelpGenerator usageGenerator)
            : this(options, usageGenerator, new ParameterlessVerbFactory())
        {
        }

        /// <summary>
        /// Create a new parser with a set of options and a custom
        /// usage generator.
        /// </summary>
        /// <param name="options">Extra options for the parser.</param>
        /// <param name="usageGenerator">
        /// Generates help documentation for this parser.
        /// </param>
        /// <param name="factory">The IOC factory used to generate necessary Verb objects.</param>
        public CliParser(ParserOptions options, IHelpGenerator usageGenerator, IVerbFactory factory)
        {
            Options = options;
            HelpGenerator = usageGenerator;
            Factory = factory;

            Triggers = new ITerminatingTrigger[]
            {
                usageGenerator,
                new ExecutingAssemblyVersion()
            };
        }

        /// <summary>
        /// Generates a new parser from the fluent builder.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="options"></param>
        internal CliParser(ParserConfig<TConf> config, ParserOptions options)
        {
            Config = config;
            Options = options;
            // TODO help
        }

        #endregion

        /// <summary>
        /// Checks the configuration type TConf for any attribute issues.
        /// </summary>
        /// <returns></returns>
        public Exception[] ValidateAttributeConfig()
        {
            var checker = new IntegrityChecker();
            return checker.EnsureAttributeIntegrity<TConf>(Options)
                .Concat(checker.EnsureVerbIntegrity<TConf>(Factory))
                .Concat(checker.EnsureTriggerIntegrity(Triggers))
                .ToArray();
        }

        /// <summary>
        /// Checks the configuration type TConf for any attribute issues.
        /// </summary>
        /// <returns></returns>
        public void EnsureValidAttributeConfig()
        {
            var checker = new IntegrityChecker();
            var errs = checker.EnsureAttributeIntegrity<TConf>(Options)
                .Concat(checker.EnsureVerbIntegrity<TConf>(Factory))
                .Concat(checker.EnsureTriggerIntegrity(Triggers))
                .ToList();

            if (errs.Any())
            {

                throw new Utils.AggregateException(errs);
            }
        }

        public IParserConfig BuildConfig()
        {
            if (Config != null)
            {
                return Config;
            }
            return Config = new AttributeParserConfig<TConf>(Options, Triggers, Factory);
        }

        #region Public Parsing Methods

        /// <summary>
        /// <para>
        /// Parses the given argument list.
        /// </para>
        /// <para>
        /// If parsing fails, error details will immediately be written
        /// to the error console and help information will be displayed.
        /// </para>
        /// <para>
        /// WARNING: This method may call <see cref="Environment.Exit"/> on
        /// error.
        /// </para>
        /// </summary>
        /// <param name="args">Argument list to parse.</param>
        /// <param name="obj">Object to fill with parsed data.</param>
        /// <returns>The object parsed from the argument list.</returns>
        public ParseResult<TConf> StrictParse(string[] args, TConf obj)
        {
            var result = Parse(args, obj);
            return result.Handle(
                val => result,
                trig => result,
                errs =>
                {
                    Console.Error.WriteLine(HelpGenerator.GetUsage(Config));
                    foreach (var err in errs)
                    {
                        Console.Error.WriteLine(err);
                    }
                    Environment.Exit(ErrorExitCode);
                    throw new InvalidOperationException();
                });
        }

        /// <summary>
        /// Make an attempt to parse the arguments and return true if
        /// parsing was successful. If it returns false, the destination
        /// object may be left in an incomplete state.
        /// </summary>
        /// <param name="args">Argument list to parse.</param>
        /// <param name="obj">Object to fill with parsed data.</param>
        /// <returns>True if parsing succeeded.</returns>
        public bool TryParse(string[] args, TConf obj)
        {
            // TODO should triggers be handled automatically in this case?
            return Parse(args, obj).Handle(
                val => true,
                trig => true,
                errs => false);
        }

        /// <summary>
        /// Parse the given argument list.
        /// </summary>
        /// <exception cref="ParseException">
        /// An error happened while parsing.
        /// </exception>
        /// <exception cref="ParserExit">
        /// Either the help or version information were triggered so
        /// parsing was aborted.
        /// </exception>
        /// <param name="args">Argument list to parse.</param>
        /// <param name="obj">Object to fill with parsed data.</param>
        public ParseResult<TConf> Parse(string[] args, TConf obj)
        {
            var conf = BuildConfig();
            conf.ArgumentPrefix = ArgumentPrefix;
            return new ParsingContext<TConf>(obj, conf).Parse(args);
        }

        #endregion
    }
}
