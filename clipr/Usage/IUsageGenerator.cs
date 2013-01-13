namespace clipr.Usage
{
    /// <summary>
    /// Generates help documentation for a parser.
    /// </summary>
    public interface IUsageGenerator
    {
        /// <summary>
        /// Short argument for triggering the help
        /// page from a command line argument.
        /// </summary>
        char? ShortName { get; }

        /// <summary>
        /// Long argument for triggering the help
        /// page from a command line argument.
        /// </summary>
        string LongName { get; }

        /// <summary>
        /// Version information that may be
        /// displayed on the help page.
        /// </summary>
        IVersion Version { get; set; }

        /// <summary>
        /// Build help information.
        /// </summary>
        /// <returns>A string containing generated help information.</returns>
        string GetUsage();
    }
}
