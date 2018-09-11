using clipr.Arguments;
using clipr.Core;
using clipr.Fluent;
using clipr.IOC;
using clipr.Triggers;
using clipr.Usage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
#if NET40
using clipr.Utils;
#endif

namespace clipr
{
    public class CliParserBuilder<TConf> where TConf : class
    {
        private readonly ParserSettings<TConf> _settings;

        private readonly List<NamedArgument> _namedArguments;

        private readonly List<PositionalArgument> _positionalArguments;

        private readonly List<MethodInfo> _postParseMethods;

        private readonly Dictionary<string, IVerbParserConfig> _verbs;

        private readonly string[] _precursorVerbs;

        private int _nextPositionalIndex;

        private bool _hasVariablePositionalList;

        private Type _defaultResourceType;

        private string _applicationName;

        private string _applicationDescription;

        private CliParserBuilder(string[] precursorVerbs)
            : this()
        {
            _precursorVerbs = precursorVerbs;
        }

        public CliParserBuilder()
        {
            _settings = new ParserSettings<TConf>();
            _namedArguments = new List<NamedArgument>();
            _positionalArguments = new List<PositionalArgument>();
            _postParseMethods = new List<MethodInfo>();
            _verbs = new Dictionary<string, IVerbParserConfig>();
            _defaultResourceType = null;
            _nextPositionalIndex = 0;
            _hasVariablePositionalList = false;
            _applicationName = null;
            _applicationDescription = null;
        }

        public CliParserBuilder<TConf> ConfigureSettings(Action<ParserSettings<TConf>> settings)
        {
            settings(_settings);
            return this;
        }

        #region Settings-specific actions

        /// <summary>
        /// Punctuation character prefixed to short and long argument
        /// names. Usually a hyphen (-).
        /// </summary>
        /// <exception cref="ArgumentIntegrityException">
        /// Character is not valid punctuation.
        /// </exception>
        public CliParserBuilder<TConf> WithArgumentPrefix(char prefix)
        {
            _settings.ArgumentPrefix = prefix;
            return this;
        }

        public CliParserBuilder<TConf> UseCaseInsensitiveParsing()
        {
            _settings.CaseInsensitive = true;
            return this;
        }

        /// <summary>
        /// Partially match unambiguous long named arguments.
        /// E.g. "--che" will match the named argument "checkout" as
        /// long as no other named argument begins with "che".
        /// </summary>
        /// <returns></returns>
        public CliParserBuilder<TConf> MatchOnPartialArgumentNames()
        {
            _settings.NamedPartialMatch = true;
            return this;
        }

        public CliParserBuilder<TConf> UseCustomHelpGenerator(IHelpGenerator help)
        {
            _settings.HelpGenerator = help;
            return this;
        }

        public CliParserBuilder<TConf> DisableHelpGenerator()
        {
            _settings.HelpGenerator = null;
            return this;
        }

        public CliParserBuilder<TConf> UseCustomVersionGenerator(IVersion version)
        {
            _settings.VersionGenerator = version;
            return this;
        }

        public CliParserBuilder<TConf> DisableVersionGenerator()
        {
            _settings.VersionGenerator = null;
            return this;
        }


        /// <summary>
        /// Add an additional trigger beyond Help and Version 
        /// that will be used when parsing
        /// </summary>
        public CliParserBuilder<TConf> AddCustomTrigger(ITerminatingTrigger trigger)
        {
            _settings.CustomTriggers.Add(trigger);
            return this;
        }

        public CliParserBuilder<TConf> DisableHelpArgumentInVerbs()
        {
            _settings.IncludeHelpTriggerInVerbs = false;
            return this;
        }

        /// <summary>
        /// An event that is fired every time an argument is parsed, just
        /// after it is added to the Options object. If you would like to
        /// stop parsing in response to an event, return a non-null
        /// ITerminating trigger from the event handler. To continue
        /// parsing, simply return null.
        /// </summary>
        public CliParserBuilder<TConf> ExecuteHandlerOnParsedArgument(
            Func<ParseEventArgs, ITerminatingTrigger> handler)
        {
            _settings.OnParseArgument = handler;
            return this;
        }

        /// <summary>
        /// The text writer where extension or trigger output should
        /// be written.
        /// </summary>
        public CliParserBuilder<TConf> SetOutputWriter(TextWriter writer)
        {
            _settings.OutputWriter = writer;
            return this;
        }

