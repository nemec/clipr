using clipr.Arguments;
using clipr.Core;

namespace clipr.Triggers
{
    /// <summary>
    /// A parser hook that can be triggered by a named argument
    /// and executes a function rather than store a value.
    /// Will terminate parsing immediately.
    /// </summary>
    public interface ITerminatingTrigger<T> : INamedArgument where T : class
    {
        /// <summary>
        /// Name of the plugin.
        /// </summary>
        string PluginName { get; }

        /// <summary>
        /// Method executed when the short or long name is parsed.
        /// </summary>
        void OnParse(IParserConfig<T> config);
    }
}
