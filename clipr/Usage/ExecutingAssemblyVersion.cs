using System;
using System.Reflection;
using clipr.Core;
using clipr.Triggers;

namespace clipr.Usage
{
    /// <summary>
    /// Version information pulled from the currently executing assembly.
    /// </summary>
    public class ExecutingAssemblyVersion : TriggerBase, IVersion
    {
        /// <inheritdoc/>
        public override string Name { get { return "Version"; } }

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

        /// <summary>
        /// Get the version of this application.
        /// </summary>
        /// <returns></returns>
        public string GetVersion()
        {
            return _version;
        }

        /// <summary>
        /// Get the name of this plugin.
        /// </summary>
        public string PluginName
        {
            get { return "Version"; }
        }

        /// <summary>
        /// Action to perform when trigger is parsed.
        /// </summary>
        /// <param name="config"></param>
        public void OnParse(IParserConfig config)
        {
            Console.Error.WriteLine(GetVersion());
        }

        /// <inheritdoc/>
        public override string Description
        {
            get { return "Displays the version of the current executable."; }
        }
    }
}
