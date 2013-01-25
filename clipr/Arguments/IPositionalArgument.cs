
namespace clipr.Arguments
{
    internal interface IPositionalArgument : IArgument
    {
        int Index { get; }
    }
}
