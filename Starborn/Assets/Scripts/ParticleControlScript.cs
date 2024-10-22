using UnityEngine;

public class ParticleController : MonoBehaviour
{
    [SerializeField] private new ParticleSystem particleSystem;
    private ParticleSystem.MainModule mainModule;
    public float moveThreshold = 0.01f; // 움직임 감지 임계값
    private Vector3 lastPosition;

    void Start()
    {
        if (particleSystem == null)
        {
            particleSystem = GetComponent<ParticleSystem>();
            if (particleSystem == null)
            {
                Debug.LogError("ParticleSystem not assigned and not found on this GameObject.");
                enabled = false;
                return;
            }
        }

        mainModule = particleSystem.main;
        lastPosition = transform.position;

        // 파티클 시스템 시작
        particleSystem.Play();
    }

    void Update()
    {
        particleSystem.transform.position = transform.position + Vector3.forward * 50f;

        Vector3 movement = transform.position - lastPosition;
        float speed = movement.magnitude / Time.deltaTime;

        if (speed > moveThreshold)
        {
            MoveParticles(5f);
        }

        lastPosition = transform.position;
    }

    public void MoveParticles(float speed)
    {
        if (particleSystem == null) return;

        int particleCount = particleSystem.particleCount;
        if (particleCount == 0) return;

        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleCount];
        particleSystem.GetParticles(particles);

        Vector3 offset = transform.up * speed * Time.deltaTime;
        for (int i = 0; i < particleCount; i++)
        {
            particles[i].position += offset;
        }

        particleSystem.SetParticles(particles, particleCount);
    }
}