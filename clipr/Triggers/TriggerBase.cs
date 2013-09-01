using System;
using System.Collections.Generic;
using System.Reflection;

namespace clipr.Triggers
{
    public abstract class TriggerBase
    {
        public List<string> MutuallyExclusiveGroups { get; set; }

        public bool ConsumesMultipleArgs { get { return false; } }

        public object Const { get; set; }
        
        public char? ShortName { get; set; }

        public string LongName { get; set; }

        public string MetaVar { get; set; }

        public PropertyInfo Property
        {
            get { return null; }
            set { throw new NotImplementedException(); }
        }


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
