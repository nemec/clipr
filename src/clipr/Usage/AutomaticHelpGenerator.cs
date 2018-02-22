using System;
using System.Collections.Generic;
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
            get { return I18N._("AutomaticHelpGenerator_Description"); }
        }

        private const string Indent = " ";

        private const int Spacing = 2;

        /// <summary>
        /// Title of the usage section.
        /// </summary>
        protected virtual string UsageTitle
        {
            get { return I18N._("AutomaticHelpGenerator_Usage"); }
        }

        /// <summary>
        /// Title of the positional arguments section.
        /// </summary>
        protected virtual string PositionalArgumentsTitle
        {
            get { return I18N._("AutomaticHelpGenerator_PositionalArgumentsTitle"); }
        }

        /// <summary>
        /// Title of the optional named arguments section.
        /// </summary>
        protected virtual string OptionalNamedArgumentsTitle
        {
            get { return I18N._("AutomaticHelpGenerator_NamedArgumentsTitle"); }
        }

        /// <summary>
        /// Title of the required named arguments section.
        /// </summary>
        protected virtual string RequiredNamedArgumentsTitle
        {
            get { return I18N._("AutomaticHelpGenerator_NamedRequiredArgumentsTitle"); }
        }

        /// <summary>
        /// Title of the Verbs section
        /// </summary>
        protected virtual string CommandsTitle
        {
            get { return I18N._("AutomaticHelpGenerator_CommandsTitle"); }
        }

        /// <summary>
        /// Title of the Verbs section
        /// </summary>
        protected virtual string CommandsUsagePlaceholder
        {
            get { return I18N._("AutomaticHelpGenerator_Usage_CommandPlaceholder"); }
        }

        private readonly Func<IPositionalArgument, int> _argumentIndex =
            p => p.Index;

        private readonly Func<INamedArgument, string> _argumentDisplayName =
            p => p.MetaVar ?? p.Name;

        /// <summary>
        /// Set the minimum allowed display width. Defaults to 20 characters.
        /// </summary>
        public DisplayWidth MinDisplayWidth { get; set; }

        private DisplayWidth _displayWidth;
        /// <summary>
        /// Set the display width of the help information.
        /// DisplayWidth.Automatic will automatically set the width based on
        /// the current console width.
        /// </summary>
        public DisplayWidth DisplayWidth
        {
            get
            {
                return _displayWidth;
            }
            set
            {
                if(ReferenceEquals(value, DisplayWidth.Automatic) ||
                    value != null && value.CompareTo(MinDisplayWidth) > 0)
                {
                    _displayWidth = value;
                }
                else
                {
                    _displayWidth = MinDisplayWidth;
                }
            }
        }

        public readonly TextWriter DefaultHelpWriter = Console.Error;

        /// <summary>
        /// Create a new generator.
        /// </summary>
        public AutomaticHelpGenerator()
        {
            MinDisplayWidth = new DisplayWidth(20);
            _displayWidth = DisplayWidth.Automatic;
            ShortName = 'h';
            LongName = "help";
        }

        /// <inheritdoc/>
        public string GetUsage(IParserConfig config)
        {
            var builder = new StringBuilder();
            builder.Append(UsageTitle);
            builder.Append(": ");

            var metadata = config.OptionType.GetTypeInfo().GetCustomAttribute<ApplicationInfoAttribute>();
            if (metadata != null && metadata.Name != null)
            {
                // Dev has overridden the program name
                builder.Append(metadata.Name);
            }
            else
            {
                // Use the assembly name as the program name
                var assembly = Assembly.GetEntryAssembly();
                builder.Append(Path.GetFileNameWithoutExtension(assembly != null ?
                    assembly.Location : typeof(AutomaticHelpGenerator<T>).GetTypeInfo().Assembly.CodeBase));
            }

            // Append the verbs in case
            var verbConfig = config as IVerbParserConfig;
            if (verbConfig != null && verbConfig.PrecursorVerbs != null)
            {
                foreach(var pre in verbConfig.PrecursorVerbs)
                {
                    builder.Append(" ");
                    builder.Append(pre);
                }
            }

            builder.Append(" ");

            // TODO list PromptIfValueMissing help message on each argument that can be prompted.
            var allNamedArgs = config.LongNameArguments.Values.Cast<INamedArgument>()
                .Concat(config.ShortNameArguments.Values.Cast<INamedArgument>())
                .Distinct().ToList();
            for(var i=0; i <allNamedArgs.Count; i++)
            {
                var arg = allNamedArgs[i];
                if(i > 0)
                {
                    builder.Append(" ");
                }
                if (!arg.Required)
                {
                    builder.Append("[ ");
                }
                builder.Append(String.Join("|", GetArgumentNames(arg, config).ToArray()));

                if (arg.Action.ConsumesArgumentValues())
                {
                    if (arg.Constraint == NumArgsConstraint.AtMost || 
                        (arg.Constraint == NumArgsConstraint.AtLeast && arg.NumArgs == 0))
                    {
                        builder.Append(" [ ");
                    }

                    for (var j = Math.Max(1, (int)arg.NumArgs) - 1; j >= 0 ; j--)
                    {
                        if(arg.MetaVar == null) continue;
                        builder.Append(" ");
                        builder.Append(arg.MetaVar.ToUpperInvariant());
                    }

                    if (arg.Constraint == NumArgsConstraint.AtLeast)
                    {
                        builder.Append(" ...");
                    }

                    if (arg.Constraint == NumArgsConstraint.AtMost ||
                        (arg.Constraint == NumArgsConstraint.AtLeast && arg.NumArgs == 0))
                    {
                        builder.Append(" ]");
                    }
                }
                if (!arg.Required)
                {
                    builder.Append(" ]");
                }
            }

            if (config.PositionalArguments.Any())
            {
                builder.Append(" ");
            }
            var allPositionalArgs = config.PositionalArguments.OrderBy(p => p.Index).ToList();
            for(var i = 0; i < allPositionalArgs.Count; i++)
            {
                var arg = allPositionalArgs[i];
                if (arg.Constraint == NumArgsConstraint.AtMost ||
                        (arg.Constraint == NumArgsConstraint.AtLeast && arg.NumArgs == 0))
                {
                    builder.Append("[ ");
                }

                for (var j = 0; j < arg.NumArgs; j++)
                {
                    if (arg.MetaVar == null) continue;
                    builder.Append(arg.MetaVar.ToUpperInvariant());
                    if (i < allPositionalArgs.Count - 1)
                    {
                        builder.Append(" ");
                    }
                }

                var store = arg.Store;

                if (EnumUtils.IsEnum(store))
                {
                    var names = EnumUtils.GetEnumValues(store);
                    if (names.Length > 0)
                    {
                        builder.Append("{");
                        builder.Append(String.Join("|", names));
                        builder.Append("}");
                    }
                }

                if (arg.Constraint == NumArgsConstraint.AtLeast)
                {
                    builder.Append("... ");
                }

                if (arg.Constraint == NumArgsConstraint.AtMost ||
                        (arg.Constraint == NumArgsConstraint.AtLeast && arg.NumArgs == 0))
                {
                    builder.Append("]");
                    if (i < allPositionalArgs.Count - 1)
                    {
                        builder.Append(" ");
                    }
                }
            }

            if (config.Verbs.Any())
            {
                builder.Append(" <");
                builder.Append(CommandsUsagePlaceholder);
                builder.Append(">");
            }

            return builder.ToString();
        }

        /// <inheritdoc/>
        public string GetHelp(IParserConfig config)
        {
            var lineWidth = (DisplayWidth ?? DisplayWidth.Default).Width;
            var positionalArgs = config.PositionalArguments.ToList();

            var namedArgs = config.LongNameArguments.Values.Cast<INamedArgument>()
                .Concat(config.ShortNameArguments.Values.Cast<INamedArgument>())
                .Distinct()
                .ToList();

            var namedRequiredArgs = namedArgs.Where(a => a.Required).ToList();
            var namedOptionalArgs = namedArgs.Where(a => !a.Required).ToList();

            var positionalDisplay = positionalArgs
                .OrderBy(_argumentIndex)
                .Select(GetArgumentsForDisplay).ToList();

            var namedRequiredDisplay = namedRequiredArgs
                .OrderBy(_argumentDisplayName)
                .Select(r => GetArgumentsForDisplay(r, config)).ToList();

            var namedOptionalDisplay = namedOptionalArgs
                .OrderBy(_argumentDisplayName)
                .Select(o => GetArgumentsForDisplay(o, config)).ToList();

            var tabstop = positionalDisplay
                .Concat(namedRequiredDisplay)
                .Concat(namedOptionalDisplay)
                .Select(a => a.ArgumentNames.Length)
                .Max() + Indent.Length + Spacing;

            var helpDataBuilder = new StringBuilder();

            helpDataBuilder.AppendLine(GetUsage(config));
            var metadata = typeof (T).GetTypeInfo().GetCustomAttribute<ApplicationInfoAttribute>();
            if (metadata != null && metadata.Description != null)
            {
                helpDataBuilder.AppendLine();
                foreach (var line in metadata.Description.ReflowWords(lineWidth))
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
                // No padding needed if there is no description
                if (String.IsNullOrEmpty(arg.Description))
                {
                    helpDataBuilder.AppendLine(arg.ArgumentNames);
                    continue;
                }
                helpDataBuilder.Append(arg.ArgumentNames.PadRight(
                    tabstop - Indent.Length));
                if (tabstop + arg.Description.Length < lineWidth)
                {
                    helpDataBuilder.AppendLine(arg.Description);
                }
                else
                {
                    var iter = arg.Description.ReflowWords(
                        lineWidth - tabstop).GetEnumerator();
                    iter.MoveNext();
                    helpDataBuilder.AppendLine(iter.Current);
                    while (iter.MoveNext())
                    {
                        if(iter.Current == null) continue;

                        helpDataBuilder.AppendLine(
                            iter.Current.PadLeft(lineWidth));
                    }
                }
            }

            var prevSection = positionalArgs.Any();
            AppendNamedArgumentList(helpDataBuilder, RequiredNamedArgumentsTitle, namedRequiredDisplay, tabstop, lineWidth, prevSection);
            prevSection |= namedRequiredDisplay.Any();
            AppendNamedArgumentList(helpDataBuilder, OptionalNamedArgumentsTitle, namedOptionalDisplay, tabstop, lineWidth, prevSection);

            if (config.Verbs.Any())
            {
                helpDataBuilder.AppendLine();
                helpDataBuilder.Append(CommandsTitle);
                helpDataBuilder.AppendLine(":");
            }
            foreach(var verb in config.Verbs.Values.OrderBy(v => v.Name))
            {
                helpDataBuilder.Append(Indent);
                // No padding needed if there is no description
                if (String.IsNullOrEmpty(verb.Description))
                {
                    helpDataBuilder.AppendLine(verb.Name);
                    continue;
                }
                helpDataBuilder.Append(verb.Name.PadRight(
                    tabstop - Indent.Length));
                if (tabstop + verb.Description.Length < lineWidth)
                {
                    helpDataBuilder.AppendLine(verb.Description);
                }
                else
                {
                    var iter = verb.Description.ReflowWords(
                        lineWidth - tabstop).GetEnumerator();
                    iter.MoveNext();
                    helpDataBuilder.AppendLine(iter.Current);
                    while (iter.MoveNext())
                    {
                        if (iter.Current == null) continue;

                        helpDataBuilder.AppendLine(
                            iter.Current.PadLeft(lineWidth));
                    }
                }
            }

            return helpDataBuilder.ToString().TrimEnd();
        }

        private static void AppendNamedArgumentList(StringBuilder helpDataBuilder, string sectionTitle, List<ArgumentDisplay> args, int tabstop, int lineWidth, bool prevSection)
        {
            if (args.Any())
            {
                if (prevSection)
                {
                    helpDataBuilder.AppendLine();
                }
                helpDataBuilder.AppendLine(sectionTitle + ":");
            }
            foreach (var arg in args)
            {
                helpDataBuilder.Append(Indent);
                // No padding needed if there is no description
                if (String.IsNullOrEmpty(arg.Description))
                {
                    helpDataBuilder.AppendLine(arg.ArgumentNames);
                    continue;
                }

                helpDataBuilder.Append(arg.ArgumentNames.PadRight(
                    tabstop - Indent.Length));
                if (tabstop + arg.Description.Length < lineWidth)
                {
                    helpDataBuilder.AppendLine(arg.Description);
                }
                else
                {
                    var iter = arg.Description.ReflowWords(
                        lineWidth - tabstop).GetEnumerator();
                    iter.MoveNext();
                    helpDataBuilder.AppendLine(iter.Current);
                    while (iter.MoveNext())
                    {
                        helpDataBuilder.Append("".PadRight(tabstop));
                        helpDataBuilder.AppendLine(iter.Current);
                    }
                }
            }
        }

        private static ArgumentDisplay GetArgumentsForDisplay(IPositionalArgument arg)
        {
            var names = new List<string>();
            if(arg.MetaVar != null) names.Add(arg.MetaVar);
            return GetArgumentsForDisplay(names, arg);
        }

        private static ArgumentDisplay GetArgumentsForDisplay(INamedArgument attr, IParserConfig config)
        {
            var names = GetArgumentNames(attr, config);
            return GetArgumentsForDisplay(names, attr);
        }

        private static ArgumentDisplay GetArgumentsForDisplay(List<string> names, IArgument attr)
        {
            if (!names.Any())
            {
                names.Add(attr.Name);
            }
            string localized = null;
            var info = attr.LocalizationInfo;
            if(info != null)
            {
                localized = I18N._(info.ResourceType, info.ResourceName);
            }
            return new ArgumentDisplay
            {
                ArgumentNames = String.Join(", ", names.ToArray()),
                Description =  localized ?? attr.Description ?? ""
            };
        }

        private static List<string> GetArgumentNames(INamedArgument attr, IParserConfig config)
        {
            var ret = new List<string>();
            if (attr.ShortName != null)
            {
                ret.Add(""+ config.Options.ArgumentPrefix + attr.ShortName);
            }
            if (attr.LongName != null)
            {
                ret.Add("" + config.Options.ArgumentPrefix + config.Options.ArgumentPrefix + attr.LongName);
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
            (config.Options.OutputWriter 
                ?? DefaultHelpWriter).WriteLine(GetHelp(config));
        }
    }
}
