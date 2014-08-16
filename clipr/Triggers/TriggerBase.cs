using System;
using System.Collections.Generic;
using clipr.Arguments;
using clipr.Core;

namespace clipr.Triggers
{
    /// <summary>
    /// Base properties of a trigger.
    /// </summary>
    public abstract class TriggerBase : INamedArgument
    {
        /// <inheritdoc/>
        public List<string> MutuallyExclusiveGroups { get; set; }

        /// <inheritdoc/>
        public bool ConsumesMultipleArgs { get { return false; } }

        /// <inheritdoc/>
        public object Const { get; set; }

        /// <inheritdoc/>
        public char? ShortName { get; set; }

        /// <inheritdoc/>
        public string LongName { get; set; }

        /// <inheritdoc/>
        public bool Required { get { return false; }}

        /// <inheritdoc/>
        public string MetaVar { get; set; }

        /// <inheritdoc/>
        public IValueStoreDefinition Store { get { return null; } }

        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public abstract string Description { get; }


        /// <inheritdoc/>
        public ParseAction Action
        {
            get { return ParseAction.Store; }
            set { throw new NotImplementedException(); }
        }


        /// <inheritdoc/>
        public uint NumArgs
        {
            get { return 1; }
            set { throw new NotImplementedException(); }
        }

        /// <inheritdoc/>
        public NumArgsConstraint Constraint
        {
            get { return NumArgsConstraint.Exactly; }
            set { throw new NotImplementedException(); }
        }
    }
}
