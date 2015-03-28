using System.Collections.Generic;
using clipr.Core;

namespace clipr.Arguments
{
    /// <summary>
    /// Defines the basic properties of an argument.
    /// </summary>
    internal abstract class BaseArgument : IArgument
    {
        public string Name { get; internal set; }

        public string Description { get; set; }

        public List<string> MutuallyExclusiveGroups { get; set; }

        public uint NumArgs { get; set; }

        public NumArgsConstraint Constraint { get; set; }

        public string MetaVar { get; set; }

        public object Const { get; set; }
        
        public ParseAction Action { get; set; }

        public bool ConsumesMultipleArgs
        {
            get
            {
                return Constraint != NumArgsConstraint.Exactly || NumArgs > 1;
            }
        }

        public IValueStoreDefinition Store { get; set; }

        /// <summary>
        /// Create a new Argument.
        /// </summary>
        protected BaseArgument(IValueStoreDefinition value)
        {
            Initialize(value);
        }

        private void Initialize(IValueStoreDefinition value)
        {
            Name = value.Name;
            MetaVar = Name;
            Store = value;
            NumArgs = 1;
        }
    }
}
