using clipr.Core;

namespace clipr.Arguments
{
    internal class PositionalArgument : BaseArgument, IPositionalArgument
    {
        /// <summary>
        /// Index in the positional argument list for this argument.
        /// </summary>
        public int Index { get; set; }

        public PositionalArgument(IValueStoreDefinition store)
            : base(store)
        {
        }
    }
}
