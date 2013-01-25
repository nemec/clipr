using System.Reflection;

namespace clipr.Arguments
{
    public interface ILongNameArgument : IArgument
    {
        string LongName { get; }
    }
}
