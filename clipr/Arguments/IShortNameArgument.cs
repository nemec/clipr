
namespace clipr.Arguments
{
    /// <summary>
    /// A named argument with a single character name.
    /// </summary>
    public interface IShortNameArgument : INamedArgumentBase
    {
        /// <summary>
        /// Single character name for the argument.
        /// </summary>
        char? ShortName { get; }
    }
}