        public CliParserBuilder<TConf> SetVerbFactory(IObjectFactory factory)
        {
            _settings.VerbFactory = factory;
            return this;
        }

        #endregion

        public CliParserBuilder<TConf> WithApplicationName(string applicationName)
        {
            _applicationName = applicationName;
            return this;
        }

        public CliParserBuilder<TConf> WithApplicationDescription(string applicationDescription)
        {
            _applicationDescription = applicationDescription;
            return this;
        }

        /// <summary>
        /// Set the default localization resource type for all arguments
        /// in this parser. This can be overridden for each individual
        /// argument, if needed. If no resource name is provided for
        /// an argument, it will default to ClassName_PropertyName,
        /// where ClassName is the options type name and PropertyName
        /// is the name of the class member holding the parsed value.
        /// For example, 'Options_Name' for a class named Options and
        /// the Name property.
        /// </summary>
        /// <param name="resourceType"></param>
        /// <returns></returns>
        public CliParserBuilder<TConf> Localize(Type resourceType)
        {
            _defaultResourceType = resourceType;
            return this;
        }

        public NamedArgumentBuilder<TArg> AddNamedArgument<TArg>(Expression<Func<TConf, TArg>> getExpr)
        {
            var arg = new NamedArgument(
                GetDefinitionFromExpression(getExpr));
            _namedArguments.Add(arg);

            return new NamedArgumentBuilder<TArg>(
                arg, _defaultResourceType, GetDefaultResourceName(arg));
        }

        public NamedArgumentListBuilder<TArg> AddNamedArgumentList<TArg>(
            Expression<Func<TConf, IEnumerable<TArg>>> getExpr)
        {
            var arg = new NamedArgument(
                GetDefinitionFromExpression(getExpr));
            _namedArguments.Add(arg);
            return new NamedArgumentListBuilder<TArg>(
                arg, _defaultResourceType, GetDefaultResourceName(arg));
        }

        public PositionalArgumentBuilder<TArg> AddPositionalArgument<TArg>(
            Expression<Func<TConf, TArg>> getExpr)
        {
            var idx = _nextPositionalIndex;
            _nextPositionalIndex++;
            var arg = new PositionalArgument(
                GetDefinitionFromExpression(getExpr))
            {
                Index = idx
            };
            _positionalArguments.Add(arg);
            return new PositionalArgumentBuilder<TArg>(
                arg, _defaultResourceType, GetDefaultResourceName(arg));
        }

        public PositionalArgumentListBuilder<TArg> AddPositionalArgumentList<TArg>(
            Expression<Func<TConf, IEnumerable<TArg>>> getExpr)
        {
            var idx = _nextPositionalIndex;
            _nextPositionalIndex++;
            var arg = new PositionalArgument(
                GetDefinitionFromExpression(getExpr))
            {
                Index = idx
            };
            _positionalArguments.Add(arg);
            var argb = new PositionalArgumentListBuilder<TArg>(
                arg, _defaultResourceType, GetDefaultResourceName(arg));
            if (arg.ConsumesMultipleArgs)
            {
                if (_hasVariablePositionalList)
                {
                    throw new ArgumentIntegrityException(
                        "Only one positional argument is allowed to consume multiple values.");
                }
                _hasVariablePositionalList = true;
            }
            return argb;
        }

        public void AddVerb<TArg>(string verbName, Expression<Func<TConf, TArg>> getExpr, Action<CliParserBuilder<TArg>> verbBuilder)
            where TArg : class
        {
            CliParserBuilder<TArg> vb;
            if (_precursorVerbs != null)
            {
                // Precursor verbs allow you to trace the 'lineage' of
                // a nested verb chain, which is helpful when displaying
                // a complete example in the help output.
                var len = _precursorVerbs.Length;
                var prec = new string[len + 1];
                Array.Copy(_precursorVerbs, prec, len);
                prec[len] = verbName;
                vb = new CliParserBuilder<TArg>(prec);
            }
            else
            {
                vb = new CliParserBuilder<TArg>();
            }

            verbBuilder(vb);
            var parser = vb.BuildParser();
            var verbConfig = parser.BuildConfig();

            var md = new RootApplicationMetadata(_applicationName, _defaultResourceType, typeof(TConf));
            var vpc = new VerbParserConfig<TArg>(
                    md,
                    verbName,
                    null,  // TODO add verb description
                    null,
                    verbConfig,
                    GetDefinitionFromExpression(getExpr),
                    _settings,
                    _precursorVerbs);
            _verbs.Add(verbName, vpc);
        }

