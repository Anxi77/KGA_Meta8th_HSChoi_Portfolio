using UnityEngine;

namespace LaserSystem2D
{
    public class LaserAudio : IEnable, IDisable
    {
        private readonly HitEvent _hitEvent;
        private readonly AudioSource _launchSound;
        private readonly AudioSource _hitSound;

        public LaserAudio(AudioSource launchSound, AudioSource hitSound, HitEvent hitEvent)
        {
            _launchSound = launchSound;
            _hitSound = hitSound;
            _hitEvent = hitEvent;
        }

        public void Enable()
        {
            _hitEvent.Hit += PlayHitSound;

            if (_launchSound != null)
            {
                _launchSound.Play();
            }
        }

        public void Disable()
        {
            _hitEvent.Hit -= PlayHitSound;

            if (_launchSound != null)
            {
                _launchSound.Stop();
            }
        }

        private void PlayHitSound()
        {
            if (_hitSound != null)
            {
                _hitSound.Play();
            }
        }   
    }
}