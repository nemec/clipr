using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace clipr.IOC
{
    delegate object ObjectActivator();

    sealed class ParameterlessVerbFactory : IVerbFactory
    {
        private readonly Dictionary<Type, ObjectActivator> _cache = new Dictionary<Type, ObjectActivator>();

        public object GetVerb(Type objectType)
        {
            ObjectActivator compiled;
            if (!_cache.TryGetValue(objectType, out compiled))
            {
                var ctor = objectType.GetConstructor(Type.EmptyTypes);
                if(ctor == null)
                {
                    throw new ArgumentException("Option or verb type '" + objectType + "' has no " +
                        "parameterless constructor. Use a custom IObjectFactory for IOC.");
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
