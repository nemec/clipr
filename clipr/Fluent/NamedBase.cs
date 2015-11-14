using System;
using System.Linq;
using clipr.Arguments;
using clipr.Core;

namespace clipr.Fluent
{
    public class NamedBase<TConfig, TNamed, TValue>
        : ArgumentBase<TConfig, NamedBase<TConfig, TNamed, TValue>, TValue>
        where TNamed : NamedBase<TConfig, TNamed, TValue>
        where TConfig : class
    {
        internal NamedArgument Arg { get; set; }

        private bool HasDefaultName { get; set; }

        internal override BaseArgument BaseArgument { get { return Arg; } }

        protected NamedBase(CliParserBuilder<TConfig> parser, IValueStoreDefinition store)
            : base(parser)
        {
            Arg = new NamedArgument(store);
            HasDefaultName = false;
        }

        private void ClearArgumentNamesIfNeeded()
        {
            if (!HasDefaultName) return;

            Arg.ShortName = null;
            Arg.LongName = null;
            HasDefaultName = true;
        }

        /// <summary>
        /// Use the property's lowercased first character as the short
        /// argument name.
        /// </summary>
        /// <returns></returns>
        public TNamed WithShortName()
        {
            return WithShortName(Arg.Name.First());
        }

        /// <summary>
        /// Use the given character as the short argument name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TNamed WithShortName(char name)
        {
            ClearArgumentNamesIfNeeded();
            Arg.ShortName = Char.ToLowerInvariant(name);
            return (TNamed)this;
        }

        /// <summary>
        /// Use the property's lowercased name as the long argument name.
        /// </summary>
        /// <returns></returns>
        public TNamed WithLongName()
        {
            return WithLongName(Arg.Name.ToLowerInvariant());
        }

        /// <summary>
        /// Use the given name as the long argument name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TNamed WithLongName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            ClearArgumentNamesIfNeeded();
            Arg.LongName = name.ToLowerInvariant();
            return (TNamed)this;
        }

        public TNamed IsInMututallyExclusiveSet(string group)
        {
            Arg.MutuallyExclusiveGroups.Add(group);
            return (TNamed)this;
        }

        public TNamed StoresTrue()
        {
            Arg.Action = ParseAction.StoreTrue;
            return (TNamed)this;
        }

        public TNamed StoresFalse()
        {
            Arg.Action = ParseAction.StoreFalse;
            return (TNamed)this;
        }

        public TNamed CountsInvocations()
        {
            Arg.Action = ParseAction.Count;
            return (TNamed)this;
        }
    } 
}
