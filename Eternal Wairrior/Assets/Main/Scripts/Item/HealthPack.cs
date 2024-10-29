using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class HealthPack : Item
{
    public float hp;
    public ParticleSystem itemParticle;

    public override void Contact() 
    {
        GameManager.Instance.player.TakeHeal(hp);
        base.Contact();
        var particle = Instantiate(itemParticle, transform.position, Quaternion.identity);
        particle.Play();
        Destroy(particle, 0.3f);
    }
}
