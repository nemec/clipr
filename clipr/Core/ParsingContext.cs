using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using clipr.Arguments;
using clipr.Triggers;
using clipr.Utils;
#if NETCORE
using System.Reflection;
#endif

namespace clipr.Core
{
    internal interface IParsingContext
    {
        ParseResultWithTrigger Parse(string[] args);
    }

    internal static class ParsingContextFactory
    {
        public static IParsingContext Create(object options, Type optionType, IVerbParserConfig parserConfig)
        {
            var contextType = typeof(ParsingContext<>)
                        .GetTypeInfo()
                        .MakeGenericType(new[] { optionType });
            return (IParsingContext)Activator.CreateInstance(
                type:contextType,
                args:new[] { options, parserConfig });
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

        ParseResultWithTrigger IParsingContext.Parse(string[] args)
        {
            return Parse(args).Handle(
                obj => ParseResultWithTrigger.Success,
                trig => new ParseResultWithTrigger(trig),
                errs => new ParseResultWithTrigger(errs));
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
        public ParseResult<T> Parse(string[] args)
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
                        return new ParseResult<T>(new ParseException(arg, String.Format(
                            @"Cannot use argument prefix ""{0}"" as " +
                            @"argument unless forced into positional " +
                            @"mode using ""{1}"".",
                            Config.ArgumentPrefix, positionalDelimiter)));
                    }
                    if (arg[1] == Config.ArgumentPrefix)  // myprog.exe --arg
                    {
                        var partition = arg.Substring(2).Split(
                            new []{Config.LongOptionSeparator}, 2);
                        if (partition.Length > 1)
                        {
                            values.Push(partition[1]);
                        }
                        var longResult = ParseOptionalArgument(partition[0], Config.LongNameArguments, values, positionalDelimiterFound);
                        if (!longResult.IsSuccess)
                        {
                            return longResult.Handle(
                                () => null,
                                trig => new ParseResult<T>(trig),
                                errs => new ParseResult<T>(errs));
                        }
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
                            var shortResult = ParseOptionalArgument(shortArg, Config.ShortNameArguments, values, positionalDelimiterFound);
                            if (!shortResult.IsSuccess)
                            {
                                return shortResult.Handle(
                                    () => null,
                                    trig => new ParseResult<T>(trig),
                                    errs => new ParseResult<T>(errs));
                            }

                            // No arguments were used... the arg
                            // we just pushed must be a group of
                            // short args.
                            if (values.Count == stackSize)
                            {
                                foreach (var shortName in values.Pop())
                                {
                                    // Arguments in a group cannot consume
                                    // values from the argStack
                                    var shortCombinedResult = ParseOptionalArgument(shortName, Config.ShortNameArguments, values, positionalDelimiterFound);
                                    if (!shortCombinedResult.IsSuccess)
                                    {
                                        return shortCombinedResult.Handle(
                                            () => null,
                                            trig => new ParseResult<T>(trig),
                                            errs => new ParseResult<T>(errs));
                                    }
                                }
                            }
                        }
                        else  // Just a single argument
                        {
                            var shortResult = ParseOptionalArgument(shortArg, Config.ShortNameArguments, values, positionalDelimiterFound);
                            if (!shortResult.IsSuccess)
                            {
                                return shortResult.Handle(
                                    () => null,
                                    trig => new ParseResult<T>(trig),
                                    errs => new ParseResult<T>(errs));
                            }
                        }
                    }
                    continue;
                }

                // Only first positional argument is eligible to be a verb
                if (!positionalArgumentStore.Any() && 
                    Config.Verbs.ContainsKey(arg))
                {
                    var verbConfig = Config.Verbs[arg];
                    var verbObj = Config.VerbFactory.GetVerb(verbConfig.Store.Type);
                    var context = ParsingContextFactory.Create(
                        verbObj, verbConfig.Store.Type, verbConfig);
                    var verbResult = context.Parse(values.ToArray());
                    if (!verbResult.IsSuccess)
                    {
                        return verbResult.Handle(
                            () => null,
                            trig => new ParseResult<T>(trig),
                            errs => new ParseResult<T>(errs));
                    }
                    verbConfig.Store.SetValue(Object, verbObj);
                    break;
                }

