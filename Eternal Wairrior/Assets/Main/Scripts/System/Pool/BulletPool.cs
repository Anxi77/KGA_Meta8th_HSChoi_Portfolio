using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool pool;
    public BulletProjectile projPrefab;

    private void Awake()
    {
        pool = this;
    }

    List<BulletProjectile> poolList = new();

    public BulletProjectile Pop()
    {
        if (poolList.Count <= 0)
        {
            Push(Instantiate(projPrefab));
        }
        BulletProjectile proj = poolList[0];

        poolList.Remove(proj);

        proj.gameObject.SetActive(true);

        proj.transform.SetParent(null);

        return proj;
    }

    public void Push(BulletProjectile proj)
    {
        poolList.Add(proj);
        proj.gameObject.SetActive(false);
        proj.gameObject.transform.position = Vector3.zero;
        proj.transform.SetParent(transform, false);
    }

    public void Push(BulletProjectile proj, float delay)
    {
        StartCoroutine(PushCoroutine(proj, delay));
    }

    IEnumerator PushCoroutine(BulletProjectile proj, float delay)
    {
        yield return new WaitForSeconds(delay);
        poolList.Add(proj);
        proj.gameObject.SetActive(false);
        proj.transform.SetParent(transform, false);
    }

}