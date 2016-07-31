using clipr.Utils;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
#if NETCORE
using System.Reflection;
#endif

namespace clipr.IOC
{
    delegate object ObjectActivator();

    sealed class ParameterlessVerbFactory : IVerbFactory
    {
        private readonly Dictionary<Type, ObjectActivator> _cache = new Dictionary<Type, ObjectActivator>();

        public bool CanCreateVerb(Type objectType)
        {
            return _cache.ContainsKey(objectType) || 
                objectType.GetTypeInfo().GetConstructor(Type.EmptyTypes) != null;
        }

        public object GetVerb(Type objectType)
        {
            ObjectActivator compiled;
            if (!_cache.TryGetValue(objectType, out compiled))
            {
                var ctor = objectType.GetTypeInfo().GetConstructor(Type.EmptyTypes);
                if(ctor == null)
                {
                    throw new ArgumentException(String.Format(
                        "Option or verb type '{0}' has no parameterless constructor. Use a custom IVerbFactory for IOC.",
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
