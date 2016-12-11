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
            var assembly = Assembly.GetEntryAssembly();
            var builder = new StringBuilder();
            builder.Append(UsageTitle);
            builder.Append(": ");
            builder.Append(Path.GetFileNameWithoutExtension(assembly != null ?
                assembly.Location : typeof(AutomaticHelpGenerator<T>).GetTypeInfo().Assembly.CodeBase));
            builder.Append(" ");

            // TODO list PromptIfValueMissing help message on each argument that can be prompted.
            foreach (var arg in config.LongNameArguments.Values.Cast<INamedArgument>()
                .Concat(config.ShortNameArguments.Values.Cast<INamedArgument>())
                .Distinct())
            {
                if (!arg.Required)
                {
                    builder.Append("[ ");
                }
                builder.Append(String.Join("|", GetArgumentNames(arg).ToArray()));
                if (!arg.Required)
                {
                    builder.Append(" ");
                }

                if (arg.Action.ConsumesArgumentValues())
                {
                    if (arg.Constraint == NumArgsConstraint.AtMost || 
                        (arg.Constraint == NumArgsConstraint.AtLeast && arg.NumArgs == 0))
                    {
                        builder.Append("[ ");
                    }

                    for (var i = 0; i < Math.Max(1, arg.NumArgs); i++)
                    {
                        if(arg.MetaVar == null) continue;
                        builder.Append(arg.MetaVar.ToUpperInvariant());
                        builder.Append(" ");
                    }

                    if (arg.Constraint == NumArgsConstraint.AtLeast)
                    {
                        builder.Append("... ");
                    }

                    if (arg.Constraint == NumArgsConstraint.AtMost ||
                        (arg.Constraint == NumArgsConstraint.AtLeast && arg.NumArgs == 0))
                    {
                        builder.Append("] ");
                    }
                }
                if (!arg.Required)
                {
                    builder.Append("] ");
                }
            }

            foreach (var arg in config.PositionalArguments.OrderBy(p => p.Index))
            {
                if (arg.Constraint == NumArgsConstraint.AtMost ||
                        (arg.Constraint == NumArgsConstraint.AtLeast && arg.NumArgs == 0))
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
                    builder.Append("] ");
                }
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
                .Select(GetArgumentsForDisplay).ToList();

            var namedOptionalDisplay = namedOptionalArgs
                .OrderBy(_argumentDisplayName)
                .Select(GetArgumentsForDisplay).ToList();

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
