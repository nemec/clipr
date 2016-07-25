using System;
using System.Reflection;
using System.Resources;

namespace clipr.Utils
{
    internal static class I18N
    {
        private static Assembly _asm = Assembly.GetAssembly(typeof(I18N));

        private static ResourceManager _mgr = new ResourceManager("clipr.Properties.Resources", _asm);

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
