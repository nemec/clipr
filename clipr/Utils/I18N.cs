using System.IO;
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
    }
}
