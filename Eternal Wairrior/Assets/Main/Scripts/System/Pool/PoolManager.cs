using UnityEngine;
using System.Collections;

public class PoolManager : SingletonManager<PoolManager>, IInitializable
{
    public bool IsInitialized { get; private set; }

    private ObjectPool objectPool;

    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize()
    {
        try
        {
            Debug.Log("Initializing PoolManager...");
            InitializePool();
            IsInitialized = true;
            Debug.Log("PoolManager initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing PoolManager: {e.Message}");
            IsInitialized = false;
        }
    }

    private void InitializePool()
    {
        if (objectPool == null)
        {
            GameObject poolObj = new GameObject("ObjectPool");
            poolObj.transform.SetParent(transform);
            objectPool = poolObj.AddComponent<ObjectPool>();
        }
    }

    public T Spawn<T>(GameObject prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        return objectPool.Spawn<T>(prefab, position, rotation);
    }

    public void Despawn<T>(T obj) where T : Component
    {
        objectPool.Despawn(obj);
    }

    public void Despawn<T>(T obj, float delay) where T : Component
    {
        StartCoroutine(DespawnCoroutine(obj, delay));
    }

    private IEnumerator DespawnCoroutine<T>(T obj, float delay) where T : Component
    {
        yield return new WaitForSeconds(delay);
        if (obj != null)
        {
            objectPool.Despawn(obj);
        }
    }

    public void ClearAllPools()
    {
        if (objectPool != null)
        {
            objectPool.ClearAllPools();
        }
    }
}