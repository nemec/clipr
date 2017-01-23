using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using clipr.Arguments;
using clipr.Triggers;
using clipr.Utils;
using clipr.IOC;

namespace clipr.Core
{
    internal class IntegrityChecker
    {
        /// <summary>
        /// <para>
        /// Iterate through all properties in <typeparamref name="T"/>
        /// and make sure all ArgumentAttributes are defined correctly.
        /// </para>
        /// <para>
        /// All argument integrity checks should be made prior to
        /// parsing so that integrity issues will surface regardless of
        /// arguments provided to the parser.
        /// </para>
        /// </summary>
        /// <exception cref="AggregateException">
        /// Contains all integrity violations found for the given type.
        /// </exception>
        /// <typeparam name="T">
        /// Perform integrity check on this type.
        /// </typeparam>
        internal IEnumerable<Exception> EnsureAttributeIntegrity<T>(ParserOptions options)
        {
            var integrityExceptions = new List<Exception>();

            var properties = typeof(T).GetTypeInfo().GetProperties().Where(
                p => p.GetCustomAttributes<ArgumentAttribute>().Any());
            foreach (var prop in properties)
            {
                ArgumentAttribute attr;

                #region Cannot have multiple ArgumentAttribute subclasses on one property.

                try
                {
                    attr = prop.GetCustomAttribute<ArgumentAttribute>();
                }
                catch (AmbiguousMatchException e)
                {
                    integrityExceptions.Add(new ArgumentIntegrityException(
                        "Cannot provide multiple ArgumentAttributes on a " +
                        "single property.", e));
                    continue;
                }

                #endregion

                // Set property data on argument
                attr.Store = new PropertyValueStore(prop);
                
                integrityExceptions.AddRange(
                    GetIntegrityExceptionsForArgument(attr));
            }

            integrityExceptions.Add(LastPositionalArgumentCanTakeMultipleValuesCheck<T>());
            integrityExceptions.Add(PostParseZeroParametersCheck<T>());
            integrityExceptions.Add(ConfigMayNotContainBothPositionalArgumentsAndVerbs<T>());
            integrityExceptions.Add(ConfigMayNotContainDuplicateArguments<T>(options));

            return integrityExceptions.Where(e => e != null);
        }

        internal IEnumerable<Exception> EnsureVerbIntegrity<T>(IVerbFactory factory)
        {
            var integrityExceptions = new List<Exception>();

            var properties = typeof(T).GetTypeInfo().GetProperties().Where(
                p => p.GetCustomAttributes<VerbAttribute>().Any());
            foreach (var prop in properties)
            {
                integrityExceptions.Add(VerbMustHaveFactoryDefined(prop, factory));
            }

            integrityExceptions.Add(CannotDefineDuplicateVerbs<T>());

            return integrityExceptions.Where(e => e != null);
        }

        internal IEnumerable<Exception> EnsureTriggerIntegrity(IEnumerable<ITerminatingTrigger> triggers)
        {
            var integrityExceptions = new List<Exception>();

            foreach (var trigger in triggers.Where(t => t != null))
            {
                integrityExceptions.AddRange(
                    GetIntegrityExceptionsForArgument(trigger));
            }

            return integrityExceptions.Where(e => e != null);
        }

        private IEnumerable<Exception> GetIntegrityExceptionsForArgument(IArgument attr)
        {
            // TODO check variable type (IEnumerable, IList) with definition of Constraint=Exact & NumArgs=1
            return new[]
                {
                    NumArgsGreaterThanZeroCheck(attr),
                    VarArgsConvertibleToIEnumerableCheck(attr),
                    OptionalConstraintMustIncludeConstIfNonNullable(attr),
                    OptionalConstraintMustBePlacedOnNullableOrReferenceType(attr),
                    ShortNameArgumentMustBeValidCharacter(attr),
                    LongNameArgumentMustBeValidCharacter(attr),
                    PositionalArgumentsCannotStoreConstValues(attr),
                    PositionalArgumentsCannotAppendValues(attr),
                    PositionalArgumentsCannotStoreCount(attr),
                    AppendConvertibleToIEnumerable(attr),
                    CountActionConvertibleToIntCheck(attr),
                    StoreTrueFalseConvertibleToBoolCheck(attr),
                    ConstActionsCannotHaveNullValueCheck(attr),
                    ConvertibleConstValuesCheck(attr)
                }.Where(e => e != null);
        }

