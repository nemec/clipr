using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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

        public Dictionary<char, PropertyInfo> ShortNameArguments { get; set; }

        public Dictionary<string, PropertyInfo> LongNameArguments { get; set; }

        public List<PropertyInfo> PositionalArguments { get; set; }

        public Dictionary<string, PropertyInfo> SubCommands { get; set; }

        public List<MethodInfo> PostParseMethods { get; set; }

        public IEnumerable<ITrigger<T>> Triggers { get; set; }

        public ParserConfig(ParserOptions options, IEnumerable<ITrigger<T>> triggers)
        {
            var type = typeof(T);

            ArgumentPrefix = '-';

            PositionalArguments = type.GetProperties()
                .Where(p => p.GetCustomAttribute<PositionalArgumentAttribute>() != null)
                .OrderBy(p => p.GetCustomAttribute<PositionalArgumentAttribute>().Index)
                .ToList();

            if (options.HasFlag(ParserOptions.CaseInsensitive))
            {
                ShortNameArguments = new Dictionary<char, PropertyInfo>(new CaseInsensitiveCharComparer());
                LongNameArguments = new Dictionary<string, PropertyInfo>(StringComparer.InvariantCultureIgnoreCase);
            }
            else
            {
                ShortNameArguments = new Dictionary<char, PropertyInfo>();
                LongNameArguments = new Dictionary<string, PropertyInfo>();
            }

            Triggers = triggers;
            foreach (var plugin in triggers.Where(p => p != null))
            {
                if (plugin.ShortName.HasValue)
                {
                    if (!_isAllowedShortName(plugin.ShortName.Value))
                    {
                        throw new ArgumentIntegrityException(String.Format(
                            "Trigger '{0}' argument {1} is not a valid short name. {2}",
                            plugin.PluginName, plugin.ShortName,
                            IsAllowedShortNameExplanation));
                    }
                    ShortNameArguments.Add(plugin.ShortName.Value, null);
                }
                if (plugin.LongName != null)
                {
                    if (!_isAllowedLongName(plugin.LongName))
                    {
                        throw new ArgumentIntegrityException(String.Format(
                            "Trigger '{0}' argument {1} is not a valid long name. {2}",
                            plugin.PluginName, plugin.LongName, IsAllowedLongNameExplanation));
                    }
                    LongNameArguments.Add(plugin.LongName, null);
                }
            }

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

            #region Init Subcommands

            SubCommands = new Dictionary<string, PropertyInfo>();

            foreach (var prop in type.GetProperties()
                .Where(p => p.GetCustomAttributes<SubCommandAttribute>().Any()))
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

            #endregion

            PostParseMethods = typeof (T).GetMethods()
                .Where(p => p.GetCustomAttribute<PostParseAttribute>() != null)
                .ToList();
        }
    }
}
