using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Dalamud.Logging;
using Dalamud.Plugin;
using SimpleTweaks.Attributes;
using SimpleTweaks.Exceptions;

namespace SimpleTweaks.Services;

[TweakService]
public class ServiceManager : IDisposable {
    private readonly ConcurrentStack<object> services = new();
    private readonly ConcurrentDictionary<Type, object> loadedServices = new();
    private readonly DalamudPluginInterface pluginInterface;

    private List<Type> GetTweakServiceDependencies(Type t) {
        var l = new List<Type>();
        
        var constructors = t.GetConstructors();
        foreach(var constructor in constructors) {
            foreach (var param in constructor.GetParameters()) {
                if (param.ParameterType.GetCustomAttribute<TweakServiceAttribute>() != null) {
                    l.Add(param.ParameterType);
                }
            }
        }

        return l;
    }

    private bool EnsureDependencies(Type forType, bool create = false, HashSet<Type>? circularDetection = null) {
        if (circularDetection != null) {
            if (circularDetection.Contains(forType)) {
                throw new CircularDependencyException();
            }
        }

        var dependencies = GetTweakServiceDependencies(forType);
        
        foreach (var dependency in dependencies) {
            if (this.loadedServices.ContainsKey(dependency)) continue;
            PluginLog.Log($"Dependancy for {forType}: {dependency}");
            if (!EnsureDependencies(dependency, true, circularDetection == null ? new HashSet<Type> { forType } : new HashSet<Type>(circularDetection) { forType })) {
                return false;
            }
        }

        if (create) {
            PluginLog.Log($"Loading Service: {forType}");
            var service = CreateGeneric(forType);
            if (service == null) return false;
            return TryAdd(forType, service);
        }
        
        return true;
    }
    
    private bool TryAdd<T>(T service) {
        if (service == null) return false;
        return TryAdd(typeof(T), service);
    }

    private bool TryAdd(Type t, object service) {
        this.services.Push(service);
        return this.loadedServices.TryAdd(t, service);
    }

    public ServiceManager(DalamudPluginInterface pluginInterface) {
        this.pluginInterface = pluginInterface;

        TryAdd(this);
        
        foreach(var autoLoad in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetCustomAttribute<TweakServiceAttribute>() is { LoadOnStartup: true})) {
            EnsureDependencies(autoLoad, true);
        }
    }

    public T? Create<T>() where T : class {
        if (!EnsureDependencies(typeof(T))) {
            return null;
        }
        return pluginInterface.Create<T>(services.ToArray());
    }

    private object? CreateGeneric(Type t) {
        if (!EnsureDependencies(t)) {
            return null;
        }
        var createMethod = pluginInterface.GetType().GetMethod("Create")?.MakeGenericMethod(t);
        return createMethod?.Invoke(pluginInterface, new object?[] { services.ToArray() });
    }

    public void Dispose() {
        while (services.TryPop(out var service)) {
            PluginLog.Log($"Disposing Service: {service}");
            if (service == this) continue;
            if (service is IDisposable disposableService) {
                disposableService.Dispose();
            }
        }
    }
}
