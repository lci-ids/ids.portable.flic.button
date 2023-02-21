using System;
using Microsoft.Extensions.DependencyInjection;

namespace IDS.Portable.Flic.Button.Platforms.Shared
{
    /// <summary>
    /// This class is used to resolve dependency injections required for native BLE implementations. 
    /// </summary>

    public static class ServiceCollection
    {
        private static readonly IServiceCollection _serviceCollection = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        private static IServiceProvider? _serviceProvider;

        public static void Init()
        {
            _serviceProvider = _serviceCollection.BuildServiceProvider(new ServiceProviderOptions()
            {
                ValidateOnBuild = true,
                ValidateScopes = true,
            });
        }

        public static void RegisterSingleton<I>(I instance)
        {
            _serviceCollection.Add(new ServiceDescriptor(typeof(I), instance));
        }

        public static void RegisterSingleton<I, T>()
        {
            _serviceCollection.Add(new ServiceDescriptor(typeof(I), typeof(T), ServiceLifetime.Singleton));
        }

        public static T Resolve<T>()
        {
            return _serviceProvider.GetRequiredService<T>();
        }
    }
}
