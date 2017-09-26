using System;
using System.Reflection;
using System.Resources;

namespace clipr.Utils
{
    internal static class I18N
    {
#if NET35
        private static Assembly _asm = Assembly.GetAssembly(typeof(I18N));
        private const string ResourceName = "clipr.Properties.Resources";
#else
        private static Assembly _asm = typeof(I18N).GetTypeInfo().Assembly;
        private const string ResourceName = "clipr.NetCore.Resources";
#endif

        private static ResourceManager _mgr = new ResourceManager(ResourceName, _asm);

        public static string _(string resourceName)
        {
            return _mgr.GetString(resourceName);
        }

        public static string _(Type resourceType, string resourceName)
        {
            var mgr = new ResourceManager(resourceType);
            return mgr.GetString(resourceName);
        }
    }
}
