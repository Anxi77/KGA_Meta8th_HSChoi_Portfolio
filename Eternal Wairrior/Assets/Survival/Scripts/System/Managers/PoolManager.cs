using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    #region Members
    private static PoolManager instance;

    public static PoolManager Instance => instance;

    public static EnemyPool laserPool;

    public static MissilePool missilePool;
    #endregion

    #region Unity Message Methods
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            DestroyImmediate(this);
            return;
        }
    }
    #endregion
}
