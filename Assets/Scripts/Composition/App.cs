using System;

namespace Composition
{
    public static class App
    {
        public static ServiceContainer Services { get; private set; }

        public static bool IsInitialized => Services != null;

        public static void Initialize(ServiceContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (Services != null) throw new InvalidOperationException("App is already initialized.");

            Services = container;
        }
    }
}

