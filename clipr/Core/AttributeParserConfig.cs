using System;
using System.Collections.Generic;
using System.Linq;
using clipr.Triggers;
using clipr.Utils;
using clipr.IOC;
using clipr.Usage;
#if NETCORE
using System.Reflection;
#endif

namespace clipr.Core
{
    internal class AttributeParserConfig<T> : ParserConfig<T> where T : class
    {
        public AttributeParserConfig(ParserOptions options, IEnumerable<ITerminatingTrigger> triggers, IVerbFactory factory)
            : base(options, triggers, factory)
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
                typeof(T).GetTypeInfo().GetProperties()
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
            PositionalArguments.AddRange(typeof(T).GetTypeInfo().GetProperties()
                .Where(p => p.GetCustomAttribute<PositionalArgumentAttribute>() != null)
                .OrderBy(p => p.GetCustomAttribute<PositionalArgumentAttribute>().Index)
                .Select(p => p.ToPositionalArgument()));
        }

        private void InitializeNamedArguments()
        {
            var props = typeof (T).GetTypeInfo().GetProperties().Where(p =>
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
            foreach (var prop in typeof(T).GetTypeInfo().GetProperties()
                .Where(p => p.GetCustomAttributes<VerbAttribute>().Any()))
            {
                foreach (var attr in prop.GetCustomAttributes<VerbAttribute>())
                {
                    // TODO deduplicate this Verb Name generation logic
                    var verbName = attr.Name ?? prop.Name.ToLowerInvariant();
                    if (verbName.StartsWith(
                        ArgumentPrefix.ToString()))
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
                        .GetTypeInfo()
                        .MakeGenericType(new[] {prop.PropertyType});
                    var verbParserConfig = Activator.CreateInstance(
                        type:parserConfigType,
                        args:new object[] {Options, null /* TODO add triggers inside verb configs */, VerbFactory});

                    var verbConfigWrapperType = typeof (VerbParserConfig<>)
                        .GetTypeInfo()
                        .MakeGenericType(new[] {prop.PropertyType});
                    var config = (IVerbParserConfig)Activator.CreateInstance(
                        type:verbConfigWrapperType,
                        args:new[] {verbParserConfig, new PropertyValueStore(prop), Options, VerbFactory, new[] { verbName } });

                    config.Name = verbName;
                    config.Description = attr.Description;
                    if (Options.IncludeHelpTriggerInVerbs)
                    {
                        var helpWrapperType = typeof(AutomaticHelpGenerator<>)
                            .GetTypeInfo()
                            .MakeGenericType(new[] { prop.PropertyType });
                        var triggers = new[] { (IHelpGenerator)Activator.CreateInstance(
                            type:helpWrapperType,
                            args:new object[] { }) };
                        config.AppendTriggers(triggers);
                    }

                    // TODO optimize this a bit
                    foreach(var innerVerb in config.Verbs.Values)
                    {
                        innerVerb.PrecursorVerbs = Enumerable.Repeat(verbName, 1).Concat(innerVerb.PrecursorVerbs).ToArray();
                    }

                    Verbs.Add(verbName, config);
                }
            }
        }

        private void InitializePostParseMethods()
        {
            PostParseMethods.AddRange(typeof(T).GetTypeInfo().GetMethods()
                .Where(p => p.GetCustomAttribute<PostParseAttribute>() != null));
        }
    }
}
