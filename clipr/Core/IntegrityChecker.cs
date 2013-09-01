using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using clipr.Arguments;
using clipr.Triggers;
using clipr.Utils;

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
        internal void EnsureAttributeIntegrity<T>()
        {
            var integrityExceptions = new List<Exception>();

            var properties = typeof (T).GetProperties().Where(
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

                integrityExceptions.AddRange(
                    GetIntegrityExceptionsForArgument(prop, attr));
            }

            integrityExceptions.Add(LastPositionalArgumentCanTakeMultipleValuesCheck<T>());
            integrityExceptions.Add(PostParseZeroParametersCheck<T>());
            integrityExceptions.Add(ConfigMayNotContainBothPositionalArgumentsAndVerbs<T>());

            integrityExceptions = integrityExceptions
                .Where(e => e != null).ToList();
            if (integrityExceptions.Any())
            {
                throw new AggregateException(integrityExceptions);
            }
        }

        internal void EnsureTriggerIntegrity<TConf>(IEnumerable<ITrigger<TConf>> triggers)
            where TConf : class 
        {
            var integrityExceptions = new List<Exception>();

            foreach (var trigger in triggers.Where(t => t != null))
            {
                integrityExceptions.AddRange(
                    GetIntegrityExceptionsForArgument(null, trigger));
            }

            integrityExceptions = integrityExceptions
                .Where(e => e != null).ToList();
            if (integrityExceptions.Any())
            {
                throw new AggregateException(integrityExceptions);
            }
        }

        private IEnumerable<Exception> GetIntegrityExceptionsForArgument(PropertyInfo prop, IArgument attr)
        {
            return new[]
                {
                    NumArgsGreaterThanZeroCheck(attr),
                    VarArgsConvertibleToIEnumerableCheck(prop, attr),
                    ShortNameArgumentMustBeValidCharacter(attr),
                    LongNameArgumentMustBeValidCharacter(attr),
                    PositionalArgumentsCannotStoreConstValues(prop, attr),
                    PositionalArgumentsCannotAppendValues(prop, attr),
                    PositionalArgumentsCannotStoreCount(prop, attr),
                    AppendConvertibleToIEnumerable(prop, attr),
                    CountActionConvertibleToIntCheck(prop, attr),
                    StoreTrueFalseConvertibleToBoolCheck(prop, attr),
                    ConstActionsCannotHaveNullValueCheck(prop, attr),
                    ConvertibleConstValuesCheck(prop, attr)
                }.Where(e => e != null);
        }

        /// <summary>
        /// NumArgs must not equal zero.
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private Exception NumArgsGreaterThanZeroCheck(IArgument attr)
        {
            if (attr.NumArgs == 0)
            {
                return new ArgumentIntegrityException(
                    "Do not define an argument count less than 1. " +
                    "Any actions that do not require arguments will " +
                    "ignore this property regardless of count.");
            }
            return null;
        }

        /// <summary>
        /// Arguments that take multiple values must be convertible to IEnumerable.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="attr"></param>
        /// <returns></returns>
        private Exception VarArgsConvertibleToIEnumerableCheck(
            PropertyInfo prop, IArgument attr)
        {
            if (attr.ConsumesMultipleArgs && !prop.IsValidIEnumerable())
            {
                return new ArgumentIntegrityException(
                    "Arguments with a variable number of values or " +
                    "more than one required value must be attached " +
                    "to a parameter assignable to IEnumerable.");
            }
            return null;
        }

        private Exception ShortNameArgumentMustBeValidCharacter(IArgument arg)
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

        private Exception LongNameArgumentMustBeValidCharacter(IArgument arg)
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
        /// <param name="prop"></param>
        /// <param name="attr"></param>
        /// <returns></returns>
        private Exception PositionalArgumentsCannotStoreConstValues(
            PropertyInfo prop, IArgument attr)
        {
            if (attr is PositionalArgumentAttribute &&
                    (attr.Action == ParseAction.StoreConst ||
                    attr.Action == ParseAction.AppendConst))
            {
                return new ArgumentIntegrityException(
                    String.Format("Positional argument {0} cannot store " +
                        "const values because the argument will always " +
                        "be required.", prop.Name));
            }
            return null;
        }

        /// <summary>
        /// Positional arguments cannot append values.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="attr"></param>
        /// <returns></returns>
        private Exception PositionalArgumentsCannotAppendValues(
            PropertyInfo prop, IArgument attr)
        {
            if (attr is PositionalArgumentAttribute &&
                    attr.Action == ParseAction.Append)
            {
                return new ArgumentIntegrityException(
                    String.Format("Positional argument {0} cannot " +
                    @"""append"" values.", prop.Name));
            }
            return null;
        }

        /// <summary>
        /// Positional arguments cannot store a count.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="attr"></param>
        /// <returns></returns>
        private Exception PositionalArgumentsCannotStoreCount(
            PropertyInfo prop, IArgument attr)
        {
            if (attr is PositionalArgumentAttribute &&
                    attr.Action == ParseAction.Count)
            {
                return new ArgumentIntegrityException(
                    String.Format("Positional argument {0} cannot store " +
                        "a count.", prop.Name));
            }
            return null;
        }

        /// <summary>
        /// Append and AppendConst actions must be convertible to IEnumerable{T}.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="attr"></param>
        /// <returns></returns>
        private Exception AppendConvertibleToIEnumerable(
            PropertyInfo prop, IArgument attr)
        {
            if ((attr.Action == ParseAction.Append ||
                    attr.Action == ParseAction.AppendConst) &&
                    !prop.IsValidIEnumerable())
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
        /// <param name="prop"></param>
        /// <param name="attr"></param>
        /// <returns></returns>
        private Exception CountActionConvertibleToIntCheck(
            PropertyInfo prop, IArgument attr)
        {
            if (attr.Action == ParseAction.Count && !prop.IsValid<int>())
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
        /// <param name="prop"></param>
        /// <param name="attr"></param>
        /// <returns></returns>
        private Exception StoreTrueFalseConvertibleToBoolCheck(
            PropertyInfo prop, IArgument attr)
        {
            if ((attr.Action == ParseAction.StoreTrue ||
                     attr.Action == ParseAction.StoreFalse) &&
                    !prop.IsValid<bool>())
            {
                return new ArgumentIntegrityException(
                            String.Format("Argument {0} with a" +
                                "ParseAction of 'StoreTrue' or " +
                            "'StoreFalse' must be attached to a property with " +
                            "a type assignable to bool.",
                            prop.Name));
            }
            return null;
        }

        /// <summary>
        /// Const actions cannot have null value.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="attr"></param>
        /// <returns></returns>
        private Exception ConstActionsCannotHaveNullValueCheck(
            PropertyInfo prop, IArgument attr)
        {
            if ((attr.Action == ParseAction.StoreConst ||
                     attr.Action == ParseAction.AppendConst) &&
                    attr.Const == prop.GetDefaultValue())
            {
                return new ArgumentIntegrityException(
                    String.Format("Argument {0} with a const action " +
                        "must be provided with a non-null Const value.",
                        prop.Name));
            }
            return null;
        }

        /// <summary>
        /// Const values must be convertible to the property type.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="attr"></param>
        /// <returns></returns>
        private Exception ConvertibleConstValuesCheck(
            PropertyInfo prop, IArgument attr)
        {
            if ((attr.Action == ParseAction.StoreConst &&
                        !prop.ValueIsConvertible(attr.Const)) ||
                    (attr.Action == ParseAction.AppendConst &&
                        !prop.ValueIsConvertibleGeneric(attr.Const)))
            {
                return new ArgumentIntegrityException(
                    String.Format("Argument {0} with a const action " +
                        "must be convertible to the property's type.",
                        prop.Name));
            }
            return null;
        }

        /// <summary>
        /// Only last positional argument may take multiple values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private Exception LastPositionalArgumentCanTakeMultipleValuesCheck<T>()
        {
            var props = typeof(T).GetProperties()
                .Where(p => p.GetCustomAttribute<PositionalArgumentAttribute>() != null)
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
            var positionalCount = typeof (T)
                .GetProperties()
                .Count(p => p.GetCustomAttribute<PositionalArgumentAttribute>() != null);
            var verbCount = typeof (T)
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

        /// <summary>
        /// PostParse methods must take zero parameters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private Exception PostParseZeroParametersCheck<T>()
        {
            var invalidPostParseMethods = typeof(T).GetMethods()
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
