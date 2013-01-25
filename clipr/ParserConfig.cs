using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using clipr.Annotations;
using clipr.Arguments;
using clipr.Triggers;

namespace clipr
{
    public class ParserConfig<T> where T : class
    {
        public readonly char[] LongOptionSeparator = new[] { '=' };

        private readonly Func<char, bool> _isAllowedShortName =
            c => Char.IsLetter(c);

        private const string IsAllowedShortNameExplanation =
            "Short arguments must be letters.";

        private readonly Func<string, bool> _isAllowedLongName =
            s => s != null &&
                Regex.IsMatch(s, @"^[a-zA-Z][a-zA-Z0-9\-]*[a-zA-Z0-9]$");

        private const string IsAllowedLongNameExplanation =
            "Long arguments must begin with a letter, contain a letter, " +
            "digit, or hyphen, and end with a letter or a digit.";

        public char ArgumentPrefix { get; set; }

        internal Dictionary<char, IShortNameArgument> ShortNameArguments { get; set; }

        internal Dictionary<string, ILongNameArgument> LongNameArguments { get; set; }

        internal List<IPositionalArgument> PositionalArguments { get; set; }

        public Dictionary<string, PropertyInfo> SubCommands { get; set; }

        public List<MethodInfo> PostParseMethods { get; set; }

        public IEnumerable<ITrigger<T>> Triggers { get; set; }

        public ParserConfig(ParserOptions options, IEnumerable<ITrigger<T>> triggers)
        {
            var type = typeof(T);

            ArgumentPrefix = '-';

            PositionalArguments = BuildPositionalArguments();

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

            InitializeTriggers(triggers);
            
            InitializeNamedArguments(type.GetProperties().Where(p =>
                p.GetCustomAttribute<NamedArgumentAttribute>() != null));  // TODO no more Attributes

            SubCommands = BuildSubCommands();
            PostParseMethods = BuildPostParseMethods();
        }

        private static List<IPositionalArgument> BuildPositionalArguments()
        {
            return typeof(T).GetProperties()  // TODO no more Attributes
                .Where(p => p.GetCustomAttribute<PositionalArgumentAttribute>() != null)
                .OrderBy(p => p.GetCustomAttribute<PositionalArgumentAttribute>().Index)
                .Select(p => p.ToPositionalArgument())
                .ToList();
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
                    IsAllowedShortNameExplanation));
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
                    trigger.PluginName, trigger.LongName, IsAllowedLongNameExplanation));
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

        private void InitializeNamedArguments(IEnumerable<PropertyInfo> props)
        {
            foreach (var prop in props)
            {
                var arg = prop.ToNamedArgument();

                var sn = GetShortName(arg);
                if (sn.HasValue)
                {
                    if (ShortNameArguments.ContainsKey(sn.Value))
                    {
                        throw new DuplicateArgumentException(sn.ToString());
                    }
                    ShortNameArguments.Add(sn.Value, arg);
                }

                var ln = GetLongName(arg);
                if (ln != null)
                {
                    if (LongNameArguments.ContainsKey(ln))
                    {
                        throw new DuplicateArgumentException(ln);
                    }
                    LongNameArguments.Add(ln, arg);
                }
            }
        }

        private char? GetShortName(IShortNameArgument arg)
        {
            return GetShortName(arg, String.Format(
                "Short name {0} is not allowed. {1}",
                arg.ShortName, IsAllowedShortNameExplanation));
        }

        private char? GetShortName(IShortNameArgument arg, string errorMessage)
        {
            if (arg.ShortName.HasValue)
            {
                if (!_isAllowedShortName(arg.ShortName.Value))
                {
                    throw new ArgumentIntegrityException(errorMessage);
                }
                return arg.ShortName.Value;
            }
            return null;
        }

        private string GetLongName(ILongNameArgument arg)
        {
            return GetLongName(arg, String.Format(
                "Long name {0} is not allowed. {1}",
                arg.LongName, IsAllowedLongNameExplanation));
        }

        private string GetLongName(ILongNameArgument arg, string errorMessage)
        {
            if (arg.LongName != null)
            {
                if (arg.LongName.Length < 2)
                {
                    throw new ArgumentIntegrityException(String.Format(
                        "Long argument {0} must have at least two characters.",
                        arg.LongName));
                }
                if (!_isAllowedLongName(arg.LongName))
                {
                    throw new ArgumentIntegrityException(errorMessage);
                }
                return arg.LongName;
            }

            return null;
        }

        private Dictionary<string, PropertyInfo> BuildSubCommands()
        {
            var subcommands = new Dictionary<string, PropertyInfo>();

            foreach (var prop in typeof(T).GetProperties()
                .Where(p => p.GetCustomAttributes<SubCommandAttribute>().Any()))  // TODO no more Attributes
            {
                foreach (var attr in prop.GetCustomAttributes<SubCommandAttribute>())
                {
                    if (attr.Name.StartsWith(ArgumentPrefix.ToString()))
                    {
                        throw new ArgumentIntegrityException(String.Format(
                            "Subcommand {0} cannot begin with {1}",
                            attr.Name, ArgumentPrefix));
                    }
                    try
                    {
                        SubCommands.Add(attr.Name, prop);
                    }
                    catch (ArgumentException)
                    {
                        throw new DuplicateArgumentException(attr.Name);
                    }
                }
            }

            return subcommands;
        } 

        private static List<MethodInfo> BuildPostParseMethods()
        {
            return typeof(T).GetMethods()
                .Where(p => p.GetCustomAttribute<PostParseAttribute>() != null)  // TODO no more Attributes
                .ToList();
        } 
    }
}
