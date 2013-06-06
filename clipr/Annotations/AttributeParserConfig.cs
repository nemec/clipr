using System;
using System.Collections.Generic;
using System.Linq;
using clipr.Triggers;

namespace clipr.Annotations
{
    internal class AttributeParserConfig<T> : ParserConfig<T> where T : class
    {
        public AttributeParserConfig(ParserOptions options, IEnumerable<ITrigger<T>> triggers)
            : base(options, triggers)
        {
            InitializeSubCommands();
            InitializeNamedArguments();
            InitializePostParseMethods();
            InitializePositionalArguments();
            InitializeRequiredMutuallyExclusiveArguments();
        }

        private void InitializeRequiredMutuallyExclusiveArguments()
        {
            RequiredMutuallyExclusiveArguments.UnionWith(
                typeof(T).GetProperties()
                    .SelectMany(p => p.GetCustomAttributes<MutuallyExclusiveGroupAttribute>()
                    .Where(a => a.Required)
                    .Select(a => a.Name)));
        }

        private void InitializePositionalArguments()
        {
            PositionalArguments.AddRange(typeof(T).GetProperties()
                .Where(p => p.GetCustomAttribute<PositionalArgumentAttribute>() != null)
                .OrderBy(p => p.GetCustomAttribute<PositionalArgumentAttribute>().Index)
                .Select(p => p.ToPositionalArgument()));
        }

        private void InitializeNamedArguments()
        {
            var props = typeof (T).GetProperties().Where(p =>
                p.GetCustomAttribute<NamedArgumentAttribute>() != null);
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

        private void InitializeSubCommands()
        {
            foreach (var prop in typeof(T).GetProperties()
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
        }

        private void InitializePostParseMethods()
        {
            PostParseMethods.AddRange(typeof(T).GetMethods()
                .Where(p => p.GetCustomAttribute<PostParseAttribute>() != null));
        }
    }
}
