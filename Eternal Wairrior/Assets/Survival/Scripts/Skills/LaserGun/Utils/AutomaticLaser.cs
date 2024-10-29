using System;
using System.Collections;
using UnityEngine;

namespace LaserSystem2D
{
    [RequireComponent(typeof(LaserBase))]
    public class AutomaticLaser : MonoBehaviour
    {
        [SerializeField] private bool _infiniteWorking = true;
        [SerializeField] private bool _startWithSleepMode = true;
        [SerializeField] [Min(0)] private float _waitTimeAtStart = 2;
        [SerializeField] [Min(0)] private float _workTime = 4;
        [SerializeField] [Min(0)] private float _sleepTime = 2;
        [SerializeField] private LaserBase _laser;

        public event Action LaserEnable;
        
        private void OnValidate()
        {
            Start();
        }

        private void Start()
        {
            _laser = GetComponent<LaserBase>();
        }

        public bool InfiniteWorking => _infiniteWorking;
    
        private void OnEnable()
        {
            StartLaser();
        }

        private void StartLaser()
        {
            StopAllCoroutines();

            if (_infiniteWorking)
            {
                _laser.Enable();
            }
            else
            {
                StartCoroutine(StartLaserCoroutine());
            }
        }

        private IEnumerator StartLaserCoroutine()
        {
            if (_startWithSleepMode == false)
            {
                LaserEnable?.Invoke();
                _laser.Enable();
            }
        
            yield return new WaitForSeconds(_waitTimeAtStart);

            while (true)
            {
                LaserEnable?.Invoke();
                _laser.Enable();
            
                yield return new WaitForSeconds(_workTime);
            
                _laser.Disable();
            
                yield return new WaitForSeconds(_sleepTime + _laser.Data.DissolveTime);
            }
        }
    }
}