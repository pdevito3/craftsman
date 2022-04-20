namespace NewCraftsman.Services;

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

public interface IBuilderService
{
    
}
public interface IModifierService
{
    
}

public static class CraftsmanServiceRegistration
{
    public static IServiceCollection AddCraftsmanBuildersAndModifiers(this IServiceCollection services, params Type[] handlerAssemblyMarkerTypes)
    {
        var assemblies = handlerAssemblyMarkerTypes.Select(t => t.GetTypeInfo().Assembly);

        if (!assemblies.Any())
        {
            throw new ArgumentException("No assemblies found to scan. Supply at least one assembly to scan for handlers.");
        }

        foreach (var assembly in assemblies)
        {
            var rules = assembly.GetTypes()
                .Where(x => !x.IsAbstract && x.IsClass &&
                            (x.GetInterface(nameof(IBuilderService)) == typeof(IBuilderService) || x.GetInterface(nameof(IModifierService)) == typeof(IModifierService)));

            foreach (var rule in rules)
            {
                foreach (var @interface in rule.GetInterfaces())
                {
                    services.Add(new ServiceDescriptor(@interface, rule, ServiceLifetime.Scoped));
                }
            }
        }

        return services;
    }
}