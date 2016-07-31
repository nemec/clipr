using System;


namespace clipr.Utils
{
#if NET35
    internal static class DotNetCoreExtensions
    {
        public static Type GetTypeInfo(this Type t)
        {
            return t;
        }
    }
#endif
}