        /// <summary>
        /// NumArgs must not equal zero.
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private static Exception NumArgsGreaterThanZeroCheck(IArgument attr)
        {
            if (attr.NumArgs == 0 && attr.Constraint != NumArgsConstraint.AtLeast)
            {
                return new ArgumentIntegrityException(
                    "A NumArgs value of zero may only be the lower bound " +
                    "on the number of arguments (NumArgsConstraint.AtLeast). " +
                    "Any actions that do not require arguments will " +
                    "ignore this property regardless of its value.");
            }
            return null;
        }

        /// <summary>
        /// Arguments that take multiple values must be convertible to IEnumerable.
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private static Exception VarArgsConvertibleToIEnumerableCheck(IArgument attr)
        {
            if (attr.ConsumesMultipleArgs && !attr.Store.Type.IsValidEnumerable())
            {
                return new ArgumentIntegrityException(
                    "Arguments with a variable number of values or " +
                    "more than one required value must be attached " +
                    "to a parameter assignable to IEnumerable.");
            }
            return null;
        }

        private static Exception OptionalConstraintMustIncludeConstIfNonNullable(IArgument attr)
        {
            if (attr.Constraint != NumArgsConstraint.Optional) return null;
            var typ = attr.Store.Type.GetTypeInfo();
            var isNullable = typ.IsValueType && typ.IsGenericType && typ.GetGenericTypeDefinition() == typeof(Nullable<>);
            if(attr.Constraint == NumArgsConstraint.Optional && !isNullable && attr.Const == null)
            {
                return new ArgumentIntegrityException(
                    "Non-nullable arguments with an Optional constraint must include" +
                    "a const value in case value is not provided at runtime.");
            }
            return null;
        }

        private static Exception OptionalConstraintMustBePlacedOnNullableOrReferenceType(IArgument attr)
        {
            if (attr.Constraint != NumArgsConstraint.Optional) return null;
            var typ = attr.Store.Type.GetTypeInfo();
            if(typ.IsValueType && typ.IsGenericType && typ.GetGenericTypeDefinition() != typeof(Nullable<>))
            {
                return new ArgumentIntegrityException(
                    "Optional constraint must be placed on a nullable or reference type.");
            }
            return null;
        }

        private static Exception ShortNameArgumentMustBeValidCharacter(IArgument arg)
        {
            var sh = arg as IShortNameArgument;
            if (sh != null
                && sh.ShortName.HasValue
                && !ArgumentValidation.IsAllowedShortName(sh.ShortName.Value))
            {
                return new ArgumentIntegrityException(String.Format(
                    "Invalid argument {0}: {1}", sh.ShortName,
                    ArgumentValidation.IsAllowedShortNameExplanation));
            }
            return null;
        }

        private static Exception LongNameArgumentMustBeValidCharacter(IArgument arg)
        {
            var sh = arg as ILongNameArgument;
            if (sh != null
                && sh.LongName != null
                && !ArgumentValidation.IsAllowedLongName(sh.LongName))
            {
                return new ArgumentIntegrityException(String.Format(
                    "Invalid argument {0}: {1}", sh.LongName,
                    ArgumentValidation.IsAllowedLongNameExplanation));
            }
            return null;
        }

        /// <summary>
        /// Positional arguments cannot store const values.
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private static Exception PositionalArgumentsCannotStoreConstValues(IArgument attr)
        {
            if (attr is PositionalArgumentAttribute &&
                    (attr.Action == ParseAction.StoreConst ||
                    attr.Action == ParseAction.AppendConst))
            {
                return new ArgumentIntegrityException(
                    String.Format("Positional argument {0} cannot store " +
                        "const values because the argument will always " +
                        "be required.", attr.Name));
            }
            return null;
        }

