using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;

namespace clipr.Usage
{
    /// <summary>
    /// Builds usage information automatically from the associated
    /// type.
    /// </summary>
    /// <typeparam name="T">Type to inspect.</typeparam>
    public class AutomaticHelpGenerator<T> : IHelpGenerator
    {
        public char? ShortName { get; set; }

        public string LongName { get; set; }

        public IVersion Version { get; set; }

        private const string Indent = " ";

        private const int LineWidth = 80;

        private const int Spacing = 2;

        protected virtual string HelpDescription
        {
            get { return "Display this help document."; }
        }

        protected virtual string VersionDescription
        {
            get { return "Display version information."; }
        }

        protected virtual string UsageTitle
        {
            get { return "usage"; }
        }

        protected virtual string PositionalArgumentsTitle
        {
            get { return "positional arguments"; }
        }

        protected virtual string OptionalArgumentsTitle
        {
            get { return "optional arguments"; }
        }

        /// <summary>
        /// Create a new generator.
        /// </summary>
        public AutomaticHelpGenerator()
        {
            ShortName = 'h';
            LongName = "help";
            Version = new ExecutingAssemblyVersion();
        }

        public string GetUsage()
        {
            var assembly = Assembly.GetEntryAssembly();
            var builder = new StringBuilder();
            builder.Append(UsageTitle);
            builder.Append(": ");
            builder.Append(Path.GetFileNameWithoutExtension(assembly != null ?
                assembly.Location : Assembly.GetExecutingAssembly().CodeBase));
            builder.Append(" ");

            var argumentProperties = GetOrderedProperties();

            foreach (var prop in argumentProperties.Where(
                p => p.GetCustomAttribute<NamedArgumentAttribute>() != null))
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

            foreach (var prop in argumentProperties.Where(
                p => p.GetCustomAttribute<PositionalArgumentAttribute>() != null)
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

        public string GetHelp()
        {
            var positionalGroup = new List<ArgumentDisplay>();
            var optionalGroup = new List<ArgumentDisplay>();

            #region Add Help argument

            var helpNames = new List<string>();
            if (ShortName.HasValue)
            {
                helpNames.Add("-" + ShortName);
            }
            if (LongName != null)
            {
                helpNames.Add("--" + LongName);
            }
            if (helpNames.Any())
            {
                optionalGroup.Add(new ArgumentDisplay
                {
                    Description = HelpDescription,
                    ArgumentNames = String.Join(", ", helpNames.ToArray())
                });
            }

            #endregion

            #region Add Version argument

            if (Version != null)
            {
                var versionNames = new List<string>();
                if (Version.ShortName.HasValue)
                {
                    versionNames.Add("-" + Version.ShortName);
                }
                if (Version.LongName != null)
                {
                    versionNames.Add("--" + Version.LongName);
                }
                if (versionNames.Any())
                {
                    optionalGroup.Add(new ArgumentDisplay
                        {
                            Description = VersionDescription,
                            ArgumentNames = String.Join(", ", versionNames.ToArray())
                        });
                }
            }

            #endregion

            var argumentProperties = GetOrderedProperties();
            
            // Print only properties with an ArgumentAttribute,
            // ordered by display name.
            foreach (var prop in argumentProperties)
            {
                var attr = prop.GetCustomAttribute<ArgumentAttribute>();
                if (attr is PositionalArgumentAttribute)
                {
                    positionalGroup.Add(GetArgumentsForDisplay(prop, attr));
                }
                else if (attr is NamedArgumentAttribute)
                {
                    optionalGroup.Add(GetArgumentsForDisplay(prop, 
                        attr as NamedArgumentAttribute));
                }
                else
                {
                    throw new InvalidOperationException(
                        "There's a new ArgumentAttribute?");
                }
            }

            var tabstop = positionalGroup
                              .Concat(optionalGroup)
                              .Select(a => a.ArgumentNames.Length)
                              .Max() + Indent.Length + Spacing;

            var helpDataBuilder = new StringBuilder();

            helpDataBuilder.AppendLine(GetUsage());

            var metadata = typeof (T).GetCustomAttribute<CommandAttribute>();
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

            if (positionalGroup.Any())
            {
                helpDataBuilder.AppendLine(PositionalArgumentsTitle + ":");
            }
            foreach (var arg in positionalGroup)
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

            if (optionalGroup.Any())
            {
                if (positionalGroup.Any())
                {
                    helpDataBuilder.AppendLine();
                }
                helpDataBuilder.AppendLine(OptionalArgumentsTitle + ":");
            }
            foreach (var arg in optionalGroup)
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

        private static IEnumerable<PropertyInfo> GetOrderedProperties()
        {
            return typeof (T).GetProperties().Where(p =>
                    p.GetCustomAttribute<ArgumentAttribute>() != null)
                .OrderBy(p => p.GetCustomAttribute<ArgumentAttribute>()
                    .GetArgumentDisplayName() ?? p.Name);
        } 

        private ArgumentDisplay GetArgumentsForDisplay(
            PropertyInfo prop, NamedArgumentAttribute attr)
        {
            var names = GetArgumentNames(attr);
            if (!names.Any())
            {
                names.Add(prop.Name);
            }
            return new ArgumentDisplay
            {
                ArgumentNames = String.Join(", ", names.ToArray()),
                Description = GetDescriptionForProperty(prop)
            };
        }

        private ArgumentDisplay GetArgumentsForDisplay(
            PropertyInfo prop, ArgumentAttribute attr)
        {
            var names = GetArgumentNames(attr);
            if (!names.Any())
            {
                names.Add(prop.Name);
            }
            return new ArgumentDisplay
            {
                ArgumentNames = String.Join(", ", names.ToArray()),
                Description = GetDescriptionForProperty(prop)
            };
        }

        protected virtual string GetDescriptionForProperty(PropertyInfo prop)
        {
            var attr = prop.GetCustomAttribute<ArgumentAttribute>();
            return attr.Description;
        }

        private static List<string> GetArgumentNames(ArgumentAttribute attr)
        {
            return new List<string>{ attr.MetaVar };
        }

        private static List<string> GetArgumentNames(NamedArgumentAttribute attr)
        {
            var ret = new List<string>();
            if (attr.ShortName != default(char))
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
    }
}
