using System.Reflection;

namespace clipr.Arguments
{
    internal abstract class BaseArgument : IArgument
    {
        public string ArgumentName { get; internal set; }

        /// <summary>
        /// Description of the argument value. Used when
        /// generating usage documentation.
        /// </summary>
        public string Description { get; set; }

        public string[] MutuallyExclusiveGroups { get; set; }

        /// <summary>
        /// Number of arguments consumed by this property.
        /// See <see cref="Constraint"/> for information on
        /// whether this number is the minimum, maximum, or exact
        /// number of arguments allowed. Defaults to 1 and should
        /// never be 0 (actions that take no parameters will ignore
        /// this property).
        /// </summary>
        public uint NumArgs { get; set; }

        /// <summary>
        /// Specifies whether or not <see cref="NumArgs"/> defines
        /// the minimum, maximum, or exact number of arguments allowed.
        /// </summary>
        public NumArgsConstraint Constraint { get; set; }

        /// <summary>
        /// Defines an alternate character to be used as a placeholder
        /// for the argument value in usage documentation. By default,
        /// the alternate name is generated from the property itself.
        /// </summary>
        public virtual string MetaVar { get; set; }

        /// <summary>
        /// The constant value stored instead of an argument for some
        /// <see cref="ParseAction"/>s.
        /// </summary>
        public object Const { get; set; }

        /// <summary>
        /// Action to perform when parsing this parameter.
        /// </summary>
        public ParseAction Action { get; set; }

        /// <summary>
        /// Whether or not the argument can handle a
        /// variable number of arguments.
        /// </summary>
        public bool ConsumesMultipleArgs
        {
            get
            {
                return NumArgs != 0 && 
                    (Constraint != NumArgsConstraint.Exactly ||
                    NumArgs > 1);
            }
        }

        /// <summary>
        /// Whether or not the argument can handle a
        /// variable number of arguments.
        /// </summary>
        internal bool HasVariableNumArgs
        {
            get
            {
                return NumArgs != 0 && Constraint != NumArgsConstraint.Exactly;
            }
        }

        public PropertyInfo Property { get; set; }

        /// <summary>
        /// Create a new Argument.
        /// </summary>
        protected BaseArgument(PropertyInfo prop)
        {
            Property = prop;
            NumArgs = 1;
        }

        /// <summary>
        /// The display name for the argument.
        /// </summary>
        /// <returns>
        /// The display name for the argument or null
        /// if the display name should be the property name.
        /// </returns>
        internal abstract string GetArgumentDisplayName();
    }
}
