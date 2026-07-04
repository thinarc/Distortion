using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Develop
{
    public static class G
    {
        private static readonly Dictionary<string, IService> services = new();
        
        public static void Reset() => services.Clear();
        
        public static T Get<T>() where T : IService
        {
            var key = typeof(T).Name;
            if (services.TryGetValue(key, out var service)) return (T)service;
            Debug.LogError($"{key} not registered with {nameof(G)}");
            throw new InvalidOperationException();
        }
        
        public static void Register<T>(T service) where T : IService
        {
            var key = typeof(T).Name;
            if (services.TryAdd(key, service)) return;
            Debug.LogError($"Attempted to register service of type {key} which is already registered with the {nameof(G)}.");
        }
        
        public static void Unregister<T>() where T : IService
        {
            var key = typeof(T).Name;
            if (services.Remove(key)) return;
            Debug.LogError($"Attempted to unregister service of type {key} which is not registered with the {nameof(G)}.");
        }
    }
}