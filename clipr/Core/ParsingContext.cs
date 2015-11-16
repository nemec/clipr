using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using clipr.Arguments;
using clipr.Triggers;
using clipr.Utils;

namespace clipr.Core
{
    internal interface IParsingContext
    {
        void Parse(string[] args);
    }

    internal static class ParsingContextFactory
    {
        public static IParsingContext Create(object options, Type optionType, object parserConfig)
        {
            var contextType = typeof(ParsingContext<>)
                        .MakeGenericType(new[] { optionType });
            return (IParsingContext)Activator.CreateInstance(
                contextType,
                new[] { options, parserConfig },
                null);
        }
    }

    internal class ParsingContext<T> : IParsingContext where T : class
    {
        private IParserConfig Config { get; set; }

        private T Object { get; set; }

        private readonly HashSet<string> _parsedMutuallyExclusiveGroups;

        private readonly HashSet<string> _parsedNamedArguments;

        public ParsingContext(T obj, IParserConfig config)
        {
            Object = obj;
            Config = config;

            _parsedMutuallyExclusiveGroups = new HashSet<string>();
            _parsedNamedArguments = new HashSet<string>();
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
            var positionalDelimiter = "" + Config.ArgumentPrefix + Config.ArgumentPrefix;
            var values = new Stack<string>(args.Reverse());
            var positionalArgumentStore = new List<string>();
            var positionalDelimiterFound = false;

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
                    positionalDelimiterFound = true;
                    positionalArgumentStore.AddRange(values);
                    break;
                }

                if (arg[0] == Config.ArgumentPrefix)
                {
                    if (arg.Length == 1)  // myprog.exe -
                    {
                        throw new ParseException(arg, String.Format(
                            @"Cannot use argument prefix ""{0}"" as " +
                            @"argument unless forced into positional " +
                            @"mode using ""{1}"".",
                            Config.ArgumentPrefix, positionalDelimiter));
                    }
                    if (arg[1] == Config.ArgumentPrefix)  // myprog.exe --arg
                    {
                        var partition = arg.Substring(2).Split(
                            new []{Config.LongOptionSeparator}, 2);
                        if (partition.Length > 1)
                        {
                            values.Push(partition[1]);
                        }
                        ParseOptionalArgument(partition[0], Config.LongNameArguments, values, positionalDelimiterFound);
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
                            ParseOptionalArgument(shortArg, Config.ShortNameArguments, values, positionalDelimiterFound);

                            // No arguments were used... the arg
                            // we just pushed must be a group of
                            // short args.
                            if (values.Count == stackSize)
                            {
                                foreach (var shortName in values.Pop())
                                {
                                    // Arguments in a group cannot consume
                                    // values from the argStack
                                    ParseOptionalArgument(shortName, Config.ShortNameArguments, values, positionalDelimiterFound);
                                }
                            }
                        }
                        else  // Just a single argument
                        {
                            ParseOptionalArgument(shortArg, Config.ShortNameArguments, values, positionalDelimiterFound);
                        }
                    }
                    continue;
                }

                // Only first positional argument is eligible to be a verb
                if (!positionalArgumentStore.Any() && 
                    Config.Verbs.ContainsKey(arg))
                {
                    var verbConfig = Config.Verbs[arg];
                    // TODO ensure verb has parameterless constructor.
                    var verbObj = Activator.CreateInstance(verbConfig.Store.Type);
                    var context = ParsingContextFactory.Create(
                        verbObj, verbConfig.Store.Type, verbConfig);
                    context.Parse(values.ToArray());
                    verbConfig.Store.SetValue(Object, verbObj);
                    break;
                }

