using System.Collections;
using UnityEngine;

namespace LaserSystem2D
{
    public class LaserDissolve : IEnable
    {
        private readonly ViewData _viewData;

        public LaserDissolve(ViewData viewData)
        {
            _viewData = viewData;
        }

        public float Value { get; private set; } = 2;
        public bool IsAnimating { get; private set; }

        public void Enable()
        {
            SetToZero();
        }

        public void SetToZero()
        {
            Value = 0;
            IsAnimating = false;
        }

        public IEnumerator Disappear()
        {
            float time = _viewData.DissolveTime;
            float startTime = time;
            IsAnimating = true;

            while (time >= 0)
            {
                time -= Time.deltaTime;
                Value = 1 - time / startTime;
                yield return null;
            }

            IsAnimating = false;
        }
    }
}