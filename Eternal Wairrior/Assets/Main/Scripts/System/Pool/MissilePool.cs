using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MissilePool : MonoBehaviour
{

    public static MissilePool pool;
    public MissileProjectile projPrefab;

    private void Awake()
    {
        pool = this;
    }

    List<MissileProjectile> poolList = new();

    public MissileProjectile Pop()
    {
        if (poolList.Count <= 0)
        {
            Push(Instantiate(projPrefab));
        }
        MissileProjectile proj = poolList[0];

        poolList.Remove(proj);

        proj.gameObject.SetActive(true);

        proj.transform.SetParent(null);

        return proj;
    }

    public void Push(MissileProjectile proj)
    {
        poolList.Add(proj);
        proj.gameObject.SetActive(false);
        proj.transform.SetParent(transform, false);
    }

    public void Push(MissileProjectile proj, float delay)
    {
        StartCoroutine(PushCoroutine(proj, delay));
    }

    IEnumerator PushCoroutine(MissileProjectile proj, float delay)
    {
        yield return new WaitForSeconds(delay);
        poolList.Add(proj);
        proj.gameObject.SetActive(false);
        proj.transform.SetParent(transform, false);
    }

}