                positionalArgumentStore.Add(arg);
            }

            positionalArgumentStore.Reverse();
            var positionalArgStack = new Stack<string>(positionalArgumentStore);
            var result = ParsePositionalArguments(positionalArgStack, positionalDelimiterFound);
            if (!result.IsSuccess)
            {
                return result.Handle(
                    () => null,  // TODO is there a better way to handle?
                    errs => new ParseResult<T>(errs));
            }

            if (positionalArgStack.Count > 0)
            {
                return new ParseResult<T>(
                    new ParseException(null, String.Format(
                        "Extra positional arguments found: {0}",
                        String.Join(" ", positionalArgStack.ToArray()))));
            }

            var cleanupResult = ParsingCleanup();
            return cleanupResult.Handle(
                () => new ParseResult<T>(Object),
                errs => new ParseResult<T>(errs));
        }

#region Private Parsing Methods

        private ParseResultWithTrigger ParseOptionalArgument<TS>(TS name, Dictionary<TS, IShortNameArgument> argDict, Stack<string> iter, bool positionalDelimiterFound)
        {
            var newDict = argDict.ToDictionary(
                k => k.Key,
                v => v.Value as INamedArgumentBase,
                argDict.Comparer);
            return ParseOptionalArgument(name, newDict, iter, positionalDelimiterFound);
        }

        private ParseResultWithTrigger ParseOptionalArgument<TS>(TS name, Dictionary<TS, ILongNameArgument> argDict, Stack<string> iter, bool positionalDelimiterFound)
        {
            var newDict = argDict.ToDictionary(
                k => k.Key,
                v => v.Value as INamedArgumentBase,
                argDict.Comparer);
            return ParseOptionalArgument(name, newDict, iter, positionalDelimiterFound);
        }

        private ParseResultWithTrigger ParseOptionalArgument<TS>(TS name, Dictionary<TS, INamedArgumentBase> argDict, Stack<string> iter, bool positionalDelimiterFound)
        {
            var argResult = GetArgument(name, argDict, Config.Options);
            return argResult.Handle(
                arg =>
                {
                    var trig = arg as ITerminatingTrigger;
                    if (trig != null)
                    {
                        trig.OnParse(Config);  // TODO keep here or call OnParse outside of the context?
                        return new ParseResultWithTrigger(trig);
                    }

                    _parsedNamedArguments.Add(arg.Name);

                    if (arg.MutuallyExclusiveGroups != null)
                    {
                        foreach (var group in arg.MutuallyExclusiveGroups)
                        {
                            if (!_parsedMutuallyExclusiveGroups.Add(group))
                            {
                                return new ParseResultWithTrigger(
                                    new ParseException(name.ToString(), String.Format(
                                        @"Mutually exclusive group ""{0}"" violated.",
                                        group)));
                            }
                        }
                    }

                    if (iter == null && arg.Action.ConsumesArgumentValues())
                    {
                        return new ParseResultWithTrigger(
                            new ParseException(name.ToString(),
                                "Arguments that consume values cannot be grouped."));
                    }
                    var prefix = Config.ArgumentPrefix
#if NET35
                .ToString(CultureInfo.CurrentCulture);
#else
                .ToString();
#endif
                    // If next item looks like an 'optional' argument, pretend like the arg list is empty
                    string next = null;
                    if (iter.Any()) next = iter.Peek();
                    if (ParamIsArgument(next)) iter = new Stack<string>();

                    if (name is char)
                    {
                        var result = ParseArgument(prefix + name, arg, iter, positionalDelimiterFound);
                        if (!result.IsSuccess)
                        {
                            return result.Handle(
                                () => null,
                                errs => new ParseResultWithTrigger(errs));
                        }
                    }
                    else
                    {
                        var result = ParseArgument(prefix + prefix + name, arg, iter, positionalDelimiterFound);
                        if (!result.IsSuccess)
                        {
                            return result.Handle(
                                () => null,
                                errs => new ParseResultWithTrigger(errs));
                        }
                    }

                    return ParseResultWithTrigger.Success;
                },
                trigger =>
                {
                    return new ParseResultWithTrigger(trigger);
                },
                errs =>
                {
                    return new ParseResultWithTrigger(errs);
                });
        }

        private bool ParamIsArgument(string param)
        {
            if (param == null || param.Length == 0 || param[0] != Config.ArgumentPrefix) return false;
            if (param.Length > 1 && Char.IsDigit(param[1])) return false;
            return true;
        }

    private static ParseResult<INamedArgumentBase> GetArgument<TS>(
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
                        return new ParseResult<INamedArgumentBase>(
                            new ParseException(name.ToString(), ""));
                    }
                    foundKey = argDict[key];
                }
                if (foundKey == null)
                {
                    return new ParseResult<INamedArgumentBase>(
                        new ParseException(name.ToString(), String.Format(
                            "Unknown argument name '{0}'.", name)));
                }
                return new ParseResult<INamedArgumentBase>(foundKey);
            }
            INamedArgumentBase arg;
            if (!argDict.TryGetValue(name, out arg))
            {
                return new ParseResult<INamedArgumentBase>(
                    new ParseException(name.ToString(), String.Format(
                        "Unknown argument name '{0}'.", name)));
            }
            return new ParseResult<INamedArgumentBase>(arg);
        }

        private ParseResult ParsePositionalArguments(Stack<string> args, bool positionalDelimiterFound)
        {
            foreach (var arg in Config.PositionalArguments)
            {
                var result = ParseArgument(arg.Name.ToLowerInvariant(), arg, args, positionalDelimiterFound);
                if (!result.IsSuccess) return result;
            }
            return ParseResult.Success;
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
        private ParseResult ParseArgument(string attrName, IArgument arg, Stack<string> remainingArgs, bool positionalDelimiterFound)
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
                                if(arg.Constraint == NumArgsConstraint.Optional)
                                {
                                    var defaultValue = arg.Const;
                                    object converted;
                                    if (TryConvertFrom(store, defaultValue, out converted))
                                    {
                                        store.SetValue(Object, converted);
                                    }
                                    else
                                    {
                                        return new ParseResult(
                                            new ParseException(attrName, String.Format(
                                                @"Argument {0} value ""{1}"" cannot be converted to the " +
                                                "required type {2}.",
                                                attrName, defaultValue, store.Type)));
                                    }
                                    break;
                                }
                                else if (!arg.PromptIfValueMissing.Enabled)
                                {
                                    return new ParseResult(
                                        new ParseException(attrName, String.Format(
                                            @"Argument ""{0}"" requires a value but " +
                                            "none was provided.", attrName)));
                                }
                                else
                                {
                                    var value = arg.PromptIfValueMissing.Prompt(attrName);
                                    try
                                    {
                                        object converted;
                                        if (TryConvertFrom(store, value, out converted))
                                        {
                                            store.SetValue(Object, converted);
                                        }
                                        else
                                        {
                                            return new ParseResult(
                                                new ParseException(attrName, String.Format(
                                                    @"Argument {0} value ""{1}"" cannot be converted to the " +
                                                    "required type {2}.",
                                                    attrName, value, store.Type)));
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        return new ParseResult(
                                            new ParseException(attrName, String.Format(
                                                @"Argument {0} value ""{1}"" cannot be converted to the " +
                                                "required type {2}.",
                                                 attrName, value, store.Type), e));
                                    }
                                    break;
                                }
                            }
                            var stringValue = remainingArgs.Pop();
                            if(arg.PromptIfValueMissing.Enabled && stringValue == arg.PromptIfValueMissing.SignalString)
                            {
                                var value = arg.PromptIfValueMissing.Prompt(attrName);
                                try
                                {
                                    object converted;
                                    if (TryConvertFrom(store, value, out converted))
                                    {
                                        store.SetValue(Object, converted);
                                    }
                                    else
                                    {
                                        return new ParseResult(
                                            new ParseException(attrName, String.Format(
                                                @"Argument {0} value ""{1}"" cannot be converted to the " +
                                                "required type {2}.",
                                                attrName, value, store.Type)));
                                    }
                                }
                                catch (Exception e)
                                {
                                    return new ParseResult(
                                        new ParseException(attrName, String.Format(
                                            @"Argument {0} value ""{1}"" cannot be converted to the " +
                                            "required type {2}.",
                                             attrName, value, store.Type), e));
                                }
                                break;
                            }

                            try
                            {
                                object converted;
                                if (TryConvertFrom(store, stringValue, out converted))
                                {
                                    store.SetValue(Object, converted);
                                }
                                else
                                {
                                    return new ParseResult(
                                        new ParseException(attrName, String.Format(
                                            @"Argument {0} value ""{1}"" cannot be converted to the " +
                                            "required type {2}.",
                                            attrName, stringValue, store.Type)));
                                }
                            }
                            catch (Exception e)
                            {
                                return new ParseResult(
                                    new ParseException(attrName, String.Format(
                                        @"Argument {0} value ""{1}"" cannot be converted to the " +
                                        "required type {2}.",
                                         attrName, stringValue, store.Type), e));
                            }
                        }
                        else
                        {
                            var existing = (IEnumerable)store.GetValue(Object);
                            var backingList = CreateGenericList(store, existing);

                            var result = ParseVarargs(attrName, backingList, arg, remainingArgs, positionalDelimiterFound);
                            if (!result.IsSuccess)
                            {
                                return result;
                            }
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
                                return new ParseResult(new ParseException(attrName));
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
                                    return new ParseResult(
                                        new ParseException(attrName, String.Format(
                                            @"Argument {0} value ""{1}"" cannot be converted to the " +
                                            "required type {2}.",
                                            attrName, stringValue, arg.Store.Type)));
                                }
                            }
                            catch (Exception e)
                            {
                                return new ParseResult(
                                    new ParseException(attrName, String.Format(
                                        @"Argument {0} value ""{1}"" cannot be converted to the " +
                                        "required type {2}.",
                                        attrName, stringValue, store.Type), e));
                            }
                        }
                        else
                        {
                            var result = ParseVarargs(attrName, backingList, arg, remainingArgs, positionalDelimiterFound);
                            if (!result.IsSuccess)
                            {
                                return result;
                            }
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
            return ParseResult.Success;
        }

        private ParseResult ParseVarargs(string attrName, IList list,
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
#if NET35
                        .ToString(CultureInfo.InvariantCulture)) &&
