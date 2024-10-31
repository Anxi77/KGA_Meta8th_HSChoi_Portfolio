using UnityEngine;

public class PoolManager : MonoBehaviour
{
    private static PoolManager instance;
    public static PoolManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<PoolManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("PoolManager");
                    instance = go.AddComponent<PoolManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    private ObjectPool objectPool;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePool();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
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
}