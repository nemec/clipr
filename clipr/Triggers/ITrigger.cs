using clipr.Arguments;

namespace clipr.Triggers
{
    /// <summary>
    /// A parser hook that can be triggered by a named argument
    /// and executes a function rather than store a value.
    /// </summary>
    public interface ITrigger<T> : INamedArgument where T : class
    {
        /// <summary>
        /// Name of the plugin.
        /// </summary>
        string PluginName { get; }

        /// <summary>
        /// Configuration of the parser.
        /// </summary>
        //ParserConfig<T> Config { get; set; }

        /// <summary>
        /// Method executed when the short or long name is parsed.
        /// </summary>
        void OnParse(IParserConfig<T> config);
    }
}
