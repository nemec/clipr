using System;
using System.Reflection;

namespace clipr.Usage
{
    /// <summary>
    /// Version information pulled from the currently executing assembly.
    /// </summary>
    public class ExecutingAssemblyVersion<T> : IVersion<T> where T : class
    {
        public string ArgumentName { get { return "Version"; } }

        public string[] MutuallyExclusiveGroups { get; set; }

        public bool ConsumesMultipleArgs { get { return false; } }

        public object Const { get; set; }

        public char? ShortName { get; set; }

        public string LongName { get; set; }

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

        public void OnParse()
        {
            Console.Error.WriteLine(GetVersion());
        }


        public ParserConfig<T> Config { get; set; }

        public PropertyInfo Property { get; set; }

        public string MetaVar { get; set; }

        public string Description
        {
            get { return "Displays the version of the current executable."; }
        }


        public ParseAction Action
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
