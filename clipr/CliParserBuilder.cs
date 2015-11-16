using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using clipr.Core;
using clipr.Fluent;
using clipr.Triggers;
using clipr.Utils;

namespace clipr
{
    public class CliParserBuilder<TConf> where TConf : class 
    {
        private FluentParserConfig<TConf> FluentConfig { get; set; }

        private ParserOptions Options { get; set; }

        private TConf Object { get; set; }

        public CliParserBuilder(TConf obj)
        {
            FluentConfig = new FluentParserConfig<TConf>(ParserOptions.None, 
                Enumerable.Empty<ITerminatingTrigger>());
            Object = obj;
        } 

        public CliParserBuilder(ParserOptions options, IEnumerable<ITerminatingTrigger> triggers)
        {
            FluentConfig = new FluentParserConfig<TConf>(options, triggers);
            Options = options;
            // Verb's settings object
            // Object must have default constructor
            Object = (TConf)Activator.CreateInstance(typeof(TConf));
        }

        // TODO pass in object

        public CliParser<TConf> Parser
        {
            get
            {
                FluentConfig.ProcessArguments();
                return new CliParser<TConf>(FluentConfig, Object, Options);
            }
        }

        /// <summary>
        /// Configure a named argument with a single value.
        /// </summary>
        /// <typeparam name="TArg">Argument type.</typeparam>
        /// <param name="getExpr">
        /// Getter expression describing where the value is stored.
        /// </param>
        /// <returns></returns>
        public Named<TConf, TArg> HasNamedArgument<TArg>(
            Expression<Func<TConf, TArg>> getExpr)
        {
            var named = new Named<TConf, TArg>(this,
                GetDefinitionFromExpression(getExpr));
            FluentConfig.Add(named);
            return named;
        }

        /// <summary>
        /// Configure a named argument with multiple values.
        /// </summary>
        /// <typeparam name="TArg">Argument type.</typeparam>
        /// <param name="getExpr">
        /// Getter expression describing where the values are stored.
        /// </param>
        /// <returns></returns>
        public NamedList<TConf, TArg> HasNamedArgumentList<TArg>(
            Expression<Func<TConf, TArg>> getExpr)
        {
            var named = new NamedList<TConf, TArg>(this,
                GetDefinitionFromExpression(getExpr));
            FluentConfig.Add(named);
            return named;
        }

        /// <summary>
        /// Configure a positional argument with a single value.
        /// </summary>
        /// <typeparam name="TArg">Argument type.</typeparam>
        /// <param name="getExpr">
        /// Getter expression describing where the value is stored.
        /// </param>
        /// <returns></returns>
        public Positional<TConf, TArg> HasPositionalArgument<TArg>(
            Expression<Func<TConf, TArg>> getExpr)
        {
            var positional = new Positional<TConf, TArg>(this,
                GetDefinitionFromExpression(getExpr));
            FluentConfig.Add(positional);
            return positional;
        }

        /// <summary>
        /// Configure a positional argument with a multiple values.
        /// </summary>
        /// <typeparam name="TArg">Argument type.</typeparam>
        /// <param name="getExpr">
        /// Getter expression describing where the values are stored.
        /// </param>
        /// <returns></returns>
        public PositionalList<TConf, TArg> HasPositionalArgumentList<TArg>(
            Expression<Func<TConf, TArg>> getExpr)
        {
            var positional = new PositionalList<TConf, TArg>(this,
                    GetDefinitionFromExpression(getExpr));
            FluentConfig.Add(positional);
            return positional;
        }

        /// <summary>
        /// Configure a verb containing sub-options.
        /// </summary>
        /// <typeparam name="TArg">Type containing the sub-options.</typeparam>
        /// <param name="verbName">Name of the verb</param>
        /// <param name="expr">Getter for the sub-option object</param>
        /// <param name="subBuilder">A parser configured to parse the sub-options.</param>
        /// <returns></returns>
        public Verb<TConf> HasVerb<TArg>(
            string verbName, Expression<Func<TConf, TArg>> expr, CliParserBuilder<TArg> subBuilder)
            where TArg : class
        {
            ParserConfig<TArg> subConfig;
            if (subBuilder.FluentConfig != null)
            {
                subBuilder.FluentConfig.ProcessArguments();
                subConfig = subBuilder.FluentConfig;
            }
            else
            {
                subConfig = new AttributeParserConfig<TArg>(Options, null /* TODO process triggers in verb */);
            }

            FluentConfig.Verbs.Add(verbName,
                new VerbParserConfig<TArg>(subConfig, GetDefinitionFromExpression(expr), Options));

            return new Verb<TConf>(this);
        }

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

                var setter = declaringType.GetMethod(setMethodName);

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

                var converters = getter.ReturnType.GetCustomAttributes<TypeConverterAttribute>()
                    .Select(a => Activator.CreateInstance(Type.GetType(a.ConverterTypeName)))
                        .OfType<TypeConverter>().ToArray();

                return new IndexerValueStore(name.ToString(), name, getter, setter, converters);
            }

            throw new InvalidOperationException();
        }
    }
}
