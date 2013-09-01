using System;
using System.Collections.Generic;
using System.Reflection;
using clipr.Arguments;

namespace clipr.Usage
{
    /// <summary>
    /// Generate usage information from localizable resource files.
    /// </summary>
    internal class ResourceUsageGenerator<T> : IHelpGenerator<T> where T : class, INamedArgument
    {
        public ParserConfig<T> Config { get; set; }

        public string Name { get { return "ResourceUsageGenerator"; } }

        public List<string> MutuallyExclusiveGroups { get; set; }

        public bool ConsumesMultipleArgs { get { return false; } }

        public object Const { get; set; }

        public char? ShortName { get; set; }

        public string LongName { get; set; }

        /// <summary>
        /// Create a new usage generator with the
        /// default argument names (-h and --help).
        /// </summary>
        public ResourceUsageGenerator()
        {
            ShortName = 'h';
            LongName = "help";
        }

        public string GetUsage()
        {
            throw new NotImplementedException();
        }


        public string GetHelp(IParserConfig<T> config)
        {
            throw new NotImplementedException();
        }

        public string PluginName
        {
            get { return "ResourceUsageGenerator"; }
        }

        public void OnParse(IParserConfig<T> config)
        {
            throw new NotImplementedException();
        }

        public PropertyInfo Property { get; set; }


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
