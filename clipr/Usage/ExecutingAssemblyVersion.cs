using System;
using System.Reflection;
using clipr.Core;
using clipr.Triggers;

namespace clipr.Usage
{
    /// <summary>
    /// Version information pulled from the currently executing assembly.
    /// </summary>
    public class ExecutingAssemblyVersion<T> : TriggerBase, IVersion<T> where T : class
    {
        public string Name { get { return "Version"; } }

        private readonly string _version;

        /// <summary>
        /// Version information pulled from the currently executing assembly.
        /// </summary>
        public ExecutingAssemblyVersion()
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            _version = AssemblyName.GetAssemblyName(assembly.Location).Version.ToString();

            ShortName = null;
            LongName = "version";
        }

        public string GetVersion()
        {
            return _version;
        }

        public string PluginName
        {
            get { return "Version"; }
        }

        public void OnParse(IParserConfig<T> config)
        {
            Console.Error.WriteLine(GetVersion());
        }

        public string Description
        {
            get { return "Displays the version of the current executable."; }
        }
    }
}
