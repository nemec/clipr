using System;

namespace clipr
{
    /// <summary>
    /// Methods marked with this attribute will be run after
    /// parsing has completed (successfully).
    /// 
    /// Method must have no parameters.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class PostParseAttribute : Attribute
    {
    }
}
