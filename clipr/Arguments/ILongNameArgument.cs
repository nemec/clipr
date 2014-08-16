
namespace clipr.Arguments
{
    /// <summary>
    /// Basic properties of a long argument.
    /// </summary>
    public interface ILongNameArgument : INamedArgumentBase
    {
        /// <summary>
        /// Longer, multi-character name for the argument.
        /// </summary>
        string LongName { get; }
    }
}
