using System.Collections;
using clipr.Core;

namespace clipr
{
    /// <summary>
    /// Action performed when an argument is parsed. Determines whether or
    /// not the argument consumes values and what is stored in the
    /// argument property.
    /// </summary>
    public enum ParseAction
    {
        /// <summary>
        /// <para>
        /// Convert the next value in the argument list and store it.
        /// If specified multiple times, overwrite previous value.
        /// </para>
        /// <para>
        /// This <see cref="ParseAction"/> consumes arguments.
        /// </para>
        /// </summary>
        Store = 0,

        /// <summary>
        /// <para>
        /// Store the value <see cref="ArgumentAttribute.Const"/>
        /// instead of an argument value.
        /// </para>
        /// <para>
        /// This <see cref="ParseAction"/> does not consume arguments.
        /// </para>
        /// </summary>
        StoreConst,

        /// <summary>
        /// <para>
        /// Store the value <value>true</value> instead of an argument value.
        /// </para>
        /// <para>
        /// This <see cref="ParseAction"/> does not consume arguments.
        /// </para>
        /// </summary>
        StoreTrue,

        /// <summary>
        /// <para>
        /// Store the value <value>false</value> instead of an argument value.
        /// </para>
        /// <para>
        /// This <see cref="ParseAction"/> does not consume arguments.
        /// </para>
        /// </summary>
        StoreFalse,

        /// <summary>
        /// <para>
        /// Convert the next value(s) in the argument list and append.
        /// </para>
        /// <para>
        /// This action must be used on an instance of
        /// <see cref="IEnumerable"/> and if this argument is specified
        /// multiple times, values are appended to the list rather
        /// than overwritten.
        /// </para>
        /// <para>
        /// This <see cref="ParseAction"/> consumes arguments.
        /// </para>
        /// </summary>
        Append,

        /// <summary>
        /// <para>
        /// Append the value <see cref="ArgumentAttribute.Const"/> to
        /// this property.
        /// </para>
        /// <para>
        /// This action must be used on an instance of
        /// <see cref="IEnumerable"/> and if this argument is specified
        /// multiple times, values are appended to the list rather
        /// than overwritten.
        /// </para>
        /// <para>
        /// This <see cref="ParseAction"/> does not consume arguments.
        /// </para>
        /// </summary>
        AppendConst,

        /// <summary>
        /// <para>
        /// Increment the value of this property by 1 every time it is
        /// specified.
        /// </para>
        /// <para>
        /// This action must be used on a <see cref="int"/>.
        /// </para>
        /// <para>
        /// This <see cref="ParseAction"/> does not consume arguments.
        /// </para>
        /// </summary>
        Count
    }

    internal static class ParseActionExtensions
    {
        /// <summary>
        /// Indicates whether or not an action has the potential to
        /// consume argument values.
        /// </summary>
        /// <param name="action">Action to test.</param>
        /// <returns>true if the action consumes values, false otherwise</returns>
        public static bool ConsumesArgumentValues(this ParseAction action)
        {
            return action == ParseAction.Store || action == ParseAction.Append;
        }
    }
}
