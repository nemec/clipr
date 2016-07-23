using System;
using System.Collections;
using System.Collections.Generic;

namespace clipr.IOC
{
    /// <summary>
    /// Easily define delegates to generate verb instances during parsing.
    /// This class will *not* handle verbs with default constructors
    /// automatically, all verbs must be explicitly added.
    /// </summary>
    public class SimpleVerbfactory : IVerbFactory, IEnumerable<object>
    {
        private Dictionary<Type, Func<object>> _verbCache = new Dictionary<Type, Func<object>>();

        /// <summary>
        /// Add a factory for the given type.
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="verbFactory"></param>
        public void Add(Type objectType, Func<object> verbFactory)
        {
            _verbCache[objectType] = verbFactory;
        }

        public void Add<T>(Func<T> verbFactory)
            where T : class
        {
            _verbCache[typeof(T)] = () => verbFactory();
        }

        public bool CanCreateVerb(Type objectType)
        {
            return _verbCache.ContainsKey(objectType);
        }

        /// <inheritdoc />
        public object GetVerb(Type objectType)
        {
            Func<object> factory;
            if(!_verbCache.TryGetValue(objectType, out factory))
            {
                throw new ArgumentException(
                    String.Format("No factory was defined for verb type {0}.", objectType));
            }
            return factory();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