#else
                        .ToString()) &&
#endif
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
                        return new ParseResult(
                            new ParseException(attrName, String.Format(
                                @"Argument {0} value ""{1}"" cannot be converted to the " +
                                "required type {2}.",
                                attrName, stringValue, arg.Store.Type)));
                    }
                }
                catch (Exception e)
                {
                    return new ParseResult(
                        new ParseException(attrName, String.Format(
                            @"Argument {0} value ""{1}"" cannot be converted to the " +
                            "required type {2}.",
                            attrName, stringValue, arg.Store.Type), e));
                }
                argsProcessed++;
            }

            if (argsProcessed < minArgs)
            {
                return new ParseResult(
                    new ParseException(attrName, String.Format(
                        "Parameter {0} requires {1} {2} value(s).",
                        arg.MetaVar ?? arg.Name,
                        arg.Constraint == NumArgsConstraint.Exactly ?
                            "exactly" : "at least",
                        minArgs)));
            }
            return ParseResult.Success;
        }

#endregion

        private ParseResult ParsingCleanup()
        {
            var missingRequiredMutexGroups = Config
                .RequiredMutuallyExclusiveArguments
                .Except(_parsedMutuallyExclusiveGroups).ToArray();
            if (missingRequiredMutexGroups.Any())
            {
                return new ParseResult(
                    new ParseException(null, String.Format(
                        @"Required mutually exclusive group(s) ""{0}"" were " +
                        "not provided.",
                        String.Join(", ", missingRequiredMutexGroups))));
            }

            var missingRequiredNamedArguments = Config
                .RequiredNamedArguments
                .Except(_parsedNamedArguments).ToArray();
            if (missingRequiredNamedArguments.Any())
            {
                return new ParseResult(
                    new ParseException(null, String.Format(
                        @"Required named argument(s) ""{0}"" were " +
                        "not provided.",
                        String.Join(", ", missingRequiredNamedArguments))));
            }

            foreach (var method in Config.PostParseMethods)
            {
                try
                {
                    method.Invoke(Object, null);
                }
                catch(Exception e)
                {
                    return new ParseResult(
                        new ParseException(null, String.Format(
                            "Post-parse method '{0}' failed with exception: {1}", method.Name, e)));
                }
            }

            return ParseResult.Success;
        }

        private static bool TryConvertFrom(IValueStoreDefinition store, object value, out object obj)
        {
            if(value == null)
            {
                // Reference types and nullables can be set to null
                obj = null;
                var sti = store.Type.GetTypeInfo();
                return !sti.IsValueType ||
                    (sti.IsGenericType &&
                     sti.GetGenericTypeDefinition() == typeof(Nullable<>));
            }

            var srcType = value.GetType();
            var destType = store.Type;

            // Types are the same, no conversion necessary
            if(srcType == destType)
            {
                obj = value;
                return true;
            }

            // Source is a string, try to convert to the destination type
            if(srcType == typeof(string))
            {
                return TryConvertFrom(store, value, out obj);
            }

            // Source is nullable and underlying type is of the same type as destination
            var ti = destType.GetTypeInfo();
            if (ti.IsGenericType)
            {
                var def = ti.GetGenericTypeDefinition();
                var args = ti.GetGenericArguments();
                if(def == typeof(Nullable<>) && args.Length == 1 && args[0] == srcType)
                {
                    obj = value;
                    return true;
                }
            }

            obj = null;
            return false;
        }

        private static bool TryConvertFrom(IValueStoreDefinition store, string value, out object obj)
        {
            var culture = CultureInfo.CurrentUICulture;

            var customConverter = store.Converters != null
                ? store.Converters
                    .FirstOrDefault(c => c.CanConvertFrom(typeof(string)))
                : null;
            if (customConverter != null)
            {
                obj = customConverter.ConvertFromString(null, culture, value);
                return true;
            }

            var converter = TypeDescriptor.GetConverter(store.Type);
            if (converter.CanConvertFrom(typeof(string)))
            {
                obj = converter.ConvertFromString(null, culture, value);
                return true;
            }
            obj = null;
            return false;
        }

        private static bool TryConvertFromGeneric(IValueStoreDefinition store, string value, out object obj)
        {
#if NET35
            var innerType = store.Type.GetGenericArguments().First();
#else
            var innerType = store.Type.GetTypeInfo().GetGenericArguments().First();
#endif
            var tempStore = new DummyValueStore
            {
                Name = store.Name,
                Converters = store.Converters
                    .Concat(AttributeConverter.GetConverters(innerType))
                    .ToArray(),
                Type = innerType
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
#if NET35
            var type = store.Type.GetGenericArguments();
#else
            var type = store.Type.GetTypeInfo().GetGenericArguments();
#endif
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
