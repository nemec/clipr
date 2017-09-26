
namespace clipr.Fluent
{
    public class Consumes<TConfig, TClass, TValue>
        where TClass : ArgumentBase<TConfig, TClass, TValue>
        where TConfig : class
    {
        private ArgumentBase<TConfig, TClass, TValue> Arg { get; set; }

        internal Consumes(TClass named)
        {
            Arg = named;
        }  

        public TClass AtLeast(uint numArguments)
        {
            Arg.BaseArgument.Constraint = NumArgsConstraint.AtLeast;
            Arg.BaseArgument.NumArgs = numArguments;
            return (TClass)Arg;
        }

        public TClass AtMost(uint numArguments)
        {
            Arg.BaseArgument.Constraint = NumArgsConstraint.AtMost;
            Arg.BaseArgument.NumArgs = numArguments;
            return (TClass)Arg;
        } 

        public TClass Exactly(uint numArguments)
        {
            Arg.BaseArgument.Constraint = NumArgsConstraint.Exactly;
            Arg.BaseArgument.NumArgs = numArguments;
            return (TClass)Arg;
        } 
    }
}
