using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static void AddSingletonServicesOfType<T>(this IServiceCollection services)
    {
        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
            .Where(x => typeof(T).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);

        foreach (var type in types)
        {
            services.AddSingleton(type);
        }
    }
}