using UnityEngine;

public class ParticleController : MonoBehaviour
{
    public ParticleSystem particleSystem;
    private ParticleSystem.MainModule mainModule;
    public float moveThreshold = 0.01f; // 움직임 감지 임계값
    private Vector3 lastPosition;

    void Start()
    {

        mainModule = particleSystem.main;
        lastPosition = transform.position;

        // 파티클 시스템 시작
        particleSystem.Play();
    }

    void Update()
    {
        particleSystem.transform.position = transform.position;

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

        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleSystem.particleCount];
        int particleCount = particleSystem.GetParticles(particles);

        for (int i = 0; i < particleCount; i++)
        {
            particles[i].position += (Vector3)(transform.up * speed * Time.deltaTime);
        }

        particleSystem.SetParticles(particles, particleCount);
    }
}