using Lean.Pool;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BulletProjectile : Projectile
{


    #region Unity Message Methods


    protected override void Awake()
    {
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void Update()
    {
        base.Update();

        base.Attack();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }


    #endregion

}