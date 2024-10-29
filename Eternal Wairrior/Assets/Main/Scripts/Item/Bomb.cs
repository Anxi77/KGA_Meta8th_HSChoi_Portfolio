using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class Bomb : Item
{
    //public static event Action OnBombExploded;
    public ParticleSystem itemParticle;

    public override void Contact()
    {
        foreach (Enemy enemy in GameManager.Instance.enemies)
        {
            Destroy(enemy.gameObject);
        }
        GameManager.Instance.enemies.Clear();
        var particle = Instantiate(itemParticle, transform.position, Quaternion.identity);
        particle.Play();
        Destroy(gameObject);
    }
}
