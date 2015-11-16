using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using clipr.Triggers;
using clipr.Utils;

namespace clipr.Core
{
    internal class AttributeParserConfig<T> : ParserConfig<T> where T : class
    {
        public AttributeParserConfig(ParserOptions options, IEnumerable<ITerminatingTrigger> triggers)
            : base(options, triggers)
        {
            InitializeVerbs();
            InitializeNamedArguments();
            InitializePostParseMethods();
            InitializePositionalArguments();
            InitializeRequiredNamedArguments();
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

        private void InitializeRequiredNamedArguments()
        {
            RequiredNamedArguments.UnionWith(
                LongNameArguments.Values
                    .Where(a => a.Required)
                    .Select(a => a.Name));
            RequiredNamedArguments.UnionWith(
                ShortNameArguments.Values
                    .Where(a => a.Required)
                    .Select(a => a.Name));
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

        private void InitializeVerbs()
        {
            foreach (var prop in typeof(T).GetProperties()
                .Where(p => p.GetCustomAttributes<VerbAttribute>().Any()))
            {
                foreach (var attr in prop.GetCustomAttributes<VerbAttribute>())
                {
                    var verbName = attr.Name ?? prop.Name;
                    if (verbName.StartsWith(
                        ArgumentPrefix.ToString(CultureInfo.InvariantCulture)))
                    {
                        throw new ArgumentIntegrityException(String.Format(
                            "Verb {0} cannot begin with {1}",
                            verbName, ArgumentPrefix));
                    }
                    if (Verbs.ContainsKey(verbName))
                    {
                        throw new DuplicateArgumentException(verbName);
                    }

                    // TODO allow verb to use other configuration types?
                    var parserConfigType = typeof (AttributeParserConfig<>)
                        .MakeGenericType(new[] {prop.PropertyType});
                    var verbParserConfig = Activator.CreateInstance(
                        parserConfigType, 
                        new object[] {Options, null /* TODO add triggers inside verb configs */}, 
                        null);

                    var verbConfigWrapperType = typeof (VerbParserConfig<>)
                        .MakeGenericType(new[] {prop.PropertyType});
                    var config = (IVerbParserConfig)Activator.CreateInstance(verbConfigWrapperType,
                        new[] {verbParserConfig, new PropertyValueStore(prop), Options});

                    Verbs.Add(verbName, config);
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
