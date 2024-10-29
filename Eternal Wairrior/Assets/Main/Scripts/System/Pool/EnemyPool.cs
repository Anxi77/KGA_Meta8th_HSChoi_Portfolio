using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool pool;
    public Enemy projPrefab;

    private void Awake()
    {
        pool = this;
    }

    List<Enemy> poolList = new();

    public Enemy Pop()
    {
        if (poolList.Count <= 0)
        {
            Push(Instantiate(projPrefab));
        }
        Enemy proj = poolList[0];

        poolList.Remove(proj);

        proj.gameObject.SetActive(true);

        proj.transform.SetParent(null);

        return proj;
    }

    public void Push(Enemy proj)
    {
        poolList.Add(proj);
        proj.gameObject.SetActive(false);
        proj.transform.SetParent(transform, false);
    }

    public void Push(Enemy proj, float delay)
    {
        StartCoroutine(PushCoroutine(proj, delay));
    }

    IEnumerator PushCoroutine(Enemy proj, float delay)
    {
        yield return new WaitForSeconds(delay);
        poolList.Add(proj);
        proj.gameObject.SetActive(false);
        proj.transform.SetParent(transform, false);
    }

}
