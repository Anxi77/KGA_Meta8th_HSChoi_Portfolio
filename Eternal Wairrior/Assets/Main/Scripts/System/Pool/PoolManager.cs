using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

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

    public void Despawn<T>(T obj, float delay) where T : Component
    {
        StartCoroutine(DespawnCoroutine(obj, delay));
    }

    private IEnumerator DespawnCoroutine<T>(T obj, float delay) where T : Component
    {
        yield return new WaitForSeconds(delay);
<<<<<<< HEAD
        objectPool.Despawn(obj);
    } 

=======
        if (obj != null)
        {
            objectPool.Despawn(obj);
        }
    }
>>>>>>> 636e55d9921dee25edf69b9286cacd4495ea6e5a
}