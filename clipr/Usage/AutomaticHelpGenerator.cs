using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;
using clipr.Arguments;
using clipr.Core;
using clipr.Triggers;
using clipr.Utils;

namespace clipr.Usage
{
    /// <summary>
    /// Builds usage information automatically from the associated
    /// type.
    /// </summary>
    public class AutomaticHelpGenerator<T> : TriggerBase, IHelpGenerator
    {
        /// <inheritdoc/>
        public override string Name { get { return "HelpGenerator"; } }

        /// <inheritdoc/>
        public override string Description
        {
            get { return "Display this help document."; }
        }

        private const string Indent = " ";

        private const int LineWidth = 80;

        private const int Spacing = 2;

        /// <summary>
        /// Description for the version help.
        /// </summary>
        protected virtual string VersionDescription
        {
            get { return "Display version information."; }
        }

        /// <summary>
        /// Title of the usage section.
        /// </summary>
        protected virtual string UsageTitle
        {
            get { return "usage"; }
        }

        /// <summary>
        /// Title of the positional arguments section.
        /// </summary>
        protected virtual string PositionalArgumentsTitle
        {
            get { return "positional arguments"; }
        }

        /// <summary>
        /// Title of the optional arguments section.
        /// </summary>
        protected virtual string OptionalArgumentsTitle
        {
            get { return "optional arguments"; }
        }

        private readonly Func<IPositionalArgument, int> _argumentIndex =
            p => p.Index;

        private readonly Func<INamedArgument, string> _argumentDisplayName =
            p => p.MetaVar ?? p.Name;

        /// <summary>
        /// Create a new generator.
        /// </summary>
        public AutomaticHelpGenerator()
        {
            ShortName = 'h';
            LongName = "help";
        }

        /// <inheritdoc/>
        public string GetUsage(IParserConfig config)
        {
            var assembly = Assembly.GetEntryAssembly();
            var builder = new StringBuilder();
            builder.Append(UsageTitle);
            builder.Append(": ");
            builder.Append(Path.GetFileNameWithoutExtension(assembly != null ?
                assembly.Location : Assembly.GetExecutingAssembly().CodeBase));
            builder.Append(" ");

            foreach (var arg in config.LongNameArguments.Values.Cast<INamedArgument>()
                .Concat(config.ShortNameArguments.Values.Cast<INamedArgument>())
                .Distinct())
            {
                builder.Append("[ ");
                builder.Append(String.Join("|", GetArgumentNames(arg).ToArray()));
                builder.Append(" ");

                if (arg.Action.ConsumesArgumentValues())
                {
                    if (arg.Constraint == NumArgsConstraint.AtMost)
                    {
                        builder.Append("[ ");
                    }

                    for (var i = 0; i < arg.NumArgs; i++)
                    {
                        if(arg.MetaVar == null) continue;
                        builder.Append(arg.MetaVar.ToUpper(CultureInfo.CurrentCulture));
                        builder.Append(" ");
                    }

                    if (arg.Constraint == NumArgsConstraint.AtLeast)
                    {
                        builder.Append("... ");
                    }

                    if (arg.Constraint == NumArgsConstraint.AtMost)
                    {
                        builder.Append("] ");
                    }
                }

                builder.Append("] ");
            }

            foreach (var arg in config.PositionalArguments.OrderBy(p => p.Index))
            {
                if (arg.Constraint == NumArgsConstraint.AtMost)
                {
                    builder.Append("[ ");
                }

                for (var i = 0; i < arg.NumArgs; i++)
                {
                    if (arg.MetaVar == null) continue;
                    builder.Append(arg.MetaVar.ToUpperInvariant());
                    builder.Append(" ");
                }

                var store = arg.Store;
                
                if (store.Type.IsSubclassOf(typeof (Enum)))
                {
                    AddEnumFormat(builder, store);
                }
                if (store.Type.GetCustomAttribute<StaticEnumerationAttribute>() != null)
                {
                    AddStaticEnumFormat(builder, store);
                }

                var staticEnum = (store.GetCustomAttribute<StaticEnumerationAttribute>() ??
                                  store.Type.GetCustomAttribute<StaticEnumerationAttribute>()) != null;
                if (staticEnum)
                {
                    AddStaticEnumFormat(builder, store);
                }

                if (arg.Constraint == NumArgsConstraint.AtLeast)
                {
                    builder.Append("... ");
                }

                if (arg.Constraint == NumArgsConstraint.AtMost)
                {
                    builder.Append("] ");
                }
            }

            return builder.ToString();
        }

        private static void AddEnumFormat(StringBuilder builder, IValueStoreDefinition store)
        {
            builder.Append("{");
            var names = Enum.GetNames(store.Type);
            for (var i = 0; i < names.Length; i++)
            {
                builder.Append(names[i]);
                if (i < names.Length - 1)
                {
                    builder.Append("|");
                }
            }
            builder.Append("}");
        }

