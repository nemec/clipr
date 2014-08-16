
namespace clipr.Arguments
{
    public interface INamedArgument : IShortNameArgument, ILongNameArgument
    {
    }

    public interface INamedArgumentBase : IArgument
    {
        bool Required { get; }
    }
}
