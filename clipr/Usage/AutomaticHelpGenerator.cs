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
    /// <typeparam name="T">Type to inspect.</typeparam>
    public class AutomaticHelpGenerator<T> : TriggerBase, IHelpGenerator<T> where T : class
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

        private readonly Func<INamedArgumentBase, string> _argumentDisplayName =
            p => p.MetaVar ?? p.Name;

        /// <summary>
        /// Create a new generator.
        /// </summary>
        public AutomaticHelpGenerator()
        {
            ShortName = 'h';
            LongName = "help";
        }

        /// <summary>
        /// Get the usage as a string.
        /// </summary>
        /// <returns></returns>
        public string GetUsage()
        {
            var assembly = Assembly.GetEntryAssembly();
            var builder = new StringBuilder();
            builder.Append(UsageTitle);
            builder.Append(": ");
            builder.Append(Path.GetFileNameWithoutExtension(assembly != null ?
                assembly.Location : Assembly.GetExecutingAssembly().CodeBase));
            builder.Append(" ");

            foreach (var prop in typeof(T).GetProperties()
                .Where(p => p.GetCustomAttribute<NamedArgumentAttribute>() != null))
            {
                var attr = prop.GetCustomAttribute<NamedArgumentAttribute>();
                var meta = attr.MetaVar ?? prop.Name;

                builder.Append("[ ");
                builder.Append(String.Join("|", GetArgumentNames(attr).ToArray()));
                builder.Append(" ");

                if (attr.Action.ConsumesArgumentValues())
                {
                    if (attr.Constraint == NumArgsConstraint.AtMost)
                    {
                        builder.Append("[ ");
                    }

                    for (var i = 0; i < attr.NumArgs; i++)
                    {
                        builder.Append(meta.ToUpperInvariant());
                        builder.Append(" ");
                    }

                    if (attr.Constraint == NumArgsConstraint.AtLeast)
                    {
                        builder.Append("... ");
                    }

                    if (attr.Constraint == NumArgsConstraint.AtMost)
                    {
                        builder.Append("] ");
                    }
                }

                builder.Append("] ");
            }

            foreach (var prop in typeof(T).GetProperties()
                .Where(p => p.GetCustomAttribute<PositionalArgumentAttribute>() != null)
                .OrderBy(p => p.GetCustomAttribute<PositionalArgumentAttribute>().Index))
            {
                var attr = prop.GetCustomAttribute<PositionalArgumentAttribute>();
                var meta = attr.MetaVar ?? prop.Name;

                if (attr.Constraint == NumArgsConstraint.AtMost)
                {
                    builder.Append("[ ");
                }

                for (var i = 0; i < attr.NumArgs; i++)
                {
                    builder.Append(meta.ToUpperInvariant());
                    builder.Append(" ");
                }

                if (prop.PropertyType.IsSubclassOf(typeof (Enum)))
                {
                    AddEnumFormat(builder, prop);
                }

                if (attr.Constraint == NumArgsConstraint.AtLeast)
                {
                    builder.Append("... ");
                }

                if (attr.Constraint == NumArgsConstraint.AtMost)
                {
                    builder.Append("] ");
                }
            }

            return builder.ToString();
        }

        private static void AddEnumFormat(StringBuilder builder, PropertyInfo prop)
        {
            builder.Append("{");
            var names = Enum.GetNames(prop.PropertyType);
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

        /// <summary>
        /// Get all help information.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public string GetHelp(IParserConfig<T> config)
        {
            var positionalArgs = typeof (T).GetProperties()
                .Where(p => p.GetCustomAttribute<PositionalArgumentAttribute>() != null)
                .Select(p => p.GetCustomAttribute<PositionalArgumentAttribute>() as IPositionalArgument)
                .ToList();

            var namedArgs = typeof(T).GetProperties()
                .Where(p => p.GetCustomAttribute<NamedArgumentAttribute>() != null)
                .Select(p => p.GetCustomAttribute<NamedArgumentAttribute>() as INamedArgumentBase)
                .ToList();

            foreach (var trigger in config.Triggers)
            {
                var names = new List<string>();
                if (trigger.ShortName.HasValue)
                {
                    names.Add("-" + trigger.ShortName);
                }
                if (trigger.LongName != null)
                {
                    names.Add("--" + trigger.LongName);
                }
                if (names.Any())
                {
                    namedArgs.Add(trigger);
                }
            }

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

            helpDataBuilder.AppendLine(GetUsage());

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

        private static ArgumentDisplay GetArgumentsForDisplay(IArgument attr)
        {
            List<string> names;

            if (attr is INamedArgument)  // TODO I don't like this
            {
                names = GetArgumentNames(attr as INamedArgument);
            }
            else
            {
                names = GetArgumentNames(attr);
            }

            if (!names.Any())
            {
                names.Add(attr.Name);
            }
            return new ArgumentDisplay
            {
                ArgumentNames = String.Join(", ", names.ToArray()),
                Description = attr.Description
            };
        }

        private static List<string> GetArgumentNames(IArgument attr)
        {
            return new List<string>{ attr.MetaVar };
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
        public void OnParse(IParserConfig<T> config)
        {
            Console.Error.WriteLine(GetHelp(config));
        }
    }
}