                positionalArgumentStore.Add(arg);
            }

            positionalArgumentStore.Reverse();
            var positionalArgStack = new Stack<string>(positionalArgumentStore);
            ParsePositionalArguments(positionalArgStack, positionalDelimiterFound);

            if (positionalArgStack.Count > 0)
            {
                throw new ParseException(null, String.Format(
                    "Extra positional arguments found: {0}",
                    String.Join(" ", positionalArgStack.ToArray())));
            }

            ParsingCleanup();
        }

        #region Private Parsing Methods

        private void ParseOptionalArgument<TS>(TS name, Dictionary<TS, IShortNameArgument> argDict, Stack<string> iter, bool positionalDelimiterFound)
        {
            var newDict = argDict.ToDictionary(
                k => k.Key,
                v => v.Value as INamedArgumentBase,
                argDict.Comparer);
            ParseOptionalArgument(name, newDict, iter, positionalDelimiterFound);
        }

        private void ParseOptionalArgument<TS>(TS name, Dictionary<TS, ILongNameArgument> argDict, Stack<string> iter, bool positionalDelimiterFound)
        {
            var newDict = argDict.ToDictionary(
                k => k.Key,
                v => v.Value as INamedArgumentBase,
                argDict.Comparer);
            ParseOptionalArgument(name, newDict, iter, positionalDelimiterFound);
        }

        private void ParseOptionalArgument<TS>(TS name, Dictionary<TS, INamedArgumentBase> argDict, Stack<string> iter, bool positionalDelimiterFound)
        {
            var arg = GetArgument(name, argDict, Config.Options);

            if (arg is ITerminatingTrigger)
            {
                (arg as ITerminatingTrigger).OnParse(Config);
                throw new ParserExit();
            }

            _parsedNamedArguments.Add(arg.Name);

            if (arg.MutuallyExclusiveGroups != null)
            {
                foreach (var group in arg.MutuallyExclusiveGroups)
                {
                    if (!_parsedMutuallyExclusiveGroups.Add(group))
                    {
                        throw new ParseException(name.ToString(), String.Format(
                            @"Mutually exclusive group ""{0}"" violated.",
                            group));
                    }
                }
            }

            if (iter == null && arg.Action.ConsumesArgumentValues())
            {
                throw new ParseException(name.ToString(),
                    "Arguments that consume values cannot be grouped.");
            }
            var prefix = Config.ArgumentPrefix.ToString(CultureInfo.CurrentCulture);
            if (name is char)
                ParseArgument(prefix + name, arg, iter, positionalDelimiterFound);
            else
                ParseArgument(prefix + prefix + name, arg, iter, positionalDelimiterFound);
        }

        private static INamedArgumentBase GetArgument<TS>(
            TS name,
            Dictionary<TS, INamedArgumentBase> argDict,
            ParserOptions options)
        {
            if (options.HasFlag(ParserOptions.NamedPartialMatch))
            {
                var nameSearch = name.ToString();
                var nameLen = nameSearch.Length;
                Func<string, string> trim = s => 
                    s.Length < nameLen ? s : s.Substring(0, nameLen);
                INamedArgumentBase foundKey = null;
                var cmp = options.HasFlag(ParserOptions.CaseInsensitive)
                    ? StringComparer.CurrentCultureIgnoreCase
                    : StringComparer.CurrentCulture;
                foreach (var key in argDict.Keys)
                {
                    if (!cmp.Equals(nameSearch, trim(key.ToString())))
                    {
                        continue;
                    }
                    if (foundKey != null)
                    {
                        throw new ParseException(name.ToString(),
                            "");
                    }
                    foundKey = argDict[key];
                }
                if (foundKey == null)
                {
                    throw new ParseException(name.ToString(), String.Format(
                        "Unknown argument name '{0}'.", name));
                }
                return foundKey;
            }
            INamedArgumentBase arg;
            if (!argDict.TryGetValue(name, out arg))
            {
                throw new ParseException(name.ToString(), String.Format(
                    "Unknown argument name '{0}'.", name));
            }
            return arg;
        }

        private void ParsePositionalArguments(Stack<string> args, bool positionalDelimiterFound)
        {
            foreach (var arg in Config.PositionalArguments)
            {
                ParseArgument(arg.Name.ToLowerInvariant(), arg, args, positionalDelimiterFound);
            }
        }

        /// <summary>
        /// Common parsing code, regardless of argument type.
        /// </summary>
        /// <param name="attrName">
        /// Name of the argument (whether short, long, or positional).
        /// e.g. "-v", "--verbose", "VALUE"
        /// </param>
        /// <param name="arg">Property associated with the argument.</param>
        /// <param name="remainingArgs">Remaining unparsed arguments.</param>
        /// <param name="positionalDelimiterFound">True if the positional delimiter, e.g. -- has been found.</param>
        private void ParseArgument(string attrName, IArgument arg, Stack<string> remainingArgs, bool positionalDelimiterFound)
        {
            var store = arg.Store;
            switch (arg.Action)
            {
                case ParseAction.Store:
                    {
                        if (!arg.ConsumesMultipleArgs)
                        {
                            if (remainingArgs.Count == 0)
                            {
                                throw new ParseException(attrName, String.Format(
                                    @"Argument ""{0}"" requires a value but " +
                                    "none was provided.", attrName));
                            }
                            var stringValue = remainingArgs.Pop();
                            try
                            {
                                object converted;
                                if (TryConvertFrom(store, stringValue, out converted))
                                {
                                    store.SetValue(Object, converted);
                                }
                                else
                                {
                                    throw new ParseException(attrName, String.Format(
                                        @"Argument {0} value ""{1}"" cannot be converted to the " +
                                        "required type {2}.",
                                        attrName, stringValue, store.Type));
                                }
                            }
                            catch (Exception e)
                            {
                                throw new ParseException(attrName, String.Format(
                                    @"Argument {0} value ""{1}"" cannot be converted to the " +
                                    "required type {2}.",
                                     attrName, stringValue, store.Type), e);
                            }
                        }
                        else
                        {
                            var existing = (IEnumerable)store.GetValue(Object);
                            var backingList = CreateGenericList(store, existing);

                            ParseVarargs(attrName, backingList, arg, remainingArgs, positionalDelimiterFound);
                            store.SetValue(Object, backingList);
                        }
                        break;
                    }
                case ParseAction.StoreConst:
                    store.SetValue(Object, arg.Const);
                    break;

                case ParseAction.StoreTrue:
                    store.SetValue(Object, true);
                    break;

                case ParseAction.StoreFalse:
                    store.SetValue(Object, false);
                    break;

                case ParseAction.Append:
                    {
                        var existing = (IEnumerable)store.GetValue(Object);
                        var backingList = CreateGenericList(store, existing);

                        if (!arg.ConsumesMultipleArgs)
                        {
                            if (remainingArgs.Count == 0)
                            {
                                throw new ParseException(attrName);
                            }
                            var stringValue = remainingArgs.Pop();
                            try
                            {
                                object converted;
                                if (TryConvertFromGeneric(store, stringValue, out converted))
                                {
                                    backingList.Add(converted);
                                }
                                else
                                {
                                    throw new ParseException(attrName, String.Format(
                                        @"Argument {0} value ""{1}"" cannot be converted to the " +
                                        "required type {2}.",
                                        attrName, stringValue, arg.Store.Type));
                                }
                            }
                            catch (Exception e)
                            {
                                throw new ParseException(attrName, String.Format(
                                    @"Argument {0} value ""{1}"" cannot be converted to the " +
                                    "required type {2}.",
                                    attrName, stringValue, store.Type), e);
                            }
                        }
                        else
                        {
                            ParseVarargs(attrName, backingList, arg, remainingArgs, positionalDelimiterFound);

                        }
                        store.SetValue(Object, backingList);
                        break;
                    }
                case ParseAction.AppendConst:
                    {
                        var existing = (IEnumerable) store.GetValue(Object);
                        var backingList = CreateGenericList(store, existing);
                        backingList.Add(arg.Const);
                        store.SetValue(Object, backingList);
                        break;
                    }
                case ParseAction.Count:
                    var cnt = (int)store.GetValue(Object);
                    store.SetValue(Object, cnt + 1);
                    break;
            }
        }

        private void ParseVarargs(string attrName, IList list,
            IArgument arg, Stack<string> args, bool positionalDelimiterFound)
        {
            var argsProcessed = 0;

            #region Set minimum and maximum argument count.

            uint minArgs = 0;
            uint maxArgs = 0;
            switch (arg.Constraint)
            {
                case NumArgsConstraint.Exactly:
                    minArgs = arg.NumArgs;
                    maxArgs = arg.NumArgs;
                    break;
                case NumArgsConstraint.AtLeast:
                    minArgs = arg.NumArgs;
                    maxArgs = uint.MaxValue;
                    break;
                case NumArgsConstraint.AtMost:
                    minArgs = 0;
                    maxArgs = arg.NumArgs;
                    break;
            }

            #endregion

            while (args.Count > 0 && argsProcessed < maxArgs)
            {
                var stringValue = args.Pop();

                // Quit if we start a new argument here.
                // But not if the next character is a digit
                if (!positionalDelimiterFound &&
                    stringValue != null &&
                    stringValue.StartsWith(Config.ArgumentPrefix
                        .ToString(CultureInfo.InvariantCulture)) &&
                    (stringValue.Length > 1 && !Char.IsDigit(stringValue[1])))
                {
                    args.Push(stringValue);
                    break;
                }
                try
                {
                    object converted;
                    if (TryConvertFromGeneric(arg.Store, stringValue, out converted))
                    {
                        list.Add(converted);
                    }
                    else
                    {
                        throw new ParseException(attrName, String.Format(
                            @"Argument {0} value ""{1}"" cannot be converted to the " +
                            "required type {2}.",
                            attrName, stringValue, arg.Store.Type));
                    }
                }
                catch (Exception e)
                {
                    throw new ParseException(attrName, String.Format(
                        @"Argument {0} value ""{1}"" cannot be converted to the " +
                        "required type {2}.",
                        attrName, stringValue, arg.Store.Type), e);
                }
                argsProcessed++;
            }

            if (argsProcessed < minArgs)
            {
                throw new ParseException(attrName, String.Format(
                    "Parameter {0} requires {1} {2} value(s).",
                    arg.MetaVar ?? arg.Name,
                    arg.Constraint == NumArgsConstraint.Exactly ?
                        "exactly" : "at least",
                    minArgs));
            }
        }

        #endregion

        private void ParsingCleanup()
        {
            var missingRequiredMutexGroups = Config
                .RequiredMutuallyExclusiveArguments
                .Except(_parsedMutuallyExclusiveGroups).ToArray();
            if (missingRequiredMutexGroups.Any())
            {
                throw new ParseException(null, String.Format(
                    @"Required mutually exclusive group(s) ""{0}"" were " +
                    "not provided.",
                    String.Join(", ", missingRequiredMutexGroups)));
            }

            var missingRequiredNamedArguments = Config
                .RequiredNamedArguments
                .Except(_parsedNamedArguments).ToArray();
            if (missingRequiredNamedArguments.Any())
            {
                throw new ParseException(null, String.Format(
                    @"Required named argument(s) ""{0}"" were " +
                    "not provided.",
                    String.Join(", ", missingRequiredNamedArguments)));
            }

            foreach (var method in Config.PostParseMethods)
            {
                method.Invoke(Object, null);
            }
        }

        private static bool TryConvertFrom(IValueStoreDefinition store, string value, out object obj)
        {
            var customConverter = store.Converters != null
                ? store.Converters
                    .FirstOrDefault(c => c.CanConvertFrom(typeof(string)))
                : null;
            if (customConverter != null)
            {
                obj = customConverter.ConvertFromInvariantString(value);
                return true;
            }

            var converter = TypeDescriptor.GetConverter(store.Type);
            if (converter.CanConvertFrom(typeof(string)))
            {
                obj = converter.ConvertFromInvariantString(value);
                return true;
            }
            obj = null;
            return false;
        }

        private static bool TryConvertFromGeneric(IValueStoreDefinition store, string value, out object obj)
        {
            var tempStore = new DummyValueStore
            {
                Name = store.Name,
                Converters = store.Converters,
                Type = store.Type.GetGenericArguments().First()
            };

            return TryConvertFrom(tempStore, value, out obj);
        }

        private class DummyValueStore : IValueStoreDefinition
        {
            public string Name { get; set; }

            public TypeConverter[] Converters { get; set; }

            public void SetValue(object source, object value)
            {
                throw new NotImplementedException();
            }

            public object GetValue(object source)
            {
                throw new NotImplementedException();
            }

            public TAttribute GetCustomAttribute<TAttribute>() where TAttribute : Attribute
            {
                return null;
            }

            public IEnumerable<TAttribute> GetCustomAttributes<TAttribute>() where TAttribute : Attribute
            {
                return Enumerable.Empty<TAttribute>();
            }

            public Type Type { get; set; }
        }

        private static IList CreateGenericList(IValueStoreDefinition store, IEnumerable initial)
        {
            var type = store.Type.GetGenericArguments();
            if (!type.Any())
            {
                return null;
            }
            var list = (IList) Activator.CreateInstance(typeof (List<>).MakeGenericType(type));
            if (initial != null)
            {
                foreach (var elem in initial)
                {
                    list.Add(elem);
                }
            }
            return list;
        }
    }
}
