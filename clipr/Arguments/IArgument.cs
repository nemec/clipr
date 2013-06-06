using System.Collections.Generic;
using System.Reflection;

namespace clipr.Arguments
{
    public interface IArgument
    {
        /// <summary>
        /// Name of the argument. Used as a last resort for choosing a
        /// display name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Description of the argument value. Used when
        /// generating usage documentation.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Number of arguments consumed by this property.
        /// See <see cref="Constraint"/> for information on
        /// whether this number is the minimum, maximum, or exact
        /// number of arguments allowed. Defaults to 1 and should
        /// never be 0 (actions that take no parameters will ignore
        /// this property).
        /// </summary>
        uint NumArgs { get; }

        /// <summary>
        /// Specifies whether or not <see cref="NumArgs"/> defines
        /// the minimum, maximum, or exact number of arguments allowed.
        /// </summary>
        NumArgsConstraint Constraint { get; }

        /// <summary>
        /// Defines an alternate character to be used as a placeholder
        /// for the argument value in usage documentation. By default,
        /// the alternate name is generated from the property itself.
        /// </summary>
        string MetaVar { get; }

        /// <summary>
        /// The constant value stored instead of an argument for some
        /// <see cref="ParseAction"/>s.
        /// </summary>
        object Const { get; }

        /// <summary>
        /// Action to perform when parsing this parameter.
        /// </summary>
        ParseAction Action { get; }

        /// <summary>
        /// Whether or not the argument can handle a
        /// variable number of arguments.
        /// </summary>
        bool ConsumesMultipleArgs { get; }

        /// <summary>
        /// List of mutually exclusive groups the argument has joined.
        /// Only one argument in a group may be specified on the command
        /// line at a time.
        /// </summary>
        List<string> MutuallyExclusiveGroups { get; }

        /// <summary>
        /// Backing property for the argument. Parsed values are set on
        /// this property.
        /// </summary>
        PropertyInfo Property { get; }
    }
}
