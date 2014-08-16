using System;
using System.Collections.Generic;
using clipr.Core;

namespace clipr.Triggers
{
    public abstract class TriggerBase
    {
        public List<string> MutuallyExclusiveGroups { get; set; }

        public bool ConsumesMultipleArgs { get { return false; } }

        public object Const { get; set; }
        
        public char? ShortName { get; set; }

        public string LongName { get; set; }

        public bool Required { get { return false; }}

        public string MetaVar { get; set; }

        public IValueStoreDefinition Store { get { return null; } }


        public ParseAction Action
        {
            get { return ParseAction.Store; }
            set { throw new NotImplementedException(); }
        }


        public uint NumArgs
        {
            get { return 1; }
            set { throw new NotImplementedException(); }
        }

        public NumArgsConstraint Constraint
        {
            get { return NumArgsConstraint.Exactly; }
            set { throw new NotImplementedException(); }
        }
    }
}
