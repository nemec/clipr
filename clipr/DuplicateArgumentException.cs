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
        public DuplicateArgumentException(string argumentName)
            : base("Duplicate argument: " + argumentName)
        {
        }
    }
}
