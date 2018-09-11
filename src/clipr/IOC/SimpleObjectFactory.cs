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
    public class SimpleObjectFactory : IObjectFactory, IEnumerable<object>
    {
        private Dictionary<Type, Func<object>> _objectCache = new Dictionary<Type, Func<object>>();

        /// <summary>
        /// Add a factory for the given type.
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="objectFactory"></param>
        public void Add(Type objectType, Func<object> objectFactory)
        {
            _objectCache[objectType] = objectFactory;
        }

        public void Add<T>(Func<T> objectFactory)
            where T : class
        {
            _objectCache[typeof(T)] = () => objectFactory();
        }

        public bool CanCreateObject(Type objectType)
        {
            return _objectCache.ContainsKey(objectType);
        }

        /// <inheritdoc />
        public object CreateObject(Type objectType)
        {
            Func<object> factory;
            if(!_objectCache.TryGetValue(objectType, out factory))
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
