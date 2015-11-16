using System;
using System.Collections.Generic;
using clipr.Core;
using clipr.Triggers;
using clipr.Usage;
using clipr.Utils;

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
            CliParser<TS> parser = null;
            try
            {
                parser = new CliParser<TS>(obj);
            }
            catch (ArgumentIntegrityException e)
            {
                Console.Error.WriteLine(e.Message);
                if (parser != null && parser.Config != null)
                {
                    Console.Error.WriteLine(
                        new AutomaticHelpGenerator<TS>().GetUsage(parser.Config));
                }
                Environment.Exit(2);
            }
            catch (AggregateException ex)
            {
                if (parser != null && parser.Config != null)
                {
                    Console.Error.WriteLine(
                        new AutomaticHelpGenerator<TS>().GetUsage(parser.Config));
                }
                ex.Handle(e =>
                {
                    Console.Error.WriteLine(e.Message);
                    return true;
                });
                Environment.Exit(2);
            }
            parser.StrictParse(args);
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
            var parser = new CliParser<TS>(obj);
            return parser.TryParse(args);
        }

        /// <summary>
        /// Parse the given argument list and return a new object
        /// containing the converted arguments.
        /// </summary>
        /// <exception cref="ParseException">
        /// An error happened while parsing.
        /// </exception>
        /// <exception cref="ParserExit">
        /// Either the help or version information were triggered so
        /// parsing was aborted.
        /// </exception>
        /// <param name="args">Argument list to parse.</param>
        /// <returns>
        /// A new object containing values parsed from the argument list.
        /// </returns>
        public static TS Parse<TS>(string[] args) where TS : class, new()
        {
            var obj = new TS();
            Parse(args, obj);
            return obj;
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
        /// <param name="obj">Parsed arguments will be store here.</param>
        public static void Parse<TS>(string[] args, TS obj) where TS : class
        {
            var parser = new CliParser<TS>(obj);
            parser.Parse(args);
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
        /// Configuration of the parser.
        /// </summary>
        public IParserConfig Config { get; private set; }

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

        private ParserOptions Options { get; set; }

        private IHelpGenerator HelpGenerator { get; set; }

        private IEnumerable<ITerminatingTrigger> Triggers { get; set; } 

        private TConf Object { get; set; }

        #endregion

        #region ctors

        /// <summary>
        /// Create a new parser with the default usage generator.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// The object used to store parsed argument values
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentIntegrityException">
        /// Some argument is invalid.
        /// </exception>
        /// <exception cref="AggregateException">
        /// Contains multiple <see cref="ArgumentIntegrityException"/>s
        /// found while checking <see cref="ArgumentAttribute"/>
        /// integrity.
        /// </exception>
        /// <param name="obj">Store parsed values in this object.</param>
        public CliParser(TConf obj)
            : this(obj, ParserOptions.None, new AutomaticHelpGenerator<TConf>())
        {
        }

        /// <summary>
        /// Create a new parser with a set of options and the
        /// default usage generator.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// The object used to store parsed argument values
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentIntegrityException">
        /// Some argument is invalid.
        /// </exception>
        /// <exception cref="AggregateException">
        /// Contains multiple <see cref="ArgumentIntegrityException"/>s
        /// found while checking <see cref="ArgumentAttribute"/>
        /// integrity.
        /// </exception>
        /// <param name="obj">Store parsed values in this object.</param>
        /// <param name="options">Extra options for the parser.</param>
        public CliParser(TConf obj, ParserOptions options)
            : this(obj, options, new AutomaticHelpGenerator<TConf>())
        {
        }

        /// <summary>
        /// Create a new parser with a custom usage generator.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// The object used to store parsed argument values
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentIntegrityException">
        /// Some argument is invalid.
        /// </exception>
        /// <exception cref="AggregateException">
        /// Contains multiple <see cref="ArgumentIntegrityException"/>s
        /// found while checking <see cref="ArgumentAttribute"/>
        /// integrity.
        /// </exception>
        /// <param name="obj">Store parsed values in this object.</param>
        /// <param name="usageGenerator">
        /// Generates help documentation for this parser.
        /// </param>
        public CliParser(TConf obj, IHelpGenerator usageGenerator)
            : this(obj, ParserOptions.None, usageGenerator)
        {
        }

        /// <summary>
        /// Create a new parser with a set of options and a custom
        /// usage generator.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// The object used to store parsed argument values
        /// is null.
        /// </exception>
        /// <exception cref="ArgumentIntegrityException">
        /// Some argument is invalid.
        /// </exception>
        /// <exception cref="AggregateException">
        /// Contains multiple <see cref="ArgumentIntegrityException"/>s
        /// found while checking <see cref="ArgumentAttribute"/>
        /// integrity.
        /// </exception>
        /// <param name="obj">Store parsed values in this object.</param>
        /// <param name="options">Extra options for the parser.</param>
        /// <param name="usageGenerator">
        /// Generates help documentation for this parser.
        /// </param>
        public CliParser(TConf obj, ParserOptions options, IHelpGenerator usageGenerator)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            Object = obj;
            Options = options;
            HelpGenerator = usageGenerator;

            Triggers = new ITerminatingTrigger[]
            {
                usageGenerator,
                new ExecutingAssemblyVersion()
            };
            var checker = new IntegrityChecker();
            checker.EnsureAttributeIntegrity<TConf>();
            checker.EnsureVerbIntegrity<TConf>();
            checker.EnsureTriggerIntegrity(Triggers);

            Config = new AttributeParserConfig<TConf>(Options, Triggers);
        }

        /// <summary>
        /// Generates a new parser from the fluent builder.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="obj"></param>
        /// <param name="options"></param>
        internal CliParser(ParserConfig<TConf> config, TConf obj, ParserOptions options)
        {
            Object = obj;
            Config = config;
            Options = options;
            // TODO help
        }

        #endregion

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
        /// <returns>The object parsed from the argument list.</returns>
        public void StrictParse(string[] args)
        {
            try
            {
                Parse(args);
                return;
            }
            catch (ParseException e)
            {
                Console.Error.WriteLine(HelpGenerator.GetUsage(Config));
                Console.Error.WriteLine(e.Message);
            }
            catch (ArgumentIntegrityException e)
            {
                Console.Error.WriteLine(HelpGenerator.GetUsage(Config));
                Console.Error.WriteLine(e.Message);
            }
            catch (AggregateException ex)
            {
                Console.Error.WriteLine(HelpGenerator.GetUsage(Config));
                ex.Handle(e =>
                {
                    Console.Error.WriteLine(e.Message);
                    return true;
                });
            }
            catch (ParserExit e)
            {
                Environment.Exit(e.ExitCode);
            }
            Environment.Exit(ErrorExitCode);
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Make an attempt to parse the arguments and return true if
        /// parsing was successful. If it returns false, the destination
        /// object may be left in an incomplete state.
        /// </summary>
        /// <param name="args">Argument list to parse.</param>
        /// <returns>True if parsing succeeded.</returns>
        public bool TryParse(string[] args)
        {
            try
            {
                Parse(args);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
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
        public void Parse(string[] args)
        {
            Config.ArgumentPrefix = ArgumentPrefix;
            new ParsingContext<TConf>(Object, Config).Parse(args);
        }

        #endregion

        
    }
}