        public void AddPostParseMethod(Action method)
        {
            if(method.Target == null || method.Target.GetType() != typeof(TConf))
            {
                throw new NotImplementedException(
                    "Only instance methods on the option type are currently supported as post-parse methods.");
            }
            // TODO allow arbitrary actions
            _postParseMethods.Add(method.Method);
        }

        public CliParser<TConf> BuildParser()
        {
            var triggers = _settings.CustomTriggers
                    .Concat(new ITerminatingTrigger[] { _settings.HelpGenerator, _settings.VersionGenerator });
            var md = new RootApplicationMetadata(_applicationName, _defaultResourceType, typeof(TConf));
            var conf = new FluentParserConfig<TConf>(
                md,
                _applicationDescription,
                _settings, 
                triggers);
            foreach (var arg in _namedArguments)
            {
                if (arg.ShortName.HasValue)
                {
                    conf.ShortNameArguments.Add(arg.ShortName.Value, arg);
                }
                if (arg.LongName != null)
                {
                    conf.LongNameArguments.Add(arg.LongName, arg);
                }
                if (arg.Required)
                {
                    conf.RequiredNamedArguments.Add(arg.Name);
                }
            }

            conf.PositionalArguments.AddRange(_positionalArguments);

            foreach (var verb in _verbs)
            {
                conf.Verbs.Add(verb.Key, verb.Value);
            }

            conf.PostParseMethods.AddRange(_postParseMethods);

            return new CliParser<TConf>(conf, _settings);
        }

        /// <summary>
        /// Turns a getter/setter expression into a ValueStore, allowing
        /// us to assign a parsed value to the appropriate option parameter.
        /// </summary>
        /// <typeparam name="TArg"></typeparam>
        /// <param name="getExpr"></param>
        /// <returns></returns>
        private static IValueStoreDefinition GetDefinitionFromExpression<TArg>(
            Expression<Func<TConf, TArg>> getExpr)
        {
            var body = getExpr.Body as MemberExpression;
            if (body != null && body.NodeType == ExpressionType.MemberAccess)
            {
                var prop = (PropertyInfo)(body).Member;
                return new PropertyValueStore(prop);
            }

            var methodBody = getExpr.Body as MethodCallExpression;
            if (methodBody != null && methodBody.NodeType == ExpressionType.Call)
            {
                var getter = methodBody.Method;

                // Special case for Indexers. Method must be a "special name"
                // and begin with "get_".
                if (!getter.Name.StartsWith("get_") && getter.IsSpecialName)
                {
                    throw new InvalidOperationException(
                        "The only method call expressions allowed are indexer-gets.");
                }

                var setMethodName = "set" + getter.Name.Substring("get".Length);

                var declaringType = getter.DeclaringType;
                if (declaringType == null)
                {
                    throw new InvalidOperationException(String.Format(
                        "Cannot find declaring type for getter {0}.",
                        getter.Name));
                }

                var setter = declaringType.GetTypeInfo().GetMethod(setMethodName);

                object name;
                var arg = methodBody.Arguments[0];
                if (arg is ConstantExpression)
                {
                    // Try to grab the value of the first argument.
                    // Works best for dict indexers (dict["key"]).
                    name = (arg as ConstantExpression).Value;
                }
                else
                {
                    // Go nuclear. Evaluate the first argument (even
                    // if it contains method calls) and assign the name
                    // to the argument's string value.
                    name = Expression
                        .Lambda(arg)
                        .Compile()
                        .DynamicInvoke();
                }

                var converters = getter.ReturnType.GetTypeInfo().GetCustomAttributes<TypeConverterAttribute>()
                    .Select(a => Activator.CreateInstance(Type.GetType(a.ConverterTypeName)))
                        .OfType<TypeConverter>().ToArray();

                return new IndexerValueStore(name.ToString(), name, getter, setter, converters);
            }

            throw new InvalidOperationException();
        }

        private string GetDefaultResourceName(IArgument arg)
        {
            return String.Format("{0}_{1}", typeof(TConf).Name, arg.Store.Name);
        }
    }
}
