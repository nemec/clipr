using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using clipr.Arguments;
using clipr.Triggers;

namespace clipr
{
    public abstract class ParserConfig<T> where T : class
    {
        public readonly char[] LongOptionSeparator = new[] { '=' };

        public char ArgumentPrefix { get; set; }

        internal readonly Dictionary<char, IShortNameArgument> ShortNameArguments;

        internal readonly Dictionary<string, ILongNameArgument> LongNameArguments;

        internal readonly List<IPositionalArgument> PositionalArguments;

        internal readonly Dictionary<string, PropertyInfo> SubCommands;

        internal readonly List<MethodInfo> PostParseMethods;

        internal readonly HashSet<string> RequiredMutuallyExclusiveArguments;

        public IEnumerable<ITrigger<T>> Triggers { get; set; }

        protected ParserConfig(ParserOptions options, IEnumerable<ITrigger<T>> triggers)
        {
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
            SubCommands = new Dictionary<string, PropertyInfo>();
            PostParseMethods = new List<MethodInfo>();
            RequiredMutuallyExclusiveArguments = new HashSet<string>();

            InitializeTriggers(triggers);
        }

        private void InitializeTriggers(IEnumerable<ITrigger<T>> triggers)
        {
            Triggers = triggers;
            foreach (var trigger in Triggers.Where(p => p != null))
            {
                trigger.Config = this;

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

        protected char? GetShortName(IShortNameArgument arg)
        {
            return GetShortName(arg, String.Format(
                "Short name {0} is not allowed. {1}",
                arg.ShortName,
                ArgumentValidation.IsAllowedShortNameExplanation));
        }

        protected char? GetShortName(IShortNameArgument arg, string errorMessage)
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

        protected string GetLongName(ILongNameArgument arg)
        {
            return GetLongName(arg, String.Format(
                "Long name {0} is not allowed. {1}",
                arg.LongName,
                ArgumentValidation.IsAllowedLongNameExplanation));
        }

        protected string GetLongName(ILongNameArgument arg, string errorMessage)
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
