using System;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable UnusedMember.Global

namespace FullPotential.Api.Ioc
{
    public class DependenciesCollection
    {
        private readonly List<Type> _doNotCache = new List<Type>();
        private readonly Dictionary<Type, Type> _registry = new Dictionary<Type, Type>();
        private readonly Dictionary<Type, object> _singletons = new Dictionary<Type, object>();
        private readonly Dictionary<Type, Dependency> _factoryDependencies = new Dictionary<Type, Dependency>();

        public void Register<TInterface, TClass>(bool newInstanceOnRequest = false)
            where TClass : class, TInterface
        {
            _registry.Add(typeof(TInterface), typeof(TClass));

            if (newInstanceOnRequest)
            {
                _doNotCache.Add(typeof(TClass));
            }
        }

        // e.g. DependenciesContext.Dependencies.Register(new Dependency {
        //    Type = typeof(ExampleDependencyMonoBehaviour),
        //    Factory = () => Instantiate(exampleDependency).GetComponent<ExampleDependencyMonoBehaviour>(),
        //    IsSingleton = true});
        public void Register(Dependency dependency)
        {
            _registry.Add(dependency.Type, dependency.Type);
            _factoryDependencies.Add(dependency.Type, dependency);

            if (!dependency.IsSingleton)
            {
                _doNotCache.Add(dependency.Type);
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

        public void ResetForTesting()
        {
            _doNotCache.Clear();
            _registry.Clear();
            _singletons.Clear();
            _factoryDependencies.Clear();
        }

        private object GetServiceInternal(Type requestedType)
        {
            if (!_registry.ContainsKey(requestedType))
            {
                throw new ArgumentException($"Type '{requestedType}' has not been registered");
            }

            var serviceType = _registry[requestedType];

            if (_singletons.ContainsKey(serviceType))
            {
                return _singletons[serviceType];
            }

            var newInstance = _factoryDependencies.ContainsKey(serviceType)
                ? _factoryDependencies[serviceType].Factory()
                : CreateInstance(serviceType);

            if (!_doNotCache.Contains(serviceType))
            {
                _singletons.Add(serviceType, newInstance);
            }

            return newInstance;
        }

        private object CreateInstance(Type serviceType)
        {
            object newInstance;

            var ctor = serviceType.GetConstructors()[0];
            var parameters = ctor.GetParameters();

            if (parameters.Length > 0)
            {
                var args = new List<object>();
                foreach (var param in ctor.GetParameters())
                {
                    if (!_registry.ContainsKey(param.ParameterType))
                    {
                        Debug.LogError($"'{serviceType}' takes a parameter of '{param.ParameterType}' but this type is not registered");
                    }
                    args.Add(GetServiceInternal(param.ParameterType));
                }

                newInstance = ctor.Invoke(args.ToArray());
            }
            else
            {
                newInstance = Activator.CreateInstance(serviceType);
            }

            return newInstance;
        }

    }
}
