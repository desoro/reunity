using System.Collections.Generic;
using UnityEngine;

namespace Phuntasia
{
    public class PoolSet
    {
        readonly GameObject _prefab;
        readonly Transform _defaultParent;
        readonly Queue<GameObject> _pool;

        public PoolSet(GameObject prefab, Transform parent)
        {
            _prefab = prefab;
            _pool = new Queue<GameObject>();

            _defaultParent = new GameObject(prefab.name).transform;
            _defaultParent.SetParent(parent);
            _defaultParent.gameObject.SetActive(false);
        }

        public GameObject GetObject(Vector3 position, Transform parent)
        {
            GameObject obj;

            if (_pool.Count == 0)
            {
                obj = GameObject.Instantiate(_prefab);
            }
            else
            {
                obj = _pool.Dequeue();
                obj.SetActive(true);
            }

            obj.transform.SetParentAndZero(parent);

            obj.transform.localPosition = position;

            return obj;
        }

        public void ReturnObject(GameObject obj)
        {
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(_defaultParent);

            _pool.Enqueue(obj);
        }
    }
}