using System.Collections;
using UnityEngine;

namespace LaserSystem2D
{
    public interface ICoroutineRunner
    {
        void StopAllCoroutines();
        Coroutine StartCoroutine(IEnumerator function);
        bool isActiveAndEnabled { get; }
    }
}