namespace clipr
{
    /// <summary>
    /// Indicates the range of argument values
    /// that may be parsed by the argument.
    /// </summary>
    public enum NumArgsConstraint
    {
        /// <summary>
        /// The parameter must provide exactly this number of arguments.
        /// </summary>
        Exactly = 0,

        /// <summary>
        /// The parameter allows either zero or one arguments. When specifying
        /// this constraint, you cannot also set NumArgs.
        /// </summary>
        Optional,

        /// <summary>
        /// The parameter must provide at least this number of arguments.
        /// </summary>
        AtLeast,

        /// <summary>
        /// The parameter must provide at most this number of arguments.
        /// </summary>
        AtMost
    }
}
