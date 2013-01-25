using clipr.Triggers;

namespace clipr.Usage
{
    /// <summary>
    /// Program version information.
    /// </summary>
    public interface IVersion<T> : ITrigger<T> where T : class
    {
        /// <summary>
        /// The version string.
        /// </summary>
        /// <returns>A string containing version information.</returns>
        string GetVersion();
    }
}
