
namespace clipr.Arguments
{
    public interface ILongNameArgument : IArgument
    {
        /// <summary>
        /// Longer, multi-character name for the argument.
        /// </summary>
        string LongName { get; }
    }
}
