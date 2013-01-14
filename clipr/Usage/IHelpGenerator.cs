namespace clipr.Usage
{
    /// <summary>
    /// Generates help documentation for a parser.
    /// </summary>
    public interface IHelpGenerator
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
        /// Build a short string displaying the order and number of 
        /// optional and required parameters.
        /// </summary>
        /// <returns></returns>
        string GetUsage();

        /// <summary>
        /// Build a complete description of valid options.
        /// </summary>
        /// <returns></returns>
        string GetHelp();
    }
}
