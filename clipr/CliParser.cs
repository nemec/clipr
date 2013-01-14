using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using clipr.Usage;

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
        public static TS ParseStrict<TS>(string[] args) where TS : class, new()
        {
            var obj = new TS();
            ParseStrict(obj, args);
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
        public static void ParseStrict<TS>(TS obj, string[] args) where TS : class
        {
            CliParser<TS> parser = null;
            try
            {
                parser = new CliParser<TS>(obj);
            }
            catch (ArgumentIntegrityException e)
            {
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine(
                    new AutomaticHelpGenerator<TS>().GetUsage());
                Environment.Exit(2);
            }
            catch (AggregateException ex)
            {
                Console.Error.WriteLine(
                    new AutomaticHelpGenerator<TS>().GetUsage());
                ex.Handle(e =>
                {
                    Console.Error.WriteLine(e.Message);
                    return true;
                });
                Environment.Exit(2);
            }
            parser.ParseStrict(args);
        }

        /// <summary>
        /// Parse the given argument list and return a new object
        /// containing the converted arguments.
        /// </summary>
        /// <param name="args">Argument list to parse.</param>
        /// <returns>
        /// A new object containing values parsed from the argument list.
        /// </returns>
        public static TS Parse<TS>(string[] args) where TS : class, new()
        {
            var obj = new TS();
            Parse(obj, args);
            return obj;
        }

        /// <summary>
        /// Parse the given argument list.
        /// </summary>
        /// <param name="obj">Parsed arguments will be store here.</param>
        /// <param name="args">Argument list to parse.</param>
        public static void Parse<TS>(TS obj, string[] args) where TS : class
        {
            var parser = new CliParser<TS>(obj);
            parser.Parse(args);
        }
    }

    /// <summary>
    /// Parses a set of argument strings into an object.
    /// </summary>
    /// <typeparam name="T">Object type being deserialized.</typeparam>
    public class CliParser<T> where T : class
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
        private char _argumentPrefix;

        #endregion

        #region Private Properties

        internal const int ErrorExitCode = 2;

        private ParserOptions Options { get; set; }

        private readonly char[] _longOptionSeparator = new[] { '=' };

        private readonly Func<char, bool> _isAllowedShortName =
            c => Char.IsLetter(c);

        private const string IsAllowedShortNameExplanation =
            "Short arguments must be letters.";

        private readonly Func<string, bool> _isAllowedLongName =
            s => Regex.IsMatch(s, @"^[a-zA-Z][a-zA-Z0-9\-]*[a-zA-Z0-9]$");

        private const string IsAllowedLongNameExplanation =
            "Long arguments must begin with a letter, contain a letter, " +
            "digit, or hyphen, and end with a letter or a digit.";

        private Dictionary<char, PropertyInfo> ShortNameArguments { get; set; }

        private Dictionary<string, PropertyInfo> LongNameArguments { get; set; }

        private List<PropertyInfo> PositionalArguments { get; set; }

        private HashSet<string> ParsedMutuallyExclusiveGroups { get; set; }

        private IHelpGenerator UsageGenerator { get; set; }

        private T Object { get; set; }

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
        public CliParser(T obj)
            : this(obj, ParserOptions.None, new AutomaticHelpGenerator<T>())
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
        public CliParser(T obj, ParserOptions options)
            : this(obj, options, new AutomaticHelpGenerator<T>())
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
        public CliParser(T obj, IHelpGenerator usageGenerator)
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
        public CliParser(T obj, ParserOptions options, IHelpGenerator usageGenerator)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            Object = obj;
            Options = options;
            UsageGenerator = usageGenerator;

            ArgumentPrefix = '-';

            InitializeArgumentCache();
            ArgumentIntegrityChecker.EnsureAttributeIntegrity<T>();
        }

        #endregion

        #region Initialization

        private void InitializeArgumentCache()
        {
            var type = typeof(T);

            PositionalArguments = type.GetProperties()
                .Where(p => p.GetCustomAttribute<PositionalArgumentAttribute>() != null)
                .OrderBy(p => p.GetCustomAttribute<PositionalArgumentAttribute>().Index)
                .ToList();

            if (Options.HasFlag(ParserOptions.CaseInsensitive))
            {
                ShortNameArguments = new Dictionary<char, PropertyInfo>(new CaseInsensitiveCharComparer());
                LongNameArguments = new Dictionary<string, PropertyInfo>(StringComparer.InvariantCultureIgnoreCase);
            }
            else
            {
                ShortNameArguments = new Dictionary<char, PropertyInfo>();
                LongNameArguments = new Dictionary<string, PropertyInfo>();
            }

            #region Usage Arguments

            if (UsageGenerator != null)
            {
                if (UsageGenerator.ShortName.HasValue)
                {
                    if (!_isAllowedShortName(UsageGenerator.ShortName.Value))
                    {
                        throw new ArgumentIntegrityException(String.Format(
                            "Usage argument {0} is not a valid short name. {1}",
                            UsageGenerator.ShortName, IsAllowedShortNameExplanation));
                    }
                    ShortNameArguments.Add(UsageGenerator.ShortName.Value, null);
                }
                if (UsageGenerator.LongName != null)
                {
                    if (!_isAllowedLongName(UsageGenerator.LongName))
                    {
                        throw new ArgumentIntegrityException(String.Format(
                            "Usage argument {0} is not a valid long name. {1}",
                            UsageGenerator.LongName, IsAllowedLongNameExplanation));
                    }
                    LongNameArguments.Add(UsageGenerator.LongName, null);
                }
            }

            #endregion

            #region Version Arguments

            var version = UsageGenerator != null ? UsageGenerator.Version : null;
            if (version != null)
            {
                if (version.ShortName.HasValue)
                {
                    if (!_isAllowedShortName(version.ShortName.Value))
                    {
                        throw new ArgumentIntegrityException(String.Format(
                            "Version argument {0} is not a valid short name. {1}",
                            version.ShortName, IsAllowedShortNameExplanation));
                    }
                    ShortNameArguments.Add(version.ShortName.Value, null);
                }
                if (version.LongName != null)
                {
                    if (!_isAllowedLongName(version.LongName))
                    {
                        throw new ArgumentIntegrityException(String.Format(
                            "Version argument {0} is not a valid long name. {1}",
                            version.LongName, IsAllowedLongNameExplanation));
                    }
                    LongNameArguments.Add(version.LongName, null);
                }
            }

            #endregion

            foreach (var prop in type.GetProperties()
                .Where(p => p.GetCustomAttribute<NamedArgumentAttribute>() != null))
            {
                var arg = prop.GetCustomAttribute<NamedArgumentAttribute>();

                #region Cache valid short arguments

                if (arg.ShortName != default(char))
                {
                    if (!_isAllowedShortName(arg.ShortName))
                    {
                        throw new ArgumentIntegrityException(String.Format(
                            "Short name {0} is not allowed. {1}",
                            arg.ShortName, IsAllowedShortNameExplanation));
                    }
                    try
                    {
                        ShortNameArguments.Add(arg.ShortName, prop);
                    }
                    catch (ArgumentException)
                    {
                        throw new DuplicateArgumentException(arg.ShortName.ToString());
                    }
                }

                #endregion

                #region Cache valid long arguments

                if (arg.LongName != null)
                {
                    if (arg.LongName.Length < 2)
                    {
                        throw new ArgumentIntegrityException(String.Format(
                            "Long argument on {0} must have at least two characters.",
                            prop.Name));
                    }
                    if (!_isAllowedLongName(arg.LongName))
                    {
                        throw new ArgumentIntegrityException(String.Format(
                            "Long name {0} is not allowed. {1}",
                            arg.LongName, IsAllowedLongNameExplanation));
                    }
                    try
                    {
                        LongNameArguments.Add(arg.LongName, prop);
                    }
                    catch (ArgumentException)
                    {
                        throw new DuplicateArgumentException(arg.LongName);
                    }
                }

                #endregion
            }
        }

        #endregion

        #region Print Usage and Version

        /// <summary>
        /// Print help information to <see cref="Console.Error"/>.
        /// </summary>
        public void PrintHelp()
        {
            PrintHelp(Console.Error);
        }

        /// <summary>
        /// Print help information to a <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="stream">Destination for usage text.</param>
        public void PrintHelp(TextWriter stream)
        {
            if (UsageGenerator != null)
            {
                stream.WriteLine(UsageGenerator.GetHelp());
            }
        }

        /// <summary>
        /// Print the version number to the error console.
        /// </summary>
        public void PrintVersion()
        {
            PrintVersion(Console.Error);
        }

        /// <summary>
        /// Print the version number to the given TextWriter.
        /// </summary>
        /// <param name="stream"></param>
        public void PrintVersion(TextWriter stream)
        {
            if (UsageGenerator != null && UsageGenerator.Version != null)
            {
                stream.WriteLine(UsageGenerator.Version.GetVersion());
            }
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
        public void ParseStrict(string[] args)
        {
            try
            {
                Parse(args);
                return;
            }
            catch (ParseException e)
            {
                Console.Error.WriteLine(UsageGenerator.GetUsage());
                Console.Error.WriteLine(e.Message);
            }
            catch (ArgumentIntegrityException e)
            {
                Console.Error.WriteLine(UsageGenerator.GetUsage());
                Console.Error.WriteLine(e.Message);
            }
            catch (AggregateException ex)
            {
                Console.Error.WriteLine(UsageGenerator.GetUsage());
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
        /// Parse the given argument list.
        /// </summary>
        /// <exception cref="ParseException"></exception>
        /// <exception cref="ParserExit">
        /// Either the help or version information were triggered so
        /// parsing was aborted.
        /// </exception>
        /// <param name="args">Argument list to parse.</param>
        public void Parse(string[] args)
        {
            ParsedMutuallyExclusiveGroups = new HashSet<string>();
            var positionalDelimiter = "" + ArgumentPrefix + ArgumentPrefix;

            var values = new Stack<string>(args.Reverse());

            var positionalArgumentStore = new List<string>();

            while (values.Count > 0)
            {
                var arg = values.Pop();

                // Empty arg
                if (string.IsNullOrEmpty(arg))
                {
                    continue;
                }

                arg = arg.Trim();

                // Rest of arguments are positional
                if (arg == positionalDelimiter)
                {
                    positionalArgumentStore.AddRange(values);
                    break;
                }

                if (arg[0] == ArgumentPrefix)
                {
                    if (arg.Length == 1)  // myprog.exe -
                    {
                        throw new ParseException(arg, String.Format(
                            @"Cannot use argument prefix ""{0}"" as " +
                            @"argument unless forced into positional " +
                            @"mode using ""{1}"".",
                            ArgumentPrefix, positionalDelimiter));
                    }
                    if (arg[1] == ArgumentPrefix)  // myprog.exe --arg
                    {
                        var partition = arg.Substring(2).Split(_longOptionSeparator, 2);
                        if (partition.Length > 1)
                        {
                            values.Push(partition[1]);
                        }
                        ParseOptionalLongArgument(partition[0], values);
                    }
                    else  // myprog.exe -a
                    {
                        var shortArg = arg[1];
                        
                        if (Char.IsDigit(shortArg))  // myprog.exe -1
                        {
                            // We're parsing a negative number...
                            // Nothing to see here.
                            positionalArgumentStore.Add(arg);
                            continue;
                        }

                        if (arg.Length > 2)
                        {
                            // Pretend the rest of the argument is a new value
                            values.Push(arg.Substring(2));

                            var stackSize = values.Count;
                            ParseOptionalShortArgument(shortArg, values);

                            // No arguments were used... the arg
                            // we just pushed must be a group of
                            // short args.
                            if (values.Count == stackSize)
                            {
                                foreach (var shortName in values.Pop())
                                {
                                    // Arguments in a group cannot consume
                                    // values from the argStack
                                    ParseOptionalShortArgument(shortName, null);
                                }
                            }
                        }
                        else  // Just a single argument
                        {
                            ParseOptionalShortArgument(shortArg, values);
                        }
                    }
                    continue;
                }

                positionalArgumentStore.Add(arg);
            }

            positionalArgumentStore.Reverse();
            var positionalArgStack = new Stack<string>(positionalArgumentStore);
            ParsePositionalArguments(positionalArgStack);

            if (positionalArgStack.Count > 0)
            {
                throw new ParseException(null, String.Format(
                    "Extra positional arguments found: {0}",
                    String.Join(" ", positionalArgStack)));
            }

            var missingRequiredMutexGroups = new HashSet<string>(
                typeof(T).GetProperties().SelectMany(p =>
                    p.GetCustomAttributes<MutuallyExclusiveGroupAttribute>()
                    .Where(a => a.Required)
                    .Select(a => a.Name)))
                .Except(ParsedMutuallyExclusiveGroups);
            if (missingRequiredMutexGroups.Any())
            {
                throw new ParseException(null, String.Format(
                    @"Required mutually exclusive group(s) ""{0}"" were " +
                    "not provided.",
                    String.Join(", ", missingRequiredMutexGroups)));
            }

            foreach (var method in typeof(T).GetMethods()
                .Where(p => p.GetCustomAttribute<PostParseAttribute>() != null))
            {
                method.Invoke(Object, null);
            }
        }

        #endregion

        #region Private Parsing Methods

        private void ParseOptionalShortArgument(char name, Stack<string> iter)
        {
            if (UsageGenerator != null && name == UsageGenerator.ShortName)
            {
                PrintHelp();
                throw new ParserExit();
            }

            PropertyInfo prop;
            if (!ShortNameArguments.TryGetValue(name, out prop))
            {
                throw new ParseException(name, String.Format(
                    "Unknown argument name '{0}'.", name));
            }
            var attr = prop.GetCustomAttribute<NamedArgumentAttribute>();
            var mutexGroup = prop.GetCustomAttributes<MutuallyExclusiveGroupAttribute>();
            foreach (var groupAttribute in mutexGroup)
            {
                if (!ParsedMutuallyExclusiveGroups.Add(groupAttribute.Name))
                {
                    throw new ParseException(name, String.Format(
                        @"Mutually exclusive group ""{0}"" violated.",
                        groupAttribute.Name));
                }
            }

            if (iter == null && attr.Action.ConsumesArgumentValues())
            {
                throw new ParseException(name,
                    "Arguments that consume values cannot be grouped.");
            }
            ParseArgument(attr.ShortName.ToString(), prop, iter);
        }

        private void ParseOptionalLongArgument(string name, Stack<string> args)
        {
            if (UsageGenerator != null && UsageGenerator.LongName == name)
            {
                PrintHelp();
                throw new ParserExit();
            }
            if (UsageGenerator != null && UsageGenerator.Version != null &&
                     UsageGenerator.Version.LongName == name)
            {
                PrintVersion();
                throw new ParserExit();
            }

            PropertyInfo prop;
            if (!LongNameArguments.TryGetValue(name, out prop))
            {
                throw new ParseException(name, String.Format(
                    "Unknown argument name '{0}'.", name));
            }

            var mutexGroup = prop.GetCustomAttributes<MutuallyExclusiveGroupAttribute>();
            foreach (var groupAttribute in mutexGroup)
            {
                if (!ParsedMutuallyExclusiveGroups.Add(groupAttribute.Name))
                {
                    throw new ParseException(name, String.Format(
                        @"Mutually exclusive group ""{0}"" violated.",
                        groupAttribute.Name));
                }
            }

            var attr = prop.GetCustomAttribute<NamedArgumentAttribute>();
            ParseArgument(attr.LongName, prop, args);
        }

        private void ParsePositionalArguments(Stack<string> args)
        {
            foreach (var prop in PositionalArguments)
            {
                ParseArgument(prop.Name.ToLowerInvariant(), prop, args);
            }
        }

        /// <summary>
        /// Common parsing code, regardless of argument type.
        /// </summary>
        /// <param name="attrName">
        /// Name of the argument (whether short, long, or positional).
        /// </param>
        /// <param name="prop">Property associated with the argument.</param>
        /// <param name="args">List of remaining unparsed arguments.</param>
        private void ParseArgument(string attrName, PropertyInfo prop, Stack<string> args)
        {
            var attr = prop.GetCustomAttribute<ArgumentAttribute>();
            switch (attr.Action)
            {
                case ParseAction.Store:
                    if (!attr.ConsumesMultipleArgs)
                    {
                        if (args.Count == 0)
                        {
                            throw new ParseException(attrName, String.Format(
                                @"Argument ""{0}"" requires a value but " +
                                "none was provided.", attrName));
                        }
                        var stringValue = args.Pop();
                        try
                        {
                            prop.SetValue(Object, ConvertFrom(prop, stringValue));
                        }
                        catch (Exception e)
                        {
                            throw new ParseException(attrName, String.Format(
                                @"Value ""{0}"" cannot be converted to the " +
                                "required type {1}.",
                                stringValue, prop.PropertyType), e);
                        }
                    }
                    else
                    {
                        var values = (IList) prop.GetValue(Object);
                        if (values != null)
                        {
                            values.Clear();  // Store overwrites always
                        }
                        else
                        {
                            values = (IList) Activator.CreateInstance(prop.PropertyType);
                        }
                        ParseVarargs(attrName, values, prop, args);
                        prop.SetValue(Object, values);
                    }
                    break;

                case ParseAction.StoreConst:
                    prop.SetValue(Object, attr.Const);
                    break;

                case ParseAction.StoreTrue:
                    prop.SetValue(Object, true);
                    break;

                case ParseAction.StoreFalse:
                    prop.SetValue(Object, false);
                    break;

                case ParseAction.Append:
                    var appValues = (IList)prop.GetValue(Object) ??
                        (IList)Activator.CreateInstance(prop.PropertyType);
                    if (!attr.ConsumesMultipleArgs)
                    {
                        if (args.Count == 0)
                        {
                            throw new ParseException(attrName);
                        }
                        var stringValue = args.Pop();
                        try
                        {
                            appValues.Add(ConvertFromGeneric(prop, stringValue));
                        }
                        catch (Exception e)
                        {
                            throw new ParseException(attrName, String.Format(
                                @"Value ""{0}"" cannot be converted to the " +
                                "required type {1}.",
                                stringValue, prop.PropertyType), e);
                        }
                    }
                    else
                    {
                        ParseVarargs(attrName, appValues, prop, args);
                        
                    }
                    prop.SetValue(Object, appValues);
                    break;
                case ParseAction.AppendConst:
                    var constValues = (IList)prop.GetValue(Object) ??
                        (IList)Activator.CreateInstance(prop.PropertyType);
                    constValues.Add(attr.Const);
                    prop.SetValue(Object, constValues);
                    break;

                case ParseAction.Count:
                    var cnt = (int)prop.GetValue(Object);
                    prop.SetValue(Object, cnt + 1);
                    break;
            }
        }

        private static void ParseVarargs(string attrName, IList list,
            PropertyInfo prop, Stack<string> args)
        {
            var attr = prop.GetCustomAttribute<ArgumentAttribute>();

            var argsProcessed = 0;

            #region Set minimum and maximum argument count.
            
            uint minArgs = 0;
            uint maxArgs = 0;
            switch (attr.Constraint)
            {
                case NumArgsConstraint.Exactly:
                    minArgs = attr.NumArgs;
                    maxArgs = attr.NumArgs;
                    break;
                case NumArgsConstraint.AtLeast:
                    minArgs = attr.NumArgs;
                    maxArgs = uint.MaxValue;
                    break;
                case NumArgsConstraint.AtMost:
                    minArgs = 0;
                    maxArgs = attr.NumArgs;
                    break;
            }

            #endregion

            while (args.Count > 0 && argsProcessed < maxArgs)
            {
                var stringValue = args.Pop();
                try
                {
                    list.Add(ConvertFromGeneric(prop, stringValue));
                }
                catch (Exception e)
                {
                    throw new ParseException(attrName, String.Format(
                        @"Value ""{0}"" cannot be converted to the " +
                        "required type {1}.",
                        stringValue, prop.PropertyType), e);
                }
                argsProcessed++;
            }

            if (argsProcessed < minArgs)
            {
                throw new ParseException(attrName, String.Format(
                    "Parameter {0} requires {1} {2} value(s).",
                    attr.MetaVar ?? prop.Name,
                    attr.Constraint == NumArgsConstraint.Exactly ?
                        "exactly" : "at least",
                    minArgs));
            }
        }

        #endregion

        private static object ConvertFrom(PropertyInfo prop, string value)
        {
            return TypeDescriptor.GetConverter(prop.PropertyType)
                                    .ConvertFromInvariantString(value);
        }

        private static object ConvertFromGeneric(PropertyInfo prop, string value)
        {
            return TypeDescriptor.GetConverter(
                prop.PropertyType.GenericTypeArguments.First())
                .ConvertFromInvariantString(value);
        }
    }
}
