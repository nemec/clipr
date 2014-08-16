using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using clipr.Arguments;
using clipr.Triggers;
using clipr.Utils;

namespace clipr.Core
{
    /// <summary>
    /// Configuration for the parser.
    /// </summary>
    /// <typeparam name="T">Option class.</typeparam>
    public interface IParserConfig<T> where T : class
    {
        /// <summary>
        /// The character prefix to use for designating arguments
        /// (typically a hyphen).
        /// </summary>
        char ArgumentPrefix { get; set; }

        /// <summary>
        /// The list of registered triggers.
        /// </summary>
        IEnumerable<ITrigger<T>> Triggers { get; set; }

        /// <summary>
        /// Initialize all triggers.
        /// </summary>
        /// <param name="triggers"></param>
        void InitializeTriggers(IEnumerable<ITrigger<T>> triggers);

        /// <summary>
        /// Retrieve the short character from an argument or throw an exception.
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        char? GetShortName(IShortNameArgument arg);

        /// <summary>
        /// Retrieve the short character from an argument or throw an exception.
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        char? GetShortName(IShortNameArgument arg, string errorMessage);

        /// <summary>
        /// Retrieve the long name from an argument or throw an exception.
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        string GetLongName(ILongNameArgument arg);

        /// <summary>
        /// Retrieve the long name from an argument or throw an exception.
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        string GetLongName(ILongNameArgument arg, string errorMessage);
    }

    internal class VerbConfig
    {
        public object Object { get; set; }

        public IParsingContext Context { get; set; }

        public IValueStoreDefinition Store { get; set; }
    }

    internal abstract class ParserConfig<T> : IParserConfig<T> where T : class
    {
        public readonly char[] LongOptionSeparator = { '=' };

        public char ArgumentPrefix { get; set; }

        protected readonly ParserOptions Options; 

        internal readonly Dictionary<char, IShortNameArgument> ShortNameArguments;

        internal readonly Dictionary<string, ILongNameArgument> LongNameArguments;

        internal readonly List<IPositionalArgument> PositionalArguments;

        internal readonly Dictionary<string, VerbConfig> Verbs;

        internal readonly List<MethodInfo> PostParseMethods;

        internal readonly HashSet<string> RequiredMutuallyExclusiveArguments;

        internal readonly HashSet<string> RequiredNamedArguments; 

        public IEnumerable<ITrigger<T>> Triggers { get; set; }

        protected ParserConfig(ParserOptions options, IEnumerable<ITrigger<T>> triggers)
        {
            Options = options;
            ArgumentPrefix = '-';

            if (options.HasFlag(ParserOptions.CaseInsensitive))
            {
                ShortNameArguments = new Dictionary<char, IShortNameArgument>(new CaseInsensitiveCharComparer());
                LongNameArguments = new Dictionary<string, ILongNameArgument>(StringComparer.InvariantCultureIgnoreCase);
            }
            else
            {
                ShortNameArguments = new Dictionary<char, IShortNameArgument>();
                LongNameArguments = new Dictionary<string, ILongNameArgument>();
            }

            PositionalArguments = new List<IPositionalArgument>();
            Verbs = new Dictionary<string, VerbConfig>();
            PostParseMethods = new List<MethodInfo>();
            RequiredMutuallyExclusiveArguments = new HashSet<string>();
            RequiredNamedArguments = new HashSet<string>();

            InitializeTriggers(triggers);
        }

        public void InitializeTriggers(IEnumerable<ITrigger<T>> triggers)
        {
            Triggers = triggers;
            if (triggers == null)
            {
                return;
            }

            foreach (var trigger in Triggers.Where(p => p != null))
            {
                var sn = GetShortName(trigger, String.Format(
                    "Trigger '{0}' argument {1} is not a valid short name. {2}",
                    trigger.PluginName, trigger.ShortName,
                    ArgumentValidation.IsAllowedShortNameExplanation));
                if (sn.HasValue)
                {
                    if (ShortNameArguments.ContainsKey(sn.Value))
                    {
                        throw new DuplicateArgumentException(sn.ToString());
                    }
                    ShortNameArguments.Add(sn.Value, trigger);
                }

                var ln = GetLongName(trigger, String.Format(
                    "Trigger '{0}' argument {1} is not a valid long name. {2}",
                    trigger.PluginName, trigger.LongName,
                    ArgumentValidation.IsAllowedLongNameExplanation));
                if (ln != null)
                {
                    if (LongNameArguments.ContainsKey(ln))
                    {
                        throw new DuplicateArgumentException(ln);
                    }
                    LongNameArguments.Add(ln, trigger);
                }
            }
        }

        public char? GetShortName(IShortNameArgument arg)
        {
            return GetShortName(arg, String.Format(
                "Short name {0} is not allowed. {1}",
                arg.ShortName,
                ArgumentValidation.IsAllowedShortNameExplanation));
        }

        public char? GetShortName(IShortNameArgument arg, string errorMessage)
        {
            if (arg.ShortName.HasValue)
            {
                if (!ArgumentValidation.IsAllowedShortName(arg.ShortName.Value))
                {
                    throw new ArgumentIntegrityException(errorMessage);
                }
                return arg.ShortName.Value;
            }
            return null;
        }

        public string GetLongName(ILongNameArgument arg)
        {
            return GetLongName(arg, String.Format(
                "Long name {0} is not allowed. {1}",
                arg.LongName,
                ArgumentValidation.IsAllowedLongNameExplanation));
        }

        public string GetLongName(ILongNameArgument arg, string errorMessage)
        {
            if (arg.LongName != null)
            {
                if (arg.LongName.Length < 2)
                {
                    throw new ArgumentIntegrityException(String.Format(
                        "Long argument {0} must have at least two characters.",
                        arg.LongName));
                }
                if (!ArgumentValidation.IsAllowedLongName(arg.LongName))
                {
                    throw new ArgumentIntegrityException(errorMessage);
                }
                return arg.LongName;
            }

            return null;
        }
    }
}
