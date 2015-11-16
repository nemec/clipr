namespace clipr
{
    /// <summary>
    /// Multiple arguments with the same name have been defined.
    /// </summary>
    public class DuplicateArgumentException : ArgumentIntegrityException
    {
        /// <summary>
        /// Multiple arguments with the same name have been defined.
        /// </summary>
        /// <param name="argumentName">Name of the duplicate argument.</param>
        internal DuplicateArgumentException(string argumentName)
            : base("Duplicate argument was defined: " + argumentName)
        {
        }
    }
}
