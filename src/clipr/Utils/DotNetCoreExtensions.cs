using System;


namespace clipr.Utils
{
#if NET40
    internal static class DotNetCoreExtensions
    {
        public static Type GetTypeInfo(this Type t)
        {
            return t;
        }
    }
#endif
}
