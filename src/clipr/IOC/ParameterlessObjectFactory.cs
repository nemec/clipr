using clipr.Utils;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
#if NETCORE || NET45
using System.Reflection;
#endif

namespace clipr.IOC
{
    delegate object ObjectActivator();

    sealed class ParameterlessObjectFactory : IObjectFactory
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly ParameterlessObjectFactory Default = new ParameterlessObjectFactory();

        private readonly Dictionary<Type, ObjectActivator> _cache = new Dictionary<Type, ObjectActivator>();

        public bool CanCreateObject(Type objectType)
        {
            return _cache.ContainsKey(objectType) || 
                objectType.GetTypeInfo().GetConstructor(Type.EmptyTypes) != null;
        }

        public object CreateObject(Type objectType)
        {
            ObjectActivator compiled;
            if (!_cache.TryGetValue(objectType, out compiled))
            {
                var ctor = objectType.GetTypeInfo().GetConstructor(Type.EmptyTypes);
                if(ctor == null)
                {
                    throw new ArgumentException(String.Format(
                        "Verb or PostParse parameter type '{0}' has no parameterless constructor. Use a custom IObjectFactory for IOC.",
                        objectType));
                }

                var newExp = Expression.New(ctor);
                var lambda = Expression.Lambda(typeof(ObjectActivator), newExp);
                compiled = (ObjectActivator)lambda.Compile();
                _cache.Add(objectType, compiled);
            }
            return compiled.Invoke();
        }
    }
}
