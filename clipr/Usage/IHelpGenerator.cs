using clipr.Core;
using clipr.Triggers;

namespace clipr.Usage
{
    /// <summary>
    /// Generates help documentation for a parser.
    /// </summary>
    public interface IHelpGenerator<T> : ITerminatingTrigger<T> where T : class
    {
        /// <summary>
        /// Build a short string displaying the order and number of 
        /// optional and required parameters.
        /// </summary>
        /// <returns></returns>
        string GetUsage();

        /// <summary>
        /// Build a complete description of valid options.
        /// </summary>
        /// <param name="config">Configuration settings.</param>
        /// <returns></returns>
        string GetHelp(IParserConfig<T> config);
    }
}
