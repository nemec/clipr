
namespace clipr.Arguments
{
    public interface ILongNameArgument : INamedArgumentBase
    {
        /// <summary>
        /// Longer, multi-character name for the argument.
        /// </summary>
        string LongName { get; }
    }
}
