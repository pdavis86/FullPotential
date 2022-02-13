using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullPotential.Core.Registry
{
    public class ServiceRegistry
    {
        private List<Type> _doNotCache = new List<Type>();
        private Dictionary<Type, Type> _registry = new Dictionary<Type, Type>();
        private Dictionary<Type, object> _cache = new Dictionary<Type, object>();

        public void Register<TInterface, TClass>(bool doNotCache = false)
            where TClass : class, TInterface
        {
            _registry.Add(typeof(TInterface), typeof(TClass));

            if (doNotCache)
            {
                _doNotCache.Add(typeof(TClass));
            }
        }

        public void Register<TClass>(bool doNotCache = false)
            where TClass : class
        {
            _registry.Add(typeof(TClass), typeof(TClass));

            if (doNotCache)
            {
                _doNotCache.Add(typeof(TClass));
            }
        }

        public T GetService<T>()
        {
            return (T)GetServiceInternal(typeof(T));
        }

        public T GetService<T>(T type)
        {
            return (T)GetServiceInternal(typeof(T));
        }

        private object GetServiceInternal(Type requestedType)
        {
            var serviceType = _registry[requestedType];

            if (_cache.ContainsKey(serviceType))
            {
                return _cache[serviceType];
            }

            var ctor = serviceType.GetConstructors()[0];
            var parameters = ctor.GetParameters();

            object newInstance;
            if (parameters.Length > 0)
            {
                var args = new List<object>();
                foreach (var param in ctor.GetParameters())
                {
                    if (!_registry.ContainsKey(param.ParameterType))
                    {
                        Debug.LogError($"'{requestedType}' takes a parameter of '{param.ParameterType}' but this type is not registered");
                    }
                    args.Add(GetServiceInternal(param.ParameterType));
                }

                newInstance = ctor.Invoke(args.ToArray());
            }
            else
            {
                newInstance = Activator.CreateInstance(serviceType);
            }

            if (!_doNotCache.Contains(serviceType))
            {
                _cache.Add(serviceType, newInstance);
            }

            return newInstance;
        }
    }
}
