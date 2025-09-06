using System;

namespace AvSafeDelivery.Services;

public static class ServiceLocator
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, object> _services = new();

    public static void Register<T>(T instance) where T : class
    {
        _services[typeof(T)] = instance!;
    }

    public static T Get<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out var svc))
            return svc as T ?? throw new InvalidOperationException("Service registered with wrong type");

        throw new InvalidOperationException($"Service of type {typeof(T).FullName} not registered");
    }
}

