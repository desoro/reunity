using System;
using System.Collections.Generic;
using UnityEngine;

namespace Phuntasia
{
    public class PoolManager
    {
        static readonly DurableBehaviour _holder;
        static readonly List<GameObject> _preloadCache;
        static readonly Dictionary<GameObject, PoolSet> _getPools;
        static readonly Dictionary<GameObject, PoolSet> _returnPools;

        static PoolManager()
        {
            _holder = new GameObject("Pool").AddComponent<DurableBehaviour>();
            _preloadCache = new List<GameObject>();
            _getPools = new Dictionary<GameObject, PoolSet>();
            _returnPools = new Dictionary<GameObject, PoolSet>();
        }

        public static void Preload(GameObject prefab, int count)
        {
            for (int n = 0; n < count; n++)
            {
                _preloadCache.Add(Get(prefab, Vector3.zero, _holder.transform));
            }

            for (int n = 0; n < count; n++)
            {
                Return(_preloadCache[n]);
            }

            _preloadCache.Clear();
        }

        public static T Get<T>(T prefab, Vector3 position, Transform parent = null)
           where T : MonoBehaviour
        {
            return Get(prefab.gameObject, position, parent).GetComponent<T>();
        }

        public static GameObject Get(GameObject prefab, Vector3 position, Transform parent = null)
        {
            if (!_getPools.TryGetValue(prefab, out var set))
            {
                set = new PoolSet(prefab, _holder.transform);
                _getPools[prefab] = set;
            }

            var obj = set.GetObject(position, parent);

            _returnPools[obj] = set;

            return obj;
        }

        public static void Return<T>(T instance)
            where T : MonoBehaviour
        {
            Return(instance.gameObject);
        }

        public static void Return(GameObject instance)
        {
            if (!_returnPools.TryGetValue(instance, out var set))
            {
                throw new InvalidOperationException($"{instance.name} is not registered.");
            }

            set.ReturnObject(instance);

            _returnPools.Remove(instance);
        }
    }
}