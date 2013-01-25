using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using clipr.Annotations;

namespace clipr
{
    internal static class AttributeIntegrityChecker
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
        internal static void EnsureAttributeIntegrity<T>()
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

                integrityExceptions.AddRange(new[]
                {
                    NumArgsGreaterThanZeroCheck(attr),
                    VarArgsConvertibleToIListCheck(prop, attr),
                    PositionalArgumentsCannotStoreConstValues(prop, attr),
                    PositionalArgumentsCannotAppendValues(prop, attr),
                    PositionalArgumentsCannotStoreCount(prop, attr),
                    AppendConvertibleToIList(prop, attr),
                    CountActionConvertibleToIntCheck(prop, attr),
                    StoreTrueFalseConvertibleToBoolCheck(prop, attr),
                    ConstActionsCannotHaveNullValueCheck(prop, attr),
                    ConvertibleConstValuesCheck(prop, attr)
                }.Where(e => e != null));
            }

            integrityExceptions.Add(LastPositionalArgumentCanTakeMultipleValuesCheck<T>());
            integrityExceptions.Add(PostParseZeroParametersCheck<T>());

            integrityExceptions = integrityExceptions
                .Where(e => e != null).ToList();
            if (integrityExceptions.Any())
            {
                throw new AggregateException(integrityExceptions);
            }
        }

        /// <summary>
        /// NumArgs must not equal zero.
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        private static Exception NumArgsGreaterThanZeroCheck(ArgumentAttribute attr)
        {
            if (attr.NumArgs == 0)
            {
                return new ArgumentIntegrityException(
                    "Do not define an argument count less than 1. " +
                    "Any actions that do not require arguments will " +
                    "ignore this property.");
            }
            return null;
        }

        /// <summary>
        /// Arguments that take multiple values must be convertible to IList.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="attr"></param>
        /// <returns></returns>
        private static Exception VarArgsConvertibleToIListCheck(
            PropertyInfo prop, ArgumentAttribute attr)
        {
            if (attr.ConsumesMultipleArgs && !prop.IsValidIList())
            {
                return new ArgumentIntegrityException(
                    "Arguments with a variable number of values or " +
                    "more than one required value must be attached " +
                    "to a parameter assignable to IList.");
            }
            return null;
        }

    /// <summary>
        /// Positional arguments cannot store const values.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="attr"></param>
        /// <returns></returns>
        private static Exception PositionalArgumentsCannotStoreConstValues(
            PropertyInfo prop, ArgumentAttribute attr)
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
        private static Exception PositionalArgumentsCannotAppendValues(
            PropertyInfo prop, ArgumentAttribute attr)
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
        private static Exception PositionalArgumentsCannotStoreCount(
            PropertyInfo prop, ArgumentAttribute attr)
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
        /// Append and AppendConst actions must be convertible to IList.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="attr"></param>
        /// <returns></returns>
        private static Exception AppendConvertibleToIList(
            PropertyInfo prop, ArgumentAttribute attr)
        {
            if ((attr.Action == ParseAction.Append ||
                    attr.Action == ParseAction.AppendConst) &&
                    !prop.IsValidIList())
            {
                return new ArgumentIntegrityException(
                    "Arguments with a ParseAction of 'Append' " +
                    "or 'AppendConst' must be attached to a " +
                    "parameter assignable to IList.");
            }
            return null;
        }

        /// <summary>
        /// Count actions must be convertible to int.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="attr"></param>
        /// <returns></returns>
        private static Exception CountActionConvertibleToIntCheck(
            PropertyInfo prop, ArgumentAttribute attr)
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
        private static Exception StoreTrueFalseConvertibleToBoolCheck(
            PropertyInfo prop, ArgumentAttribute attr)
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
        private static Exception ConstActionsCannotHaveNullValueCheck(
            PropertyInfo prop, ArgumentAttribute attr)
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
        private static Exception ConvertibleConstValuesCheck(
            PropertyInfo prop, ArgumentAttribute attr)
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
        private static Exception LastPositionalArgumentCanTakeMultipleValuesCheck<T>()
        {
            var props = typeof(T).GetProperties()
                .Where(p => p.GetCustomAttribute<PositionalArgumentAttribute>() != null);
            foreach (var prop in props.Take(props.Count() - 1))
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
        /// PostParse methods must take zero parameters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static Exception PostParseZeroParametersCheck<T>()
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
