using System;
using System.Collections.Generic;

namespace Composition
{
    public sealed class ServiceContainer
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public void Register<T>(T instance) where T : class
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            _services[typeof(T)] = instance;
        }

        public T Resolve<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }

            throw new InvalidOperationException($"Service not registered: {typeof(T).FullName}");
        }

        public bool TryResolve<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var obj) && obj is T typed)
            {
                service = typed;
                return true;
            }

            service = null;
            return false;
        }
    }
}