        /// <summary>
        /// Positional arguments cannot append values.
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private static Exception PositionalArgumentsCannotAppendValues(IArgument attr)
        {
            if (attr is IPositionalArgument &&
                    attr.Action == ParseAction.Append)
            {
                return new ArgumentIntegrityException(
                    String.Format("Positional argument {0} cannot " +
                    @"""append"" values.", attr.Name));
            }
            return null;
        }

        /// <summary>
        /// Positional arguments cannot store a count.
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private static Exception PositionalArgumentsCannotStoreCount(IArgument attr)
        {
            if (attr is IPositionalArgument &&
                    attr.Action == ParseAction.Count)
            {
                return new ArgumentIntegrityException(
                    String.Format("Positional argument {0} cannot store " +
                        "a count.", attr.Name));
            }
            return null;
        }

        /// <summary>
        /// Append and AppendConst actions must be convertible to IEnumerable{T}.
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private static Exception AppendConvertibleToIEnumerable(IArgument attr)
        {
            if ((attr.Action == ParseAction.Append ||
                    attr.Action == ParseAction.AppendConst) &&
                    !attr.Store.Type.IsValidEnumerable())
            {
                return new ArgumentIntegrityException(
                    "Arguments with a ParseAction of 'Append' " +
                    "or 'AppendConst' must be attached to a " +
                    "parameter assignable to IEnumerable<>.");
            }
            return null;
        }

        /// <summary>
        /// Count actions must be convertible to int.
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private static Exception CountActionConvertibleToIntCheck(IArgument attr)
        {
            if (attr.Action == ParseAction.Count && !attr.Store.Type.IsValid<int>())
            {
                return new ArgumentIntegrityException(
                    "Arguments with a ParseAction of 'Count' must be " +
                    "attached to a property with a type assignable to int.");
            }
            return null;
        }

        /// <summary>
        /// StoreTrue and StoreFalse actions must be convertible to bool.
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private static Exception StoreTrueFalseConvertibleToBoolCheck(IArgument attr)
        {
            if ((attr.Action == ParseAction.StoreTrue ||
                     attr.Action == ParseAction.StoreFalse) &&
                    !attr.Store.Type.IsValid<bool>())
            {
                return new ArgumentIntegrityException(
                            String.Format("Argument {0} with a" +
                                "ParseAction of 'StoreTrue' or " +
                            "'StoreFalse' must be attached to a property with " +
                            "a type assignable to bool.",
                            attr.Name));
            }
            return null;
        }

        /// <summary>
        /// Const actions cannot have null value.
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private static Exception ConstActionsCannotHaveNullValueCheck(IArgument attr)
        {
            if ((attr.Action == ParseAction.StoreConst ||
                     attr.Action == ParseAction.AppendConst) &&
                    attr.Const == attr.Store.Type.GetDefaultValue())
            {
                return new ArgumentIntegrityException(
                    String.Format("Argument {0} with a const action " +
                        "must be provided with a non-null Const value.",
                        attr.Name));
            }
            return null;
        }

        /// <summary>
        /// Const values must be convertible to the property type.
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private static Exception ConvertibleConstValuesCheck(IArgument attr)
        {
            if ((attr.Action == ParseAction.StoreConst &&
                        !attr.Store.Type.ValueIsConvertible(attr.Const)) ||
                    (attr.Action == ParseAction.AppendConst &&
                        !attr.Store.Type.ValueIsConvertibleGeneric(attr.Const)))
            {
                return new ArgumentIntegrityException(
                    String.Format("Argument {0} with a const action " +
                        "must be convertible to the property's type.",
                        attr.Name));
            }
            return null;
        }

        /// <summary>
        /// Only last positional argument may take multiple values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static Exception LastPositionalArgumentCanTakeMultipleValuesCheck<T>()
        {
            var props = typeof(T).GetTypeInfo().GetProperties()
                .Where(p => p.GetCustomAttribute<PositionalArgumentAttribute>() != null)
                .OrderBy(p => p.GetCustomAttribute<PositionalArgumentAttribute>().Index)
                .ToList();
            foreach (var prop in props.Take(props.Count - 1))
            {
                var attr = prop.GetCustomAttribute<PositionalArgumentAttribute>();
                if (attr.HasVariableNumArgs)
                {
                    return new ArgumentIntegrityException(
                        String.Format("Positional argument {0} is not the " +
                            "last positional argument so it must take " +
                            "a constant number of values.", prop.Name));
                }
            }
            return null;
        }

        /// <summary>
        /// Configuration objects may not use both positional parameters
        /// and verbs in the same object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static Exception ConfigMayNotContainBothPositionalArgumentsAndVerbs<T>()
        {
            var positionalCount = typeof(T)
                .GetTypeInfo()
                .GetProperties()
                .Count(p => p.GetCustomAttribute<PositionalArgumentAttribute>() != null);
            var verbCount = typeof(T)
                .GetTypeInfo()
                .GetProperties()
                .Count(p => p.GetCustomAttributes<VerbAttribute>().Any());
            if (positionalCount > 0 && verbCount > 0)
            {
                return new ArgumentIntegrityException(
                    "Configuration object may not contain both " +
                    "Positional arguments and Verbs.");
            }
            return null;
        }

        private static Exception ConfigMayNotContainDuplicateArguments<T>(ParserOptions options)
        {
            var named = typeof(T)
                .GetTypeInfo()
                .GetProperties()
                .Select(p => p.GetCustomAttribute<NamedArgumentAttribute>())
                .Where(a => a != null);
            var dupes = named
                .Where(k => k.ShortName.HasValue)
                .GroupBy(k => options.HasFlag(ParserOptions.CaseInsensitive)
                    ? k.ShortName.ToString().ToLowerInvariant()
                    : k.ShortName.ToString())
                .Where(a => a.Count() > 1)
                .Select(a => a.Key);
            var dupel = named
                .Where(k => k.LongName != null)
                .GroupBy(k => options.HasFlag(ParserOptions.CaseInsensitive) 
                    ? k.LongName.ToLowerInvariant()
                    : k.LongName)
                .Where(a => a.Count() > 1)
                .Select(a => a.Key);

            var errs = new List<Exception>();
            foreach(var dupe in dupes)
            {
                errs.Add(new DuplicateArgumentException(dupe));
            }
            foreach(var dupe in dupel)
            {
                errs.Add(new DuplicateArgumentException(dupe));
            }
            if(errs.Count == 0)
            {
                return null;
            }
            if(errs.Count == 1)
            {
                return errs[0];
            }
            return new Utils.AggregateException(errs);
        }

        private static Exception VerbMustHaveFactoryDefined(PropertyInfo verbProp, IVerbFactory factory)
        {
            if (!factory.CanCreateVerb(verbProp.PropertyType))
            {
                return new ArgumentIntegrityException(String.Format(
                        "Verb '{0}' has no default constructor or factory defined for its type. Use a custom IVerbFactory for this parser.",
                        verbProp.Name));
            }
            return null;
        }

        private static Exception CannotDefineDuplicateVerbs<T>()
        {
            var verbs = typeof(T)
                .GetTypeInfo()
                .GetProperties()
                .SelectMany(p => 
                    p.GetCustomAttributes<VerbAttribute>().Select(a => new
                    {
                        Prop = p,
                        Attr = a
                    }));
            var checkedVerbNames = new HashSet<string>();
            foreach (var verb in verbs)
            {
                // TODO de-duplicate this Verb Name generation logic
                var name = verb.Attr.Name ?? verb.Prop.Name.ToLowerInvariant();
                if (!checkedVerbNames.Add(name))
                {
                    return new DuplicateVerbException(name);
                }
            }
            return null;
        }

        /// <summary>
        /// PostParse methods must take zero parameters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static Exception PostParseZeroParametersCheck<T>()
        {
            var invalidPostParseMethods = typeof(T).GetTypeInfo().GetMethods()
                .Where(m => m.GetCustomAttribute<PostParseAttribute>() != null)
                .Where(m => m.GetParameters().Length != 0);
            foreach (var method in invalidPostParseMethods)
            {
                return new ArgumentIntegrityException(
                    String.Format("The PostParse method {0} must have " +
                                  "zero parameters.", method.Name));
            }
            return null;
        }
    }
}
