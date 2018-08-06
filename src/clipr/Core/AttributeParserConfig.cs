using System;
using System.Collections.Generic;
using System.Linq;
using clipr.Triggers;
using clipr.Utils;
using clipr.IOC;
using clipr.Usage;
#if NETCORE || NET45
using System.Reflection;
#endif

namespace clipr.Core
{
    internal class AttributeParserConfig<T> : ParserConfig
    {
        // WARNING - if you modify this constructor, make sure to make the appropriate changes
        // to the method AttributeConfigParser.InitializeVerbs, which uses Activator.CreateInstance
        // to create newinstances of this class.
        public AttributeParserConfig(
            RootApplicationMetadata metadata,
            IParserSettings options, 
            IEnumerable<ITerminatingTrigger> triggers)
            : base(metadata, options, triggers)
        {
            InitializeLocalization();
            InitializeApplicationInfo();
            InitializeRootMetadata();
            InitializeVerbs();
            InitializeNamedArguments();
            InitializePostParseMethods();
            InitializePositionalArguments();
            InitializeRequiredNamedArguments();
        }

        private void InitializeLocalization()
        {
            var locAttr = typeof(T).GetCustomAttribute<LocalizeAttribute>();
            if(locAttr != null)
            {
                if(locAttr.ResourceType == null)
                {
                    throw new ArgumentException(
                        "Localization on a class must contain a ResourceType");
                }
                LocalizationInfo = new LocalizationInfo
                {
                    ResourceType = locAttr.ResourceType,
                    ResourceName = locAttr.ResourceName
                };
            }
        }

        private void InitializeApplicationInfo()
        {
            var appInfo = typeof(T).GetCustomAttribute<ApplicationInfoAttribute>();
            if(appInfo != null)
            {
                Name = appInfo.Name;
                Description = appInfo.Description;
            }
        }

        private void InitializeRootMetadata()
        {if (RootMetadata == null)
            {
                Type res = null;
                if(LocalizationInfo != null)
                {
                    res = LocalizationInfo.ResourceType;
                }
                RootMetadata = new RootApplicationMetadata(
                    Name,
                    res,
                    typeof(T));
            }
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
                .Select(p => p.ToPositionalArgument(RootMetadata.ResourceType)));
        }

        private void InitializeNamedArguments()
        {
            var props = typeof (T).GetTypeInfo().GetProperties().Where(p =>
                p.GetCustomAttribute<NamedArgumentAttribute>() != null);
            foreach (var prop in props)
            {
                var arg = prop.ToNamedArgument(RootMetadata.ResourceType);

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
                        Settings.ArgumentPrefix.ToString()))
                    {
                        throw new ArgumentIntegrityException(String.Format(
                            "Verb {0} cannot begin with {1}",
                            verbName, Settings.ArgumentPrefix));
                    }
                    if (Verbs.ContainsKey(verbName))
                    {
                        throw new DuplicateArgumentException(verbName);
                    }

                    Type resourceType = RootMetadata.ResourceType;
                    if(LocalizationInfo != null && LocalizationInfo.ResourceType != null)
                    {
                        resourceType = LocalizationInfo.ResourceType;
                    }

                    // TODO allow verb to use other configuration types?
                    var parserConfigType = typeof (AttributeParserConfig<>)
                        .GetTypeInfo()
                        .MakeGenericType(new[] {prop.PropertyType});
                    var verbParserConfig = Activator.CreateInstance(
                        type:parserConfigType,
                        args:new object[]
                        {
                            RootMetadata,
                            Settings,
                            null /* TODO add triggers inside verb configs */});

                    LocalizationInfo localizationInfo = null;

                    var localizeAttr = prop.GetCustomAttribute<LocalizeAttribute>();
                    if (localizeAttr != null)
                    {
                        localizationInfo = AttributeConverter.GetLocalizationInfo(prop, resourceType);
                    }

                    var verbConfigWrapperType = typeof (VerbParserConfig<>)
                        .GetTypeInfo()
                        .MakeGenericType(new[] {prop.PropertyType});
                    var config = (IVerbParserConfig)Activator.CreateInstance(
                        type:verbConfigWrapperType,
                        args:new[]
                        {
                            RootMetadata,
                            verbName,
                            attr.Description,
                            localizationInfo,
                            verbParserConfig,
                            new PropertyValueStore(prop),
                            Settings,
                            new[] { verbName }
                        });

                    if (Settings.IncludeHelpTriggerInVerbs)
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