        private static void AddStaticEnumFormat(StringBuilder builder, IValueStoreDefinition store)
        {
            builder.Append("{");
            var names = store.Type.GetFields(
                BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsInitOnly &&
                            store.Type.IsAssignableFrom(f.FieldType))
                .Select(f => f.Name)
                .ToArray();
            builder.Append(String.Join("|", names));
            builder.Append("}");
        }

        /// <inheritdoc/>
        public string GetHelp(IParserConfig config)
        {
            var positionalArgs = config.PositionalArguments.ToList();

            var namedArgs = config.LongNameArguments.Values.Cast<INamedArgument>()
                .Concat(config.ShortNameArguments.Values.Cast<INamedArgument>())
                .Distinct()
                .ToList();

            var positionalDisplay = positionalArgs
                .OrderBy(_argumentIndex)
                .Select(GetArgumentsForDisplay).ToList();

            var namedDisplay = namedArgs
                .OrderBy(_argumentDisplayName)
                .Select(GetArgumentsForDisplay).ToList();

            var tabstop = positionalDisplay
                .Concat(namedDisplay)
                .Select(a => a.ArgumentNames.Length)
                .Max() + Indent.Length + Spacing;

            var helpDataBuilder = new StringBuilder();

            helpDataBuilder.AppendLine(GetUsage(config));

            var metadata = typeof (T).GetCustomAttribute<ApplicationInfoAttribute>();
            if (metadata != null && metadata.Description != null)
            {
                helpDataBuilder.AppendLine();
                foreach (var line in metadata.Description.ReflowWords(LineWidth))
                {
                    helpDataBuilder.Append(Indent);
                    helpDataBuilder.AppendLine(line);
                }
                helpDataBuilder.AppendLine();
            }

            if (positionalArgs.Any())
            {
                helpDataBuilder.AppendLine(PositionalArgumentsTitle + ":");
            }
            foreach (var arg in positionalDisplay)
            {
                helpDataBuilder.Append(Indent);
                helpDataBuilder.Append(arg.ArgumentNames.PadRight(
                    tabstop - Indent.Length));
                if (tabstop + arg.Description.Length < LineWidth)
                {
                    helpDataBuilder.AppendLine(arg.Description);
                }
                else
                {
                    var iter = arg.Description.ReflowWords(
                        LineWidth - tabstop).GetEnumerator();
                    iter.MoveNext();
                    helpDataBuilder.AppendLine(iter.Current);
                    while (iter.MoveNext())
                    {
                        if(iter.Current == null) continue;

                        helpDataBuilder.AppendLine(
                            iter.Current.PadLeft(LineWidth));
                    }
                }
            }

            if (namedArgs.Any())
            {
                if (positionalArgs.Any())
                {
                    helpDataBuilder.AppendLine();
                }
                helpDataBuilder.AppendLine(OptionalArgumentsTitle + ":");
            }
            foreach (var arg in namedDisplay)
            {
                helpDataBuilder.Append(Indent);
                helpDataBuilder.Append(arg.ArgumentNames.PadRight(
                    tabstop - Indent.Length));
                if (tabstop + arg.Description.Length < LineWidth)
                {
                    helpDataBuilder.AppendLine(arg.Description);
                }
                else
                {
                    var iter = arg.Description.ReflowWords(
                        LineWidth - tabstop).GetEnumerator();
                    iter.MoveNext();
                    helpDataBuilder.AppendLine(iter.Current);
                    while (iter.MoveNext())
                    {
                        helpDataBuilder.Append("".PadRight(tabstop));
                        helpDataBuilder.AppendLine(iter.Current);
                    }
                }
            }

            return helpDataBuilder.ToString().TrimEnd();
        }

        private static ArgumentDisplay GetArgumentsForDisplay(IPositionalArgument arg)
        {
            var names = new List<string>();
            if(arg.MetaVar != null) names.Add(arg.MetaVar);
            return GetArgumentsForDisplay(names, arg);
        }

        private static ArgumentDisplay GetArgumentsForDisplay(INamedArgument attr)
        {
            var names = GetArgumentNames(attr);
            return GetArgumentsForDisplay(names, attr);
        }

        private static ArgumentDisplay GetArgumentsForDisplay(List<string> names, IArgument attr)
        {
            if (!names.Any())
            {
                names.Add(attr.Name);
            }
            return new ArgumentDisplay
            {
                ArgumentNames = String.Join(", ", names.ToArray()),
                Description = attr.Description ?? ""
            };
        }

        private static List<string> GetArgumentNames(INamedArgument attr)
        {
            var ret = new List<string>();
            if (attr.ShortName != null)
            {
                ret.Add("-" + attr.ShortName);
            }
            if (attr.LongName != null)
            {
                ret.Add("--" + attr.LongName);
            }
            return ret;
        }

        private class ArgumentDisplay
        {
            public string ArgumentNames { get; set; }

            public string Description { get; set; }
        }

        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public string PluginName
        {
            get { return "Help"; }
        }

        /// <summary>
        /// Action to perform when this trigger is parsed.
        /// </summary>
        /// <param name="config"></param>
        public void OnParse(IParserConfig config)
        {
            Console.Error.WriteLine(GetHelp(config));
        }
    }
}
