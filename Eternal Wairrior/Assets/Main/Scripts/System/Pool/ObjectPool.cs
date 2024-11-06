using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public Component prefab;
        public int initialSize = 20;
    }

    [SerializeField] private List<Pool> pools = new List<Pool>();
    private Dictionary<string, Queue<Component>> poolDictionary;
    private Dictionary<string, Pool> poolSettings;

    private const int DEFAULT_POOL_SIZE = 20;
    private const int EXPAND_SIZE = 10;

    private void Awake()
    {
        poolDictionary = new Dictionary<string, Queue<Component>>();
        poolSettings = new Dictionary<string, Pool>();
        InitializePools();
    }

    private void InitializePools()
    {
        foreach (Pool pool in pools)
        {
            CreatePool(pool);
        }
    }

    private void CreatePool(Pool pool)
    {
        Queue<Component> objectPool = new Queue<Component>();
        poolSettings[pool.tag] = pool;

        for (int i = 0; i < pool.initialSize; i++)
        {
            CreateNewObjectInPool(pool.tag, objectPool);
        }

        poolDictionary[pool.tag] = objectPool;
    }

    private Component CreateNewObjectInPool(string tag, Queue<Component> pool)
    {
        if (!poolSettings.TryGetValue(tag, out Pool settings))
            return null;

        Component obj = Instantiate(settings.prefab);
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(transform);
        pool.Enqueue(obj);
        return obj;
    }

    public T Spawn<T>(GameObject prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        T component = prefab.GetComponent<T>();
        if (component == null)
        {
            Debug.LogError($"Prefab {prefab.name} does not have component of type {typeof(T)}");
            return null;
        }

        string tag = prefab.name;

        if (!poolDictionary.ContainsKey(tag))
        {
            Pool newPool = new Pool
            {
                tag = tag,
                prefab = component,
                initialSize = DEFAULT_POOL_SIZE
            };
            CreatePool(newPool);
            //Debug.Log($"Created new pool for {tag}");
        }

        Queue<Component> pool = poolDictionary[tag];

        if (pool.Count == 0)
        {
            for (int i = 0; i < EXPAND_SIZE; i++)
            {
                CreateNewObjectInPool(tag, pool);
            }
            Debug.Log($"Expanded pool {tag} by {EXPAND_SIZE}");
        }

        Component obj = pool.Dequeue();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.gameObject.SetActive(true);

        if (obj is IPoolable poolable)
        {
            poolable.OnSpawnFromPool();
        }

        return obj as T;
    }

    public void Despawn<T>(T obj) where T : Component
    {
        string tag = obj.gameObject.name;
        if (!poolDictionary.ContainsKey(tag))
        {
            Pool newPool = new Pool
            {
                tag = tag,
                prefab = obj,
                initialSize = DEFAULT_POOL_SIZE
            };
            CreatePool(newPool);
        }

        if (obj is IPoolable poolable)
        {
            poolable.OnReturnToPool();
        }

        obj.gameObject.SetActive(false);
        obj.transform.SetParent(transform);
        poolDictionary[tag].Enqueue(obj);
    }

    public void ClearAllPools()
    {
        foreach (var pool in poolDictionary.Values)
        {
            while (pool.Count > 0)
            {
                var obj = pool.Dequeue();
                if (obj != null)
                {
                    Destroy(obj.gameObject);
                }
            }
        }
        poolDictionary.Clear();
        poolSettings.Clear();
    }
}

public interface IPoolable
{
    void OnSpawnFromPool();
    void OnReturnToPool();
}