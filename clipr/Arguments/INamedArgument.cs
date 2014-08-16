
namespace clipr.Arguments
{
    /// <summary>
    /// A combination of short and long named arguments.
    /// </summary>
    public interface INamedArgument : IShortNameArgument, ILongNameArgument
    {
    }

    /// <summary>
    /// Properties that apply to both short and long named arguments.
    /// </summary>
    public interface INamedArgumentBase : IArgument
    {
        /// <summary>
        /// Whether or not this named argument is required.
        /// </summary>
        bool Required { get; }
    }
}
