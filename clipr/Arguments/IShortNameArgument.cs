
namespace clipr.Arguments
{
    public interface IShortNameArgument : IArgument
    {
        /// <summary>
        /// Single character name for the argument.
        /// </summary>
        char? ShortName { get; }
    }
}
