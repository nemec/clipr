using System.Reflection;

namespace clipr.Arguments
{
    public interface IArgument
    {
        string ArgumentName { get; }
        
        PropertyInfo Property { get; set; }

        string[] MutuallyExclusiveGroups { get; set; }

        ParseAction Action { get; set; }

        object Const { get; set; }

        string MetaVar { get; set; }

        string Description { get; }

        bool ConsumesMultipleArgs { get; }
    }
}
