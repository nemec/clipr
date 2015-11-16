namespace clipr
{
    /// <summary>
    /// Multiple verbs with the same name have been defined.
    /// </summary>
    public class DuplicateVerbException : ArgumentIntegrityException
    {
        /// <summary>
        /// Multiple verbs with the same name have been defined.
        /// </summary>
        /// <param name="verbName">Name of the duplicate verb.</param>
        internal DuplicateVerbException(string verbName)
            : base("Duplicate verb was defined: " + verbName)
        {
        }
    }
}
