using UnityEngine;

public class BindEffect : MonoBehaviour, IPoolable
{
    private ParticleSystem[] particleSystems;

    private void Awake()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>(true);
    }

    public void OnSpawnFromPool()
    {
        Debug.Log($"BindEffect spawned: {gameObject.name}, Particle count: {particleSystems.Length}");

        foreach (var ps in particleSystems)
        {
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Play(true);
            }
        }
    }

    public void OnReturnToPool()
    {
        foreach (var ps in particleSystems)
        {
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        transform.SetParent(null);
    }

    private void OnEnable()
    {
        Debug.Log($"BindEffect enabled: {gameObject.name}");
    }
}