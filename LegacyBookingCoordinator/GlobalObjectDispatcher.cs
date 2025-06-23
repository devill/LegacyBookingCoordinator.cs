using System;
using System.Collections.Generic;

namespace LegacyBookingCoordinator
{
    public class GlobalObjectDispatcher
    {
        private static GlobalObjectDispatcher? _instance;
        private static readonly object _lock = new object();
        
        private readonly Dictionary<Type, Queue<object>> _queuedObjects = new Dictionary<Type, Queue<object>>();
        private readonly Dictionary<Type, object> _alwaysObjects = new Dictionary<Type, object>();

        private GlobalObjectDispatcher() { }

        public static GlobalObjectDispatcher Instance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new GlobalObjectDispatcher();
                }
            }
            return _instance;
        }

        public T Create<T>(params object[] args)
        {
            var type = typeof(T);
            
            // Check if we have queued objects from SetOne calls
            if (_queuedObjects.ContainsKey(type) && _queuedObjects[type].Count > 0)
            {
                return (T)_queuedObjects[type].Dequeue();
            }
            
            // Check if we have a SetAlways override
            if (_alwaysObjects.ContainsKey(type))
            {
                return (T)_alwaysObjects[type];
            }
            
            // Default creation using reflection
            return (T)Activator.CreateInstance(type, args)!;
        }

        public void SetOne<T>(T obj)
        {
            var type = typeof(T);
            if (!_queuedObjects.ContainsKey(type))
            {
                _queuedObjects[type] = new Queue<object>();
            }
            _queuedObjects[type].Enqueue(obj!);
        }

        public void SetAlways<T>(T obj)
        {
            _alwaysObjects[typeof(T)] = obj!;
        }

        public void Clear<T>()
        {
            var type = typeof(T);
            _alwaysObjects.Remove(type);
            _queuedObjects.Remove(type);
        }

        public void ClearAll()
        {
            _alwaysObjects.Clear();
            _queuedObjects.Clear();
        }
    }
}