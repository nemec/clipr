
namespace clipr.Arguments
{
    public interface IShortNameArgument : INamedArgumentBase
    {
        /// <summary>
        /// Single character name for the argument.
        /// </summary>
        char? ShortName { get; }
    }
}
