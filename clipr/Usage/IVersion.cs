
namespace clipr.Usage
{
    /// <summary>
    /// Program version information.
    /// </summary>
    public interface IVersion
    {
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
        /// The version string.
        /// </summary>
        /// <returns>A string containing version information.</returns>
        string GetVersion();
    }
}
