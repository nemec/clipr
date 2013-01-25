using System.Reflection;

namespace clipr.Arguments
{
    public interface IShortNameArgument : IArgument
    {
        char? ShortName { get; }
    }
}
