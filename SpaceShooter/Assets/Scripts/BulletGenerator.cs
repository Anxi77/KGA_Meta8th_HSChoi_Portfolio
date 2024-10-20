using UnityEngine;
using System.Collections;
using TMPro;

public class BulletGenerator : MonoBehaviour
{
    public GameObject Bullet;
    public float fireRate = 5f;
    public int maxBullets = 10;
    private GameObject player;
    public float reloadTime = 0f;
    private bool isFiring = false;
    private Coroutine missileFire;
    public TextMeshProUGUI Text;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("PlayerMove");
        if (Text != null)
        {
            Text.gameObject.SetActive(true);
        }
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isFiring)
        {
            missileFire = StartCoroutine(FireBullets());
        }
        else if (Input.GetMouseButtonUp(0) && isFiring) 
        {
            isFiring = false;   
            StopCoroutine(missileFire);
        }
    }

    IEnumerator FireBullets()
    {
        print("Fire !!");
        isFiring = true;
        int bulletsFired = 0;

        while (true)
        {
            Text.gameObject.SetActive(true);
            Text.text = $"Magazine : {maxBullets - bulletsFired}";
            if (bulletsFired >= maxBullets)
            {
                StartCoroutine(Reloading());
                yield return new WaitForSecondsRealtime(reloadTime);
                StopCoroutine(Reloading());
                bulletsFired = 0;
            }

            if (player != null)
            {
                Instantiate(Bullet, transform.position, Quaternion.identity);
                bulletsFired++;
            }
            yield return new WaitForSeconds(1f / fireRate);
        }
    }

    IEnumerator Reloading() 
    {
        if (Text != null)
        {
            float remainingTime = reloadTime;
            while (remainingTime > 0)
            {
                Text.text = $"Reloading... {remainingTime:F1}s";
                yield return new WaitForSeconds(Time.deltaTime);
                remainingTime -= Time.deltaTime;
            }
            Text.gameObject.SetActive(false);
        }
    }

}