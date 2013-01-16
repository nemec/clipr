using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace clipr
{
    internal class ParsingContext<T> where T : class
    {
        private ParserConfig<T> Config { get; set; }

        private T Object { get; set; }

        private HashSet<string> ParsedMutuallyExclusiveGroups { get; set; }

        public ParsingContext(T obj, ParserConfig<T> config)
        {
            Object = obj;
            Config = config;

            ParsedMutuallyExclusiveGroups = new HashSet<string>();
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
                        var partition = arg.Substring(2).Split(Config.LongOptionSeparator, 2);
                        if (partition.Length > 1)
                        {
                            values.Push(partition[1]);
                        }
                        ParseOptionalArgument(partition[0], Config.LongNameArguments, values);
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
                            ParseOptionalArgument(shortArg, Config.ShortNameArguments, values);

                            // No arguments were used... the arg
                            // we just pushed must be a group of
                            // short args.
                            if (values.Count == stackSize)
                            {
                                foreach (var shortName in values.Pop())
                                {
                                    // Arguments in a group cannot consume
                                    // values from the argStack
                                    ParseOptionalArgument(shortName, Config.ShortNameArguments, values);
                                }
                            }
                        }
                        else  // Just a single argument
                        {
                            ParseOptionalArgument(shortArg, Config.ShortNameArguments, values);
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
                    String.Join(" ", positionalArgStack.ToArray())));
            }

            ParsingCleanup();
        }

        #region Private Parsing Methods

        private void ParseOptionalArgument<TS>(TS name, IDictionary<TS, PropertyInfo> argDict, Stack<string> iter)
        {
            PropertyInfo prop;
            if (!argDict.TryGetValue(name, out prop))
            {
                throw new ParseException(name.ToString(), String.Format(
                    "Unknown argument name '{0}'.", name));
            }

            var attr = prop.GetCustomAttribute<ArgumentAttribute>();
            var mutexGroup = prop.GetCustomAttributes<MutuallyExclusiveGroupAttribute>();
            foreach (var groupAttribute in mutexGroup)
            {
                if (!ParsedMutuallyExclusiveGroups.Add(groupAttribute.Name))
                {
                    throw new ParseException(name.ToString(), String.Format(
                        @"Mutually exclusive group ""{0}"" violated.",
                        groupAttribute.Name));
                }
            }

            if (iter == null && attr.Action.ConsumesArgumentValues())
            {
                throw new ParseException(name.ToString(),
                    "Arguments that consume values cannot be grouped.");
            }
            ParseArgument(name.ToString(), prop, iter);
        }

        private void ParsePositionalArguments(Stack<string> args)
        {
            foreach (var prop in Config.PositionalArguments)
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
                            prop.SetValue(Object, ConvertFrom(prop, stringValue), null);
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
                        var values = (IList)prop.GetValue(Object, null);
                        if (values != null)
                        {
                            values.Clear();  // Store overwrites always
                        }
                        else
                        {
                            values = (IList)Activator.CreateInstance(prop.PropertyType);
                        }
                        ParseVarargs(attrName, values, prop, args);
                        prop.SetValue(Object, values, null);
                    }
                    break;

                case ParseAction.StoreConst:
                    prop.SetValue(Object, attr.Const, null);
                    break;

                case ParseAction.StoreTrue:
                    prop.SetValue(Object, true, null);
                    break;

                case ParseAction.StoreFalse:
                    prop.SetValue(Object, false, null);
                    break;

                case ParseAction.Append:
                    var appValues = (IList)prop.GetValue(Object, null) ??
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
                    prop.SetValue(Object, appValues, null);
                    break;
                case ParseAction.AppendConst:
                    var constValues = (IList)prop.GetValue(Object, null) ??
                        (IList)Activator.CreateInstance(prop.PropertyType);
                    constValues.Add(attr.Const);
                    prop.SetValue(Object, constValues, null);
                    break;

                case ParseAction.Count:
                    var cnt = (int)prop.GetValue(Object, null);
                    prop.SetValue(Object, cnt + 1, null);
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

        private void ParsingCleanup()
        {
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
                    String.Join(", ", missingRequiredMutexGroups.ToArray())));
            }

            foreach (var method in Config.PostParseMethods)
            {
                method.Invoke(Object, null);
            }
        }

        private static object ConvertFrom(PropertyInfo prop, string value)
        {
            return TypeDescriptor.GetConverter(prop.PropertyType)
                                    .ConvertFromInvariantString(value);
        }

        private static object ConvertFromGeneric(PropertyInfo prop, string value)
        {
            return TypeDescriptor.GetConverter(
                prop.PropertyType.GetGenericArguments().First())
                .ConvertFromInvariantString(value);
        }
    }
}
