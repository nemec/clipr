using System;
using System.Collections.Generic;
using clipr.Core;

namespace clipr.Usage
{
    /// <summary>
    /// Generate usage information from localizable resource files.
    /// TODO implement help from localized resource files
    /// </summary>
    internal class ResourceUsageGenerator : IHelpGenerator
    {
        public IParserConfig Config { get; set; }

        public string Name { get { return "ResourceUsageGenerator"; } }

        public List<string> MutuallyExclusiveGroups { get; set; }

        public bool ConsumesMultipleArgs { get { return false; } }

        public object Const { get; set; }

        public char? ShortName { get; set; }

        public string LongName { get; set; }

        public bool Required { get { return false; } }

        /// <summary>
        /// Create a new usage generator with the
        /// default argument names (-h and --help).
        /// </summary>
        public ResourceUsageGenerator()
        {
            ShortName = 'h';
            LongName = "help";
        }

        public string GetUsage(IParserConfig config)
        {
            throw new NotImplementedException();
        }


        public string GetHelp(IParserConfig config)
        {
            throw new NotImplementedException();
        }

        public string PluginName
        {
            get { return "ResourceUsageGenerator"; }
        }

        public void OnParse(IParserConfig config)
        {
            throw new NotImplementedException();
        }

        public IValueStoreDefinition Store { get; set; }


        public string MetaVar { get; set; }

        public string Description
        {
            get { return "Generates usage information from resource files."; }
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


        public uint NumArgs
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

        public NumArgsConstraint Constraint
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
