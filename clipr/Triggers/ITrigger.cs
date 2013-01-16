namespace clipr.Triggers
{
    /// <summary>
    /// A parser hook that can be triggered by a named argument
    /// and executes a function rather than store a value.
    /// </summary>
    public interface ITrigger<T> where T : class
    {
        /// <summary>
        /// Name of the plugin.
        /// </summary>
        string PluginName { get; }

        /// <summary>
        /// Configuration of the parser.
        /// </summary>
        ParserConfig<T> Config { get; set; }

        /// <summary>
        /// Short argument for triggering the help
        /// page from a command line argument.
        /// </summary>
        char? ShortName { get; set; }

        /// <summary>
        /// Long argument for triggering the help
        /// page from a command line argument.
        /// </summary>
        string LongName { get; set; }

        /// <summary>
        /// Method executed when the short or long name is parsed.
        /// </summary>
        void OnParse();
    }
}
